using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingSplashController
    {
        private readonly ParticleSystem fishingSplashParticle;
        private ParticleSystem activeSplashParticle;
        private Vector3 pendingSplashPosition;

        public FishingSplashController(ParticleSystem fishingSplashParticle)
        {
            this.fishingSplashParticle = fishingSplashParticle;
            activeSplashParticle = ResolveSceneParticle();
        }

        public void Reset()
        {
            ParticleSystem particle = ResolveSceneParticle();
            if (particle == null)
            {
                return;
            }

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Clear(true);
        }

        public void UpdatePendingPosition(
            bool hasSurfaceHit,
            Vector3 surfaceHitPoint,
            Vector3 fallbackPosition,
            Vector3 splashWorldOffset,
            bool clampToSurface,
            float minimumSplashHeightOffset)
        {
            Vector3 splashBasePosition = hasSurfaceHit ? surfaceHitPoint : fallbackPosition;
            pendingSplashPosition = splashBasePosition + splashWorldOffset;

            if (clampToSurface)
            {
                float waterSurfaceY = hasSurfaceHit ? surfaceHitPoint.y : splashBasePosition.y;
                pendingSplashPosition.y = Mathf.Max(
                    pendingSplashPosition.y,
                    waterSurfaceY + minimumSplashHeightOffset);
            }

            ParticleSystem particle = ResolveSceneParticle();
            if (particle != null)
            {
                particle.transform.position = pendingSplashPosition;
            }
        }

        public void Play()
        {
            ParticleSystem particle = ResolveSceneParticle();
            if (particle == null)
            {
                return;
            }

            if (!particle.gameObject.activeSelf)
            {
                particle.gameObject.SetActive(true);
            }

            particle.transform.position = pendingSplashPosition;
            particle.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Clear(true);
            particle.Play(true);
        }

        private ParticleSystem ResolveSceneParticle()
        {
            if (activeSplashParticle != null)
            {
                return activeSplashParticle;
            }

            if (fishingSplashParticle == null)
            {
                return null;
            }

            GameObject sourceObject = fishingSplashParticle.gameObject;
            bool isSceneObject = sourceObject.scene.IsValid() && sourceObject.scene.isLoaded;
            activeSplashParticle = isSceneObject
                ? fishingSplashParticle
                : Object.Instantiate(fishingSplashParticle);

            activeSplashParticle.gameObject.name = $"{sourceObject.name} Runtime";
            activeSplashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            activeSplashParticle.Clear(true);
            return activeSplashParticle;
        }
    }
}
