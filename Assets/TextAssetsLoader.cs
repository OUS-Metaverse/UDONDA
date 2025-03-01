using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public enum LoadingState
{
    NotLoaded,
    Loading,
    Loaded
}

public class TextAssetsLoader : UdonSharpBehaviour
{
    [SerializeField] readonly int loadPerFrame = 10;
    [SerializeField] TextAsset _engWordMap;
    [SerializeField] TextAsset _kanaMap;
    [SerializeField] TextAsset _wordList;

    private string[] _engWordMapLines;
    private string[] _kanaMapLines;
    private string[] _wordListLines;

    private int _engWordMapLoadedIndex = 0;
    private int _kanaMapLoadedIndex = 0;
    private int _wordListLoadedIndex = 0;

    /// <summary>
    /// 英単語対応マップ
    /// </summary>
    /// <remarks>
    /// 英単語をキー、対応するかなを値とする。
    /// </remarks>
    public DataDictionary engWordMap;
    /// <summary>
    /// かなローマ字対応マップ
    /// </summary>
    /// <remarks>
    /// かなをキー、対応するローマ字のリストを値とする。
    /// </remarks>
    public DataDictionary kanaMap;
    /// <summary>
    /// ワードリスト
    /// </summary>
    /// <remarks>
    /// ワードリストの各要素は、以下のような構造を持つ。
    /// <code>
    /// [0]: オリジナルのかな
    /// [1]: トークンのリスト
    /// </code>
    /// トークンのリストの各要素は通常ローマ字の候補リストを持つが、
    /// 英単語区間の場合は、以下のような構造を持つ。
    /// <code>
    /// [0]: 英単語
    /// [1]: ローマ字の候補リスト
    /// </code>
    /// </remarks>
    public DataList wordList;

    public float progress = 0.0f;
    public LoadingState state = LoadingState.NotLoaded;

    void Start()
    {
        _engWordMapLines = _engWordMap.text.Split('\n');
        _kanaMapLines = _kanaMap.text.Split('\n');
        _wordListLines = _wordList.text.Split('\n');
        state = LoadingState.Loading;
    }

    void Update()
    {
        if (state != LoadingState.Loading)
        {
            return;
        }
        // フレーム毎に一定数のデータを読み込む。
        for (int i = 0; i < loadPerFrame; i++)
        {
            if (_engWordMapLoadedIndex < _engWordMapLines.Length)
            {
                // 英単語対応リスト
                string[] parts = _engWordMapLines[_engWordMapLoadedIndex].Split(',');
                engWordMap.Add(parts[0], parts[1].TrimEnd());
                _engWordMapLoadedIndex++;
            }
            else if (_kanaMapLoadedIndex < _kanaMapLines.Length)
            {
                // かなローマ字対応リスト
                string[] parts = _kanaMapLines[_kanaMapLoadedIndex].Split(',');
                DataList list = new DataList();
                list.Capacity = parts.Length - 1;
                for (int j = 1; j < parts.Length; j++)
                {
                    list.Add(parts[j].TrimEnd());
                }
                kanaMap.Add(parts[0], list);
                _kanaMapLoadedIndex++;
            }
            else if (_wordListLoadedIndex < _wordListLines.Length)
            {
                // ワードリスト
                string[] parts = _wordListLines[_wordListLoadedIndex].Split(',');
                DataList data = new DataList();
                data.Capacity = 2;
                data.Add(parts[1]);
                data.Add(GenerateRomajiTokens(parts[0].TrimEnd()));
                wordList.Add(new DataToken(data));
                _wordListLoadedIndex++;
            }
            else
            {
                progress = 1.0f;
                state = LoadingState.Loaded;
                _engWordMapLines = null;
                _kanaMapLines = null;
                _wordListLines = null;
                return;
            }
        }
        progress = (float)(_engWordMapLoadedIndex + _kanaMapLoadedIndex + _wordListLoadedIndex) / (_engWordMapLines.Length + _kanaMapLines.Length + _wordListLines.Length);
    }
    
    private DataList GenerateRomajiTokens(string kana)
    {
        DataList kanaTokens = TokenizeKana(kana);
        DataList romajiTokens = new DataList();
        romajiTokens.Capacity = kanaTokens.Count;
        bool engWordFlag = false;
        DataList engRomajiTokensTmp = new DataList();
        int tokenIndex = 0;
        for (int i = 0; i < kanaTokens.Count; i++)
        {
            DataToken kanaToken = kanaTokens[tokenIndex];

            if (kanaToken.String == "\\")
            {
                // 英単語区間の開始
                // 英単語区間は、１つ目の要素に英単語を、２つ目の要素にローマ字の候補を格納する。
                engWordFlag = true;

                int endIndex = kanaTokens.IndexOf(";", tokenIndex);

                engRomajiTokensTmp.Clear();
                engRomajiTokensTmp.Capacity = endIndex - tokenIndex - 1;

                string word = "";
                for (int j = tokenIndex + 1; j < endIndex; j++)
                {
                    word += kanaTokens[j].String;
                }
                engWordMap.TryGetValue(word, out DataToken enWord);
                DataList engTokens = new DataList();
                engTokens.Capacity = 2;
                engTokens.Add(enWord.String);
                romajiTokens.Add(engTokens);
                tokenIndex++;
                continue;
            }
            else if (kanaToken.String == ";")
            {
                // 英単語区間の終了
                // ローマ字の候補を追加する。
                engWordFlag = false;
                romajiTokens[romajiTokens.Count - 1].DataList.Add(engRomajiTokensTmp.DeepClone());
                tokenIndex++;
                continue;
            }

            DataList romajiCandidates = new DataList();
            // かなトークンが１文字の場合は対応するローマ字が１パターンの場合が多く、
            // かなトークンが２文字（拗音）の場合は対応するローマ字が４パターン以上あることが多い。
            romajiCandidates.Capacity = kanaToken.String.Length == 1 ? 1 : 4;

            if (kanaToken.String == "っ")
            {
                // 促音の次の文字がある場合は、その文字に対応するローマ字も追加する。
                if (tokenIndex + 1 < kanaTokens.Count && kanaMap.TryGetValue(kanaTokens[tokenIndex + 1].String, out DataToken value))
                {
                    for (int j = 0; j < value.DataList.Count; j++)
                    {
                        romajiCandidates.Add(value.DataList[j].String[0].ToString());
                    }
                }
            }
            else if (kanaToken.String == "ん")
            {
                // 次の文字がnと結合する音以外の場合は"n"を追加する。
                int referenceIndex = kanaTokens[tokenIndex + 1].String == ";" ? tokenIndex + 2 : tokenIndex + 1;
                if (referenceIndex < kanaTokens.Count && !"あいうえおなにぬねのん".Contains(kanaTokens[referenceIndex].String[0]))
                {
                    romajiCandidates.Add("n");
                }
            }
            
            // かなローマ字対応リストからローマ字の候補を取得する。
            if (kanaMap.TryGetValue(kanaToken.String, out DataToken romajis))
            {
                romajiCandidates.AddRange(romajis.DataList);
            }

            // 拗音を一文字ずつ入力した場合のローマ字を生成する。
            if (kanaToken.String.Length > 1)
            {
                DataList romajiCandidatesTmp = new DataList();
                romajiCandidatesTmp.Capacity = 2;
                bool firstChar = true;
                foreach (char c in kanaToken.String)
                {
                    if (kanaMap.TryGetValue(c.ToString(), out DataToken value))
                    {
                        int romajiCandidatesTmpCount = romajiCandidatesTmp.Count;
                        for (int j = 0; j < value.DataList.Count; j++)
                        {
                            string romaji = value.DataList[j].String;

                            if (firstChar)
                            {
                                // 初回は候補をそのまま追加する。
                                romajiCandidatesTmp.Add(romaji);
                            }
                            else
                            {
                                for (int k = 0; k < romajiCandidatesTmpCount; k++)
                                {
                                    if (j == value.DataList.Count - 1)
                                    {
                                        // 最後のローマ字候補は、既存の候補に追加する。
                                        romajiCandidatesTmp[k] = romajiCandidatesTmp[k].String + romaji;
                                    }
                                    else
                                    {
                                        // それ以外の場合は、新しい候補を追加する。
                                        romajiCandidatesTmp.Add(romajiCandidatesTmp[k].String + romaji);
                                    }
                                }
                            }
                        }
                    }
                    firstChar = false;
                }
                romajiCandidates.AddRange(romajiCandidatesTmp);
            }
            
            romajiCandidates.TrimExcess();
            if (engWordFlag)
            {
                // 英単語区間の場合は、ローマ字の候補を一時的に保持する。
                engRomajiTokensTmp.Add(romajiCandidates);
            }
            else
            {
                romajiTokens.Add(romajiCandidates);
            }
            tokenIndex++;
        }
        return romajiTokens;
    }

    private static DataList TokenizeKana(in string kana)
    {
        DataList tokens = new DataList();
        tokens.Capacity = kana.Length;
        foreach (char c in kana)
        {
            if ("ぁぃぅぇぉゃゅょ".Contains(c))
            {
                tokens.SetValue(tokens.Count - 1, tokens[tokens.Count - 1].String + c);
            }
            else
            {
                tokens.Add(c.ToString());
            }
        }
        return tokens;
    }
}
