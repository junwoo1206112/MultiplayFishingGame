using System.Collections;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingBiteSystem : MonoBehaviour
    {
        [Header("Bite Settings")]
        [SerializeField] private GameObject biteSignalPrefab;
        [SerializeField] private Vector3 biteSignalOffset = new Vector3(0f, 2.5f, 0f);
        [SerializeField] private float minBiteWaitTime = 2f;
        [SerializeField] private float maxBiteWaitTime = 5f;
        [SerializeField] private float biteWindowDuration = 1.5f;

        private GameObject activeBiteSignal;
        private bool isBiteActive;
        private Coroutine biteWaitRoutine;
        private Coroutine biteWindowRoutine;

        public bool IsBiteActive => isBiteActive;
        public event System.Action BiteStarted;
        public event System.Action BiteEnded;

        public void StartWaitingForBite()
        {
            StopBiteLogic();
            biteWaitRoutine = StartCoroutine(WaitForBite());
        }

        public void StopBiteLogic()
        {
            if (biteWaitRoutine != null) StopCoroutine(biteWaitRoutine);
            if (biteWindowRoutine != null) StopCoroutine(biteWindowRoutine);
            
            bool wasBiteActive = isBiteActive;
            isBiteActive = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);

            if (wasBiteActive)
            {
                BiteEnded?.Invoke();
            }
            
            biteWaitRoutine = null;
            biteWindowRoutine = null;
        }

        private IEnumerator WaitForBite()
        {
            float waitTime = Random.Range(minBiteWaitTime, maxBiteWaitTime);
            yield return new WaitForSeconds(waitTime);
            biteWindowRoutine = StartCoroutine(BiteWindow());
        }

        private IEnumerator BiteWindow()
        {
            isBiteActive = true;
            Debug.Log("<color=red>BITE!</color>");
            BiteStarted?.Invoke();

            if (biteSignalPrefab != null)
            {
                activeBiteSignal = Instantiate(biteSignalPrefab, transform.position + biteSignalOffset, Quaternion.identity);
                activeBiteSignal.transform.SetParent(transform);
            }

            yield return new WaitForSeconds(biteWindowDuration);

            isBiteActive = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);
            BiteEnded?.Invoke();
            
            Debug.Log("Fish got away...");
            
            // 다시 입질 대기
            biteWaitRoutine = StartCoroutine(WaitForBite());
        }
    }
}
