using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkReceiver : MonoBehaviour {



    [PunRPC]
    public void ReceiveTaskProgress(int iTask, int iPlayer, int nNewValue) {
        Debug.LogFormat("Receieved Claim {0} for player {1}", TaskManager.inst.lstBingoBoard[iTask], iPlayer);
        TaskManager.inst.lstBingoBoard[iTask].ChangeProgress(iPlayer, nNewValue);
    }

    [PunRPC]
    public void ReceiveToggleFlag(int iTask, int iPlayer) {
        Debug.LogFormat("Received flag {0} for player {1}", TaskManager.inst.lstBingoBoard[iTask], iPlayer);
        TaskManager.inst.lstBingoBoard[iTask].flag.ToggleFlag(iPlayer);
    }

    [PunRPC]
    public void ReceiveColorChange(int iPlayer, float[] arCol) {
        Debug.Log("Receiving colour change");
        TaskManager.inst.lstAllPlayers[iPlayer].SetColour(NetworkSender.DeserializeColor(arCol));

        //Manually update the colours on the board for that player
        TaskManager.inst.lstAllPlayers[iPlayer].UpdateBoardClaimColour();
    }

    [PunRPC]
    public void ReceiveNameChange(int iPlayer, string sName) {
        Debug.Log("Receiving name change");
        TaskManager.inst.lstAllPlayers[iPlayer].SetName(sName);
    }

    [PunRPC]
    public void ReceiveGenerateBoard(string sTaskFile, int nBoardSize, float fDifficulty, float fDifficultyVariance,
        int nLinesNeeded, int nSeed) {
        Debug.Log("Receiving board generation");

        TaskManager.inst.SetTaskFile(sTaskFile);
        TaskManager.inst.SetBoardSize(nBoardSize);
        TaskManager.inst.SetDifficulty(fDifficulty);
        TaskManager.inst.SetDifficultyVariability(fDifficultyVariance);
        TaskManager.inst.SetLinesNeeded(nLinesNeeded);
        TaskManager.inst.SetSeed(nSeed);

        TaskManager.inst.GenerateBoard();

        TaskManager.inst.UpdateAllTaskDescriptions();
    }
}
