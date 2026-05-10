using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MultiplayFishing.UI
{
    public class FishingUI_TEST : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform timerRect;
        [SerializeField] private float fillAmount = 0.1f;
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private float shakeDuration = 0.15f;

        [Header("Timer Settings")]
        [SerializeField] private float timerDuration = 120f;
        [SerializeField] private Image tFillImage;

        private Color colorLow = new Color(0.408f, 0.741f, 0.910f);
        private Color colorMid = new Color(0.914f, 0.796f, 0.200f);
        private Color colorHigh = new Color(0.898f, 0.420f, 0.263f);
        private Vector2 originalAnchoredPos;
        private Vector2 originalBorderPos;
        private Coroutine shakeCoroutine;

        private float timerElapsed;
        private bool isTimerRunning;
        private RectTransform borderRect;

        private void Awake()
        {
            if (backgroundRect != null)
                originalAnchoredPos = backgroundRect.anchoredPosition;
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillAmount = 0f;
                fillImage.color = colorLow;
                Debug.Log($"[TEST] FillImage initialized: fillAmount = {fillImage.fillAmount}");
            }
            if (tFillImage != null)
            {
                tFillImage.type = Image.Type.Filled;
                tFillImage.fillAmount = 0f;
                Debug.Log("[TEST] T_Fill Image initialized");
            }

            Transform catchingBar = timerRect.parent;
            if (catchingBar != null)
            {
                Transform border = catchingBar.Find("Border");
                if (border != null)
                {
                    borderRect = border.GetComponent<RectTransform>();
                    if (borderRect != null)
                    {
                        originalBorderPos = borderRect.anchoredPosition;
                        Debug.Log("[TEST] Border found and cached");
                    }
                }
            }

            StartTimer();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[TEST] Space key pressed!");
                if (fillImage != null)
                {
                    fillImage.fillAmount += fillAmount;
                    fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);
                    UpdateFillColor();
                    Debug.Log($"[TEST] FillAmount: {fillImage.fillAmount}");
                }
                Shake();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[TEST] R key pressed!");
                if (fillImage != null)
                {
                    fillImage.fillAmount = 0f;
                    fillImage.color = colorLow;
                    Debug.Log("[TEST] FillAmount reset to 0!");
                }
            }

            if (isTimerRunning)
            {
                UpdateTimer();
            }
        }

        private void StartTimer()
        {
            timerElapsed = 0f;
            isTimerRunning = true;
            if (tFillImage != null)
            {
                tFillImage.fillAmount = 0f;
            }
            Debug.Log("[TEST] Timer started!");
        }

        private void UpdateTimer()
        {
            if (tFillImage == null) return;

            timerElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timerElapsed / timerDuration);
            tFillImage.fillAmount = progress;

            Debug.Log($"[TEST] Timer: {timerElapsed:F2}s / {timerDuration}s ({(progress * 100):F0}%)");

            if (timerElapsed >= timerDuration)
            {
                StopTimer();
            }
        }

        private void OnTimerKeyDown()
        {
            if (isTimerRunning)
                StopTimer();
            else
                StartTimer();
        }

private void StopTimer()
        {
            isTimerRunning = false;
            Debug.Log("[TEST] Timer stopped!");
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
            {
                fillImage.color = colorLow;
            }
            else if (fillImage.fillAmount <= 0.75f)
            {
                fillImage.color = colorMid;
            }
            else
            {
                fillImage.color = colorHigh;
            }
        }
    }
}
