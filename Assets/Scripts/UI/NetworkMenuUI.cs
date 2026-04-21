using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MultiplayFishing.Network;

namespace MultiplayFishing.UI
{
    public class NetworkMenuUI : MonoBehaviour
    {
        [Header("Dependency")]
        [SerializeField] private FishingRoomManager manager;

        [Header("Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button copyIPButton;

        [Header("Input")]
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private TMP_InputField nameInput;

        [Header("Display")]
        [SerializeField] private GameObject offlineControlsRoot;
        [SerializeField] private GameObject onlineControlsRoot;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text connectionInfoText;

        private const string PlayerNameKey = "PlayerName";

        void Start()
        {
            if (manager == null) manager = FindAnyObjectByType<FishingRoomManager>();

            if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
            if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
            if (disconnectButton != null) disconnectButton.onClick.AddListener(OnDisconnectClicked);
            if (copyIPButton != null) copyIPButton.onClick.AddListener(OnCopyIPClicked);

            if (nameInput != null)
            {
                nameInput.text = PlayerPrefs.GetString(PlayerNameKey, $"낚시꾼 {Random.Range(100, 999)}");
                nameInput.onEndEdit.AddListener(SavePlayerName);
            }

            SetupUIPositions();
            Refresh();
        }

        private void SavePlayerName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                PlayerPrefs.SetString(PlayerNameKey, name.Trim());
                PlayerPrefs.Save();
            }
        }

        private void SetupUIPositions()
        {
            if (onlineControlsRoot != null)
            {
                RectTransform rect = onlineControlsRoot.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    rect.anchoredPosition = new Vector2(-20, -20);
                }
            }

            if (connectionInfoText != null)
            {
                connectionInfoText.alignment = TextAlignmentOptions.Center;
                RectTransform rect = connectionInfoText.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(-940, -20);
                    rect.sizeDelta = new Vector2(400, 50);
                }
            }
        }

        void OnEnable() { FishingRoomManager.NetworkStateChanged += Refresh; Refresh(); }
        void OnDisable() { FishingRoomManager.NetworkStateChanged -= Refresh; }

        void OnHostClicked() { if (manager != null) manager.StartHost(); }
        void OnJoinClicked()
        {
            if (manager == null) return;
            string addr = (addressInput != null && !string.IsNullOrWhiteSpace(addressInput.text)) ? addressInput.text.Trim() : "localhost";
            manager.networkAddress = addr;
            manager.StartClient();
        }

        void OnDisconnectClicked()
        {
            if (manager == null) return;
            if (NetworkServer.active && NetworkClient.isConnected) manager.StopHost();
            else if (NetworkClient.isConnected) manager.StopClient();
        }

        void OnCopyIPClicked() { GUIUtility.systemCopyBuffer = FishingRoomManager.GetLocalIPAddress(); }

        void Refresh()
        {
            if (manager == null) return;
            bool isOffline = manager.mode == NetworkManagerMode.Offline;
            if (offlineControlsRoot != null) offlineControlsRoot.SetActive(isOffline);
            if (onlineControlsRoot != null) onlineControlsRoot.SetActive(!isOffline);
            
            if (copyIPButton != null) copyIPButton.gameObject.SetActive(manager.mode == NetworkManagerMode.Host);

            if (statusText != null) statusText.text = isOffline ? "오프라인" : $"{manager.ModeText} 모드";
        }

        void Update()
        {
            // 실시간으로 씬 내의 플레이어 오브젝트 수를 세어서 표시
            if (manager != null && manager.mode != NetworkManagerMode.Offline && connectionInfoText != null)
            {
                connectionInfoText.text = $"[ 인원: {manager.ConnectedClientCount}/{manager.maxConnections} ]";
            }
        }
    }
}
