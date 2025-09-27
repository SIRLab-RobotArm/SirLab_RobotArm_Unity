using UnityEngine;
using Unity.Robotics.ROSTCPConnector;


public class Quest2ArmController : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/arm_controller/joint_trajectory";

    private float[] jointPositions = new float[5]; // joint2~joint5 + gripper

    // Unity 내 로봇팔 링크 연결
    public GameObject link2;
    public GameObject link3;
    public GameObject link4;
    public GameObject link5;
    public GameObject gripper;

    void Start()
    {
        //ros = ROSConnection.GetOrCreateInstance();
        //ros.RegisterPublisher<JointTrajectoryMsg>(topicName);

        // ArticulationDrive 기본 세팅
        InitJoint(link2);
        InitJoint(link3);
        InitJoint(link4);
        InitJoint(link5);
        InitJoint(gripper);
    }

    void InitJoint(GameObject link)
    {
        var ab = link.GetComponent<ArticulationBody>();
        if (ab == null) return;

        ArticulationDrive drive = ab.xDrive;
        drive.forceLimit = 100f;
        drive.stiffness = 200f;
        drive.damping = 40f;
        ab.xDrive = drive;
    }

    void Update()
    {
        // 🎮 Quest2 입력 → jointPositions 업데이트
        jointPositions[0] += OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * 0.01f;
        jointPositions[1] += OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * 0.01f;

        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            jointPositions[2] += 0.01f;
            Debug.Log("test");
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            jointPositions[2] -= 0.01f;

        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
            jointPositions[3] += 0.01f;
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            jointPositions[3] -= 0.01f;

        if (OVRInput.Get(OVRInput.Button.One))
            jointPositions[4] += 0.01f;
        if (OVRInput.Get(OVRInput.Button.Two))
            jointPositions[4] -= 0.01f;

        // 👉 Unity 로봇팔 모델에도 반영
        ApplyJointPositionsToUnityModel();

        // 👉 ROS 발행
        //PublishJointCommands();
    }

    void ApplyJointPositionsToUnityModel()
    {
        SetJointTarget(link2, jointPositions[0]);
        SetJointTarget(link3, jointPositions[1]);
        SetJointTarget(link4, jointPositions[2]);
        SetJointTarget(link5, jointPositions[3]);
        SetJointTarget(gripper, jointPositions[4]);
    }

    void SetJointTarget(GameObject link, float targetPosition)
    {
        if (link == null) return;
        ArticulationBody ab = link.GetComponent<ArticulationBody>();
        if (ab == null) return;

        ArticulationDrive drive = ab.xDrive;
        drive.target = targetPosition * Mathf.Rad2Deg; // rad → deg 변환
        ab.xDrive = drive;
    }

    //void PublishJointCommands()
    //{
    //    JointTrajectoryMsg trajectoryMsg = new JointTrajectoryMsg();
    //    trajectoryMsg.joint_names = new string[] { "joint1", "joint2", "joint3", "joint4", "joint5" };
    //    trajectoryMsg.points = new JointTrajectoryPointMsg[1];

    //    JointTrajectoryPointMsg point = new JointTrajectoryPointMsg();
    //    point.positions = new double[] {
    //        0, // joint1 (고정)
    //        jointPositions[0],
    //        jointPositions[1],
    //        jointPositions[2],
    //        jointPositions[3]
    //    };

    //    point.time_from_start = new DurationMsg(1, 0);
    //    trajectoryMsg.points[0] = point;
    //    ros.Publish(topicName, trajectoryMsg);
    //}
}
