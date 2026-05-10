using System.Collections.Generic;
using UnityEngine;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform contentParent;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        private IUserService userService;
        private IDataService dataService;
        private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

        private void Awake()
        {
            AutoBindReferences();
        }

        private void Start()
        {
            if (!DIContainer.TryResolve(out userService) || !DIContainer.TryResolve(out dataService))
            {
                Debug.LogWarning("[InventoryUI] UserService or DataService is not ready. Inventory UI will wait for services.");
                if (windowRoot != null) windowRoot.SetActive(false);
                return;
            }

            userService.OnDataChanged += RefreshList;
            
            if (windowRoot != null) windowRoot.SetActive(false);
            RefreshList();
        }

        private void AutoBindReferences()
        {
            if (windowRoot == null)
            {
                var panel = transform.Find("InventoryPanel");
                if (panel != null)
                    windowRoot = panel.gameObject;
            }

            if (contentParent == null)
            {
                var content = transform.Find("InventoryPanel/LeftContent/Scroll View/Viewport/Content");
                if (content != null)
                    contentParent = content;
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey))
            {
                ToggleWindow();
            }
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= RefreshList;
        }

        public void ToggleWindow()
        {
            if (windowRoot == null) return;
            
            bool nextState = !windowRoot.activeSelf;
            windowRoot.SetActive(nextState);

            if (nextState)
            {
                RefreshList();
                // 창이 열릴 때 마우스 커서 잠금 해제 (필요 시)
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// 일괄 판매 버튼 클릭 시 호출될 메서드
        /// </summary>
        public void OnSellAllClicked()
        {
            if (userService == null) return;

            userService.SellAllFish();
        }

        public void RefreshList()
        {
            if (userService == null || dataService == null) return;

            if (contentParent == null)
            {
                Debug.LogWarning("[InventoryUI] Content parent is not assigned.");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] Slot prefab is not assigned.");
                return;
            }

            // 1. 기존 슬롯 정리
            foreach (var slot in activeSlots)
            {
                Destroy(slot.gameObject);
            }
            activeSlots.Clear();

            // 2. 인벤토리 아이템 생성
            var userData = userService.UserData;
            if (userData == null || userData.inventory == null) return;

            var inventory = userData.inventory;
            foreach (var item in inventory)
            {
                GameObject obj = Instantiate(slotPrefab, contentParent);
                InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();
                
                if (slotUI != null)
                {
                    var fishInfo = dataService.GetFishData(item.fishId);
                    slotUI.Setup(item, fishInfo, userService);
                    activeSlots.Add(slotUI);
                }
            }
        }
    }
}
