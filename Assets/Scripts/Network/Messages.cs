using Mirror;
using UnityEngine;

namespace MultiplayFishing.Network
{
    public struct ChannelSelectMessage : NetworkMessage
    {
        public string channelId;
    }

    public struct RoomCreateMessage : NetworkMessage
    {
        public string roomName;
        public int maxPlayers;
        public bool isPublic;
        public string password;
    }

    public struct RoomListUpdateMessage : NetworkMessage
    {
        public string roomName;
        public int currentPlayerCount;
        public int maxPlayers;
        public bool isPublic;
        public bool isInProgress;
    }

    public struct PlayerNameMessage : NetworkMessage
    {
        public string name;
    }
}
