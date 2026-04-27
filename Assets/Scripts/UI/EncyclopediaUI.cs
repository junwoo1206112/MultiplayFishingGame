using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.UI
{
    public class EncyclopediaUI : MonoBehaviour
    {
        [Header("Window Controls")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.E;

        [Header("Progress UI")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Slider progressBar;

        [Header("Grid List")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform gridParent;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private Image detailSilhouette;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailRank;
        [SerializeField] private TMP_Text detailDesc;
        [SerializeField] private TMP_Text detailMaxRecord;

        private IUserService userService;
        private IDataService dataService;
        private List<EncyclopediaSlotUI> activeSlots = new List<EncyclopediaSlotUI>();

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            dataService = DIContainer.Resolve<IDataService>();

            if (windowRoot != null) windowRoot.SetActive(false);
            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleWindow();
            }
        }

        public void ToggleWindow()
        {
            if (windowRoot == null) return;
            bool nextState = !windowRoot.activeSelf;
            windowRoot.SetActive(nextState);

            if (nextState)
            {
                RefreshUI();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void RefreshUI()
        {
            // 1. 기존 슬롯 정리
            foreach (var slot in activeSlots) Destroy(slot.gameObject);
            activeSlots.Clear();

            // 2. 전체 데이터 가져오기
            var allFish = dataService.GetAllFishData();
            int discoveredCount = 0;

            foreach (var fish in allFish)
            {
                bool discovered = userService.UserData.IsDiscovered(fish.id);
                if (discovered) discoveredCount++;

                GameObject obj = Instantiate(slotPrefab, gridParent);
                var slotUI = obj.GetComponent<EncyclopediaSlotUI>();
                slotUI.Setup(fish, discovered, OnSlotSelected);
                activeSlots.Add(slotUI);
            }

            // 3. 진행도 갱신
            float progress = (float)discoveredCount / allFish.Count;
            if (progressText != null) progressText.text = $"수집 진행도: {discoveredCount} / {allFish.Count} ({progress * 100:F0}%)";
            if (progressBar != null) progressBar.value = progress;
        }

        private void OnSlotSelected(FishDataSO data, bool isDiscovered)
        {
            if (detailPanel == null) return;
            detailPanel.SetActive(true);

            if (isDiscovered)
            {
                detailName.text = data.fishName;
                detailRank.text = $"{data.rank} Grade";
                detailDesc.text = data.description;
                detailIcon.sprite = data.fishIcon;
                detailIcon.color = Color.white;
                detailSilhouette.gameObject.SetActive(false);

                // 최고 기록 가져오기
                var record = userService.UserData.GetRecord(data.id);
                detailMaxRecord.text = $"최대 기록: {record.maxRecord:F1} cm";
            }
            else
            {
                detailName.text = "???";
                detailRank.text = "? Grade";
                detailDesc.text = "아직 발견하지 못한 물고기입니다.";
                detailIcon.sprite = data.fishIcon;
                detailIcon.color = Color.black; // 실루엣 처리
                detailSilhouette.gameObject.SetActive(true);
                detailMaxRecord.text = "최대 기록: -- cm";
            }
        }
    }
}
