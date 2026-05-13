using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingHookWaterContact : MonoBehaviour
    {
        [SerializeField] private float triggerRadius = 0.12f;
        [SerializeField] private float contactCooldown = 0.35f;

        private FishingController fishingController;
        private LayerMask waterLayerMask;
        private SphereCollider triggerCollider;
        private bool wasTouchingWater;
        private float lastContactTime = -999f;

        public void Initialize(FishingController controller, LayerMask waterMask)
        {
            fishingController = controller;
            waterLayerMask = waterMask.value != 0 ? waterMask : ResolveDefaultWaterLayerMask();
            EnsurePhysicsTrigger();
        }

        private void Update()
        {
            if (fishingController == null)
            {
                return;
            }

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                triggerRadius,
                waterLayerMask,
                QueryTriggerInteraction.Collide);

            bool isTouchingWater = hits != null && hits.Length > 0;
            if (isTouchingWater && !wasTouchingWater)
            {
                PlaySplash(transform.position);
            }

            wasTouchingWater = isTouchingWater;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsWaterCollider(other))
            {
                return;
            }

            PlaySplash(transform.position);
        }

        private void PlaySplash(Vector3 splashPosition)
        {
            if (Time.time - lastContactTime < contactCooldown)
            {
                return;
            }

            lastContactTime = Time.time;
            fishingController.PlayHookWaterSplash(splashPosition);
        }

        private bool IsWaterCollider(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            return (waterLayerMask.value & (1 << other.gameObject.layer)) != 0;
        }

        private void EnsurePhysicsTrigger()
        {
            triggerCollider = GetComponent<SphereCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<SphereCollider>();
            }

            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;

            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;
        }

        private static LayerMask ResolveDefaultWaterLayerMask()
        {
            int waterLayer = LayerMask.NameToLayer("Water");
            if (waterLayer >= 0)
            {
                return 1 << waterLayer;
            }

            int oceanLayer = LayerMask.NameToLayer("Ocean");
            if (oceanLayer >= 0)
            {
                return 1 << oceanLayer;
            }

            return Physics.DefaultRaycastLayers;
        }
    }
}
