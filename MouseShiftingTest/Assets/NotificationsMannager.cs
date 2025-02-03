using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsMannager : MonoBehaviour
{

    //An array of TextMesh objects for each step in the notification process
    // - Like steps that progress as the user completes tasks
    public TextMesh[] steps;

    //TextMesh that shows current stage of the trial
    public TextMesh personalStageBar;

    //Track the goals completed
    public int counterGoals;

    //Determines whether the master has the ability to decide the next action
    public bool masterDecide;

    // Start is called before the first frame update
    void Start()
    {
        //Initialise the counter for completed goals to 0, make sure system is reset
        counterGoals = 0;
        masterDecide = false;
    }

    //Detect if a trigger was pressed
    private bool triggerPressed;

    //Register a completed goal in the trial
    public void registerGoal()
    {
        //Increase the counter to show that one has been completed
        counterGoals++;

        //If two goals have been completed, displayt notification
        if(counterGoals == 2)
        {
            //Call method to show its done
            showGoalDone(true);
            //Reset counter
            counterGoals = 0;
            //Allow master to decide next action
            masterDecide = true;
        }

        


    }
    
    //Displays or hides "goal done" notification, but not implemented yet?
    void showGoalDone(bool show)
    {
       
    }
    // Update is called once per frame
    void Update()
    {
    }

    //Change the title of the step in the notification system
    public void changeTitle(string ntitle)
    {
        //Update the text on the stage bar
        //Remember: stage bar is a textmesh used to show the progress of the user
        personalStageBar.text = ntitle;
    }

    //Highlights a step in the notificatoin system
    public void lightStepNotification(int step)
    {
        //Deactivate all steps initially
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].gameObject.SetActive(false);
        }

        //Activate a specific step based on the given index.
        //This goes back to the steps TextMesh array from the beginning of this script.
        steps[step].gameObject.SetActive(true);
    }
    /*
    public void messageToUser(string message)
    {
        StartCoroutine(showMessage(message));
    }
    /*
    IEnumerator showMessage(string message)
    {
        personalNotificationBar.gameObject.SetActive(true);
        personalNotificationBar.text = message;
        yield return new WaitForSeconds(2);
        personalNotificationBar.gameObject.SetActive(false);
    }
    */
    /*
    public void normalSettings()
    {
        counterGoals = 0;
        showGoalDone(false);
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].color = Color.white;
            steps[i].fontSize = 35;
        }
    }*/
}
