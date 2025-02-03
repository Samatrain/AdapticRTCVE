using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Master conttroller of level logic.
//Responsible for managing the logic of the level... actions like progressing through stages
//  and handling UI instructions. Photon used for networking and multiplayer synch.
public class LevelController : MonoBehaviour, IPunObservable
{
    // Script to export logs
    private PersistanceManager persistanceManager;

  
    //Tracks if level is started
    public bool started;

    //Bool for triggering survey
    public bool surveyActivated;

    // Object to mannage all the text instructions to the users
    // TODO attach in VO editor
    public GameObject notificacionTextObject;

    // Start is called before the first frame update
    void Start()
    {
      

    }

    //PunRPC is a remote procedure that can be triggered across the network
    //PunRPC is a method-call on remote clients in the same room
    // - Photon is a Unity asset made for online multiplayer features
    [PunRPC]
    public void pressNextsStage()
    {
        //If the game is not started, start it. 
        //Meant to start with the tutorial then progress
        if (!started)
        {
            started = true;

            //TextMesh text = notificacionTextObject.GetComponent<TextMesh>();
            //text.text = "TUTORIAL";



            //TODO What is needed for having the instructinos and UI guide work

        }
        else
        {

        }

    }

    // Update is called once per frame
    void Update()
    {
        //On spacebar press, move to the next stage
        //PhotonTargets.All makes sure that all players in session call the pressNextsStage function
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<PhotonView>().RPC("pressNextsStage", PhotonTargets.All);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }
}
