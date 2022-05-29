using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class TaskManager : MonoBehaviour {

    public const string sLogFileDir = "Tasks/";
    public string sLogFileName = "botw.tasks";
    public List<Player> lstAllPlayers;

    public List<GameObject> lstGoTasks;
    public List<Task> lstBingoBoard;
    public GameObject goBingoBoard;

    public GameObject pfTask;
    public GameObject pfLine;

    public float fHorizontalSpacing = 30f;
    public float fVerticalSpacing = 30f;

    public List<PossibleTask> lstAllPossibleTasks;

    public List<int> lstPossibleTaskIndicesToUse; //Keep a list of all indices of possible tasks that we can use
    public int iPossibleTaskIndex; //How far through our list of Possible Task Indices we've progressed through
    public Dictionary<int, List<Task>> dictUsedTasks; //Maps indices of lstAllPossibleTasks to the instances that it has been used so far 

    public GenerationParam pBoardSize;
    public GenerationParam pBoardDifficulty;
    public GenerationParam pPercentDifficultyVariability;
    public GenerationParam pLinesNeeded;

    public int nSelectedPlayer;

    public List<Line> lstLines;

    public int GetBoardSize() {
        return Mathf.CeilToInt(pBoardSize.fValue);
    }

    public Task GetTask(int i, int j) {
        return lstBingoBoard[i * GetBoardSize() + j];
    }

    public void InitLines() {
        lstLines = new List<Line>();

        List<Task> lstPosDiag = new List<Task>();
        List<Task> lstNegDiag = new List<Task>();

        //Create a Line for each Row and Column
        for(int i = 0; i < GetBoardSize(); i++) {

            List<Task> lstLineRow = new List<Task>();
            List<Task> lstLineColumn = new List<Task>();

            for(int j = 0; j < GetBoardSize(); j++) {
                lstLineRow.Add(GetTask(i, j));
                lstLineColumn.Add(GetTask(j, i));
            }

            lstLines.Add(new Line(lstLineRow));
            lstLines.Add(new Line(lstLineColumn));

            //Add an entry for the diagonals
            lstPosDiag.Add(GetTask(i, i));
            lstNegDiag.Add(GetTask(i, GetBoardSize() - i - 1));
        }

        lstLines.Add(new Line(lstPosDiag));
        lstLines.Add(new Line(lstNegDiag));

    }

    public void InitBingoBoard(int nSeed) {

        Random.InitState(nSeed);
        Debug.Log("Seed set to " + nSeed);

        //Create a randomized list of the possible tasks we will attempt to select from
        InitializeTaskIndices();
        iPossibleTaskIndex = 0;

        dictUsedTasks = new Dictionary<int, List<Task>>();

        lstGoTasks = new List<GameObject>(GetBoardSize() * GetBoardSize());
        lstBingoBoard = new List<Task>(GetBoardSize() * GetBoardSize());

        for(int i = 0; i < GetBoardSize(); i++) {
            for(int j = 0; j < GetBoardSize(); j++) {

                GameObject goNewTask = Instantiate(pfTask, goBingoBoard.transform);
                goNewTask.transform.localPosition = new Vector3((j - GetBoardSize() / 2) * fHorizontalSpacing, (i - GetBoardSize() / 2) * fVerticalSpacing, 0f);
                lstGoTasks.Add(goNewTask);

                Task newTask = goNewTask.GetComponent<Task>();
                newTask.Init(this);

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

        InitLines();
        CleanupBoard();

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

                case "Freq":

                    newPossibleTask.SetFrequencyModifier(float.Parse(arsSplitEntry[1]));
                    break;

                default:

                    Debug.LogErrorFormat("Unrecognized entry: {0}", arsSplitEntry[0]);
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
            //Debug.Log(pt);
        }

    }

    public float GetMaxUsableDifficulty() {
        return pBoardDifficulty.fValue * (1 + pPercentDifficultyVariability.fValue);
    }

    public float GetMinUsableDifficulty() {
        return pBoardDifficulty.fValue * (1 - pPercentDifficultyVariability.fValue);
    }

    public float GetRandomDifficultyInRange(PossibleTask possibleTask) {
        return Random.Range(Mathf.Max(possibleTask.nMinDifficulty, GetMinUsableDifficulty()),
            Mathf.Min(possibleTask.nMaxDifficulty, GetMaxUsableDifficulty()));
    }

    public int DifficultyToValue(PossibleTask possibleTask, float fDesiredDifficulty) {
        return Mathf.CeilToInt(Mathf.Lerp(possibleTask.nMinValue, possibleTask.nMaxValue,
                Mathf.InverseLerp(possibleTask.nMinDifficulty, possibleTask.nMaxDifficulty, fDesiredDifficulty)));
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


        int nValueToSet = 0;

        //Look up the other instances of this PossibleTask that have already been used
        if(dictUsedTasks.ContainsKey(lstPossibleTaskIndicesToUse[iPossibleTaskIndex]) == true) {
            //Fetch the list of prior tasks that are already using this PossibleTask
            List<Task> lstDuplicateTask = dictUsedTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]];

            Debug.LogFormat("{0} has {1} duplicates already", possibleTaskCur, lstDuplicateTask.Count);

            int nAttempts = 10;
            while(nAttempts-- > 0) {

                //For each attempt, generate a random difficulty
                float fDesiredDifficulty = GetRandomDifficultyInRange(possibleTaskCur);

                //Convert it into a parameter value of that difficulty
                nValueToSet = DifficultyToValue(possibleTaskCur, fDesiredDifficulty);

                bool bFail = false;

                //Now loop through all of the existing Tasks that use the same PossibleTask and ensure we're not too close in value to any of them
                for(int i = 0; i < lstDuplicateTask.Count; i++) {
                    Debug.LogFormat("Is new value {0} too close to existing duplicate value {1}?: {2}", nValueToSet, lstDuplicateTask[i].nParameterValue,
                        Mathf.Abs(nValueToSet - lstDuplicateTask[i].nParameterValue) < possibleTaskCur.nMinDelta);
                    if(Mathf.Abs(nValueToSet - lstDuplicateTask[i].nParameterValue) < possibleTaskCur.nMinDelta) {
                        bFail = true;
                        break;
                    }
                }

                //If we make it to this point and we didn't fail, then we successfully found a duplicate parameter value we can use
                if(bFail == false) {
                    break;
                }
                //If we encountered a too-close duplicate, then continue on to the next attempt

            }

            //After exiting the loop, check if we ran out of attempts or not
            if(nAttempts <= 0) {
                Debug.LogFormat("Failed too many times to fill out {0} - Skipping...", possibleTaskCur);
                return false;
            } else {
                Debug.LogFormat("Successfully adding duplicate with value {0}", nValueToSet);
            }

        } else {
            //If we didn't have an entry for this PossibleTask, then no conflicts exists and we're fine - can just generate a random value
            //  somewhere in the allowable overlap of ranges

            float fDesiredDifficulty = GetRandomDifficultyInRange(possibleTaskCur);

            //Convert the desired difficulty scaling into the value that would give that difficulty scaling
            nValueToSet = DifficultyToValue(possibleTaskCur, fDesiredDifficulty);

            //Since this entry didn't exist in our dictionary of used tasks, then we should initialize a list and add it to the dictionary
            dictUsedTasks.Add(lstPossibleTaskIndicesToUse[iPossibleTaskIndex], new List<Task>());
        }

        task.SetTask(possibleTaskCur);

        task.SetParameterValue(nValueToSet);

        //If we've gotten to this point, then we know we passed all our validity checks and can add this task to the board -
        //   just have to make sure to record it in our dictionary of used tasks
        dictUsedTasks[lstPossibleTaskIndicesToUse[iPossibleTaskIndex]].Add(task);

        return true;
    }

    class Descending : IComparer<float> {

        public int Compare(float f1, float f2) {
            if(f1 > f2) return 1;
            else if(f1 < f2) return -1;
            else return 0;
        }
    }

    //Generate a list of all the indices we can select our tasks from (in a random order and
    //   with duplicates if the task allows them)
    public void InitializeTaskIndices() {

        //Make a list of pairs of biased randomized priorities and associated indices
        List<KeyValuePair<float, int>> lstPriorityPairs = new List<KeyValuePair<float, int>>();

        for(int i = 0; i < lstAllPossibleTasks.Count; i++) {
            for(int j = 0; j < lstAllPossibleTasks[i].nMaxCount; j++) {
                lstPriorityPairs.Add(new KeyValuePair<float, int>(lstAllPossibleTasks[i].fFrequencyModifier * Random.Range(0f, 1f), i));

            }
        }

        //Now sort the list to be used for Task selection, and discard the randomized priority
        lstPossibleTaskIndicesToUse = lstPriorityPairs.OrderByDescending(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

    }

    public Line GetHardestLine() {

        float fCurMax = lstLines[0].GetTotalDifficulty();
        Line lineCurBest = lstLines[0];

        foreach(Line l in lstLines) {
            float fCurDiff = l.GetTotalDifficulty();
            if(fCurDiff > fCurMax) {
                lineCurBest = l;
                fCurMax = fCurDiff;
            }
        }

        return lineCurBest;
    }

    public Line GetEasiestLine() {

        float fCurMin = lstLines[0].GetTotalDifficulty();
        Line lineCurBest = lstLines[0];

        foreach(Line l in lstLines) {
            float fCurDiff = l.GetTotalDifficulty();
            if(fCurDiff < fCurMin) {
                lineCurBest = l;
                fCurMin = fCurDiff;
            }
        }

        return lineCurBest;
    }

    public void CleanupBoard() {
        //Check the difficulty of each line to ensure they are approximately equal

        Debug.LogFormat("Difficulty Range: {0} - {1}", GetEasiestLine().GetTotalDifficulty(), GetHardestLine().GetTotalDifficulty());

    }

    public void DestroyBoard() {

        for(int i = 0; i < lstGoTasks.Count; i++) {
            Destroy(lstGoTasks[i]);
        }

        lstGoTasks = null;
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

    public void SetSelectedPlayer(int _id) {

        int nPrevSelection = nSelectedPlayer;

        nSelectedPlayer = _id;

        Debug.LogFormat("{0} selected with previous id {1}", nSelectedPlayer, nPrevSelection);

        if(nSelectedPlayer != nPrevSelection) {
            //If we're changing the target to something different, then we'll need to change some selection status
            lstAllPlayers[nPrevSelection].OnDeselectPlayer();
            lstAllPlayers[nSelectedPlayer].OnSelectPlayer();
        }

    }
}
