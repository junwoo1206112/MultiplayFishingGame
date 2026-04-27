using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text expText;
        [SerializeField] private Slider expBar;
        [SerializeField] private TMP_Text goldText;

        private IUserService userService;

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
            userService.OnDataChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (userService != null)
                userService.OnDataChanged -= RefreshUI;
        }

        public void RefreshUI()
        {
            var data = userService.UserData;
            
            if (tierText != null) tierText.text = $"Tier {data.currentTier}";
            if (goldText != null) goldText.text = $"{data.gold:N0} G";

            if (expBar != null)
            {
                int nextExp = data.GetExpForNextTier();
                expBar.maxValue = nextExp;
                expBar.value = data.currentExp;
                
                if (expText != null) expText.text = $"{data.currentExp} / {nextExp}";
            }
        }
    }
}
