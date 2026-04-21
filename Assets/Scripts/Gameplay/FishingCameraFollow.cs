using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// 시네머신의 모든 자동 기능을 차단하고, 수학적으로 3인칭 시점을 강제 고정합니다.
    /// </summary>
    public class FishingCameraFollow : MonoBehaviour
    {
        [Header("Simple 3rd Person Settings")]
        [SerializeField] private float distance = 8.0f;    // 뒤로 떨어진 거리
        [SerializeField] private float height = 5.0f;      // 기본 높이 (verticalAngle로 대체됨)
        [SerializeField] private float lookAtHeight = 1.5f; // 캐릭터의 어디를 쳐다볼지 (머리 높이)
        
        [Header("Vertical Rotation Settings")]
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float minVerticalAngle = -10f; // 위를 볼 때의 제한 (내려다보기)
        [SerializeField] private float maxVerticalAngle = 60f;  // 아래를 볼 때의 제한 (올려다보기)
        [SerializeField] private float defaultVerticalAngle = 20f; // 기본 내려다보는 각도

        private float currentVerticalAngle;
        private CinemachineCamera vcam;

        void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            currentVerticalAngle = defaultVerticalAngle;
            
            // 시네머신의 자동 기능을 모두 끕니다. (우리가 직접 제어하기 위함)
            if (vcam != null)
            {
                vcam.Follow = null;
                vcam.LookAt = null;
                
                // 기존 시네머신 추적 컴포넌트들을 모두 비활성화
                foreach (var comp in vcam.GetComponents<CinemachineComponentBase>())
                {
                    comp.enabled = false;
                }
            }
        }

        void LateUpdate()
        {
            // 1. 로컬 플레이어 확인
            if (NetworkClient.localPlayer == null) return;

            // 2. 가상 카메라 참조 확인 및 자동 할당
            if (vcam == null)
            {
                vcam = GetComponent<CinemachineCamera>();
                if (vcam == null)
                {
                    vcam = Object.FindFirstObjectByType<CinemachineCamera>();
                }
                
                if (vcam == null) return; 
            }

            Transform player = NetworkClient.localPlayer.transform;

            // 3. 마우스 입력 처리 (수직 회전)
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float mouseY = Input.GetAxis("Mouse Y");
                currentVerticalAngle -= mouseY * mouseSensitivity;
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            }

            // 4. 카메라 위치 계산
            // 캐릭터의 LookAt 포인트 설정
            Vector3 lookAtPos = player.position + (Vector3.up * lookAtHeight);
            
            // 플레이어의 방향(forward)을 기준으로 수직 각도를 적용한 뒤쪽 방향 계산
            // 플레이어의 right 축을 기준으로 회전시킵니다.
            Quaternion verticalRotation = Quaternion.AngleAxis(currentVerticalAngle, player.right);
            Vector3 backwardDirection = -player.forward;
            Vector3 rotatedBackward = verticalRotation * backwardDirection;
            
            // 최종 위치: LookAt 지점에서 회전된 방향으로 distance만큼 떨어진 곳
            Vector3 targetPos = lookAtPos + (rotatedBackward * distance);
            
            // 5. 카메라 위치 및 회전 강제 적용
            vcam.transform.position = targetPos;
            vcam.transform.LookAt(lookAtPos);

            // 6. 시네머신 브레인에게 위치 갱신 알림
            vcam.OnTargetObjectWarped(player, Vector3.zero);
        }
    }
}
