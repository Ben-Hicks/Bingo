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
    public GameObject pfLine;

    public int nBoardSize = 5;
    public float fPercentDifficultyVariability = 12.5f; //The maximum difference away from average difficulty a task can be
    public int nLinesNeeded = 3;

    public float fHorizontalSpacing = 30f;
    public float fVerticalSpacing = 30f;

    public List<PossibleTask> lstAllPossibleTasks;

    public List<int> lstPossibleTaskIndicesToUse; //Keep a list of all indices of possible tasks that we can use
    public int iPossibleTaskIndex; //How far through our list of Possible Task Indices we've progressed through
    public Dictionary<int, List<Task>> dictUsedTasks; //Maps indices of lstAllPossibleTasks to the instances that it has been used so far 

    public int nBoardDifficulty;

    public void InitBingoBoard(int nSeed) {

        Random.InitState(nSeed);
        Debug.Log("Seed set to " + nSeed);

        //Create a randomized list of the possible tasks we will attempt to select from
        InitializeTaskIndices();
        iPossibleTaskIndex = 0;

        dictUsedTasks = new Dictionary<int, List<Task>>();

        lstBingoBoard = new List<Task>(nBoardSize * nBoardSize);

        for(int i = 0; i < nBoardSize; i++) {
            for(int j = 0; j < nBoardSize; j++) {

                GameObject goNewTask = Instantiate(pfTask, goBingoBoard.transform);
                goNewTask.transform.localPosition = new Vector3((j - nBoardSize / 2) * fHorizontalSpacing, (i - nBoardSize / 2) * fVerticalSpacing, 0f);

                Task newTask = goNewTask.GetComponent<Task>();

                //Scan through all the available tasks to be used until we find one that works
                while(iPossibleTaskIndex < lstPossibleTaskIndicesToUse.Count) {
                    if(AttemptFillOutTask(newTask) == false) {
                        //If we weren't successful in filling out this task, increment which task we're trying and try again
                        iPossibleTaskIndex++;

                        if(iPossibleTaskIndex == lstPossibleTaskIndicesToUse.Count) {
                            //If we scanned through the whole list and couldn't finish making a board, panic and quit
                            Debug.LogError("Failed to generate a random card at this difficulty!  I guess I'll die now...");
                            Debug.LogErrorFormat("Failed at i={0}, j={1}, iPossibleTaskIndex={2}, lstPossibleTaskIndicesToUse.Count={3}",
                                i, j, iPossibleTaskIndex, lstPossibleTaskIndicesToUse.Count);
                            Application.Quit();
                        }
                    } else {
                        //If we're successful, then we can break and end our job for this task
                        iPossibleTaskIndex++;
                        break;
                    }
                }



                lstBingoBoard.Add(newTask);

            }
        }


    }

    public void LoadAllPossibleTasks() {

        lstAllPossibleTasks = new List<PossibleTask>();

        string sFilePath = string.Concat(sLogFileDir, sLogFileName);

        if(File.Exists(sFilePath) == false) {
            Debug.LogErrorFormat("Path to file doesn't exist: {0}", sFilePath);
            return;
        }

        string[] arsLogLines = File.ReadAllLines(sFilePath);

        //For each line in the tasks file, create a PossibleTask
        foreach(string sLine in arsLogLines) {

            //If the line is empty or starts with a comment, then skip the line
            if(sLine == "" || sLine[0] == '#') continue;

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

                //Check if the Entry is malformed
                if(arsSplitEntry.Length != 2) {
                    Debug.LogErrorFormat("Error! {0} is a malformed entry", sEntry);
                }


                switch(arsSplitEntry[0]) {
                case "Desc":
                    newPossibleTask.SetRawDescription(arsSplitEntry[1]);
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

    public float GetMaxUsableDifficulty() {
        return nBoardDifficulty * (1 + fPercentDifficultyVariability);
    }

    public float GetMinUsableDifficulty() {
        return nBoardDifficulty * (1 - fPercentDifficultyVariability);
    }

    public bool AttemptFillOutTask(Task task) {

        //Get the next index we're set to use, and find the PossibleTask it refers to
        PossibleTask possibleTaskCur = lstAllPossibleTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]];

        //We have a PossibleTask to fill out - let's attempt a random selection and double-check that
        //  it doesn't conflict with any other instances of the same PossibleTask

        int nAttemptedValue = Random.Range(possibleTaskCur.nMinValue, possibleTaskCur.nMaxValue + 1);

        //First, check if the difficulty of the PossibleTask is close enough to the desired difficulty to be used
        if(possibleTaskCur.nMinDifficulty > GetMaxUsableDifficulty()) {
            //Skip because the lowest possible difficulty is still too large
            return false;
        }

        if(possibleTaskCur.nMaxDifficulty < GetMinUsableDifficulty()) {
            //Skip because the highest possible difficulty is still too small
            return false;
        }

        //Look up the other instances of this PossibleTask that have already been used
        if(dictUsedTasks.ContainsKey(lstPossibleTaskIndicesToUse[iPossibleTaskIndex]) == true) {
            //Fetch the list of prior tasks that are already using this PossibleTask
            List<Task> lstDuplicateTask = dictUsedTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]];

            //Currently just rejecting if an instance of this already exists
            Debug.LogFormat("Failed too many times to fill out {0} - Skipping...", possibleTaskCur);
            return false;

        } else {
            //If we didn't have an entry for this PossibleTask, then no conflicts exists and we're fine - can just generate a random value
            //  somewhere in the allowable overlap of ranges
            task.SetTask(possibleTaskCur);

            float fDesiredDifficulty = Random.Range(Mathf.Max(possibleTaskCur.nMinDifficulty, GetMinUsableDifficulty()),
                Mathf.Min(possibleTaskCur.nMaxDifficulty, GetMaxUsableDifficulty()));

            //Convert the desired difficulty scaling into the value that would give that difficulty scaling
            int nValue = Mathf.CeilToInt(Mathf.Lerp(possibleTaskCur.nMinValue, possibleTaskCur.nMaxValue,
                Mathf.InverseLerp(possibleTaskCur.nMinDifficulty, possibleTaskCur.nMaxDifficulty, fDesiredDifficulty)));

            task.SetParameterValue(nValue);

            //Since this entry didn't exist in our dictionary of used tasks, then we should initialize a list and add it to the dictionary
            dictUsedTasks.Add(lstPossibleTaskIndicesToUse[iPossibleTaskIndex], new List<Task>());
        }

        //If we've gotten to this point, then we know we passed all our validity checks and can add this task to the board -
        //   just have to make sure to record it in our dictionary of used tasks
        dictUsedTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]].Add(task);

        return true;
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


    public void CleanupBoard() {
        //Check the difficulty of each line to ensure they are approximately equal

    }

    public void DestroyBoard() {

    }

    public void GenerateRandomBoard() {

        DestroyBoard();
        InitBingoBoard(Random.Range(0, 10000));

    }

    // Start is called before the first frame update
    void Start() {

        LoadAllPossibleTasks();

        GenerateRandomBoard();

    }



    // Update is called once per frame
    void Update() {

    }
}
