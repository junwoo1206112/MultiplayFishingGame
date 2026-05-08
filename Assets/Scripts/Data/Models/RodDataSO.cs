using UnityEngine;

namespace MultiplayFishing.Data.Models
{
    [CreateAssetMenu(fileName = "NewRodData", menuName = "Fishing/Rod Data")]
    public class RodDataSO : ScriptableObject
    {
        public string id;
        public string rodName;
        public Sprite icon;

        [Header("등급 (★ 표기)")]
        [Tooltip("예: ★★★★★ (5성), ★★★★ (4성)")]
        public string rank;

        [Header("상점 정보")]
        public int price;

        [Header("스탯")]
        public float castDistanceBonus;
        public float catchChanceBonus;
        public float durability;

        [Header("설명")]
        [TextArea(3, 10)]
        public string description;
    }
}
