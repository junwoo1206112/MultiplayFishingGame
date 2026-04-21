using UnityEngine;

namespace MultiplayFishing.Data
{
    [CreateAssetMenu(fileName = "NewChannelData", menuName = "MultiplayFishing/ChannelData")]
    public class ChannelData : ScriptableObject
    {
        [Header("Channel Info")]
        public string channelId;
        public string displayName;
        public int maxRooms = 20;
        public Color32 color = Color.white;
    }
}
