using Mirror;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform fallbackTarget;

        [Header("Third Person Camera")]
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float lookAtHeight = 1.5f;
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float minPitch = -15f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float defaultPitch = 20f;
        [SerializeField] private bool lockCursorOnPlay = true;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            Transform target = GetTarget();
            yaw = target != null ? target.eulerAngles.y : transform.eulerAngles.y;
            pitch = defaultPitch;

            if (lockCursorOnPlay)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void LateUpdate()
        {
            Transform target = GetTarget();
            if (target == null) return;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 lookAtPosition = target.position + (Vector3.up * lookAtHeight);
            Vector3 cameraPosition = lookAtPosition + (cameraRotation * Vector3.back * distance);

            transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            transform.LookAt(lookAtPosition);
        }

        private Transform GetTarget()
        {
            if (NetworkClient.localPlayer != null)
            {
                return NetworkClient.localPlayer.transform;
            }

            return fallbackTarget;
        }
    }
}
