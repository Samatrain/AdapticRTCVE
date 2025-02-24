using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMannager : Photon.PunBehaviour, IPunObservable
{

    // Mannager of the level
    public LevelMannager levelMannager;

    //Target platform. For now the default platform is gonna be Vive_VTVT
    public LevelMannager.Platform platform;

    // PUN log level. Describes what is gonna be logged
    private PhotonLogLevel photonLogLevel = PhotonLogLevel.ErrorsOnly;

    // Max number of players allowed in the room.
    public byte maxPlayersPerRoom = 2;

    //Describes the status (active or not) or each player
    public bool[] activePlayers;

    //TODO Managemente of an UI to know the network state.
    // Previous experiments used a small dahsboard insied the room. Maybe.
    // Connects to UINetwork script, updates text when users are connected.
    public UINetwork uINetwork;

    // Version. default parameter for us
    private string _gameVersion = "0.1";

    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    private bool isConnecting;

    // These are the options that will have the room that we're creating for the players
    private RoomOptions roomOptions;

    const string ROOM_NAME = "Adaptic CVE";

    const string LEVEL_NAME = "SampleScene";


    private void Awake()
    {
        PlayerPrefs.SetInt("Platform", (int)platform);
        // #NotImportant
        // Force LogLevel
        PhotonNetwork.logLevel = photonLogLevel;

        // #Critical
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = false;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;
    }

    

    void Start()
    {
        GameObject uiNetworkObj = GameObject.Find("UINetwork");
        uINetwork = uiNetworkObj.GetComponent<UINetwork>();
        roomOptions = new RoomOptions { IsVisible = true, MaxPlayers = maxPlayersPerRoom };
        DontDestroyOnLoad(this);
        Connect();
    }

    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    public void Connect()
    {
        if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
        {
            return;
        }
        // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
        isConnecting = true;



        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            //RoomOptions roomOptions = new RoomOptions { IsVisible = true, MaxPlayers = MaxPlayersPerRoom };
            PhotonNetwork.JoinOrCreateRoom(ROOM_NAME, roomOptions, TypedLobby.Default);
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

    }

    public void loadLevel()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(LEVEL_NAME);
            Debug.Log("NetworkManager.cs ---- I´m the master client");
        }
        //Debug.Log("The Room name is: " + PhotonNetwork.room.Name);
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer temp = PhotonNetwork.playerList[i];
            Debug.Log(temp.NickName + " is in the room " + PhotonNetwork.room.Name);
        }
    }

    //---------------------------------------------
    // Photon
    //--------------------------------------------

    public override void OnConnectedToMaster()
    {
        Debug.Log("CONECTADO AL MAESTRO");
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            //*Does this create a room for the second person to join?
            PhotonNetwork.JoinOrCreateRoom(ROOM_NAME, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(ROOM_NAME, roomOptions, TypedLobby.Default);
        Debug.Log("JFA NetworkMannager.cs ---- Failed to join room");
    }

    // This method is called when you as a player connect to the game room
    public override void OnJoinedRoom()
    {
        PhotonPlayer[] players = PhotonNetwork.playerList;

        for (int i = 0; i < players.Length; i++)
        {
            PhotonPlayer temp = players[i];
            if (temp.IsLocal)
            {
                temp.NickName = "Player " + i;
                photonView.RPC("ActivatePlayer", PhotonTargets.All, i, true);
            }
        }

        //Spawn the VR Player and Disable Non-Local Controls
        levelMannager.spawnConnectedPlayer();
        disableNonLocalVRControls();
    }

    //Revisit this maybe to make sure one player isn't using the other?
    //  It seems like right now, the 2nd player isn't being differentiated from the first,
    //  so they're both attached to the first OVR rig. Maybe I need to put a 2nd one in the outliner but it should be creating a new prefab
    void disableNonLocalVRControls()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Make sure OVR Rig has this tag

        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            OVRCameraRig cameraRig = player.GetComponentInChildren<OVRCameraRig>();

            if (pv != null && !pv.isMine)
            {
                // Disable camera for non-local players
                if (cameraRig != null)
                {
                    cameraRig.enabled = false;
                    player.GetComponent<AudioListener>().enabled = false;
                }
            }
        }
    }
    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        //Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting
        refreshConnectedPlayers();
    }
    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        // Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects
        refreshConnectedPlayers();
    }

    public void refreshConnectedPlayers()
    {
        PhotonPlayer[] players = PhotonNetwork.playerList;
        for (int i = 0; i < maxPlayersPerRoom; i++)
        {
            PhotonPlayer temp = null;
            if (i < players.Length)
            {
                temp = players[i];
            }
            if (temp == null)
            {
                // If there is no player in i position it means that position hasn't been occupied by any player
                photonView.RPC("ActivatePlayer", PhotonTargets.All, i, false);
            }
            else
            {
                photonView.RPC("ActivatePlayer", PhotonTargets.All, i, true);
            }
        }
    }

    public void OnDestroy()
    {
        PhotonNetwork.LeaveRoom();
    }


    public void disconnect()
    {
        PhotonNetwork.Disconnect();
    }



    public override void OnDisconnectedFromPhoton()
    {
        //netWorkStatusUI.disconnectUI();
        isConnecting = false;
        Debug.LogWarning("NetworkManager: OnDisconnectedFromPhoton() was called by PUN");
    }

    [PunRPC]
    public void ActivatePlayer(int playerPos, bool playerState)
    {
        activePlayers[playerPos] = playerState;
        uINetwork.refreshStatus(activePlayers);
        //netWorkStatusUI.setPlayerStatusTexts(playerPos, activePlayers[playerPos] + "");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
