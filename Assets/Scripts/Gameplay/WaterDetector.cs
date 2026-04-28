using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class WaterDetector : MonoBehaviour
    {
        [Header("Ocean Detection")]
        [SerializeField] private LayerMask oceanLayer;
        [SerializeField] private float forwardCheckDistance = 3f;
        [SerializeField] private float downCheckDistance = 5f;

        private void Awake()
        {
            if (oceanLayer == 0)
                oceanLayer = 1 << LayerMask.NameToLayer("Ocean");
        }

        public bool CanFish()
        {
            int mask = oceanLayer != 0 ? oceanLayer.value : (1 << LayerMask.NameToLayer("Ocean"));
            Vector3 checkPos = transform.position + transform.forward * forwardCheckDistance;
            return Physics.Raycast(checkPos, Vector3.down, downCheckDistance, mask);
        }
    }
}
