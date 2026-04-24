using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class FishingUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject chargingPanel;
        [SerializeField] private GameObject catchingPanel;
        [SerializeField] private GameObject alertPanel; // "!" 아이콘용

        [Header("Charging UI")]
        [SerializeField] private Slider chargingBar;

        [Header("Catching UI")]
        [SerializeField] private Slider catchingBar;
        [SerializeField] private TMP_Text catchingText;

        private FishingController targetController;

        private void Start()
        {
            // 로컬 플레이어가 생성될 때까지 대기하거나 찾음
            FindLocalFishingController();
            
            // 초기 상태: 모두 숨김
            if (chargingPanel) chargingPanel.SetActive(false);
            if (catchingPanel) catchingPanel.SetActive(false);
            if (alertPanel) alertPanel.SetActive(false);
        }

        private void FindLocalFishingController()
        {
            // 실제 환경에서는 FishingPlayer가 생성된 후 Controller를 주입하거나 
            // 이벤트를 통해 전달받는 것이 좋으나, 여기서는 간단히 검색 방식을 사용합니다.
            var players = FindObjectsByType<FishingPlayer>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.isLocalPlayer)
                {
                    targetController = p.GetComponent<FishingController>();
                    if (targetController != null)
                    {
                        targetController.OnStateChanged += HandleStateChanged;
                        targetController.OnChargeProgressChanged += UpdateChargeBar;
                        targetController.OnCatchProgressChanged += UpdateCatchBar;
                    }
                    break;
                }
            }
        }

        private void Update()
        {
            if (targetController == null)
            {
                FindLocalFishingController();
            }
        }

        private void OnDestroy()
        {
            if (targetController != null)
            {
                targetController.OnStateChanged -= HandleStateChanged;
                targetController.OnChargeProgressChanged -= UpdateChargeBar;
                targetController.OnCatchProgressChanged -= UpdateCatchBar;
            }
        }

        private void HandleStateChanged(FishingState state)
        {
            if (chargingPanel) chargingPanel.SetActive(state == FishingState.Charging);
            if (catchingPanel) catchingPanel.SetActive(state == FishingState.Catching);
            if (alertPanel) alertPanel.SetActive(state == FishingState.Nibble);

            // 입질 시 효과음 재생 등 추가 가능
            if (state == FishingState.Nibble)
            {
                Debug.Log("입질! 빨리 낚으세요!");
            }
        }

        private void UpdateChargeBar(float progress)
        {
            if (chargingBar) chargingBar.value = progress;
        }

        private void UpdateCatchBar(float current, float target)
        {
            if (catchingBar)
            {
                catchingBar.maxValue = target;
                catchingBar.value = current;
            }

            if (catchingText)
            {
                catchingText.text = $"연타!! ({current} / {target})";
            }
        }
    }
}
