using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventoryUI : MonoBehaviour
    {
        private enum InventoryMode { Fish, Rod, Bait }

        [Header("Settings")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private Button exitButton;

        [Header("Inventory View")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform contentParent;

        private TabButton tabButton;
        private Button[] topFilterButtons;
        private GameObject detailRoot;
        private Image detailIconImage;
        private TMP_Text detailNameText;
        private TMP_Text detailDescText;
        private TMP_Text detailPriceText;
        private TMP_Text detailSizeText;
        private GameObject inventoryEmptyText;

        private IUserService userService;
        private IDataService dataService;
        private InventoryMode currentMode = InventoryMode.Fish;
        private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

        private readonly string[] categoryLabels = { "물고기", "낚싯대", "미끼", "전체" };
        private Color filterNormalColor = Color.white;
        private Color filterSelectedColor = new Color(0.847f, 0.918f, 0.180f);

        private void Awake()
        {
            tabButton = GetComponent<TabButton>();
        }

        private void Start()
        {
            ApplyDefaultLayout();

            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            userService.OnDataChanged += RefreshList;

            if (exitButton != null)
                exitButton.onClick.AddListener(ToggleWindow);

            ResolveReferences();
            SetupTopTabs();

            if (windowRoot != null) windowRoot.SetActive(false);
            RefreshList();
        }

        private void ResolveReferences()
        {
            Transform panelTf = windowRoot != null ? windowRoot.transform : null;
            if (panelTf == null) return;

            Transform rightContent = panelTf.Find("RightContent");
            Transform rightPanel = rightContent != null ? rightContent.Find("RightPanel") : null;
            detailRoot = rightPanel != null ? rightPanel.gameObject : null;

            if (rightPanel != null)
            {
                detailIconImage = FindComponentInChild<Image>(rightPanel, "Item_Image");
                detailNameText = FindComponentInChildren<TMP_Text>(rightPanel, "Item_Name");
                detailDescText = FindComponentInChild<TMP_Text>(rightPanel, "Text_Description");
                detailPriceText = FindComponentInChild<TMP_Text>(rightPanel, "Price");
                detailSizeText = FindComponentInChild<TMP_Text>(rightPanel, "Text_Size");
            }

            inventoryEmptyText = FindInChild(panelTf, "EmptyLabel");
        }

        private void SetupTopTabs()
        {
            topFilterButtons = tabButton != null && tabButton.TopButtons != null
                ? tabButton.TopButtons : new Button[0];

            if (tabButton != null)
                tabButton.OnTopTabIndexChanged += OnFilterChanged;

            for (int i = 0; i < topFilterButtons.Length && i < categoryLabels.Length; i++)
            {
                var btn = topFilterButtons[i];
                if (btn == null) continue;
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = categoryLabels[i];
            }

            UpdateFilterHighlight(0);
        }

        private void OnFilterChanged(int index)
        {
            switch (index)
            {
                case 0: currentMode = InventoryMode.Fish; break;
                case 1: currentMode = InventoryMode.Rod; break;
                case 2: currentMode = InventoryMode.Bait; break;
                default: currentMode = InventoryMode.Fish; break;
            }
            UpdateFilterHighlight(index);
            RefreshList();
        }

        private void UpdateFilterHighlight(int selected)
        {
            for (int i = 0; i < topFilterButtons.Length; i++)
            {
                if (topFilterButtons[i] == null) continue;
                Image img = topFilterButtons[i].GetComponent<Image>();
                if (img != null)
                    img.color = i == selected ? filterSelectedColor : filterNormalColor;
            }
        }

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
            windowRect.sizeDelta = new Vector2(1100f, 700f);
            windowRect.localScale = Vector3.one;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey))
                ToggleWindow();
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= RefreshList;

            if (tabButton != null)
                tabButton.OnTopTabIndexChanged -= OnFilterChanged;
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

        private void RefreshList()
        {
            ClearSlots();
            if (contentParent == null || slotPrefab == null) return;

            switch (currentMode)
            {
                case InventoryMode.Fish:
                    RefreshFishList();
                    break;
                case InventoryMode.Rod:
                    RefreshRodList();
                    break;
                case InventoryMode.Bait:
                    RefreshBaitList();
                    break;
            }
        }

        private void RefreshFishList()
        {
            var inventory = userService.UserData.inventory;
            if (inventory.Count == 0)
            {
                if (inventoryEmptyText != null) inventoryEmptyText.SetActive(true);
                return;
            }
            if (inventoryEmptyText != null) inventoryEmptyText.SetActive(false);

            foreach (var item in inventory)
            {
                var fishInfo = dataService.GetFishData(item.fishId);
                if (fishInfo == null) continue;

                GameObject obj = Instantiate(slotPrefab, contentParent);
                InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(item, fishInfo, userService);
                    var capturedItem = item;
                    var capturedFish = fishInfo;
                    slotUI.onSlotClicked = () => ShowItemDetail(capturedItem, capturedFish);
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void RefreshRodList()
        {
            var rodIds = userService.UserData.ownedRodIds;
            if (rodIds.Count == 0)
            {
                if (inventoryEmptyText != null) inventoryEmptyText.SetActive(true);
                return;
            }
            if (inventoryEmptyText != null) inventoryEmptyText.SetActive(false);

            foreach (var rodId in rodIds)
            {
                var rodData = dataService.GetRodData(rodId);
                if (rodData == null) continue;

                GameObject obj = Instantiate(slotPrefab, contentParent);
                InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    bool isEquipped = rodId == userService.UserData.equippedRodId;
                    slotUI.SetupRod(rodData, isEquipped, userService);
                    var capturedRod = rodData;
                    slotUI.onSlotClicked = () => ShowItemDetail(capturedRod);
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void RefreshBaitList()
        {
            var baitIds = userService.UserData.ownedBaitIds;
            if (baitIds.Count == 0)
            {
                if (inventoryEmptyText != null) inventoryEmptyText.SetActive(true);
                return;
            }
            if (inventoryEmptyText != null) inventoryEmptyText.SetActive(false);

            foreach (var baitId in baitIds)
            {
                var baitData = dataService.GetBaitData(baitId);
                if (baitData == null) continue;

                GameObject obj = Instantiate(slotPrefab, contentParent);
                InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    bool isEquipped = baitId == userService.UserData.equippedBaitId;
                    slotUI.SetupBait(baitData, isEquipped, userService);
                    var capturedBait = baitData;
                    slotUI.onSlotClicked = () => ShowItemDetail(capturedBait);
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in activeSlots)
                Destroy(slot.gameObject);
            activeSlots.Clear();
        }

        private void ShowItemDetail(InventoryItem item, FishDataSO fishInfo)
        {
            if (detailRoot != null) detailRoot.SetActive(true);
            if (detailIconImage != null) detailIconImage.sprite = fishInfo.fishIcon;
            if (detailNameText != null) detailNameText.text = fishInfo.fishName;
            if (detailDescText != null) detailDescText.text = fishInfo.description;
            if (detailPriceText != null) detailPriceText.text = $"{fishInfo.sellPrice:N0} G";
            if (detailSizeText != null) detailSizeText.text = $"{item.length:F1} cm";
        }

        private void ShowItemDetail(RodDataSO rodData)
        {
            if (detailRoot != null) detailRoot.SetActive(true);
            if (detailIconImage != null) detailIconImage.sprite = rodData.icon;
            if (detailNameText != null) detailNameText.text = rodData.rodName;
            if (detailDescText != null) detailDescText.text = rodData.description;
            if (detailPriceText != null) detailPriceText.text = $"{rodData.price:N0} G";
            if (detailSizeText != null)
                detailSizeText.text = $"사거리 +{rodData.castDistanceBonus:F1}m | 포획률 +{rodData.catchChanceBonus * 100:F0}% | 내구도 {rodData.durability:F0}";
        }

        private void ShowItemDetail(BaitDataSO baitData)
        {
            if (detailRoot != null) detailRoot.SetActive(true);
            if (detailIconImage != null) detailIconImage.sprite = baitData.icon;
            if (detailNameText != null) detailNameText.text = baitData.baitName;
            if (detailDescText != null) detailDescText.text = baitData.description;
            if (detailPriceText != null) detailPriceText.text = $"{baitData.price:N0} G";
            if (detailSizeText != null)
                detailSizeText.text = $"포획률 +{baitData.catchChanceBonus * 100:F0}% | 유인 어종 {baitData.attractionFishIds.Length}종";
        }

        private static T FindComponentInChild<T>(Transform parent, string childName) where T : Component
        {
            Transform child = FindDeepChild(parent, childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static T FindComponentInChildren<T>(Transform parent, string childName) where T : Component
        {
            Transform child = FindDeepChild(parent, childName);
            return child != null ? child.GetComponentInChildren<T>() : null;
        }

        private static GameObject FindInChild(Transform parent, string childName)
        {
            Transform child = FindDeepChild(parent, childName);
            return child != null ? child.gameObject : null;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
