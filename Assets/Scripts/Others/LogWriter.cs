using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Every reading and writting operation on a log file is done by this class
/// </summary>
public static class LogWriter
{
    static string MCTreeSearchLogFilePath = "Assets/Resources/MCTreeSearchLog.txt";

    static string BasicBotTestingLogFile = "Assets/Resources/BasicBotTestingLog.txt";
    static string MCTSBotTestingLogFile = "Assets/Resources/MCTSBotTestingLog.txt";
    static string HumanBotTestingLogFile = "Assets/Resources/HumanBotTestingLog.txt";

    static string BasicBotGensLogFile = "Assets/Resources/BasicBotGensLog.txt";
    static string MCTSBotGensLogFile = "Assets/Resources/MCTSBotGensLogFile.txt";
    static string HumanBotGensLogFile = "Assets/Resources/HumanBotGensLogFile.txt";

    #region MCTS methods

    public static void InitializeMCTreeSearchLog()
    {
        File.WriteAllText(MCTreeSearchLogFilePath, "");
    }

    public static void WriteMCTreeSearch(string text)
    {
        File.AppendAllText(MCTreeSearchLogFilePath, text + "\n");
    }

    #endregion

    #region Genetic algorithm methods

    public static TetrisGeneration[] ReadGenerationsArray(BotVersion botVersion, string path = null)
    {
        if(path == null) path = GetBotVersionGensLogFilePath(botVersion);

        if(path == "")
        {
            Debug.LogError("Wrong bot version");
            return null;
        }
        string json = File.ReadAllText(path);

        return JsonHelper.FromJsonArray<TetrisGeneration>(json);
    }

    public static List<TetrisGeneration> ReadGenerationsList(BotVersion botVersion, string path = null)
    {
        if (path == null) path = GetBotVersionGensLogFilePath(botVersion);

        if (path == "")
        {
            Debug.LogError("Wrong bot version");
            return null;
        }
        string json = File.ReadAllText(path);

        return JsonHelper.FromJsonList<TetrisGeneration>(json);
    }

    public static TetrisGeneration GetGeneration(BotVersion botVersion, int index)
    {
        TetrisGeneration[] tetrisGenerations = ReadGenerationsArray(botVersion);

        if (tetrisGenerations == null) return null;
        if(index >= tetrisGenerations.Length || index < 0)
        {
            Debug.LogError("Wrong index. Out of bounds");
            return null;
        }
        return tetrisGenerations[index];
    }

    public static void WriteGeneration(BotVersion botVersion, TetrisGeneration tetrisGeneration)
    {
        string path = GetBotVersionGensLogFilePath(botVersion);

        if (path == "")
        {
            Debug.Log("Wrong bot version");
        }
        else
        {
            List<TetrisGeneration> currentGenerations = ReadGenerationsList(botVersion, path);

            if (currentGenerations == null) currentGenerations = new List<TetrisGeneration>();

            if (tetrisGeneration.genIndex > currentGenerations.Count)
                currentGenerations.Add(tetrisGeneration);
            else
                currentGenerations[tetrisGeneration.genIndex - 1] = tetrisGeneration;

            string json = JsonHelper.ToJson(currentGenerations, true);

            File.WriteAllText(path, json);
        }
    }

    public static void WriteTesting(BotVersion botVersion, TestingData data)
    {
        string path = GetBotVersionTestingLogFilePath(botVersion);

        string json = JsonUtility.ToJson(data, true);

        File.AppendAllText(path, json);
    }

    private static string GetBotVersionGensLogFilePath(BotVersion botVersion)
    {
        switch (botVersion)
        {
            case BotVersion.TetrisBot:
                return BasicBotGensLogFile;

            case BotVersion.MCTSBot:
                return MCTSBotGensLogFile;

            case BotVersion.HumanizedBot:
                return HumanBotGensLogFile;
        }
        return "";
    }

    private static string GetBotVersionTestingLogFilePath(BotVersion botVersion)
    {
        switch (botVersion)
        {
            case BotVersion.TetrisBot:
                return BasicBotTestingLogFile;

            case BotVersion.MCTSBot:
                return MCTSBotTestingLogFile;

            case BotVersion.HumanizedBot:
                return HumanBotTestingLogFile;
        }
        return "";
    }

    #endregion
}
