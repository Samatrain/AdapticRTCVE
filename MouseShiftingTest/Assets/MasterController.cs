using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// In Adaptic + retargetin project this script manages the whole logic.
// In second project this mannages only the user logic, locally and in network
//IPunObservable is an interface required by Photon to synchronise variables over a network
public class MasterController : MonoBehaviour, IPunObservable
{
    // Networking mannagement here//
    #region NetworkinMannagement
    // Current player ID
    public int userId;

    //Local player instance. Used to verufy if the local player is represented in the scene.
    // JFGA TODO. Verify if this is needed or not
    public static GameObject LocalPlayerInstance;
    #endregion

    //Render appearance of players' head and hand objects
    public Renderer rendHead;
    public Renderer rendHand;

    //Enum to define conditions
    public enum CONDITION
    {
        SM_RT,
        SM_OO,
        NM_RT,
        NM_OO
    }

    /* --- SCRIPTS --- */
    // Script for manage tracking. 
    private TrackerMannager trackerMannager;

    // Information for the user. No sync here
    //Handles user notifications, like guiding players or feedback
    private NotificationsMannager notificationsMannager;

    // Survey to be performed in each case.
    private SurveyMannager surveyMannager;

    // prop mannager
    private PropMannager propMannager;

    private PersistanceManager persistanceManager;

    private Logic logic;

    /* --- OTHER FIELDS -- */
    // Base condition
    public CONDITION condition;
  
    public int stageCounter;

    public bool surveyActivated;


    // - Sets the GameObject's name to the network player's nickname
    // - PhotonView.owner identifies the player owning this object
    private void Awake()
    {
        this.gameObject.name = GetComponent<PhotonView>().owner.NickName;
    }

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent is retrieving instances of other scripts that are attached
        //  to the same GameObject
        //TrackerManager manages player tracking and interactions with virtual objects.
        trackerMannager = gameObject.GetComponent<TrackerMannager>();
        //NotificationsManager handles the visual notifications for the user
        notificationsMannager = gameObject.GetComponent<NotificationsMannager>();
        //SurveyManager allows use of in-game surveys to collect data for feedback
        surveyMannager = gameObject.GetComponent<SurveyMannager>();
        //Logic controls the game's core logic like player turns
        logic = gameObject.GetComponent<Logic>();
        //PropManager controls the objects that the player can manipulate
        propMannager = gameObject.GetComponent<PropMannager>();
        //PersistanceManager stores data from the trials
        persistanceManager = gameObject.GetComponent<PersistanceManager>();

        //Sets the initial state to false to make sure the survey system is disabled
        surveyActivated = false;
        
        // TODO the steps should be shared. Notification Mannager to be changes drasticly
        //notificationsMannager.lightStepNotification(1);

         
         
    }

    //Turn transparency on/off depending on whose turn it is
    public void changeTransparency(bool onTurn)
    {
        Color c = rendHead.material.color;
        c.a = onTurn ? 1f : 0.5f;
        rendHead.material.color = c;


        c = rendHand.material.color;
        c.a = onTurn ? 1f : 0.5f;
        rendHand.material.color = c;
    }

    // - Update stage is the object belongs to the local player
    // - Only the owning player updates because of PhotonView?
    public void changeStage(LogicGame.STAGE nStage)
    {
        if (GetComponent<PhotonView>().isMine)
        {
            persistanceManager.currentStage = nStage.ToString("G");
        }
    }

    //Recording of the trial
    // - gathers info like trial ID and saves the data
    public void startRecording(string idTrial)
    {
        if (GetComponent<PhotonView>().isMine)
        { 
            persistanceManager.trialId = idTrial;
            persistanceManager.currentStage = "TUTORIAL";
            persistanceManager.recording = true;
            persistanceManager.saveGeneral();
        }
    }

    //Record to the persistance manager for the local user
    public void setRecording(bool rRecord)
    {

        if (GetComponent<PhotonView>().isMine)
            persistanceManager.recording = rRecord;
    }

    //Updates the condition and enables the PropManager if the condition is SM_RT
    public void setCondition(CONDITION nCondition)
    {
        if(GetComponent<PhotonView>().isMine)
        { 
            condition = nCondition;
            if (condition == CONDITION.SM_RT)
                propMannager.enabled = true;
        }
    }

    //When condition SM_RT is met, send appropriate command to Arduino
    public void presetPtop(PropMannager.PRESET_TYPE presetType)
    {
        if(GetComponent<PhotonView>().isMine && this.condition == CONDITION.SM_RT)
        {
            propMannager.adapticCommand(presetType);
        }
    }

    public void setNew()
    {
        
       /*
        if (notificacionTextObject != null)
            notificacionTextObject.SetActive(true);
            */
        
        //persistanceManager.recording = false;
        //persistanceManager.userId = System.DateTime.Now.ToString("yyMMddHHmmss");
        //TextMesh text = notificacionTextObject.GetComponent<TextMesh>();
        //text.text = "Welcome";*/
    }

    // Update is called once per frame
    
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
           /* trackerMannager.setTrackers();

            if (!started)
            {
                started = true;

                currentStage = EXP_STAGE.PROP_NOT_MATCHING_PLUS_RETARGETING;
                stagesDone[0] = true;

                notificationsMannager.lightStepNotification(1);

                if (notificacionTextObject != null)
                {
                    TextMesh text = notificacionTextObject.GetComponent<TextMesh>();
                    text.text = "TUTORIAL";
                }

                if (persistanceManager != null)
                    persistanceManager.saveGeneral();
                else
                    Debug.LogError("PersistanceMannager missing!!! No results reported");
                //Call persistance to update
            }
            else
                nextStage();

            trackerMannager.setTrackers();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            setNew();
            notificationsMannager.normalSettings();
            persistanceManager.recording = false;

            GameObject leftProp = GameObject.Find("LeftProp");
            if (leftProp != null)
                leftProp.GetComponent<PropController>().angleNumber = 0;

            GameObject rightProp = GameObject.Find("RightProp");
            if (rightProp != null)
                rightProp.GetComponent<PropController>().angleNumber = 0;*/

        }
    }


    public void setNewLogic()
    {
        if(GetComponent<PhotonView>().isMine)
        { 
            logic.setNew();
        }
    }


    public  int[] currentScenarioConfiguration()
    {
        return logic.currentScenario;
    }

    [PunRPC]
    public void fillPlayerInformation()
    {
        if(GetComponent<PhotonView>().isMine)
        {
           // logic.fillPlayerInformation(); //This works
            logic.fillPlayerInformationFull();
        }
    }

    public void nextStage()
    {
        if (GetComponent<PhotonView>().isMine)
        {
            logic.onTurnStep();
        }

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }
}
