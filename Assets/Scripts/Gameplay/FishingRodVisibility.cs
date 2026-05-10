using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingRodVisibility : MonoBehaviour
    {
        [Header("Rod Visibility")]
        [SerializeField] private GameObject rodVisualRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private string rodHideStateName = "rod-out";
        [SerializeField] private string rodShowStateName = "rod-in";
        [SerializeField, Range(0f, 1f)] private float rodHideNormalizedTime = 0.95f;

        private int rodHideStateHash;
        private int rodShowStateHash;
        private bool wasInRodHideState;
        private bool wasInRodShowState;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            rodHideStateHash = Animator.StringToHash(rodHideStateName);
            rodShowStateHash = Animator.StringToHash(rodShowStateName);
            ResolveRodVisualRoot();
        }

        private void Update()
        {
            UpdateRodVisibilityFromAnimator();
        }

        public void SetRodVisible(bool visible)
        {
            if (rodVisualRoot != null && rodVisualRoot.activeSelf != visible)
            {
                rodVisualRoot.SetActive(visible);
            }
        }

        // Animation Events에서 호출
        public void HideRodEvent()
        {
            SetRodVisible(false);
        }

        public void ShowRodEvent()
        {
            SetRodVisible(true);
        }

        private void ResolveRodVisualRoot()
        {
            if (rodVisualRoot != null) return;

            Transform[] children = GetComponentsInChildren<Transform>(true);
            // Try multiple common rod names
            foreach (Transform child in children)
            {
                string lower = child.name.ToLower();
                if (lower.Contains("rod") || lower.Contains("fishing") || lower.Contains("낚시"))
                {
                    rodVisualRoot = child.gameObject;
                    Debug.Log($"[FishingRodVisibility] Auto-resolved rod visual: {child.name}");
                    return;
                }
            }

            Debug.LogWarning("[FishingRodVisibility] No rod visual found. Searching for any child...");
            if (transform.childCount > 0)
            {
                rodVisualRoot = transform.GetChild(0).gameObject;
                Debug.Log($"[FishingRodVisibility] Fallback to first child: {rodVisualRoot.name}");
            }
        }

        private void UpdateRodVisibilityFromAnimator()
        {
            if (animator == null || rodVisualRoot == null) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isInRodHideState = stateInfo.shortNameHash == rodHideStateHash;
            bool isInRodShowState = stateInfo.shortNameHash == rodShowStateHash;

            if (isInRodShowState && !wasInRodShowState)
            {
                SetRodVisible(true);
            }

            if (isInRodHideState && stateInfo.normalizedTime >= rodHideNormalizedTime)
            {
                SetRodVisible(false);
            }

            wasInRodHideState = isInRodHideState;
            wasInRodShowState = isInRodShowState;
        }
    }
}