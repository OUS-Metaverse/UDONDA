﻿using TMPro;
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
    [SerializeField] TMP_Text leaderboard;
    [SerializeField] TextAssetsLoader textAssetsLoader;
    [SerializeField] AudioSource audioSource;    [SerializeField] GameObject laserPointerL, laserPointerR;

    
    [Header("Audio Clips")]
    [SerializeField] AudioClip keyStrokeSound;
    [SerializeField] AudioClip missSound;
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip extendTimeSound;
    [SerializeField] AudioClip gameStartSound;
    [SerializeField] AudioClip gameEndSound;

    [UdonSynced, FieldChangeCallback(nameof(LeaderboardData))] string _leaderboardData;
    string LeaderboardData
    {
        get => _leaderboardData;
        set
        {
            _leaderboardData = value;
            if (VRCJson.TryDeserializeFromJson(_leaderboardData, out DataToken result))
            {
                DataList leaderboardData = result.DataList;

                leaderboard.text = "";
                for (int i = 0; i < leaderboardData.Count; i++)
                {
                    DataList data = leaderboardData[i].DataList;
                    leaderboard.text += i + 1 + ". " + data[0] + " " + data[1] + "円\n";
                }
            }
        }
    }

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
                _inputWord = _inputWord.Substring(0, InputWord.Length - 1);
                noMissCount = 0;

                audioSource.PlayOneShot(missSound);
            }
            else
            {
                UpdateNoMissCount();
                if (_inputWord == romajiPreview)
                {
                    revenue += 100;
                    WordIndex = Random.Range(0, textAssetsLoader.wordList.Count);
                    _inputWord = "";
                    RequestSerialization();

                    audioSource.PlayOneShot(correctSound);
                }
                else
                {
                    if (_inputWord.Length <= romajiPreview.Length)
                    {
                        romajiWord.text = "<color=\"red\">" + romajiPreview.Insert(_inputWord.Length, "</color>");
                    }

                    audioSource.PlayOneShot(keyStrokeSound);
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
        if (Networking.LocalPlayer.IsUserInVR())
        {
            laserPointerL.SetActive(true);
            laserPointerR.SetActive(true);
        }
        gameStartTime = System.DateTime.Now.ToBinary();
        RequestSerialization();

        audioSource.PlayOneShot(gameStartSound);
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
            audioSource.PlayOneShot(gameEndSound);

            gameStartTime = 0;
            noMissCount = 0;
            limitTime = 60;
            remainingTime.text = "終了！";
            originalWord.text = "今日の収入: " + revenue + "円";
            romajiWord.text = "";

            DataList score = new DataList();
            score.Capacity = 2;
            score.Add(new DataToken(Networking.LocalPlayer.displayName));
            score.Add(new DataToken(revenue));

            if (VRCJson.TryDeserializeFromJson(LeaderboardData, out DataToken result))
            {
                DataList leaderboardData = result.DataList;
                for (int i = 0; i < leaderboardData.Count; i++)
                {
                    DataList data = leaderboardData[i].DataList;
                    if (data[1].Double < revenue)
                    {
                        leaderboardData.Insert(i, score);
                        break;
                    }
                }
                if (leaderboardData.Count > 10)
                {
                    leaderboardData.RemoveAt(0);
                }
                
                if (VRCJson.TrySerializeToJson(leaderboardData, JsonExportType.Minify, out DataToken json))
                {
                    LeaderboardData = json.String;
                    RequestSerialization();
                }
            }
            else
            {
                DataList leaderboardData = new DataList();
                leaderboardData.Add(score);
                if (VRCJson.TrySerializeToJson(leaderboardData, JsonExportType.Minify, out DataToken json))
                {
                    LeaderboardData = json.String;
                }
                RequestSerialization();
            }

            revenue = -10000;
        }
    }

    private void UpdateNoMissCount()
    {
        noMissCount++;
        if (noMissCount == 10)
        {
            limitTime += 1;
            audioSource.PlayOneShot(extendTimeSound);
        }
        else if (noMissCount == 20)
        {
            limitTime += 2;
            audioSource.PlayOneShot(extendTimeSound);
        }
        else if (noMissCount == 30)
        {
            limitTime += 3;
            noMissCount = 0;
            audioSource.PlayOneShot(extendTimeSound);
        }
    }

    // [RecursiveMethod]
    // private string ParseList(DataList list)
    // {
    //     string result = "[";
    //     for (int i = 0; i < list.Count; i++)
    //     {
    //         DataToken token = list[i];
    //         if (token.TokenType == TokenType.DataList)
    //         {
    //             result += ParseList(token.DataList);
    //         }
    //         else
    //         {
    //             result += "[" + token.String + "],";
    //         }
    //     }
    //     result += "]";
    //     return result;
    // }
}
