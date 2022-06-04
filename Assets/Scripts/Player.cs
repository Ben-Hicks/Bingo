using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public TaskManager taskmanager;

    public string sName;
    public int id;
    public Color colClaimed;

    private bool bColourChanged;
    private float fCurRefreshColourTime;
    public const float fREFRESHCOLOURFREQUENCY = 0.25f;

    public List<Task> lstTasksClaimed;
    public List<Flag> lstFlags;
    public List<Line> lstLinesCompleted;

    public FlexibleColorPicker colourpicker;
    public InputField inputName;
    public GameObject goColorPreview;
    public GameObject goColorSelecter;
    public const float fLockedColorPreviewTop = 0f;
    public const float fLockedColorPreviewBot = -20f;

    public Text txtTasksClaimed;
    public Text txtLinesCompleted;

    public Image imgSelected;


    public void SetColour(Color _colClaimed) {
        colClaimed = _colClaimed;

        colourpicker.SetColor(colClaimed);
    }

    public void OnColourChange() {

        if(colClaimed != colourpicker.color) {
            colClaimed = colourpicker.color;
            bColourChanged = true;
        }

        AttemptPlayerSelection();
    }

    public void SetName(string _sName) {
        sName = _sName;
        inputName.SetTextWithoutNotify(sName);
    }

    public void OnNameChange() {
        sName = inputName.text;
        AttemptPlayerSelection();
    }

    public void OnFinishNameChange() {
        //When we finish editing a name, send the result over the network
        if(NetworkSender.inst != null) {
            NetworkSender.inst.SendNameChange(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        lstLinesCompleted = new List<Line>();
        lstTasksClaimed = new List<Task>();
        lstFlags = new List<Flag>();
    }

    public void UpdateBoardClaimColour() {
        //Go through all the Tasks we've claimed and have them update their
        //  graphics to our new colour (only triggering periodically so we don't get spammed with colour change
        //  events while we're making a change

        for(int i = 0; i < lstTasksClaimed.Count; i++) {
            lstTasksClaimed[i].UpdateVisualClaimed();
        }
    }

    // Update is called once per frame
    void Update() {

        fCurRefreshColourTime += Time.deltaTime;

        if(fCurRefreshColourTime > fREFRESHCOLOURFREQUENCY && bColourChanged) {

            if(NetworkSender.inst != null) {
                NetworkSender.inst.SendColorChange(this);
            } else {
                UpdateBoardClaimColour();
            }

            fCurRefreshColourTime = 0f;
            bColourChanged = false;
        }

    }

    public void UpdateVisualLinesCompleted() {
        txtLinesCompleted.text = lstLinesCompleted.Count.ToString();
    }

    public void UpdateVisualTasksCompleted() {
        txtTasksClaimed.text = lstTasksClaimed.Count.ToString();
    }

    public void AttemptPlayerSelection() {
        if(NetworkSender.inst != null) {
            //If we're in a networked room, then we don't need to ever reselect - it's locked
            //  to whichever player is controlled locally
            return;
        } else {
            //If were not networked, just select this player
            taskmanager.SetSelectedPlayer(id);
        }
    }

    public void ReactClaimedTask(Task task) {

        if(lstTasksClaimed.Contains(task)) {
            Debug.LogErrorFormat("{0} already owns {1}!", sName, task);
        }
        lstTasksClaimed.Add(task);

        UpdateVisualTasksCompleted();
    }

    public void ReactUnclaimedTask(Task task) {

        if(lstTasksClaimed.Contains(task) == false) {
            Debug.LogErrorFormat("{0} doesn't own {1}!", sName, task);
        }
        lstTasksClaimed.Remove(task);

        UpdateVisualTasksCompleted();
    }

    public void ReactCompletedLine(Line line) {

        if(lstLinesCompleted.Contains(line)) {
            Debug.LogErrorFormat("{0} already completed {1}!", sName, line);
        }
        lstLinesCompleted.Add(line);

        UpdateVisualLinesCompleted();
    }

    public void ReactUncompletedLine(Line line) {

        if(lstLinesCompleted.Contains(line) == false) {
            Debug.LogErrorFormat("{0} doesn't own {1}!", sName, line);
        }
        lstLinesCompleted.Remove(line);

        UpdateVisualLinesCompleted();
    }

    public void ReactFlaggedTask(Flag flag) {
        lstFlags.Add(flag);
    }

    public void ReactUnflaggedTask(Flag flag) {
        lstFlags.Remove(flag);
    }

    public void OnSelectPlayer() {
        //Debug.LogFormat("Selecting {0}", id);
        imgSelected.enabled = true;
        UpdateFlagVisuals();
    }

    public void OnDeselectPlayer() {
        //Debug.LogFormat("Unselecting {0}", id);
        imgSelected.enabled = false;
        UpdateFlagVisuals();
    }

    public void UpdateFlagVisuals() {
        for(int i = 0; i < lstFlags.Count; i++) {
            lstFlags[i].UpdateVisual();
        }
    }

    public void LockUIModifiers() {
        //Stop the name from being edited
        inputName.interactable = false;

        //And disable the Colour-picking UI element
        goColorSelecter.SetActive(false);

        //And expand the Color-preview window to cover the empty space of the Colour picker
        goColorPreview.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, fLockedColorPreviewTop - fLockedColorPreviewBot);
        goColorPreview.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (fLockedColorPreviewBot - fLockedColorPreviewTop));
    }

}
