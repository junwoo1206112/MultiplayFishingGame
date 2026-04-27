using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace MultiplayFishing.Gameplay
{
    [DefaultExecutionOrder(100)]
    public class FishingCameraFollow : MonoBehaviour
    {
        [Header("Orbit Settings")]
        [SerializeField] private float pitchSpeed = 3f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float defaultPitch = 20f;
        [SerializeField] private float cameraDistance = 8f;
        [SerializeField] private float lookAtHeight = 1.5f;

        private Camera mainCamera;
        private float pitch;
        private bool initialized;

        void Awake()
        {
            pitch = defaultPitch;
        }

        void LateUpdate()
        {
            if (NetworkClient.localPlayer == null) return;

            if (!initialized)
            {
                Initialize();
                if (!initialized) return;
            }

            Transform player = NetworkClient.localPlayer.transform;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                pitch -= Input.GetAxis("Mouse Y") * pitchSpeed;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            float yaw = player.eulerAngles.y;

            Vector3 lookAtPos = player.position + Vector3.up * lookAtHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = orbitRotation * new Vector3(0f, 0f, -cameraDistance);
            Vector3 targetPos = lookAtPos + offset;

            mainCamera.transform.position = targetPos;
            mainCamera.transform.rotation = Quaternion.LookRotation(lookAtPos - targetPos);
        }

        private void Initialize()
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;

            CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                brain.enabled = false;
            }

            CinemachineCamera[] vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var vcam in vcams)
            {
                vcam.enabled = false;
            }

            Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in allCameras)
            {
                if (cam != mainCamera)
                {
                    cam.enabled = false;
                }
            }

            pitch = defaultPitch;
            initialized = true;
        }
    }
}