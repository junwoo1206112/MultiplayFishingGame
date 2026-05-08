using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MultiplayFishing.UI
{
    public class ConfirmDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialogRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private System.Action onConfirm;

        private void Start()
        {
            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            if (dialogRoot != null)
                dialogRoot.SetActive(false);
        }

        public void Show(string title, string message, System.Action onConfirm)
        {
            this.onConfirm = onConfirm;
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            if (dialogRoot != null) dialogRoot.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            onConfirm?.Invoke();
            Hide();
        }

        public void Hide()
        {
            if (dialogRoot != null) dialogRoot.SetActive(false);
            onConfirm = null;
        }
    }
}
