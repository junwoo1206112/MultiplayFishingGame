using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayFishing.UI
{
    public class TabButton : MonoBehaviour
    {
        [Header("Top Tab Settings")]
        [SerializeField] private Transform tabGroup;
        [SerializeField] private Color selectedColor = new Color(0.847f, 0.918f, 0.180f);
        [SerializeField] private GameObject exitButtonObject;

        [Header("Side Tab Settings")]
        [SerializeField] private Transform sideTabGroup;

        [Header("Side Tab Panel Settings")]
        [SerializeField] private Image sideTabPanelImage;
        [SerializeField] private Color sideTabPanelSelectedColor = new Color(1.0f, 0.894f, 0.835f);
        [SerializeField] private int sideTabPanelChangeIndex = 1;

        private Button[] topButtons;
        private Image[] topImages;
        private Dictionary<int, Color> topOriginalColors = new Dictionary<int, Color>();
        private int currentTopIndex = -1;

        private Button[] sideButtons;
        private int currentSideIndex = -1;

        private Color cachedPanelOriginalColor;

        public event System.Action<int> OnTopTabIndexChanged;
        public event System.Action<int> OnSideTabIndexChanged;

        public Button[] TopButtons => topButtons;
        public Button[] SideButtons => sideButtons;
        public int CurrentTopIndex => currentTopIndex;
        public int CurrentSideIndex => currentSideIndex;

        private void Start()
        {
            SetupTopTabs();
            SetupSideTabs();
        }

        private void SetupTopTabs()
        {
            if (tabGroup == null) return;

            int topCount = tabGroup.childCount;
            topButtons = new Button[topCount];
            topImages = new Image[topCount];

            for (int i = 0; i < topCount; i++)
            {
                topButtons[i] = tabGroup.GetChild(i).GetComponent<Button>();
                topImages[i] = topButtons[i].GetComponent<Image>();
                topOriginalColors[i] = topImages[i].color;

                int index = i;
                topButtons[i].onClick.AddListener(() => OnTopTabSelected(index));
            }
        }

        private void SetupSideTabs()
        {
            if (sideTabGroup == null) return;

            if (sideTabPanelImage != null)
            {
                cachedPanelOriginalColor = sideTabPanelImage.color;
            }

            int sideCount = sideTabGroup.childCount;
            sideButtons = new Button[sideCount];

            for (int i = 0; i < sideCount; i++)
            {
                sideButtons[i] = sideTabGroup.GetChild(i).GetComponent<Button>();

                int index = i;
                sideButtons[i].onClick.AddListener(() => OnSideTabSelected(index));
            }
        }

        private void OnTopTabSelected(int index)
        {
            if (currentTopIndex == index) return;
            currentTopIndex = index;

            for (int i = 0; i < topImages.Length; i++)
            {
                if (topButtons[i].gameObject == exitButtonObject) continue;
                topImages[i].color = i == index ? selectedColor : topOriginalColors[i];
            }

            HandleSideTabsVisibility(index);
            OnTopTabIndexChanged?.Invoke(index);
        }

        private void OnSideTabSelected(int index)
        {
            if (currentSideIndex == index) return;
            currentSideIndex = index;

            HandlePanelColor(index);
            OnSideTabIndexChanged?.Invoke(index);
        }

        private void HandleSideTabsVisibility(int topIndex)
        {
            if (sideTabGroup == null) return;

            bool showSideTabs = topIndex == 1;
            sideTabGroup.gameObject.SetActive(showSideTabs);

            if (showSideTabs)
            {
                ResetSideTabSelection();
            }
        }

        private void ResetSideTabSelection()
        {
            currentSideIndex = -1;
            if (sideTabPanelImage != null)
            {
                sideTabPanelImage.color = cachedPanelOriginalColor;
            }
        }

        private void HandlePanelColor(int sideIndex)
        {
            if (sideTabPanelImage == null) return;

            if (sideIndex == sideTabPanelChangeIndex)
            {
                sideTabPanelImage.color = sideTabPanelSelectedColor;
            }
            else
            {
                sideTabPanelImage.color = cachedPanelOriginalColor;
            }
        }
    }
}