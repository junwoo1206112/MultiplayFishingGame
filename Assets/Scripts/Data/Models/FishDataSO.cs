using UnityEngine;

namespace MultiplayFishing.Data.Models
{
    [CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
    public class FishDataSO : ScriptableObject
    {
        public string id;
        public string fishName;
        public Sprite fishIcon; 
        
        [Header("등급 (★ 표기)")]
        [Tooltip("예: ★★★★★ (5성), ★★★★ (4성)")]
        public string rank; // ★, ★★, ★★★, ★★★★, ★★★★★
        
        [Header("포획 데이터")]
        public float catchChance; // 등급에 따른 확률 (%)
        public int expReward;     // 잡았을 때 획득 경험치
        public int sellPrice;     // 상점 판매 가격 (Gold)
        
        [Header("크기 범위 (cm)")]
        public float minSize; 
        public float maxSize; 

        [Header("설명")]
        [TextArea(3, 10)]
        public string description;
    }
}
