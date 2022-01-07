using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    #region Game Stats elements

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI linesText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI piecesText;

    public string defaultScoreText;
    public string defaultLinesText;
    public string defaultLevelText;
    public string defaultPiecesText;

    #endregion

    #region Text variables

    public string pausedText = "Paused";
    public string stoppedText = "Stopped";
    public string playingText = "{0} is playing";

    public TMP_Text nowPlayingText;

    #endregion

    #region Bot Stats elements

    public TMP_InputField generationField;
    public TMP_InputField holesWeightField;
    public Slider holesWeightSlider;
    public TMP_InputField bumpinessWeightField;
    public Slider bumpinessWeightSlider;
    public TMP_InputField linesWeightField;
    public Slider linesWeightSlider;
    public TMP_InputField linesHolesWeightField;
    public Slider linesHolesWeightSlider;
    public TMP_InputField humanizedWeightField;
    public Slider humanizedWeightSlider;

    #endregion

    #region Settings elements

    public TMP_Dropdown modeDropdown;
    public TMP_Dropdown botDropdown;
    public Button otherSettingsButton;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;

    public GameObject otherSettingsPanel;
    public GameObject trainingWarningWindow;

    #endregion

    private TetrisBoardController tbController;
    public TetrisBoardController TBController
    {
        get
        {
            if (tbController == null) tbController = TetrisBoardController.Instance;
            return tbController;
        }
    }

    private OtherSettingsController osController;
    public OtherSettingsController OSController
    {
        get
        {
            if (osController == null) osController = OtherSettingsController.Instance;
            return osController;
        }
    }

    private static UIController instance;
    public static UIController Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playButton.interactable = true;
        pauseButton.interactable = false;
        stopButton.interactable = false;

        botDropdown.transform.parent.gameObject.SetActive(false);

        otherSettingsPanel.SetActive(false);
        trainingWarningWindow.SetActive(false);

        UpdateNowPlayingText(true, false, null);
    }

    public void UpdateNowPlayingText(bool stopped, bool paused, string whoIsPlaying)
    {
        if(stopped)
        {
            nowPlayingText.text = stoppedText;
        }
        else if(paused)
        {
            nowPlayingText.text = pausedText;
        }
        else
        {
            nowPlayingText.text = string.Format(playingText, whoIsPlaying);
        }
    }

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
        generationField.text = generation.ToString();
    }

    public void UpdateHolesWeightText(float holesWeight)
    {
        holesWeightField.text = holesWeight.ToString("0.000");
        holesWeightSlider.value = holesWeight;
    }

    public void UpdateBumpinessWeightText(float bumpinessWeight)
    {
        bumpinessWeightField.text = bumpinessWeight.ToString("0.000");
        bumpinessWeightSlider.value = bumpinessWeight;
    }

    public void UpdateLinesWeightText(float linesWeight)
    {
        linesWeightField.text = linesWeight.ToString("0.000");
        linesWeightSlider.value = linesWeight;
    }

    public void UpdateRowsHolesWeightText(float linesHolesWeight)
    {
        linesHolesWeightField.text = linesHolesWeight.ToString("0.000");
        linesHolesWeightSlider.value = linesHolesWeight;
    }

    public void UpdateHumanizedText(float humanizedWeight)
    {
        humanizedWeightField.text = humanizedWeight.ToString("0.000");
        humanizedWeightSlider.value = humanizedWeight;
    }

    public void PlayButtonClick()
    {
        if(!TBController.pausedGame)
        {
            if((PlayMode)modeDropdown.value != PlayMode.Training)
            {
                StartGame();
            }
            else
            {
                trainingWarningWindow.SetActive(true);
            }
        }
        else
        {
            TBController.pausedGame = false;

            playButton.interactable = false;
            pauseButton.interactable = true;
            stopButton.interactable = true;
        }

        if((PlayMode)modeDropdown.value == PlayMode.Player)
        {
            UpdateNowPlayingText(false, false, "Player");
        }
        else
        {
            UpdateNowPlayingText(false, false, botDropdown.options[botDropdown.value].text.ToString());
        }
    }

    public void StartGame()
    {
        TBController.playMode = (PlayMode)modeDropdown.value;
        TBController.botVersion = (BotVersion)botDropdown.value;

        EnableBotSettings(false);
        EnableWeightFields(TBController.playMode == PlayMode.ShowOff);

        OSController.SendValuesToBoard();
        TBController.InitializeGame();

        playButton.interactable = false;
        pauseButton.interactable = true;
        stopButton.interactable = true;
    }

    public void PauseButtonClick()
    {
        playButton.interactable = true;
        pauseButton.interactable = false;

        TBController.pausedGame = true;

        UpdateNowPlayingText(false, true, null);
    }

    public void StopButtonClick()
    {
        TBController.GameOver(true);

        UpdateNowPlayingText(true, false, null);
    }

    public void ResetButtons()
    {
        playButton.interactable = true;
        pauseButton.interactable = false;
        stopButton.interactable = false;
    }

    public void EnableBotSettings(bool value)
    {
        modeDropdown.interactable = value;
        botDropdown.interactable = value;
        otherSettingsButton.interactable = value;
    }

    public void EnableWeightFields(bool value)
    {
        generationField.interactable = value;
        holesWeightField.interactable = value;
        bumpinessWeightField.interactable = value;
        linesWeightField.interactable = value;
        linesHolesWeightField.interactable = value;
        humanizedWeightField.interactable = value;
        holesWeightSlider.interactable = value;
        bumpinessWeightSlider.interactable = value;
        linesWeightSlider.interactable = value;
        linesHolesWeightSlider.interactable = value;
        humanizedWeightSlider.interactable = value;
    }

    public void OnChangeModeDropdown(int value)
    {
        botDropdown.transform.parent.gameObject.SetActive(value != 0);
    }

    public void OnChangeGeneration(string value)
    {
        int intValue;
        if(int.TryParse(value, out intValue))
        {
            TBController.generation = intValue;
            generationField.image.color = Color.white;
        }
        else
        {
            generationField.image.color = Color.red;
        }
    }

    public void OnChangeHolesWeightField(string value)
    {
        float floatValue;
        if (float.TryParse(value, out floatValue))
        {
            TBController.holesWeight = floatValue;
            holesWeightSlider.value = floatValue;
            holesWeightField.image.color = Color.white;
        }
        else
        {
            holesWeightField.image.color = Color.red;
        }
    }

    public void OnChangeHolesWeightSlider(float value)
    {
        TBController.holesWeight = value;
        holesWeightField.text = value.ToString("0.000");
    }

    public void OnChangeBumpWeightField(string value)
    {
        float floatValue;
        if (float.TryParse(value, out floatValue))
        {
            TBController.bumpinessWeight = floatValue;
            bumpinessWeightSlider.value = floatValue;
            bumpinessWeightField.image.color = Color.white;
        }
        else
        {
            bumpinessWeightField.image.color = Color.red;
        }
    }

    public void OnChangeBumpWeightSlider(float value)
    {
        TBController.bumpinessWeight = value;
        bumpinessWeightField.text = value.ToString("0.000");
    }

    public void OnChangeLinesWeightField(string value)
    {
        float floatValue;
        if (float.TryParse(value, out floatValue))
        {
            TBController.linesWeight = floatValue;
            linesWeightSlider.value = floatValue;
            linesWeightField.image.color = Color.white;
        }
        else
        {
            linesWeightField.image.color = Color.red;
        }
    }

    public void OnChangeLinesWeightSlider(float value)
    {
        TBController.linesWeight = value;
        linesWeightField.text = value.ToString("0.000");
    }

    public void OnChangeLinesHolesWeightField(string value)
    {
        float floatValue;
        if (float.TryParse(value, out floatValue))
        {
            TBController.linesHolesWeight = floatValue;
            linesHolesWeightSlider.value = floatValue;
            linesHolesWeightField.image.color = Color.white;
        }
        else
        {
            linesHolesWeightField.image.color = Color.red;
        }
    }

    public void OnChangeLinesHolesWeightSlider(float value)
    {
        TBController.linesHolesWeight = value;
        linesHolesWeightField.text = value.ToString("0.000");
    }

    public void OnChangeHumanizedWeightField(string value)
    {
        float floatValue;
        if (float.TryParse(value, out floatValue))
        {
            TBController.humanizedWeight = floatValue;
            humanizedWeightSlider.value = floatValue;
            humanizedWeightField.image.color = Color.white;
        }
        else
        {
            humanizedWeightField.image.color = Color.red;
        }
    }

    public void OnChangeHumanizedWeightSlider(float value)
    {
        TBController.humanizedWeight = value;
        humanizedWeightField.text = value.ToString("0.000");
    }

    public void OnClickOtherSettings()
    {
        otherSettingsPanel.SetActive(true);
    }

    public void OnClickGoToStatsButton()
    {
        SceneManager.LoadScene(1);
    }
}
