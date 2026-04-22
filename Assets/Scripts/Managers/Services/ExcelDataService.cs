using MultiplayFishing.Data.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayFishing.Core
{
    public class ExcelDataService : IDataService
    {
        private Dictionary<string, FishDataSO> fishDataMap = new Dictionary<string, FishDataSO>();

        public void LoadData()
        {
            fishDataMap.Clear();
            var loadedFishes = Resources.LoadAll<FishDataSO>("Data/Fish");
            foreach (var fish in loadedFishes)
            {
                if (!fishDataMap.ContainsKey(fish.id))
                {
                    fishDataMap.Add(fish.id, fish);
                }
            }
            Debug.Log($"[ExcelDataService] Loaded {fishDataMap.Count} fish data items.");
        }

        public FishDataSO GetFishData(string id)
        {
            fishDataMap.TryGetValue(id, out var data);
            return data;
        }

        public List<FishDataSO> GetAllFishData()
        {
            return fishDataMap.Values.ToList();
        }
    }
}
