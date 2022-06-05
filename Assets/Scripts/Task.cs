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

    public int[] arnPlayerProgress;

    public void SetId(int _id) {
        id = _id;
    }

    public void SetTask(PossibleTask _taskBase) {

        taskBase = _taskBase;

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
        string sProgress = arnPlayerProgress[taskmanager.nSelectedPlayer] == 0 ? nParameterValue.ToString() :
            string.Format("{0}/{1}", arnPlayerProgress[taskmanager.nSelectedPlayer], nParameterValue);

        return string.Format(taskBase.sRawDescription, sProgress);
    }

    public void UpdateVisualDescription() {
        txtDescription.text = GetFilledDescription(); ;
    }


    public void UpdateVisualDifficulty() {

        txtDifficulty.text = string.Format("{0:.##}", fDifficulty);
    }

    public void Init(TaskManager _taskmanager) {
        taskmanager = _taskmanager;
        arnPlayerProgress = new int[taskmanager.lstAllPlayers.Count];
        flag.arbFlagged = new bool[taskmanager.lstAllPlayers.Count];
    }

    void ClearClaimColours() {
        for(int i = 0; i < lstgoClaimColours.Count; i++) {
            Destroy(lstgoClaimColours[i]);
        }

        lstgoClaimColours = null;
    }

    public void UpdateVisualClaimed() {

        List<Color> lstColorsClaiming = new List<Color>();

        for(int i = 0; i < arnPlayerProgress.Length; i++) {
            if(arnPlayerProgress[i] == nParameterValue) {
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

    public void OnLeftClickTask() {

        //Left click
        if(Input.GetMouseButtonUp(0)) {

            //Check if the ctrl key is held down - if so, open the url
            if(Input.GetKey(KeyCode.LeftControl)) {
                OpenURL();
            } else if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                //If shift is held, then fully complete the task
                FullyCompleteTask();
            } else {
                //By default, just increment our progress
                IncrementProgress();

            }
        } else if(Input.GetMouseButtonUp(1)) {
            //Right click

            DecrementProgress();
        }

    }

    public void OnRightClickTask() {
        DecrementProgress();
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

    public void FullyCompleteTask() {
        RequestNewProgressValue(taskmanager.nSelectedPlayer, nParameterValue);
    }

    public void IncrementProgress() {
        int iPlayer = taskmanager.nSelectedPlayer;

        if(arnPlayerProgress[iPlayer] == nParameterValue) {
            Debug.Log("No need to increment, since we're already at maximum");
            return;
        }

        int nNewValue = arnPlayerProgress[iPlayer] + 1;

        RequestNewProgressValue(iPlayer, nNewValue);

    }

    public void DecrementProgress() {
        int iPlayer = taskmanager.nSelectedPlayer;

        if(arnPlayerProgress[iPlayer] == 0) {
            Debug.Log("No need to decrement, since we're already at 0");
            return;
        }

        int nNewValue = arnPlayerProgress[iPlayer] - 1;

        RequestNewProgressValue(iPlayer, nNewValue);

    }

    public void RequestNewProgressValue(int iPlayer, int nNewValue) {
        //If we have a NetworkMessenger spawned, then issue a network message through that
        if(NetworkSender.inst != null) {
            NetworkSender.inst.SendTaskProgress(id, iPlayer, nNewValue);
        } else {
            ChangeProgress(iPlayer, nNewValue);
        }
    }

    public void ChangeProgress(int iPlayer, int nNewValue) {

        int nOldProgress = arnPlayerProgress[iPlayer];

        if(nOldProgress == nNewValue) {
            Debug.Log("No need to do anything if this is already our value");
            return;
        }

        arnPlayerProgress[iPlayer] = nNewValue;

        UpdateVisualDescription();

        //If we've reached completion, then we should update colours and see if any lines are complete
        if(IsCompleteBy(iPlayer)) {

            taskmanager.lstAllPlayers[iPlayer].ReactClaimedTask(this);

            for(int i = 0; i < lstLinesIn.Count; i++) {
                lstLinesIn[i].UpdateCompletion(iPlayer);
            }

            UpdateVisualClaimed();
        } else if(nOldProgress == nParameterValue) {
            //If we previously were completed, then we no longer are

            taskmanager.lstAllPlayers[iPlayer].ReactUnclaimedTask(this);

            for(int i = 0; i < lstLinesIn.Count; i++) {
                lstLinesIn[i].UpdateCompletion(iPlayer);
            }

            UpdateVisualClaimed();
        }
    }


    public bool IsCompleteBy(int iPlayer) {
        return arnPlayerProgress[iPlayer] == nParameterValue;
    }

    public void Update() {

    }

    public override string ToString() {
        return GetFilledDescription();
    }

}
