using UnityEngine;
using TMPro;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class PlayerNameDisplay : MonoBehaviour
    {
        [SerializeField] private FishingPlayer player;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Transform targetCamera;

        private void Awake()
        {
            if (player == null) player = GetComponentInParent<FishingPlayer>();
            if (nameText == null) nameText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (player != null)
            {
                player.OnPlayerNameChangedEvent += UpdateName;
                // 초기값 설정
                UpdateName(player.playerName);
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.OnPlayerNameChangedEvent -= UpdateName;
            }
        }

        private void Start()
        {
            if (targetCamera == null && Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
        }

        private void UpdateName(string newName)
        {
            if (nameText != null)
            {
                nameText.text = string.IsNullOrEmpty(newName) ? "Loading..." : newName;
            }
        }

        private void LateUpdate()
        {
            // 카메라를 바라보게 함 (Billboarding)
            if (targetCamera != null)
            {
                transform.LookAt(transform.position + targetCamera.forward);
            }
            else if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
        }
    }
}
