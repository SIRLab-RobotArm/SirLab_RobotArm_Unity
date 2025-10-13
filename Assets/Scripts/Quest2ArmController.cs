// using UnityEngine;
// using Unity.Robotics.ROSTCPConnector;
// using System.Collections;

// public class Quest2ArmController : MonoBehaviour
// {
//     ROSConnection ros;
//     public string topicName = "/arm_controller/joint_trajectory";

//     private float[] jointPositions = new float[5]; // joint2~joint5 + gripper
//     private Vector3 lastControllerPosition;

//     // Unity 내 로봇팔 링크 연결
//     public GameObject link2;
//     public GameObject link3;
//     public GameObject link4;
//     public GameObject link5;
//     public GameObject gripper;

//     void Awake()
//     {
//         // Start()보다 먼저 실행되므로 PhysX 초기화 전에 설정 가능
//         InitializeAllJoints();
//     }

//     IEnumerator Start()
//     {

//         Debug.Log("🎮 Quest2ArmController starts!");
//         lastControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);

//         // 혹시라도 ArticulationBody 초기화가 늦을 경우 대비
//         yield return null;
//         InitializeAllJoints();

//         // ROS 연결은 필요 시 주석 해제
//         // ros = ROSConnection.GetOrCreateInstance();
//         // ros.RegisterPublisher<JointTrajectoryMsg>(topicName);
//     }


//     void InitializeAllJoints()
//     {
//         // 각 관절의 stiffness, damping, forceLimit 값을 여기서 설정하세요.
//         // 예시 값:
//         InitJoint(link2, 1000f, 100f, 100f);
//         InitJoint(link3, 1000f, 100f, 100f);
//         InitJoint(link4, 1000f, 100f, 100f);
//         InitJoint(link5, 1000f, 100f, 100f);
//         InitJoint(gripper, 1000f, 100f, 100f);
//     }

//     void InitJoint(GameObject link, float stiffness, float damping, float forceLimit)
//     {
//         var ab = link.GetComponent<ArticulationBody>();
//         if (ab == null) return;

//         ArticulationDrive drive = ab.xDrive;
//         drive.forceLimit = forceLimit;
//         drive.stiffness = stiffness;
//         drive.damping = damping;
//         ab.xDrive = drive;
//     }

//     void Update()
//     {
//         // 🎮 Quest2 입력 → jointPositions 업데이트

//         float handTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
//         // 🎮 왼손 컨트롤러의 위치를 가져옵니다.
//         Vector3 currentControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
//         // 🎮 이전 프레임으로부터의 위치 '변화량'을 계산합니다.
//         Vector3 deltaPosition = currentControllerPosition - lastControllerPosition;

//         // 🧭 로봇팔의 로컬 기준으로 변환
//         Vector3 localDelta = transform.InverseTransformDirection(deltaPosition);

//         if (handTriggerValue > 0.6f)
//         {
//             // 🎯 Time.deltaTime 보정 + 로컬 좌표 기준으로 관절 제어
//             float speedMultiplier = 10.0f;

//             // jointPositions[0] += deltaPosition.x * -speedMultiplier * Time.deltaTime;  // 좌우
//             // jointPositions[1] += deltaPosition.z * speedMultiplier * Time.deltaTime;  // 앞뒤
//             // jointPositions[2] += deltaPosition.y * -speedMultiplier * Time.deltaTime;  // 위아래

//             jointPositions[0] += deltaPosition.x * -speedMultiplier;  // 좌우
//             jointPositions[1] += deltaPosition.z * speedMultiplier;  // 앞뒤
//             jointPositions[2] += deltaPosition.y * -speedMultiplier;  // 위아래

//             // jointPositions[0] += localDelta.x * -speedMultiplier;  // 좌우
//             // jointPositions[1] += localDelta.z * speedMultiplier;  // 앞뒤
//             // jointPositions[2] += localDelta.y * -speedMultiplier;  // 위아래
//         }

//         // 🎮 현재 위치를 다음 프레임을 위한 '이전 위치'로 저장합니다.
//         lastControllerPosition = currentControllerPosition;

//         // 👉 Unity 로봇팔 모델에도 반영
//         ApplyJointPositionsToUnityModel();

//         // 👉 ROS 발행
//         //PublishJointCommands();
//     }

//     void ApplyJointPositionsToUnityModel()
//     {
//         SetJointTarget(link2, jointPositions[0]);
//         SetJointTarget(link3, jointPositions[1]);
//         SetJointTarget(link4, jointPositions[2]);
//         SetJointTarget(link5, jointPositions[3]);
//         SetJointTarget(gripper, jointPositions[4]);
//     }

//     void SetJointTarget(GameObject link, float targetPosition)
//     {
//         if (link == null) return;
//         ArticulationBody ab = link.GetComponent<ArticulationBody>();
//         if (ab == null) return;

//         ArticulationDrive drive = ab.xDrive;
//         drive.target = targetPosition * Mathf.Rad2Deg; // rad → deg 변환
//         ab.xDrive = drive;
//     }

//     //void PublishJointCommands()
//     //{
//     //    JointTrajectoryMsg trajectoryMsg = new JointTrajectoryMsg();
//     //    trajectoryMsg.joint_names = new string[] { "joint1", "joint2", "joint3", "joint4", "joint5" };
//     //    trajectoryMsg.points = new JointTrajectoryPointMsg[1];

//     //    JointTrajectoryPointMsg point = new JointTrajectoryPointMsg();
//     //    point.positions = new double[] {
//     //        0, // joint1 (고정)
//     //        jointPositions[0],
//     //        jointPositions[1],
//     //        jointPositions[2],
//     //        jointPositions[3]
//     //    };

//     //    point.time_from_start = new DurationMsg(1, 0);
//     //    trajectoryMsg.points[0] = point;
//     //    ros.Publish(topicName, trajectoryMsg);
//     //}
// }


using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using System.Collections;

public class Quest2ArmController : MonoBehaviour
{
    // ROS 연결 관련 변수는 주석 처리 상태 유지
    // ROSConnection ros;
    // public string topicName = "/arm_controller/joint_trajectory";

    // 관절의 목표 위치를 저장하는 배열
    private float[] jointPositions = new float[5]; // joint2~joint5 + gripper

    // Unity 내 로봇팔 링크 연결 (Inspector에서 설정)
    public GameObject link2;
    public GameObject link3;
    public GameObject link4;
    public GameObject link5;
    public GameObject gripper;

    // 조작 감도 설정
    private float speedMultiplier = 10.0f; // 조이스틱 및 버튼 감도
    private const float GRIPPER_SPEED = 0.5f; // 그리퍼 조종 감도

    void Awake()
    {
        // ArticulationBody의 물리 속성 초기 설정
        InitializeAllJoints();
    }

    // Start() 함수는 코루틴 대신 void로 변경 (yield return null; 필요없음)
    IEnumerator Start()
    {
        Debug.Log("🎮 Quest2ArmController starts!");
        yield return null;
        InitializeAllJoints();
    }

    void InitializeAllJoints()
    {
        // 각 관절의 stiffness, damping, forceLimit 값을 여기서 설정
        // 모든 관절에 동일한 값 적용
        InitJoint(link2, 1000f, 100f, 100f);
        InitJoint(link3, 1000f, 100f, 100f);
        InitJoint(link4, 1000f, 100f, 100f);
        InitJoint(link5, 1000f, 100f, 100f);
        InitJoint(gripper, 1000f, 100f, 100f);
    }

    void InitJoint(GameObject link, float stiffness, float damping, float forceLimit)
    {
        if (link == null) return;
        var ab = link.GetComponent<ArticulationBody>();
        if (ab == null) return;

        ArticulationDrive drive = ab.xDrive;
        drive.forceLimit = forceLimit;
        drive.stiffness = stiffness;
        drive.damping = damping;
        ab.xDrive = drive;
    }

    void Update()
    {
        // 🎮 PrimaryHandTrigger (그립)의 값을 가져와서 조건부 로직에 사용
        float handTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        bool isHandTriggerPressed = handTriggerValue > 0.6f;

        // 🕹️ 조이스틱 입력 (thumbstick)
        Vector2 thumbstickValue = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        if (isHandTriggerPressed)
        {
            // PrimaryHandTrigger를 누른 상태에서는 link5를 조종
            jointPositions[3] += thumbstickValue.x * speedMultiplier * Time.deltaTime; // jointPositions[3]
            jointPositions[4] += thumbstickValue.y * speedMultiplier * Time.deltaTime; // 그리퍼
        }
        else
        {
            // PrimaryHandTrigger를 누르지 않은 상태에서는 link2와 link3를 조종
            jointPositions[0] += thumbstickValue.x * speedMultiplier * Time.deltaTime; // link2
            jointPositions[1] += thumbstickValue.y * speedMultiplier * Time.deltaTime; // link3
        }

        // 🔘 버튼 입력
        // Button One (오른손 A 또는 왼손 X 버튼) - link4 앞뒤 조종
        if (OVRInput.Get(OVRInput.Button.One))
        {
            jointPositions[2] += speedMultiplier * Time.deltaTime;
        }
        // Button Two (오른손 B 또는 왼손 Y 버튼) - link4 앞뒤 조종
        if (OVRInput.Get(OVRInput.Button.Two))
        {
            jointPositions[2] -= speedMultiplier * Time.deltaTime;
        }

        // 👉 로봇팔 모델에 값 반영
        ApplyJointPositionsToUnityModel();
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
        drive.target = targetPosition * Mathf.Rad2Deg;
        ab.xDrive = drive;
    }

    // ROS 발행 코드 (주석 처리)
    // void PublishJointCommands() { ... }
}