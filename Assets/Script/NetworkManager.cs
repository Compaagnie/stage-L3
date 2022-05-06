using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
      
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server!!");
        base.OnConnectedToMaster();

        // Create the room
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 3,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        Connect();
    }
    
    public void Connect()
    {
        Debug.Log("Try to connect ...");
        PhotonNetwork.ConnectUsingSettings();
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room !!");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("new player");
        base.OnPlayerEnteredRoom(newPlayer);
    }
  
}