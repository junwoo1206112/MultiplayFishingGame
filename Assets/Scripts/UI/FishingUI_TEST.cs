using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MultiplayFishing.UI
{
    public class FishingUI_TEST : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private float fillAmount = 0.1f;
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private float shakeDuration = 0.15f;

        private Color colorLow = new Color(0.408f, 0.741f, 0.910f);
        private Color colorMid = new Color(0.914f, 0.796f, 0.200f);
        private Color colorHigh = new Color(0.898f, 0.420f, 0.263f);
        private Vector2 originalAnchoredPos;
        private Coroutine shakeCoroutine;

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
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
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

            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[TEST] R key pressed!");
                if (fillImage != null)
                {
                    fillImage.fillAmount = 0f;
                    fillImage.color = colorLow;
                    Debug.Log("[TEST] FillAmount reset to 0!");
                }
            }
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
                elapsed += Time.deltaTime;
                yield return null;
            }
            backgroundRect.anchoredPosition = originalAnchoredPos;
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
