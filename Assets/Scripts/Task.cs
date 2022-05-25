using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour {

    public PossibleTask taskBase;
    public int nParameterValue;
    public float fDifficulty;

    public Image imgBackground;
    public Text txtDescription;
    public Text txtDifficulty;

    public List<Player> lstCompletedBy;

    public void SetTask(PossibleTask _taskBase) {

        taskBase = _taskBase;

        //If we have a fixed difficulty, then set it - otherwise, wait for our parameter's value to set it
        if(taskBase.bUsingParameter == false) {
            fDifficulty = taskBase.nMinDifficulty;
        }

        UpdateVisuals();

    }

    public void SetParameterValue(int _nParameterValue) {

        nParameterValue = _nParameterValue;

        fDifficulty = Mathf.Lerp(taskBase.nMinDifficulty, taskBase.nMaxDifficulty,
            Mathf.InverseLerp(taskBase.nMinValue, taskBase.nMaxValue, nParameterValue));

        UpdateVisuals();
    }

    public void UpdateVisuals() {
        UpdateVisualDescription();
        UpdateVisualDifficulty();
    }

    public void UpdateVisualDescription() {

        string sDescription = "";

        if(taskBase.bUsingParameter) {
            sDescription = string.Format(taskBase.sRawDescription, nParameterValue);
        } else {
            sDescription = taskBase.sRawDescription;
        }

        txtDescription.text = sDescription;
    }


    public void UpdateVisualDifficulty() {

        txtDifficulty.text = string.Format("{0:.##}", fDifficulty);
    }

}
