using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
public class LeftArmController : MonoBehaviour
{

    ROSConnection ros;
    public string topicName = "/left_arm_target_pose";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
    }

    void Update()
    {
        Vector3 pos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        Quaternion rot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        PoseStampedMsg pose = new PoseStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg(),
            pose = new PoseMsg
            {
                position = new PointMsg(pos.x, pos.y, pos.z),
                orientation = new QuaternionMsg(rot.x, rot.y, rot.z, rot.w)
            }
        };

        ros.Publish(topicName, pose);
    }

}