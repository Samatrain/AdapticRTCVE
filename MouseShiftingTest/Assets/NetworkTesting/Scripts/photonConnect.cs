using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class photonConnect : MonoBehaviour
{
    //When building, both versions need to be correct for both users
    public string versionName = "0.1";

    public GameObject sectionView1, sectionView2, sectionView3;

    public void connectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings(versionName);

        Debug.Log("Connecting to Photon...");
    }

    //tells us if we have been connected to a general server for our application
    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);

        Debug.Log("We are connected to master");
    }

    private void OnJoinedLobby()
    {
        sectionView1.SetActive(false);
        sectionView2.SetActive(true);

        //When a user has joined lobby, can see rooms
        Debug.Log("On Joined Lobby");
    }

    //If user disconnects mid-session
    private void OnDisconnectedFromPhoton()
    {
        if (sectionView1.activeInHierarchy)
            sectionView1.SetActive(false);

        if(sectionView2.activeInHierarchy)
            sectionView2.SetActive(false);

        sectionView3.SetActive(true);

        Debug.Log("Disconnected from Photon");
    }

    //Should recognise there is no internet connection
    //This function might not be necessary because of OnDisconnectedFromPhoton
    private void OnFailedToConnectToPhoton()
    {

    }

}
