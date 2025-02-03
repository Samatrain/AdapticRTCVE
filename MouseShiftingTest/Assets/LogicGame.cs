using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicGame : MonoBehaviour, IPunObservable
{
    public enum STAGE
    {
        // Bases A and B
        // Base A: SM_RT 
        // Base B: NM_RT or NM_RT -> TODO
        TUTORIAL, // TODO what kind of props are gonna use here
        FIRST, // Base A User 1, Base B User 2
        SECOND // Base A User 2, Base B User 2 
    }


    public STAGE currenStage;
    public int numStage;
    public readonly STAGE[] stages = new STAGE[] { STAGE.TUTORIAL, STAGE.FIRST, STAGE.SECOND };


    public bool started;
    public bool paused;

    public NotificationsMannager[] playersNotifications;
    public CentralBannerMannaher centralBannerMannaher;
    public MasterController[] playerMasters;
    // public Logic[] playerLogics;

    // Picking up positions
    public GameObject[] positions;

    // Objects
    public GameObject[] objects;

    // Both home positions
    public GameObject[] homePositions;

    public GameObject[] homePositions2;


    public GameObject currentEndObject;
    public GameObject currentEndPosition;

    public int currentPlayer;

    public string experimentId;

    //Same part definitions
    // 0 Pre Start
    // 1 Grabbing the object
    // 2 Docking in home point
    // 3 returning object -- Maybe part 4 is not necessary
    public int repetition;
    public const int REPETITIONS = 16;

    


    public void setPlayers()
    {
        playerMasters = new MasterController[2];
        //playerLogics = new Logic[2];
        GameObject master = GameObject.Find("Player 0");
        if (master != null)
        {
            playerMasters[0] = master.GetComponent<MasterController>();
            // playerLogics[0] = master.GetComponent<Logic>();
            master.GetComponent<Logic>().logicGame = this;
            playerMasters[0].setCondition(MasterController.CONDITION.SM_RT);
            playerMasters[0].startRecording(experimentId);
        }
        else
            GetComponent<PhotonView>().RPC("boadcastError", PhotonTargets.All, "ERROR FROM " + PhotonNetwork.player.NickName + " NOT PLAYER 0 FOUND");
        master = GameObject.Find("Player 1");
        if (master != null)
        {
            playerMasters[1] = master.GetComponent<MasterController>();
            //playerLogics[1] = master.GetComponent<Logic>();
            master.GetComponent<Logic>().logicGame = this;
            playerMasters[1].setCondition(MasterController.CONDITION.NM_OO);
            playerMasters[1].startRecording(experimentId);
        }
        else
            GetComponent<PhotonView>().RPC("boadcastError", PhotonTargets.All, "ERROR FROM " + PhotonNetwork.player.NickName + " NOT PLAYER 1 FOUND");


        Debug.Log("Players ready!");
    }

    [PunRPC]
    public void boadcastError(string msg)
    {
        Debug.LogError(msg);
    }


    [PunRPC]
    public void refreshPlayerProp()
    {
        PropSpecs propSpecs = currentEndObject.GetComponent<PropSpecs>();
        playerMasters[(currentPlayer + 1) % 2].presetPtop(propSpecs.type);
    }

    public void initialObjectPosition()
    {

        currentPlayer = 0;
        playerMasters[1].setNewLogic();// DELETE this and the methind on mastercontroller
        playerMasters[1].nextStage();

        if(!PhotonNetwork.isMasterClient)
        {
            int[] currSce = playerMasters[1].currentScenarioConfiguration();
            GetComponent<PhotonView>().RPC("setPosObj", PhotonTargets.All, currSce[0], currSce[1]);
        }
        playerMasters[0].setNewLogic();
        playerMasters[0].nextStage();

        playerMasters[0].changeTransparency(currentPlayer == 0);
        playerMasters[1].changeTransparency(currentPlayer == 1);

    }

    [PunRPC]
    public void setPosObj(int indexObj, int indexPos)
    {
        currentEndObject = objects[indexObj];
        currentEndPosition = positions[indexPos];
        currentEndObject.SetActive(true);
        currentEndObject.transform.position = currentEndPosition.transform.position;
        refreshPlayerProp();
    }

    [PunRPC]
    public void enableHomePoint(int indexHomePoint, bool nState, bool isSecond )
    {
        if(!isSecond)
            homePositions[indexHomePoint].SetActive(nState);
        else
            homePositions2[indexHomePoint].SetActive(nState);
    }

    [PunRPC]
    public void movePropDock(bool toHomePoint, Quaternion nOrientation)
    {
        PropSpecs propSpecs = currentEndObject.GetComponent<PropSpecs>();
        GameObject dockProp = propSpecs.ghost;
        //if (dockProp.GetComponent<PhotonView>().owner.ID != PhotonNetwork.player.ID)
        //dockProp.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player.ID);
        if (toHomePoint)
        {
            dockProp.transform.parent = homePositions[currentPlayer].transform.parent.transform;
            dockProp.transform.localPosition = new Vector3(0.0f, 0f, 0.0f);
            dockProp.transform.rotation = nOrientation;
        }
        else
        {
            dockProp.transform.rotation = nOrientation;
            dockProp.transform.Rotate(new Vector3(0f, 180f, 0));
            dockProp.transform.parent = currentEndPosition.transform;
            dockProp.transform.localPosition = Vector3.zero;
        }

    }


    [PunRPC]
    public void nextStage()
    {
        paused = false;
        numStage++;
        if(numStage>2)
        {
            started = false;

        }
        else
        {

            repetition = 0;
            currentPlayer = 0;
            playerMasters[0].setRecording(true);
            playerMasters[1].setRecording(true);

            playerMasters[1].fillPlayerInformation();
            playerMasters[1].nextStage();

            if (!PhotonNetwork.isMasterClient)
            {
                int[] currSce = playerMasters[1].currentScenarioConfiguration();
                GetComponent<PhotonView>().RPC("setPosObj", PhotonTargets.All, currSce[0], currSce[1]);
            }

            playerMasters[0].fillPlayerInformation();
            playerMasters[0].nextStage();


            currenStage = stages[numStage];
            centralBannerMannaher.temporalMessage("Comenzando parte " + numStage);
            playerMasters[0].changeStage(currenStage);
            playerMasters[1].changeStage(currenStage);
        } 
    }

    [PunRPC]
    public void setActiveGhost(bool active)
    {

        PropSpecs propSpecs = currentEndObject.GetComponent<PropSpecs>();
        propSpecs.ghost.SetActive(active);
    }

    [PunRPC]
    public void relocatePropDock()
    {
        PropSpecs propSpecs = currentEndObject.GetComponent<PropSpecs>();
        propSpecs.ghost.transform.parent = transform;
        propSpecs.ghost.transform.localPosition = Vector3.zero;
    }

    [PunRPC]
    public void nextStep()
    {
        repetition += currentPlayer;
        if(repetition == REPETITIONS || (currenStage== STAGE.TUTORIAL && repetition == 4))
        {
            paused = true;

            playerMasters[0].setRecording(false);
            playerMasters[1].setRecording(false);
            if (currenStage == STAGE.TUTORIAL)
                centralBannerMannaher.permanentMessage("Fin del tutorial");
            if (currenStage == STAGE.FIRST)
                centralBannerMannaher.permanentMessage("Fin de la primera parte");
            if (currenStage == STAGE.SECOND)
            {
                centralBannerMannaher.permanentMessage("Fin de la prueba");
            }

            paused = true;
            currentPlayer = -1;
            playerMasters[0].nextStage();// DELETE this and the methind on mastercontroller
            playerMasters[1].nextStage();
            playerMasters[0].changeTransparency(currentPlayer == 0);
            playerMasters[1].changeTransparency(currentPlayer == 1);
            objects[0].SetActive(false);
            objects[1].SetActive(false);
        }
        else
        {
           // playersNotifications[currentPlayer].lightStepNotification(7);
            currentPlayer = (currentPlayer + 1) % 2;
            playerMasters[0].nextStage();// DELETE this and the methind on mastercontroller
            playerMasters[1].nextStage();
            playerMasters[0].changeTransparency(currentPlayer == 0);
            playerMasters[1].changeTransparency(currentPlayer == 1);
        }
        

    }

    [PunRPC]
    public void refreshObjectAndPosition(int indexObj, int indexPos)
    {
        objects[0].SetActive(false);
        objects[1].SetActive(false);
        currentEndObject = objects[indexObj];
        currentEndPosition = positions[indexPos];
        currentEndObject.SetActive(true);
        refreshPlayerProp();
    }




    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        repetition = 0;

        currenStage = STAGE.TUTORIAL;
        numStage = 0;
    }
    [PunRPC]
    public void getStared( string nId)
    {
        currenStage = STAGE.TUTORIAL;
        numStage = 0;
        experimentId = nId;
        started = true;
        setPlayers();
        initialObjectPosition();
        centralBannerMannaher.temporalMessage("Tutorial...");
        
    }


    [PunRPC]
    public void resetBanner(string nMessage)
    {
        numStage = 0;
        centralBannerMannaher.permanentMessage(nMessage);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!started)
            {
                if(numStage > 2)
                {
                    GetComponent<PhotonView>().RPC("resetBanner", PhotonTargets.All, "Bienvenido!");
                }
                else
                {
                    string nId = System.DateTime.Now.ToString("yyMMddHHmmss");
                    GetComponent<PhotonView>().RPC("getStared", PhotonTargets.All, nId);
                }
               
            }
            else
            {
                if(paused)
                    GetComponent<PhotonView>().RPC("nextStage", PhotonTargets.All);
            }
        }
    }
}
