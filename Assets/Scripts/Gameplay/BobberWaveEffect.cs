using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// hookPoint에 달린 물결 이펙트(SpriteRenderer)를 자연스럽게 연출합니다.
    /// 실제 찌가 물에 떠 있을 때처럼:
    ///  - 잔물결이 주기적으로 퍼져나가고 (scale pulse)
    ///  - 퍼지면서 투명해지며 (alpha fade)
    ///  - 여러 겹의 파동이 시차를 두고 반복됩니다.
    /// </summary>
    public class BobberWaveEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Wave Scale")]
        [Tooltip("잔물결 최소 크기")]
        [SerializeField] private float minScale = 0.15f;
        [Tooltip("잔물결 최대 크기 (퍼져나간 최대 반경)")]
        [SerializeField] private float maxScale = 0.6f;

        [Header("Wave Timing")]
        [Tooltip("한 번 물결이 퍼지는 데 걸리는 시간(초)")]
        [SerializeField] private float waveDuration = 2.0f;
        [Tooltip("물결 반복 사이 대기 시간(초)")]
        [SerializeField] private float wavePause = 0.5f;
        [Tooltip("2차 물결의 시작 지연 비율 (0~1). 첫 물결이 이 비율만큼 진행됐을 때 두 번째 물결 시작")]
        [SerializeField] private float secondaryWaveDelay = 0.35f;

        [Header("Alpha")]
        [Tooltip("물결 시작 시 투명도")]
        [SerializeField] private float alphaStart = 0.5f;
        [Tooltip("물결 끝 시 투명도")]
        [SerializeField] private float alphaEnd = 0f;

        [Header("Position")]
        [Tooltip("수면 위 높이 오프셋")]
        [SerializeField] private float heightOffset = 0.01f;

        [Header("Randomness")]
        [Tooltip("물결 크기의 랜덤 변동 범위")]
        [SerializeField] private float scaleRandomness = 0.08f;
        [Tooltip("물결 주기의 랜덤 변동 범위(초)")]
        [SerializeField] private float timingRandomness = 0.3f;

        private Vector3 initialLocalPosition;
        private float primaryPhase;
        private float secondaryPhase;
        private float primaryCycleDuration;
        private float secondaryCycleDuration;
        private float primaryScaleJitter;
        private float secondaryScaleJitter;
        private bool isEffectActive;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            initialLocalPosition = transform.localPosition;
            RandomizeCycle(ref primaryCycleDuration, ref primaryScaleJitter);
            RandomizeCycle(ref secondaryCycleDuration, ref secondaryScaleJitter);
            
            // 초기에는 보이지 않도록 설정
            SetEffectActive(false);
        }

        public void SetEffectActive(bool active)
        {
            isEffectActive = active;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = active;
            }
            
            if (active)
            {
                // 활성화될 때 페이즈 초기화
                primaryPhase = 0f;
                secondaryPhase = 0f;
            }
        }

        private void LateUpdate()
        {
            if (!isEffectActive) return;
            // — 위치: 항상 부모(hookPoint) 바로 위, 수평으로 고정 —
            Vector3 parentPos = transform.parent != null ? transform.parent.position : transform.position;
            transform.position = parentPos + new Vector3(initialLocalPosition.x, heightOffset, initialLocalPosition.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // — 1차 물결 —
            primaryPhase += Time.deltaTime;
            float totalPrimaryCycle = primaryCycleDuration + wavePause;
            if (primaryPhase >= totalPrimaryCycle)
            {
                primaryPhase -= totalPrimaryCycle;
                RandomizeCycle(ref primaryCycleDuration, ref primaryScaleJitter);
            }

            float primaryT = Mathf.Clamp01(primaryPhase / primaryCycleDuration);
            float primaryScale = EvaluateWaveScale(primaryT, primaryScaleJitter);
            float primaryAlpha = EvaluateWaveAlpha(primaryT);

            // — 2차 물결 (시차) —
            secondaryPhase += Time.deltaTime;
            float totalSecondaryCycle = secondaryCycleDuration + wavePause;
            float secondaryOffset = secondaryWaveDelay * secondaryCycleDuration;

            if (secondaryPhase >= totalSecondaryCycle + secondaryOffset)
            {
                secondaryPhase -= totalSecondaryCycle;
                RandomizeCycle(ref secondaryCycleDuration, ref secondaryScaleJitter);
            }

            float delayedPhase = secondaryPhase - secondaryOffset;
            float secondaryT = delayedPhase > 0f ? Mathf.Clamp01(delayedPhase / secondaryCycleDuration) : 0f;
            float secondaryScale = EvaluateWaveScale(secondaryT, secondaryScaleJitter) * 0.6f;
            float secondaryAlpha = EvaluateWaveAlpha(secondaryT) * 0.4f;

            // — 합성: 큰 쪽 scale, 알파는 합산 —
            float finalScale = Mathf.Max(primaryScale, secondaryScale);
            float finalAlpha = Mathf.Clamp01(primaryAlpha + secondaryAlpha);

            transform.localScale = new Vector3(finalScale, finalScale, finalScale);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = finalAlpha;
                spriteRenderer.color = color;
            }
        }

        /// <summary>
        /// 물결이 퍼지는 easing curve: 처음엔 빠르게, 끝엔 느리게 (EaseOut).
        /// </summary>
        private float EvaluateWaveScale(float t, float jitter)
        {
            if (t >= 1f) return 0f;

            // EaseOutQuad: 자연스럽게 감속하며 퍼져나감
            float eased = 1f - (1f - t) * (1f - t);
            return Mathf.Lerp(minScale, maxScale + jitter, eased);
        }

        /// <summary>
        /// 물결이 퍼지면서 투명해지는 curve.
        /// 처음 20%는 alphaStart 유지 → 이후 점진적 페이드아웃.
        /// </summary>
        private float EvaluateWaveAlpha(float t)
        {
            if (t >= 1f) return 0f;

            // 초반 20%는 불투명 유지, 이후 페이드
            float fadeStart = 0.2f;
            if (t < fadeStart) return alphaStart;

            float fadeT = (t - fadeStart) / (1f - fadeStart);
            // EaseInQuad: 끝으로 갈수록 빠르게 사라짐
            float eased = fadeT * fadeT;
            return Mathf.Lerp(alphaStart, alphaEnd, eased);
        }

        private void RandomizeCycle(ref float cycleDuration, ref float scaleJitter)
        {
            cycleDuration = waveDuration + Random.Range(-timingRandomness, timingRandomness);
            cycleDuration = Mathf.Max(0.5f, cycleDuration);
            scaleJitter = Random.Range(-scaleRandomness, scaleRandomness);
        }
    }
}
