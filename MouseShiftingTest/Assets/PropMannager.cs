using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System;
//TODO Name and hierarchy is not good here.
// This manages the comunication with the Adaptci.
public class PropMannager : MonoBehaviour
{

    //presets for the different physical objects
    public enum PRESET_TYPE
    {
        NONE,
        FLAT,
        CYLINDER,
        BOOK
    };

    private MasterController masterController;

    //hardcoded to COM3 connection for arduino?
    //portName is a variable to be assigned in Unity's inspector
    public static string serialName = @"\\.\COM3";
    public string portName;

    public SerialPort mySPort;// = new SerialPort(serialName, 115200);

    //open the serial port for communication
    public void openPort()
    {
        mySPort.Open();
        mySPort.ReadTimeout = 10;
       // if(masterController.isDemo)
         //   mySPort.Write("<-99,4>");
    }

    // Start is called before the first frame update
    //initialises the serial connection when conditions arem et
    void Start()
    {

        //execute when the object is controlled by the local player (PhotonView.isMine)
        masterController = GetComponent<MasterController>();
        if (!GetComponent<PhotonView>().isMine )
        {
            this.enabled = false;
        }
        //what is condition SM_RT?
        //when it is met, attempt to open the serial port
        if (masterController != null
                && masterController.condition == MasterController.CONDITION.SM_RT)
        {
            try
            {
                openPort();
            }
            catch (Exception e)
            {
                mySPort = null;
                Debug.Log("ERROR OPENNING PORT " + e.Message);
            }
        }
        else
            this.enabled = false;
        
        
    }

    // Update is called once per frame
    void Update()
    {

        //when SM_RT condition is met, attempt to reopen the port
        //basically keep checking to make sure its open
        if (masterController.condition == MasterController.CONDITION.SM_RT)
            try
            {
                if (mySPort.CDHolding)
                { }
            }
            catch (Exception e)
            {
                Debug.Log("Port closed, re opening");
                mySPort = new SerialPort(serialName, 115200);
                openPort(); //REMOVE THIS
            }
    }

    //send commands to the device (arduino?)
    public void adapticCommand(PRESET_TYPE type)
    {
        
        //Debug.Log("PropManager ---- Adaptic  " + type.ToString());
        if (type == PRESET_TYPE.FLAT)
        {
            mySPort.Write("<1>");
        }
        else if (type == PRESET_TYPE.CYLINDER)
        {
            mySPort.Write("<2>");
        }
        else if (type == PRESET_TYPE.BOOK)
        {
            mySPort.Write("<1>");
        }
        //clear output buffer to keep communication clear
        mySPort.DiscardOutBuffer();
    }

    public string readData()
    {
        string data = "";
        /*//for(int i =0; i<10000 && !string.Equals(data,"OK"); i++)
            try
            {
                data = mySPort.ReadLine();
            }
            catch(Exception e)
            {

            }*/

        return data;
    }
}
