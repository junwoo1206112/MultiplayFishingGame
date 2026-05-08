using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.UI
{
    public class ShopInventoryPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button sellAllButton;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private ConfirmDialog confirmDialog;

        private IUserService userService;
        private IDataService dataService;
        private List<ShopInventorySlotUI> activeSlots = new List<ShopInventorySlotUI>();

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            userService.OnDataChanged += RefreshList;

            if (sellAllButton != null)
                sellAllButton.onClick.AddListener(OnSellAllClicked);
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= RefreshList;
        }

        public void RefreshList()
        {
            if (contentParent == null || slotPrefab == null) return;

            foreach (var slot in activeSlots)
                Destroy(slot.gameObject);
            activeSlots.Clear();

            var inventory = userService.UserData.inventory;

            if (emptyText != null)
                emptyText.gameObject.SetActive(inventory.Count == 0);

            if (sellAllButton != null)
                sellAllButton.gameObject.SetActive(inventory.Count > 0);

            foreach (var item in inventory)
            {
                var slotGO = Instantiate(slotPrefab, contentParent);
                var slotUI = slotGO.GetComponent<ShopInventorySlotUI>();
                if (slotUI != null)
                {
                    var fishInfo = dataService.GetFishData(item.fishId);
                    slotUI.Setup(item, fishInfo, userService);
                    activeSlots.Add(slotUI);
                }
            }
        }

        private void OnSellAllClicked()
        {
            var inventory = userService.UserData.inventory;
            if (inventory.Count == 0) return;

            int totalGain = 0;
            foreach (var item in inventory)
            {
                var fishInfo = dataService.GetFishData(item.fishId);
                if (fishInfo != null)
                    totalGain += fishInfo.sellPrice;
            }

            if (confirmDialog != null)
            {
                confirmDialog.Show(
                    "전체 판매",
                    $"보유한 모든 물고기를 판매합니다.\n총 {inventory.Count}마리, 예상 수익: {totalGain:N0} G",
                    () => userService.SellAllFish()
                );
            }
            else
            {
                userService.SellAllFish();
            }
        }
    }
}
