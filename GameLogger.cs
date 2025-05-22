using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

[Serializable]
public class GameLog
{
    public string outcome;
    public Vector3 playerPosition;
    public Vector3 npcPosition;
    public float gameTime;
}

[Serializable]
public class GameLogWrapper
{
    public List<GameLog> logs = new List<GameLog>();
}

public class GameLogger : MonoBehaviour
{
    private string logFilePath;
    public GameLog latestLog;

    void Awake()
    {
        // Save to a proper folder within the Unity project
        string folderPath = Path.Combine(Application.dataPath, "Logs");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        logFilePath = Path.Combine(folderPath, "GameplayLogs.json");
        Debug.Log("Log file will be loaded from: " + logFilePath);
    }

    public void SaveGameplayData(GameLog newLog)
    {
        List<GameLog> logs = new List<GameLog>();

        if (File.Exists(logFilePath))
        {
            string existingJson = File.ReadAllText(logFilePath);
            GameLogWrapper wrapper = JsonUtility.FromJson<GameLogWrapper>(existingJson);
            if (wrapper != null && wrapper.logs != null)
                logs = wrapper.logs;
        }

        logs.Add(newLog);
        if (logs.Count > 100)
            logs.RemoveAt(0);

        GameLogWrapper newWrapper = new GameLogWrapper { logs = logs };
        string json = JsonUtility.ToJson(newWrapper, true);
        File.WriteAllText(logFilePath, json, Encoding.UTF8);

        Debug.Log("Game data saved to: " + logFilePath);
    }

    public void LoadLatestLog()
    {
        if (!File.Exists(logFilePath)) return;

        string jsonData = File.ReadAllText(logFilePath);
        GameLogWrapper logWrapper = JsonUtility.FromJson<GameLogWrapper>(jsonData);
        if (logWrapper.logs.Count > 0)
        {
            latestLog = logWrapper.logs[logWrapper.logs.Count - 1];
            ApplyLogToNPCBehavior(latestLog);
        }
    }

    private void ApplyLogToNPCBehavior(GameLog log)
    {
        if (log == null) return;

        Debug.Log("Player's last position: " + log.playerPosition);
        Debug.Log("NPC's last position: " + log.npcPosition);

        if (log.outcome == "Win")
        {
            Debug.Log("Player won. Adjust NPC difficulty if needed...");
            // Add your dynamic behavior here
        }
    }
}