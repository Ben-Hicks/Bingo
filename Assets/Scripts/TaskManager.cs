using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class TaskManager : MonoBehaviour {

    public static TaskManager inst;

    public const string sLogFileDir = "Tasks/";
    public Dropdown dropdownTaskFile;
    public List<string> lstTaskFileOptions;
    public string sTaskFileName;
    public List<Player> lstAllPlayers;

    public List<Task> lstBingoBoard;
    public GameObject goBingoBoard;

    public GameObject pfTask;
    public GameObject pfLine;

    public float fHorizontalSpacing = 30f;
    public float fVerticalSpacing = 30f;

    public string sLoadedTasks;
    public List<PossibleTask> lstAllPossibleTasks;

    public List<int> lstPossibleTaskIndicesToUse; //Keep a list of all indices of possible tasks that we can use
    public int iPossibleTaskIndex; //How far through our list of Possible Task Indices we've progressed through
    public Dictionary<int, List<Task>> dictUsedTasks; //Maps indices of lstAllPossibleTasks to the instances that it has been used so far 

    public GenerationParam pBoardSize;
    public GenerationParam pBoardDifficulty;
    public GenerationParam pPercentDifficultyVariability;
    public GenerationParam pLinesNeeded;
    public InputField inputSeed;
    public int nSeed;

    public int nSelectedPlayer;


    public List<Line> lstLines;

    private void Awake() {
        inst = this;
    }

    public int GetBoardSize() {
        return pBoardSize.GetIntValue();
    }

    public Task GetTask(int i, int j) {
        return lstBingoBoard[i * GetBoardSize() + j];
    }

    public void InitLines() {
        lstLines = new List<Line>();

        Line linePosDiag = new Line();
        Line lineNegDiag = new Line();

        //Create a Line for each Row and Column
        for(int i = 0; i < GetBoardSize(); i++) {

            Line lineRow = new Line();
            Line lineColumn = new Line();

            for(int j = 0; j < GetBoardSize(); j++) {
                lineRow.AddTask(GetTask(i, j));
                lineColumn.AddTask(GetTask(j, i));
            }

            lstLines.Add(lineRow);
            lstLines.Add(lineColumn);

            //Add an entry for the diagonals
            linePosDiag.AddTask(GetTask(i, i));
            lineNegDiag.AddTask(GetTask(i, GetBoardSize() - i - 1));
        }

        lstLines.Add(linePosDiag);
        lstLines.Add(lineNegDiag);

    }

    public void CheckIfNoMoreTasks() {
        if(iPossibleTaskIndex >= lstPossibleTaskIndicesToUse.Count) {
            //If we scanned through the whole list and couldn't finish making a board, panic and quit
            Debug.LogError("Failed to generate a random card at this difficulty!  I guess I'll die now...");
            //TODO - make this a little friendlier
            Application.Quit();
        }
    }

    public void InitBingoBoard(int nSeed) {

        Random.InitState(nSeed);
        Debug.Log("Seed set to " + nSeed);

        //Load in all the tasks needed for our current .tasks selection
        LoadAllPossibleTasks();

        //Create a randomized list of the possible tasks we will attempt to select from
        InitializeTaskIndices();
        iPossibleTaskIndex = 0;

        dictUsedTasks = new Dictionary<int, List<Task>>();

        lstBingoBoard = new List<Task>(GetBoardSize() * GetBoardSize());

        for(int i = 0; i < GetBoardSize(); i++) {
            for(int j = 0; j < GetBoardSize(); j++) {

                CheckIfNoMoreTasks();

                GameObject goNewTask = Instantiate(pfTask, goBingoBoard.transform);
                goNewTask.transform.localPosition = new Vector3((j - GetBoardSize() / 2) * fHorizontalSpacing, (i - GetBoardSize() / 2) * fVerticalSpacing, 0f);


                Task newTask = goNewTask.GetComponent<Task>();
                newTask.Init(this);
                newTask.SetId(lstBingoBoard.Count);

                //Scan through all the available tasks to be used until we find one that works
                while(iPossibleTaskIndex < lstPossibleTaskIndicesToUse.Count) {
                    bool bSuccess = AttemptFillOutTask(newTask);

                    iPossibleTaskIndex++;

                    if(bSuccess) {
                        //If we're successful, then we can break and end our job for this task
                        break;
                    }

                }

                lstBingoBoard.Add(newTask);

            }
        }

        InitLines();
        //CleanupBoard();

    }

    public void LoadAllPossibleTasks() {

        if(sTaskFileName == sLoadedTasks) {
            //We already have this set of tasks loaded, so no need to do anything extra
            Debug.Log("This task set is already loaded - no need to re-load anything");
            return;
        }

        string sFilePath = string.Format("{0}{1}.tasks", sLogFileDir, sTaskFileName);

        if(File.Exists(sFilePath) == false) {
            Debug.LogErrorFormat("Path to file doesn't exist: {0}", sFilePath);
            return;
        }

        //Record that this is the set of tasks that we have loaded in currently
        sLoadedTasks = sTaskFileName;

        lstAllPossibleTasks = new List<PossibleTask>();

        string[] arsTaskLines = File.ReadAllLines(sFilePath);

        //For each line in the tasks file, create a PossibleTask
        foreach(string sLine in arsTaskLines) {

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

                case "URL":

                    newPossibleTask.SetURL(arsSplitEntry[1]);
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
        return pBoardDifficulty.GetValue() * (1 + pPercentDifficultyVariability.GetValue());
    }

    public float GetMinUsableDifficulty() {
        return pBoardDifficulty.GetValue() * (1 - pPercentDifficultyVariability.GetValue());
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

            //Debug.LogFormat("{0} has {1} duplicates already", possibleTaskCur, lstDuplicateTask.Count);

            int nAttempts = 10;
            while(nAttempts-- > 0) {

                //For each attempt, generate a random difficulty
                float fDesiredDifficulty = GetRandomDifficultyInRange(possibleTaskCur);

                //Convert it into a parameter value of that difficulty
                nValueToSet = DifficultyToValue(possibleTaskCur, fDesiredDifficulty);

                bool bFail = false;

                //Now loop through all of the existing Tasks that use the same PossibleTask and ensure we're not too close in value to any of them
                for(int i = 0; i < lstDuplicateTask.Count; i++) {
                    //Debug.LogFormat("Is new value {0} too close to existing duplicate value {1}?: {2}", nValueToSet, lstDuplicateTask[i].nParameterValue,
                    //Mathf.Abs(nValueToSet - lstDuplicateTask[i].nParameterValue) < possibleTaskCur.nMinDelta);
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
                //Debug.LogFormat("Failed too many times to fill out {0} - Skipping...", possibleTaskCur);
                return false;
            } else {
                //Debug.LogFormat("Successfully adding duplicate with value {0}", nValueToSet);
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

    public void SwapTasks(Task t1, Task t2) {

        PossibleTask ptSwap = t1.taskBase;
        int nValueSwap = t1.nParameterValue;

        t1.SetTask(t2.taskBase);
        t1.SetParameterValue(t2.nParameterValue);

        t2.SetTask(ptSwap);
        t2.SetParameterValue(nValueSwap);

    }

    public void CleanupBoard() {

        //Start with randommly scrambling the tasks so there's no positional bias of tasks
        for(int i = 0; i < lstBingoBoard.Count - 1; i++) {
            //Randomly select a later index to swap to position i
            int j = Random.Range(i, lstBingoBoard.Count);

            SwapTasks(lstBingoBoard[i], lstBingoBoard[j]);
        }

        //Check the difficulty of each line to ensure they are approximately equal

        Debug.LogFormat("Difficulty Range: {0} - {1}", GetEasiestLine().GetTotalDifficulty(), GetHardestLine().GetTotalDifficulty());

    }

    public void DestroyBoard() {

        for(int i = 0; i < lstBingoBoard.Count; i++) {

            //Unclaim all claims players have over this task
            for(int j = 0; j < lstBingoBoard[i].arbCompletedBy.Length; j++) {
                if(lstBingoBoard[i].arbCompletedBy[j]) {

                    lstBingoBoard[i].Unclaim(j);

                }

                if(lstBingoBoard[i].flag.arbFlagged[j]) {

                    lstBingoBoard[i].flag.UnsetFlag(j);

                }
            }

            //Destroy the associated gameobject with this task
            Destroy(lstBingoBoard[i].gameObject);
        }

    }



    public void SetTaskFile(string _sTaskFileName) {
        Debug.Log("Setting task file to " + _sTaskFileName);
        sTaskFileName = _sTaskFileName;

        //Update the dropdown to the corresponding entry we've set
        for(int i = 0; i < lstTaskFileOptions.Count; i++) {
            if(sTaskFileName == lstTaskFileOptions[i]) {
                dropdownTaskFile.SetValueWithoutNotify(i);
                return;
            }
        }
        Debug.LogErrorFormat("Couldn't find the corresponding task file");
    }

    public void OnDropdownTaskFileChange() {
        SetTaskFile(lstTaskFileOptions[dropdownTaskFile.value]);
    }

    public void InitTaskFileOptions() {

        //Search through all the .tasks files in our Tasks Directory and add them to our list
        lstTaskFileOptions = Directory.GetFiles(sLogFileDir, "*.tasks").Select((string sPath) => Path.GetFileNameWithoutExtension(sPath)).ToList();

        //Update the dropdown to have these as our options
        dropdownTaskFile.ClearOptions();

        dropdownTaskFile.AddOptions(lstTaskFileOptions.Select((string s) => new Dropdown.OptionData(s)).ToList());

        dropdownTaskFile.SetValueWithoutNotify(0);
        OnDropdownTaskFileChange();
    }

    public void SetSeed(int _nSeed) {
        nSeed = _nSeed;
        inputSeed.SetTextWithoutNotify(nSeed.ToString());
    }

    public void OnSeedChange() {
        int nNewSeed = 0;
        if(int.TryParse(inputSeed.text, out nNewSeed) == false) {
            Debug.LogFormat("Failed to parse {0} as a seed", inputSeed.text);
        }
        SetSeed(nNewSeed);
    }

    public void SetBoardSize(int _nBoardSize) {
        pBoardSize.SetValue(_nBoardSize);
    }

    public void SetDifficulty(float _fDifficulty) {
        pBoardDifficulty.SetValue(_fDifficulty);
    }

    public void SetDifficultyVariability(float _fDifficultyVariability) {
        pPercentDifficultyVariability.SetValue(_fDifficultyVariability);
    }

    public void SetLinesNeeded(int _nLinesNeeded) {
        pLinesNeeded.SetValue(_nLinesNeeded);
    }

    public void GenerateBoard() {
        DestroyBoard();

        if(nSeed == 0) {
            //If we haven't set a seed, then just fill in a random one
            SetSeed(Random.Range(0, 1000000));
        }

        InitBingoBoard(nSeed);
    }

    public void RequestBoardGeneration() {
        if(nSeed == 0) {
            //If we haven't set a seed, then just fill in a random one
            SetSeed(Random.Range(0, 1000000));
        }

        //If we have a NetworkSender, then send a generation request through it
        if(NetworkSender.inst != null) {
            NetworkSender.inst.SendGenerateBoard(sTaskFileName, pBoardSize.GetIntValue(),
                pBoardDifficulty.GetValue(), pPercentDifficultyVariability.GetValue(),
                pLinesNeeded.GetIntValue(), nSeed);
        } else {
            //Otherwise, just generate a board locally
            GenerateBoard();
        }
    }

    // Start is called before the first frame update
    void Start() {

        //Initialize options for the various tasks files we have
        InitTaskFileOptions();

    }



    // Update is called once per frame
    void Update() {

    }

    public void SetSelectedPlayer(int _id) {

        int nPrevSelection = nSelectedPlayer;

        nSelectedPlayer = _id;

        if(nSelectedPlayer != nPrevSelection) {
            //If we're changing the target to something different, then we'll need to change some selection status
            lstAllPlayers[nPrevSelection].OnDeselectPlayer();
            lstAllPlayers[nSelectedPlayer].OnSelectPlayer();
        }

    }
}
