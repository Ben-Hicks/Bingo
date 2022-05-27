using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line {


    public List<Task> lstTasks;

    public Line(List<Task> _lstTasks) {

        lstTasks = _lstTasks;

    }

    public float GetTotalDifficulty() {

        float fSum = 0.0f;

        foreach(Task t in lstTasks) {
            fSum += t.fDifficulty;
        }

        return fSum;
    }


}
