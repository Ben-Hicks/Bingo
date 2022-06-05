using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks {

    public Text txtConnectionStatus;
    public InputField inputRoomName;

    public Button btnConnect;

    public void OnConnectionStatusChange() {

        if(PhotonNetwork.IsConnected == false) {
            txtConnectionStatus.text = "Not Connected";
        } else if(PhotonNetwork.InRoom == false) {
            txtConnectionStatus.text = "Not in Room";
        } else {
            //txtConnectionStatus.text = string.Format("Players Connected: {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            txtConnectionStatus.text = string.Format("Connected? {0}, Server? {1}, Room? {2}, Players? {3}",
                PhotonNetwork.IsConnected, PhotonNetwork.Server, PhotonNetwork.CurrentRoom, "XXX");
        }
    }



    // Start is called before the first frame update
    void Start() {
        btnConnect.gameObject.SetActive(false);

        PhotonNetwork.GameVersion = "v1";

        //On startup, connect to the master server
        PhotonNetwork.ConnectUsingSettings();


    }


    public void ConnectToRoom() {
        //Can't join a room if we're not even connected
        if(PhotonNetwork.IsConnected == false) {
            Debug.Log("Can't join a room when we're not yet connected");
            return;
        }

        ExitGames.Client.Photon.Hashtable expectedRoomProperties = new ExitGames.Client.Photon.Hashtable();

        Debug.Log(PhotonNetwork.Server.ToString());

        bool bSent = PhotonNetwork.JoinRoom(GetRoomNameToJoin());
        //bool bSent = PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 5);
        Debug.LogFormat("Sent={0}", bSent);
    }

    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();

        Debug.Log("Connected to master");

        btnConnect.gameObject.SetActive(true);
    }

    public string GetRoomNameToJoin() {
        return inputRoomName.text;
    }


    public override void OnJoinRoomFailed(short returnCode, string message) {
        base.OnJoinRoomFailed(returnCode, message);

        Debug.Log("Failed to join room");

        //Since we failed to join an existing room, let's make our own
        RoomOptions roomOptions = new RoomOptions();

        //Can initialize any room properties needed here if multiple people may be playing different games
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();

        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(GetRoomNameToJoin(), roomOptions);

    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();

        Debug.Log("Joined room");

        OnConnectionStatusChange();

        btnConnect.gameObject.SetActive(false);

        //Now that we've connected to a room, if we're the master, then we should spawn a NetworkMessenger to handle networking requests
        if(PhotonNetwork.IsMasterClient == true) {
            Debug.Log("Spawning NetworkMessenger");
            PhotonNetwork.InstantiateSceneObject("Prefabs/pfNetworkMessenger", Vector3.zero, Quaternion.identity);

        }

        TaskManager.inst.UpdateShownPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log("Room creation failed");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);

        OnConnectionStatusChange();

        Debug.Log("Another player joined this room");

        TaskManager.inst.UpdateShownPlayers();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);

        OnConnectionStatusChange();

        Debug.Log("Another player left this room");
    }

    // Update is called once per frame
    void Update() {
        OnConnectionStatusChange();
    }
}
