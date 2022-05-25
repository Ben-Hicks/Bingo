using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TaskManager : MonoBehaviour {

    public const string sLogFileDir = "Tasks/";
    public string sLogFileName = "botw.tasks";

    public List<Task> lstBingoBoard;
    public GameObject goBingoBoard;

    public GameObject pfTask;
    public const int NBOARDSIZE = 5;
    public int nLinesNeeded = 3;

    public float fHorizontalSpacing = 30f;
    public float fVerticalSpacing = 30f;

    public List<PossibleTask> lstAllPossibleTasks;

    public List<int> lstPossibleTaskIndicesToUse; //Keep a list of all indices of possible tasks that we can use
    public int iPossibleTaskIndex; //How far through our list of Possible Task Indices we've progressed through
    public Dictionary<int, List<Task>> dictUsedTasks; //Maps indices of lstAllPossibleTasks to the instances that it has been used so far 

    public void InitBingoBoard() {

        //Create a randomized list of the possible tasks we will attempt to select from
        InitializeTaskIndices();
        iPossibleTaskIndex = 0;

        dictUsedTasks = new Dictionary<int, List<Task>>();

        lstBingoBoard = new List<Task>(NBOARDSIZE * NBOARDSIZE);

        for(int i = 0; i < NBOARDSIZE; i++) {
            for(int j = 0; j < NBOARDSIZE; j++) {

                GameObject goNewTask = Instantiate(pfTask, goBingoBoard.transform);
                goNewTask.transform.localPosition = new Vector3((j - NBOARDSIZE / 2) * fHorizontalSpacing, (i - NBOARDSIZE / 2) * fVerticalSpacing, 0f);

                Task newTask = goNewTask.GetComponent<Task>();

                FillOutTask(newTask);

                lstBingoBoard.Add(newTask);

            }
        }


    }

    public void LoadAllPossibleTasks() {

        lstAllPossibleTasks = new List<PossibleTask>();

        string[] arsLogLines = File.ReadAllLines(string.Concat(sLogFileDir, sLogFileName));

        //For each line in the tasks file, create a PossibleTask
        foreach(string sLine in arsLogLines) {

            PossibleTask newPossibleTask = new PossibleTask();

            //Entries for tasks are as follows:
            //Desc:<Description> (Required - do this first)
            //Value:<int>-<int> (If using a parameter, provide a range of values it can take)
            //Diff:<int> or Diff:<int>-<int> (Required - use either a fixed value or a range of difficulties)
            //Max-count:<int> (Define how many times this task can appear on the card)
            //Min-delta:<int> (The minimum difference in value needed between this task and others of the same type)

            //Any unspecified optional fields will try to take on a reasonable value
            string[] arsSplitLine = sLine.Split(',');

            foreach(string sEntry in arsSplitLine) {
                //For each entry of a task line, figure out what it's representing, then fill out the PossibleTask's field with the given value

                string[] arsSplitEntry = sEntry.Split(':');
                switch(arsSplitLine[0]) {
                case "Desc":
                    newPossibleTask.SetRawDescription(arsSplitLine[1]);
                    break;

                case "Value":
                    string[] arsSplitValue = arsSplitEntry[1].Split('-');

                    newPossibleTask.SetMinValue(int.Parse(arsSplitValue[0]));
                    newPossibleTask.SetMaxValue(int.Parse(arsSplitValue[1]));
                    break;

                case "Diff":
                    string[] arsSplitDiff = arsSplitEntry[1].Split('-');

                    //Check if we've given a range of difficulties or just a single fixed one
                    if(arsSplitDiff.Length == 1) {
                        //If we have a fixed difficulty
                        newPossibleTask.SetFixedDifficulty(int.Parse(arsSplitDiff[0]));
                    } else {
                        //If we have a range of difficulties
                        newPossibleTask.SetMinDifficulty(int.Parse(arsSplitDiff[0]));
                        newPossibleTask.SetMaxDifficulty(int.Parse(arsSplitDiff[1]));
                    }
                    break;

                case "Max-count":

                    newPossibleTask.SetMaxCount(int.Parse(arsSplitEntry[1]));
                    break;

                case "Min-delta":

                    newPossibleTask.SetMinDelta(int.Parse(arsSplitEntry[1]));
                    break;
                }


            }
            //Now that we've finished processing all the entries on this line, we can check if its fully filled out

            if(newPossibleTask.IsSufficientlyFilledOut() == false) {
                Debug.LogErrorFormat("Since {0} wasn't filled out properly, we're skipping it", newPossibleTask);
            } else {
                //If the task is properly filled out, then we can add it to our list of tasks we can select from
                lstAllPossibleTasks.Add(newPossibleTask);
            }
        }

        PrintAllPossibleTasks();
    }

    public void PrintAllPossibleTasks() {

        foreach(PossibleTask pt in lstAllPossibleTasks) {
            Debug.Log(pt);
        }

    }


    public void FillOutTask(Task task, int nFailures = 0) {

        //Get the next index we're set to use, and find the PossibleTask it refers to
        PossibleTask possibleTaskCur = lstAllPossibleTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]];

        if(nFailures > 10) {
            Debug.LogErrorFormat("Failed too many times to fill out {0} - Skipping...", possibleTaskCur);

            iPossibleTaskIndex++;
            //Recurse, but now we'll be looking at the next index in our list
            FillOutTask(task, 0);
        }

        //We have a PossibleTask to fill out - let's attempt a random selection and double-check that
        //  it doesn't conflict with any other instances of the same PossibleTask

        int nAttemptedValue = Random.Range(possibleTaskCur.nMinValue, possibleTaskCur.nMaxValue + 1);

        //Look up the other instances of this PossibleTask that have already been used
        if(dictUsedTasks.ContainsKey(lstPossibleTaskIndicesToUse[iPossibleTaskIndex])) {

        }
        List<Task> lstDuplicateTask = dictUsedTasks

    }


    //Generate a list of all the indices we can select our tasks from (in a random order and
    //   with duplicates if the task allows them)
    public void InitializeTaskIndices() {

        lstPossibleTaskIndicesToUse = new List<int>();

        for(int i = 0; i < lstAllPossibleTasks.Count; i++) {
            for(int j = 0; j < lstAllPossibleTasks[i].nMaxCount; j++) {
                lstPossibleTaskIndicesToUse.Add(i);
            }
        }

        //Now scramble the list of indices randomly (swap each element with a random one later(ish) in the list
        for(int i = 0; i < lstPossibleTaskIndicesToUse.Count; ++i) {
            int j = Random.Range(i, lstPossibleTaskIndicesToUse.Count);
            var tmp = lstPossibleTaskIndicesToUse[i];
            lstPossibleTaskIndicesToUse[i] = lstPossibleTaskIndicesToUse[j];
            lstPossibleTaskIndicesToUse[j] = tmp;
        }

    }

    // Start is called before the first frame update
    void Start() {

        LoadAllPossibleTasks();

        InitBingoBoard();

    }

    // Update is called once per frame
    void Update() {

    }
}
