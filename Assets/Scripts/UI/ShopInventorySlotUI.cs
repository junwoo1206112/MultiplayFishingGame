using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class ShopInventorySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fishIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lengthText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button sellButton;
        [SerializeField] private ConfirmDialog confirmDialog;

        private InventoryItem itemData;
        private IUserService userService;

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            this.itemData = item;
            this.userService = userService;

            if (fishInfo != null)
            {
                if (nameText != null) nameText.text = fishInfo.fishName;
                if (fishIcon != null) fishIcon.sprite = fishInfo.fishIcon;
                if (priceText != null) priceText.text = $"{fishInfo.sellPrice:N0} G";
            }

            if (lengthText != null) lengthText.text = $"{item.length:F1} cm";

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }
        }

        private void OnSellClicked()
        {
            var fishInfo = DIContainer.Resolve<IDataService>().GetFishData(itemData.fishId);
            string fishName = fishInfo != null ? fishInfo.fishName : "물고기";
            int price = fishInfo != null ? fishInfo.sellPrice : 0;

            if (confirmDialog != null)
            {
                confirmDialog.Show(
                    "판매 확인",
                    $"{fishName}을(를) {price:N0} G에 판매하시겠습니까?",
                    () => userService.SellFish(itemData.instanceId)
                );
            }
            else
            {
                userService.SellFish(itemData.instanceId);
            }
        }
    }
}
