using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerMannager : MonoBehaviour
{
    //Trackers for the VR controllers
    public Tracker leftTracker;
    public Tracker rightTracker;

    //These functional trackers act as references to the actual trackers
    //  to get data from them for operations
    //These can then either match the physical trackers or be reassigned
    //  based on their condition for retargeting
    //They are meant to be used as references when the RT or OO conditions are met
    public Tracker fLeftTracker;
    public Tracker fRightTracker;

    //Reference to the MasterController script that manages logic and other scripts
    private MasterController masterController;

    //Manage the overall tracking system to provide tracker references
    public TrackerSystemsMannager trackerSystemsMannager;

    // Start is called before the first frame update
    void Start()
    {
        //Get the MasterController component attached to this GameObject
        masterController = gameObject.GetComponent<MasterController>();
        
        //If this script doesn't belong to the local player, disable it.
        if (!GetComponent<PhotonView>().isMine)
        {
            this.enabled = false;
            trackerSystemsMannager.gameObject.SetActive(false);
        }
       
            
    }

    // Update is called once per frame
    void Update()
    {
        //Ensure the script is only active for the local player
        if (GetComponent<PhotonView>().isMine)
        {
            //Check if left tracker (controller) is not assigned
            if(leftTracker == null)
            {
                //If it's not assigned, try to assign it from the TrackerSystemsManager
                if (trackerSystemsMannager.leftTracker != null)
                    leftTracker = trackerSystemsMannager.leftTracker;
                else
                    Debug.LogError("No tracker found");
            }
            //Check if right tracker (controller) is not assigned
            if (rightTracker == null)
            {
                //If it's not assigned, try to assign it from the TrackerSystemsManager
                if (trackerSystemsMannager.rightTracker != null)
                    rightTracker = trackerSystemsMannager.rightTracker;
                else
                    Debug.LogError("No tracker found");
            }
        }
    }
    // Pair tracker with propcontroller. If RT condition is applied, both cirutl objects will be attached to the same
    // tracker. Left one here.
    public void setTrackers()
    {
        //if (GetComponent<PhotonView>().isMine)
        //{
            //Check the current condition in the MasterController
            if (masterController.condition == MasterController.CONDITION.NM_RT ||
                masterController.condition == MasterController.CONDITION.SM_RT)
            {
                //Retargeting conditions:
                //  - Both functional trackers use the left tracker
                fLeftTracker = leftTracker;
                fRightTracker = leftTracker;
            }
            else
            {
                //Non-retargeting conditions:
                //  - Functional trackers match their respective physical trackers
                fLeftTracker = leftTracker;
                fRightTracker = rightTracker;
            }
       // }
    }
}
