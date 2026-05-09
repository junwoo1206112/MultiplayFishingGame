using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingBobbingEffect : MonoBehaviour
    {
        [Header("Bobbing Settings")]
        [SerializeField] private Transform hookPoint;
        [SerializeField] private bool enableBobbing = true;
        [SerializeField] private float bobAmplitude = 0.03f;
        [SerializeField] private float bobFrequency = 1.2f;
        [SerializeField] private float bobSecondaryRatio = 0.3f;
        [SerializeField] private float bobSecondaryFrequencyMultiplier = 1.7f;
        [SerializeField] private float swayAmplitude = 0.01f;
        [SerializeField] private float swayFrequency = 0.8f;
        [SerializeField] private float bobFadeInDuration = 0.5f;

        private Vector3 bobbingBasePosition;
        private float bobbingStartTime;
        private bool isBobbingActive;

        public bool IsBobbingActive => isBobbingActive;

        private void Awake()
        {
            if (hookPoint == null)
            {
                // FishingController에서 hookPoint를 찾음
                Transform[] children = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.name.Contains("Hook") || child.name.Contains("hook"))
                    {
                        hookPoint = child;
                        break;
                    }
                }
            }
        }

        public void SetHookPoint(Transform newHookPoint)
        {
            hookPoint = newHookPoint;
        }

        public void StartBobbing(Vector3 basePosition)
        {
            if (!enableBobbing || hookPoint == null) return;

            bobbingBasePosition = basePosition;
            bobbingStartTime = Time.time;
            isBobbingActive = true;
        }

        public void StopBobbing()
        {
            if (!isBobbingActive) return;

            isBobbingActive = false;

            // 보빙 종료 시 기준 위치로 복원
            if (hookPoint != null)
            {
                hookPoint.position = bobbingBasePosition;
            }
        }

        public void UpdateBobbing()
        {
            if (!isBobbingActive || hookPoint == null) return;

            float elapsed = Time.time - bobbingStartTime;

            // 부드러운 페이드인
            float fadeIn = bobFadeInDuration > 0f
                ? Mathf.Clamp01(elapsed / bobFadeInDuration)
                : 1f;

            // 1차 파동 + 2차 파동 조합 (자연스러운 불규칙 움직임)
            float t = Time.time;
            float primaryWave = Mathf.Sin(t * bobFrequency * Mathf.PI * 2f);
            float secondaryWave = Mathf.Sin(t * bobFrequency * bobSecondaryFrequencyMultiplier * Mathf.PI * 2f);
            float bobY = (primaryWave + secondaryWave * bobSecondaryRatio) * bobAmplitude * fadeIn;

            // 수평 흔들림 (X, Z)
            float swayX = Mathf.Sin(t * swayFrequency * Mathf.PI * 2f) * swayAmplitude * fadeIn;
            float swayZ = Mathf.Cos(t * swayFrequency * 0.8f * Mathf.PI * 2f) * swayAmplitude * 0.5f * fadeIn;

            hookPoint.position = bobbingBasePosition + new Vector3(swayX, bobY, swayZ);
        }
    }
}