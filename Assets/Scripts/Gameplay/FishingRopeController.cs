using System.Collections;
using System.Reflection;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingRopeController
    {
        private readonly Transform tipPoint;
        private readonly Transform hookPoint;
        private readonly GameObject fishingRopeObject;
        private readonly Component fishingRopeComponent;
        private readonly Transform originalHookParent;
        private readonly Vector3 originalHookLocalPosition;
        private readonly Quaternion originalHookLocalRotation;
        private readonly Vector3 originalHookLocalScale;

        public FishingRopeController(
            Transform tipPoint,
            Transform hookPoint,
            GameObject fishingRopeObject,
            Component fishingRopeComponent)
        {
            this.tipPoint = tipPoint;
            this.hookPoint = hookPoint;
            this.fishingRopeObject = fishingRopeObject;
            this.fishingRopeComponent = fishingRopeComponent;

            if (hookPoint != null)
            {
                originalHookParent = hookPoint.parent;
                originalHookLocalPosition = hookPoint.localPosition;
                originalHookLocalRotation = hookPoint.localRotation;
                originalHookLocalScale = hookPoint.localScale;
            }
        }

        public bool IsConfigured => tipPoint != null && hookPoint != null;

        public Vector3 GetIdleHookPosition(Transform owner, Vector3 idleHookOffset)
        {
            if (tipPoint == null)
            {
                return owner.position;
            }

            return tipPoint.TransformPoint(idleHookOffset);
        }

        public float GetDesiredRopeLength(Vector3 targetPosition, float minimumLength, float slack)
        {
            if (tipPoint == null)
            {
                return minimumLength;
            }

            float directDistance = Vector3.Distance(tipPoint.position, targetPosition);
            return Mathf.Max(minimumLength, directDistance + Mathf.Max(0f, slack));
        }

        public void SetHookPosition(Vector3 position)
        {
            if (hookPoint != null)
            {
                hookPoint.position = position;
            }
        }

        public void DetachHookFromRod()
        {
            if (hookPoint == null || hookPoint.parent == null)
            {
                return;
            }

            hookPoint.SetParent(null, true);
        }

        public void RestoreHookToRod()
        {
            if (hookPoint == null || originalHookParent == null)
            {
                return;
            }

            hookPoint.SetParent(originalHookParent, false);
            hookPoint.localPosition = originalHookLocalPosition;
            hookPoint.localRotation = originalHookLocalRotation;
            hookPoint.localScale = originalHookLocalScale;
        }

        public void RestoreHookToRod(Vector3 worldPosition)
        {
            if (hookPoint == null || originalHookParent == null)
            {
                return;
            }

            hookPoint.SetParent(originalHookParent, false);
            hookPoint.localRotation = originalHookLocalRotation;
            hookPoint.localScale = originalHookLocalScale;
            hookPoint.position = worldPosition;
        }

        public void SetVisible(bool visible)
        {
            if (fishingRopeObject != null)
            {
                fishingRopeObject.SetActive(visible);
            }
        }

        public void SetRopeLength(float ropeLength)
        {
            if (fishingRopeComponent == null)
            {
                return;
            }

            PropertyInfo ropeLengthProperty = fishingRopeComponent.GetType().GetProperty("ropeLength");
            if (ropeLengthProperty != null && ropeLengthProperty.CanWrite)
            {
                ropeLengthProperty.SetValue(fishingRopeComponent, ropeLength);
                return;
            }

            FieldInfo ropeLengthField = fishingRopeComponent.GetType().GetField("ropeLength");
            if (ropeLengthField != null)
            {
                ropeLengthField.SetValue(fishingRopeComponent, ropeLength);
            }
        }

        public IEnumerator MoveHook(
            Vector3 targetPosition,
            float startDelay,
            float duration,
            float arcHeight,
            float ropeSlack,
            float minimumRopeLength,
            bool showRopeOnStart,
            bool hideRopeOnComplete,
            bool useArcPath,
            bool stopAtWaterSurface,
            float waterSurfaceY,
            System.Action onWaterHit = null,
            FishingLineVisual lineVisual = null)
        {
            if (!IsConfigured)
            {
                yield break;
            }

            // Let FishingLineVisual know the rope is controlling hook movement.
            lineVisual?.SetHookControlledByRope(true);

            try
            {
                if (startDelay > 0f)
                {
                    yield return new WaitForSeconds(startDelay);
                }

                if (!hideRopeOnComplete)
                {
                    DetachHookFromRod();
                }

                if (showRopeOnStart)
                {
                    SetVisible(true);
                }

                // Apply the cast rope target only when the throw actually begins,
                // so the lure does not appear to snap toward the water on click.
                SetRopeLength(GetDesiredRopeLength(targetPosition, minimumRopeLength, ropeSlack));

                Vector3 startPosition = hookPoint.position;
                Vector3 controlPoint = GetArcControlPoint(startPosition, targetPosition, arcHeight);
                float elapsed = 0f;
                float safeDuration = Mathf.Max(0.01f, duration);
                bool hasHitWater = false;

                while (elapsed < safeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / safeDuration));

                    Vector3 nextPosition = useArcPath
                        ? EvaluateQuadraticBezier(startPosition, controlPoint, targetPosition, t)
                        : Vector3.Lerp(startPosition, targetPosition, t);

                    // Clamp to the water surface so the hook does not keep sinking.
                    if (stopAtWaterSurface && nextPosition.y < waterSurfaceY)
                    {
                        nextPosition.y = waterSurfaceY;

                        // Trigger the splash only on the first water contact.
                        if (!hasHitWater)
                        {
                            hasHitWater = true;
                            onWaterHit?.Invoke();
                        }
                    }

                    hookPoint.position = nextPosition;
                    SetRopeLength(GetDesiredRopeLength(nextPosition, minimumRopeLength, ropeSlack));
                    yield return null;
                }

                // Apply the final evaluated position after the motion completes.
                Vector3 finalPosition = useArcPath
                    ? EvaluateQuadraticBezier(startPosition, controlPoint, targetPosition, 1f)
                    : targetPosition;

                // Keep the final position from ending below the water surface clamp.
                if (stopAtWaterSurface && finalPosition.y < waterSurfaceY)
                {
                    finalPosition.y = waterSurfaceY;
                }

                hookPoint.position = finalPosition;
                SetRopeLength(GetDesiredRopeLength(finalPosition, minimumRopeLength, ropeSlack));

                if (hideRopeOnComplete)
                {
                    RestoreHookToRod();
                    SetVisible(false);
                }
            }
            finally
            {
                // Hand hook control back to FishingLineVisual when rope motion ends.
                lineVisual?.SetHookControlledByRope(false);
            }
        }

        private Vector3 GetArcControlPoint(Vector3 startPosition, Vector3 targetPosition, float baseArcHeight)
        {
            Vector3 controlPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f);
            float horizontalDistance = Vector3.Distance(
                new Vector3(startPosition.x, 0f, startPosition.z),
                new Vector3(targetPosition.x, 0f, targetPosition.z));

            float dynamicArcHeight = Mathf.Max(baseArcHeight, horizontalDistance * 0.2f);
            controlPoint.y = Mathf.Max(startPosition.y, targetPosition.y) + dynamicArcHeight;
            return controlPoint;
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * start
                + 2f * oneMinusT * t * control
                + t * t * end;
        }
    }
}
