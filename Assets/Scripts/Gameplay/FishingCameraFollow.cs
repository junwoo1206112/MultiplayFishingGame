using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace MultiplayFishing.Gameplay
{
    public class FishingCameraFollow : MonoBehaviour
    {
        [Header("Orbit Settings")]
        [SerializeField] private float orbitSpeed = 3f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float defaultPitch = 20f;
        [SerializeField] private float cameraDistance = 8f;
        [SerializeField] private float lookAtHeight = 1.5f;

        private CinemachineCamera vcam;
        private float pitch;
        private float yaw;
        private bool targetSet;

        void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            pitch = defaultPitch;
            yaw = transform.eulerAngles.y;

            if (vcam != null)
            {
                var thirdPerson = vcam.GetComponent<CinemachineThirdPersonFollow>();
                if (thirdPerson != null) thirdPerson.enabled = false;

                var composer = vcam.GetComponent<CinemachineRotationComposer>();
                if (composer != null) composer.enabled = false;

                var decollider = vcam.GetComponent<CinemachineDecollider>();
                if (decollider != null) decollider.enabled = false;

                vcam.Target.TrackingTarget = null;
                vcam.Target.LookAtTarget = null;
            }
        }

        void LateUpdate()
        {
            if (NetworkClient.localPlayer == null) return;

            if (vcam == null)
            {
                vcam = Object.FindFirstObjectByType<CinemachineCamera>();
                if (vcam == null) return;
            }

            Transform player = NetworkClient.localPlayer.transform;

            if (!targetSet)
            {
                vcam.Target.TrackingTarget = player;
                vcam.Target.LookAtTarget = player;
                vcam.Priority.Enabled = true;
                vcam.Priority.Value = 10;
                targetSet = true;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += Input.GetAxis("Mouse X") * orbitSpeed;
                pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Vector3 lookAtPos = player.position + Vector3.up * lookAtHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = orbitRotation * new Vector3(0f, 0f, -cameraDistance);
            Vector3 targetPos = lookAtPos + offset;

            vcam.transform.position = targetPos;
            vcam.transform.rotation = Quaternion.LookRotation(lookAtPos - targetPos);
        }
    }
}
