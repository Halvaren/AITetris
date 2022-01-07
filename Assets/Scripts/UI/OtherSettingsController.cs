using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OtherSettingsController : MonoBehaviour
{
    #region Tab1 elements

    public GameObject tab1;
    public GameObject tab1Button;

    public TMP_InputField initialActionTimeField;
    public TMP_InputField nextActionsTimeField;

    public float defaultInitialActionTime = 1;
    public float defaultNextActionsTime = 0.25f;

    #endregion

    #region Tab2 elements

    public GameObject tab2;
    public GameObject tab2Button;

    public TMP_InputField populationSizeField;
    public TMP_InputField mutationRateField;
    public TMP_InputField pieceLimitTrainingField;

    public int defaultPopulationSize = 10;
    public float defaultMutationRate = 0.1f;
    public int defaultPieceLimitTraining = 150;

    #endregion

    #region Tab3 elements

    public GameObject tab3;
    public GameObject tab3Button;

    public TMP_InputField initialCalculationTimeField;
    public TMP_InputField decreasingCalculationTimeFactorField;

    public float defaultInitialCalculationTime = 1;
    public float defaultDecreasingCalculationTimeFactor = 0.05f;

    #endregion

    private int currentTab = 1;

    private TetrisBoardController tbController;
    public TetrisBoardController TBController
    {
        get
        {
            if(tbController == null)
            {
                tbController = TetrisBoardController.Instance;
            }
            return tbController;
        }
    }

    private static OtherSettingsController instance;
    public static OtherSettingsController Instance
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

    private void OnEnable()
    {
        ChangeTab(currentTab);
    }

    public void ChangeTab(int newTab)
    {
        if (newTab == 1) tab1.transform.SetAsLastSibling();
        if (newTab == 2) tab2.transform.SetAsLastSibling();
        if (newTab == 3) tab3.transform.SetAsLastSibling();

        tab1Button.SetActive(newTab != 1);
        tab2Button.SetActive(newTab != 2);
        tab3Button.SetActive(newTab != 3);

        currentTab = newTab;
    }

    public void SetInitialActionTime(float value)
    {
        initialActionTimeField.text = value.ToString();
    }

    public void SetNextActionsTime(float value)
    {
        nextActionsTimeField.text = value.ToString();
    }

    public void SetPopulationSize(int value)
    {
        populationSizeField.text = value.ToString();
    }

    public void SetMutationRate(float value)
    {
        mutationRateField.text = value.ToString();
    }

    public void SetPieceLimit(int value)
    {
        pieceLimitTrainingField.text = value.ToString();
    }

    public void SetInitialCalculationTime(float value)
    {
        initialCalculationTimeField.text = value.ToString();
    }

    public void SetDecreasingCalculationFactor(float value)
    {
        decreasingCalculationTimeFactorField.text = value.ToString();
    }

    public void SetDefaultValues()
    {
        SetInitialActionTime(defaultInitialActionTime);
        SetNextActionsTime(defaultNextActionsTime);

        SetPopulationSize(defaultPopulationSize);
        SetMutationRate(defaultMutationRate);
        SetPieceLimit(defaultPieceLimitTraining);

        SetInitialCalculationTime(defaultInitialCalculationTime);
        SetDecreasingCalculationFactor(defaultDecreasingCalculationTimeFactor);
    }

    public void SendValuesToBoard()
    {
        float initialActionTime;
        if (float.TryParse(initialActionTimeField.text, out initialActionTime))
            TBController.initialActionTime = initialActionTime;

        float nextActionsTime;
        if (float.TryParse(nextActionsTimeField.text, out nextActionsTime))
            TBController.nextActionsTime = nextActionsTime;

        int populationSize;
        if (int.TryParse(populationSizeField.text, out populationSize))
            TBController.populationSize = populationSize;

        float mutationRate;
        if (float.TryParse(mutationRateField.text, out mutationRate))
            TBController.mutationRate = mutationRate;

        int pieceLimit;
        if (int.TryParse(pieceLimitTrainingField.text, out pieceLimit))
            TBController.pieceLimitTraining = pieceLimit;

        float initialCalculationTime;
        if (float.TryParse(initialCalculationTimeField.text, out initialCalculationTime))
            TBController.initialCalculationTime = initialCalculationTime;

        float decreasingCalculationTimeFactor;
        if (float.TryParse(decreasingCalculationTimeFactorField.text, out decreasingCalculationTimeFactor))
            TBController.decreasingCalculationTimeFactor = decreasingCalculationTimeFactor;
    }

    public void OnClickCloseButton()
    {
        gameObject.SetActive(false);
    }
}
