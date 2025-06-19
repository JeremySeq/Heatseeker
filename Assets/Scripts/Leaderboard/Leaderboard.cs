using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [Serializable]
    public class ScoreSubmission
    {
        public int score;
        public string username;
        public string signature;
    }
    
    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public string username;
    }

    [Serializable]
    public class ScoreList
    {
        public ScoreEntry[] scores;
    }
    
    public TextMeshProUGUI LeaderboardText;
    [SerializeField] private TMP_InputField usernameInput;
    private const string ServerURL = "https://jeremyseq.dev";
    private static readonly string SECRET_KEY = "74e114910d9155164ffb1a6b185b0062";

    private void Start()
    {
        // Set input field to current username
        if (UserManager.Instance != null)
            usernameInput.text = UserManager.Instance.Username;

        // Listen for user finishing input
        usernameInput.onEndEdit.AddListener(OnUsernameInputEndEdit);

        StartCoroutine(LoadLeaderboard());
    }

    private void OnUsernameInputEndEdit(string input)
    {
        if (UserManager.Instance != null)
            UserManager.Instance.Username = input;
    }

    IEnumerator LoadLeaderboard()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerURL + "/api/games/heatseeker/top10");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            LeaderboardText.text = "Error loading leaderboard.";
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;

            ScoreEntry[] scores = JsonHelper.FromJson<ScoreEntry>(json);

            LeaderboardText.text = "Leaderboard\n";
            for (int i = 0; i < scores.Length; i++)
            {
                LeaderboardText.text += $"{i}. {scores[i].username}: {scores[i].score}\n";
            }
        }
    }

    public static IEnumerator SubmitScore(int score)
    {
        string username = UserManager.Instance != null ? UserManager.Instance.Username : "Anonymous";
        
        string signature = GenerateHMAC(username + score, SECRET_KEY);

        ScoreSubmission submission = new ScoreSubmission
        {
            username = username,
            score = score,
            signature = signature
        };

        string json = JsonUtility.ToJson(submission);
        UnityWebRequest www = UnityWebRequest.Put(ServerURL + "/api/games/heatseeker/submit", json);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log("Error uploading score: " + www.error);
        else
            Debug.Log("Score uploaded!");
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
    
    private static string GenerateHMAC(string message, string hexKey)
    {
        byte[] keyBytes = HexStringToBytes(hexKey);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hash = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    
    private static byte[] HexStringToBytes(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
}
