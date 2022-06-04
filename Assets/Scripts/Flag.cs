using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flag : MonoBehaviour {

    public Task task;

    public Color colFlagged;
    public Color colUnflagged;
    public Color colVisible;

    public Image imgFlag;

    public bool[] arbFlagged;
    public bool bVisible;



    public void SetVisible() {
        bVisible = true;
        UpdateVisual();
    }

    public void UnsetVisible() {
        bVisible = false;
        UpdateVisual();
    }

    public void OnClickFlag() {

        if(NetworkSender.inst != null) {
            NetworkSender.inst.SendToggleFlag(task.id, TaskManager.inst.nSelectedPlayer);
        } else {
            ToggleFlag(TaskManager.inst.nSelectedPlayer);
        }

    }

    public void ToggleFlag(int iPlayer) {
        if(arbFlagged[iPlayer]) UnsetFlag(iPlayer);
        else SetFlag(iPlayer);
    }


    public void SetFlag(int iPlayer) {
        arbFlagged[iPlayer] = true;
        TaskManager.inst.lstAllPlayers[iPlayer].ReactFlaggedTask(this);
        UpdateVisual();
    }

    public void UnsetFlag(int iPlayer) {
        arbFlagged[iPlayer] = false;
        TaskManager.inst.lstAllPlayers[iPlayer].ReactUnflaggedTask(this);
        UpdateVisual();
    }

    public void UpdateVisual() {

        if(arbFlagged[TaskManager.inst.nSelectedPlayer]) {
            imgFlag.color = colFlagged;
        } else if(bVisible) {
            imgFlag.color = colVisible;
        } else {
            imgFlag.color = colUnflagged;
        }

    }

    public void Start() {
        UpdateVisual();
    }
}
