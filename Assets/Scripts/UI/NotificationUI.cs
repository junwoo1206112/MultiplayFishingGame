using UnityEngine;
using TMPro;
using System.Collections;
using MultiplayFishing.Gameplay;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class NotificationUI : MonoBehaviour, INotificationService
    {
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform container;
        [SerializeField] private float displayDuration = 5f;

        private void OnEnable()
        {
            FishingPlayer.OnSystemMessage += ShowMessage;
        }

        private void OnDisable()
        {
            FishingPlayer.OnSystemMessage -= ShowMessage;
        }

        public void ShowMessage(string message)
        {
            if (messagePrefab == null || container == null) return;
            if (!gameObject.activeInHierarchy) return; // 씬에 활성화된 경우만 실행
            StartCoroutine(CreateMessageRoutine(message));
        }

        private IEnumerator CreateMessageRoutine(string message)
        {
            GameObject msgObj = Instantiate(messagePrefab, container);
            TMP_Text textComponent = msgObj.GetComponentInChildren<TMP_Text>();

            if (textComponent != null)
            {
                textComponent.text = message;
            }

            yield return new WaitForSeconds(displayDuration);

            CanvasGroup cg = msgObj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float elapsed = 0;
                while (elapsed < 1f)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = 1f - elapsed;
                    yield return null;
                }
            }

            Destroy(msgObj);
        }
    }
}
