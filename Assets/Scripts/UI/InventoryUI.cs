using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.UI
{
    public enum InventoryCategory
    {
        Fish,
        Rods,
        Baits
    }

    public enum InventorySource
    {
        Inventory,
        Shop
    }

    public class InventoryUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform contentParent;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Top Tab Group")]
        [SerializeField] private List<Button> categoryButtons = new List<Button>();

        [Header("Side Tab Group")]
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button shopButton;

        [Header("Detail Panel")]
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescriptionText;
        [SerializeField] private Image detailIconImage;
        [SerializeField] private TMP_Text detailWeightText;
        [SerializeField] private TMP_Text detailPriceText;
        [SerializeField] private Button shopBuyButton;
        [SerializeField] private GameObject detailRoot;

        private readonly List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();
        private IUserService userService;
        private IDataService dataService;
        private InventoryCategory currentCategory = InventoryCategory.Fish;
        private InventorySource currentSource = InventorySource.Inventory;
        private string selectedShopFishId;

        private void Awake()
        {
            AutoBindReferences();
        }

        private void Start()
        {
            SetupButtons();

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
            {
                userService.OnDataChanged -= OnDataChanged;
            }
        }

        public void ToggleWindow()
        {
            if (windowRoot == null) return;

            bool nextState = !windowRoot.activeSelf;
            windowRoot.SetActive(nextState);

            if (nextState)
            {
                currentSource = InventorySource.Inventory;
                currentCategory = InventoryCategory.Fish;
                RefreshList();
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
            UpdateTabHighlight();

            switch (currentCategory)
            {
                case InventoryCategory.Fish:
                    RefreshFishList();
                    break;
                case InventoryCategory.Rods:
                    RefreshRodList();
                    break;
                case InventoryCategory.Baits:
                    RefreshBaitList();
                    break;
            }
        }

        private void RefreshFishList()
        {
            if (currentSource == InventorySource.Inventory)
            {
                var inventory = userService.UserData.inventory
                    .OrderByDescending(item => item.caughtTime)
                    .ThenBy(item => item.fishId);

                foreach (InventoryItem item in inventory)
                {
                    FishDataSO fish = dataService.GetFishData(item.fishId);
                    if (fish == null) continue;

                    InventorySlotUI slot = CreateSlot();
                    slot.SetupInventoryFish(item, fish, userService);
                    slot.OnSlotClicked += OnSlotClicked;
                    activeSlots.Add(slot);
                }

                return;
            }

            foreach (FishDataSO fish in dataService.GetAllFishData().OrderBy(fish => fish.id))
            {
                InventorySlotUI slot = CreateSlot();
                slot.SetupFishCatalog(fish, OnBuyFishClicked);
                slot.OnSlotClicked += OnSlotClicked;
                activeSlots.Add(slot);
            }
        }

        private void RefreshRodList()
        {
            IEnumerable<RodDataSO> rods = dataService.GetAllRodData().OrderBy(rod => rod.id);
            if (currentSource == InventorySource.Inventory)
            {
                rods = rods.Where(rod => userService.IsRodOwned(rod.id));
            }

            foreach (RodDataSO rod in rods)
            {
                bool owned = userService.IsRodOwned(rod.id);
                bool equipped = userService.UserData.equippedRodId == rod.id;

                InventorySlotUI slot = CreateSlot();
                slot.SetupRod(rod, owned, equipped, _ => OnRodAction(rod, owned, equipped));
                slot.OnSlotClicked += _ => ShowRodDetail(rod, owned, equipped);
                activeSlots.Add(slot);
            }
        }

        private void RefreshBaitList()
        {
            IEnumerable<BaitDataSO> baits = dataService.GetAllBaitData().OrderBy(bait => bait.id);
            if (currentSource == InventorySource.Inventory)
            {
                baits = baits.Where(bait => userService.IsBaitOwned(bait.id));
            }

            foreach (BaitDataSO bait in baits)
            {
                bool owned = userService.IsBaitOwned(bait.id);
                bool equipped = userService.UserData.equippedBaitId == bait.id;

                InventorySlotUI slot = CreateSlot();
                slot.SetupBait(bait, owned, equipped, _ => OnBaitAction(bait, owned, equipped));
                slot.OnSlotClicked += _ => ShowBaitDetail(bait, owned, equipped);
                activeSlots.Add(slot);
            }
        }

        private void OnRodAction(RodDataSO rod, bool owned, bool equipped)
        {
            if (rod == null) return;

            if (equipped)
            {
                userService.UnequipRod();
            }
            else if (owned)
            {
                userService.EquipRod(rod.id);
            }
            else
            {
                userService.BuyItem(ShopItemType.Rod, rod.id);
            }

            RefreshList();
        }

        private void OnBaitAction(BaitDataSO bait, bool owned, bool equipped)
        {
            if (bait == null) return;

            if (equipped)
            {
                userService.UnequipBait();
            }
            else if (owned)
            {
                userService.EquipBait(bait.id);
            }
            else
            {
                userService.BuyItem(ShopItemType.Bait, bait.id);
            }

            RefreshList();
        }

        private void OnBuyFishClicked(InventorySlotUI slot)
        {
            if (slot == null || slot.FishInfo == null) return;

            selectedShopFishId = slot.FishInfo.id;
            BuyFish();
        }

        private void OnSlotClicked(InventorySlotUI slot)
        {
            if (slot == null) return;

            selectedShopFishId = slot.FishInfo != null ? slot.FishInfo.id : null;
            ShowFishDetail(slot.FishInfo, slot.ItemData);
        }

        private void ShowFishDetail(FishDataSO fishInfo, InventoryItem itemData)
        {
            if (fishInfo == null) return;

            SetDetailIcon(fishInfo.fishIcon);
            if (detailNameText != null) detailNameText.text = fishInfo.fishName;
            if (detailDescriptionText != null) detailDescriptionText.text = string.IsNullOrEmpty(fishInfo.description) ? "설명이 없습니다." : fishInfo.description;
            if (detailWeightText != null)
            {
                detailWeightText.text = itemData != null
                    ? $"{itemData.length:F1} cm / {fishInfo.weight:F1} kg"
                    : $"크기: {fishInfo.minSize:F1}-{fishInfo.maxSize:F1} cm / 무게: {fishInfo.weight:F1} kg";
            }
            if (detailPriceText != null) detailPriceText.text = $"가격: {fishInfo.sellPrice:N0} G";

            bool canBuy = currentSource == InventorySource.Shop;
            SetBuyButton(canBuy, $"{fishInfo.sellPrice:N0} G", BuyFish);
            if (detailRoot != null) detailRoot.SetActive(true);
        }

        private void ShowRodDetail(RodDataSO rod, bool owned, bool equipped)
        {
            if (rod == null) return;

            SetDetailIcon(rod.icon);
            if (detailNameText != null) detailNameText.text = rod.rodName;
            if (detailDescriptionText != null) detailDescriptionText.text = string.IsNullOrEmpty(rod.description) ? "설명이 없습니다." : rod.description;
            if (detailWeightText != null) detailWeightText.text = $"등급: {rod.rank} / 내구도: {rod.durability:F0}";
            if (detailPriceText != null) detailPriceText.text = $"가격: {rod.price:N0} G";
            SetBuyButton(false, string.Empty, null);
            if (detailRoot != null) detailRoot.SetActive(true);
        }

        private void ShowBaitDetail(BaitDataSO bait, bool owned, bool equipped)
        {
            if (bait == null) return;

            SetDetailIcon(bait.icon);
            if (detailNameText != null) detailNameText.text = bait.baitName;
            if (detailDescriptionText != null) detailDescriptionText.text = string.IsNullOrEmpty(bait.description) ? "설명이 없습니다." : bait.description;
            if (detailWeightText != null) detailWeightText.text = $"등급: {bait.rank} / 확률 보너스: {bait.catchChanceBonus:F1}%";
            if (detailPriceText != null) detailPriceText.text = $"가격: {bait.price:N0} G";
            SetBuyButton(false, string.Empty, null);
            if (detailRoot != null) detailRoot.SetActive(true);
        }

        private void BuyFish()
        {
            if (string.IsNullOrEmpty(selectedShopFishId)) return;

            FishDataSO fishInfo = dataService.GetFishData(selectedShopFishId);
            if (fishInfo == null || userService.UserData.gold < fishInfo.sellPrice) return;

            userService.UserData.gold -= fishInfo.sellPrice;
            userService.UserData.AddToInventory(selectedShopFishId, (fishInfo.minSize + fishInfo.maxSize) * 0.5f);
            userService.Save();
            RefreshList();
        }

        private InventorySlotUI CreateSlot()
        {
            if (slotPrefab == null)
            {
                slotPrefab = CreateDefaultSlotPrefab();
            }

            GameObject obj = Instantiate(slotPrefab, contentParent);
            obj.SetActive(true);
            InventorySlotUI slot = obj.GetComponent<InventorySlotUI>();
            if (slot == null) slot = obj.AddComponent<InventorySlotUI>();
            return slot;
        }

        private void SetupButtons()
        {
            int categoryCount = Mathf.Min(categoryButtons.Count, 3);
            for (int i = 0; i < categoryCount; i++)
            {
                int index = i;
                categoryButtons[i].onClick.RemoveAllListeners();
                categoryButtons[i].onClick.AddListener(() => SetCategory(index));
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveAllListeners();
                inventoryButton.onClick.AddListener(() => SetSource(InventorySource.Inventory));
            }

            if (shopButton != null)
            {
                shopButton.onClick.RemoveAllListeners();
                shopButton.onClick.AddListener(() => SetSource(InventorySource.Shop));
            }
        }

        private void SetCategory(int index)
        {
            if (index > (int)InventoryCategory.Baits) return;

            currentCategory = (InventoryCategory)index;
            RefreshList();
        }

        private void SetSource(InventorySource source)
        {
            currentSource = source;
            RefreshList();
        }

        private void OnDataChanged()
        {
            RefreshList();
        }

        private void AutoBindReferences()
        {
            if (windowRoot == null)
            {
                Transform panel = transform.Find("InventoryPanel");
                if (panel != null) windowRoot = panel.gameObject;
            }

            if (contentParent == null)
            {
                Transform content = transform.Find("InventoryPanel/LeftContent/ItemGridPanel/Scroll View/Viewport/Content");
                if (content != null) contentParent = content;
            }

            if (categoryButtons.Count == 0)
            {
                Transform tabGroup = transform.Find("InventoryPanel/TopTapGroup");
                if (tabGroup != null)
                {
                    foreach (Transform child in tabGroup)
                    {
                        Button button = child.GetComponent<Button>();
                        if (button != null) categoryButtons.Add(button);
                        if (categoryButtons.Count >= 3) break;
                    }
                }
            }

            Transform leftContent = transform.Find("InventoryPanel/LeftContent");
            if (leftContent != null)
            {
                List<Button> sideButtons = leftContent.GetComponentsInChildren<Button>(true)
                    .Where(button => button.transform.IsChildOf(leftContent) && !button.transform.IsChildOf(contentParent))
                    .ToList();

                if (inventoryButton == null && sideButtons.Count > 0) inventoryButton = sideButtons[0];
                if (shopButton == null && sideButtons.Count > 1) shopButton = sideButtons[1];
            }

            if (detailNameText == null) detailNameText = FindText("InventoryPanel/RightContent/RightPanel/DetailNamePanel/Text");
            if (detailDescriptionText == null) detailDescriptionText = FindText("InventoryPanel/RightContent/RightPanel/DetailIcon");
            if (detailIconImage == null) detailIconImage = FindImage("InventoryPanel/RightContent/RightPanel/FishIcons");
            if (detailWeightText == null) detailWeightText = FindText("InventoryPanel/RightContent/RightPanel/CmKg");
            if (detailPriceText == null) detailPriceText = FindText("InventoryPanel/RightContent/RightPanel/DetailPriceText");
            if (shopBuyButton == null) shopBuyButton = FindButton("InventoryPanel/RightContent/RightPanel/BuyButton");
            if (detailRoot == null)
            {
                Transform root = transform.Find("InventoryPanel/RightContent");
                if (root != null) detailRoot = root.gameObject;
            }
        }

        private TMP_Text FindText(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<TMP_Text>() : null;
        }

        private Image FindImage(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Image>() : null;
        }

        private Button FindButton(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private void UpdateTabHighlight()
        {
            int categoryCount = Mathf.Min(categoryButtons.Count, 3);
            for (int i = 0; i < categoryCount; i++)
            {
                SetButtonColor(categoryButtons[i], i == (int)currentCategory);
            }

            SetButtonColor(inventoryButton, currentSource == InventorySource.Inventory);
            SetButtonColor(shopButton, currentSource == InventorySource.Shop);
        }

        private static void SetButtonColor(Button button, bool selected)
        {
            if (button == null) return;

            Image image = button.targetGraphic as Image;
            if (image == null) image = button.GetComponent<Image>();
            if (image != null) image.color = selected ? new Color(0.61f, 0.73f, 0.12f, 1f) : Color.white;
        }

        private void SetDetailIcon(Sprite sprite)
        {
            if (detailIconImage == null) return;

            detailIconImage.sprite = sprite != null ? sprite : CreatePlaceholderSprite(128);
            detailIconImage.preserveAspect = true;
            detailIconImage.gameObject.SetActive(true);
        }

        private void SetBuyButton(bool active, string text, UnityEngine.Events.UnityAction action)
        {
            if (shopBuyButton == null) return;

            TMP_Text label = shopBuyButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;

            shopBuyButton.onClick.RemoveAllListeners();
            if (action != null) shopBuyButton.onClick.AddListener(action);
            shopBuyButton.gameObject.SetActive(active);
        }

        private void ClearContent()
        {
            foreach (InventorySlotUI slot in activeSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }

            activeSlots.Clear();

            if (contentParent == null) return;
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void ClearDetail()
        {
            if (detailIconImage != null)
            {
                detailIconImage.sprite = null;
                detailIconImage.gameObject.SetActive(false);
            }
            if (detailNameText != null) detailNameText.text = string.Empty;
            if (detailDescriptionText != null) detailDescriptionText.text = string.Empty;
            if (detailWeightText != null) detailWeightText.text = string.Empty;
            if (detailPriceText != null) detailPriceText.text = string.Empty;
            SetBuyButton(false, string.Empty, null);
            if (detailRoot != null) detailRoot.SetActive(false);
        }

        private void SetNetworkUIVisible(bool visible)
        {
            NetworkMenuUI networkUI = FindFirstObjectByType<NetworkMenuUI>(FindObjectsInactive.Include);
            if (networkUI != null)
            {
                networkUI.enabled = visible;
                networkUI.SetVisible(visible);
            }
        }

        private GameObject CreateDefaultSlotPrefab()
        {
            GameObject slot = new GameObject("InventorySlot_Runtime");
            slot.SetActive(false);

            RectTransform rectTransform = slot.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 130f);

            Image background = slot.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);

            VerticalLayoutGroup layout = slot.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 8, 8);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateImageChild(slot.transform, "FishIcon", new Vector2(80f, 80f));
            CreateTextChild(slot.transform, "NameText", string.Empty, 14f, new Vector2(90f, 20f), TextAlignmentOptions.Center);
            CreateTextChild(slot.transform, "LengthText", string.Empty, 12f, new Vector2(90f, 18f), TextAlignmentOptions.Center);

            GameObject buttonObject = new GameObject("SellButton");
            buttonObject.transform.SetParent(slot.transform, false);
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(80f, 26f);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.86f, 0.22f, 0.18f, 0.95f);
            buttonObject.AddComponent<Button>();
            CreateTextChild(buttonObject.transform, "Text", string.Empty, 12f, new Vector2(80f, 26f), TextAlignmentOptions.Center);

            slot.AddComponent<InventorySlotUI>();
            slot.transform.SetParent(transform, false);
            return slot;
        }

        private static void CreateImageChild(Transform parent, string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            Image image = child.AddComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
        }

        private static void CreateTextChild(Transform parent, string name, string text, float fontSize, Vector2 size, TextAlignmentOptions alignment)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            TMP_Text label = child.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset fontAsset = ResolveKoreanFontAsset();
            if (fontAsset != null) label.font = fontAsset;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private static TMP_FontAsset ResolveKoreanFontAsset()
        {
            if (TMP_Settings.fallbackFontAssets == null) return null;

            foreach (TMP_FontAsset fontAsset in TMP_Settings.fallbackFontAssets)
            {
                if (fontAsset != null && fontAsset.name.Contains("Nanum")) return fontAsset;
            }

            return null;
        }

        private static Sprite CreatePlaceholderSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            int cell = Mathf.Max(1, size / 8);

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ((i % size) / cell + (i / size) / cell) % 2 == 0
                    ? new Color(0.3f, 0.3f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.5f);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            sprite.name = "PlaceholderIcon";
            return sprite;
        }
    }
}
