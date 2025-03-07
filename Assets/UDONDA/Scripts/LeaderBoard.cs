using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LeaderBoard : UdonSharpBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text scoreText;
    
    DataList leaderboardData = new DataList();
    [UdonSynced, FieldChangeCallback(nameof(SerializedLeaderboardData))] string _serializedLeaderboardData;
    string SerializedLeaderboardData
    {
        get => _serializedLeaderboardData;
        set
        {
            _serializedLeaderboardData = value;

            if (VRCJson.TryDeserializeFromJson(_serializedLeaderboardData, out DataToken token))
            {
                leaderboardData = token.DataList;
                UpdateLeaderboardText();
            }
            else
            {
                Debug.LogError(token.ToString());
            }
        }
    }

    public void AddScore(string name, int score)
    {
        DataList scoreData = new DataList(new DataToken[2] { name, score });

        for (int i = 0; i <= leaderboardData.Count; i++)
        {
            if (i == leaderboardData.Count)
            {
                leaderboardData.Add(scoreData);
                break;
            }

            DataList data = leaderboardData[i].DataList;
            if (data[1].Number <= score)
            {
                leaderboardData.Insert(i, scoreData);
                break;
            }
        }

        if (leaderboardData.Count > 13)
        {
            leaderboardData.RemoveAt(13);
        }

        UpdateLeaderboardText();

        if (VRCJson.TrySerializeToJson(leaderboardData, JsonExportType.Minify, out DataToken result))
        {
            _serializedLeaderboardData = result.String;
            RequestSerialization();
        }
        else
        {
            Debug.LogError(result.ToString());
        }
    }

    private void UpdateLeaderboardText()
    {
        playerNameText.text = "";
        for (int i = 0; i < leaderboardData.Count; i++)
        {
            DataList data = leaderboardData[i].DataList;
            playerNameText.text += $"{i + 1}. {data[0].String}\n";
            scoreText.text += $"{(int)data[1].Number}本\n";
        }
    }
}
