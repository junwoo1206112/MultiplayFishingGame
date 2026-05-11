using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public enum ViewMode { Inventory, Shop, Rods }

    public class InventoryUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform contentParent;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Top Tap Group")]
        [SerializeField] private List<Button> tapButtons = new List<Button>();
        [SerializeField] private List<TMP_Text> tapTexts = new List<TMP_Text>();

        [Header("Detail Panel")]
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescriptionText;
        [SerializeField] private Image detailIconImage;
        [SerializeField] private TMP_Text detailWeightText;
        [SerializeField] private TMP_Text detailPriceText;
        [SerializeField] private Button shopBuyButton;
        [SerializeField] private GameObject detailRoot;

        private IUserService userService;
        private IDataService dataService;
        private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();
        private ViewMode currentView = ViewMode.Inventory;
        private string selectedShopFishId;

        private static readonly string[] TabLabels = { "인벤", "샵", "로즈" };

        private void Awake() { AutoBindReferences(); }

        private void Start()
        {
            SetupTapButtons();
            SetTapTexts();

            if (!DIContainer.TryResolve(out userService) || !DIContainer.TryResolve(out dataService))
            {
                Debug.LogWarning("[InventoryUI] UserService or DataService is not ready.");
                if (windowRoot != null) windowRoot.SetActive(false);
                return;
            }

            userService.OnDataChanged += OnDataChanged;
            if (windowRoot != null) windowRoot.SetActive(false);
            RefreshList();
        }

        private void AutoBindReferences()
        {
            if (windowRoot == null)
            {
                var p = transform.Find("InventoryPanel");
                if (p != null) windowRoot = p.gameObject;
            }
            if (contentParent == null)
            {
                var c = transform.Find("InventoryPanel/LeftContent/ItemGridPanel/Scroll View/Viewport/Content");
                if (c != null) contentParent = c;
            }
            if (tapButtons.Count == 0)
            {
                var tg = transform.Find("InventoryPanel/TopTapGroup");
                if (tg != null)
                {
                    foreach (Transform child in tg)
                    {
                        var btn = child.GetComponent<Button>();
                        if (btn != null) { tapButtons.Add(btn); tapTexts.Add(btn.GetComponentInChildren<TMP_Text>(true)); }
                    }
                }
            }
            if (detailNameText == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/DetailNamePanel/Text"); if (t != null) detailNameText = t.GetComponent<TMP_Text>(); }
            if (detailDescriptionText == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/DetailIcon"); if (t != null) detailDescriptionText = t.GetComponent<TMP_Text>(); }
            if (detailIconImage == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/FishIcons"); if (t != null) detailIconImage = t.GetComponent<Image>(); }
            if (detailWeightText == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/CmKg"); if (t != null) detailWeightText = t.GetComponent<TMP_Text>(); }
            if (detailPriceText == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/DetailPriceText"); if (t != null) detailPriceText = t.GetComponent<TMP_Text>(); }
            if (shopBuyButton == null) { var t = transform.Find("InventoryPanel/RightContent/RightPanel/BuyButton"); if (t != null) shopBuyButton = t.GetComponent<Button>(); }
            if (detailRoot == null) { var r = transform.Find("InventoryPanel/RightContent"); if (r != null) detailRoot = r.gameObject; }
        }

        private void SetupTapButtons()
        {
            for (int i = 0; i < tapButtons.Count; i++)
            {
                int idx = i;
                tapButtons[i].onClick.RemoveAllListeners();
                tapButtons[i].onClick.AddListener(() => OnTapClicked(idx));
            }
        }

        private void SetTapTexts()
        {
            for (int i = 0; i < tapTexts.Count && i < TabLabels.Length; i++)
            {
                if (tapTexts[i] != null) tapTexts[i].text = TabLabels[i];
            }
        }

        private void OnTapClicked(int index)
        {
            if (index == 0) SwitchView(ViewMode.Inventory);
            else if (index == 1) SwitchView(ViewMode.Shop);
            else if (index == 2) SwitchView(ViewMode.Rods);
        }

        private void SwitchView(ViewMode newView)
        {
            if (currentView == newView) return;
            currentView = newView;
            ClearContent();
            ClearDetail();
            UpdateTapHighlight();

            if (currentView == ViewMode.Inventory) RefreshList();
            else if (currentView == ViewMode.Shop) RefreshShopList();
            else RefreshRodList();
        }

        private void UpdateTapHighlight()
        {
            for (int i = 0; i < tapButtons.Count; i++)
            {
                if (tapButtons[i] == null) continue;
                Image img = tapButtons[i].targetGraphic as Image;
                if (img == null) img = tapButtons[i].GetComponent<Image>();
                if (img != null) img.color = i == (int)currentView ? new Color(0.8f, 0.8f, 0.2f, 1f) : Color.white;
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey)) ToggleWindow();
        }

        private void OnDestroy()
        {
            if (userService != null) userService.OnDataChanged -= OnDataChanged;
        }

        private void OnDataChanged()
        {
            if (currentView == ViewMode.Inventory) RefreshList();
            else if (currentView == ViewMode.Rods) RefreshRodList();
        }

        public void ToggleWindow()
        {
            if (windowRoot == null) return;
            bool nextState = !windowRoot.activeSelf;
            windowRoot.SetActive(nextState);
            if (nextState)
            {
                currentView = ViewMode.Inventory;
                ClearDetail();
                RefreshList();
                UpdateTapHighlight();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SetNetworkUIVisible(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                SetNetworkUIVisible(true);
            }
        }

        public void RefreshList()
        {
            if (userService == null || dataService == null || contentParent == null) return;
            ClearContent();
            ClearDetail();
            if (slotPrefab == null) slotPrefab = CreateDefaultSlotPrefab();

            var userData = userService.UserData;
            if (userData == null || userData.inventory == null) return;

            for (int i = userData.inventory.Count - 1; i >= 0; i--)
            {
                var item = userData.inventory[i];
                var fishInfo = dataService.GetFishData(item.fishId);
                if (fishInfo == null) continue;
                GameObject obj = Instantiate(slotPrefab, contentParent);
                obj.SetActive(true);
                var slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(item, fishInfo, userService);
                    slotUI.OnSlotClicked += OnSlotClicked;
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void RefreshShopList()
        {
            if (dataService == null || contentParent == null) return;
            ClearContent();
            ClearDetail();
            var allFish = dataService.GetAllFishData();
            foreach (var fish in allFish)
            {
                GameObject obj = Instantiate(slotPrefab ?? CreateDefaultSlotPrefab(), contentParent);
                obj.SetActive(true);
                var slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetupCatalog(fish, OnBuySlotClicked);
                    slotUI.OnSlotClicked += OnSlotClicked;
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void RefreshRodList()
        {
            if (dataService == null || contentParent == null) return;
            ClearContent();
            ClearDetail();
            var allRods = dataService.GetAllRodData();
            foreach (var rod in allRods)
            {
                GameObject obj = Instantiate(slotPrefab ?? CreateDefaultSlotPrefab(), contentParent);
                obj.SetActive(true);
                var slotUI = obj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    SetupRodSlot(slotUI, rod);
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void SetupRodSlot(InventorySlotUI slotUI, RodDataSO rod)
        {
            bool owned = userService != null && userService.IsRodOwned(rod.id);
            bool equipped = userService != null && userService.UserData.equippedRodId == rod.id;
            if (slotUI == null) return;

            var iconImage = slotUI.GetComponentInChildren<Image>(true);
            if (iconImage != null && iconImage.name == "FishIcon")
                iconImage.sprite = rod.icon ?? CreatePlaceholderSprite(80);

            Transform nt = slotUI.transform.Find("NameText");
            TMP_Text nameText = nt != null ? nt.GetComponent<TMP_Text>() : null;
            if (nameText == null)
            {
                GameObject no = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                no.transform.SetParent(slotUI.transform, false);
                nameText = no.GetComponent<TMP_Text>();
                nameText.fontSize = 14f;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.color = Color.white;
            }
            nameText.text = rod.rodName;

            var sellBtn = slotUI.GetComponentInChildren<Button>(true);
            if (sellBtn != null && sellBtn.name == "SellButton")
            {
                var btnText = sellBtn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                    btnText.text = equipped ? "해제" : (owned ? "장착" : $"{rod.price}G");
                sellBtn.onClick.RemoveAllListeners();
                string rodId = rod.id;
                bool isOwned = owned, isEquipped = equipped;
                sellBtn.onClick.AddListener(() =>
                {
                    if (userService == null) return;
                    if (isEquipped) { userService.UnequipRod(); RefreshRodList(); }
                    else if (isOwned) { userService.EquipRod(rodId); RefreshRodList(); }
                    else { BuyRod(rod); }
                });
                sellBtn.gameObject.SetActive(true);
            }
        }

        private void BuyRod(RodDataSO rod)
        {
            if (userService == null || userService.UserData.gold < rod.price) return;
            userService.UserData.gold -= rod.price;
            userService.UserData.ownedRodIds.Add(rod.id);
            userService.Save();
            RefreshRodList();
        }

        private void ClearContent()
        {
            foreach (var s in activeSlots) Destroy(s.gameObject);
            activeSlots.Clear();
            foreach (Transform c in contentParent) Destroy(c.gameObject);
        }

        private void OnBuySlotClicked(InventorySlotUI slot)
        {
            if (slot?.FishInfo == null || userService == null) return;
            selectedShopFishId = slot.FishInfo.id;
            BuyFish();
        }

        private void OnSlotClicked(InventorySlotUI slot)
        {
            if (slot == null) return;
            if (currentView == ViewMode.Shop)
            {
                selectedShopFishId = slot.FishInfo != null ? slot.FishInfo.id : null;
                ShowShopFishDetail(slot.FishInfo);
            }
            else
            {
                ShowFishDetail(slot.FishInfo, slot.ItemData);
            }
        }

        private void ShowFishDetail(FishDataSO fishInfo, InventoryItem itemData)
        {
            if (detailIconImage != null) { detailIconImage.sprite = fishInfo != null ? fishInfo.fishIcon : CreatePlaceholderSprite(128); detailIconImage.preserveAspect = true; detailIconImage.gameObject.SetActive(true); }
            if (detailNameText != null) detailNameText.text = fishInfo != null ? fishInfo.fishName : "???";
            if (detailDescriptionText != null) detailDescriptionText.text = fishInfo != null && !string.IsNullOrEmpty(fishInfo.description) ? fishInfo.description : "설명이 없습니다.";
            if (detailWeightText != null && itemData != null) detailWeightText.text = $"{itemData.length:F1} cm / {(fishInfo != null ? fishInfo.weight : 0f):F1} kg";
            else if (detailWeightText != null && fishInfo != null) detailWeightText.text = $"크기: {fishInfo.minSize:F1}~{fishInfo.maxSize:F1} cm / 무게: {fishInfo.weight:F1} kg";
            if (detailPriceText != null && fishInfo != null) detailPriceText.text = $"판매가: {fishInfo.sellPrice}G";
            if (shopBuyButton != null) shopBuyButton.gameObject.SetActive(false);
            if (detailRoot != null) detailRoot.SetActive(true);
        }

        private void ShowShopFishDetail(FishDataSO fishInfo)
        {
            if (fishInfo == null) return;
            if (detailIconImage != null) { detailIconImage.sprite = fishInfo.fishIcon ?? CreatePlaceholderSprite(128); detailIconImage.preserveAspect = true; detailIconImage.gameObject.SetActive(true); }
            if (detailNameText != null) detailNameText.text = fishInfo.fishName;
            if (detailDescriptionText != null) detailDescriptionText.text = !string.IsNullOrEmpty(fishInfo.description) ? fishInfo.description : "설명이 없습니다.";
            if (detailWeightText != null) detailWeightText.text = $"크기: {fishInfo.minSize:F1}~{fishInfo.maxSize:F1} cm / 무게: {fishInfo.weight:F1} kg";
            if (detailPriceText != null) detailPriceText.text = $"가격: {fishInfo.sellPrice}G";
            if (shopBuyButton != null)
            {
                var tmp = shopBuyButton.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = $"{fishInfo.sellPrice}G 구매";
                shopBuyButton.onClick.RemoveAllListeners();
                shopBuyButton.onClick.AddListener(BuyFish);
                shopBuyButton.gameObject.SetActive(true);
            }
            if (detailRoot != null) detailRoot.SetActive(true);
        }

        private void BuyFish()
        {
            if (string.IsNullOrEmpty(selectedShopFishId) || userService == null) return;
            var fishInfo = dataService.GetFishData(selectedShopFishId);
            if (fishInfo == null || userService.UserData.gold < fishInfo.sellPrice) return;
            userService.UserData.gold -= fishInfo.sellPrice;
            userService.UserData.AddToInventory(selectedShopFishId, (fishInfo.minSize + fishInfo.maxSize) * 0.5f);
            userService.Save();
            RefreshShopList();
        }

        private void SetNetworkUIVisible(bool visible)
        {
            var networkUI = FindFirstObjectByType<NetworkMenuUI>(FindObjectsInactive.Include);
            if (networkUI != null) { networkUI.enabled = visible; networkUI.SetVisible(visible); }
        }

        private void ClearDetail()
        {
            if (detailIconImage != null) { detailIconImage.sprite = null; detailIconImage.gameObject.SetActive(false); }
            if (detailNameText != null) detailNameText.text = "";
            if (detailDescriptionText != null) detailDescriptionText.text = "";
            if (detailWeightText != null) detailWeightText.text = "";
            if (detailPriceText != null) detailPriceText.text = "";
            if (shopBuyButton != null) { shopBuyButton.onClick.RemoveAllListeners(); shopBuyButton.gameObject.SetActive(false); }
            if (detailRoot != null) detailRoot.SetActive(false);
        }

        private GameObject CreateDefaultSlotPrefab()
        {
            GameObject slot = new GameObject("InventorySlot_Runtime");
            slot.SetActive(false);
            RectTransform r = slot.AddComponent<RectTransform>(); r.sizeDelta = new Vector2(100f, 130f);
            Image bg = slot.AddComponent<Image>(); bg.color = new Color(0f, 0f, 0f, 0.55f);
            VerticalLayoutGroup l = slot.AddComponent<VerticalLayoutGroup>();
            l.padding = new RectOffset(0, 0, 8, 8); l.spacing = 6f; l.childAlignment = TextAnchor.UpperCenter; l.childForceExpandWidth = true; l.childForceExpandHeight = false;
            CreateImageChild(slot.transform, "FishIcon", new Vector2(80f, 80f));
            GameObject sb = new GameObject("SellButton"); sb.transform.SetParent(slot.transform, false);
            RectTransform br = sb.AddComponent<RectTransform>(); br.sizeDelta = new Vector2(80f, 26f);
            Image bi = sb.AddComponent<Image>(); bi.color = new Color(0.86f, 0.22f, 0.18f, 0.95f);
            sb.AddComponent<Button>();
            CreateTextChild(sb.transform, "Text", "Sell", 12f, new Vector2(80f, 26f), TextAlignmentOptions.Center);
            slot.AddComponent<InventorySlotUI>();
            slot.transform.SetParent(transform, false);
            return slot;
        }

        private static void CreateImageChild(Transform parent, string name, Vector2 size)
        {
            GameObject c = new GameObject(name); c.transform.SetParent(parent, false);
            RectTransform r = c.AddComponent<RectTransform>(); r.sizeDelta = size;
            Image i = c.AddComponent<Image>(); i.color = Color.white;
        }

        private static void CreateTextChild(Transform parent, string name, string text, float fontSize, Vector2 size, TextAlignmentOptions alignment)
        {
            GameObject c = new GameObject(name); c.transform.SetParent(parent, false);
            RectTransform r = c.AddComponent<RectTransform>(); r.sizeDelta = size;
            TMP_Text l = c.AddComponent<TextMeshProUGUI>();
            if (ResolveKoreanFontAsset() != null) l.font = ResolveKoreanFontAsset();
            l.text = text; l.fontSize = fontSize; l.alignment = alignment; l.color = Color.white; l.raycastTarget = false;
        }

        private static TMP_FontAsset ResolveKoreanFontAsset()
        {
            if (TMP_Settings.fallbackFontAssets == null) return null;
            foreach (TMP_FontAsset f in TMP_Settings.fallbackFontAssets)
                if (f != null && f.name.Contains("Nanum")) return f;
            return null;
        }

        private static Sprite CreatePlaceholderSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            int cell = size / 8;
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = ((i % size) / cell + (i / size) / cell) % 2 == 0 ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
            tex.SetPixels(pixels); tex.Apply();
            Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            s.name = "PlaceholderIcon"; return s;
        }
    }
}
