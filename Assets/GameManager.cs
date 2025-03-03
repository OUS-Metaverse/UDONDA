using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [SerializeField] TMP_Text remainingTime;
    [SerializeField] TMP_Text originalWord;
    [SerializeField] TMP_Text romajiWord;
    [SerializeField] TextAssetsLoader textAssetsLoader;

    private DataList _word;

    [UdonSynced] long gameStartTime;
    [UdonSynced] int limitTime = 60;

    [UdonSynced] int revenue = -10000;

    int noMissCount = 0;

    DataList _word;
    [UdonSynced, FieldChangeCallback(nameof(WordIndex))] int _wordIndex;
    int WordIndex
    {
        get => _wordIndex;
        set
        {
            _wordIndex = value;
            if (textAssetsLoader.state != LoadingState.Loaded)
            {
                return;
            }
            _word = textAssetsLoader.wordList[_wordIndex].DataList;
            Debug.Log(_word[0].String);
            Debug.Log(ParseList(_word[1].DataList));
            originalWord.text = _word[0].String;
            romajiWord.text = "";
            for (int i = 0; i < _word[1].DataList.Count; i++)
            {
                DataList tokenList = _word[1].DataList[i].DataList;
                romajiWord.text += tokenList[0].String;
            }
        }
    }
    [UdonSynced, FieldChangeCallback(nameof(InputWord))] string _inputWord = "";
    string InputWord
    {
        get => _inputWord;
        set
        {
            _inputWord = value;

            if (_word == null)
            {
                return;
            }

            bool match = true;
            string romajiPreview = "";
            int headIndex = 0;
            for (int i = 0; i < _word[1].DataList.Count; i++)
            {
                // i番目のトークン情報
                // 0番目は1つ目のローマ字候補か、英単語。英単語の場合は1番目にローマ字候補が入る。
                DataList tokenInfo = _word[1].DataList[i].DataList;
                
                // まだ入力されてないトークン
                if (headIndex >= _inputWord.Length)
                {
                    romajiPreview += tokenInfo[0].String;
                    continue;
                }

                // 先頭の候補（英単語区間の場合は英単語）が一致するか
                if (tokenInfo[0].String.StartsWith(_inputWord.Substring(headIndex, System.Math.Min(tokenInfo[0].String.Length, _inputWord.Length - headIndex))))
                {
                    romajiPreview += tokenInfo[0].String;
                    headIndex += tokenInfo[0].String.Length;
                    continue;
                }

                // "nn"と入力した場合の対応
                if (headIndex > 0 && (_inputWord.Substring(headIndex - 1, 2) == "nn" || _inputWord.Substring(headIndex - 1, 2) == "n'") && _inputWord.Substring(headIndex - 2, 1) != "n")
                {
                    if (_word[1].DataList[i-1].DataList[0].String == "n")
                    {
                        if (_inputWord.Substring(headIndex, 1) == "n")
                        {
                            romajiPreview += "n";
                        }
                        else
                        {
                            romajiPreview += "'";
                        }
                        headIndex++;
                        i--;
                        continue;
                    }
                }
                
                if (tokenInfo[1].TokenType == TokenType.DataList)
                {
                    // 英単語区間
                    bool match2 = true;
                    DataList engTokenInfo = tokenInfo[1].DataList;
                    for (int j = 0; j < engTokenInfo.Count; j++)
                    {
                        // j番目のトークン情報
                        DataList romajis = engTokenInfo[j].DataList;

                        if (headIndex >= _inputWord.Length)
                        {
                            romajiPreview += romajis[0].String;
                            continue;
                        }

                        bool found = false;
                        for (int k = 0; k < romajis.Count; k++)
                        {
                            DataToken romaji = romajis[k];

                            if (romaji.String.StartsWith(_inputWord.Substring(headIndex, System.Math.Min(romaji.String.Length, _inputWord.Length - headIndex))))
                            {
                                romajiPreview += romaji.String;
                                headIndex += romaji.String.Length;
                                found = true;
                                break;
                            }

                            // "nn"と入力した場合の対応
                            if (headIndex > 0 && (_inputWord.Substring(headIndex - 1, 2) == "nn" || _inputWord.Substring(headIndex - 1, 2) == "n'") && _inputWord.Substring(headIndex - 2, 1) != "n")
                            {
                                if (engTokenInfo[j-1].DataList[0].String == "n")
                                {
                                    if (_inputWord.Substring(headIndex, 1) == "n")
                                    {
                                        romajiPreview += "n";
                                    }
                                    else
                                    {
                                        romajiPreview += "'";
                                    }
                                    headIndex++;
                                    k--;
                                    continue;
                                }
                            }
                        }
                        if (!found)
                        {
                            match2 = false;
                        }
                    }
                    if (match2) continue;
                }
                else
                {
                    // 通常のかなトークン
                    bool found = false;
                    for (int j = 1; j < tokenInfo.Count; j++)
                    {
                        DataToken romaji = tokenInfo[j];

                        if (romaji.String.StartsWith(_inputWord.Substring(headIndex, System.Math.Min(romaji.String.Length, _inputWord.Length - headIndex))))
                        {
                            romajiPreview += romaji.String;
                            headIndex += romaji.String.Length;
                            found = true;
                            break;
                        }
                    }
                    if (found) continue;
                }

                match = false;
                break;
            }

            if (!match)
            {
                noMissCount = 0;
            }
            else
            {
                UpdateNoMissCount();
                {
                    revenue += 100;
                    WordIndex = Random.Range(0, textAssetsLoader.wordList.Count);
                    _inputWord = "";
                    RequestSerialization();
                }
                else
                {
                    if (_inputWord.Length <= romajiPreview.Length)
                    {
                        romajiWord.text = "<color=\"red\">" + romajiPreview.Insert(_inputWord.Length, "</color>");
                    }
                }
            }
        }
    }

    public void GameStart()
    {
        if (textAssetsLoader.state != LoadingState.Loaded || gameStartTime != 0)
            return;
            
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        WordIndex = Random.Range(0, textAssetsLoader.wordList.Count);
        gameStartTime = System.DateTime.Now.ToBinary();
        RequestSerialization();
    }

    public void OnInputKey(char c)
    {
        if (!(gameStartTime != 0 && Networking.IsOwner(gameObject)))
            return;

        InputWord += c;
        RequestSerialization();
    }

    void Update()
    {
        if (gameStartTime == 0)
            return;
        
        int remaining = (int)(System.DateTime.FromBinary(gameStartTime) - System.DateTime.Now).TotalSeconds + limitTime;
        remainingTime.text = "残り時間 " + remaining + "秒";
        if (remaining <= 0)
        {
            gameStartTime = 0;
            remainingTime.text = "終了！";
            originalWord.text = "今日の収入: " + revenue + "円";
            romajiWord.text = "";
            }
            else
            {
            revenue = -10000;
            noMissCount = 0;
            limitTime = 60;
        }
    }

    private void UpdateNoMissCount()
    {
        noMissCount++;
        if (noMissCount == 10)
        {
            limitTime += 1;
        }
        else if (noMissCount == 20)
        {
            limitTime += 2;
        }
        else if (noMissCount == 30)
        {
            limitTime += 3;
            noMissCount = 0;
        }
    }
}
