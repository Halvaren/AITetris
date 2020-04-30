using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public string logFilePath = "Asset/Resources/MCTreeSearchLog.txt";

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {
        File.WriteAllText(logFilePath, "");
    }

    public void Write(string text)
    {
        File.AppendAllText(logFilePath, text + "\n");
    }
}
