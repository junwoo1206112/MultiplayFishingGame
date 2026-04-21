using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// Spawns a splash when the hook crosses the water surface.
    /// Uses the water object's transform Y as a simple, stable surface height.
    /// </summary>
    public class FishingSplashEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform hookPoint;
        [SerializeField] private Transform waterObject;

        [Header("Particle Settings")]
        [SerializeField] private GameObject splashParticlePrefab;
        [SerializeField] private Vector3 splashOffset = new Vector3(0f, 0.05f, 0f);
        [SerializeField] private float waterLevelTolerance = 0.05f;
        [SerializeField] private float spawnCooldown = 0.3f;
        [SerializeField] private float particleLifetime = 2f;

        private float lastSpawnTime = float.NegativeInfinity;
        private bool wasAboveWater;
        private bool initialized;

        private void Start()
        {
            ResolveReferences();

            if (hookPoint == null || waterObject == null)
            {
                enabled = false;
                return;
            }

            wasAboveWater = IsAboveWater(hookPoint.position.y, GetWaterLevel());
            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized || hookPoint == null || waterObject == null || splashParticlePrefab == null)
            {
                return;
            }

            float waterLevel = GetWaterLevel();
            bool isAboveWater = IsAboveWater(hookPoint.position.y, waterLevel);

            if (wasAboveWater && !isAboveWater)
            {
                TrySpawnSplash(waterLevel);
            }

            wasAboveWater = isAboveWater;
        }

        private void ResolveReferences()
        {
            if (hookPoint == null)
            {
                Transform foundHook = transform.Find("HookPoint");
                if (foundHook != null)
                {
                    hookPoint = foundHook;
                }
            }

            if (waterObject == null)
            {
                GameObject water = GameObject.Find("WaterBlock_50m");
                if (water != null)
                {
                    waterObject = water.transform;
                }
                else
                {
                    Debug.LogWarning("[FishingSplashEffect] WaterBlock_50m was not found. Assign the water object in the Inspector.", this);
                }
            }
        }

        private float GetWaterLevel()
        {
            return waterObject.position.y;
        }

        private bool IsAboveWater(float hookY, float waterLevel)
        {
            return hookY > waterLevel + waterLevelTolerance;
        }

        private void TrySpawnSplash(float waterLevel)
        {
            if (Time.time - lastSpawnTime < spawnCooldown)
            {
                return;
            }

            Vector3 splashPosition = new Vector3(
                hookPoint.position.x + splashOffset.x,
                waterLevel + splashOffset.y,
                hookPoint.position.z + splashOffset.z);

            GameObject particle = Instantiate(splashParticlePrefab, splashPosition, Quaternion.identity);
            Destroy(particle, particleLifetime);
            lastSpawnTime = Time.time;
        }
    }
}
