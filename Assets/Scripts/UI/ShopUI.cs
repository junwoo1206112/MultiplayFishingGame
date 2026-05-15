using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Top Bar")]
        [SerializeField] private TMP_Text goldText;

        [Header("Category Tabs")]
        [SerializeField] private Button rodTabButton;
        [SerializeField] private Button baitTabButton;
        [SerializeField] private Button sellTabButton;
        [SerializeField] private GameObject rodTabHighlight;
        [SerializeField] private GameObject baitTabHighlight;
        [SerializeField] private GameObject sellTabHighlight;

        [Header("Item List")]
        [SerializeField] private Transform itemContentParent;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Detail Panel")]
        [SerializeField] private ShopDetailPanel detailPanel;

        [Header("Inventory Panel")]
        [SerializeField] private ShopInventoryPanel inventoryPanel;

        [Header("Confirm Dialog")]
        [SerializeField] private ConfirmDialog confirmDialog;

        private IUserService userService;
        private IDataService dataService;
        private List<ShopSlotUI> activeSlots = new List<ShopSlotUI>();

        private enum TabType { Rods, Baits, Sell }
        private TabType currentTab = TabType.Rods;

        private void ApplyDefaultLayout()
        {
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            RectTransform root = transform as RectTransform;
            if (root != null)
            {
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.pivot = new Vector2(0.5f, 0.5f);
                root.anchoredPosition = Vector2.zero;
                root.sizeDelta = Vector2.zero;
                root.localScale = Vector3.one;
            }

            RectTransform windowRect = windowRoot != null ? windowRoot.transform as RectTransform : null;
            if (windowRect == null) return;

            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            windowRect.sizeDelta = new Vector2(1400f, 800f);
            windowRect.localScale = Vector3.one;
        }

        private void Start()
        {
            ApplyDefaultLayout();

            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            userService.OnDataChanged += RefreshAll;

            if (rodTabButton != null)
                rodTabButton.onClick.AddListener(() => SwitchTab(TabType.Rods));
            if (baitTabButton != null)
                baitTabButton.onClick.AddListener(() => SwitchTab(TabType.Baits));
            if (sellTabButton != null)
                sellTabButton.onClick.AddListener(() => SwitchTab(TabType.Sell));

            if (windowRoot != null) windowRoot.SetActive(false);
            SwitchTab(TabType.Rods);
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
                userService.OnDataChanged -= RefreshAll;
        }

        public void ToggleWindow()
        {
            if (windowRoot == null) return;

            bool nextState = !windowRoot.activeSelf;
            windowRoot.SetActive(nextState);

            if (nextState)
            {
                RefreshAll();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void CloseWindow()
        {
            if (windowRoot == null) return;
            windowRoot.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SwitchTab(TabType tab)
        {
            currentTab = tab;

            if (rodTabHighlight != null) rodTabHighlight.SetActive(tab == TabType.Rods);
            if (baitTabHighlight != null) baitTabHighlight.SetActive(tab == TabType.Baits);
            if (sellTabHighlight != null) sellTabHighlight.SetActive(tab == TabType.Sell);

            bool showDetailPanel = tab != TabType.Sell;
            if (detailPanel != null) detailPanel.gameObject.SetActive(showDetailPanel);
            if (inventoryPanel != null) inventoryPanel.gameObject.SetActive(tab == TabType.Sell);

            RefreshItemList();
        }

        private void RefreshAll()
        {
            if (goldText != null)
                goldText.text = $"{userService.UserData.gold:N0} G";

            RefreshItemList();
            if (inventoryPanel != null)
                inventoryPanel.RefreshList();
        }

        private void RefreshItemList()
        {
            if (itemContentParent == null || itemSlotPrefab == null) return;

            foreach (var slot in activeSlots)
                Destroy(slot.gameObject);
            activeSlots.Clear();

            if (currentTab == TabType.Rods)
            {
                var rods = dataService.GetAllRodData();
                foreach (var rod in rods)
                {
                    var slotGO = Instantiate(itemSlotPrefab, itemContentParent);
                    var slotUI = slotGO.GetComponent<ShopSlotUI>();
                    if (slotUI != null)
                    {
                        bool owned = userService.IsRodOwned(rod.id);
                        bool equipped = userService.UserData.equippedRodId == rod.id;
                        slotUI.Setup(rod.rodName, rod.icon, rod.rank, rod.price, owned, equipped, () => OnItemSelected(rod.id, ShopItemType.Rod));
                        activeSlots.Add(slotUI);
                    }
                }
            }
            else if (currentTab == TabType.Baits)
            {
                var baits = dataService.GetAllBaitData();
                foreach (var bait in baits)
                {
                    var slotGO = Instantiate(itemSlotPrefab, itemContentParent);
                    var slotUI = slotGO.GetComponent<ShopSlotUI>();
                    if (slotUI != null)
                    {
                        bool owned = userService.IsBaitOwned(bait.id);
                        bool equipped = userService.UserData.equippedBaitId == bait.id;
                        slotUI.Setup(bait.baitName, bait.icon, bait.rank, bait.price, owned, equipped, () => OnItemSelected(bait.id, ShopItemType.Bait));
                        activeSlots.Add(slotUI);
                    }
                }
            }

            if (activeSlots.Count > 0)
                activeSlots[0].Select();
        }

        private void OnItemSelected(string itemId, ShopItemType itemType)
        {
            if (detailPanel == null) return;

            if (itemType == ShopItemType.Rod)
            {
                var rod = dataService.GetRodData(itemId);
                if (rod == null) return;

                bool owned = userService.IsRodOwned(itemId);
                bool equipped = userService.UserData.equippedRodId == itemId;
                detailPanel.ShowRodDetail(rod, owned, equipped);
            }
            else if (itemType == ShopItemType.Bait)
            {
                var bait = dataService.GetBaitData(itemId);
                if (bait == null) return;

                bool owned = userService.IsBaitOwned(itemId);
                bool equipped = userService.UserData.equippedBaitId == itemId;
                detailPanel.ShowBaitDetail(bait, owned, equipped);
            }
        }

        public void OnBuyClicked(string itemId, ShopItemType itemType)
        {
            if (confirmDialog != null)
            {
                var itemName = itemType == ShopItemType.Rod
                    ? dataService.GetRodData(itemId)?.rodName
                    : dataService.GetBaitData(itemId)?.baitName;
                confirmDialog.Show($"구매 확인", $"{itemName}을(를) 구매하시겠습니까?", () =>
                {
                    bool localSuccess = userService.BuyItem(itemType, itemId);
                    if (localSuccess)
                    {
                        var player = GetLocalPlayer();
                        if (player != null)
                        {
                            player.CmdBuyItem((int)itemType, itemId);
                        }
                        RefreshAll();
                        detailPanel.ShowMessage("구매 완료!");
                    }
                    else
                    {
                        detailPanel.ShowMessage("골드가 부족합니다.");
                    }
                });
            }
        }

        public void OnEquipClicked(string itemId, ShopItemType itemType)
        {
            var player = GetLocalPlayer();
            if (player == null) return;

            if (itemType == ShopItemType.Rod)
            {
                userService.EquipRod(itemId);
                player.CmdEquipRod(itemId);
            }
            else
            {
                userService.EquipBait(itemId);
                player.CmdEquipBait(itemId);
            }

            RefreshAll();
        }

        public void OnUnequipClicked(ShopItemType itemType)
        {
            var player = GetLocalPlayer();
            if (player == null) return;

            if (itemType == ShopItemType.Rod)
            {
                userService.UnequipRod();
                player.CmdUnequipRod();
            }
            else
            {
                userService.UnequipBait();
                player.CmdUnequipBait();
            }

            RefreshAll();
        }

        private FishingPlayer GetLocalPlayer()
        {
            if (NetworkClient.localPlayer != null)
                return NetworkClient.localPlayer.GetComponent<FishingPlayer>();
            return null;
        }
    }
}
