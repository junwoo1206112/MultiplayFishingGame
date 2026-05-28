using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Slot Button")]
        [SerializeField] private Button slotButton;

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
        private FishDataSO fishInfo;
        private IUserService userService;

        public Action onSlotClicked;

        private readonly Color activeStarColor = new Color(1f, 0.84f, 0f);
        private readonly Color inactiveStarColor = new Color(0.5f, 0.5f, 0.5f);

        private readonly Color[] rankColors = new Color[]
        {
            new Color(0.976f, 0.890f, 0.725f),
            new Color(0.769f, 1.0f, 0.780f),
            new Color(0.780f, 0.945f, 1.0f),
            new Color(0.957f, 0.890f, 1.0f),
            new Color(0.957f, 0.890f, 1.0f),
        };

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            this.itemData = item;
            this.fishInfo = fishInfo;
            this.userService = userService;

            if (fishInfo != null)
            {
                if (fishNameText != null) fishNameText.text = fishInfo.fishName;
                if (fishIconImage != null) fishIconImage.sprite = fishInfo.fishIcon;
                if (sizeText != null) sizeText.text = $"{item.length:F1} cm";
                if (sellPriceText != null)
                {
                    sellPriceText.text = $"{fishInfo.sellPrice:N0} G";
                    sellPriceText.gameObject.SetActive(true);
                }
                if (descriptionText != null) descriptionText.text = fishInfo.description;

                int starCount = FishDataSO.GetStarCount(fishInfo.rank);
                SetStarCount(starCount);

                if (slotBackground != null)
                {
                    int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                    slotBackground.color = rankColors[rankIndex];
                }
            }

            if (starContainer != null) starContainer.gameObject.SetActive(true);

            if (sellButton != null)
            {
                SetSellButtonLabel("판매");
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSlotClicked?.Invoke());
            }
        }

        public void SetupRod(RodDataSO rodData, bool isEquipped, IUserService userService)
        {
            this.userService = userService;

            if (fishIconImage != null) fishIconImage.sprite = rodData.icon;
            if (fishNameText != null) fishNameText.text = rodData.rodName;
            if (sizeText != null) sizeText.text = $"거리 +{rodData.castDistanceBonus:F1}m\n확률 +{rodData.catchChanceBonus * 100:F0}%";
            if (sellPriceText != null)
            {
                sellPriceText.text = $"{rodData.price:N0} G";
                sellPriceText.gameObject.SetActive(true);
            }
            if (descriptionText != null) descriptionText.text = rodData.description;

            int starCount = FishDataSO.GetStarCount(rodData.rank);
            SetStarCount(starCount);

            if (slotBackground != null)
            {
                int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                slotBackground.color = rankColors[rankIndex];
            }

            if (starContainer != null) starContainer.gameObject.SetActive(true);

            if (sellButton != null)
            {
                SetSellButtonLabel(isEquipped ? "장착 해제" : "장착");
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                if (isEquipped)
                    sellButton.onClick.AddListener(() => userService.UnequipRod());
                else
                    sellButton.onClick.AddListener(() => userService.EquipRod(rodData.id));
            }

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSlotClicked?.Invoke());
            }
        }

        public void SetupBait(BaitDataSO baitData, bool isEquipped, IUserService userService)
        {
            this.userService = userService;

            if (fishIconImage != null) fishIconImage.sprite = baitData.icon;
            if (fishNameText != null) fishNameText.text = baitData.baitName;
            if (sizeText != null) sizeText.text = $"확률 +{baitData.catchChanceBonus * 100:F0}%\n유인 {baitData.attractionFishIds.Length}종";
            if (sellPriceText != null)
            {
                sellPriceText.text = $"{baitData.price:N0} G";
                sellPriceText.gameObject.SetActive(true);
            }
            if (descriptionText != null) descriptionText.text = baitData.description;

            int starCount = FishDataSO.GetStarCount(baitData.rank);
            SetStarCount(starCount);

            if (slotBackground != null)
            {
                int rankIndex = Mathf.Clamp(starCount - 1, 0, rankColors.Length - 1);
                slotBackground.color = rankColors[rankIndex];
            }

            if (starContainer != null) starContainer.gameObject.SetActive(true);

            if (sellButton != null)
            {
                SetSellButtonLabel(isEquipped ? "장착 해제" : "장착");
                sellButton.gameObject.SetActive(true);
                sellButton.onClick.RemoveAllListeners();
                if (isEquipped)
                    sellButton.onClick.AddListener(() => userService.UnequipBait());
                else
                    sellButton.onClick.AddListener(() => userService.EquipBait(baitData.id));
            }

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSlotClicked?.Invoke());
            }
        }

        private void SetStarCount(int count)
        {
            if (starContainer == null) return;
            for (int i = 0; i < starContainer.childCount; i++)
            {
                var child = starContainer.GetChild(i).GetComponent<Image>();
                if (child != null)
                    child.color = i < count ? activeStarColor : inactiveStarColor;
            }
        }

        private void SetSellButtonLabel(string label)
        {
            var tmpText = sellButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) tmpText.text = label;
        }

        private void OnSellClicked()
        {
            userService.SellFish(itemData.instanceId);
        }
    }
}
