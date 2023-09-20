using CymaticLabs.Unity3D.Amqp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAmqpClient : MonoBehaviour
{
    public AmqpClient myClient;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Publish()
    {
        string exchangeName = "amq.topic";
        string routingKey = "hello";
        string message = "hihihi";

        myClient.PublishToExchange(exchangeName, routingKey, message);

    }
}
