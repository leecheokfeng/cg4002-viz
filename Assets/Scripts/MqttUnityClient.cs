using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using M2MqttUnity;
using System;
using System.Linq;

public class MqttUnityClient : MonoBehaviour
{
    private MqttClient client;
    //public string broker = "test.mosquitto.org";
    public string broker = "172.26.190.146";

    // Set this in the inspector. If "false", the visibility reply will not be sent.
    // This is meant for stereo mode, where one (right side) camera should not send
    // the data so that we avoid double sending.
    public bool sendReply = true;

    // Topics to subscribe/publish to
    private string gamestateTopic = "";
    private string actionTopic = "";
    private string visibilityTopic = "";

    public HudController hudController;

    // List to store the messages
    private List<string> eventMessages = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        SetupClient();
        
    }

    // What happens when client receives a message
    private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string receivedMessage = Encoding.UTF8.GetString(e.Message);

        if (e.Topic == gamestateTopic)
        {
            Debug.Log($"Received gamestate. Gamestate: {receivedMessage}");

            // Delimiter "-"
            StoreMessage(e.Topic + "-" + receivedMessage);
        }
        else if (e.Topic == actionTopic)
        {
            string action = receivedMessage;
            Debug.Log($"Received action. Action: {action}");
            Debug.Log($"e.Topic is: {e.Topic}");
            Debug.Log($"actionTopic is: {actionTopic}");

            // Delimiter "-"
            StoreMessage(e.Topic + "-" + action);
        }
    }

    // Publish to visibility topic "True" or "False" depending on opponent detection.
    private void SendVisibilityReply(bool can_see)
    {
        string vis_reply;
        if (can_see)
        {
            vis_reply = "True";
        }
        else
        {
            vis_reply = "False";
        }
        client.Publish(visibilityTopic, Encoding.UTF8.GetBytes(vis_reply));
        Debug.Log($"Sent visibility data. can_see: {vis_reply}");
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.Disconnect();
        }
    }

    // Re-Setup broker to client connections. Put this in Start() or
    // call this (as debug mode button) whenever POV changes
    public void SetupClient()
    {
        // Disconnect from current connection.
        if (client != null)
        {
            client.Disconnect();
        }

        // Handle subcribed/published topics depending on player
        // Player 1
        if (ChangeScene.player == 1)
        {
            gamestateTopic = "Player 1 GameState";
            actionTopic = "Player 1 Action";
            visibilityTopic = "Player 2 Visibility";
        }
        // Player 2
        else
        {
            gamestateTopic = "Player 2 GameState";
            actionTopic = "Player 2 Action";
            visibilityTopic = "Player 1 Visibility";
        }

        client = new MqttClient(broker);

        // Register to message received event
        client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

        // Create a unique client ID and connect
        string clientId = System.Guid.NewGuid().ToString();
        client.Connect(clientId);

        // Subscribe to topics
        client.Subscribe(new string[] { gamestateTopic, actionTopic },
            new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    // Update is called once per frame
    void Update()
    {
        if (eventMessages.Count > 0)
        {
            // ToList() prevents InvalidOperationException: Collection was modified; enumeration operation may not execute.
            foreach (string msg in eventMessages.ToList())
            {
                ProcessMessage(msg);
            }
            eventMessages.Clear();
        }
    }

    // Adds received messages to eventMessage list.
    private void StoreMessage(string eventMsg)
    {
        eventMessages.Add(eventMsg);
    }

    /**
     * Input: msg - topic and message concatenated into single string
     * Parses msg into topic and message. Depending on topic (action/gamestate), calls
     * relevant functions to update HudController.
     */
    private void ProcessMessage(string msg)
    {
        Debug.Log("processed message: " + msg);

        // Determine if topic is action or gamestate
        string[] msg_parts = msg.Split("-");
        string topic = msg_parts[0];

        // If topic contains actions
        if (topic == actionTopic)
        {
            string action = msg_parts[1];
            bool can_see = TransmitActionToVisualiser(action);

            // Only true for LEFT camera (so no double sending in stereo mode)
            if (sendReply == true)
            {
                SendVisibilityReply(can_see);
            }
            
        }
        // If topic contains gamestate
        else if (topic == gamestateTopic)
        {
            // Split gamestate eg. "100,30,3,6,2,1,90,20,2,5,1,0" into array
            string[] gamestateElements = msg_parts[1].Split(",");

            TransmitGamestateToVisualiser(gamestateElements);
        }
        
    }

    // Transmit gamestate to HudController
    void TransmitGamestateToVisualiser(string[] gamestateElements)
    {
        // Exception handling
        try
        {
            // Convert string[] to int[]
            int[] ints = Array.ConvertAll(gamestateElements, int.Parse);

            int[] p1_state = { ints[0], ints[1], ints[2], ints[3], ints[4], ints[5] };
            int[] p2_state = { ints[6], ints[7], ints[8], ints[9], ints[10], ints[11] };

            hudController.ChangeGameState(p1_state, p2_state);
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log("Error message: " + e.Message + " Changing gamestate failed, likely missing data.");
        }
        // If gamestate update fails for whatever other reason.
        catch (Exception e)
        {
            Debug.Log("Error message: " + e.Message + " Changing gamestate failed." +
                " Possible causes: Data type conversion error.");
        }
        
    }

    // Transmit action to HudController and get response if needed
    bool TransmitActionToVisualiser(string action)
    {
        int response = hudController.HandleActionFromGameEngine(action, ChangeScene.player);
        // if opponent is detected, return true
        if (response == 1)
        {
            return true;
        }
        // else return false
        else
        {
            return false;
        }
    }
}
