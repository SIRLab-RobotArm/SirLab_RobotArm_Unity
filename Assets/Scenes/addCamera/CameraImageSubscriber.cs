using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;   // ImageMsg 타입 사용 (sensor_msgs/msg/Image에 해당)

/// <summary>
/// ROS2에서 들어오는 이미지 토픽(/camera/camera/color/image_raw)을 구독해서
/// Unity UI의 RawImage에 실시간으로 띄워주는 스크립트.
/// 
/// 흐름 요약:
/// 1. Start()에서 ROSConnection 인스턴스를 가져오고, 특정 토픽을 Subscribe.
/// 2. 이미지 메시지를 받으면(OnImageReceived) 처음 한 번 Texture2D를 생성.
/// 3. 그 이후에는 msg.data(byte[])를 Texture2D에 그대로 복사해서 RawImage에 표시.
/// </summary>

public class CameraImageSubscriber : MonoBehaviour
{
    // --------------------
    // 1. 인스펙터에서 설정할 수 있는 필드들
    // --------------------

    // 구독할 ROS 이미지 토픽 이름
    // - 실제 ROS 쪽에서 `ros2 topic list` 했을 때 나오는 이름과 동일해야 함.
    // - 예: "/camera/color/image_raw" 또는 "/camera/camera/color/image_raw" 등
    public string topicName = "/camera/camera/color/image_raw";

    // Unity UI에 있는 RawImage 컴포넌트
    // - 여기에 Texture2D를 할당해서 화면에 띄운다.
    // - Canvas 안에 RawImage 만들어서 이 필드에 드래그&드롭하면 됨.
    public RawImage rawImageTarget;

    // --------------------
    // 2. 내부에서 사용할 필드들
    // --------------------

    // ROS-TCP Connector를 통해 ROS와 통신하는 싱글톤 객체
    ROSConnection ros;

    // 수신한 이미지를 복사해서 보여줄 Unity 텍스처
    Texture2D texture;

    // Texture2D를 이미 생성했는지 여부를 체크하는 플래그
    // - 첫 프레임에서만 width/height 크기에 맞춰 Texture2D를 만들고,
    //   이후에는 계속 재사용하기 위해 사용.
    bool textureInitialized = false;

    // --------------------
    // 3. Unity 생명주기 함수
    // --------------------

    void Start()
    {
        // ROSConnection 인스턴스를 가져오거나, 없으면 새로 생성
        // - ROSConnection 설정은 보통 씬에 하나만 존재.
        // - Project Settings 또는 ROSConnection 오브젝트에서
        //   IP, 포트, 메시지 등록 등이 되어 있어야 함.
        ros = ROSConnection.GetOrCreateInstance();

        // 특정 토픽을 구독(Subscribe) 등록.
        // - <ImageMsg>는 ROS 메시지 타입(sensor_msgs/msg/Image).
        // - topicName: 위에서 지정한 문자열 토픽 이름.
        // - OnImageReceived: 메시지가 수신될 때마다 호출될 콜백 함수.
        ros.Subscribe<ImageMsg>(topicName, OnImageReceived);
    }

    // --------------------
    // 4. ROS 이미지 콜백 함수
    // --------------------
    // 이 함수는 ROS에서 새로운 ImageMsg가 들어올 때마다 호출된다.
    void OnImageReceived(ImageMsg msg)
    {
        // 4-1. Texture2D를 아직 만든 적이 없다면(첫 프레임이라면),
        //      메시지의 width/height 정보를 이용해서 텍스처 생성.
        if (!textureInitialized)
        {
            // ROS 메시지 안에 포함된 이미지 너비/높이.
            // - msg.width, msg.height는 uint32 타입이므로 형변환 필요.
            int width = (int)msg.width;
            int height = (int)msg.height;

            // RealSense 컬러 이미지 포맷은 보통 "rgb8" (3채널, 8비트)
            // - Unity TextureFormat.RGB24: R,G,B 8비트씩 총 24비트 포맷.
            // - mipChain(마지막 인자)은 false로 해서 mipmap 사용 안 함.
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            // RawImage 타겟이 셋팅되어 있다면
            // - 지금 만든 Texture2D를 RawImage의 texture로 연결.
            // - 이후 texture 내용이 바뀌면 RawImage에도 바로 반영됨.
            if (rawImageTarget != null)
            {
                rawImageTarget.texture = texture;

                // 필요하다면 AspectRatioFitter를 써서 화면 비율을 맞추거나,
                // RectTransform 사이즈를 width/height 비율에 맞게 조정하는 것도 가능.
            }

            // 이제부터는 텍스처가 초기화되었다고 표시.
            textureInitialized = true;
        }

        // 4-2. 실제 픽셀 데이터를 Texture2D에 복사하는 부분
        // msg.data: raw 이미지 바이트 배열 (row-major 순서로 픽셀들이 나열되어 있음)
        // - ROS의 sensor_msgs/Image의 data 필드가 여기에 매핑된 것.
        // - 포맷이 rgb8인 경우: [R,G,B, R,G,B, R,G,B, ...] 이런 식으로 들어옴.
        texture.LoadRawTextureData(msg.data);

        // 4-3. 텍스처 적용
        // - LoadRawTextureData로 복사한 후에는 Apply()를 호출해줘야
        //   GPU 쪽 텍스처로 실제 반영되고 화면에 업데이트됨.
        texture.Apply();
    }
}
