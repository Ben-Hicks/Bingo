using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveManager : MonoBehaviour {

    public const string sSaveFileDir = "Saves/";

    public TaskManager taskmanager;
    public InputField inputRoomName;

    StreamWriter swFileWriter;


    public void SaveToFile() {
        //Touch the Saves directory
        Directory.CreateDirectory(sSaveFileDir);

        string sSaveFilePath = string.Format("{0}{1}.bingo", sSaveFileDir, inputRoomName.text);

        //Attempt to initialize the writer
        swFileWriter = new StreamWriter(sSaveFilePath, false);

        swFileWriter.WriteLine(string.Format("Tasks:{0}", taskmanager.sTaskFileName));
        swFileWriter.WriteLine(string.Format("Seed:{0}", taskmanager.nSeed));
        swFileWriter.WriteLine(string.Format("Size:{0}", taskmanager.pBoardSize.GetIntValue()));
        swFileWriter.WriteLine(string.Format("Difficulty:{0}", taskmanager.pBoardDifficulty.GetValue()));
        swFileWriter.WriteLine(string.Format("Variability:{0}", taskmanager.pPercentDifficultyVariability.GetValue()));
        swFileWriter.WriteLine(string.Format("Lines:{0}", taskmanager.pLinesNeeded.GetIntValue()));
        swFileWriter.WriteLine("EndGenerationParams");

        for(int i = 0; i < taskmanager.lstAllPlayers.Count; i++) {
            Color colClaimed = taskmanager.lstAllPlayers[i].colClaimed;
            swFileWriter.WriteLine(string.Format("Player:{0}:{1}:Colour:{2}:{3}:{4}", i, taskmanager.lstAllPlayers[i].sName,
                colClaimed.r, colClaimed.g, colClaimed.b));
        }

        for(int i = 0; i < taskmanager.lstBingoBoard.Count; i++) {
            Task taskCur = taskmanager.lstBingoBoard[i];
            //Build a string that has entries for each player that has completed the task
            string sTaskEntry = string.Format("Task:{0}:PlayerEntries", i);

            for(int j = 0; j < taskCur.arbCompletedBy.Length; j++) {
                sTaskEntry += string.Format(":{0},{1},{2}", j, taskCur.arbCompletedBy[j], taskCur.flag.arbFlagged[j]);
            }

            swFileWriter.WriteLine(sTaskEntry);
        }

        swFileWriter.Close();
    }


    public void LoadFromFile() {

        int nSelecting = taskmanager.nSelectedPlayer;

        string sSaveFilePath = string.Format("{0}{1}.bingo", sSaveFileDir, inputRoomName.text);

        if(File.Exists(sSaveFilePath) == false) {
            Debug.LogErrorFormat("Path to file doesn't exist: {0}", sSaveFilePath);
            return;
        }

        string[] arsSaveLines = File.ReadAllLines(sSaveFilePath);


        foreach(string sLine in arsSaveLines) {

            //If the line is empty or starts with a comment, then skip the line
            if(sLine == "" || sLine[0] == '#') continue;

            string[] arsSplitLine = sLine.Split(':');

            switch(arsSplitLine[0]) {

            case "Tasks":
                taskmanager.SetTaskFile(arsSplitLine[1]);
                break;

            case "Seed":
                taskmanager.SetSeed(int.Parse(arsSplitLine[1]));
                break;

            case "Size":
                taskmanager.SetBoardSize(int.Parse(arsSplitLine[1]));
                break;

            case "Difficulty":
                taskmanager.SetDifficulty(float.Parse(arsSplitLine[1]));
                break;

            case "Variability":
                taskmanager.SetDifficultyVariability(float.Parse(arsSplitLine[1]));
                break;

            case "Lines":
                taskmanager.SetLinesNeeded(int.Parse(arsSplitLine[1]));
                break;

            case "EndGenerationParams":
                //Once we reach the end of the section, we can generate the bingo board
                // The following lines can be about setting up player info
                taskmanager.RequestBoardGeneration();
                break;

            case "Player":
                int iPlayer = int.Parse(arsSplitLine[1]);

                string sName = arsSplitLine[2];
                Color colClaimed = new Color(float.Parse(arsSplitLine[4]), float.Parse(arsSplitLine[5]), float.Parse(arsSplitLine[6]));

                taskmanager.lstAllPlayers[iPlayer].SetName(sName);
                taskmanager.lstAllPlayers[iPlayer].SetColour(colClaimed);

                //Send the change if we are in networking mode
                if(NetworkSender.inst != null) {
                    NetworkSender.inst.SendNameChange(taskmanager.lstAllPlayers[iPlayer]);
                    NetworkSender.inst.SendColorChange(taskmanager.lstAllPlayers[iPlayer]);
                }

                break;

            case "Task":

                //Figure out which task this line is representing
                int iTask = int.Parse(arsSplitLine[1]);

                //After the first three entries we'll have an variable number of entries for each player that has claimed this task
                for(int i = 3; i < arsSplitLine.Length; i++) {

                    //Subdivide each player entry into its index, claim, and flag
                    string[] arsPlayerEntry = arsSplitLine[i].Split(',');

                    int iPlyr = int.Parse(arsPlayerEntry[0]);
                    bool bClaimed = bool.Parse(arsPlayerEntry[1]);
                    bool bFlagged = bool.Parse(arsPlayerEntry[2]);

                    if(NetworkSender.inst != null) {
                        //If online, send a message requesting a claim/flag
                        if(bClaimed) {
                            NetworkSender.inst.SendToggleTask(iTask, iPlyr);
                        }
                        if(bFlagged) {
                            NetworkSender.inst.SendToggleFlag(iTask, iPlyr);
                        }
                    } else {
                        //If offline, just claim/flag the task directly
                        if(bClaimed) {
                            taskmanager.lstBingoBoard[iTask].RequestToggle(iPlyr);
                        }
                        if(bFlagged) {
                            taskmanager.lstBingoBoard[iTask].flag.ToggleFlag(iPlyr);
                        }
                    }
                }

                break;
            }
        }

        taskmanager.SetSelectedPlayer(nSelecting);
    }

}