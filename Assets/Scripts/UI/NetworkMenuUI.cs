using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

        private Canvas rootCanvas;
        private Transform searchRoot;

        void Awake()
        {
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
                nameInput.text = PlayerPrefs.GetString(PlayerNameKey, $"낚시꾼 {Random.Range(100, 999)}");
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

        void OnHostClicked() { ForceSaveName(); if (manager != null) manager.StartHost(); }
        void OnJoinClicked()
        {
            ForceSaveName();
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
            
            if (offlineControlsRoot != null) 
                offlineControlsRoot.SetActive(isOffline);
            
            if (onlineControlsRoot != null) 
                onlineControlsRoot.SetActive(!isOffline);
            
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
