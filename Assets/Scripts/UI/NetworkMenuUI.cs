using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MultiplayFishing.Network;
using MultiplayFishing.Gameplay;

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

        private bool showConnectionInfo = true;

        private Canvas rootCanvas;
        private Transform searchRoot;

        void Awake()
        {
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            rootCanvas = GetComponentInParent<Canvas>();
            searchRoot = rootCanvas != null ? rootCanvas.transform : transform;

            FindReferences();

            if (nameInput != null)
            {
                nameInput.onEndEdit.AddListener(SavePlayerName);
            }
        }

        void Start()
        {
            EnsureManager();

            if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
            if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
            if (disconnectButton != null) disconnectButton.onClick.AddListener(OnDisconnectClicked);
            if (copyIPButton != null) copyIPButton.onClick.AddListener(OnCopyIPClicked);

            if (nameInput != null)
            {
                nameInput.text = PlayerPrefs.GetString(PlayerNameKey, $"낚시꾼 {UnityEngine.Random.Range(100, 999)}");
            }

            SetupUIPositions();
            Refresh();
        }

        private void FindReferences()
        {
            Transform root = searchRoot != null ? searchRoot : transform;

            if (nameInput == null)
            {
                nameInput = FindInactiveComponentInChildren<TMP_InputField>(root, "NameInputField");
                if (nameInput == null)
                {
                    TMP_InputField[] inputs = root.GetComponentsInChildren<TMP_InputField>(true);
                    foreach (var input in inputs)
                    {
                        if (input == addressInput) continue;
                        nameInput = input;
                        break;
                    }
                }
            }

            if (offlineControlsRoot == null)
            {
                Transform off = root.Find("Panel_Offline");
                if (off != null) offlineControlsRoot = off.gameObject;
            }

            if (onlineControlsRoot == null)
            {
                Transform on = root.Find("Panel_Online");
                if (on != null) onlineControlsRoot = on.gameObject;
            }
        }

        private T FindInactiveComponentInChildren<T>(Transform root, string name) where T : Component
        {
            Transform found = root.Find(name);
            if (found != null) return found.GetComponent<T>();
            foreach (Transform child in root)
            {
                T result = FindInactiveComponentInChildren<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void EnsureManager()
        {
            if (manager == null)
            {
                manager = FindAnyObjectByType<FishingRoomManager>();
            }
        }

        private void SavePlayerName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                PlayerPrefs.SetString(PlayerNameKey, name.Trim());
                PlayerPrefs.Save();
            }
        }

        private void ForceSaveName()
        {
            if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
            {
                PlayerPrefs.SetString(PlayerNameKey, nameInput.text.Trim());
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

            if (nameInput != null)
            {
                RectTransform rect = nameInput.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(-120, -170);
                    rect.sizeDelta = new Vector2(300, 50);
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if (offlineControlsRoot != null) offlineControlsRoot.SetActive(visible);
            if (onlineControlsRoot != null) onlineControlsRoot.SetActive(visible);
        }

        void OnEnable()
        {
            FishingRoomManager.NetworkStateChanged += Refresh;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureManager();
            Refresh();
        }

        void OnDisable()
        {
            FishingRoomManager.NetworkStateChanged -= Refresh;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureManager();
            Refresh();
        }

        void OnHostClicked()
        {
            ForceSaveName();
            if (manager == null) return;
            string roomName = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text)
                ? $"{nameInput.text.Trim()}"
                : $"낚시방";
            manager.CreateRoom(roomName);
        }

        void OnJoinClicked()
        {
            ForceSaveName();
            if (manager == null) return;
            string lobbyId = (addressInput != null && !string.IsNullOrWhiteSpace(addressInput.text))
                ? addressInput.text.Trim()
                : "";
            manager.JoinRoom(lobbyId);
        }

        void OnDisconnectClicked()
        {
            if (manager == null) return;
            try
            {
                if (NetworkServer.active && NetworkClient.isConnected) manager.StopHost();
                else if (NetworkClient.active) manager.StopClient();
                else if (NetworkServer.active) manager.StopServer();
            }
            catch (NullReferenceException)
            {
                if (NetworkServer.active) NetworkServer.Shutdown();
                if (NetworkClient.active) NetworkClient.Disconnect();
            }
        }

        void OnCopyIPClicked()
        {
            string joinCode = manager.CurrentJoinCode;
            if (!string.IsNullOrEmpty(joinCode))
                GUIUtility.systemCopyBuffer = joinCode;
        }

        void Refresh()
        {
            if (manager == null) return;
            
            bool isOffline = !NetworkServer.active && !NetworkClient.active;
            bool isHost = NetworkServer.active && NetworkClient.active;
            bool isWaiting = isHost && !manager.IsRelayReady;
            
            if (offlineControlsRoot != null) 
                offlineControlsRoot.SetActive(isOffline);
            
            if (onlineControlsRoot != null) 
                onlineControlsRoot.SetActive(!isOffline);
            
            if (copyIPButton != null) copyIPButton.gameObject.SetActive(isHost && !isWaiting);

            if (statusText != null)
            {
                if (isOffline)
                    statusText.text = "오프라인";
                else if (isWaiting)
                    statusText.text = "릴레이 서버 연결 중...";
                else
                    statusText.text = $"{manager.ModeText} 모드";
            }
        }

        void Update()
        {
            EnsureManager();
            bool isOffline = !NetworkServer.active && !NetworkClient.active;

            if (offlineControlsRoot != null)
                offlineControlsRoot.SetActive(isOffline);
            if (onlineControlsRoot != null)
                onlineControlsRoot.SetActive(!isOffline);

            if (manager == null) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                showConnectionInfo = !showConnectionInfo;
            }

            bool isHost = NetworkServer.active && NetworkClient.active;
            bool isWaiting = isHost && !manager.IsRelayReady;
            string joinCode = manager.CurrentJoinCode;

            if (copyIPButton != null)
                copyIPButton.gameObject.SetActive(!string.IsNullOrEmpty(joinCode));

            if (statusText != null)
            {
                if (isOffline)
                    statusText.text = "오프라인";
                else if (isWaiting)
                    statusText.text = "릴레이 서버 연결 중...";
                else
                    statusText.text = $"{manager.ModeText} 모드";
            }

            if (connectionInfoText != null)
            {
                if (!isOffline && showConnectionInfo)
                {
                    int playerCount = isHost
                        ? NetworkServer.connections.Count
                        : FindObjectsByType<FishingPlayer>(FindObjectsSortMode.None).Length;
                    string text = $"[ 인원: {playerCount}/{manager.maxConnections} ]";
                    if (isHost && !string.IsNullOrEmpty(joinCode))
                    {
                        text += $"\n참가 코드: {joinCode}";
                    }
                    connectionInfoText.text = text;
                }
                else
                {
                    connectionInfoText.text = "";
                }
            }
        }
    }
}
