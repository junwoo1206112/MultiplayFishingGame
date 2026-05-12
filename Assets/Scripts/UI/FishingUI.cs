using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.UI
{
    public class FishingUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject chargingPanel;
        [SerializeField] private GameObject catchingPanel;
        [SerializeField] private GameObject alertPanel;

        [Header("Charging UI")]
        [SerializeField] private Slider chargingBar;

        [Header("Catching UI")]
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform borderRect;
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

        private FishingController targetController;

        private void Awake()
        {
            AutoBindReferences();
            ApplyDefaultLayout();

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

        private void ApplyDefaultLayout()
        {
            RectTransform root = transform as RectTransform;
            if (root != null)
            {
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.pivot = new Vector2(0.5f, 0.5f);
                root.anchoredPosition = Vector2.zero;
                root.sizeDelta = Vector2.zero;
                root.localScale = Vector3.one;
            }

            SetPanelLayout(chargingPanel, new Vector2(0f, 1f), new Vector2(88f, -88f), new Vector2(96f, 96f));
            SetPanelLayout(alertPanel, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(280f, 88f));
            SetPanelLayout(catchingPanel, new Vector2(0.5f, 0f), new Vector2(0f, 96f), new Vector2(420f, 72f));

            ApplyCatchingBarLayout();
            ApplyTimerLayout();
        }

        private static void SetPanelLayout(GameObject panel, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            if (panel == null) return;

            RectTransform rect = panel.transform as RectTransform;
            if (rect == null) return;

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private void ApplyCatchingBarLayout()
        {
            if (catchingPanel == null) return;

            RectTransform panelRect = catchingPanel.transform as RectTransform;
            if (panelRect != null)
                panelRect.localScale = Vector3.one;

            if (backgroundRect != null)
                SetStretchLayout(backgroundRect, new Vector2(0f, 0f), new Vector2(0f, 0f));

            if (borderRect != null)
                SetStretchLayout(borderRect, new Vector2(0f, 0f), new Vector2(0f, 0f));

            Transform fillArea = catchingPanel.transform.Find("C_Background/Fill Area");
            if (fillArea is RectTransform fillAreaRect)
                SetStretchLayout(fillAreaRect, new Vector2(18f, 16f), new Vector2(-18f, -16f));

            if (fillImage != null)
            {
                RectTransform fillRect = fillImage.transform as RectTransform;
                if (fillRect != null)
                    SetStretchLayout(fillRect, Vector2.zero, Vector2.zero);

                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = 0;
            }
        }

        private void ApplyTimerLayout()
        {
            if (chargingPanel == null) return;

            Transform fillArea = chargingPanel.transform.Find("Fill Area");
            if (fillArea is RectTransform fillAreaRect)
                SetStretchLayout(fillAreaRect, new Vector2(12f, 12f), new Vector2(-12f, -12f));

            if (tFillImage != null)
            {
                RectTransform fillRect = tFillImage.transform as RectTransform;
                if (fillRect != null)
                    SetStretchLayout(fillRect, Vector2.zero, Vector2.zero);
            }
        }

        private static void SetStretchLayout(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private void Start()
        {
            HideAllPanels();
            FindLocalFishingController();
        }

        private void AutoBindReferences()
        {
            if (chargingPanel == null)
            {
                var timer = transform.Find("Timer");
                if (timer != null)
                    chargingPanel = timer.gameObject;
            }

            if (catchingPanel == null)
            {
                var catchingBar = transform.Find("CatchingBar");
                if (catchingBar != null)
                    catchingPanel = catchingBar.gameObject;
            }

            if (alertPanel == null)
            {
                var message = transform.Find("F_Message");
                if (message != null)
                    alertPanel = message.gameObject;
            }

            if (chargingBar == null && chargingPanel != null)
                chargingBar = chargingPanel.GetComponent<Slider>();

            if (backgroundRect == null)
            {
                var background = transform.Find("CatchingBar/C_Background");
                if (background != null)
                    backgroundRect = background.GetComponent<RectTransform>();
            }

            if (borderRect == null)
            {
                var border = transform.Find("CatchingBar/Border");
                if (border != null)
                    borderRect = border.GetComponent<RectTransform>();
            }

            if (fillImage == null)
            {
                var fill = transform.Find("CatchingBar/C_Background/Fill Area/C_Fill");
                if (fill != null)
                    fillImage = fill.GetComponent<Image>();
            }

            if (tFillImage == null)
            {
                var timerFill = transform.Find("Timer/Fill Area/T_Fill");
                if (timerFill != null)
                    tFillImage = timerFill.GetComponent<Image>();
            }
        }

        private void FindLocalFishingController()
        {
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
            HideAllPanels();

            switch (state)
            {
                case FishingState.Charging:
                    if (chargingPanel) chargingPanel.SetActive(true);
                    OnChargingStarted();
                    break;
                case FishingState.Nibble:
                    if (alertPanel) alertPanel.SetActive(true);
                    Debug.Log("입질! 빨리 낚으세요!");
                    break;
                case FishingState.Catching:
                    if (catchingPanel) catchingPanel.SetActive(true);
                    OnCatchingStarted();
                    break;
                default:
                    OnFishingEnded();
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (chargingPanel) chargingPanel.SetActive(false);
            if (catchingPanel) catchingPanel.SetActive(false);
            if (alertPanel) alertPanel.SetActive(false);
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
            if (targetController == null)
            {
                FindLocalFishingController();
                return;
            }

            if (isTimerRunning)
                UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (tFillImage == null) return;

            float duration = timerDuration;
            if (targetController != null)
            {
                duration = targetController.CatchingDuration;
            }

            timerElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timerElapsed / duration);
            tFillImage.fillAmount = progress;

            if (timerElapsed >= duration)
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
                if (backgroundRect == null)
                    yield break;

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

        public void UpdateChargeBar(float progress)
        {
            if (chargingBar) chargingBar.value = progress;
        }

        public void UpdateCatchBar(float current, float target)
        {
            if (target <= 0f) return;

            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(current / target);
                UpdateFillColor();
            }
            if (current > 0 && backgroundRect != null)
                Shake();
        }
    }
}
