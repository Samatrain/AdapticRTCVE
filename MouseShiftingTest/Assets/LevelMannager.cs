using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Photon.Realtime;

public class LevelMannager : Photon.PunBehaviour, IPunObservable
{
    /*
     *Platforms to be supported
     * Vive_HTVT : Vive headset, HT Hand tracking with Vive Pro framework, VT Object tracking with Vive trackers
     * Vive_VTVT : Vive headset, VT Hand tracking with Vive tracker (No finger tracking)k, VT Object tracking with Vive trackers
     * Vive_LMOT : Vive headset, LM Hand tracking with Leapmotion, OT Object tracking with Optitrack
     */

    /*
     * Platform to add:
     *  Oculus Quest 2 (OVRToolkit)
     */
    public enum Platform { Vive_HTVT, Vive_VTVT, Vive_LMOT, OVRPlayerController };

    static public LevelMannager Instance;

    [Tooltip("Prefab Vive_HTVT : Vive headset, HT Hand tracking with Vive Pro framework, VT Object tracking with Vive trackers")]
    public GameObject UserViveHTVT;

    [Tooltip("Prefab Vive_VTVT : Vive headset, VT Hand tracking with Vive tracker (No finger tracking)k, VT Object tracking with Vive trackers")]
    public GameObject UserViveVTVT;

    [Tooltip("Prefab Vive_LMOT : Vive headset, LM Hand tracking with Leapmotion, OT Object tracking with Optitrack")]
    public GameObject UserViveLMOT;

    //Check if this works for Oculus
    //[Tooltip("Prefab OVRPlayerController")]
    public GameObject UserOVRPlayerController;

    [Tooltip("Spawn location for participants")]
    public Transform[] spawnLocations;

    public Transform currentSpawnPosition;

    public Platform currentPlatform;

    // Array of users
    public MasterController[] users;

    //Parent object for VR player rigs
    public GameObject vrPlayersParent;

    public int playerId;

    // Start is called before the first frame update
    void Start()
    {
        if (UserOVRPlayerController == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'LevelMannager'", this);
        }

        if (PhotonNetwork.connected)
        {
            //AssignVRRig();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //called when a player connects to the game
    //spawns the player using spawnPlayer
    public void spawnConnectedPlayer()
    {
        string playerNickname = PhotonNetwork.player.NickName;
        Transform spawnPosition = spawnLocations[PhotonNetwork.playerList.Length - 1];

        GameObject playerInstance = PhotonNetwork.Instantiate("OVRPlayerController", spawnPosition.position, spawnPosition.rotation, 0);

        Debug.Log("Spawned player: " + playerNickname + " at " + spawnPosition.position);

        //Set the parent of hte instantiated OVR rig to the "VRPlayers" parent
        if(vrPlayersParent != null)
        {
            playerInstance.transform.SetParent(vrPlayersParent.transform);
        }
    }

    //calculates spawn position based on player's id
    public void spawnPlayer(string playerNickname, Platform destPlatform)
    {
        char[] nickNamechars = playerNickname.ToCharArray();
        playerId = Int32.Parse(nickNamechars[playerNickname.Length - 1] + "");
        Transform spawnPosition = spawnLocations[playerId];
        currentSpawnPosition = spawnPosition;

        //GameObject.Find("/TutorialIslandP" + (playerId + 1) + "/Deco/Door/TutorialStatus/TutorialCanvas").transform.gameObject.SetActive(true);
        // TODO do whatever is needed it to assure correct orientation
        // Maybe locally
        GameObject userGameObject = null;
        string prefabName = null;

        switch (destPlatform)
        {
            case Platform.Vive_VTVT:
                userGameObject = UserViveVTVT;
                prefabName = "Vive_VTVT";
                break;
            case Platform.OVRPlayerController:
                userGameObject = UserOVRPlayerController;
                prefabName = "OVRPlayerController";
                break;
        }

        if(userGameObject != null)
        {
            GameObject playerInstance = PhotonNetwork.Instantiate(prefabName, spawnPosition.position, spawnPosition.rotation, 0);

            PhotonView playerView = playerInstance.GetComponent<PhotonView>();
            playerView.TransferOwnership(PhotonNetwork.player);

            if(vrPlayersParent != null)
            {
                playerInstance.transform.SetParent(vrPlayersParent.transform);
            }
        }
        else
        {
            Debug.Log("JFGA LevelMannager.cs ----Error Not prefab selected ----");
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    //Getter function to return current platform selected
    public Platform getCurrentPlatform()
    {
        return currentPlatform;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new NotImplementedException();
    }
}


