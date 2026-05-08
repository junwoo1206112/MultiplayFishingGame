using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

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

        public void Setup(string itemName, Sprite icon, string rank, int price, bool owned, bool equipped, UnityAction onClick)
        {
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

        public void Select()
        {
            if (slotButton != null)
                slotButton.onClick.Invoke();
        }
    }
}
