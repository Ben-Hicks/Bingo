using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour {

    public TaskManager taskmanager;
    public int id;

    public GameObject pfClaimedColour;
    public List<GameObject> lstgoClaimColours;

    public PossibleTask taskBase;
    public int nParameterValue;
    public float fDifficulty;

    public Image imgBackground;
    public Text txtDescription;
    public Text txtDifficulty;

    public Flag flag;
    public List<Line> lstLinesIn = new List<Line>();

    public bool[] arbCompletedBy;

    public void SetId(int _id) {
        id = _id;
    }

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

    public string GetFilledDescription() {
        return string.Format(taskBase.sRawDescription, nParameterValue);
    }

    public void UpdateVisualDescription() {

        string sDescription = "";

        if(taskBase.bUsingParameter) {
            sDescription = GetFilledDescription();
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

        //Check if the ctrl key is held down - if so, open the url, otherwise claim the task
        if(Input.GetKey(KeyCode.LeftControl)) {
            OpenURL();
        } else {

            //If we have a NetworkMessenger spawned, then issue a network message through that
            if(NetworkSender.inst != null) {
                NetworkSender.inst.SendToggleTask(this, taskmanager.nSelectedPlayer);
            } else {
                //If we don't have a NetworkMessenger spawned, then just handle this locally
                ToggleClaimed(taskmanager.nSelectedPlayer);
            }
        }

    }


    public void OnStartHover() {
        //Debug.LogFormat("Start hover {0}", taskBase.sRawDescription);
        flag.SetVisible();
    }

    public void OnStopHover() {
        //Debug.LogFormat("End Hover {0}", taskBase.sRawDescription);
        flag.UnsetVisible();
    }

    public void OpenURL() {

        if(taskBase.sURL == "") return;

        //If we have a URL, we can open it in a browser
        Application.OpenURL(string.Format("https://www.{0}", taskBase.sURL));
    }

    public void ToggleClaimed(int iPlayer) {

        if(arbCompletedBy[iPlayer]) {
            Unclaim(iPlayer);
        } else {
            Claim(iPlayer);
        }
    }

    public void Claim(int iPlayer) {
        if(arbCompletedBy[iPlayer]) {
            Debug.LogError("Already claimed");
            return;
        }

        arbCompletedBy[iPlayer] = true;

        taskmanager.lstAllPlayers[iPlayer].ReactClaimedTask(this);

        for(int i = 0; i < lstLinesIn.Count; i++) {
            lstLinesIn[i].UpdateCompletion(iPlayer);
        }

        UpdateVisualClaimed();
    }

    public void Unclaim(int iPlayer) {
        if(arbCompletedBy[iPlayer] == false) {
            Debug.LogError("Not yet claimed");
            return;
        }

        arbCompletedBy[iPlayer] = false;

        taskmanager.lstAllPlayers[iPlayer].ReactUnclaimedTask(this);

        for(int i = 0; i < lstLinesIn.Count; i++) {
            lstLinesIn[i].UpdateCompletion(iPlayer);
        }

        UpdateVisualClaimed();
    }

    public void Update() {

    }

    public override string ToString() {
        return GetFilledDescription();
    }

}
