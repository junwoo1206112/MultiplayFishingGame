using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MultiplayFishing.Network;

namespace MultiplayFishing.UI
{
    public class NetworkMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button disconnectButton;

        [Header("Input")]
        [SerializeField] private TMP_InputField addressInput;

        [Header("Display")]
        [SerializeField] private GameObject offlineControlsRoot;
        [SerializeField] private GameObject onlineControlsRoot;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text connectionInfoText;
        [SerializeField] private string defaultAddress = "127.0.0.1";

        FishingNetworkManager manager;

        void Awake()
        {
            manager = FindFirstObjectByType<FishingNetworkManager>();
        }

        void OnEnable()
        {
            if (hostButton != null)
                hostButton.onClick.AddListener(OnHostClicked);

            if (joinButton != null)
                joinButton.onClick.AddListener(OnJoinClicked);

            if (disconnectButton != null)
                disconnectButton.onClick.AddListener(OnDisconnectClicked);

            FishingNetworkManager.NetworkStateChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (hostButton != null)
                hostButton.onClick.RemoveListener(OnHostClicked);

            if (joinButton != null)
                joinButton.onClick.RemoveListener(OnJoinClicked);

            if (disconnectButton != null)
                disconnectButton.onClick.RemoveListener(OnDisconnectClicked);

            FishingNetworkManager.NetworkStateChanged -= Refresh;
        }

        void Update()
        {
            Refresh();
        }

        #region Button Handlers

        void OnHostClicked()
        {
            if (!CanStartNetwork()) return;
            manager.StartHost();
        }

        void OnJoinClicked()
        {
            if (!CanStartNetwork()) return;
            manager.networkAddress = GetAddress();
            manager.StartClient();
        }

        void OnDisconnectClicked()
        {
            if (manager == null) return;

            switch (manager.mode)
            {
                case NetworkManagerMode.Host:
                    manager.StopHost();
                    break;
                case NetworkManagerMode.ClientOnly:
                    manager.StopClient();
                    break;
                case NetworkManagerMode.ServerOnly:
                    manager.StopServer();
                    break;
            }
        }

        #endregion

        #region Helpers

        string GetAddress()
        {
            if (addressInput != null && !string.IsNullOrWhiteSpace(addressInput.text))
                return addressInput.text.Trim();
            return defaultAddress;
        }

        bool CanStartNetwork()
        {
            return manager != null && manager.mode == NetworkManagerMode.Offline;
        }

        #endregion

        #region UI Refresh

        void Refresh()
        {
            if (manager == null) return;

            bool offline = manager.mode == NetworkManagerMode.Offline;
            bool canStart = offline;

            if (offlineControlsRoot != null)
                offlineControlsRoot.SetActive(offline);

            if (onlineControlsRoot != null)
                onlineControlsRoot.SetActive(!offline);

            if (hostButton != null)
                hostButton.interactable = canStart;

            if (joinButton != null)
                joinButton.interactable = canStart;

            if (addressInput != null)
                addressInput.interactable = canStart;

            if (disconnectButton != null)
                disconnectButton.interactable = !offline;

            UpdateStatusText();
            UpdateConnectionInfo();
        }

        void UpdateStatusText()
        {
            if (statusText == null) return;

            if (manager.mode == NetworkManagerMode.Offline)
            {
                statusText.text = "오프라인";
            }
            else if (NetworkClient.active && !NetworkClient.isConnected)
            {
                statusText.text = $"{manager.networkAddress}에 연결 중...";
            }
            else
            {
                statusText.text = $"{manager.ModeText} | {Transport.active}";
            }
        }

        void UpdateConnectionInfo()
        {
            if (connectionInfoText == null) return;

            if (manager.mode == NetworkManagerMode.Offline)
            {
                connectionInfoText.text = "";
                return;
            }

            if (manager.mode == NetworkManagerMode.Host)
            {
                string localIP = FishingNetworkManager.GetLocalIPAddress();
                connectionInfoText.text = $"내 IP: {localIP}\n다른 플레이어가 이 IP로 접속 가능\n접속 인원: {manager.ConnectedClientCount}/{manager.MaxPlayers}";
            }
            else if (manager.mode == NetworkManagerMode.ClientOnly)
            {
                if (NetworkClient.isConnected)
                    connectionInfoText.text = $"{manager.networkAddress}에 연결됨\n접속 인원: {manager.ConnectedClientCount}/{manager.MaxPlayers}";
                else
                    connectionInfoText.text = "연결 중...";
            }
            else if (manager.mode == NetworkManagerMode.ServerOnly)
            {
                string localIP = FishingNetworkManager.GetLocalIPAddress();
                connectionInfoText.text = $"서버 IP: {localIP}\n접속 인원: {manager.ConnectedClientCount}/{manager.MaxPlayers}";
            }
        }

        #endregion
    }
}