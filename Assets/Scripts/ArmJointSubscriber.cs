using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ArmJointSubscriber : MonoBehaviour
{
    [Header("ROS Topic Name")]
    public string jointTopic = "/ik_left_joint_commands";

    [Header("Robot Joint Transforms")]
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform joint4;
    public Transform gripperLeft;
    public Transform gripperRight;

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointStateMsg>(jointTopic, OnReceiveJointState);
        Debug.Log($"[ArmJointSubscriber] Subscribed to: {jointTopic}");
    }

    void OnReceiveJointState(JointStateMsg msg)
    {
        for (int i = 0; i < msg.name.Length; i++)
        {
            string name = msg.name[i];
            float rad = (float)msg.position[i];
            float deg = rad * Mathf.Rad2Deg;

            switch (name)
            {
                case "joint1":
                    RotateZ(joint1, deg);
                    break;
                case "joint2":
                    RotateZ(joint2, deg);
                    break;
                case "joint3":
                    RotateZ(joint3, deg);
                    break;
                case "joint4":
                    RotateZ(joint4, deg);
                    break;
                case "gripper_left_joint":
                    RotateZ(gripperLeft, deg);
                    break;
                case "gripper_right_joint":
                    RotateZ(gripperRight, deg);
                    break;
            }
        }
    }

    void RotateZ(Transform t, float angleDeg)
    {
        if (t == null) return;
        t.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
    }
}
