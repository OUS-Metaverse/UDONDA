﻿using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [SerializeField] TMP_Text remainingTime;
    [SerializeField] TMP_Text originalKana;
    [SerializeField] TMP_Text originalWord;
    [SerializeField] TMP_Text romajiWord;
    [SerializeField] LeaderBoard leaderboard;
    [SerializeField] TextAssetsLoader textAssetsLoader;
    [SerializeField] GameObject laserPointerL, laserPointerR;
    [SerializeField] SoundManager soundManager;

    [UdonSynced, FieldChangeCallback(nameof(GameStartTime))] long _gameStartTime;
    long GameStartTime
    {
        get => _gameStartTime;
        set {
            _gameStartTime = value;
            
            if (_gameStartTime == 0)
                remainingTime.text = "";
        }
    }
    [UdonSynced] int limitTime;

    int score;
    int noMissCount;
    int totalStrokeCount;
    int totalMissCount;

    DataList wordData;

    [UdonSynced, FieldChangeCallback(nameof(OriginalKanaText))] string _originalKanaText;
    string OriginalKanaText
    {
        get => _originalKanaText;
        set
        {
            _originalKanaText = value;
            originalKana.text = _originalKanaText;
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(OriginalWordText))] string _originalWordText;
    string OriginalWordText
    {
        get => _originalWordText;
        set
        {
            _originalWordText = value;
            originalWord.text = _originalWordText;
        }
    }
    
    string inputWord = "";
    [UdonSynced, FieldChangeCallback(nameof(RomajiWordText))] string _romajiWordText = "";
    string RomajiWordText
    {
        get => _romajiWordText;
        set
        {
            _romajiWordText = value;
            romajiWord.text = _romajiWordText;
        }
    }

    public void GameStart()
    {
        if (textAssetsLoader.state != LoadingState.Loaded || GameStartTime != 0)
            return;
            
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        // VRの場合はレーザーポインターを表示する
        if (Networking.LocalPlayer.IsUserInVR())
        {
            laserPointerL.SetActive(true);
            laserPointerR.SetActive(true);
        }

        // 初期化
        GameStartTime = System.DateTime.Now.ToBinary();
        limitTime = 60;
        score = 0;
        noMissCount = 0;
        totalStrokeCount = 0;
        totalMissCount = 0;
        inputWord = "";
        SelectNextWord();

        RequestSerialization();
        soundManager.PlayGameStartSound();
    }

    public void GameEnd()
    {
        // レーザーポインターを非表示にする
        laserPointerL.SetActive(false);
        laserPointerR.SetActive(false);
        
        GameStartTime = 0;

        if (score > 30)
            OriginalKanaText = "とんでもない大食い";
        else if (score > 20)
            OriginalKanaText = "すごい！";
        else if (score > 10)
            OriginalKanaText = "なかなか食べますね…";
        else
            OriginalKanaText = "かわいい";

        OriginalWordText = score + "本食べた！";

        RomajiWordText = $"ミスタイプ: {totalMissCount}回 平均タイプ数: {(totalStrokeCount - totalMissCount) / (float)limitTime:N1}回/秒";
        
        Networking.SetOwner(Networking.LocalPlayer, leaderboard.gameObject);
        leaderboard.AddScore(Networking.LocalPlayer.displayName, score);

        RequestSerialization();
        soundManager.PlayGameEndSound();
    }

    public void OnInputKey(char c)
    {
        if (GameStartTime == 0 || !Networking.IsOwner(gameObject))
            return;

        DataList result = RomajiValidator.Validate(wordData[2].DataList, inputWord + c);
        if (result[0].Boolean)
        {
            inputWord += c;
            UpdateNoMissCount();
            if (inputWord == result[1].String)
            {
                score += 1;
                inputWord = "";
                SelectNextWord();

                soundManager.PlayCorrectSound();
            }
            else
            {
                RomajiWordText = "<color=\"red\">" + result[1].String.Insert(inputWord.Length, "</color>");
                
                soundManager.PlayKeyStrokeSound();
            }
        }
        else
        {
            noMissCount = 0;
            totalMissCount++;
            soundManager.PlayMissSound();
        }

        RequestSerialization();
    }

    void Update()
    {
        if (GameStartTime == 0)
            return;
        
        int remaining = (int)(System.DateTime.FromBinary(GameStartTime) - System.DateTime.Now).TotalSeconds + limitTime;
        remainingTime.text = "残り時間 " + remaining + "秒";

        if (remaining <= 0 && Networking.IsOwner(gameObject))
            GameEnd();
    }

    private void SelectNextWord()
    {
        wordData = textAssetsLoader.wordList[Random.Range(0, textAssetsLoader.wordList.Count)].DataList;
        OriginalKanaText = wordData[1].String;
        OriginalWordText = wordData[0].String;
        DataList result = RomajiValidator.Validate(wordData[2].DataList, "");
        RomajiWordText = result[1].String;
    }

    private void UpdateNoMissCount()
    {
        noMissCount++;
        totalStrokeCount++;
        if (noMissCount == 10)
        {
            limitTime += 1;
            soundManager.PlayExtendTimeSound();
        }
        else if (noMissCount == 20)
        {
            limitTime += 2;
            soundManager.PlayExtendTimeSound();
        }
        else if (noMissCount == 30)
        {
            limitTime += 3;
            noMissCount = 0;
            soundManager.PlayExtendTimeSound();
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
