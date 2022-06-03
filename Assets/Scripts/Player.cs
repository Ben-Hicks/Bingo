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
    public List<Line> lstLinesCompleted;

    public FlexibleColorPicker colourpicker;
    public InputField inputName;

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

        SelectPlayer();
    }

    public void SetName(string _sName) {
        sName = _sName;
        inputName.SetTextWithoutNotify(sName);
    }

    public void OnNameChange() {
        sName = inputName.text;
        SelectPlayer();
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

    public void SelectPlayer() {
        taskmanager.SetSelectedPlayer(id);
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

    public void OnSelectPlayer() {
        //Debug.LogFormat("Selecting {0}", id);
        imgSelected.enabled = true;
    }

    public void OnDeselectPlayer() {
        //Debug.LogFormat("Unselecting {0}", id);
        imgSelected.enabled = false;
    }


}
