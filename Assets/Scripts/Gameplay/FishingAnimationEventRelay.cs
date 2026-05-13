using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public sealed class FishingAnimationEventRelay : MonoBehaviour
    {
        private FishingController fishingController;

        private void Awake()
        {
            ResolveController();
        }

        public void Initialize(FishingController controller)
        {
            fishingController = controller;
        }

        public void OnCastRelease()
        {
            ResolveController();
            fishingController?.OnCastRelease();
        }

        private void ResolveController()
        {
            if (fishingController != null)
            {
                return;
            }

            fishingController = GetComponentInParent<FishingController>();
        }
    }
}
