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

        public bool IsWaterInFront(Vector3 origin, Vector3 forward, out bool isOcean)
        {
            float rad = castAngle * Mathf.Deg2Rad;
            Vector3 direction = (forward * Mathf.Cos(rad) + Vector3.down * Mathf.Sin(rad)).normalized;

            if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, maxDistance, waterLayerMask))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ocean"))
                {
                    int groundMask = LayerMask.GetMask("Ground");
                    bool groundBetween = Physics.Raycast(origin, direction, hit.distance, groundMask);
                    isOcean = !groundBetween;
                }
                else
                {
                    isOcean = false;
                }
                return true;
            }

            isOcean = false;
            return false;
        }
    }
}
