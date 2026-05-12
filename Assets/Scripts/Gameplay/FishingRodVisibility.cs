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
            if (animator == null) animator = GetComponentInChildren<Animator>();
            rodHideStateHash = Animator.StringToHash(rodHideStateName);
            rodShowStateHash = Animator.StringToHash(rodShowStateName);
            ValidateAssignedRodRoot();
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

        // Called by Animation Events on rod-in/rod-out.
        public void HideRodEvent()
        {
            SetRodVisible(false);
        }

        public void ShowRodEvent()
        {
            SetRodVisible(true);
        }

        public void RefreshReferences()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            ValidateAssignedRodRoot();
            ResolveRodVisualRoot();
        }

        private void ValidateAssignedRodRoot()
        {
            if (rodVisualRoot == null) return;
            if (rodVisualRoot.transform.root == transform.root) return;

            Debug.LogWarning($"[FishingRodVisibility] Ignoring rod visual outside this player hierarchy: {rodVisualRoot.name}");
            rodVisualRoot = null;
        }

        private void ResolveRodVisualRoot()
        {
            if (rodVisualRoot != null) return;

            Transform rodSocket = FindChildRecursive(transform.root, "RodSocket");
            if (rodSocket != null)
            {
                Transform model = FindBestRodVisual(rodSocket);
                rodVisualRoot = model != null ? model.gameObject : rodSocket.gameObject;
                Debug.Log($"[FishingRodVisibility] Auto-resolved rod visual: {rodVisualRoot.name}");
                return;
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == transform) continue;
                if (IsRodVisualCandidate(child))
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

        private static Transform FindChildRecursive(Transform parent, string exactName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == exactName)
                {
                    return child;
                }

                Transform result = FindChildRecursive(child, exactName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Transform FindBestRodVisual(Transform rodSocket)
        {
            foreach (Transform child in rodSocket.GetComponentsInChildren<Transform>(true))
            {
                if (child == rodSocket) continue;
                if (IsRodVisualCandidate(child))
                {
                    return child;
                }
            }

            return null;
        }

        private static bool IsRodVisualCandidate(Transform child)
        {
            string lower = child.name.ToLowerInvariant();
            if (lower.Contains("line") ||
                lower.Contains("hook") ||
                lower.Contains("tip") ||
                lower.Contains("guide") ||
                lower.Contains("socket") ||
                lower.Contains("lure"))
            {
                return false;
            }

            return lower == "model" ||
                   lower.Contains("rod") ||
                   lower.Contains("fishingrod") ||
                   lower.Contains("sk_fishingrod");
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
