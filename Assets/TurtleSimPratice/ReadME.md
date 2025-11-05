완벽해요 👍
이제 당신이 실제로 진행한 **Turtlesim–Unity 연동 과정**과
중간에 겪은 **에러/해결 기록**까지 모두 포함한 **최종 README.md (완성본)** 을 아래에 정리해드릴게요.
그대로 프로젝트 폴더(`SirLab_RobotArm_Unity/`)에 `README.md`로 저장하면 됩니다.

---

# 🐢 Unity ↔ ROS2 Turtlesim 연동 매뉴얼 (WSL + Unity)

> **환경:**
>
> * Windows 11 + WSL2 (Ubuntu 22.04)
> * ROS2 Humble
> * Unity 2022.x 이상
> * ROS–TCP Connector / ROS–TCP Endpoint

---

## 📘 프로젝트 개요

이 프로젝트는 **ROS2의 기본 시뮬레이터 Turtlesim**과 **Unity**를 연동하여
ROS에서의 거북이 움직임을 Unity에서 실시간으로 시각화하고,
Unity에서도 ROS 거북이를 직접 조작할 수 있도록 구성되었습니다.

---

## ⚙️ ROS2 환경 설정

### 1️⃣ ROS2 실행

WSL(우분투)에서 터미널 3개를 각각 열고 다음 명령어 실행 👇

```bash
# 터미널 1: Turtlesim 실행
source /opt/ros/humble/setup.bash
ros2 run turtlesim turtlesim_node

# 터미널 2: 키보드 입력 제어
source /opt/ros/humble/setup.bash
ros2 run turtlesim turtle_teleop_key

# 터미널 3: Unity와 ROS 통신 서버
source /opt/ros/humble/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint
```

> ✅ 참고:
> `default_server_endpoint`가 Unity와 ROS 간 **Bridge 역할**을 합니다.
> IP/Port는 기본적으로 `0.0.0.0:10000`에서 열립니다.

---

## 🧩 Unity 설정

### 1️⃣ ROS-TCP Connector 패키지 설치

* Unity 메뉴 → `Window > Package Manager > + > Add package from git URL`
* 입력:

  ```
  https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
  ```

---

### 2️⃣ ROS Settings 구성

`Robotics > ROS Settings` 메뉴에서 다음 설정 입력:

| 설정 항목                | 값                          |
| -------------------- | -------------------------- |
| **ROS IP Address**   | `hostname -I` 로 확인한 WSL IP |
| **Port**             | `10000`                    |
| **Connect On Start** | ✅ 체크                       |

> 🔍 예시
> `hostname -I` 결과가 `192.168.117.132` 라면,
> Unity → ROS IP Address를 `192.168.117.132`로 입력해야 함.

---

### 3️⃣ ROS 메시지(.msg) 불러오기

1. WSL에서 turtlesim 메시지 복사:

   ```bash
   mkdir -p "/mnt/d/PARA/0. Project/캡스톤/SirLab_RobotArm_Unity/Assets/ROSMessages/turtlesim"
   cp /opt/ros/humble/share/turtlesim/msg/*.msg \
   "/mnt/d/PARA/0. Project/캡스톤/SirLab_RobotArm_Unity/Assets/ROSMessages/turtlesim/"
   ```

2. Unity에서 메뉴 실행:
   `Robotics > ROS > Generate ROS Messages`

   * **ROS message path:**
     `D:\PARA\0. Project\캡스톤\SirLab_RobotArm_Unity\Assets\ROSMessages`
   * **Built message path:**
     `Assets\Scripts\ROSMessages`
   * **Build** 클릭 ✅

> 결과적으로 `PoseMsg.cs`, `ColorMsg.cs` 등 생성됨.

---

## 🧠 Unity 스크립트

### ▶ TurtlePoseSubscriber.cs

```csharp
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Turtlesim;

public class TurtlePoseSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/turtle1/pose";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseMsg>(topicName, OnPoseReceived);
    }

    void OnPoseReceived(PoseMsg msg)
    {
        Vector3 pos = new Vector3(msg.x, msg.y, 0f);
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, 0f, -msg.theta * Mathf.Rad2Deg);
    }
}
```

---

### ▶ TurtleVelocityPublisher.cs

```csharp
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;  // TwistMsg

public class TurtleVelocityPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/turtle1/cmd_vel";
    public float speed = 1.0f;
    public float turnSpeed = 1.0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);
    }

    void Update()
    {
        float move = Input.GetAxis("Vertical");
        float turn = -Input.GetAxis("Horizontal");

        TwistMsg msg = new TwistMsg();
        msg.linear.x = move * speed;
        msg.angular.z = turn * turnSpeed;

        ros.Publish(topicName, msg);
    }
}
```

---

## 🧱 Unity 씬 구성

| 오브젝트              | 컴포넌트                       | 설명                                |
| ----------------- | -------------------------- | --------------------------------- |
| **ROSConnection** | ROS Connection             | IP: 192.168.xxx.xxx / Port: 10000 |
| **Turtle (Cube)** | TurtlePoseSubscriber.cs    | ROS → Unity 위치 동기화                |
| **Controller**    | TurtleVelocityPublisher.cs | Unity → ROS 제어 명령 송신              |

---

## 🚀 실행 순서 요약

| 순서 | 위치        | 명령어 / 동작                                            |
| -- | --------- | --------------------------------------------------- |
| ①  | ROS 터미널 1 | `ros2 run turtlesim turtlesim_node`                 |
| ②  | ROS 터미널 2 | `ros2 run turtlesim turtle_teleop_key`              |
| ③  | ROS 터미널 3 | `ros2 run ros_tcp_endpoint default_server_endpoint` |
| ④  | Unity     | ▶ Play 클릭 (연결 성공 시 ROS↔Unity 통신 로그 표시)              |

---

## ⚠️ 문제 해결 (Troubleshooting)

| 에러 메시지                                                            | 원인                        | 해결 방법                                                      |
| ----------------------------------------------------------------- | ------------------------- | ---------------------------------------------------------- |
| ❌ `Connection failed (SocketException: 대상 컴퓨터에서 연결을 거부)`          | Unity의 ROS IP / Port 불일치  | `hostname -I`로 IP 확인 후 Unity ROS Settings의 IP와 일치시키기       |
| ❌ `Unknown message class 'ROSMessages/Pose'`                      | PoseMsg.cs의 메시지 이름이 잘못됨   | PoseMsg.cs 안의 `k_RosMessageName`을 `"turtlesim/Pose"`로 수정   |
| ⚠️ `ROSConnection.instance is obsolete`                           | ROS-TCP Connector의 API 변경 | `ROSConnection.GetOrCreateInstance()`로 교체                  |
| ⚠️ `Inconsistent declaration of topic '/turtle1/pose'`            | Unity와 ROS 메시지 이름 불일치     | `/turtle1/pose`와 `RosMessageTypes.Turtlesim.PoseMsg` 일치 확인 |
| ⚠️ Shader Keyword State mismatch                                  | Unity 렌더러 관련 (무시 가능)      | 빌드/실행엔 영향 없음                                               |
| ❌ `Failed to resolve message name: No module named 'ROSMessages'` | Unity에서 잘못된 패키지명으로 요청     | `.cs` 파일 수정 후 Unity 재컴파일 필요                                |

---

## ✅ 결과 확인

* ROS2 터틀심 창에서 거북이 이동 → Unity의 Cube도 동기화됨
* Unity에서 키보드(WASD / 화살표) 입력 → 터틀심 창의 거북이 실제로 움직임

🟢 **연동 성공 시 흐름**
`TurtleSim Node ↔ ROS TCP Endpoint ↔ Unity ROSConnection`

---

## 📁 폴더 구조 예시

```
Assets/
 ├── ROSMessages/
 │    └── turtlesim/
 │         ├── Pose.msg
 │         ├── Color.msg
 │         └── Velocity.msg
 ├── Scripts/
 │    ├── ROSMessages/
 │    │     └── Turtlesim/
 │    │          ├── PoseMsg.cs
 │    │          └── ColorMsg.cs
 │    ├── TurtlePoseSubscriber.cs
 │    └── TurtleVelocityPublisher.cs
 └── Scenes/
      └── TurtleSim.unity
```

---

## 🧠 요약

| 방향          | 토픽                 | 메시지 타입                | 설명                |
| ----------- | ------------------ | --------------------- | ----------------- |
| ROS → Unity | `/turtle1/pose`    | `turtlesim/Pose`      | 거북이 위치/회전 데이터 수신  |
| Unity → ROS | `/turtle1/cmd_vel` | `geometry_msgs/Twist` | Unity 입력을 ROS에 송신 |

---

> ✅ **결론**
> 세 개의 ROS 노드(`turtlesim_node`, `turtle_teleop_key`, `ros_tcp_endpoint`)와
> Unity의 ROSConnection을 연결하여
> **ROS의 거북이 움직임을 Unity에서 시각화 & 제어**하는 데 성공했습니다 🐢💨

---

원하면 이 README 끝부분에
“**추가 확장: RealSense D435 카메라 토픽 시각화**” 섹션도 바로 이어서 만들 수 있어요.
지금 구조 그대로 쓰면 카메라 토픽(`/camera/color/image_raw`)도 바로 연동됩니다.
