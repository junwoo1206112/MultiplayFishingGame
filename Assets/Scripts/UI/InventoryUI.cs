using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
        [SerializeField] private Button exitButton;

        private IUserService userService;
        private IDataService dataService;
        private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            userService.OnDataChanged += RefreshList;
            
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }

            if (windowRoot != null) windowRoot.SetActive(false);
            RefreshList();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
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
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OnSellAllClicked()
        {
            userService.SellAllFish();
        }

        public void OnExitClicked()
        {
            ToggleWindow();
        }

        public void RefreshList()
        {
            if (contentParent == null || slotPrefab == null) return;

            foreach (var slot in activeSlots)
            {
                Destroy(slot.gameObject);
            }
            activeSlots.Clear();

            var inventory = userService.UserData.inventory;
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