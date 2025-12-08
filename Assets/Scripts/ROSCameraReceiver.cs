using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ROSCameraReceiver : MonoBehaviour
{
    [Header("ROS Settings")]
    // 수정 1: 맨 앞에 '/'를 반드시 붙여야 합니다.
    public string topicName = "/camera/camera/color/image_raw/compressed"; 

    [Header("UI Settings")]
    public RawImage cameraView;

    private ROSConnection ros;
    private Texture2D texture2D;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        texture2D = new Texture2D(640, 480); // 초기 크기 지정
        cameraView.texture = texture2D;

        // 구독 시작
        ros.Subscribe<CompressedImageMsg>(topicName, UpdateImage);
        
        Debug.Log($"[ROS] Subscribing to: {topicName}");
    }

    void UpdateImage(CompressedImageMsg message)
    {
        // 수정 2: 데이터가 진짜 들어오는지 로그 찍어보기 (성공하면 주석 처리하세요)
        Debug.Log($"[ROS] Image Received! Size: {message.data.Length} bytes");

        texture2D.LoadImage(message.data);
        // texture2D.Apply(); // LoadImage가 자동으로 하므로 생략 가능
    }
}