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
    [SerializeField] LeaderBoard leaderboard;
    [SerializeField] TextAssetsLoader textAssetsLoader;
    [SerializeField] GameObject laserPointerL, laserPointerR;
    [SerializeField] SoundManager soundManager;

    [UdonSynced] long gameStartTime;
    [UdonSynced] int limitTime;

    int revenue;

    int noMissCount;

    DataList wordData;
    [UdonSynced, FieldChangeCallback(nameof(OriginalWordText))] string _originalWordText;
    /// <summary>
    /// オリジナルの単語の表示文字列
    /// </summary>
    /// <remarks>
    /// setterで表示を更新する
    /// </remarks>
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
    /// <summary>
    /// ローマ字の表示文字列
    /// </summary>
    /// <remarks>
    /// setterで表示を更新する
    /// </remarks>
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
        if (textAssetsLoader.state != LoadingState.Loaded || gameStartTime != 0)
            return;
            
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        // VRの場合はレーザーポインターを表示する
        if (Networking.LocalPlayer.IsUserInVR())
        {
            laserPointerL.SetActive(true);
            laserPointerR.SetActive(true);
        }

        // 初期化
        gameStartTime = System.DateTime.Now.ToBinary();
        limitTime = 60;
        revenue = -10000;
        noMissCount = 0;
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
        
        gameStartTime = 0;
        OriginalWordText = "今日の収入: " + revenue + "円";
        RomajiWordText = "";
        
        leaderboard.AddScore(Networking.LocalPlayer.displayName, revenue);

        RequestSerialization();
        soundManager.PlayGameEndSound();
    }

    public void OnInputKey(char c)
    {
        if (gameStartTime == 0 || !Networking.IsOwner(gameObject))
            return;

        DataList result = RomajiValidator.Validate(wordData[1].DataList, inputWord + c);
        if (result[0].Boolean)
        {
            inputWord += c;
            UpdateNoMissCount();
            if (inputWord == result[1].String)
            {
                revenue += 100;
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
            soundManager.PlayMissSound();
        }

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
            remainingTime.text = "終了！";

            if (Networking.IsOwner(gameObject))
                GameEnd();
        }
    }

    private void SelectNextWord()
    {
        wordData = textAssetsLoader.wordList[Random.Range(0, textAssetsLoader.wordList.Count)].DataList;
        OriginalWordText = wordData[0].String;
        DataList result = RomajiValidator.Validate(wordData[1].DataList, "");
        RomajiWordText = result[1].String;
    }

    private void UpdateNoMissCount()
    {
        noMissCount++;
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
