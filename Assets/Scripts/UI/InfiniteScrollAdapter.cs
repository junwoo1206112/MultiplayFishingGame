using UnityEngine;
using UnityEngine.UI;

namespace MultiplayFishing.UI
{
    public class InfiniteScrollAdapter : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private float itemHeight = 100f;
        [SerializeField] private float itemSpacing = 10f;
        [SerializeField] private int columnCount = 1;
        [SerializeField] private int bufferCount = 2;

        private System.Collections.Generic.List<MonoBehaviour> activeItems = new System.Collections.Generic.List<MonoBehaviour>();
        private int totalCount = 0;
        private float lastScrollY = 0f;
        private float scrollThreshold = 5f;

        private float TotalItemHeight => itemHeight + itemSpacing;
        private int VisibleRowCount => Mathf.CeilToInt(viewportRect.rect.height / TotalItemHeight) + bufferCount;
        private int VisibleItemCount => VisibleRowCount * columnCount;

        private void Start()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrollChanged);
            }
        }

        private void OnScrollChanged(Vector2 pos)
        {
            float currentY = contentRect.anchoredPosition.y;
            if (System.Math.Abs(currentY - lastScrollY) > scrollThreshold)
            {
                lastScrollY = currentY;
                RefreshVisibleItems();
            }
        }

        public void SetTotalCount(int count)
        {
            totalCount = count;
            float contentHeight = Mathf.CeilToInt(totalCount / (float)columnCount) * TotalItemHeight;
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1f, contentHeight));
            RefreshVisibleItems();
        }

        private void RefreshVisibleItems()
        {
            if (totalCount == 0) return;

            float contentTop = contentRect.anchoredPosition.y;
            int startRow = Mathf.Max(0, Mathf.FloorToInt(contentTop / TotalItemHeight));
            int startIndex = startRow * columnCount;
            startIndex = System.Math.Min(startIndex, System.Math.Max(0, totalCount - VisibleItemCount));

            ReturnAllItemsToPool();

            int endIndex = System.Math.Min(startIndex + VisibleItemCount, totalCount);

            for (int i = startIndex; i < endIndex; i++)
            {
                CreateItem(i);
            }
        }

        private void CreateItem(int index)
        {
            if (itemPrefab == null) return;

            GameObject obj = Instantiate(itemPrefab, contentRect);
            obj.SetActive(true);
            MonoBehaviour component = obj.GetComponent<MonoBehaviour>();
            if (component != null)
            {
                activeItems.Add(component);
            }
        }

        private void ReturnAllItemsToPool()
        {
            foreach (var item in activeItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            activeItems.Clear();
        }

        private void OnDestroy()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
            }
            ReturnAllItemsToPool();
        }
    }
}