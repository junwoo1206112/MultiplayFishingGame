using UnityEngine;
using UnityEngine.UI;
using MultiplayFishing.Data.Models;
using System;

namespace MultiplayFishing.UI
{
    public class EncyclopediaSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fishIcon;
        [SerializeField] private Image silhouetteOverlay; // 검은색 실루엣용 덮개
        [SerializeField] private Button clickButton;

        private FishDataSO fishData;
        private bool isDiscovered;
        private Action<FishDataSO, bool> onSlotClicked;

        public void Setup(FishDataSO data, bool discovered, Action<FishDataSO, bool> callback)
        {
            this.fishData = data;
            this.isDiscovered = discovered;
            this.onSlotClicked = callback;

            if (fishIcon != null)
            {
                fishIcon.sprite = data.fishIcon;
                // 미발견 시 어둡게 또는 실루엣 처리
                silhouetteOverlay.gameObject.SetActive(!discovered);
            }

            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(() => onSlotClicked?.Invoke(fishData, isDiscovered));
            }
        }
    }
}
