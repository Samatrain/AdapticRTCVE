using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System;

// - PersistanceManager is mainly logging information like position, rotation, timestamps,
//      trial IDs and task progress
// - Data is written to CSV files for storage
public class PersistanceManager : MonoBehaviour
{

    private string hpLine;
    private string rtLine; //runtime? data
    public int counter;

    public float topRegisterToSave;
    //Master
    private LevelController levelController;

    private MasterController masterController;

    // Right hand of leapmotion. need to know position to log
    // Maybe is better to ask for the tracker;
    public HandHTCTrack trackedHand;
    
    // Reference to the actual object to pick
    // TODO mannagement to switch between both users
    public PropSpecs trackedObject;

    public Tracker tracker;

    //TODO this must be assigned on runtime
    //Camera Reference User 1
    public GameObject camera;

    // Id of the current trial
    public string trialId;

    // Id of movement
    public string movementId;

    // 1 or 2
    public string currentStage;

    public int idStation;

    // This must be defined 
    // 0 From HP tp grab the object
    // 1 from object position to dock in home point
    // 2 from home point after dock to second hp
    // 3 from second hp tp grab object in hp
    // 4 from hp to dock in endposition
    public int taskPart;

    public string condition;

    // Counter of movements in this experiment
    public int counterMovements;
    
    // Stopwatch to measure times
    private Stopwatch sw;

    public string PATH_LOCAL ;

    public const string NAME_GENERAL = "general.csv";
    public const string NAME_DETAIL_DOCK = "detail_dock.csv";
    public const string NAME_RUN_TIME = "runtime.csv";
    public const string NAME_HEAD_POSE = "headpose.csv";


    // General Fields 
    public const string HEADINGS_GEN_ID = "ID_TRIAL";
    public const string HEADINGS_GEN_TIME_STAMP = "TIME_STAMP";
    public const string HEADINGS_GEN_ID_STATION = "ID_STATION";
    public const string HEADINGS_GEN_CONDITION = "CONDITION";


    // Detail Dock Fields
    public const string HEADINGS_DET_ID_TRIAL = "ID_TRIAL ";
    public const string HEADINGS_DET_ID_MOVEMENT = "ID_MOVEMENT ";
    public const string HEADINGS_DET_STAGE = "STAGE";
    public const string HEADINGS_DET_ID_STATION = "ID_STATION";
    public const string HEADINGS_DET_TIME_STAMP = "TIME_STAMP ";
    public const string HEADINGS_DET_TIME = "TIME ";
    public const string HEADINGS_DET_TASK_PART = "TASK_PART ";
    public const string HEADINGS_DET_GOAL_POS_X = "GOAL_POS_X ";
    public const string HEADINGS_DET_GOAL_POS_Y = "GOAL_POS_Y ";
    public const string HEADINGS_DET_FINAL_POS_X = "FINAL_POS_X ";
    public const string HEADINGS_DET_FINAL_POS_Y = "FINAL_POS_Y ";
    public const string HEADINGS_DET_GOAL_POS_Z = "GOAL_POS_Z ";
    public const string HEADINGS_DET_FINAL_POS_Z = "FINAL_POS_Z ";
    public const string HEADINGS_DET_GOAL_ROT_QX = "GOAL_ROT_QX ";
    public const string HEADINGS_DET_GOAL_ROT_QY = "GOAL_ROT_QY ";
    public const string HEADINGS_DET_GOAL_ROT_QZ = "GOAL_ROT_QZ ";
    public const string HEADINGS_DET_GOAL_ROT_QW = "GOAL_ROT_QW ";
    public const string HEADINGS_DET_FINAL_ROT_QX = "FINAL_ROT_QX ";
    public const string HEADINGS_DET_FINAL_ROT_QY = "FINAL_ROT_QY ";
    public const string HEADINGS_DET_FINAL_ROT_QZ = "FINAL_ROT_QZ ";
    public const string HEADINGS_DET_FINAL_ROT_QW = "FINAL_ROT_QW ";
    public const string HEADINGS_DET_GOAL_EULER_AX = "GOAL_EULER_AX ";
    public const string HEADINGS_DET_GOAL_EULER_AY = "GOAL_EULER_AY ";
    public const string HEADINGS_DET_GOAL_EULER_AZ = "GOAL_EULER_AZ ";
    public const string HEADINGS_DET_FINAL_EULER_AX = "FINAL_EULER_AX ";
    public const string HEADINGS_DET_FINAL_EULER_AY = "FINAL_EULER_AY ";
    public const string HEADINGS_DET_FINAL_EULER_AZ = "FINAL_EULER_AZ ";
    public const string HEADINGS_DET_SHAPE = "SHAPE";
    public const string HEADINGS_DET_CONDITION = "CONDITION";


    // Run time fields. Hand and object positions
    public const string HEADINGS_RT_ID_TRIAL = "ID_TRIAL";
    public const string HEADINGS_RT_ID_MOVEMENT = "ID_MOVEMENT";
    public const string HEADINGS_RT_ID_STATION = "ID_STATION";
    public const string HEADINGS_RT_TIME_STAMP = "TIME_STAMP";
    public const string HEADINGS_RT_REAL_POS_X = "REAL_POS_X";
    public const string HEADINGS_RT_REAL_POS_Y = "REAL_POS_Y";
    public const string HEADINGS_RT_REAL_POS_Z = "REAL_POS_Z";
    public const string HEADINGS_RT_RET_POS_X = "RET_POS_X";
    public const string HEADINGS_RT_RET_POS_Y = "RET_POS_Y";
    public const string HEADINGS_RT_RET_POS_Z = "RET_POS_Z";
    public const string HEADINGS_RT_TRACK_REAL_POS_X = "TRACK_REAL_POS_X";
    public const string HEADINGS_RT_TRACK_REAL_POS_Y = "TRACK_REAL_POS_Y";
    public const string HEADINGS_RT_TRACK_REAL_POS_Z = "TRACK_REAL_POS_Z";
    public const string HEADINGS_RT_TRACK_RET_POS_X = "TRACK_RET_POS_X";
    public const string HEADINGS_RT_TRACK_RET_POS_Y = "TRACK_RET_POS_Y";
    public const string HEADINGS_RT_TRACK_RET_POS_Z = "TRACK_RET_POS_Z";
    public const string HEADINGS_RT_TRACK_ROT_QUA_X = "TRACK_ROT_QUA_X";
    public const string HEADINGS_RT_TRACK_ROT_QUA_Y = "TRACK_ROT_QUA_Y";
    public const string HEADINGS_RT_TRACK_ROT_QUA_Z = "TRACK_ROT_QUA_Z";
    public const string HEADINGS_RT_TRACK_ROT_QUA_W = "TRACK_ROT_QUA_W";
    public const string HEADINGS_RT_TRACK_EULER_AX = "TRACK_EULER_AX ";
    public const string HEADINGS_RT_TRACK_EULER_AY = "TRACK_EULER_AY";
    public const string HEADINGS_RT_TRACK_EULER_AZ = "TRACK_EULER_AZ";
    public const string HEADINGS_RT_CURR_STAGE = "CURR_STAGE";

    // Head pose pos and rot;
    public const string HEADINGS_HP_ID_TRIAL = "ID_TRIAL";
    public const string HEADINGS_HP_ID_MOVEMENT = "ID_MOVEMENT";
    public const string HEADINGS_HP_ID_STATION = "ID_STATION";
    public const string HEADINGS_HP_TIME_STAMP = "TIME_STAMP";
    public const string HEADINGS_HP_POS_X = "POS_X";
    public const string HEADINGS_HP_POS_Y = "POS_Y";
    public const string HEADINGS_HP_POS_Z = "POS_Z";
    public const string HEADINGS_HP_ROT_QUA_X = "ROT_QUA_X";
    public const string HEADINGS_HP_ROT_QUA_Y = "ROT_QUA_Y";
    public const string HEADINGS_HP_ROT_QUA_Z = "ROT_QUA_Z";
    public const string HEADINGS_HP_ROT_QUA_W = "ROT_QUA_W";
    public const string HEADINGS_HP_EULER_AX = "EULER_AX";
    public const string HEADINGS_HP_EULER_AY = "EULER_AY";
    public const string HEADINGS_HP_EULER_AZ = "EULER_AZ";
    public const string HEADINGS_HP_CURR_STAGE = "CURR_STAGE";


    public bool recording;
    public bool killProcess;

    //Local data saving
    private IEnumerator saveLocal(string path, string[] entries, string[] values)
    {
        if (recording)
        { 
            StreamWriter sw = null;
            //Create a file to write to if it doesn't exist
            if (!File.Exists(path))
            {
                sw = new StreamWriter(path, true);
                sw.WriteLine(string.Join(";", entries));
            }
            sw = sw ?? new StreamWriter(path, true);

            sw.WriteLine(string.Join(";", values));
            sw.Close();
        }
        yield return null;
    }

    //Save runtime and pose data
    private IEnumerator saveLocal(string path, string[] entries, string values)
    {
        if (recording)
        {
            StreamWriter sw = null;
            if (!File.Exists(path))
            {
                sw = new StreamWriter(path, true);
                sw.WriteLine(string.Join(";", entries));
            }
            sw = sw ?? new StreamWriter(path, true);

            sw.WriteLine(values);
            sw.Close();
        }
        yield return null;
    }


    private IEnumerator saveRecords()
    {
        while(true )
        {
            if ( recording)
            {
                counter++;
                 saveRunTime();
                saveHeadPose();
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;
    }


    
    private void Update()
    {
       
        
    }

    private void Start()
    {

        //Check if local user's component
        if (!GetComponent<PhotonView>().isMine)
        {
            this.enabled = false;
        }
        topRegisterToSave = 150; //threshold for saving
        recording = false; //recording starts as default
        PATH_LOCAL = Application.dataPath + @"/Logout/"; //save path
        if (!Directory.Exists(PATH_LOCAL))
            Directory.CreateDirectory(PATH_LOCAL); //create directory if it doesnt exist
        counterMovements = 1;
        counter = 0;
        
        masterController = gameObject.GetComponent<MasterController>();
        StartCoroutine(saveRecords());
       
    }

    [PunRPC]
    public void setIdTrial(string nTrialId)
    {
        trialId = nTrialId;
    }

    public void saveGeneral( )
    {

        //extract station ID from the object name
        idStation = Int32.Parse(gameObject.name.ToCharArray()[gameObject.name.Length - 1] + "");
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss dd/MM/yy"); //get current timestamp
        


        string[] values = { trialId, timeStamp, idStation + "" , masterController.condition.ToString("G")};
        string pathLocal = PATH_LOCAL + NAME_GENERAL;
        string[] entries2 = { HEADINGS_GEN_ID, HEADINGS_GEN_TIME_STAMP, HEADINGS_GEN_ID_STATION, HEADINGS_GEN_CONDITION };

        
         StartCoroutine(saveLocal(pathLocal, entries2, values));;
    }

    //initalise movement-related data
    public void startDocking(int partMovement)
    {
        taskPart = partMovement; //set the current task part
        sw = Stopwatch.StartNew(); //start stopwatch timer
        counterMovements++; //increment movement counter
        movementId = System.DateTime.Now.ToString("HHmmss") +"N" + counterMovements; //generate unique movement ID
       // currentStage = levelController.currenStage.ToString("G");
    }

    public void saveDocking()
    {
        // textId
        Vector3 goalPosition = Vector3.zero;
        Quaternion goalRotation = Quaternion.identity;
        Vector3 finalPosition = Vector3.zero;
        Quaternion finalRotation = Quaternion.identity;
        string shape = "";

        //if an object is benig tracked, log its position and rotation
        if (trackedObject != null)
        {
            finalPosition = trackedObject.gameObject.transform.position;
            finalRotation = trackedObject.gameObject.transform.rotation;
            GameObject goalObj = trackedObject.ghost;
            goalPosition = goalObj.transform.position;
            goalRotation = goalObj.transform.rotation;
            shape = trackedObject.type.ToString("G");
            
        }

        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss dd/MM/yy");


        string[] values = { trialId, movementId, currentStage, idStation.ToString(), timeStamp, sw.ElapsedMilliseconds.ToString(), taskPart.ToString(),
                            goalPosition.x.ToString(), goalPosition.y.ToString() , goalPosition.z.ToString() , finalPosition.x.ToString(), finalPosition.y.ToString() , finalPosition.z.ToString(),
                            goalRotation.x.ToString(), goalRotation.y.ToString() , goalRotation.z.ToString() , goalRotation.w.ToString(), finalRotation.x.ToString(), finalRotation.y.ToString() , finalRotation.z.ToString() , finalRotation.w.ToString(),
                            goalRotation.eulerAngles.x.ToString(), goalRotation.eulerAngles.y.ToString() , goalRotation.eulerAngles.z.ToString() , finalRotation.eulerAngles.x.ToString(), finalRotation.eulerAngles.y.ToString() , finalRotation.eulerAngles.z.ToString(),
                            shape, masterController.condition.ToString("G")  };
        sw.Stop();



  

        string pathLocal = PATH_LOCAL + NAME_DETAIL_DOCK;
        string[] entries2 = { HEADINGS_DET_ID_TRIAL, HEADINGS_DET_ID_MOVEMENT, HEADINGS_DET_STAGE, HEADINGS_DET_ID_STATION, HEADINGS_DET_TIME_STAMP, HEADINGS_DET_TIME, HEADINGS_DET_TASK_PART,
                            HEADINGS_DET_GOAL_POS_X, HEADINGS_DET_GOAL_POS_Y, HEADINGS_DET_GOAL_POS_Z, HEADINGS_DET_FINAL_POS_X, HEADINGS_DET_FINAL_POS_Y, HEADINGS_DET_FINAL_POS_Z,
                            HEADINGS_DET_GOAL_ROT_QX, HEADINGS_DET_GOAL_ROT_QY, HEADINGS_DET_GOAL_ROT_QZ, HEADINGS_DET_GOAL_ROT_QW, HEADINGS_DET_FINAL_ROT_QX, HEADINGS_DET_FINAL_ROT_QY, HEADINGS_DET_FINAL_ROT_QZ, HEADINGS_DET_FINAL_ROT_QW,
                            HEADINGS_DET_GOAL_EULER_AX,HEADINGS_DET_GOAL_EULER_AY,HEADINGS_DET_GOAL_EULER_AZ,HEADINGS_DET_FINAL_EULER_AX,HEADINGS_DET_FINAL_EULER_AY,HEADINGS_DET_FINAL_EULER_AZ,HEADINGS_DET_SHAPE,HEADINGS_DET_CONDITION};

  
        StartCoroutine(saveLocal(pathLocal, entries2, values));

    }

    public void saveRunTime()
    {

        Vector3 handPos = Vector3.zero;
        Vector3 handRetPos = Vector3.zero;
        Vector3 trackPos = Vector3.zero;
        Vector3 trackPosRet = Vector3.zero;
        Quaternion trackRot = Quaternion.identity;
        bool grasping = false;
        bool handDetected = false;

        if(trackedHand != null)
        {   

            handPos = trackedHand.giveRealPosition();
            handRetPos = trackedHand.giveRetPosition();
        }
        if(trackedObject != null)
        {
            trackRot = trackedObject.transform.rotation;
        }
        if (tracker != null)
        {
            trackPos = tracker.realPosi;
            trackPosRet = tracker.retPosi;

        }

        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss dd/MM/yy");


        string[] values = { trialId, movementId, idStation.ToString(), timeStamp,
                            handPos.x.ToString(),handPos.y.ToString(),handPos.z.ToString(), handRetPos.x.ToString(),handRetPos.y.ToString(),handRetPos.z.ToString(),
                            trackPos.x.ToString(),trackPos.y.ToString(),trackPos.z.ToString(), trackPosRet.x.ToString(), trackPosRet.y.ToString(), trackPosRet.z.ToString(),
                            trackRot.x.ToString(),trackRot.y.ToString(),trackRot.z.ToString(),trackRot.w.ToString(), trackRot.eulerAngles.x.ToString(),trackRot.eulerAngles.y.ToString(),trackRot.eulerAngles.z.ToString()
                            , currentStage
                            };

        string pathLocal = PATH_LOCAL + NAME_RUN_TIME;
        string[] entries2 = { HEADINGS_RT_ID_TRIAL, HEADINGS_RT_ID_MOVEMENT, HEADINGS_RT_ID_STATION, HEADINGS_RT_TIME_STAMP,
                             HEADINGS_RT_REAL_POS_X, HEADINGS_RT_REAL_POS_Y, HEADINGS_RT_REAL_POS_Z, HEADINGS_RT_RET_POS_X, HEADINGS_RT_RET_POS_Y, HEADINGS_RT_RET_POS_Z
                            , HEADINGS_RT_TRACK_REAL_POS_X , HEADINGS_RT_TRACK_REAL_POS_Y, HEADINGS_RT_TRACK_REAL_POS_Z, HEADINGS_RT_TRACK_RET_POS_X, HEADINGS_RT_TRACK_RET_POS_Y, HEADINGS_RT_TRACK_RET_POS_Z
                            , HEADINGS_RT_TRACK_ROT_QUA_X, HEADINGS_RT_TRACK_ROT_QUA_Y, HEADINGS_RT_TRACK_ROT_QUA_Z, HEADINGS_RT_TRACK_ROT_QUA_W, HEADINGS_RT_TRACK_EULER_AX,HEADINGS_RT_TRACK_EULER_AY,HEADINGS_RT_TRACK_EULER_AZ, HEADINGS_RT_CURR_STAGE};

        if (counter < topRegisterToSave)
            rtLine = rtLine + string.Join(";", values) + Environment.NewLine;
        else if (counter == topRegisterToSave)
            rtLine = rtLine + string.Join(";", values);
        else
        {
            StartCoroutine(saveLocal(pathLocal, entries2, rtLine));
            rtLine = "";
        }
            // StartCoroutine(saveLocal(pathLocal, entries2, values));
        
    }

    public void saveHeadPose()
    {
        
        Vector3 hpPos = camera.transform.position;
        Quaternion hpRot = camera.transform.rotation;
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss dd/MM/yy");


        string[] values = { trialId, movementId, idStation.ToString(), timeStamp,
                            hpPos.x.ToString(),hpPos.y.ToString(),hpPos.z.ToString(),hpRot.x.ToString(),hpRot.y.ToString(),hpRot.z.ToString(),hpRot.w.ToString(),
            hpRot.eulerAngles.x.ToString(),hpRot.eulerAngles.y.ToString(),hpRot.eulerAngles.z.ToString(), currentStage };



        string pathLocal = PATH_LOCAL + NAME_HEAD_POSE;
        string[] entries2 = { HEADINGS_HP_ID_TRIAL, HEADINGS_HP_ID_MOVEMENT, HEADINGS_HP_ID_STATION,HEADINGS_HP_TIME_STAMP,
                             HEADINGS_HP_POS_X, HEADINGS_HP_POS_Y, HEADINGS_HP_POS_Z, HEADINGS_HP_ROT_QUA_X, HEADINGS_HP_ROT_QUA_Y, HEADINGS_HP_ROT_QUA_Z, HEADINGS_HP_ROT_QUA_W,
            HEADINGS_HP_EULER_AX,HEADINGS_HP_EULER_AY,HEADINGS_HP_EULER_AZ,HEADINGS_HP_CURR_STAGE };
        if (counter < topRegisterToSave)
            hpLine = hpLine + string.Join(";", values) + Environment.NewLine;
        else if (counter == topRegisterToSave)
            hpLine = hpLine + string.Join(";", values);
        else
        {
            StartCoroutine(saveLocal(pathLocal, entries2, hpLine));
            hpLine = "";
            counter = 1;
        }
        // StartCoroutine(saveLocal(pathLocal, entries2, values));

       
        
    }

   

}
