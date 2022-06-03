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

    public static float[] SerializeColor(Color col) {
        float[] arCol = { col.r, col.g, col.b, col.a };

        return arCol;
    }

    public static Color DeserializeColor(float[] arCol) {
        Color col = new Color(arCol[0], arCol[1], arCol[2], arCol[3]);

        return col;
    }

    public void SendToggleTask(Task task, int iPlayer) {
        Debug.Log("Sending Toggle");
        photonview.RPC("ReceiveToggleTask", RpcTarget.AllBufferedViaServer, task.id, iPlayer);
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
