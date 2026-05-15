using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MultiplayFishing.UI;

namespace MultiplayFishing.Editor
{
    public static class StoreInventoryUIRepairTool
    {
        private const string StorePrefabPath = "Assets/Prefabs/Game UI/Store.prefab";
        private const string InventoryPrefabPath = "Assets/Prefabs/Game UI/Inventory 1.prefab";
        private const string InventorySlotPrefabPath = "Assets/Prefabs/Game UI/InventorySlot.prefab";
        private const string ShopSlotPrefabPath = "Assets/Prefabs/Game UI/ShopSlot.prefab";
        private const string ShopInventorySlotPrefabPath = "Assets/Prefabs/Game UI/ShopInventorySlot.prefab";
        private const string PlayScenePath = "Assets/Scenes/PlayScene.unity";

        private const string TextStore = "\uC0C1\uC810";
        private const string TextRod = "\uB09A\uC2EF\uB300";
        private const string TextBait = "\uBBF8\uB07C";
        private const string TextSellFish = "\uBB3C\uACE0\uAE30 \uD310\uB9E4";
        private const string TextItem = "\uC544\uC774\uD15C";
        private const string TextBuy = "\uAD6C\uB9E4";
        private const string TextEquip = "\uC7A5\uCC29";
        private const string TextUnequip = "\uD574\uC81C";
        private const string TextSell = "\uD310\uB9E4";
        private const string TextOwned = "\uBCF4\uC720";
        private const string TextMyFish = "\uB0B4 \uBB3C\uACE0\uAE30";
        private const string TextSellAll = "\uC804\uCCB4 \uD310\uB9E4";
        private const string TextNoFish = "\uBCF4\uC720\uD55C \uBB3C\uACE0\uAE30\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.";
        private const string TextConfirm = "\uD655\uC778";
        private const string TextCancel = "\uCDE8\uC18C";

        [MenuItem("Tools/UI/Repair Store And Inventory UI")]
        public static void RepairStoreAndInventoryUI()
        {
            EnsureFolder("Assets/Prefabs/Game UI");

            CreateInventorySlotPrefab();
            RepairInventoryPrefab();
            CreateShopSlotPrefab();
            CreateShopInventorySlotPrefab();
            RebuildStorePrefab();
            RepairFishingUIPrefab();

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EnsurePlayScenePlacement();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Keep this menu silent so Unity's Console does not retain noisy editor-tool stack traces.
        }

        private static void CreateInventorySlotPrefab()
        {
            GameObject root = new GameObject("InventorySlot", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(InventorySlotUI));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150f, 166f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(1f, 0.86f, 0.68f, 1f);

            Button button = root.GetComponent<Button>();
            button.targetGraphic = background;

            LayoutElement layout = root.GetComponent<LayoutElement>();
            layout.preferredWidth = 150f;
            layout.preferredHeight = 166f;

            Image fishIcon = CreateImage("FishIcon", root.transform, new Color(1f, 1f, 1f, 0.95f));
            SetRect(fishIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(96f, 74f));
            fishIcon.preserveAspect = true;

            TMP_Text nameText = CreateText("FishNameText", root.transform, "Fish", 18, TextAlignmentOptions.Center);
            SetRect(nameText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 42f), new Vector2(-12f, 26f));
            nameText.color = new Color(0.22f, 0.16f, 0.10f, 1f);
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 12f;
            nameText.fontSizeMax = 18f;
            nameText.maxVisibleLines = 1;

            TMP_Text sizeText = CreateText("SizeText", root.transform, "0.0 cm", 16, TextAlignmentOptions.Center);
            SetRect(sizeText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 20f), new Vector2(-12f, 22f));
            sizeText.color = new Color(0.42f, 0.30f, 0.18f, 1f);

            TMP_Text priceText = CreateText("PriceText", root.transform, "0 G", 15, TextAlignmentOptions.Center);
            SetRect(priceText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 4f), new Vector2(-12f, 18f));
            priceText.color = new Color(0.42f, 0.30f, 0.18f, 1f);
            priceText.gameObject.SetActive(false);

            TMP_Text descriptionText = CreateText("DescriptionText", root.transform, "", 12, TextAlignmentOptions.Center);
            SetRect(descriptionText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, -18f), new Vector2(-12f, 20f));
            descriptionText.color = new Color(0.42f, 0.30f, 0.18f, 1f);
            descriptionText.gameObject.SetActive(false);

            GameObject starContainer = new GameObject("StarContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starContainer.transform.SetParent(root.transform, false);
            SetRect((RectTransform)starContainer.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(96f, 18f));
            HorizontalLayoutGroup starLayout = starContainer.GetComponent<HorizontalLayoutGroup>();
            starLayout.spacing = 2f;
            starLayout.childAlignment = TextAnchor.MiddleCenter;
            starLayout.childControlWidth = false;
            starLayout.childControlHeight = false;

            for (int i = 0; i < 5; i++)
            {
                Image star = CreateImage($"Star{i + 1}", starContainer.transform, new Color(0.5f, 0.5f, 0.5f, 1f));
                star.rectTransform.sizeDelta = new Vector2(14f, 14f);
            }

            Button sellButton = CreateButton("SellButton", root.transform, TextSell, new Color(0.28f, 0.52f, 0.80f, 1f));
            SetRect((RectTransform)sellButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-34f, 22f), new Vector2(54f, 24f));
            sellButton.gameObject.SetActive(false);

            SerializedObject serialized = new SerializedObject(root.GetComponent<InventorySlotUI>());
            SetObject(serialized, "fishIconImage", fishIcon);
            SetObject(serialized, "fishNameText", nameText);
            SetObject(serialized, "sizeText", sizeText);
            SetObject(serialized, "sellPriceText", priceText);
            SetObject(serialized, "descriptionText", descriptionText);
            SetObject(serialized, "starContainer", starContainer.GetComponent<RectTransform>());
            SetObject(serialized, "slotBackground", background);
            SetObject(serialized, "sellButton", sellButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, InventorySlotPrefabPath);
        }

        private static void RepairInventoryPrefab()
        {
            if (!File.Exists(InventoryPrefabPath))
            {
                return;
            }

            GameObject inventory = PrefabUtility.LoadPrefabContents(InventoryPrefabPath);

            try
            {
                RemoveComponent<InventorySlotUI>(inventory);

                InventoryUI inventoryUI = EnsureComponent<InventoryUI>(inventory);
                CanvasScaler scaler = EnsureComponent<CanvasScaler>(inventory);
                EnsureComponent<GraphicRaycaster>(inventory);

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                RectTransform inventoryRect = EnsureRectTransform(inventory);
                SetRect(inventoryRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                Transform windowRoot = FindDeepChild(inventory.transform, "InventoryPanel");
                Transform scrollView = FindDeepChild(inventory.transform, "Scroll View");
                Transform viewport = scrollView != null ? FindDirectChild(scrollView, "Viewport") : null;
                Transform content = viewport != null ? FindDirectChild(viewport, "Content") : null;

                if (content == null && viewport != null && viewport.childCount > 0)
                {
                    content = viewport.GetChild(0);
                    content.name = "Content";
                }

                if (viewport != null && content == null)
                {
                    GameObject contentObject = EnsureChild(viewport, "Content", typeof(RectTransform));
                    content = contentObject.transform;
                }

                if (content != null)
                {
                    DestroyChildren(content);
                    RemoveComponent<VerticalLayoutGroup>(content.gameObject);
                    GridLayoutGroup grid = EnsureComponent<GridLayoutGroup>(content.gameObject);
                    grid.cellSize = new Vector2(150f, 166f);
                    grid.spacing = new Vector2(16f, 16f);
                    grid.padding = new RectOffset(14, 14, 14, 14);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 3;
                    grid.childAlignment = TextAnchor.UpperLeft;

                    ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(content.gameObject);
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }

                Button exitButton = null;
                Transform exitButtonTransform = FindDeepChild(inventory.transform, "Exit_Button");
                if (exitButtonTransform != null)
                {
                    exitButton = exitButtonTransform.GetComponent<Button>();
                }

                SerializedObject serialized = new SerializedObject(inventoryUI);
                SetObject(serialized, "windowRoot", windowRoot != null ? windowRoot.gameObject : null);
                SetObject(serialized, "slotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(InventorySlotPrefabPath));
                SetObject(serialized, "contentParent", content);
                SetKeyCode(serialized, "toggleKey", KeyCode.I);
                SetObject(serialized, "exitButton", exitButton);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(inventory, InventoryPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(inventory);
            }
        }

        private static void RepairFishingUIPrefab()
        {
            const string fishingPrefabPath = "Assets/Prefabs/Game UI/FishingUI.prefab";
            if (!File.Exists(fishingPrefabPath))
            {
                return;
            }

            GameObject fishingUI = PrefabUtility.LoadPrefabContents(fishingPrefabPath);

            try
            {
                RepairFishingUIObject(fishingUI);
                PrefabUtility.SaveAsPrefabAsset(fishingUI, fishingPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(fishingUI);
            }
        }

        private static void RepairFishingUIObject(GameObject fishingUI)
        {
            RectTransform rootRect = EnsureRectTransform(fishingUI);
            SetRect(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform timer = FindDirectChild(fishingUI.transform, "Timer");
            Transform catchingBar = FindDirectChild(fishingUI.transform, "CatchingBar");
            Transform alertPanel = FindDirectChild(fishingUI.transform, "F_Message");

            SetFishingPanel(timer, new Vector2(0f, 1f), new Vector2(88f, -88f), new Vector2(96f, 96f), false);
            SetFishingPanel(catchingBar, new Vector2(0.5f, 0f), new Vector2(0f, 96f), new Vector2(420f, 72f), false);
            SetFishingPanel(alertPanel, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(280f, 88f), false);

            if (timer != null)
            {
                SetStretchLayout(timer.Find("Fill Area") as RectTransform, new Vector2(12f, 12f), new Vector2(-12f, -12f));
                SetStretchLayout(timer.Find("Fill Area/T_Fill") as RectTransform, Vector2.zero, Vector2.zero);
            }

            if (catchingBar != null)
            {
                SetStretchLayout(catchingBar.Find("C_Background") as RectTransform, Vector2.zero, Vector2.zero);
                SetStretchLayout(catchingBar.Find("Border") as RectTransform, Vector2.zero, Vector2.zero);
                SetStretchLayout(catchingBar.Find("C_Background/Fill Area") as RectTransform, new Vector2(18f, 16f), new Vector2(-18f, -16f));
                SetStretchLayout(catchingBar.Find("C_Background/Fill Area/C_Fill") as RectTransform, Vector2.zero, Vector2.zero);
            }

            FishingUI component = fishingUI.GetComponent<FishingUI>();
            if (component == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(component);
            SetObject(serialized, "chargingPanel", timer != null ? timer.gameObject : null);
            SetObject(serialized, "catchingPanel", catchingBar != null ? catchingBar.gameObject : null);
            SetObject(serialized, "alertPanel", alertPanel != null ? alertPanel.gameObject : null);
            SetObject(serialized, "chargingBar", timer != null ? timer.GetComponent<Slider>() : null);
            SetObject(serialized, "fillImage", FindImage(catchingBar, "C_Background/Fill Area/C_Fill"));
            SetObject(serialized, "backgroundRect", catchingBar != null ? catchingBar.Find("C_Background") : null);
            SetObject(serialized, "borderRect", catchingBar != null ? catchingBar.Find("Border") : null);
            SetObject(serialized, "tFillImage", FindImage(timer, "Fill Area/T_Fill"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFishingPanel(Transform panel, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, bool active)
        {
            if (panel == null)
            {
                return;
            }

            SetRect((RectTransform)panel, anchor, anchor, anchoredPosition, size);
            panel.gameObject.SetActive(active);
        }

        private static void SetStretchLayout(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Image FindImage(Transform root, string path)
        {
            if (root == null)
            {
                return null;
            }

            Transform target = root.Find(path);
            return target != null ? target.GetComponent<Image>() : null;
        }

        private static void CreateShopSlotPrefab()
        {
            GameObject root = new GameObject("ShopSlot", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(ShopSlotUI));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 112f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.96f, 0.91f, 0.80f, 0.95f);

            Button button = root.GetComponent<Button>();
            button.targetGraphic = background;

            LayoutElement layout = root.GetComponent<LayoutElement>();
            layout.preferredWidth = 260f;
            layout.preferredHeight = 112f;

            Image iconImage = CreateImage("IconImage", root.transform, new Color(1f, 1f, 1f, 0.85f));
            SetRect(iconImage.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(76f, 76f));

            TMP_Text nameText = CreateText("NameText", root.transform, "Item", 22, TextAlignmentOptions.Left);
            SetRect(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(82f, -22f), new Vector2(-100f, 28f));

            TMP_Text rankText = CreateText("RankText", root.transform, "*", 18, TextAlignmentOptions.Left);
            SetRect(rankText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(84f, -54f), new Vector2(90f, 24f));

            TMP_Text priceText = CreateText("PriceText", root.transform, "0 G", 18, TextAlignmentOptions.Right);
            SetRect(priceText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-54f, 24f), new Vector2(96f, 24f));

            GameObject ownedBadge = CreateBadge("OwnedBadge", root.transform, TextOwned);
            SetRect((RectTransform)ownedBadge.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-46f, -22f), new Vector2(72f, 24f));

            GameObject equippedBadge = CreateBadge("EquippedBadge", root.transform, TextEquip);
            SetRect((RectTransform)equippedBadge.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-46f, -52f), new Vector2(72f, 24f));

            SerializedObject serialized = new SerializedObject(root.GetComponent<ShopSlotUI>());
            SetObject(serialized, "iconImage", iconImage);
            SetObject(serialized, "nameText", nameText);
            SetObject(serialized, "rankText", rankText);
            SetObject(serialized, "priceText", priceText);
            SetObject(serialized, "ownedBadge", ownedBadge);
            SetObject(serialized, "equippedBadge", equippedBadge);
            SetObject(serialized, "slotButton", button);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ShopSlotPrefabPath);
        }

        private static void CreateShopInventorySlotPrefab()
        {
            GameObject root = new GameObject("ShopInventorySlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ShopInventorySlotUI));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(520f, 88f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.93f, 0.96f, 0.98f, 0.95f);

            LayoutElement layout = root.GetComponent<LayoutElement>();
            layout.preferredHeight = 88f;

            Image fishIcon = CreateImage("FishIcon", root.transform, new Color(1f, 1f, 1f, 0.9f));
            SetRect(fishIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(64f, 64f));

            TMP_Text nameText = CreateText("NameText", root.transform, "Fish", 22, TextAlignmentOptions.Left);
            SetRect(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(92f, -20f), new Vector2(-224f, 28f));

            TMP_Text lengthText = CreateText("LengthText", root.transform, "0.0 cm", 18, TextAlignmentOptions.Left);
            SetRect(lengthText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, 24f), new Vector2(120f, 24f));

            TMP_Text priceText = CreateText("PriceText", root.transform, "0 G", 20, TextAlignmentOptions.Right);
            SetRect(priceText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-150f, 0f), new Vector2(100f, 28f));

            Button sellButton = CreateButton("SellButton", root.transform, TextSell, new Color(0.28f, 0.52f, 0.80f, 1f));
            SetRect((RectTransform)sellButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-56f, 0f), new Vector2(82f, 38f));

            SerializedObject serialized = new SerializedObject(root.GetComponent<ShopInventorySlotUI>());
            SetObject(serialized, "fishIcon", fishIcon);
            SetObject(serialized, "nameText", nameText);
            SetObject(serialized, "lengthText", lengthText);
            SetObject(serialized, "priceText", priceText);
            SetObject(serialized, "sellButton", sellButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ShopInventorySlotPrefabPath);
        }

        private static void RebuildStorePrefab()
        {
            if (!File.Exists(StorePrefabPath))
            {
                EditorUtility.DisplayDialog("Store UI Repair", "Store.prefab was not found.", "OK");
                return;
            }

            GameObject store = PrefabUtility.LoadPrefabContents(StorePrefabPath);

            try
            {
                RemoveComponent<InventoryUI>(store);
                RemoveComponent<InventorySlotUI>(store);
                RemoveComponent<TabButton>(store);

                ShopUI shopUI = EnsureComponent<ShopUI>(store);
                CanvasScaler scaler = EnsureComponent<CanvasScaler>(store);
                EnsureComponent<GraphicRaycaster>(store);

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                RectTransform storeRect = EnsureRectTransform(store);
                SetRect(storeRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                DeactivateLegacyChildren(store.transform);

                GameObject windowRoot = EnsureChild(store.transform, "WindowRoot", typeof(Image));
                windowRoot.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.18f, 0.96f);
                SetRect((RectTransform)windowRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 800f));

                TMP_Text goldText = BuildTopBar(windowRoot.transform);
                BuildTabs(windowRoot.transform, out Button rodTabButton, out Button baitTabButton, out Button sellTabButton, out GameObject rodTabHighlight, out GameObject baitTabHighlight, out GameObject sellTabHighlight);

                Transform itemContentParent = BuildItemListPanel(windowRoot.transform);
                ShopDetailPanel detailPanel = BuildDetailPanel(windowRoot.transform);
                ShopInventoryPanel inventoryPanel = BuildSellInventoryPanel(windowRoot.transform);
                ConfirmDialog confirmDialog = BuildConfirmDialog(windowRoot.transform);

                SerializedObject shopSerialized = new SerializedObject(shopUI);
                SetObject(shopSerialized, "windowRoot", windowRoot);
                SetKeyCode(shopSerialized, "toggleKey", KeyCode.B);
                SetObject(shopSerialized, "goldText", goldText);
                SetObject(shopSerialized, "rodTabButton", rodTabButton);
                SetObject(shopSerialized, "baitTabButton", baitTabButton);
                SetObject(shopSerialized, "sellTabButton", sellTabButton);
                SetObject(shopSerialized, "rodTabHighlight", rodTabHighlight);
                SetObject(shopSerialized, "baitTabHighlight", baitTabHighlight);
                SetObject(shopSerialized, "sellTabHighlight", sellTabHighlight);
                SetObject(shopSerialized, "itemContentParent", itemContentParent);
                SetObject(shopSerialized, "itemSlotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ShopSlotPrefabPath));
                SetObject(shopSerialized, "detailPanel", detailPanel);
                SetObject(shopSerialized, "inventoryPanel", inventoryPanel);
                SetObject(shopSerialized, "confirmDialog", confirmDialog);
                shopSerialized.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject inventorySerialized = new SerializedObject(inventoryPanel);
                SetObject(inventorySerialized, "slotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ShopInventorySlotPrefabPath));
                SetObject(inventorySerialized, "confirmDialog", confirmDialog);
                inventorySerialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(store, StorePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(store);
            }
        }

        private static TMP_Text BuildTopBar(Transform parent)
        {
            GameObject topBar = EnsureChild(parent, "TopBar", typeof(Image));
            SetRect((RectTransform)topBar.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -42f), new Vector2(-48f, 64f));
            topBar.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.12f, 0.9f);

            TMP_Text titleText = EnsureText(topBar.transform, "TitleText", TextStore, 34, TextAlignmentOptions.Left);
            SetRect(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(200f, 42f));

            TMP_Text goldText = EnsureText(topBar.transform, "GoldText", "0 G", 28, TextAlignmentOptions.Right);
            SetRect(goldText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(220f, 42f));
            return goldText;
        }

        private static void BuildTabs(Transform parent, out Button rodTabButton, out Button baitTabButton, out Button sellTabButton, out GameObject rodTabHighlight, out GameObject baitTabHighlight, out GameObject sellTabHighlight)
        {
            GameObject tabs = EnsureChild(parent, "TabButtons");
            SetRect((RectTransform)tabs.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(248f, -118f), new Vector2(650f, 52f));

            HorizontalLayoutGroup layout = EnsureComponent<HorizontalLayoutGroup>(tabs);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            rodTabButton = EnsureTabButton(tabs.transform, "RodTab", TextRod, out rodTabHighlight);
            baitTabButton = EnsureTabButton(tabs.transform, "BaitTab", TextBait, out baitTabHighlight);
            sellTabButton = EnsureTabButton(tabs.transform, "SellTab", TextSellFish, out sellTabHighlight);
        }

        private static Transform BuildItemListPanel(Transform parent)
        {
            GameObject panel = EnsureChild(parent, "ItemListPanel", typeof(Image), typeof(ScrollRect));
            SetRect((RectTransform)panel.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(250f, -446f), new Vector2(430f, 580f));
            panel.GetComponent<Image>().color = new Color(0.17f, 0.21f, 0.23f, 0.96f);

            GameObject viewport = EnsureChild(panel.transform, "Viewport", typeof(Image), typeof(Mask));
            SetRect((RectTransform)viewport.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -24f));
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = EnsureChild(viewport.transform, "Content", typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            SetRect((RectTransform)content.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = panel.GetComponent<ScrollRect>();
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.content = (RectTransform)content.transform;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return content.transform;
        }

        private static ShopDetailPanel BuildDetailPanel(Transform parent)
        {
            GameObject panel = EnsureChild(parent, "DetailPanel", typeof(Image), typeof(ShopDetailPanel));
            SetRect((RectTransform)panel.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-358f, -404f), new Vector2(470f, 620f));
            panel.GetComponent<Image>().color = new Color(0.19f, 0.23f, 0.25f, 0.96f);

            Image iconImage = EnsureImage(panel.transform, "IconImage", new Color(1f, 1f, 1f, 0.9f));
            SetRect(iconImage.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(92f, -92f), new Vector2(120f, 120f));

            TMP_Text nameText = EnsureText(panel.transform, "NameText", TextItem, 30, TextAlignmentOptions.Left);
            SetRect(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(180f, -62f), new Vector2(-220f, 38f));

            TMP_Text rankText = EnsureText(panel.transform, "RankText", "*", 22, TextAlignmentOptions.Left);
            SetRect(rankText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(180f, -104f), new Vector2(-220f, 30f));

            TMP_Text priceText = EnsureText(panel.transform, "PriceText", "0 G", 24, TextAlignmentOptions.Left);
            SetRect(priceText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(180f, -142f), new Vector2(-220f, 32f));

            GameObject statsSection = EnsureChild(panel.transform, "StatsSection", typeof(Image));
            SetRect((RectTransform)statsSection.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -244f), new Vector2(-48f, 108f));
            statsSection.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.15f, 0.65f);

            TMP_Text statsText = EnsureText(statsSection.transform, "StatsText", "", 20, TextAlignmentOptions.TopLeft);
            SetRect(statsText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -20f));

            TMP_Text descriptionText = EnsureText(panel.transform, "DescriptionText", "", 20, TextAlignmentOptions.TopLeft);
            SetRect(descriptionText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, -398f), new Vector2(-48f, 150f));

            Button buyButton = EnsureButton(panel.transform, "BuyButton", TextBuy, new Color(0.24f, 0.58f, 0.35f, 1f));
            SetRect((RectTransform)buyButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(88f, 80f), new Vector2(132f, 44f));

            Button equipButton = EnsureButton(panel.transform, "EquipButton", TextEquip, new Color(0.24f, 0.45f, 0.75f, 1f));
            SetRect((RectTransform)equipButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(132f, 44f));

            Button unequipButton = EnsureButton(panel.transform, "UnequipButton", TextUnequip, new Color(0.58f, 0.35f, 0.25f, 1f));
            SetRect((RectTransform)unequipButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-88f, 80f), new Vector2(132f, 44f));

            TMP_Text messageText = EnsureText(panel.transform, "MessageText", "", 20, TextAlignmentOptions.Center);
            SetRect(messageText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 28f), new Vector2(-48f, 28f));

            ShopDetailPanel detailPanel = panel.GetComponent<ShopDetailPanel>();
            SerializedObject serialized = new SerializedObject(detailPanel);
            SetObject(serialized, "iconImage", iconImage);
            SetObject(serialized, "nameText", nameText);
            SetObject(serialized, "rankText", rankText);
            SetObject(serialized, "priceText", priceText);
            SetObject(serialized, "descriptionText", descriptionText);
            SetObject(serialized, "statsText", statsText);
            SetObject(serialized, "statsSection", statsSection);
            SetObject(serialized, "buyButton", buyButton);
            SetObject(serialized, "equipButton", equipButton);
            SetObject(serialized, "unequipButton", unequipButton);
            SetObject(serialized, "messageText", messageText);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return detailPanel;
        }

        private static ShopInventoryPanel BuildSellInventoryPanel(Transform parent)
        {
            GameObject panel = EnsureChild(parent, "InventoryPanel", typeof(Image), typeof(ShopInventoryPanel));
            SetRect((RectTransform)panel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(464f, -400f), new Vector2(-560f, 540f));
            panel.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.24f, 0.96f);

            TMP_Text header = EnsureText(panel.transform, "Header", TextMyFish, 28, TextAlignmentOptions.Left);
            SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -34f), new Vector2(-180f, 36f));

            Button sellAllButton = EnsureButton(panel.transform, "SellAllButton", TextSellAll, new Color(0.68f, 0.42f, 0.22f, 1f));
            SetRect((RectTransform)sellAllButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-84f, -34f), new Vector2(132f, 38f));

            TMP_Text emptyText = EnsureText(panel.transform, "EmptyText", TextNoFish, 22, TextAlignmentOptions.Center);
            SetRect(emptyText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-48f, -96f));

            GameObject scrollView = EnsureChild(panel.transform, "ScrollView", typeof(Image), typeof(ScrollRect));
            SetRect((RectTransform)scrollView.transform, Vector2.zero, Vector2.one, new Vector2(0f, -40f), new Vector2(-48f, -120f));
            scrollView.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.35f);

            GameObject viewport = EnsureChild(scrollView.transform, "Viewport", typeof(Image), typeof(Mask));
            SetRect((RectTransform)viewport.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = EnsureChild(viewport.transform, "Content", typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            SetRect((RectTransform)content.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.content = (RectTransform)content.transform;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            ShopInventoryPanel inventoryPanel = panel.GetComponent<ShopInventoryPanel>();
            SerializedObject serialized = new SerializedObject(inventoryPanel);
            SetObject(serialized, "contentParent", content.transform);
            SetObject(serialized, "slotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ShopInventorySlotPrefabPath));
            SetObject(serialized, "sellAllButton", sellAllButton);
            SetObject(serialized, "emptyText", emptyText);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return inventoryPanel;
        }

        private static ConfirmDialog BuildConfirmDialog(Transform parent)
        {
            GameObject dialog = EnsureChild(parent, "ConfirmDialog", typeof(ConfirmDialog));
            SetRect((RectTransform)dialog.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject dialogRoot = EnsureChild(dialog.transform, "DialogRoot", typeof(Image));
            SetRect((RectTransform)dialogRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dialogRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            GameObject panel = EnsureChild(dialogRoot.transform, "Panel", typeof(Image));
            SetRect((RectTransform)panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 240f));
            panel.GetComponent<Image>().color = new Color(0.94f, 0.91f, 0.84f, 1f);

            TMP_Text titleText = EnsureText(panel.transform, "TitleText", TextConfirm, 28, TextAlignmentOptions.Center);
            titleText.color = Color.black;
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -36f), new Vector2(-48f, 38f));

            TMP_Text messageText = EnsureText(panel.transform, "MessageText", "", 22, TextAlignmentOptions.Center);
            messageText.color = Color.black;
            SetRect(messageText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, -12f), new Vector2(-64f, -118f));

            Button confirmButton = EnsureButton(panel.transform, "ConfirmButton", TextConfirm, new Color(0.24f, 0.55f, 0.32f, 1f));
            SetRect((RectTransform)confirmButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-86f, 38f), new Vector2(130f, 42f));

            Button cancelButton = EnsureButton(panel.transform, "CancelButton", TextCancel, new Color(0.55f, 0.32f, 0.28f, 1f));
            SetRect((RectTransform)cancelButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(86f, 38f), new Vector2(130f, 42f));

            ConfirmDialog confirmDialog = dialog.GetComponent<ConfirmDialog>();
            SerializedObject serialized = new SerializedObject(confirmDialog);
            SetObject(serialized, "dialogRoot", dialogRoot);
            SetObject(serialized, "titleText", titleText);
            SetObject(serialized, "messageText", messageText);
            SetObject(serialized, "confirmButton", confirmButton);
            SetObject(serialized, "cancelButton", cancelButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            dialogRoot.SetActive(false);
            return confirmDialog;
        }

        private static void EnsurePlayScenePlacement()
        {
            if (!File.Exists(PlayScenePath))
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            Scene scene = activeScene.path == PlayScenePath
                ? activeScene
                : EditorSceneManager.OpenScene(PlayScenePath, OpenSceneMode.Single);

            GameObject dynamicCanvas = GameObject.Find("Dynamic UI Canvas");
            if (dynamicCanvas == null)
            {
                return;
            }

            GameObject storeInstance = GameObject.Find("Store");
            GameObject storePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StorePrefabPath);
            if (storeInstance == null && storePrefab != null)
            {
                storeInstance = (GameObject)PrefabUtility.InstantiatePrefab(storePrefab, scene);
                storeInstance.name = "Store";
            }

            if (storeInstance != null)
            {
                storeInstance.transform.SetParent(dynamicCanvas.transform, false);
                storeInstance.SetActive(true);
            }

            GameObject inventoryInstance = GameObject.Find("Inventory 1");
            if (inventoryInstance == null)
            {
                GameObject inventoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventoryPrefabPath);
                if (inventoryPrefab != null)
                {
                    inventoryInstance = (GameObject)PrefabUtility.InstantiatePrefab(inventoryPrefab, scene);
                    inventoryInstance.name = "Inventory 1";
                }
            }

            if (inventoryInstance != null)
            {
                inventoryInstance.transform.SetParent(dynamicCanvas.transform, false);
                inventoryInstance.SetActive(true);
                RepairInventorySceneInstance(inventoryInstance);
            }

            GameObject fishingUIInstance = GameObject.Find("FishingUI");
            if (fishingUIInstance != null)
            {
                RepairFishingUIObject(fishingUIInstance);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RepairInventorySceneInstance(GameObject inventory)
        {
            RemoveComponent<InventorySlotUI>(inventory);

            InventoryUI inventoryUI = EnsureComponent<InventoryUI>(inventory);
            Transform windowRoot = FindDeepChild(inventory.transform, "InventoryPanel");
            Transform scrollView = FindDeepChild(inventory.transform, "Scroll View");
            Transform viewport = scrollView != null ? FindDirectChild(scrollView, "Viewport") : null;
            Transform content = viewport != null ? FindDirectChild(viewport, "Content") : null;

            if (content == null && viewport != null && viewport.childCount > 0)
            {
                content = viewport.GetChild(0);
                content.name = "Content";
            }

            if (content != null)
            {
                DestroyChildren(content);
                RemoveComponent<VerticalLayoutGroup>(content.gameObject);
                GridLayoutGroup grid = EnsureComponent<GridLayoutGroup>(content.gameObject);
                grid.cellSize = new Vector2(150f, 166f);
                grid.spacing = new Vector2(16f, 16f);
                grid.padding = new RectOffset(14, 14, 14, 14);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;
                grid.childAlignment = TextAnchor.UpperLeft;

                ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(content.gameObject);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            Button exitButton = null;
            Transform exitButtonTransform = FindDeepChild(inventory.transform, "Exit_Button");
            if (exitButtonTransform != null)
            {
                exitButton = exitButtonTransform.GetComponent<Button>();
            }

            SerializedObject serialized = new SerializedObject(inventoryUI);
            SetObject(serialized, "windowRoot", windowRoot != null ? windowRoot.gameObject : null);
            SetObject(serialized, "slotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(InventorySlotPrefabPath));
            SetObject(serialized, "contentParent", content);
            SetKeyCode(serialized, "toggleKey", KeyCode.I);
            SetObject(serialized, "exitButton", exitButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button EnsureTabButton(Transform parent, string name, string label, out GameObject highlight)
        {
            Button button = EnsureButton(parent, name, label, new Color(0.22f, 0.28f, 0.30f, 1f));
            LayoutElement layout = EnsureComponent<LayoutElement>(button.gameObject);
            layout.preferredWidth = 160f;
            layout.preferredHeight = 48f;

            highlight = EnsureChild(button.transform, "Highlight", typeof(Image));
            SetRect((RectTransform)highlight.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 3f), new Vector2(-10f, 6f));
            highlight.GetComponent<Image>().color = new Color(0.95f, 0.78f, 0.24f, 1f);
            return button;
        }

        private static Button EnsureButton(Transform parent, string name, string label, Color color)
        {
            GameObject buttonObject = EnsureChild(parent, name, typeof(Image), typeof(Button));
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            TMP_Text text = EnsureText(buttonObject.transform, "Text", label, 20, TextAlignmentOptions.Center);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.color = Color.white;
            return button;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            TMP_Text text = CreateText("Text", buttonObject.transform, label, 20, TextAlignmentOptions.Center);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static GameObject CreateBadge(string name, Transform parent, string label)
        {
            GameObject badge = new GameObject(name, typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(parent, false);
            badge.GetComponent<Image>().color = new Color(0.92f, 0.64f, 0.18f, 1f);

            TMP_Text text = CreateText("Text", badge.transform, label, 14, TextAlignmentOptions.Center);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return badge;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, int size, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TMP_Text tmp = obj.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            GameObject obj = EnsureChild(parent, name, typeof(Image));
            Image image = obj.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text EnsureText(Transform parent, string name, string text, int size, TextAlignmentOptions alignment)
        {
            Transform existing = FindDirectChild(parent, name);
            TMP_Text tmp;
            if (existing == null)
            {
                tmp = CreateText(name, parent, text, size, alignment);
            }
            else
            {
                tmp = existing.GetComponent<TMP_Text>();
                if (tmp == null)
                {
                    tmp = existing.gameObject.AddComponent<TextMeshProUGUI>();
                }
            }

            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
        {
            Transform existing = FindDirectChild(parent, name);
            GameObject obj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                obj.transform.SetParent(parent, false);
            }

            foreach (System.Type componentType in components)
            {
                if (obj.GetComponent(componentType) == null)
                {
                    obj.AddComponent(componentType);
                }
            }

            obj.SetActive(true);
            return obj;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void DestroyChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            return component != null ? component : obj.AddComponent<T>();
        }

        private static RectTransform EnsureRectTransform(GameObject obj)
        {
            RectTransform rect = obj.transform as RectTransform;
            if (rect != null)
            {
                return rect;
            }

            return obj.AddComponent<RectTransform>();
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void DeactivateLegacyChildren(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.name == "WindowRoot")
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private static void RemoveComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component, true);
            }
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string name = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetObject(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetKeyCode(SerializedObject serialized, string propertyName, KeyCode value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = (int)value;
            }
        }
    }
}
