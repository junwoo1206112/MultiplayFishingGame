using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// 로컬 플레이어의 시네머신 카메라 연결 및 마우스 상하 시선 처리(Look Up/Down)를 담당합니다.
    /// </summary>
    public class FishingCameraFollow : NetworkBehaviour
    {
        [Header("Camera Target")]
        [Tooltip("카메라가 추적할 지점 (보통 플레이어 머리 위치의 빈 오브젝트)")]
        [SerializeField] private Transform cameraTarget;

        [Header("Rotation Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -40f; // 아래로 보는 제한
        [SerializeField] private float maxPitch = 60f;  // 위로 보는 제한

        private float _currentPitch = 0f;
        private CinemachineCamera _vcam;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // 씬의 시네머신 카메라 찾기
            _vcam = FindFirstObjectByType<CinemachineCamera>();

            if (_vcam != null)
            {
                Transform target = cameraTarget != null ? cameraTarget : transform;
                _vcam.Follow = target;
                _vcam.LookAt = target;
                
                Debug.Log($"[FishingCameraFollow] 시네머신 카메라 연결 완료: {target.name}");
            }

            // 마우스 커서 잠금
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            // 내 플레이어일 때만 시선 처리를 수행 (시점은 로컬에서만 중요)
            if (!isLocalPlayer) return;

            HandlePitchRotation();
        }

        /// <summary>
        /// 마우스 Y축 입력을 받아 시각적인 상하 회전을 처리합니다.
        /// </summary>
        private void HandlePitchRotation()
        {
            if (cameraTarget == null) return;

            // 마우스 Y축 이동값 읽기
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 현재 각도에서 마우스 이동값 반영 (마우스 올리면 고개 들기)
            _currentPitch -= mouseY;
            
            // 각도 제한 적용 (Clamping)
            _currentPitch = Mathf.Clamp(_currentPitch, minPitch, maxPitch);

            // 카메라 타겟에 회전 적용
            cameraTarget.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
