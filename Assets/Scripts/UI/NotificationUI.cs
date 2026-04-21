using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayFishing.UI
{
    public class NotificationUI : MonoBehaviour
    {
        public static NotificationUI Instance { get; private set; }

        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform container;
        [SerializeField] private float displayDuration = 5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ShowMessage(string message)
        {
            Debug.Log($"NotificationUI: ShowMessage 호출됨 - {message}");
            StartCoroutine(CreateMessageRoutine(message));
        }

        private IEnumerator CreateMessageRoutine(string message)
        {
            if (messagePrefab == null)
            {
                Debug.LogError("NotificationUI: Message Prefab이 할당되지 않았습니다!");
                yield break;
            }
            if (container == null)
            {
                Debug.LogError("NotificationUI: Container가 할당되지 않았습니다!");
                yield break;
            }

            GameObject msgObj = Instantiate(messagePrefab, container);
            Debug.Log($"NotificationUI: 메시지 오브젝트 생성됨 - {message}");
            TMP_Text textComponent = msgObj.GetComponentInChildren<TMP_Text>();
            
            if (textComponent != null)
            {
                textComponent.text = message;
            }

            // 일정 시간 대기 후 삭제
            yield return new WaitForSeconds(displayDuration);
            
            // 페이드 아웃 효과 (간단하게 구현)
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
