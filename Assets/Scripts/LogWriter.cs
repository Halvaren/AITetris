using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SocialPlatforms.Impl;

public class LogWriter : MonoBehaviour
{
    private static LogWriter instance;
    public static LogWriter Instance
    {
        get
        {
            return instance;
        }
    }

    public string MCTreeSearchLogFilePath = "Assets/Resources/MCTreeSearchLog.txt";
    public string ScoreLogFilePath = "Assets/Resources/ScoreLog.txt";

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {
        File.WriteAllText(MCTreeSearchLogFilePath, "");
    }

    public void WriteMCTreeSearch(string text)
    {
        File.AppendAllText(MCTreeSearchLogFilePath, text + "\n");
    }

    public void WriteScoreLog(string text)
    {
        File.AppendAllText(ScoreLogFilePath, text + "\n");
    }
}
