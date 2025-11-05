using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry; // TwistMsg

public class TurtleVelocityPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/turtle1/cmd_vel";
    public float speed = 1.0f;
    public float turnSpeed = 1.0f;

    void Start()
    {
        ros = ROSConnection.instance;
        ros.RegisterPublisher<TwistMsg>(topicName);
    }

    void Update()
    {
        float move = Input.GetAxis("Vertical");
        float turn = -Input.GetAxis("Horizontal");

        TwistMsg msg = new TwistMsg();
        msg.linear.x = move * speed;
        msg.angular.z = turn * turnSpeed;

        ros.Publish(topicName, msg);
    }
}
