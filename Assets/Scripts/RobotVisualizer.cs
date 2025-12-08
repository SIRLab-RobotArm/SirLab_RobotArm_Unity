using UnityEngine;

public class RobotVisualizer : MonoBehaviour
{
    [Header("Joint Transforms (Drag & Drop)")]
    public Transform joint1; // 허리
    public Transform joint2; // 어깨
    public Transform joint3; // 팔꿈치
    public Transform joint4; // 손목
    public Transform gripperLeft;
    public Transform gripperRight;

    [Header("Settings")]
    public float rotationSpeed = 50.0f; // 회전 속도

    // 각 관절의 회전 축 (모델마다 다를 수 있음, 보통 Y축)
    private Vector3 axisJ1 = Vector3.up;    // Y축
    private Vector3 axisJ2 = Vector3.right; // X축 (모델에 따라 forward 등 확인 필요)
    private Vector3 axisJ3 = Vector3.right;
    private Vector3 axisJ4 = Vector3.right;

    // 관절 제한 각도 (Min, Max) - OpenManipulator 사양 참고
    // Unity Inspector에서 조절 가능
    public Vector2 limitJ1 = new Vector2(-180, 180);
    public Vector2 limitJ2 = new Vector2(-100, 90);
    public Vector2 limitJ3 = new Vector2(-80, 80);
    public Vector2 limitJ4 = new Vector2(-100, 110);
    
    // 그리퍼 설정
    public float gripperOpenAngle = 0.0f;  // 열렸을 때 로컬 회전값
    public float gripperCloseAngle = -20.0f; // 닫혔을 때 로컬 회전값

    // 현재 각도 저장용
    private float[] currentAngles = new float[4];

    void Start()
    {
        // 시작 시 현재 로봇의 각도를 저장
        if(joint1) currentAngles[0] = GetInspectorRotation(joint1, axisJ1);
        if(joint2) currentAngles[1] = GetInspectorRotation(joint2, axisJ2);
        if(joint3) currentAngles[2] = GetInspectorRotation(joint3, axisJ3);
        if(joint4) currentAngles[3] = GetInspectorRotation(joint4, axisJ4);
    }

    void Update()
    {
        // VRJoyPublisher와 똑같은 입력 로직 사용
        HandleInput();
    }

    void HandleInput()
    {
        // 1. 입력 받기
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        float handTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        float indexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

        float inputX = Mathf.Abs(stick.x) > 0.1f ? stick.x : 0.0f;
        float inputY = Mathf.Abs(stick.y) > 0.1f ? stick.y : 0.0f;

        bool isHandTriggerOn = handTrigger > 0.5f;
        bool isIndexTriggerOn = indexTrigger > 0.5f;

        // 2. 관절 각도 계산 (속도 * 시간 * 입력값)
        float moveStep = rotationSpeed * Time.deltaTime;

        // Joint 1 (허리) - 항상 작동
        currentAngles[0] += inputX * moveStep; // 방향 반대면 -= 로 수정

        if (isHandTriggerOn && isIndexTriggerOn)
        {
            // Joint 4 (손목)
            currentAngles[3] += inputY * moveStep;
        }
        else if (isHandTriggerOn && !isIndexTriggerOn)
        {
            // Joint 3 (팔꿈치)
            currentAngles[2] += inputY * moveStep;
        }
        else
        {
            // Joint 2 (어깨)
            // 참고: 어깨는 위로 올리는 게 -(마이너스) 일 수도 있음. 모델 확인 필요.
            currentAngles[1] += inputY * moveStep; 
        }

        // 3. 각도 제한 (Clamp) 및 적용
        ApplyRotation(joint1, 0, axisJ1, limitJ1);
        ApplyRotation(joint2, 1, axisJ2, limitJ2);
        ApplyRotation(joint3, 2, axisJ3, limitJ3);
        ApplyRotation(joint4, 3, axisJ4, limitJ4);

        // 4. 그리퍼 처리 (선형 보간)
        // indexTrigger (0.0 ~ 1.0) 값을 OpenAngle ~ CloseAngle 사이로 변환
        if (gripperLeft && gripperRight)
        {
            float targetGripperAngle = Mathf.Lerp(gripperOpenAngle, gripperCloseAngle, indexTrigger);
            
            // 그리퍼는 보통 왼쪽/오른쪽이 반대로 움직임 (모델 축 확인 필수)
            gripperLeft.localEulerAngles = new Vector3(0, 0, targetGripperAngle); 
            gripperRight.localEulerAngles = new Vector3(0, 0, targetGripperAngle); // 축이 다르면 -targetGripperAngle
        }
    }

    // 각도 제한 후 회전 적용 함수
    void ApplyRotation(Transform joint, int index, Vector3 axis, Vector2 limit)
    {
        if (joint == null) return;

        // 각도 제한
        currentAngles[index] = Mathf.Clamp(currentAngles[index], limit.x, limit.y);

        // 로컬 회전 적용 (쿼터니언 변환)
        joint.localRotation = Quaternion.AngleAxis(currentAngles[index], axis);
    }

    // 현재 각도를 -180 ~ 180 형태로 가져오는 헬퍼 함수
    float GetInspectorRotation(Transform t, Vector3 axis)
    {
        float angle = 0;
        if (axis == Vector3.up) angle = t.localEulerAngles.y;
        else if (axis == Vector3.right) angle = t.localEulerAngles.x;
        else if (axis == Vector3.forward) angle = t.localEulerAngles.z;

        return (angle > 180) ? angle - 360 : angle;
    }
}