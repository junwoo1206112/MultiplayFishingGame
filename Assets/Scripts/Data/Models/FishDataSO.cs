using UnityEngine;

namespace MultiplayFishing.Data.Models
{
    [CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
    public class FishDataSO : ScriptableObject
    {
        public string id;
        public string fishName;
        public string rank; // A, B, C, D, E
        public float catchChance; // %
        public int sellPrice; // Gold
        public float lengthCm; // cm
        [TextArea(3, 10)]
        public string description;
    }
}
