using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class ShopDetailPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private GameObject statsSection;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        [SerializeField] private TMP_Text messageText;

        private ShopUI shopUI;
        private string currentItemId;
        private ShopItemType currentItemType;

        private void Start()
        {
            shopUI = GetComponentInParent<ShopUI>();
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
            if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
            if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipClicked);
            Clear();
        }

        public void ShowRodDetail(RodDataSO rod, bool owned, bool equipped)
        {
            currentItemId = rod.id;
            currentItemType = ShopItemType.Rod;

            gameObject.SetActive(true);
            if (iconImage != null) iconImage.sprite = rod.icon;
            if (nameText != null) nameText.text = rod.rodName;
            if (rankText != null) rankText.text = rod.rank;
            if (priceText != null) priceText.text = owned ? "소유함" : $"{rod.price:N0} G";
            if (descriptionText != null) descriptionText.text = rod.description;
            if (statsSection != null) statsSection.SetActive(true);
            if (statsText != null)
                statsText.text = $"캐스팅 거리: +{rod.castDistanceBonus}m\n포획 확률: +{rod.catchChanceBonus}%\n내구도: {rod.durability}";

            if (buyButton != null) buyButton.gameObject.SetActive(!owned);
            if (equipButton != null) equipButton.gameObject.SetActive(owned && !equipped);
            if (unequipButton != null) unequipButton.gameObject.SetActive(equipped);
            HideMessage();
        }

        public void ShowBaitDetail(BaitDataSO bait, bool owned, bool equipped)
        {
            currentItemId = bait.id;
            currentItemType = ShopItemType.Bait;

            gameObject.SetActive(true);
            if (iconImage != null) iconImage.sprite = bait.icon;
            if (nameText != null) nameText.text = bait.baitName;
            if (rankText != null) rankText.text = bait.rank;
            if (priceText != null) priceText.text = owned ? "소유함" : $"{bait.price:N0} G";
            if (descriptionText != null) descriptionText.text = bait.description;
            if (statsSection != null) statsSection.SetActive(true);
            if (statsText != null)
            {
                string attractionInfo = (bait.attractionFishIds == null || bait.attractionFishIds.Length == 0)
                    ? "모든 물고기"
                    : string.Join(", ", bait.attractionFishIds);
                statsText.text = $"유인: {attractionInfo}\n포획 확률: +{bait.catchChanceBonus}%";
            }

            if (buyButton != null) buyButton.gameObject.SetActive(!owned);
            if (equipButton != null) equipButton.gameObject.SetActive(owned && !equipped);
            if (unequipButton != null) unequipButton.gameObject.SetActive(equipped);
            HideMessage();
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            currentItemId = null;
        }

        public void ShowMessage(string msg)
        {
            if (messageText != null)
            {
                messageText.text = msg;
                messageText.gameObject.SetActive(true);
            }
        }

        private void HideMessage()
        {
            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }

        private void OnBuyClicked()
        {
            if (shopUI != null && !string.IsNullOrEmpty(currentItemId))
                shopUI.OnBuyClicked(currentItemId, currentItemType);
        }

        private void OnEquipClicked()
        {
            if (shopUI != null && !string.IsNullOrEmpty(currentItemId))
                shopUI.OnEquipClicked(currentItemId, currentItemType);
        }

        private void OnUnequipClicked()
        {
            if (shopUI != null)
                shopUI.OnUnequipClicked(currentItemType);
        }
    }
}
