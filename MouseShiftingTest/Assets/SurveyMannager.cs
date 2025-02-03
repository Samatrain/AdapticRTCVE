using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveyMannager : MonoBehaviour
{
    /* --- variables ---*/
    public bool isSurveyActive;     //check if the survey is up
    private int questionNumber;     //track current number of question
    public GameObject[] options;    //Array of options GameObjects
    public GameObject normalMenu;   //normal menu UI element shown when survey is inactive
    public GameObject surveyMenu;   //survey menu UI shown when survey is ACTIVE
    public TextMesh questionMesh;   //displays question text in the survey menu
    private int actualStage;        //tracks the current stage of the survey
    private string answer;          //stores the user's answer to the question

    private string strPrevStage;    //stores the previous stage as reference
    private string strCurrStage;    //stores the current stage as reference

    // Handle the press of the trigger.
    private bool triggerPressed;    //bool to check if trigger is pressed
    LineRenderer lineRenderer = new LineRenderer(); //visually represent ray from the controller

    private NotificationsMannager notificationsMannager; //reference to NotificationsManager script to provide feedback to the user
    private PersistanceManager persistanceManager; //reference to PersistanceManager to save data
    // Start is called before the first frame update
    void Start()
    {
        isSurveyActive = false; //initialises survey state to inactive (so its not up at the start)
        lineRenderer = gameObject.GetComponent<LineRenderer>(); //retrieves LineRenderer component
        notificationsMannager = gameObject.GetComponent<NotificationsMannager>();   //retrieves NotificationManager component
        //persistanceManager  = gameObject.GetComponent<PersistanceManager>();
        lineRenderer.startWidth = 0; //set the start of the width of the LineRenderer (line won't initially be visible)
        lineRenderer.endWidth = 0.05f; //setwidth ti visible whena ctive
        lineRenderer.startColor = Color.green; //colour of the starting point of the line is green
        lineRenderer.endColor = Color.white; //colour of the end point of the line is white
        actualStage = 0;
        strPrevStage = ""; //initialise previous stage to empty string
        strCurrStage = ""; //initialise current stage to empty string
        lineRenderer.enabled = false; //disables LineRenderer at first to make the line invisible
    }

    // Update is called once per frame
    void Update()
    {
        //Check if the survey is active
        // - **find what makes the survey active?**
        if(isSurveyActive)
        {

            int layerMask = 1 << 9; //raycasting
            RaycastHit hit; //store info about raycast hits

            //Get controller position and rotation (OVR is for Oculus controllers)
            Vector3 controllerPoristion = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) + GameObject.Find("OVRCameraRig").transform.position;
            Vector3 controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch) * Vector3.forward;
            Ray raydirection = new Ray(controllerPoristion, controllerRotation); //creates ray in the dircetion of the contorller's forward motion

            lineRenderer.SetPosition(0, controllerPoristion); //set the start point of the line at the controller's position
            lineRenderer.SetPosition(1, controllerPoristion + raydirection.direction*5f); //set the endpoint of the line 5 units away in the direction of the ray
            
            //loop through options and set material colour to white
            for (int i = 0; i < options.Length; i++)
            {
                Renderer rend = options[i].GetComponent<Renderer>();
                rend.material.color = Color.white;
            }

            //raycast to determine which option the controller is pointint at
            if (Physics.Raycast(raydirection, out hit, Mathf.Infinity, layerMask))
            {
                Renderer rend = hit.collider.gameObject.GetComponent<Renderer>();
                rend.material.color = Color.green; //highlight the selected option in green
                TextMesh textAnswer = hit.collider.gameObject.GetComponentInChildren<TextMesh>(); //get text associated witht he selected option
                answer = textAnswer.text; //store the text of the selected option as the answer
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log("Did Hit");
            }
            else
            {
                answer = ""; //if no option is selected, reset the answer
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                //Debug.Log("Did not Hit");
            }

            //If the trigger is pressed, meaning if the user has made a selection
            if (!triggerPressed)
            {
                //See if the trigger has been pressed more than 70%
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.7f)
                {
                    triggerPressed = true; //mark the trigger as pressed
                    if (answer != "") //if an answer is selected, proceed to the next question in the survey
                        nextQuestion();
                    //else
                       // notificationsMannager.messageToUser("Select one of the options in the wall");
                }
            }
            else
            {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) < 0.5f)
                {
                    triggerPressed = false;
                }
            }
        }
    }

    public void startSurvey(int stage, string strageStr)
    {
        strPrevStage = strCurrStage; //save the current stage as the previous stage
        strCurrStage = strageStr;   //set the current stage to the new stage
        actualStage = stage;    //set the actual stage number
        lineRenderer.enabled = true;    //enable the LineRenderer to visualise the ray
        isSurveyActive = true;  //mark the survey as active
        normalMenu.SetActive(false); //hide the normal menu
        surveyMenu.SetActive(true); //show the survey menu
        questionMesh.text = "From 1 to 7, how easy was to accomplish the task? \n(1 Very hard - 7 Very easy)";
        questionNumber = 1;
    }

    //Handles progression of the survey
    // - Determine which question to show next and manage the end of the survey once all questoins have been answered
    private void nextQuestion()
    {
        //Remove any newline characters fromt eh question text ot ensure it is
        //  properly formatted before moving to the nex question.
        string question = questionMesh.text.Replace("\n","");

        //If greater than 2, the survey is in its later stages
        if (actualStage > 2)
        {
            if(questionNumber == 1)
            { 
                questionMesh.text = "From 1 to 7, Compared to the previous stage \n how easier was to accomplish the task? \n (1 Much harder - 7 Much easier)";
                questionNumber++; //increase question number to indicate a new question
            }
            else
            {
                //End the survey when no more questions to ask
                isSurveyActive = false; //set the survey to inactive
                normalMenu.SetActive(true); //show the normal menu (it was hidden during the survey)
                surveyMenu.SetActive(false); //hide the survey menu
                lineRenderer.enabled = false; //disable the LineRendere (ray visualisation)
            }
        }
        else
        {
            //end the survey if the stage is 2 or lower (after showing the first question)
            isSurveyActive = false; //set the survey to inactive, ending the survey
            normalMenu.SetActive(true); //show the normal menu
            surveyMenu.SetActive(false); //hide the survey menu
            lineRenderer.enabled = false; //disable ray visualisation
        }
    }

}
