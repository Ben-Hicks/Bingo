using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkSender : MonoBehaviour {
    public static NetworkSender inst;

    public PhotonView photonview;

    private void Awake() {
        inst = this;
    }

    public void Start() {
        //On startup, we'll lock our player selection to the corresponding id of our member of the photon room
        TaskManager.inst.SetSelectedPlayer(PhotonNetwork.LocalPlayer.ActorNumber - 1);

        //Then disabled the editing of all other players
        for(int i = 0; i < TaskManager.inst.lstAllPlayers.Count; i++) {
            if(i == PhotonNetwork.LocalPlayer.ActorNumber - 1) continue;

            TaskManager.inst.lstAllPlayers[i].LockUIModifiers();
        }
    }

    public static float[] SerializeColor(Color col) {
        float[] arCol = { col.r, col.g, col.b, col.a };

        return arCol;
    }

    public static Color DeserializeColor(float[] arCol) {
        Color col = new Color(arCol[0], arCol[1], arCol[2], arCol[3]);

        return col;
    }

    public void SendTaskProgress(int iTask, int iPlayer, int nProgress) {
        Debug.LogFormat("Sending progress for {0} for player {1} of value {2}", iTask, iPlayer, nProgress);
        photonview.RPC("ReceiveTaskProgress", RpcTarget.AllBufferedViaServer, iTask, iPlayer, nProgress);
    }

    public void SendToggleFlag(int iTask, int iPlayer) {
        Debug.LogFormat("Sending Flag of {0} for player {1}", iTask, iPlayer);
        photonview.RPC("ReceiveToggleFlag", RpcTarget.AllBufferedViaServer, iTask, iPlayer);
    }

    public void SendColorChange(Player player) {
        Debug.Log("Sending colour change");
        photonview.RPC("ReceiveColorChange", RpcTarget.AllBufferedViaServer, player.id, SerializeColor(player.colClaimed));
    }

    public void SendNameChange(Player player) {
        Debug.Log("Sending name change");
        photonview.RPC("ReceiveNameChange", RpcTarget.AllBufferedViaServer, player.id, player.sName);
    }

    public void SendGenerateBoard(string sTaskFile, int nBoardSize, float fDifficulty, float fDifficultyVariance,
        int nLinesNeeded, int nSeed) {
        Debug.Log("Sending board generation");
        photonview.RPC("ReceiveGenerateBoard", RpcTarget.AllBufferedViaServer,
            sTaskFile, nBoardSize, fDifficulty, fDifficultyVariance, nLinesNeeded, nSeed);

    }
}
