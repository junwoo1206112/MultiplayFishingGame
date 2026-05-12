using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MultiplayFishing.UI
{
    public sealed class StartSceneTransition : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private string targetSceneName = "GamePlay";
        [SerializeField] private float fadeDuration = 0.8f;

        private bool isTransitioning;

        private void Awake()
        {
            if (fadeOverlay != null)
            {
                Color color = fadeOverlay.color;
                color.a = 0f;
                fadeOverlay.color = color;
                fadeOverlay.raycastTarget = false;
            }
        }

        private void OnEnable()
        {
            if (startButton != null)
                startButton.onClick.AddListener(BeginTransition);
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(BeginTransition);
        }

        public void BeginTransition()
        {
            if (isTransitioning)
                return;

            StartCoroutine(FadeOutAndLoad());
        }

        private IEnumerator FadeOutAndLoad()
        {
            isTransitioning = true;

            if (startButton != null)
                startButton.interactable = false;

            if (fadeOverlay != null)
            {
                fadeOverlay.raycastTarget = true;
                float elapsed = 0f;

                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float alpha = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
                    Color color = fadeOverlay.color;
                    color.a = alpha;
                    fadeOverlay.color = color;
                    yield return null;
                }
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}
