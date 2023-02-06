using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoBasic : MonoBehaviour {

    private SerialPort arduino;
    public string port;
    
    private Thread readThread;
    private string readMessage;
    private string errorMessage;
    private bool isNewMessage;

    void Start() {
        if (port != "") {
            arduino = new SerialPort(port, 115200);
            arduino.ReadTimeout = 10;
            try {
                arduino.Open();
                readThread = new Thread(new ThreadStart(ArduinoRead));
                readThread.Start();
                Debug.Log("SerialPort Open");
            } catch (System.Exception e) {
                Debug.Log("SerialPort Fail To Open");
                Debug.Log(e);
            }
        }
    }

    // Print message when read a new message
    void Update() {
        if (isNewMessage) {
            Debug.Log(readMessage);
        }
        isNewMessage = false;
    }

    // Read messages from Arduino
    private void ArduinoRead() {
        while (arduino.IsOpen) {
            try {
                readMessage = arduino.ReadLine(); 
                isNewMessage = true;

            } catch (System.Exception e) {
                errorMessage = e.Message;
            }
        }
    }

    // Write messages to Arduino
    public void ArduinoWrite(string message) {
        Debug.Log(message);
        arduino.Write(message);
    }

    // Close Arduino when application ends
    void OnApplicationQuit() {
        if (arduino != null) {
            if (arduino.IsOpen) {
                arduino.Close();
            }
        }
    }

}