using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class VRJoyPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/vr/joy";
    
    // 조이스틱 데드존
    public const float DEADZONE = 0.1f;
    // 트리거가 눌렸다고 판단할 기준값 (0.0 ~ 1.0)
    private const float TRIGGER_THRESHOLD = 0.5f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JoyMsg>(topicName); // JoyMsg : ROS Message Type for C#. it is converted to sensor_msgs/Joy in ROS by ros_tcp_connector.
    }

    void Update()
    {
        // === 1. 입력 감지 (왼쪽 컨트롤러 단독 사용) ===
        
        // 스틱 (X, Y 축) - Vector2
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        
        // 트리거 (누르는 깊이) - float (0.0 ~ 1.0)
        float handTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch); 
        float indexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        
        // === 2. 입력값 정제 (Deadzone 처리) ===
        float inputX = Mathf.Abs(stick.x) > DEADZONE ? stick.x : 0.0f;
        float inputY = Mathf.Abs(stick.y) > DEADZONE ? stick.y : 0.0f;

        // 트리거 상태 확인 (눌렸는지 안 눌렸는지)
        bool isHandTriggerOn = handTrigger > TRIGGER_THRESHOLD;
        bool isIndexTriggerOn = indexTrigger > TRIGGER_THRESHOLD;


        // === 3. Joy 메시지 생성 및 매핑 ===
        // axes 배열: [J1, J2, J3, J4, GripClose, GripOpen] 순서로 약속
        JoyMsg joy = new JoyMsg();
        joy.axes = new float[6];

        joy.axes[0] = inputX; // 허리 (좌우)

        if (isHandTriggerOn && isIndexTriggerOn)
        {
            // [모드 3] 둘 다 누름 -> Joint 4 (손목) 제어
            // J4는 Stick Y로 제어
            joy.axes[1] = 0.0f;
            joy.axes[2] = 0.0f;
            joy.axes[3] = inputY; 
        }
        else if (isHandTriggerOn && !isIndexTriggerOn)
        {
            // [모드 2] 중지(Hand)만 누름 -> Joint 3 (팔꿈치) 제어
            // J3는 Stick Y로 제어
            joy.axes[1] = 0.0f;
            joy.axes[2] = inputY;
            joy.axes[3] = 0.0f;
        }
        else if (isIndexTriggerOn && !isHandTriggerOn)
        {
            joy.axes[4] = indexTrigger;
            joy.axes[5] = indexTrigger;

        }
        else
        {
            // [모드 1: 기본] 아무것도 안 누름 -> Joint 1, 2 (허리, 어깨) 제어
            joy.axes[1] = inputY; // 어깨 (상하)
            joy.axes[2] = 0.0f;
            joy.axes[3] = 0.0f;
        }

        // --- B. 그리퍼 제어 (항상 전송) ---
        // 검지 트리거의 깊이(0.0~1.0)를 그대로 보냅니다.
        // 팔 움직임 모드와 상관없이 항상 작동해야 물건을 잡은 채로 팔을 움직일 수 있습니다.
        // joy.axes[4] = indexTrigger;
        // joy.axes[5] = indexTrigger;

        // === 4. 전송 ===
        ros.Publish(topicName, joy);
    }
}