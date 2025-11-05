using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;   // ImageMsg

public class CameraImageSubscriber : MonoBehaviour
{
    public string topicName = "/camera/camera/color/image_raw";
    public RawImage rawImageTarget;

    ROSConnection ros;
    Texture2D texture;
    bool textureInitialized = false;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImageMsg>(topicName, OnImageReceived);
    }

    void OnImageReceived(ImageMsg msg)
    {
        // 처음 한 번만 Texture2D 생성
        if (!textureInitialized)
        {
            int width = (int)msg.width;
            int height = (int)msg.height;

            // RealSense 컬러는 보통 rgb8 (3채널)
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            if (rawImageTarget != null)
            {
                rawImageTarget.texture = texture;
            }

            textureInitialized = true;
        }

        // msg.data: raw 이미지 바이트 배열 (row-major)
        texture.LoadRawTextureData(msg.data);
        texture.Apply();
    }
}
