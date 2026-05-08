using UnityEngine;

namespace MultiplayFishing.Data.Models
{
    [CreateAssetMenu(fileName = "NewBaitData", menuName = "Fishing/Bait Data")]
    public class BaitDataSO : ScriptableObject
    {
        public string id;
        public string baitName;
        public Sprite icon;

        [Header("등급 (★ 표기)")]
        [Tooltip("예: ★★★★★ (5성), ★★★★ (4성)")]
        public string rank;

        [Header("상점 정보")]
        public int price;

        [Header("스탯")]
        public string[] attractionFishIds;
        public float catchChanceBonus;

        [Header("설명")]
        [TextArea(3, 10)]
        public string description;
    }
}
