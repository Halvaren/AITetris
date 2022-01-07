using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingWarningController : MonoBehaviour
{
    private UIController uiController;
    public UIController UIController
    {
        get
        {
            if (uiController == null) uiController = UIController.Instance;
            return uiController;
        }
    }

    public void OnClickContinue()
    {
        UIController.StartGame();
        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}
