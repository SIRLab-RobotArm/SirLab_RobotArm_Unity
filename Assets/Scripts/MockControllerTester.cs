using UnityEngine;
using RosMessageTypes.Geometry; // PoseStampedMsg, PointMsg, QuaternionMsg
using RosMessageTypes.Sensor;   // JointStateMsg
using Unity.Robotics.ROSTCPConnector;
using System.Text; // String Builder를 위해 추가

public class MockControllerTester : MonoBehaviour
{
    // --- ROS ---
    ROSConnection ros;

    // --- 1. 발신 (Publisher) ---
    public string poseTopic = "/unity_target_pose";
    public float speed = 0.5f;
    public float radius = 0.1f;
    private Vector3 initialPosition;

    // --- 2. 수신 (Subscriber) ---
    public string jointTopic = "/ik_joint_commands";


    // 로봇 조인트 오브젝트
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform joint4;
    public Transform gripperLeft;
    public Transform gripperRight;
    
    // --- 디버깅용 ---
    // 발신 로그를 매 프레임마다 찍으면 콘솔이 멈출 수 있으므로, 1초에 한 번만 찍도록 제한합니다.
    // private float publishDebugTimer = 0f;
    // private const float PUBLISH_DEBUG_INTERVAL = 1.0f; // 1초 간격

    private float sendTimer = 0f;
    private const float SEND_INTERVAL = 5f;


    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // --- 1. 연결 및 등록 확인 로그 ---
        Debug.Log("--- ROS 연결 시도 ---");
        
        // 1. 발신자 등록
        ros.RegisterPublisher<PoseStampedMsg>(poseTopic);
        Debug.Log($"발신자(Publisher) 등록: <{poseTopic}>");

        // 2. 수신자 등록 (콜백 함수 지정)
        ros.Subscribe<JointStateMsg>(jointTopic, OnReceiveJointState);
        Debug.Log($"수신자(Subscriber) 등록: <{jointTopic}>");

        initialPosition = new Vector3(0.0f, 0.2f, 0.3f);
        
        Debug.Log("--- 테스트 시작 ---");
    }

    void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= SEND_INTERVAL)
        {
            sendTimer = 0f;

            float x = initialPosition.x + Mathf.Sin(Time.time * speed) * radius;
            float y = initialPosition.y + Mathf.Cos(Time.time * speed) * radius;
            float z = initialPosition.z;

            Vector3 mockPosition = new Vector3(x, y, z);
            Quaternion mockRotation = Quaternion.Euler(90, 0, 0); 

            PublishPose(mockPosition, mockRotation);

            Debug.Log($"[발신] ROS로 Pose 전송 -> Pos: ({mockPosition.x:F3}, {mockPosition.y:F3}, {mockPosition.z:F3})");
        }
    }

    // void Update()
    // {
    //     // --- 1. 발신 로직 ---
    //     float x = initialPosition.x + Mathf.Sin(Time.time * speed) * radius;
    //     float y = initialPosition.y + Mathf.Cos(Time.time * speed) * radius;
    //     float z = initialPosition.z;
        
    //     Vector3 mockPosition = new Vector3(x, y, z);
    //     Quaternion mockRotation = Quaternion.Euler(90, 0, 0); 
        
    //     PublishPose(mockPosition, mockRotation);

    //     // --- 2. 발신 데이터 디버그 로그 (1초마다) ---
    //     publishDebugTimer += Time.deltaTime;
    //     if (publishDebugTimer >= PUBLISH_DEBUG_INTERVAL)
    //     {
    //         publishDebugTimer = 0f; // 타이머 리셋
            
    //         // Vector3를 소수점 3자리까지 포매팅하여 로그 출력
    //         Debug.Log($"[발신] ROS로 Pose 전송 -> Pos: ({mockPosition.x:F3}, {mockPosition.y:F3}, {mockPosition.z:F3})");
    //     }
    // }

    /// <summary>
    /// 가상 위치(Pose)를 ROS로 전송합니다.
    /// </summary>
    void PublishPose(Vector3 pos, Quaternion rot)
    {
        PoseStampedMsg poseMsg = new PoseStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = "world"
            },
            pose = new PoseMsg
            {
                position = new PointMsg(pos.x, pos.y, pos.z),
                orientation = new QuaternionMsg(rot.x, rot.y, rot.z, rot.w)
            }
        };

        ros.Publish(poseTopic, poseMsg);
    }

    /// <summary>
    /// ROS로부터 JointState 메시지를 수신했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    // void ReceiveJointStateCallback(JointStateMsg msg)
    // {
    //     // --- 3. 수신 데이터 디버그 로그 ---
        
    //     // StringBuilder를 사용하면 여러 줄의 문자열을 효율적으로 만들 수 있습니다.
    //     StringBuilder sb = new StringBuilder();
    //     sb.AppendLine("--- [수신] IK 솔버 응답 (관절 각도) ---"); // 헤더

    //     for (int i = 0; i < msg.name.Length; i++)
    //     {
    //         float positionDeg = (float)msg.position[i] * Mathf.Rad2Deg;
            
    //         // 각 관절의 이름과 각도(도)를 문자열에 추가
    //         sb.AppendLine($"- {msg.name[i]}: {positionDeg:F2} 도");
    //     }

    //     // 완성된 문자열을 Unity 콘솔에 한 번에 출력
    //     Debug.Log(sb.ToString());
    // }

    void OnReceiveJointState(JointStateMsg msg)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- [수신] IK 솔버 응답 (관절 각도) ---");
        for (int i = 0; i < msg.name.Length; i++)
        {
            float positionDeg = (float)msg.position[i] * Mathf.Rad2Deg;
            sb.AppendLine($"- {msg.name[i]}: {positionDeg:F2} 도");
            sb.AppendLine($"- {msg.name[i]}: {positionDeg:F2} 도");
        }
        Debug.Log(sb.ToString());

        ApplyJointState(msg);
    }

    void ApplyJointState(JointStateMsg msg)
    {
        for (int i = 0; i < msg.name.Length; i++)
        {
            string jointName = msg.name[i];
            float angleRad = (float)msg.position[i];
            float angleDeg = angleRad * Mathf.Rad2Deg;

            switch (jointName)
            {
                case "joint1":
                    RotateZ(joint1, angleRad);
                    break;
                case "joint2":
                    RotateZ(joint2, angleRad);
                    break;
                case "joint3":
                    RotateZ(joint3, angleRad);
                    break;
                case "joint4":
                    RotateZ(joint4, angleRad);
                    break;
                case "gripper_left_joint":
                    RotateZ(gripperLeft, angleRad);
                    break;
                case "gripper_right_joint":
                    RotateZ(gripperRight, angleRad);
                    break;
            }
        }
    }

    void RotateZ(Transform transform, float angleRad)
    {
        if(jointTopic == null) return;
        transform.rotation = Quaternion.Euler(0, 0, angleRad * Mathf.Rad2Deg);
    }

}