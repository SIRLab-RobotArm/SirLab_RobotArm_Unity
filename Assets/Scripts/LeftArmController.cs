// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;

// public class LeftArmController : MonoBehaviour
// {

//     // ArticulationBody 리스트로 관절들을 한 번에 관리합니다.
//     // 인스펙터에서 순서대로 드래그 앤 드롭으로 할당합니다.
//     public List<ArticulationBody> joints;

//     // constant variable for PrimaryHandTrigger input value
//     private const float GRIP_THRESHOLD = 0.6f;
//     private const float SPEED_MULTIPLIER = 10.0f;
//     private const float STIFFNESS = 1000.0f;
//     private const float DAMPING = 100.0f;
//     private const float FORCELIMIT = 100.0f;


//     private float[] jointPositions;
//     private Vector3 lastControllerPosition;

//     void Awake()
//     {
//         Debug.Log("🎮 LeftArmController Awaking!");
//         if (joints == null || joints.Count == 0)
//         {
//             Debug.LogError("관절(Joints)이 할당되지 않았습니다!");
//             this.enabled = false;
//             return;
//         }

//         // 물리 프로퍼티 설정은 Awake에서!
//         InitializeAllJoints();
//     }

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     IEnumerator Start()
//     {
//         Debug.Log("🎮 LeftArmController starts!");
//         yield return null;
//         InitializeAllJoints();
//         jointPositions = new float[joints.Count];
//         lastControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
//     }

//     void InitializeAllJoints()
//     {
//         foreach (var joint in joints)
//         {
//             SetDriveProperties(joint, STIFFNESS, DAMPING, FORCELIMIT);
//         }
//     }

//     void SetDriveProperties(ArticulationBody joint, float stiffness, float damping, float forceLimit)
//     {
//         if (joint == null) return;
//         var drive = joint.xDrive;
//         drive.stiffness = stiffness;
//         drive.damping = damping;
//         drive.forceLimit = forceLimit;
//         joint.xDrive = drive;
//     }

//     // Update is called once per frame
//     // 입력 처리는 Update에서 수행하여 놓치는 입력이 없도록 합니다.
//     void Update()
//     {
//         HandleControllerInput();
//     }

//     // 물리 업데이트는 FixedUpdate에서 수행하여 안정성을 높입니다.
//     void FixedUpdate()
//     {
//         ApplyJointPositions();
//     }

//     void HandleControllerInput()
//     {
//         Vector3 currentControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
//         float handTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

//         if (handTriggerValue > GRIP_THRESHOLD)
//         {
//             Vector3 deltaPosition = currentControllerPosition - lastControllerPosition;

//             // 로직은 그대로 유지 (더 직관적인 제어를 원한다면 IK 도입 필요)
//             if (joints.Count > 0) jointPositions[0] += deltaPosition.x * -SPEED_MULTIPLIER; // 좌우
//             if (joints.Count > 1) jointPositions[1] += deltaPosition.z * SPEED_MULTIPLIER;  // 앞뒤
//             if (joints.Count > 2) jointPositions[2] += deltaPosition.y * -SPEED_MULTIPLIER; // 위아래
//         }

//         lastControllerPosition = currentControllerPosition;
//     }

//     void ApplyJointPositions()
//     {
//         for (int i = 0; i < joints.Count; i++)
//         {
//             if (joints[i] != null)
//             {
//                 var drive = joints[i].xDrive;
//                 drive.target = jointPositions[i] * Mathf.Rad2Deg; // Radian -> Degree 변환
//                 joints[i].xDrive = drive;
//             }
//         }
//     }
// }


// using UnityEngine;
// using System.Collections.Generic;

// public class LeftArmController : MonoBehaviour
// {
//     [Header("로봇팔 링크 (link2~link5 순서로 할당)")]
//     public ArticulationBody[] armJoints;  // 4개 관절 (어깨좌우, 어깨앞뒤, 팔꿈치, 손목)
//     public ArticulationBody gripperJoint; // 그리퍼 제어용

//     [Header("IK 타겟 (컨트롤러가 이걸 움직임)")]
//     public Transform ikTarget;  // 로봇팔이 따라가야 할 최종 목표(End Effector)를 나타내는 오브젝트입니다. 이 오브젝트의 위치와 회전은 VR 컨트롤러의 입력에 따라 실시간으로 업데이트됩니다.

//     [Header("컨트롤러 설정")]
//     public OVRInput.Controller controller = OVRInput.Controller.LTouch;

//     [Header("IK 설정")]
//     public int ikIterations = 10;   // IK 계산을 최대 몇 번 반복할지 설정합니다. 반복 횟수가 많을수록 더 정확해지지만 성능이 저하될 수 있습니다.
//     public float ikThreshold = 0.01f;   // 목표 지점과 현재 끝점(End Effector) 사이의 거리가 이 값보다 작아지면 IK 계산을 중단합니다.
//     public float moveSpeed = 8.0f;
//     public float rotateSpeed = 8.0f;

//     // 그리퍼 각도 설정
//     private const float GRIPPER_OPEN = 0f;
//     private const float GRIPPER_CLOSE = 45f;

//     void Start()
//     {
//         Debug.Log("🎮 LeftArmController (IK version) started!");
//     }

//     void Update()
//     {
//         if (ikTarget == null) return;

//         // 1️⃣ 컨트롤러 입력으로 IK 타겟 위치/회전 갱신
//         Vector3 targetPos = OVRInput.GetLocalControllerPosition(controller);
//         Quaternion targetRot = OVRInput.GetLocalControllerRotation(controller);
//         ikTarget.position = Vector3.Lerp(ikTarget.position, targetPos, Time.deltaTime * moveSpeed);
//         ikTarget.rotation = Quaternion.Slerp(ikTarget.rotation, targetRot, Time.deltaTime * rotateSpeed);

//         // 2️⃣ 그리퍼 제어
//         HandleGripper();

//         // 3️⃣ 로봇팔 IK 계산
//         SolveIK_CCD();
//     }

//     void HandleGripper()
//     {
//         if (gripperJoint == null) return;
//         float gripValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
//         float targetAngle = Mathf.Lerp(GRIPPER_OPEN, GRIPPER_CLOSE, gripValue);
//         var drive = gripperJoint.xDrive;
//         drive.target = targetAngle;
//         gripperJoint.xDrive = drive;
//     }

//     void SolveIK_CCD()
//     {
//         if (armJoints == null || armJoints.Length < 1) return;

//         // End Effector는 마지막 관절의 자식으로 간주하거나, 마지막 관절 자체를 사용합니다.
//         // Tip 오브젝트를 따로 두는 것이 더 정확할 수 있습니다. 여기서는 마지막 관절을 사용합니다.
//         Transform endEffector = armJoints[armJoints.Length - 1].transform;
//         Vector3 targetPosition = ikTarget.position;

//         // 목표 지점이 이미 도달 범위 내에 있는지 확인
//         float totalLength = 0;
//         for (int i = 0; i < armJoints.Length; i++)
//         {
//             if (i > 0) totalLength += Vector3.Distance(armJoints[i].transform.position, armJoints[i - 1].transform.position);
//         }
//         float distanceToTarget = Vector3.Distance(armJoints[0].transform.position, targetPosition);
//         if (distanceToTarget > totalLength)
//         {
//             //Debug.LogWarning("IK Target is out of reach!");
//             // return; // 도달 불가능할 때 계산을 스킵하려면 주석 해제
//         }


//         for (int iter = 0; iter < ikIterations; iter++)
//         {
//             // 종료 조건: 목표 지점에 충분히 가까워지면 반복 중단
//             if (Vector3.Distance(endEffector.position, targetPosition) < ikThreshold)
//             {
//                 break;
//             }

//             // 손목부터 어깨 순서로 역방향 계산
//             for (int i = armJoints.Length - 1; i >= 0; i--)
//             {
//                 ArticulationBody currentJoint = armJoints[i];
//                 Transform jointTransform = currentJoint.transform;

//                 // 1. 회전축 설정 (모든 관절이 Local X축 기준)
//                 Vector3 rotationAxis = jointTransform.right;

//                 // 2. 벡터들을 관절의 회전 평면에 투영
//                 Vector3 toEndEffector = endEffector.position - jointTransform.position;
//                 Vector3 toTarget = targetPosition - jointTransform.position;
//                 Vector3 toEndEffectorProjected = Vector3.ProjectOnPlane(toEndEffector, rotationAxis);
//                 Vector3 toTargetProjected = Vector3.ProjectOnPlane(toTarget, rotationAxis);

//                 // 3. 평면상에서 두 벡터 사이의 정확한 회전 각도 계산 (SignedAngle 사용)
//                 float angleDelta = Vector3.SignedAngle(toEndEffectorProjected, toTargetProjected, rotationAxis);

//                 // 4. 현재 관절 각도를 가져와서 새로운 목표 각도 설정 (+=가 아닌 =)
//                 // jointPosition은 라디안 단위이므로 각도로 변환
//                 float currentAngle = currentJoint.jointPosition[0] * Mathf.Rad2Deg;
//                 float newTargetAngle = currentAngle + angleDelta;

//                 // 5. ArticulationBody에 설정된 Limit 값 존중
//                 ArticulationDrive drive = currentJoint.xDrive;
//                 newTargetAngle = Mathf.Clamp(newTargetAngle, drive.lowerLimit, drive.upperLimit);

//                 drive.target = newTargetAngle;
//                 currentJoint.xDrive = drive;
//             }
//         }
//     }
// }


using UnityEngine;

public class LeftArmController : MonoBehaviour
{
    [Header("Articulation Joints (link2 ~ link5 순서대로 할당)")]
    public ArticulationBody[] armJoints;

    [Header("Rig에서 움직이는 동일한 Transform들 (IK 결과)")]
    public Transform[] rigJoints;

    [Header("회전 반영 세기 (부드럽게 보간)")]
    public float syncSpeed = 10f;

    // 로컬 기준 회전축 (ArticulationBody가 X축만 회전한다고 가정)
    private Vector3 localAxis = Vector3.right;

    void Update()
    {
        if (armJoints == null || rigJoints == null) return;
        if (armJoints.Length != rigJoints.Length)
        {
            Debug.LogWarning("⚠️ armJoints와 rigJoints 배열 길이가 다릅니다!");
            return;
        }

        // 각 링크별로 Rig 회전값을 ArticulationBody로 반영
        for (int i = 0; i < armJoints.Length; i++)
        {
            var ab = armJoints[i];
            var rigTransform = rigJoints[i];

            // 현재 ArticulationBody 회전 (local 기준)
            float currentAngle = ab.jointPosition[0] * Mathf.Rad2Deg;

            // Rig 회전에서 목표 각도 추출 (X축 회전만 사용)
            Quaternion localRot = Quaternion.Inverse(rigTransform.parent.rotation) * rigTransform.rotation;
            localRot.ToAngleAxis(out float angle, out Vector3 axis);

            // 축 방향 일치 여부 보정
            if (Vector3.Dot(axis, rigTransform.parent.TransformDirection(localAxis)) < 0)
                angle = -angle;

            // 목표 각도 보정 (범위 제한)
            float targetAngle = Mathf.Clamp(angle, ab.xDrive.lowerLimit, ab.xDrive.upperLimit);

            // 부드럽게 보간
            float newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * syncSpeed);

            // xDrive 갱신
            var drive = ab.xDrive;
            drive.target = newAngle;
            ab.xDrive = drive;
        }
    }
}
