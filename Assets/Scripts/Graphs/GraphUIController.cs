using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum TetrisDataStat
{
    BestScore, ScoreMean, Level, Lines
}

public class GraphUIController : MonoBehaviour
{
    public TMP_Dropdown trainingTestingDropdown;
    public TMP_Dropdown statDropdown;

    public Toggle tetrisBotToggle;
    public Toggle mctsBotToggle;
    public Toggle humanBotToggle;

    public TMP_Text tetrisBotToggleText;
    public TMP_Text mctsBotToggleText;
    public TMP_Text humanBotToggleText;

    public TMP_Text xAxisName;
    public TMP_Text yAxisName;

    private List<TetrisGeneration> tetrisBotTrainingData;
    private List<TetrisGeneration> mctsBotTrainingData;
    private List<TetrisGeneration> humanBotTrainingData;

    private GraphDrawer graphDrawer;
    public GraphDrawer GraphDrawer
    {
        get
        {
            if (graphDrawer == null) graphDrawer = GraphDrawer.Instance;
            return graphDrawer;
        }
    }

    private void Start()
    {
        yAxisName.text = statDropdown.options[statDropdown.value].text;

        tetrisBotTrainingData = LogWriter.ReadGenerationsList(BotVersion.TetrisBot);
        mctsBotTrainingData = LogWriter.ReadGenerationsList(BotVersion.MCTSBot);
        humanBotTrainingData = LogWriter.ReadGenerationsList(BotVersion.HumanizedBot);

        if(tetrisBotTrainingData == null)
        {
            tetrisBotToggle.isOn = false;
            tetrisBotToggle.interactable = false;
        }

        if(mctsBotTrainingData == null)
        {
            mctsBotToggle.isOn = false;
            mctsBotToggle.interactable = false;
        }

        if(humanBotTrainingData == null)
        {
            humanBotToggle.isOn = false;
            humanBotToggle.interactable = false;
        }

        SendAllDataToDrawer();

        tetrisBotToggleText.color = GraphDrawer.tetrisBotColor;
        mctsBotToggleText.color = GraphDrawer.mctsBotColor;
        humanBotToggleText.color = GraphDrawer.humanBotColor;
    }

    private void SendAllDataToDrawer()
    {
        int maxElements = tetrisBotToggle.isOn ? tetrisBotTrainingData.Count : 0;
        maxElements = mctsBotToggle.isOn ? Mathf.Max(maxElements, mctsBotTrainingData.Count) : maxElements;
        maxElements = humanBotToggle.isOn ? Mathf.Max(maxElements, humanBotTrainingData.Count) : maxElements;

        KeyValuePair<float, float> tetrisBotMinMaxValues = new KeyValuePair<float, float>(float.MaxValue, float.MinValue);
        KeyValuePair<float, float> mctsBotMinMaxValues = new KeyValuePair<float, float>(float.MaxValue, float.MinValue);
        KeyValuePair<float, float> humanBotMinMaxValues = new KeyValuePair<float, float>(float.MaxValue, float.MinValue);

        if (tetrisBotToggle.isOn) tetrisBotMinMaxValues = SendDataToDrawer(BotVersion.TetrisBot, maxElements);
        if (mctsBotToggle.isOn) mctsBotMinMaxValues = SendDataToDrawer(BotVersion.MCTSBot, maxElements);
        if (humanBotToggle.isOn) humanBotMinMaxValues = SendDataToDrawer(BotVersion.HumanizedBot, maxElements);

        float minValue = tetrisBotMinMaxValues.Key;
        float maxValue = tetrisBotMinMaxValues.Value;

        if (mctsBotMinMaxValues.Key < minValue) minValue = mctsBotMinMaxValues.Key;
        if (mctsBotMinMaxValues.Value > maxValue) maxValue = mctsBotMinMaxValues.Value;
        if (humanBotMinMaxValues.Key < minValue) minValue = humanBotMinMaxValues.Key;
        if (humanBotMinMaxValues.Value > maxValue) maxValue = humanBotMinMaxValues.Value;

        GraphDrawer.DrawGridAndAxis(maxElements, minValue, maxValue);
    }

    private KeyValuePair<float, float> SendDataToDrawer(BotVersion botVersion, int maxElements)
    {
        List<float> data = new List<float>();

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        if(trainingTestingDropdown.value == 0)
        {
            List<TetrisGeneration> botVersionTrainingData = null;
            switch(botVersion)
            {
                case BotVersion.TetrisBot:
                    botVersionTrainingData = tetrisBotTrainingData;
                    break;
                case BotVersion.MCTSBot:
                    botVersionTrainingData = mctsBotTrainingData;
                    break;
                case BotVersion.HumanizedBot:
                    botVersionTrainingData = humanBotTrainingData;
                    break;
            }

            for(int i = 0; i < botVersionTrainingData.Count; i++)
            {
                float value = -1;
                switch ((TetrisDataStat) statDropdown.value)
                {
                    case TetrisDataStat.BestScore:
                        value = botVersionTrainingData[i].bestScore;
                        data.Add(value);
                        break;
                    case TetrisDataStat.ScoreMean:
                        value = botVersionTrainingData[i].scoreMean;
                        data.Add(value);
                        break;
                    case TetrisDataStat.Lines:
                        value = botVersionTrainingData[i].lines;
                        data.Add(value);
                        break;
                    case TetrisDataStat.Level:
                        value = botVersionTrainingData[i].level;
                        data.Add(value);
                        break;
                }

                if(value != -1)
                {
                    if (value < minValue) minValue = value;
                    else if (value > maxValue) maxValue = value;
                }
            }

            if(maxElements > botVersionTrainingData.Count)
            {
                for(int i = botVersionTrainingData.Count; i < maxElements; i++)
                {
                    data.Add(-1);
                }
            }
        }
        else
        {
            //TODO
        }

        GraphDrawer.DrawValues(data, botVersion, minValue, maxValue);

        return new KeyValuePair<float, float>(minValue, maxValue);
    }

    public void OnChangeTrainingTestingDropdown(int newValue)
    {
        GraphDrawer.CleanGraph();
        SendAllDataToDrawer();
    }

    public void OnClickTetrisBotToggle(bool newValue)
    {
        GraphDrawer.CleanGraph();
        SendAllDataToDrawer();
    }

    public void OnClickMCTSBotToggle(bool newValue)
    {
        GraphDrawer.CleanGraph();
        SendAllDataToDrawer();
    }

    public void OnClickHumanBotToggle(bool newValue)
    {
        GraphDrawer.CleanGraph();
        SendAllDataToDrawer();
    }

    public void OnChangeStatDropdown(int newValue)
    {
        GraphDrawer.CleanGraph();
        SendAllDataToDrawer();

        yAxisName.text = statDropdown.options[statDropdown.value].text;
    }

    public void OnClickGoToTBButton()
    {
        SceneManager.LoadScene(0);
    }
}
