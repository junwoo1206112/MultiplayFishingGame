using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MultiplayFishing.UI
{
    public class StartSceneTransition : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string targetSceneName = "Lobby";

        [Header("Fade")]
        [SerializeField] private float fadeOutDuration = 1.6f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("References")]
        [SerializeField] private Button startButton;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Image fadeOverlay;

        private bool isTransitioning;

        private void Awake()
        {
            if (startButton == null)
            {
                startButton = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartTransition);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartTransition);
            }
        }

        public void StartTransition()
        {
            if (isTransitioning)
            {
                return;
            }

            StartCoroutine(TransitionRoutine());
        }

        private IEnumerator TransitionRoutine()
        {
            isTransitioning = true;

            if (startButton != null)
            {
                startButton.interactable = false;
            }

            if (fadeOverlay == null)
            {
                if (targetCanvas == null)
                {
                    targetCanvas = GetComponentInParent<Canvas>();
                }

                if (targetCanvas != null)
                {
                    GameObject overlayObject = new GameObject("Fade Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    overlayObject.transform.SetParent(targetCanvas.transform, false);
                    overlayObject.transform.SetAsLastSibling();

                    RectTransform rectTransform = overlayObject.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;

                    fadeOverlay = overlayObject.GetComponent<Image>();
                    fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                    fadeOverlay.raycastTarget = false;
                }
                else
                {
                    Debug.LogWarning("[StartSceneTransition] Fade overlay could not be created because Canvas is missing.");
                }
            }
            else
            {
                fadeOverlay.transform.SetAsLastSibling();
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.raycastTarget = true;
            }

            float duration = Mathf.Max(0.01f, fadeOutDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetFadeAlpha(Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            SetFadeAlpha(1f);
            SceneManager.LoadScene(targetSceneName);
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeOverlay == null)
            {
                return;
            }

            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, Mathf.Clamp01(alpha));
        }
    }
}
