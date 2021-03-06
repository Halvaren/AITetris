﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SocialPlatforms.Impl;
using Assets.Scripts.GeneticAlgorithm;
using Assets.Scripts;

/// <summary>
/// Every reading and writting operation on a log file is done by this class
/// </summary>
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

    public string BasicBotTestingLogFile = "Assets/Resources/BasicBotTestingLog.txt";
    public string MCTSBotTestingLogFile = "Assets/Resources/MCTSBotTestingLog.txt";
    public string HumanBotTestingLogFile = "Assets/Resources/HumanBotTestingLog.txt";

    public string BasicBotGensLogFile = "Assets/Resources/BasicBotGensLog.txt";
    public string MCTSBotGensLogFile = "Assets/Resources/MCTSBotGensLogFile.txt";
    public string HumanBotGensLogFile = "Assets/Resources/HumanBotGensLogFile.txt";

    private void Awake()
    {
        instance = this;
    }

    #region MCTS methods

    public void InitializeMCTreeSearchLog()
    {
        File.WriteAllText(MCTreeSearchLogFilePath, "");
    }

    public void WriteMCTreeSearch(string text)
    {
        File.AppendAllText(MCTreeSearchLogFilePath, text + "\n");
    }

    #endregion

    #region Genetic algorithm methods

    public TetrisGeneration[] ReadGenerationsArray(BotVersion botVersion)
    {
        string path = GetBotVersionGensLogFilePath(botVersion);

        if(path == "")
        {
            Debug.Log("Wrong bot version");
            return null;
        }
        string json = File.ReadAllText(path);
        Debug.Log(json);

        return JsonHelper.FromJsonArray<TetrisGeneration>(json);
    }

    public List<TetrisGeneration> ReadGenerationsList(BotVersion botVersion)
    {
        string path = GetBotVersionGensLogFilePath(botVersion);

        if (path == "")
        {
            Debug.Log("Wrong bot version");
            return null;
        }
        string json = File.ReadAllText(path);

        return JsonHelper.FromJsonList<TetrisGeneration>(json);
    }

    public TetrisGeneration GetGeneration(BotVersion botVersion, int index)
    {
        TetrisGeneration[] tetrisGenerations = ReadGenerationsArray(botVersion);

        if (tetrisGenerations == null) return null;
        if(index >= tetrisGenerations.Length || index < 0)
        {
            Debug.Log("Wrong index. Out of bounds");
            return null;
        }
        return tetrisGenerations[index];
    }

    public void WriteGeneration(BotVersion botVersion, TetrisGeneration tetrisGeneration)
    {
        string path = GetBotVersionGensLogFilePath(botVersion);

        if (path == "")
        {
            Debug.Log("Wrong bot version");
        }
        else
        {
            List<TetrisGeneration> currentGenerations = ReadGenerationsList(botVersion);

            if (currentGenerations == null) currentGenerations = new List<TetrisGeneration>();

            if (tetrisGeneration.generation > currentGenerations.Count)
                currentGenerations.Add(tetrisGeneration);
            else
                currentGenerations[tetrisGeneration.generation - 1] = tetrisGeneration;

            string json = JsonHelper.ToJson(currentGenerations, true);

            File.WriteAllText(path, json);
        }
    }

    public void WriteTesting(BotVersion botVersion, TestingData data)
    {
        string path = GetBotVersionTestingLogFilePath(botVersion);

        string json = JsonUtility.ToJson(data, true);

        File.AppendAllText(path, json);
    }

    private string GetBotVersionGensLogFilePath(BotVersion botVersion)
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

    private string GetBotVersionTestingLogFilePath(BotVersion botVersion)
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
