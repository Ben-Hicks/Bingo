using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour {

    public TaskManager taskmanager;

    public GameObject pfClaimedColour;
    public List<GameObject> lstgoClaimColours;

    public PossibleTask taskBase;
    public int nParameterValue;
    public float fDifficulty;

    public Image imgBackground;
    public Text txtDescription;
    public Text txtDifficulty;

    public bool[] arbCompletedBy;

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

    public void Init(TaskManager _taskmanager) {
        taskmanager = _taskmanager;
        arbCompletedBy = new bool[taskmanager.lstAllPlayers.Count];
    }

    void ClearClaimColours() {
        for(int i = 0; i < lstgoClaimColours.Count; i++) {
            Destroy(lstgoClaimColours[i]);
        }

        lstgoClaimColours = null;
    }

    public void UpdateVisualClaimed() {

        List<Color> lstColorsClaiming = new List<Color>();

        for(int i = 0; i < arbCompletedBy.Length; i++) {
            if(arbCompletedBy[i]) {
                lstColorsClaiming.Add(taskmanager.lstAllPlayers[i].colClaimed);
            }
        }

        RectTransform rt = imgBackground.GetComponent<RectTransform>();
        float fTaskSize = rt.rect.width;

        ClearClaimColours();
        lstgoClaimColours = new List<GameObject>();

        for(int i = 0; i < lstColorsClaiming.Count; i++) {
            //Spawn a coloured rectangle linearly spaced between -fTaskSize/2 and +fTaskSize/2 (with negated y coordinates)
            float fInterpolatedCoord = i * (fTaskSize / lstColorsClaiming.Count) - fTaskSize / 2;

            GameObject goClaimedColour = Instantiate(pfClaimedColour, imgBackground.transform);
            goClaimedColour.transform.localPosition = new Vector3(fInterpolatedCoord, -fInterpolatedCoord, 0f);

            goClaimedColour.GetComponent<Image>().color = lstColorsClaiming[i];

            lstgoClaimColours.Add(goClaimedColour);

        }

    }

    public void OnClickTask() {
        ToggleClaimed(taskmanager.nSelectedPlayer);
    }

    public void ToggleClaimed(int id) {

        if(arbCompletedBy[id]) {
            Unclaim(id);
        } else {
            Claim(id);
        }
    }

    public void Claim(int id) {
        if(arbCompletedBy[id]) {
            Debug.LogError("Already claimed");
            return;
        }

        arbCompletedBy[id] = true;

        taskmanager.lstAllPlayers[id].ReactClaimedTask(this);

        UpdateVisualClaimed();
    }

    public void Unclaim(int id) {
        if(arbCompletedBy[id] == false) {
            Debug.LogError("Not yet claimed");
            return;
        }

        arbCompletedBy[id] = false;

        taskmanager.lstAllPlayers[id].ReactUnclaimedTask(this);

        UpdateVisualClaimed();
    }

    public void Update() {

        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            ToggleClaimed(0);
        } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            ToggleClaimed(1);
        } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            ToggleClaimed(2);
        } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            ToggleClaimed(3);
        } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            ToggleClaimed(4);
        }
    }
}
