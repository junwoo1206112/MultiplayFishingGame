using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingAnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private Animator animator;
        [SerializeField] private string fishingParameter = "fishing";
        [SerializeField] private string hasFishParameter = "HasFish";
        [SerializeField] private string rodEquippedParameter = "RodEquipped";
        [SerializeField] private string rodTakeOutTrigger = "RodTakeOut";
        [SerializeField] private string rodPutAwayTrigger = "RodPutAway";

        private int fishingParameterHash;
        private int hasFishParameterHash;
        private int rodEquippedParameterHash;
        private int rodTakeOutTriggerHash;
        private int rodPutAwayTriggerHash;

        private bool hasFishingParameter;
        private bool hasHasFishParameter;
        private bool hasRodEquippedParameter;
        private bool hasRodTakeOutTrigger;
        private bool hasRodPutAwayTrigger;

        private bool currentIsFishingActive;
        private bool currentIsRodEquipped;

        public bool HasFishingParameter => hasFishingParameter;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            CacheAnimatorParameters();
        }

        public void SetFishingActive(bool isActive)
        {
            currentIsFishingActive = isActive;
            if (animator != null && hasFishingParameter)
            {
                animator.SetBool(fishingParameterHash, isActive);
            }
        }

        public void SetHasFish(bool hasFish)
        {
            if (animator != null && hasHasFishParameter)
            {
                animator.SetBool(hasFishParameterHash, hasFish);
            }
        }

        public void SetRodEquipped(bool isEquipped)
        {
            currentIsRodEquipped = isEquipped;
            if (animator == null) return;

            if (hasRodEquippedParameter)
            {
                animator.SetBool(rodEquippedParameterHash, isEquipped);
            }

            if (isEquipped && hasRodTakeOutTrigger)
            {
                animator.ResetTrigger(rodPutAwayTriggerHash);
                animator.SetTrigger(rodTakeOutTriggerHash);
            }
            else if (!isEquipped && hasRodPutAwayTrigger)
            {
                animator.ResetTrigger(rodTakeOutTriggerHash);
                animator.SetTrigger(rodPutAwayTriggerHash);
            }
        }

        public void CacheAnimatorParameters()
        {
            if (animator == null)
            {
                Debug.LogWarning("[FishingAnimationController] No Animator assigned/auto-found!");
                return;
            }

            fishingParameterHash = Animator.StringToHash(fishingParameter);
            hasFishParameterHash = Animator.StringToHash(hasFishParameter);
            rodEquippedParameterHash = Animator.StringToHash(rodEquippedParameter);
            rodTakeOutTriggerHash = Animator.StringToHash(rodTakeOutTrigger);
            rodPutAwayTriggerHash = Animator.StringToHash(rodPutAwayTrigger);

            hasFishingParameter = false;
            hasHasFishParameter = false;
            hasRodEquippedParameter = false;
            hasRodTakeOutTrigger = false;
            hasRodPutAwayTrigger = false;

            foreach (var p in animator.parameters)
            {
                if (p.nameHash == fishingParameterHash) hasFishingParameter = true;
                if (p.nameHash == hasFishParameterHash) hasHasFishParameter = true;
                if (p.nameHash == rodEquippedParameterHash) hasRodEquippedParameter = true;
                if (p.nameHash == rodTakeOutTriggerHash) hasRodTakeOutTrigger = true;
                if (p.nameHash == rodPutAwayTriggerHash) hasRodPutAwayTrigger = true;
            }

            Debug.Log($"[FishingAnimationController] Animator params: fishing={hasFishingParameter}, HasFish={hasHasFishParameter}, RodEquipped={hasRodEquippedParameter}, RodTakeOut={hasRodTakeOutTrigger}, RodPutAway={hasRodPutAwayTrigger}, Controller={animator.runtimeAnimatorController?.name ?? "NULL"}");
        }
    }
}