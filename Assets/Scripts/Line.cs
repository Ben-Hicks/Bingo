using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line {

    public List<Task> lstTasks;
    public bool[] arbCompletedBy;

    public Line() {

        arbCompletedBy = new bool[TaskManager.inst.lstAllPlayers.Count];
        lstTasks = new List<Task>();

    }

    public void AddTask(Task task) {
        lstTasks.Add(task);
        task.lstLinesIn.Add(this);
    }

    public float GetTotalDifficulty() {

        float fSum = 0.0f;

        foreach(Task t in lstTasks) {
            fSum += t.fDifficulty;
        }

        return fSum;
    }

    public override string ToString() {
        return string.Format("{0} - {1}", lstTasks[0], lstTasks[lstTasks.Count - 1]);
    }

    public bool IsCompleteBy(int iPlayer) {
        for(int i = 0; i < lstTasks.Count; i++) {
            if(lstTasks[i].arbCompletedBy[iPlayer] == false) {
                return false;
            }
        }

        return true;
    }

    public void UpdateCompletion(int iPlayer) {

        bool bIsNowComplete = IsCompleteBy(iPlayer);
        Debug.LogFormat("Line({0}) completion: {1}", ToString(), bIsNowComplete);

        if(bIsNowComplete && arbCompletedBy[iPlayer] == false) {
            //Then this is a newly completed line
            arbCompletedBy[iPlayer] = true;
            TaskManager.inst.lstAllPlayers[iPlayer].ReactCompletedLine(this);
        } else if(bIsNowComplete == false && arbCompletedBy[iPlayer]) {
            //Then this is now incomplete
            arbCompletedBy[iPlayer] = false;
            TaskManager.inst.lstAllPlayers[iPlayer].ReactUncompletedLine(this);
        }
    }

}
