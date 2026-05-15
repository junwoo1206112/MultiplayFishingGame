using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Fish Info")]
        [SerializeField] private Image fishIconImage;
        [SerializeField] private TMP_Text fishNameText;
        [SerializeField] private TMP_Text sizeText;
        [SerializeField] private TMP_Text sellPriceText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Rank Stars")]
        [SerializeField] private RectTransform starContainer;
        [SerializeField] private int maxStars = 5;

        [Header("Slot Background")]
        [SerializeField] private Image slotBackground;

        [Header("Sell Button")]
        [SerializeField] private Button sellButton;

        private InventoryItem itemData;
        private IUserService userService;

        private readonly Color activeStarColor = new Color(1f, 0.84f, 0f);
        private readonly Color inactiveStarColor = new Color(0.5f, 0.5f, 0.5f);

        // Grade 1~5 background colors
        private readonly Color[] rankColors = new Color[]
        {
            new Color(0.976f, 0.890f, 0.725f),  // #f9e3b9 - Grade 1
            new Color(0.769f, 1.0f, 0.780f),     // #c4ffc7 - Grade 2
            new Color(0.780f, 0.945f, 1.0f),     // #c7f1ff - Grade 3
            new Color(0.957f, 0.890f, 1.0f),     // #f4e3ff - Grade 4
            new Color(0.957f, 0.890f, 1.0f),     // #f4e3ff - Grade 5
        };

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            this.itemData = item;
            this.userService = userService;

            if (fishInfo != null)
            {
                if (fishNameText != null) fishNameText.text = fishInfo.fishName;
                if (fishIconImage != null) fishIconImage.sprite = fishInfo.fishIcon;
                if (sizeText != null) sizeText.text = $"{item.length:F1} cm";
                if (sellPriceText != null) sellPriceText.text = $"{fishInfo.sellPrice:N0} G";
                if (descriptionText != null) descriptionText.text = fishInfo.description;

                // 별 색상 적용
                int starCount = FishDataSO.GetStarCount(fishInfo.rank);
                if (starContainer != null)
                {
                    for (int i = 0; i < starContainer.childCount; i++)
                    {
                        var child = starContainer.GetChild(i).GetComponent<Image>();
                        if (child != null)
                            child.color = i < starCount ? activeStarColor : inactiveStarColor;
                    }
                }

                // 슬롯 배경색 (등급별)
                if (slotBackground != null)
                {
                    int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                    slotBackground.color = rankColors[rankIndex];
                }
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }
        }

        private void OnSellClicked()
        {
            userService.SellFish(itemData.instanceId);
        }
    }
}
