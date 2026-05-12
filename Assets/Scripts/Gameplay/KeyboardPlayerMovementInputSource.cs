using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    public sealed class KeyboardPlayerMovementInputSource : IPlayerMovementInputSource
    {
        public Vector2 ReadMove()
        {
            if (Keyboard.current == null) return Vector2.zero;

            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            return input.normalized;
        }

        public bool ReadSprint()
        {
            return Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        }
    }
}
