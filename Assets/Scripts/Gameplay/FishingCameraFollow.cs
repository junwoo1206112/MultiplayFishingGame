using Mirror;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MultiplayFishing.Gameplay
{
    [DefaultExecutionOrder(100)]
    public class FishingCameraFollow : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera playerVcam;
        [SerializeField] private string playerVcamName = "PlayerVcam";
        [SerializeField] private string cameraTargetName = "CameraTarget";
        [SerializeField] private bool useManualCamera = true;

        [Header("Third Person Follow")]
        [FormerlySerializedAs("distance")]
        [SerializeField] private float cameraDistance = 8f;
        [FormerlySerializedAs("height")]
        [SerializeField] private float shoulderHeight = 2f;
        [SerializeField] private float verticalArmLength = 1f;
        [SerializeField] private float lookAtHeight = 1.5f;
        [SerializeField] private Vector3 fallbackTargetOffset = new Vector3(0f, 1.6f, 0.1f);
        [SerializeField] private float pitch = 18f;
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float rotationSharpness = 20f;
        [SerializeField] private float keyboardYawSpeed = 120f;
        [SerializeField] private float mouseYawSpeed = 0.12f;

        [Header("Scene Camera")]
        [SerializeField] private bool disableExtraCameras = true;

        private CinemachineCamera activeVcam;
        private Camera mainCamera;
        private Transform currentTarget;
        private bool warnedMissingVcam;
        private bool hasManualYaw;
        private bool manualCameraConfigured;
        private float manualYaw;

        private void LateUpdate()
        {
            if (!ShouldConfigureCamera()) return;
            if (NetworkClient.localPlayer == null) return;

            Transform localPlayer = NetworkClient.localPlayer.transform;
            Transform cameraTarget = ResolveCameraTarget(localPlayer);

            if (useManualCamera)
            {
                ConfigureManualCamera(localPlayer, cameraTarget);
                currentTarget = cameraTarget;
                return;
            }

            if (currentTarget == cameraTarget && activeVcam != null) return;
            ConfigureCinemachine(cameraTarget);
        }

        private bool ShouldConfigureCamera()
        {
            NetworkIdentity networkIdentity = GetComponentInParent<NetworkIdentity>();
            return networkIdentity == null || networkIdentity.isLocalPlayer;
        }

        private void ConfigureCinemachine(Transform cameraTarget)
        {
            EnsureMainCamera();

            activeVcam = ResolvePlayerVcam();
            if (activeVcam == null)
            {
                if (!warnedMissingVcam)
                {
                    Debug.LogWarning("[FishingCameraFollow] PlayerVcam could not be found in the scene.");
                    warnedMissingVcam = true;
                }
                return;
            }

            activeVcam.gameObject.SetActive(true);
            activeVcam.enabled = true;

            var target = activeVcam.Target;
            target.TrackingTarget = cameraTarget;
            target.LookAtTarget = cameraTarget;
            activeVcam.Target = target;

            ConfigureThirdPersonFollow(activeVcam);
            ConfigureRotationComposer(activeVcam);

            currentTarget = cameraTarget;
            Debug.Log("[FishingCameraFollow] PlayerVcam is now following the local player.");
        }

        private void ConfigureManualCamera(Transform localPlayer, Transform cameraTarget)
        {
            EnsureMainCamera();
            if (mainCamera == null) return;

            if (!manualCameraConfigured)
            {
                DisableCinemachine();
                DestroyMirrorControllerUI();
                manualCameraConfigured = true;
            }

            if (!hasManualYaw)
            {
                manualYaw = mainCamera.transform.eulerAngles.y;
                hasManualYaw = true;
            }

            // Track player's facing direction so camera follows Q/E rotation
            manualYaw = Mathf.LerpAngle(manualYaw, cameraTarget.eulerAngles.y, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));

            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                manualYaw += Mouse.current.delta.ReadValue().x * mouseYawSpeed;
            }

            Vector3 lookAtPosition = cameraTarget.position + Vector3.up * lookAtHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, manualYaw, 0f);
            Vector3 desiredPosition = lookAtPosition + orbitRotation * new Vector3(0f, shoulderHeight * 0.15f, -cameraDistance);
            Quaternion desiredRotation = Quaternion.LookRotation(lookAtPosition - desiredPosition, Vector3.up);

            float followT = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            float rotationT = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, followT);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, desiredRotation, rotationT);
        }

        private Transform ResolveCameraTarget(Transform localPlayer)
        {
            Transform cameraTarget = localPlayer.Find(cameraTargetName);
            if (cameraTarget != null) return cameraTarget;

            GameObject targetObject = new GameObject(cameraTargetName);
            Transform targetTransform = targetObject.transform;
            targetTransform.SetParent(localPlayer, false);
            targetTransform.localPosition = fallbackTargetOffset;
            targetTransform.localRotation = Quaternion.identity;
            return targetTransform;
        }

        private void EnsureMainCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;

            CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                brain.enabled = !useManualCamera;
            }

            if (!disableExtraCameras) return;

            Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in allCameras)
            {
                if (cam != mainCamera)
                {
                    cam.enabled = false;
                }
            }
        }

        private void DisableCinemachine()
        {
            CinemachineCamera[] vcams = Object.FindObjectsByType<CinemachineCamera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (CinemachineCamera vcam in vcams)
            {
                vcam.enabled = false;
            }
        }

        private void DestroyMirrorControllerUI()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "PlayerControllerUI(Clone)")
                {
                    Destroy(canvas.gameObject);
                }
            }
        }

        private CinemachineCamera ResolvePlayerVcam()
        {
            if (playerVcam != null) return playerVcam;

            playerVcam = GetComponent<CinemachineCamera>();
            if (playerVcam != null) return playerVcam;

            CinemachineCamera[] vcams = Object.FindObjectsByType<CinemachineCamera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (CinemachineCamera vcam in vcams)
            {
                if (vcam.name == playerVcamName)
                {
                    playerVcam = vcam;
                    return playerVcam;
                }
            }

            if (vcams.Length > 0)
            {
                playerVcam = vcams[0];
            }

            return playerVcam;
        }

        private void ConfigureThirdPersonFollow(CinemachineCamera vcam)
        {
            CinemachineThirdPersonFollow follow = vcam.GetComponent<CinemachineThirdPersonFollow>();
            if (follow == null) return;

            Vector3 shoulderOffset = follow.ShoulderOffset;
            shoulderOffset.y = shoulderHeight;
            follow.ShoulderOffset = shoulderOffset;
            follow.VerticalArmLength = verticalArmLength;
            follow.CameraDistance = cameraDistance;
        }

        private void ConfigureRotationComposer(CinemachineCamera vcam)
        {
            CinemachineRotationComposer composer = vcam.GetComponent<CinemachineRotationComposer>();
            if (composer == null) return;

            composer.TargetOffset = new Vector3(0f, lookAtHeight, 0f);
        }
    }
}
