using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
public class DualArmController : MonoBehaviour
{

    ROSConnection ros;
    public string leftTopic = "/left_arm_target_pose";
    public string rightTopic = "/right_arm_target_pose";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(leftTopic);
        ros.RegisterPublisher<PoseStampedMsg>(rightTopic);
    }

    void Update()
    {
        // 왼쪽 컨트롤러
        float leftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        if (leftTrigger > 0.5f) // 트리거가 눌렸을 때만
        {
            PublishControllerPose(
                OVRInput.Controller.LTouch,
                leftTopic
            );
        }

        // 오른쪽 컨트롤러
        float rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        if (rightTrigger > 0.5f)
        {
            PublishControllerPose(
                OVRInput.Controller.RTouch,
                rightTopic
            );
        }
    }

    // ROS 메시지 구조 만들기
    void PublishControllerPose(OVRInput.Controller controller, string topicName)
    {
        Vector3 pos = OVRInput.GetLocalControllerPosition(controller); // 3차원 공간에서의 (x, y, z) 위치 또는 방향 (m 단위), ROS에서는 이걸 geometry_msgs/Point로 보냅니다.
        Quaternion rot = OVRInput.GetLocalControllerRotation(controller); // 3D 공간에서의 회전 (x, y, z, w), geometry_msgs/Quaternion)으로 표현

        // ROS 메시지 쪽 구조
        // geometry_msgs/PoseStamped
        // ├─ std_msgs/Header header
        // │   ├─ uint32 seq
        // │   ├─ builtin_interfaces/Time stamp (메시지 생성 시각)
        // │   └─ string frame_id (좌표계 이름)
        // └─ geometry_msgs/Pose
        //     ├─ geometry_msgs/Point position
        //     │   ├─ float64 x
        //     │   ├─ float64 y
        //     │   └─ float64 z
        //     └─ geometry_msgs/Quaternion orientation
        //         ├─ float64 x
        //         ├─ float64 y
        //         ├─ float64 z
        //         └─ float64 w

        PoseStampedMsg pose = new PoseStampedMsg // PoseStamped = Pose + Header, 위치·회전 정보에 시간과 좌표계 정보(header) 를 붙인 메시지
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