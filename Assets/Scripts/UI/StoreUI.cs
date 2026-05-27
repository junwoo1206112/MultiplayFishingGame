using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.UI
{
    public class StoreUI : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private GameObject windowRoot;

        [Header("Gold Panel")]
        [SerializeField] private TMP_Text goldText;

        [Header("Inventory Section")]
        [SerializeField] private GameObject inventoryContent;
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private Transform inventoryParent;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        private IUserService userService;
        private IDataService dataService;
        private StoreSellHandler sellHandler;
        private List<InventorySlotUI> activeInventorySlots = new List<InventorySlotUI>();

        private readonly Color[] rankColors = new Color[]
        {
            new Color(0.976f, 0.890f, 0.725f),
            new Color(0.769f, 1.0f, 0.780f),
            new Color(0.780f, 0.945f, 1.0f),
            new Color(0.957f, 0.890f, 1.0f),
            new Color(1.0f, 0.843f, 0.0f),
        };

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();
            sellHandler = GetComponent<StoreSellHandler>();

            userService.OnDataChanged += RefreshGold;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseWindow);
            }

            if (windowRoot != null)
                windowRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= RefreshGold;

            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }

        private void RefreshGold()
        {
            if (goldText != null && userService != null)
                goldText.text = $"{userService.UserData.gold:N0}";
        }

        public void OpenWindow()
        {
            if (windowRoot == null) return;
            if (windowRoot.activeSelf) return;

            windowRoot.SetActive(true);
            RefreshGold();
            LoadInventoryItems();
            if (sellHandler != null)
                sellHandler.EnableSelling(activeInventorySlots);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseWindow()
        {
            if (windowRoot == null) return;
            if (!windowRoot.activeSelf) return;

            if (sellHandler != null)
                sellHandler.DisableSelling();
            windowRoot.SetActive(false);
            Cursor.visible = true;
        }

        private void LoadInventoryItems()
        {
            if (inventoryParent == null || inventorySlotPrefab == null) return;

            ClearInventorySlots();

            var inventory = userService.UserData.inventory;
            foreach (var item in inventory)
            {
                GameObject obj = Instantiate(inventorySlotPrefab, inventoryParent);
                InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();

                if (slotUI != null)
                {
                    var fishInfo = dataService.GetFishData(item.fishId);
                    slotUI.Setup(item, fishInfo, userService);
                    activeInventorySlots.Add(slotUI);

                    Transform buttonTransform = obj.transform.Find("Button");
                    if (buttonTransform != null)
                    {
                        Image backgroundImage = buttonTransform.GetComponent<Image>();
                        if (backgroundImage != null && fishInfo != null)
                        {
                            int starCount = FishDataSO.GetStarCount(fishInfo.rank);
                            int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                            backgroundImage.color = rankColors[rankIndex];
                        }
                    }
                }
            }
        }

        private void ClearInventorySlots()
        {
            foreach (var slot in activeInventorySlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            activeInventorySlots.Clear();
        }
    }
}
