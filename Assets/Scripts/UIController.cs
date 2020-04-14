using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI linesText;
    public TextMeshProUGUI levelText;

    public string defaultScoreText;
    public string defaultLinesText;
    public string defaultLevelText;

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
}
