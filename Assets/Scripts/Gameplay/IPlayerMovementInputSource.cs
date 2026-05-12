using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public interface IPlayerMovementInputSource
    {
        Vector2 ReadMove();
        bool ReadSprint();
    }
}
