using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class WaterDetector : MonoBehaviour
    {
        [Header("Water Detection")]
        [SerializeField] private LayerMask waterLayerMask;
        [SerializeField] private float castAngle = 40f;
        [SerializeField] private float sphereRadius = 0.5f;
        [SerializeField] private float maxDistance = 12f;

        public bool IsWaterInFront(Vector3 origin, Vector3 forward)
        {
            float rad = castAngle * Mathf.Deg2Rad;
            Vector3 direction = (forward * Mathf.Cos(rad) + Vector3.down * Mathf.Sin(rad)).normalized;

            return Physics.SphereCast(origin, sphereRadius, direction, out _, maxDistance, waterLayerMask);
        }
    }
}
