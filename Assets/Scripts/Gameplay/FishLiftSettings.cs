using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    [System.Serializable]
    public class FishLiftSettings
    {
        [Header("Approach")]
        [Tooltip("Distance from the player where the fish is pulled near the water surface.")]
        public float approachDistance = 0.5f;

        [Tooltip("Time used to pull the fish toward the player near the water surface.")]
        public float approachDuration = 0.6f;

        [Tooltip("Extra distance that can be searched toward the water if the approach point is under terrain.")]
        public float shoreSearchDistance = 4f;

        [Tooltip("Ground must be below the water surface by this margin, otherwise the point is treated as inside the shore.")]
        public float shoreSurfaceClearance = 0.2f;

        [Header("Lift")]
        [Tooltip("Height added above the water surface during the lift.")]
        public float liftHeight = 0.3f;

        [Tooltip("Forward distance moved while lifting the fish.")]
        public float liftForwardOffset = 0.2f;

        [Tooltip("Time used for the lift motion.")]
        public float liftDuration = 0.5f;

        [Header("Hand Attach")]
        [Tooltip("Time used to move from the lifted position to the hand attach position.")]
        public float attachDuration = 0.3f;

        [Tooltip("World offset applied to the final hand attach position.")]
        public Vector3 handOffset = new Vector3(0f, 0.1f, 0.3f);
    }
}
