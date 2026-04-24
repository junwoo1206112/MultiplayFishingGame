using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayFishing.Gameplay
{
    public class FishingClickChallengeUI : MonoBehaviour
    {
        [Header("Challenge")]
        [SerializeField] private int requiredClicks = 15;
        [SerializeField] private int countdownStart = 3;
        [SerializeField] private float countdownInterval = 1f;

        [Header("Runtime UI")]
        [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private int countdownFontSize = 96;
        [SerializeField] private int progressFontSize = 42;

        private Canvas canvas;
        private Image panel;
        private Text countdownText;
        private Text progressText;
        private Coroutine challengeRoutine;
        private int currentClicks;
        private bool isRunning;
        private bool acceptsClicks;

        public bool IsRunning => isRunning;
        public event System.Action ChallengeSucceeded;

        public void BeginChallenge()
        {
            EnsureUI();

            if (challengeRoutine != null)
            {
                StopCoroutine(challengeRoutine);
            }

            currentClicks = 0;
            isRunning = true;
            acceptsClicks = false;
            canvas.gameObject.SetActive(true);
            UpdateProgressText();
            challengeRoutine = StartCoroutine(RunCountdown());
        }

        public void RegisterClick()
        {
            if (!isRunning || !acceptsClicks)
            {
                return;
            }

            currentClicks = Mathf.Min(currentClicks + 1, requiredClicks);
            UpdateProgressText();

            if (currentClicks >= requiredClicks)
            {
                CompleteChallenge();
            }
        }

        public void CancelChallenge()
        {
            if (challengeRoutine != null)
            {
                StopCoroutine(challengeRoutine);
                challengeRoutine = null;
            }

            isRunning = false;
            acceptsClicks = false;

            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        private IEnumerator RunCountdown()
        {
            for (int count = Mathf.Max(1, countdownStart); count > 0; count--)
            {
                SetPanelVisible(true);
                countdownText.text = count.ToString();
                yield return new WaitForSeconds(countdownInterval);
            }

            countdownText.text = "GO!!";
            acceptsClicks = true;
            yield return new WaitForSeconds(0.4f);
            countdownText.text = string.Empty;
            SetPanelVisible(false);
            challengeRoutine = null;
        }

        private void CompleteChallenge()
        {
            isRunning = false;
            acceptsClicks = false;

            if (challengeRoutine != null)
            {
                StopCoroutine(challengeRoutine);
                challengeRoutine = null;
            }

            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }

            ChallengeSucceeded?.Invoke();
        }

        private void EnsureUI()
        {
            if (canvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("Fishing Click Challenge UI");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject("Dim Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            panel = panelObject.AddComponent<Image>();
            panel.color = panelColor;
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            countdownText = CreateText(canvasObject.transform, "Countdown Text", countdownFontSize, new Vector2(0.5f, 0.55f));
            progressText = CreateText(canvasObject.transform, "Progress Text", progressFontSize, new Vector2(0.5f, 0.42f));
            canvasObject.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, int fontSize, Vector2 anchor)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textColor;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 160f);
            rect.anchoredPosition = Vector2.zero;

            return text;
        }

        private void UpdateProgressText()
        {
            if (progressText == null)
            {
                return;
            }

            progressText.text = $"{currentClicks} / {requiredClicks}";
        }

        private void SetPanelVisible(bool visible)
        {
            if (panel != null)
            {
                panel.enabled = visible;
            }
        }
    }
}
