using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public class Word : UdonSharpBehaviour
{
    public static Word New(string kana, string original)
    {
        string[] buff = new string[]
        {
            kana,
            original
        };
        return (Word)(object)buff;
    }
}

public static class WordExt
{
    const char EN_START_MARKER = '\\';
    const char EN_END_MARKER = ';';

    public static string Original(this Word w)
    {
        return ((string[])(object)w)[1];
    }

    public static DataList RomajiCandidates(this Word w, TextAssetsLoader textAssetsLoader)
    {
        string kana = ((string[])(object)w)[0];

        DataList candidates = new DataList();
        DataList enCandidatesTmp = new DataList();
        int index = 0;
        foreach (char c in kana)
        {
            char next_c = index + 1 < kana.Length ? kana[index + 1] : '\0';

            if (c == EN_START_MARKER)
            {
                enCandidatesTmp.Clear();
                int endIndex = kana.IndexOf(EN_END_MARKER, index) - 1;
                string word = kana.Substring(index + 1, endIndex - index);
                Debug.Log("EnWord: " + word);
                if (textAssetsLoader.engWordMap.TryGetValue(word, out DataToken enWord))
                {
                    Debug.Log("Found: " + enWord.String);
                    if (candidates.Count == 0)
                    {
                        enCandidatesTmp.Add(enWord.String);
                    }
                    else
                    {
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            enCandidatesTmp.Add(candidates[i].String + enWord.String);
                        }
                    }
                }
                index++;
                continue;
            }
            else if (c == EN_END_MARKER)
            {
                Debug.Log("EnEnd");
                candidates.InsertRange(0, enCandidatesTmp);
                index++;
                continue;
            }

            Debug.Log("Kana: " + c);
            DataList allRomajis = new DataList();
            if (c == 'っ')
            {
                allRomajis.Add("ltu");
                allRomajis.Add("xtu");
                if (textAssetsLoader.kanaMap.TryGetValue(next_c.ToString(), out DataToken sokuonRomajis))
                {
                    for (int i = 0; i < sokuonRomajis.DataList.Count; i++)
                    {
                        Debug.Log("Found: " + sokuonRomajis.DataList[i].String);
                        allRomajis.Add(sokuonRomajis.DataList[i].String[0].ToString());
                    }
                }
            }

            if (textAssetsLoader.kanaMap.TryGetValue(c.ToString(), out DataToken romajis))
            {
                for (int i = 0; i < romajis.DataList.Count; i++)
                {
                    Debug.Log("Found: " + romajis.DataList[i].String);
                }
                allRomajis.InsertRange(0, romajis.DataList);
            }
            
            if (textAssetsLoader.sokuonyouonMap.TryGetValue(new string(new char[] {c, next_c}), out DataToken contractedRomajis))
            {
                for (int i = 0; i < contractedRomajis.DataList.Count; i++)
                {
                    Debug.Log("Found: " + contractedRomajis.DataList[i].String);
                }
                allRomajis.InsertRange(0, contractedRomajis.DataList);
            }

            DataList tmpCandidates = candidates.DeepClone();
            candidates.Clear();
            if (tmpCandidates.Count == 0)
            {
                candidates.InsertRange(0, allRomajis);
            }
            else
            {
                for (int i = 0; i < tmpCandidates.Count; i++)
                {
                    for (int j = 0; j < allRomajis.Count; j++)
                    {
                        candidates.Add(tmpCandidates[i].String + allRomajis[j].String);
                    }
                }
            }
            index++;
        }
        for (int i = 0; i < candidates.Count; i++)
        {
            Debug.Log(candidates[i].String);
        }
        return candidates;
    }
}