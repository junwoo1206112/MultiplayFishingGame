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
        private Canvas parentCanvas;

        private void Awake()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            if (player == null)
            {
                player = GetComponentInParent<FishingPlayer>();
            }

            if (nameText == null)
            {
                nameText = GetComponentInChildren<TMP_Text>();
            }

            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void OnEnable()
        {
            FindReferences();

            if (player != null)
            {
                player.OnPlayerNameChangedEvent += UpdateName;
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
            TryAssignCamera();
        }

        private void TryAssignCamera()
        {
            if (targetCamera == null && Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace && parentCanvas.worldCamera == null)
            {
                if (Camera.main != null)
                {
                    parentCanvas.worldCamera = Camera.main;
                }
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
            TryAssignCamera();
            if (targetCamera != null)
            {
                transform.LookAt(transform.position + targetCamera.forward);
            }
        }
    }
}
