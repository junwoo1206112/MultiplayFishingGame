using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class WaterDetector : MonoBehaviour
    {
        [Header("Ocean Detection")]
        [SerializeField] private LayerMask oceanLayer;
        [SerializeField] private float forwardCheckDistance = 3f;
        [SerializeField] private float rayStartHeight = 2f;
        [SerializeField] private float downCheckDistance = 5f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private void Awake()
        {
            EnsureOceanLayer();
        }

        public bool CanFish()
        {
            if (!EnsureOceanLayer())
            {
                return false;
            }

            return TryGetClosestSurface(out RaycastHit hit)
                && IsOceanLayer(hit.collider.gameObject.layer);
        }

        private bool EnsureOceanLayer()
        {
            if (oceanLayer != 0)
            {
                return true;
            }

            int layer = LayerMask.NameToLayer("Ocean");
            if (layer < 0)
            {
                Debug.LogWarning("WaterDetector could not find an Ocean layer. Assign oceanLayer in the inspector.", this);
                return false;
            }

            oceanLayer = 1 << layer;
            return true;
        }

        private Vector3 GetRayOrigin()
        {
            return transform.position
                + transform.forward * forwardCheckDistance
                + Vector3.up * rayStartHeight;
        }

        private bool TryGetClosestSurface(out RaycastHit closestHit)
        {
            Vector3 rayOrigin = GetRayOrigin();
            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                downCheckDistance,
                Physics.DefaultRaycastLayers,
                triggerInteraction);

            closestHit = default;
            float closestDistance = float.PositiveInfinity;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                }
            }

            return closestHit.collider != null;
        }

        private bool IsOceanLayer(int layer)
        {
            return (oceanLayer.value & (1 << layer)) != 0;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 rayOrigin = GetRayOrigin();
            Vector3 rayEnd = rayOrigin + Vector3.down * downCheckDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rayOrigin, rayEnd);
            Gizmos.DrawSphere(rayOrigin, 0.08f);
        }
    }
}
