using UnityEngine;
using UnityEngine.UI;
<<<<<<< HEAD
using TMPro;
=======
using System.Collections;
>>>>>>> origin/Map
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class FishingUI : MonoBehaviour
    {
<<<<<<< HEAD
        [Header("Panels")]
        [SerializeField] private GameObject chargingPanel;
        [SerializeField] private GameObject catchingPanel;
        [SerializeField] private GameObject alertPanel; // "!" 아이콘용

=======
>>>>>>> origin/Map
        [Header("Charging UI")]
        [SerializeField] private Slider chargingBar;

        [Header("Catching UI")]
<<<<<<< HEAD
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
=======
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform borderRect;
        [SerializeField] private float fillAmount = 0.1f;
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private float shakeDuration = 0.15f;

        [Header("Timer Settings")]
        [SerializeField] private float timerDuration = 120f;
        [SerializeField] private Image tFillImage;

        [Header("Colors")]
        private Color colorLow = new Color(0.408f, 0.741f, 0.910f);
        private Color colorMid = new Color(0.914f, 0.796f, 0.200f);
        private Color colorHigh = new Color(0.898f, 0.420f, 0.263f);

        private Vector2 originalAnchoredPos;
        private Vector2 originalBorderPos;
        private Coroutine shakeCoroutine;

        private float timerElapsed;
        private bool isTimerRunning;

        private UIManager uiManager;

        private void Awake()
        {
            if (backgroundRect != null)
                originalAnchoredPos = backgroundRect.anchoredPosition;

            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillAmount = 0f;
                fillImage.color = colorLow;
            }

            if (tFillImage != null)
            {
                tFillImage.type = Image.Type.Filled;
                tFillImage.fillAmount = 0f;
            }

            if (borderRect != null)
                originalBorderPos = borderRect.anchoredPosition;
        }

        private void Start()
        {
            uiManager = GetComponentInParent<UIManager>();
            if (uiManager != null)
            {
                uiManager.OnPanelStateChanged += HandlePanelStateChanged;
>>>>>>> origin/Map
            }
        }

        private void OnDestroy()
        {
<<<<<<< HEAD
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
=======
            if (uiManager != null)
            {
                uiManager.OnPanelStateChanged -= HandlePanelStateChanged;
            }
        }

        private void HandlePanelStateChanged(FishingState state)
        {
            switch (state)
            {
                case FishingState.Charging:
                    OnChargingStarted();
                    break;
                case FishingState.Catching:
                    OnCatchingStarted();
                    break;
                case FishingState.Idle:
                case FishingState.Failure:
                case FishingState.Success:
                    OnFishingEnded();
                    break;
            }
        }

        private void OnChargingStarted()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.color = colorLow;
            }
        }

        private void OnCatchingStarted()
        {
            timerElapsed = 0f;
            isTimerRunning = true;
            if (tFillImage != null)
                tFillImage.fillAmount = 0f;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.color = colorLow;
            }
        }

        private void OnFishingEnded()
        {
            isTimerRunning = false;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.color = colorLow;
            }
            if (tFillImage != null)
                tFillImage.fillAmount = 0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && isTimerRunning)
            {
                if (fillImage != null)
                {
                    fillImage.fillAmount += fillAmount;
                    fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);
                    UpdateFillColor();
                }
                Shake();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (fillImage != null)
                {
                    fillImage.fillAmount = 0f;
                    fillImage.color = colorLow;
                }
            }

            if (isTimerRunning)
                UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (tFillImage == null) return;

            timerElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timerElapsed / timerDuration);
            tFillImage.fillAmount = progress;

            if (timerElapsed >= timerDuration)
                StopTimer();
        }

        private void StopTimer()
        {
            isTimerRunning = false;
        }

        private void Shake()
        {
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeIntensity;
                backgroundRect.anchoredPosition = originalAnchoredPos + new Vector2(x, 0f);
                if (borderRect != null)
                    borderRect.anchoredPosition = originalBorderPos + new Vector2(x, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            backgroundRect.anchoredPosition = originalAnchoredPos;
            if (borderRect != null)
                borderRect.anchoredPosition = originalBorderPos;
        }

        private void UpdateFillColor()
        {
            if (fillImage.fillAmount <= 0.4f)
                fillImage.color = colorLow;
            else if (fillImage.fillAmount <= 0.75f)
                fillImage.color = colorMid;
            else
                fillImage.color = colorHigh;
        }

        public void UpdateCatchBar(float current, float target)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(current / target);
                UpdateFillColor();
            }
        }

        public void UpdateChargeBar(float progress)
        {
            if (chargingBar != null)
                chargingBar.value = progress;
        }
    }
}
>>>>>>> origin/Map
