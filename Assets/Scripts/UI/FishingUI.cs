using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class FishingUI : MonoBehaviour
    {
        [Header("Charging UI")]
        [SerializeField] private Slider chargingBar;

        [Header("Catching UI")]
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
            }
        }

        private void OnDestroy()
        {
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