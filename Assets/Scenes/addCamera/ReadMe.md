# 🎥 ROS2 ↔ Unity RealSense D435 카메라 연동 가이드 (WSL 환경)

> **환경:**
>
> * Ubuntu 22.04 (WSL2)
> * ROS2 Humble
> * Intel RealSense D435
> * Unity 2022.x 이상 + ROS–TCP Connector

---

## 📘 개요

이 프로젝트는 **Intel RealSense D435 카메라**의 실시간 영상을
**ROS2 → Unity**로 전송해 Unity UI(RawImage)에 표시하는 과정입니다.

---

## ⚙️ ROS2 측 설정

### 1️⃣ 카메라 연결 확인

먼저 Windows에서 장치가 연결되어 있는지 확인:

```bash
lsusb | grep -i realsense
```

예시 출력:

```
Bus 003 Device 002: ID 8086:0b07 Intel Corp. RealSense D435
```

이게 나오면 연결 정상 ✅

---

### 2️⃣ RealSense ROS 드라이버 실행

ROS2에서 D435를 인식시키려면 다음 명령 실행:

```bash
source /opt/ros/humble/setup.bash
ros2 launch realsense2_camera rs_launch.py
```

정상일 경우 다음과 같은 토픽이 생성됨:

```bash
ros2 topic list | grep camera
```

결과 예시:

```
/camera/camera/color/image_raw
/camera/camera/color/camera_info
/camera/camera/depth/image_rect_raw
...
```

---

### 3️⃣ ROS–Unity 통신 서버 실행

다른 터미널에서 ROS–TCP Endpoint 실행:

```bash
source /opt/ros/humble/setup.bash
source ~/colcon_ws/install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint
```

터미널에 다음과 같은 로그가 나오면 성공:

```
[INFO] [UnityEndpoint]: Starting server on 0.0.0.0:10000
[INFO] [UnityEndpoint]: Connection from 192.168.xxx.xxx
```

---

## 🧩 Unity 설정

### 1️⃣ ROS–TCP Connector 설치

`Window > Package Manager > + > Add package from git URL`

```
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
```

---

### 2️⃣ ROS 메시지 생성 (`ImageMsg`)

1. **WSL에서 `Image.msg` 복사:**

   ```bash
   mkdir -p "/mnt/d/PARA/0. Project/캡스톤/SirLab_RobotArm_Unity/Assets/ROSMessages/sensor_msgs"
   cp /opt/ros/humble/share/sensor_msgs/msg/Image.msg \
   "/mnt/d/PARA/0. Project/캡스톤/SirLab_RobotArm_Unity/Assets/ROSMessages/sensor_msgs/"
   ```

2. **Unity에서 메시지 빌드:**

   * `Robotics > ROS > Generate ROS Messages`
   * **ROS message path:**
     `D:\PARA\0. Project\캡스톤\SirLab_RobotArm_Unity\Assets\ROSMessages`
   * **Built message path:**
     `Assets\Scripts\ROSMessages`
   * → **Build** 클릭 ✅
     `Assets/Scripts/ROSMessages/Sensor/ImageMsg.cs` 생성 확인

---

### 3️⃣ UI 구성

1. **Hierarchy → UI → Canvas** 생성
2. Canvas 안에 **Raw Image** 추가

   * 이름: `CameraView`
   * Inspector에서 RectTransform 크기 조절 (예: 1920×1080)

---

### 4️⃣ 스크립트 추가

**`CameraImageSubscriber.cs`**

```csharp
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
        if (!textureInitialized)
        {
            int width = (int)msg.width;
            int height = (int)msg.height;

            texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            rawImageTarget.texture = texture;
            textureInitialized = true;
        }

        texture.LoadRawTextureData(msg.data);
        texture.Apply();
    }
}
```

---

### 5️⃣ Unity 오브젝트 설정

1. `CameraView` 오브젝트 선택 → `Add Component` → `CameraImageSubscriber`
2. **Raw Image Target**에 자기 자신(`CameraView`) 드래그
3. **Topic Name** = `/camera/camera/color/image_raw`
4. 씬에 **ROSConnection 오브젝트** 존재 확인

   * ROS IP: `hostname -I` 결과값
   * Port: `10000`
   * Connect On Start ✅

---

## 🚀 실행 순서 요약

| 순서 | 동작           | 명령어                                                 |                    |
| -- | ------------ | --------------------------------------------------- | ------------------ |
| ①  | 카메라 연결 확인    | `lsusb                                              | grep -i realsense` |
| ②  | 카메라 노드 실행    | `ros2 launch realsense2_camera rs_launch.py`        |                    |
| ③  | Unity 브리지 실행 | `ros2 run ros_tcp_endpoint default_server_endpoint` |                    |
| ④  | Unity 실행     | ▶ Play                                              |                    |

---

## ⚠️ 트러블슈팅

| 문제                                        | 원인              | 해결 방법                                     |
| ----------------------------------------- | --------------- | ----------------------------------------- |
| Unity 콘솔에 `No module named 'ROSMessages'` | 메시지 빌드 안 됨      | `.msg` 복사 후 `Generate ROS Messages` 다시 실행 |
| Unity에서 화면 안 뜸                            | topicName 불일치   | `/camera/camera/color/image_raw` 맞는지 확인   |
| 색 이상 (BGR 뒤집힘)                            | 인코딩 `bgr8` 사용 중 | `LoadRawTextureData` 전 색상 스왑 추가 필요        |
| 연결 실패                                     | IP/Port 불일치     | Unity `ROS Settings`에서 IP 확인              |
| ROS 카메라 안 잡힘                              | USB 장치 미연결      | Windows에서 `usbipd`로 WSL attach 확인         |

---

## ✅ 결과

Unity의 Canvas → `RawImage` 오브젝트에
**RealSense D435 컬러 영상**이 실시간으로 표시됩니다 🎥
(ROS2에서 받은 `/camera/camera/color/image_raw` 토픽을 Texture2D로 변환)

---

> 🔧 **확장 가능:**
>
> * `/camera/camera/depth/image_rect_raw` 로 변경하면 **깊이 영상** 표시 가능
> * Turtlesim 및 다른 ROS 토픽과 병렬 실행도 가능 (동시 브리지)

---

