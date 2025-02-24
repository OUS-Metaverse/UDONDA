using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public class TextAssetsLoader : UdonSharpBehaviour
{
    [SerializeField] TextAsset _engWordMap;
    [SerializeField] TextAsset _kanaMap;
    [SerializeField] TextAsset _sokuonYouonMap;
    [SerializeField] TextAsset _wordList;

    public DataDictionary engWordMap;
    public DataDictionary kanaMap;
    public DataDictionary sokuonyouonMap;
    public DataList wordList;

    private bool loaded = false;

    public void Load()
    {
        if (loaded)
        {
            return;
        }
        engWordMap = LoadEngWordMap();
        kanaMap = LoadKanaMap();
        sokuonyouonMap = LoadSokuonYouonMap();
        wordList = LoadWordList();
        loaded = true;
    }
    
    private DataDictionary LoadEngWordMap()
    {
        string[] lines = _engWordMap.text.Split('\n');
        DataDictionary kanaToEn = new DataDictionary();
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            kanaToEn.Add(parts[0], parts[1]);
        }
        return kanaToEn;
    }

    private DataDictionary LoadKanaMap()
    {
        string[] lines = _kanaMap.text.Split('\n');
        DataDictionary kanaToRomaji = new DataDictionary();
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            DataList list = new DataList();
            for (int i = 1; i < parts.Length; i++)
            {
                list.Add(parts[i]);
            }
            kanaToRomaji.Add(parts[0], list);
        }
        return kanaToRomaji;
    }

    private DataDictionary LoadSokuonYouonMap()
    {
        string[] lines = _sokuonYouonMap.text.Split('\n');
        DataDictionary contractedKanaToRomaji = new DataDictionary();
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            DataList list = new DataList();
            for (int i = 1; i < parts.Length; i++)
            {
                list.Add(parts[i]);
            }
            contractedKanaToRomaji.Add(parts[0], list);
        }
        return contractedKanaToRomaji;
    }

    public DataList LoadWordList()
    {
        string[] lines = _wordList.text.Split('\n');
        DataList wordList = new DataList();
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            wordList.Add(new DataToken((object)Word.New(parts[0], parts[1])));
        }
        return wordList;
    }
}
