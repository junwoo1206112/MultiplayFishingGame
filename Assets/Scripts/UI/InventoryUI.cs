using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Infinite Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private float slotHeight = 110f;
        [SerializeField] private float slotSpacing = 10f;
        [SerializeField] private int columnCount = 4;
        [SerializeField] private float scrollThreshold = 5f;

        private IUserService userService;
        private IDataService dataService;
        private GridLayoutGroup gridLayoutGroup;

        private Pool<InventorySlotUI> slotPool;
        private System.Collections.Generic.List<InventorySlotUI> activeSlots = new System.Collections.Generic.List<InventorySlotUI>();
        private int currentStartIndex = 0;
        private int totalCount = 0;
        private int bufferCount = 2;
        private float lastScrollY = 0f;

        private float TotalSlotHeight => slotHeight + slotSpacing;
        private int VisibleRowCount => Mathf.CeilToInt(viewportRect.rect.height / TotalSlotHeight) + bufferCount;
        private int VisibleSlotCount => VisibleRowCount * columnCount;

        private void Awake()
        {
            gridLayoutGroup = contentRect.GetComponent<GridLayoutGroup>();
        }

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            slotPool = new Pool<InventorySlotUI>(
                () => CreateNewSlot(),
                VisibleSlotCount + bufferCount * columnCount
            );

            scrollRect.onValueChanged.AddListener(OnScrollChanged);

            userService.OnDataChanged += OnInventoryChanged;

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

        private void OnInventoryChanged()
        {
            if (windowRoot != null && windowRoot.activeSelf)
            {
                RefreshList();
            }
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= OnInventoryChanged;
            if (scrollRect != null)
                scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
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

        private InventorySlotUI CreateNewSlot()
        {
            GameObject obj = Instantiate(slotPrefab, contentRect);
            obj.SetActive(false);
            return obj.GetComponent<InventorySlotUI>();
        }

        private void OnScrollChanged(Vector2 pos)
        {
            float currentY = contentRect.anchoredPosition.y;
            if (System.Math.Abs(currentY - lastScrollY) > scrollThreshold)
            {
                lastScrollY = currentY;
                RefreshVisibleSlots();
            }
        }

        public void RefreshList()
        {
            totalCount = userService.UserData.inventory.Count;

            int rowCount = Mathf.CeilToInt(totalCount / (float)columnCount);
            float contentHeight = rowCount * TotalSlotHeight;
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1f, contentHeight));

            RefreshVisibleSlots();
        }

        private void RefreshVisibleSlots()
        {
            if (totalCount == 0)
            {
                ReturnAllSlotsToPool();
                return;
            }

            float contentTop = contentRect.anchoredPosition.y;
            int startRow = Mathf.Max(0, Mathf.FloorToInt(contentTop / TotalSlotHeight));
            int startIndex = startRow * columnCount;
            startIndex = System.Math.Min(startIndex, System.Math.Max(0, totalCount - VisibleSlotCount));

            if (startIndex == currentStartIndex && activeSlots.Count == System.Math.Min(VisibleSlotCount, totalCount - startIndex))
                return;

            gridLayoutGroup.enabled = false;
            ReturnAllSlotsToPool();
            currentStartIndex = startIndex;

            int endIndex = System.Math.Min(currentStartIndex + VisibleSlotCount, totalCount);

            for (int i = currentStartIndex; i < endIndex; i++)
            {
                InventorySlotUI slot = slotPool.Get();
                slot.gameObject.SetActive(true);
                activeSlots.Add(slot);
                BindSlotData(slot, i);
            }

            gridLayoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        private void BindSlotData(InventorySlotUI slot, int index)
        {
            var inventory = userService.UserData.inventory;

            if (index < 0 || index >= inventory.Count)
            {
                slot.gameObject.SetActive(false);
                return;
            }

            var item = inventory[index];
            var fishInfo = dataService.GetFishData(item.fishId);
            slot.Setup(item, fishInfo, userService);
        }

        private void ReturnAllSlotsToPool()
        {
            for (int i = activeSlots.Count - 1; i >= 0; i--)
            {
                InventorySlotUI slot = activeSlots[i];
                slot.gameObject.SetActive(false);
                slotPool.Return(slot);
            }
            activeSlots.Clear();
        }
    }
}