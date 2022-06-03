using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line {


    public List<Task> lstTasks;

    public Line() {

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


}
