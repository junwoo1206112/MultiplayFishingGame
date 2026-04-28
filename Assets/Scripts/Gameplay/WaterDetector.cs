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

            Vector3 rayOrigin = GetRayOrigin();
            return Physics.Raycast(
                rayOrigin,
                Vector3.down,
                downCheckDistance,
                oceanLayer,
                triggerInteraction);
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
