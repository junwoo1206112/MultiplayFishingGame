using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingSplashController
    {
        private readonly ParticleSystem fishingSplashParticle;
        private Vector3 pendingSplashPosition;

        public FishingSplashController(ParticleSystem fishingSplashParticle)
        {
            this.fishingSplashParticle = fishingSplashParticle;
        }

        public void Reset()
        {
            if (fishingSplashParticle == null)
            {
                return;
            }

            fishingSplashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fishingSplashParticle.Clear(true);
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

            if (fishingSplashParticle != null)
            {
                fishingSplashParticle.transform.position = pendingSplashPosition;
            }
        }

        public void Play()
        {
            if (fishingSplashParticle == null)
            {
                return;
            }

            if (!fishingSplashParticle.gameObject.activeSelf)
            {
                fishingSplashParticle.gameObject.SetActive(true);
            }

            fishingSplashParticle.transform.position = pendingSplashPosition;
            fishingSplashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fishingSplashParticle.Clear(true);
            fishingSplashParticle.Play();
        }
    }
}
