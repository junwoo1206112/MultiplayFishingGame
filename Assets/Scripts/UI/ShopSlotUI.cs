using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject ownedBadge;
        [SerializeField] private GameObject equippedBadge;
        [SerializeField] private Button slotButton;

        private UnityAction onClick;

        private RodDataSO rodData;
        private BaitDataSO baitData;

        public void Setup(string itemName, Sprite icon, string rank, int price, bool owned, bool equipped, UnityAction onClick)
        {
            rodData = null;
            baitData = null;
            nameText.text = itemName;
            if (iconImage != null) iconImage.sprite = icon;
            rankText.text = rank;
            priceText.text = owned ? "" : $"{price:N0} G";
            ownedBadge.SetActive(owned && !equipped);
            equippedBadge.SetActive(equipped);

            this.onClick = onClick;
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(onClick);
            }
        }

        public void Setup(RodDataSO rod, bool owned, bool equipped, UnityAction<RodDataSO> onSelected)
        {
            rodData = rod;
            baitData = null;
            nameText.text = rod.rodName;
            if (iconImage != null) iconImage.sprite = rod.icon;
            rankText.text = rod.rank;
            priceText.text = owned ? "" : $"{rod.price:N0} G";
            ownedBadge.SetActive(owned && !equipped);
            equippedBadge.SetActive(equipped);

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSelected(rod));
            }
        }

        public void Setup(BaitDataSO bait, bool owned, bool equipped, UnityAction<BaitDataSO> onSelected)
        {
            rodData = null;
            baitData = bait;
            nameText.text = bait.baitName;
            if (iconImage != null) iconImage.sprite = bait.icon;
            rankText.text = bait.rank;
            priceText.text = owned ? "" : $"{bait.price:N0} G";
            ownedBadge.SetActive(owned && !equipped);
            equippedBadge.SetActive(equipped);

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSelected(bait));
            }
        }

        public void Select()
        {
            if (slotButton != null)
                slotButton.onClick.Invoke();
        }
    }
}
