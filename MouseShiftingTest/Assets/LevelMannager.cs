using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int playerId;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //called when a player connects to the game
    //spawns the player using spawnPlayer
    public void spawnConnectedPlayer()
    {

        //track which headset is being used
        int platformSelected = PlayerPrefs.GetInt("Platform");
        currentPlatform = (Platform)platformSelected;
        //Debug.Log("The platform selected is: " + currentPlatform.ToString());
        Instance = this;


        //I think this coul be done in a better way. Check TODO
        if (MasterController.LocalPlayerInstance == null)
        {
            PhotonPlayer actualPlayer = PhotonNetwork.player;
            //Debug.Log("Nickname of the local is " + actualPlayer.NickName);
            spawnPlayer(actualPlayer.NickName, currentPlatform); //spanw player at currentplatform location
        }
        else
        {
            //Debug.Log("Ignoring scene load for " + SceneManager.GetActiveScene().name);
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
        string contentNames = null;
        switch (destPlatform)
        {
            case Platform.Vive_VTVT:
                userGameObject = UserViveVTVT;
                contentNames = "Vive_VTVT";
                break;
        }

        if(userGameObject != null)
        {
            PhotonNetwork.Instantiate(contentNames, spawnPosition.position, spawnPosition.rotation, 0);
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
