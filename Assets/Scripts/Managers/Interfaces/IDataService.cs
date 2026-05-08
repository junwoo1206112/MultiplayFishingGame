using MultiplayFishing.Data.Models;
using System.Collections.Generic;

namespace MultiplayFishing.Core
{
    public interface IDataService
    {
        void LoadData();
        FishDataSO GetFishData(string id);
        List<FishDataSO> GetAllFishData();
        RodDataSO GetRodData(string id);
        List<RodDataSO> GetAllRodData();
        BaitDataSO GetBaitData(string id);
        List<BaitDataSO> GetAllBaitData();
    }
}
