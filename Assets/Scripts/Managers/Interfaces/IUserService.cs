using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Core
{
    public interface IUserService
    {
        UserSaveData UserData { get; }
        void AddFish(string fishId);
        void SellFish(string fishId);
        void Save();
        void Load();
    }
}
