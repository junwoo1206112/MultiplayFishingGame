using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fishIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lengthText;
        [SerializeField] private Button sellButton;

        private InventoryItem itemData;
        private IUserService userService;

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            this.itemData = item;
            this.userService = userService;

            if (fishInfo != null)
            {
                nameText.text = fishInfo.fishName;
                fishIcon.sprite = fishInfo.fishIcon;
            }

            lengthText.text = $"{item.length:F1} cm";

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
