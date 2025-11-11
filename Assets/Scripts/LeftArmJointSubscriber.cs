using UnityEngine;
using RosMessageTypes.Sensor; // ROS 메시지 타입 중 sensor_msgs/JointState 사용
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;

public class LeftArmJointSubscriber : MonoBehaviour
{

    // ROS 연결
    private ROSConnection ros;

    // 구독할 토픽 이름 (MoveIt!에서 발행하는 JointState)
    public string topicName = "/left_arm_joint_states";

    // Unity 로봇팔 각 조인트에 해당하는 Transform
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform joint4;
    public Transform joint5;

    // ROS에서 전달받은 각도(rad 단위) 저장
    private Dictionary<string, float> jointAngles = new Dictionary<string, float>();

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointStateMsg>(topicName, OnJointStateReceived);
    }

    // 콜백 함수: ROS로부터 JointState 메시지를 받을 때마다 호출됨
    void OnJointStateReceived(JointStateMsg msg)
    {
        // JointStateMsg 구조:
        // msg.name[]  -> 조인트 이름 리스트 (예: ["joint1", "joint2", ...])
        // msg.position[] -> 각 조인트의 각도(rad)
        //   msg.velocity[], msg.effort[] 는 여기선 사용하지 않음
        for (int i = 0; i < msg.name.Length; i++)
        {
            string jointName = msg.name[i];         // 조인트 이름 가져오기
            float angle = (float)msg.position[i];   // 조인트 회전 각도(rad)
            jointAngles[jointName] = angle;         // 딕셔너리에 저장
        }
    }

    void Update()
    {
        // 각도 업데이트 (라디안 → 도 단위 변환)
        if (jointAngles.ContainsKey("joint1"))
            joint1.localRotation = Quaternion.Euler(0, Mathf.Rad2Deg * jointAngles["joint1"], 0);
        if (jointAngles.ContainsKey("joint2"))
            joint2.localRotation = Quaternion.Euler(Mathf.Rad2Deg * jointAngles["joint2"], 0, 0);
        if (jointAngles.ContainsKey("joint3"))
            joint3.localRotation = Quaternion.Euler(Mathf.Rad2Deg * jointAngles["joint3"], 0, 0);
        if (jointAngles.ContainsKey("joint4"))
            joint4.localRotation = Quaternion.Euler(0, Mathf.Rad2Deg * jointAngles["joint4"], 0);
        if (jointAngles.ContainsKey("joint5"))
            joint5.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * jointAngles["joint5"]);
    }
}
