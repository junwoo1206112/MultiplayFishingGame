using UnityEngine;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject chargingPanel;
        [SerializeField] private GameObject catchingPanel;
        [SerializeField] private GameObject alertPanel;

        public event System.Action<FishingState> OnPanelStateChanged;

        private void Start()
        {
            HideAllPanels();
        }

        private void OnEnable()
        {
            FindFishingController();
        }

        private void OnDestroy()
        {
        }

        private void FindFishingController()
        {
            var players = FindObjectsByType<FishingPlayer>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.isLocalPlayer)
                {
                    var controller = p.GetComponent<FishingController>();
                    if (controller != null)
                    {
                        controller.OnStateChanged += HandleStateChanged;
                    }
                    break;
                }
            }
        }

        private void HandleStateChanged(FishingState state)
        {
            HideAllPanels();

            switch (state)
            {
                case FishingState.Charging:
                    if (chargingPanel) chargingPanel.SetActive(true);
                    break;
                case FishingState.Nibble:
                    if (alertPanel) alertPanel.SetActive(true);
                    break;
                case FishingState.Catching:
                    if (catchingPanel) catchingPanel.SetActive(true);
                    break;
            }

            OnPanelStateChanged?.Invoke(state);
        }

        private void HideAllPanels()
        {
            if (chargingPanel) chargingPanel.SetActive(false);
            if (catchingPanel) catchingPanel.SetActive(false);
            if (alertPanel) alertPanel.SetActive(false);
        }
    }
}