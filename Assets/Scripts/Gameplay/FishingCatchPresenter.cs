using System.Collections;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingCatchPresenter : MonoBehaviour
    {
        [Header("Fish Visuals")]
        [SerializeField] private GameObject fishPrefab;
        [SerializeField] private Transform handsPoint;
        [SerializeField] private GameObject rodModel;
        [SerializeField] private float fishWiggleAmount = 12f;
        [SerializeField] private float fishWiggleSpeed = 15f;
        [SerializeField] private float caughtFishLifetime = 6f;
        [SerializeField] private bool handOffToHands = false;

        [Header("Fish Attachment")]
        [SerializeField] private Vector3 hookAttachLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 hookAttachLocalEulerAngles = new Vector3(90f, 0f, 0f);
        [SerializeField] private Vector3 handAttachLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 handAttachLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 handDropStartOffset = new Vector3(0f, 0.3f, 0f);
        [SerializeField] private float handDropDuration = 0.25f;

        private Animator animator;
        private int hasFishParameterHash;
        private bool hasHasFishParameter;
        private GameObject activeFish;
        private Coroutine dropRoutine;
        private bool handoffTriggered;
        private Animator activeFishAnimator;
        private bool isPreviewFish;
        private bool isAnimating;

        public void Initialize(Animator playerAnimator, string hasFishParam)
        {
            animator = playerAnimator;
            hasFishParameterHash = Animator.StringToHash(hasFishParam);
            CheckAnimatorParameter();
        }

        public void PerformCatch(Transform hookPoint, float reelTime)
        {
            if (fishPrefab == null || hookPoint == null)
            {
                return;
            }

            if (hasHasFishParameter)
            {
                animator.SetBool(hasFishParameterHash, true);
            }

            EnsureFishAttachedToHook(hookPoint);

            if (activeFishAnimator != null)
            {
                activeFishAnimator.enabled = false;
            }

            isPreviewFish = false;
            handoffTriggered = false;

            StartCoroutine(AnimateFish(activeFish, hookPoint, reelTime));
            Destroy(activeFish, Mathf.Max(reelTime + 2f, caughtFishLifetime));
        }

        public void ShowFishPreview(Transform hookPoint)
        {
            if (fishPrefab == null || hookPoint == null || activeFish != null)
            {
                return;
            }

            EnsureFishAttachedToHook(hookPoint);
            isPreviewFish = true;
        }

        public void ClearFishPreview()
        {
            if (!isPreviewFish || handoffTriggered)
            {
                return;
            }

            CleanupActiveFish();
        }

        private IEnumerator AnimateFish(GameObject fish, Transform hookPoint, float reelTime)
        {
            isAnimating = true;
            float elapsed = 0f;
            Quaternion baseRotation = Quaternion.Euler(hookAttachLocalEulerAngles);

            while (elapsed < reelTime && fish != null)
            {
                elapsed += Time.deltaTime;
                float wig = elapsed * fishWiggleSpeed;
                float wiggleX = Mathf.Sin(wig) * fishWiggleAmount;
                float wiggleZ = Mathf.Cos(wig * 0.8f) * (fishWiggleAmount * 0.5f);

                if (fish.transform.parent == hookPoint)
                {
                    fish.transform.localRotation = baseRotation * Quaternion.Euler(wiggleX, 0f, wiggleZ);
                }

                if (handOffToHands && elapsed >= reelTime * 0.9f && fish.transform.parent == hookPoint)
                {
                    AttachFishToHands();
                }

                yield return null;
            }

            isAnimating = false;

            while (fish != null)
            {
                yield return null;
            }

            if (hasHasFishParameter)
            {
                animator.SetBool(hasFishParameterHash, false);
            }

            if (rodModel != null)
            {
                rodModel.SetActive(true);
            }

            if (ReferenceEquals(activeFish, fish))
            {
                activeFish = null;
                activeFishAnimator = null;
                handoffTriggered = false;
            }
        }

        public void AttachFishToHands()
        {
            if (!handOffToHands)
            {
                return;
            }

            if (handoffTriggered || activeFish == null || handsPoint == null)
            {
                return;
            }

            handoffTriggered = true;

            if (rodModel != null)
            {
                rodModel.SetActive(false);
            }

            activeFish.transform.SetParent(handsPoint);
            activeFish.transform.localPosition = handDropStartOffset;
            activeFish.transform.localRotation = Quaternion.Euler(handAttachLocalEulerAngles);

            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
            }

            dropRoutine = StartCoroutine(DropToHands(activeFish));
        }

        public void OnCarryHandsReady()
        {
            AttachFishToHands();
        }

        private IEnumerator DropToHands(GameObject fish)
        {
            float dropTime = Mathf.Max(0.01f, handDropDuration);
            float elapsed = 0f;
            Vector3 startPos = handDropStartOffset;
            Quaternion startRot = fish.transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(handAttachLocalEulerAngles);

            while (elapsed < dropTime && fish != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dropTime;
                fish.transform.localPosition = Vector3.Lerp(startPos, handAttachLocalPosition, t);
                fish.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            if (fish != null)
            {
                ApplyHandAttachPose();
            }

            if (ReferenceEquals(activeFish, fish))
            {
                dropRoutine = null;
            }
        }

        private void CheckAnimatorParameter()
        {
            hasHasFishParameter = false;
            if (animator == null)
            {
                return;
            }

            foreach (var p in animator.parameters)
            {
                if (p.nameHash == hasFishParameterHash)
                {
                    hasHasFishParameter = true;
                    break;
                }
            }
        }

        private void CleanupActiveFish()
        {
            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
                dropRoutine = null;
            }

            if (activeFish != null)
            {
                Destroy(activeFish);
                activeFish = null;
            }

            activeFishAnimator = null;
            handoffTriggered = false;
            isPreviewFish = false;
            isAnimating = false;

            if (rodModel != null)
            {
                rodModel.SetActive(true);
            }
        }

        private void EnsureFishAttachedToHook(Transform hookPoint)
        {
            if (activeFish == null)
            {
                activeFish = Instantiate(fishPrefab, hookPoint.position, Quaternion.identity);
            }

            activeFish.transform.SetParent(hookPoint);
            ApplyHookAttachPose();

            activeFishAnimator = activeFish.GetComponent<Animator>();
            if (activeFishAnimator != null)
            {
                activeFishAnimator.enabled = false;
            }
        }

        private void ApplyHookAttachPose()
        {
            if (activeFish == null)
            {
                return;
            }

            activeFish.transform.localPosition = hookAttachLocalPosition;
            activeFish.transform.localRotation = Quaternion.Euler(hookAttachLocalEulerAngles);
        }

        private void ApplyHandAttachPose()
        {
            if (activeFish == null)
            {
                return;
            }

            activeFish.transform.localPosition = handAttachLocalPosition;
            activeFish.transform.localRotation = Quaternion.Euler(handAttachLocalEulerAngles);
        }
    }
}
