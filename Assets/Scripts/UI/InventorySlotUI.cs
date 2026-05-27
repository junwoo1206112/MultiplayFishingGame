using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;
using System;

namespace MultiplayFishing.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
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

        public event Action<string> onRightClick;

        private InventoryItem itemData;
        private IUserService userService;

        private readonly Color activeStarColor = new Color(1f, 0.84f, 0f);
        private readonly Color inactiveStarColor = new Color(0.5f, 0.5f, 0.5f);

        private readonly Color[] rankColors = new Color[]
        {
            new Color(0.976f, 0.890f, 0.725f),
            new Color(0.769f, 1.0f, 0.780f),
            new Color(0.780f, 0.945f, 1.0f),
            new Color(0.957f, 0.890f, 1.0f),
            new Color(1.0f, 0.843f, 0.0f),
        };

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            this.itemData = item;
            this.userService = userService;

            if (fishInfo != null)
            {
                fishNameText.text = fishInfo.fishName;
                fishIconImage.sprite = fishInfo.fishIcon;

                float randomSize = UnityEngine.Random.Range(fishInfo.minSize, fishInfo.maxSize);
                sizeText.text = $"{randomSize:F1} cm";

                sellPriceText.text = $"{fishInfo.sellPrice:N0} G";
                descriptionText.text = fishInfo.description;

                int starCount = FishDataSO.GetStarCount(fishInfo.rank);
                for (int i = 0; i < starContainer.childCount; i++)
                {
                    var child = starContainer.GetChild(i).GetComponent<Image>();
                    child.color = i < starCount ? activeStarColor : inactiveStarColor;
                }

                if (slotBackground != null)
                {
                    int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                    slotBackground.color = rankColors[rankIndex];
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (itemData != null && !string.IsNullOrEmpty(itemData.instanceId))
                {
                    onRightClick?.Invoke(itemData.instanceId);
                }
            }
        }
    }
}