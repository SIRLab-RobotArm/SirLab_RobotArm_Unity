using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Turtlesim;

public class TurtlePoseSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/turtle1/pose";

    void Start()
    {
        ros = ROSConnection.instance;
        ros.Subscribe<PoseMsg>(topicName, OnPoseReceived);
    }

    void OnPoseReceived(PoseMsg msg)
    {
        // 2D → Unity 3D 좌표계로 매핑
        Vector3 pos = new Vector3(msg.x, msg.y, 0f);
        transform.position = pos;

        // 회전(θ는 라디안)
        transform.rotation = Quaternion.Euler(0f, 0f, -msg.theta * Mathf.Rad2Deg);
    }
}
