
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [SerializeField] TMP_Text originalWord;
    [SerializeField] TMP_Text romajiWord;
    [SerializeField] TextAssetsLoader textAssetsLoader;

    private string inputWord = "";
    private DataList romajiCandidates;

    [UdonSynced] private bool gameStarted = false;
    [UdonSynced, FieldChangeCallback(nameof(wordIndex))] private int _wordIndex;
    private int wordIndex
    {
        get => _wordIndex;
        set
        {
            _wordIndex = value;
            Word word = (Word)textAssetsLoader.wordList[_wordIndex].Reference;
            originalWord.text = word.Original();
            romajiCandidates = word.RomajiCandidates(textAssetsLoader);
            romajiWord.text = romajiCandidates[0].String;
        }
    }

    public void GameStart()
    {
        if (gameStarted)
        {
            return;
        }
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        textAssetsLoader.Load();
        wordIndex = 62; // Random.Range(0, textAssetsLoader.wordList.Count);
        gameStarted = true;
        RequestSerialization();
    }

    public void OnInputKey(char c)
    {
        if (!gameStarted)
        {
            return;
        }

        inputWord += c;

        for (int i = 0; i < romajiCandidates.Count; i++)
        {
            if (romajiCandidates[i].String == inputWord)
            {
                wordIndex = Random.Range(0, textAssetsLoader.wordList.Count);
                inputWord = "";
                RequestSerialization();
                return;
            }
            else if (romajiCandidates[i].String.StartsWith(inputWord))
            {
                romajiWord.text = "<color=\"red\">" + romajiCandidates[i].String.Insert(inputWord.Length, "</color>");
                return;
            }
        }
        inputWord = inputWord.Substring(0, inputWord.Length - 1);
    }
}
