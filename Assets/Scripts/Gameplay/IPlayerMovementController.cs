namespace MultiplayFishing.Gameplay
{
    public interface IPlayerMovementController
    {
        bool IsMovementBlocked { get; }
        bool IsSprinting { get; }
        void SetMovementBlocked(bool blocked);
    }
}
