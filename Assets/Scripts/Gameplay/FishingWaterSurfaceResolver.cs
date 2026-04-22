using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingWaterSurfaceResolver
    {
        private readonly Camera playerCamera;
        private readonly Transform tipPoint;
        private readonly LayerMask waterLayerMask;
        private readonly float waterRayStartHeight;
        private readonly float downwardCastBias;
        private readonly float maxCastDistance;

        public Transform WaterSurfaceTransform { get; private set; }

        public FishingWaterSurfaceResolver(
            Camera playerCamera,
            Transform tipPoint,
            Transform waterSurfaceTransform,
            LayerMask waterLayerMask,
            float waterRayStartHeight,
            float downwardCastBias,
            float maxCastDistance)
        {
            this.playerCamera = playerCamera;
            this.tipPoint = tipPoint;
            WaterSurfaceTransform = waterSurfaceTransform;
            this.waterLayerMask = waterLayerMask;
            this.waterRayStartHeight = waterRayStartHeight;
            this.downwardCastBias = downwardCastBias;
            this.maxCastDistance = maxCastDistance;
        }

        public Vector3 ResolveCastTarget(
            Transform owner,
            Vector3 castTargetOffset,
            float fallbackCastDistance,
            out bool hasSurfaceHit,
            out Vector3 surfaceHitPoint)
        {
            Vector3 fallbackTarget = GetFallbackCastTarget(owner, castTargetOffset, fallbackCastDistance);
            if (TryGetSurfaceHit(owner, out RaycastHit hit))
            {
                hasSurfaceHit = true;
                surfaceHitPoint = hit.point;

                Vector3 targetPosition = hit.point
                    + owner.right * castTargetOffset.x
                    + owner.up * castTargetOffset.y
                    + owner.forward * castTargetOffset.z;

                if (TryGetSurfaceHeight(out float waterSurfaceY))
                {
                    targetPosition.y = waterSurfaceY + castTargetOffset.y;
                }
                else if (tipPoint != null)
                {
                    targetPosition.y = Mathf.Min(targetPosition.y, tipPoint.position.y);
                }

                return targetPosition;
            }

            hasSurfaceHit = false;
            surfaceHitPoint = Vector3.zero;

            if (TryGetSurfaceHeight(out float fallbackWaterSurfaceY))
            {
                fallbackTarget.y = fallbackWaterSurfaceY + castTargetOffset.y;
            }

            return fallbackTarget;
        }

        public bool TryGetSurfaceHeight(out float waterSurfaceY)
        {
            EnsureWaterSurfaceTransform();

            if (WaterSurfaceTransform == null)
            {
                waterSurfaceY = 0f;
                return false;
            }

            waterSurfaceY = WaterSurfaceTransform.position.y;
            return true;
        }

        private bool TryGetSurfaceHit(Transform owner, out RaycastHit hit)
        {
            if (playerCamera != null)
            {
                Ray screenCenterRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(screenCenterRay, out hit, maxCastDistance, waterLayerMask, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }

            Vector3 rayOrigin = owner.position + Vector3.up * waterRayStartHeight;
            Vector3 forwardDirection = owner.forward;
            Vector3 biasedDirection = (forwardDirection + Vector3.down * downwardCastBias).normalized;

            if (Physics.Raycast(rayOrigin, biasedDirection, out hit, maxCastDistance, waterLayerMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            if (tipPoint != null)
            {
                Vector3 tipBiasedDirection = (tipPoint.forward + Vector3.down * downwardCastBias).normalized;
                if (Physics.Raycast(tipPoint.position, tipBiasedDirection, out hit, maxCastDistance, waterLayerMask, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }

            hit = default;
            return false;
        }

        private Vector3 GetFallbackCastTarget(Transform owner, Vector3 castTargetOffset, float fallbackCastDistance)
        {
            if (tipPoint != null)
            {
                return tipPoint.position
                    + owner.right * castTargetOffset.x
                    + owner.up * castTargetOffset.y
                    + owner.forward * (fallbackCastDistance + castTargetOffset.z);
            }

            return owner.position
                + owner.right * castTargetOffset.x
                + owner.up * castTargetOffset.y
                + owner.forward * (fallbackCastDistance + castTargetOffset.z);
        }

        private void EnsureWaterSurfaceTransform()
        {
            if (WaterSurfaceTransform != null)
            {
                return;
            }

            GameObject waterObject = GameObject.Find("WaterBlock_50m");
            if (waterObject == null)
            {
                waterObject = GameObject.Find("WaterBlock_50m (1)");
            }

            if (waterObject != null)
            {
                WaterSurfaceTransform = waterObject.transform;
            }
        }
    }
}
