using UnityEngine;

namespace MultiplayFishing.Data.Models
{
    [CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
    public class FishDataSO : ScriptableObject
    {
        public string id;
        public string fishName;
        public Sprite fishIcon; 
        public string rank; // S, A, B, C, D
        public float catchChance; // %
        public int sellPrice; // Gold
        
        [Header("Size Range (cm)")]
        public float minSize; 
        public float maxSize; 

        [Header("Progression")]
        public int expReward; // 이 물고기를 잡았을 때 얻는 경험치
        
        [TextArea(3, 10)]
        public string description;
    }
}
