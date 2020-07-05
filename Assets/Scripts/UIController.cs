using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI linesText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI piecesText;
    public TextMeshProUGUI generationText;
    public TextMeshProUGUI holesWeightText;
    public TextMeshProUGUI bumpinessWeightText;
    public TextMeshProUGUI linesWeightText;
    public TextMeshProUGUI rowsHolesWeightText;
    public TextMeshProUGUI humanizedWeightText;

    public string defaultScoreText;
    public string defaultLinesText;
    public string defaultLevelText;
    public string defaultPiecesText;
    public string defaultGenerationText;
    public string defaultHolesWeightText;
    public string defaultBumpinessWeightText;
    public string defaultLinesWeightText;
    public string defaultRowsHolesWeightText;
    public string defaultHumanizedWeightText;

    public void UpdateScoreText(int score)
    {
        scoreText.text = defaultScoreText + score;
    }

    public void UpdateLinesText(int lines)
    {
        linesText.text = defaultLinesText + lines;
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = defaultLevelText + level;
    }

    public void UpdatePiecesText(int pieces)
    {
        piecesText.text = defaultPiecesText + pieces;
    }

    public void UpdateGenerationText(int generation)
    {
        generationText.text = defaultGenerationText + generation;
    }

    public void UpdateHolesWeightText(float holesWeight)
    {
        holesWeightText.text = defaultHolesWeightText + holesWeight.ToString("0.000");
    }

    public void UpdateBumpinessWeightText(float bumpinessWeight)
    {
        bumpinessWeightText.text = defaultBumpinessWeightText + bumpinessWeight.ToString("0.000");
    }

    public void UpdateLinesWeightText(float linesWeight)
    {
        linesWeightText.text = defaultLinesWeightText + linesWeight.ToString("0.000");
    }

    public void UpdateRowsHolesWeightText(float rowsHolesWeight)
    {
        rowsHolesWeightText.text = defaultRowsHolesWeightText + rowsHolesWeight.ToString("0.000");
    }

    public void UpdateHumanizedText(float humanizedWeight)
    {
        humanizedWeightText.text = defaultHumanizedWeightText + humanizedWeight.ToString("0.000");
    }
}
