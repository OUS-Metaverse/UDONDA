using System.Text;
using UdonSharp;
using VRC.SDK3.Data;

public static class RomajiValidator
{
    /// <summary>
    /// ローマ字の入力を検証する
    /// </summary>
    /// <param name="wordTokens">トークンのリスト</param>
    /// <param name="inputRomaji">入力されたローマ字</param>
    /// <returns>
    /// <para>入力が正しいかどうかを表すBooleanと、入力に対応する完全なローマ字の候補のDataList</para>
    /// <para>UdonSharpの再帰関数ではref/out引数を使えないため、DataListで返す</para>
    /// </returns>
    [RecursiveMethod]
    public static DataList Validate(DataList wordTokens, in string inputRomaji)
    {
        bool isInputValid = true;
        StringBuilder candidateBuilder = new StringBuilder(EstimatedCandidateLength(wordTokens));

        for (int i = 0; i < wordTokens.Count; i++)
        {
            int headIndex = candidateBuilder.Length;
            DataList tokenData = wordTokens[i].DataList;

            // headIndex が入力の長さ以上（＝未入力のトークン）の場合は、
            // そのトークンの最初の候補を採用する
            if (headIndex >= inputRomaji.Length)
            {
                candidateBuilder.Append(tokenData[0].String);
                continue;
            }

            if (IsEnglishWordToken(tokenData))
            {
                // 英単語にマッチするかどうかを判定する
                if (IsMatch(tokenData[0].String, inputRomaji, headIndex))
                {
                    candidateBuilder.Append(tokenData[0].String);
                }
                else
                {
                    DataList result = Validate(tokenData[1].DataList, inputRomaji.Substring(headIndex));
                    if (result[0].Boolean)
                    {
                        candidateBuilder.Append(result[1].String);
                    }
                    else
                    {
                        isInputValid = false;
                    }
                }
            }
            else
            {
                // ローマ字の各候補にマッチするかどうかを判定する
                bool isMatch = false;
                if (tokenData[0].String == "n")
                {
                    // 先頭の候補が"n"の場合（＝「ん」のトークン）は、
                    // まず"n"以外の候補に完全一致するか検証する
                    for (int j = 1; j < tokenData.Count; j++)
                    {
                        if (IsStrictMatch(tokenData[j].String, inputRomaji, headIndex))
                        {
                            candidateBuilder.Append(tokenData[j].String);
                            isMatch = true;
                            break;
                        }
                    }
                    // "n"以外の候補に完全一致しなかった場合は、
                    // 次が "n" と接続しないトークンであれば"n"を許容する
                    if (!isMatch && inputRomaji[headIndex] == 'n'
                        && wordTokens.TryGetValue(i + 1, out DataToken nextToken)
                        && !"aiueon".Contains(nextToken.DataList[0].String[0])
                    )
                    {
                        candidateBuilder.Append("n");
                        isMatch = true;
                    }
                }
                else
                {
                    for (int j = 0; j < tokenData.Count; j++)
                    {
                        if (IsMatch(tokenData[j].String, inputRomaji, headIndex))
                        {
                            candidateBuilder.Append(tokenData[j].String);
                            isMatch = true;
                            break;
                        }
                    }
                }
                if (!isMatch)
                {
                    isInputValid = false;
                }
            }
        }

        return new DataList(new DataToken[2] { isInputValid, candidateBuilder.ToString() });;
    }

    /// <summary>
    /// 候補の長さを見積もる
    /// </summary>
    /// <param name="wordTokens">トークンのリスト</param>
    /// <returns>
    /// 各トークンの最初の候補の長さの合計
    /// </returns>
    static int EstimatedCandidateLength(DataList wordTokens)
    {
        int result = 0;
        for (int i = 0; i < wordTokens.Count; i++)
        {
            DataList token = wordTokens[i].DataList;
            result += token[0].String.Length;
        }
        return result;
    }

    /// <summary>
    /// トークンが英単語かどうかを判定する
    /// </summary>
    /// <param name="tokenData">トークン</param>
    /// <returns>
    /// 英単語の場合は true、それ以外の場合は false
    /// </returns>
    static bool IsEnglishWordToken(DataList tokenData)
    {
        return tokenData[1].TokenType == TokenType.DataList;
    }

    /// <summary>
    /// 入力されたローマ字が候補に対して前方一致するかどうかを判定する
    /// </summary>
    /// <param name="candidate">候補</param>
    /// <param name="input">入力されたローマ字</param>
    /// <param name="startIndex">入力されたローマ字の開始位置</param>
    /// <returns>
    /// 前方一致する場合は true、それ以外の場合は false
    /// </returns>
    static bool IsMatch(string candidate, string input, int startIndex)
    {
        if (startIndex + candidate.Length > input.Length)
        {
            return candidate.StartsWith(input.Substring(startIndex));
        }
        else
        {
            return candidate.StartsWith(input.Substring(startIndex, candidate.Length));
        }
    }

    /// <summary>
    /// 入力されたローマ字が候補に対して完全一致するかどうかを判定する
    /// </summary>
    /// <param name="candidate">候補</param>
    /// <param name="input">入力されたローマ字</param>
    /// <param name="startIndex">入力されたローマ字の開始位置</param>
    /// <returns>
    /// 完全一致する場合は true、それ以外の場合は false
    /// </returns>
    static bool IsStrictMatch(string candidate, string input, int startIndex)
    {
        if (startIndex + candidate.Length > input.Length)
        {
            return candidate == input.Substring(startIndex);
        }
        else
        {
            return candidate == input.Substring(startIndex, candidate.Length);
        }
    }
}
