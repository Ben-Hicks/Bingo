using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour {

    public PossibleTask taskBase;
    public int nParameterValue;

    public Image imgBackground;
    public Text txtDescription;

    public List<Player> lstCompletedBy;

    public void SetTask(PossibleTask _taskBase) {

        taskBase = _taskBase;

        txtDescription.text = GetDescription();

    }

    public string GetDescription() {

        if(taskBase.bUsingParameter) {
            return string.Format(taskBase.sRawDescription, nParameterValue);
        } else {
            return taskBase.sRawDescription;
        }

    }


}
