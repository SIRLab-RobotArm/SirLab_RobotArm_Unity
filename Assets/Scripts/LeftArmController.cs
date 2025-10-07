using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LeftArmController : MonoBehaviour
{

    // ArticulationBody 리스트로 관절들을 한 번에 관리합니다.
    // 인스펙터에서 순서대로 드래그 앤 드롭으로 할당합니다.
    public List<ArticulationBody> joints;

    // constant variable for PrimaryHandTrigger input value
    private const float GRIP_THRESHOLD = 0.6f;
    private const float SPEED_MULTIPLIER = 10.0f;
    private const float STIFFNESS = 1000.0f;
    private const float DAMPING = 100.0f;
    private const float FORCELIMIT = 100.0f;


    private float[] jointPositions;
    private Vector3 lastControllerPosition;

    void Awake()
    {
        Debug.Log("🎮 LeftArmController Awaking!");
        if (joints == null || joints.Count == 0)
        {
            Debug.LogError("관절(Joints)이 할당되지 않았습니다!");
            this.enabled = false;
            return;
        }

        // 물리 프로퍼티 설정은 Awake에서!
        InitializeAllJoints();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        Debug.Log("🎮 LeftArmController starts!");
        yield return null;
        InitializeAllJoints();
        jointPositions = new float[joints.Count];
        lastControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
    }

    void InitializeAllJoints()
    {
        foreach (var joint in joints)
        {
            SetDriveProperties(joint, STIFFNESS, DAMPING, FORCELIMIT);
        }
    }

    void SetDriveProperties(ArticulationBody joint, float stiffness, float damping, float forceLimit)
    {
        if (joint == null) return;
        var drive = joint.xDrive;
        drive.stiffness = stiffness;
        drive.damping = damping;
        drive.forceLimit = forceLimit;
        joint.xDrive = drive;
    }

    // Update is called once per frame
    // 입력 처리는 Update에서 수행하여 놓치는 입력이 없도록 합니다.
    void Update()
    {
        HandleControllerInput();
    }

    // 물리 업데이트는 FixedUpdate에서 수행하여 안정성을 높입니다.
    void FixedUpdate()
    {
        ApplyJointPositions();
    }

    void HandleControllerInput()
    {
        Vector3 currentControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        float handTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

        if (handTriggerValue > GRIP_THRESHOLD)
        {
            Vector3 deltaPosition = currentControllerPosition - lastControllerPosition;

            // 로직은 그대로 유지 (더 직관적인 제어를 원한다면 IK 도입 필요)
            if (joints.Count > 0) jointPositions[0] += deltaPosition.x * -SPEED_MULTIPLIER; // 좌우
            if (joints.Count > 1) jointPositions[1] += deltaPosition.z * SPEED_MULTIPLIER;  // 앞뒤
            if (joints.Count > 2) jointPositions[2] += deltaPosition.y * -SPEED_MULTIPLIER; // 위아래
        }

        lastControllerPosition = currentControllerPosition;
    }

    void ApplyJointPositions()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i] != null)
            {
                var drive = joints[i].xDrive;
                drive.target = jointPositions[i] * Mathf.Rad2Deg; // Radian -> Degree 변환
                joints[i].xDrive = drive;
            }
        }
    }
}
