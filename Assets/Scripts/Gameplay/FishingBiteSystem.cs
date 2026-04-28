using System.Collections;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingBiteSystem : MonoBehaviour
    {
        [Header("Bite Settings")]
        [SerializeField] private GameObject biteSignalPrefab;
        [SerializeField] private Vector3 biteSignalOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private float minBiteWaitTime = 2f;
        [SerializeField] private float maxBiteWaitTime = 5f;
        [SerializeField] private float biteWindowDuration = 1.5f;

        private Transform hookPoint;
        private GameObject activeBiteSignal;
        private bool isBiteActive;
        private bool isBiteHeldForChallenge;
        private Coroutine biteWaitRoutine;
        private Coroutine biteWindowRoutine;

        public bool IsBiteActive => isBiteActive;
        public event System.Action BiteStarted;
        public event System.Action BiteEnded;

        public void SetHookPoint(Transform hook)
        {
            hookPoint = hook;
        }

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
            isBiteHeldForChallenge = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);

            if (wasBiteActive)
            {
                BiteEnded?.Invoke();
            }
            
            biteWaitRoutine = null;
            biteWindowRoutine = null;
        }

        public void HoldBiteForChallenge()
        {
            if (!isBiteActive)
            {
                return;
            }

            isBiteHeldForChallenge = true;
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
                Vector3 signalPos = (hookPoint != null)
                    ? hookPoint.position + biteSignalOffset
                    : transform.position + biteSignalOffset;
                activeBiteSignal = Instantiate(biteSignalPrefab, signalPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(biteWindowDuration);

            if (isBiteHeldForChallenge)
            {
                biteWindowRoutine = null;
                yield break;
            }

            isBiteActive = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);
            BiteEnded?.Invoke();
            
            Debug.Log("Fish got away...");
            
            // 다시 입질 대기
            biteWaitRoutine = StartCoroutine(WaitForBite());
        }
    }
}
