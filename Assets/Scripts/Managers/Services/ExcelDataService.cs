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
            // 엑셀 데이터가 정상적으로 입력된(이름이 있고 확률이 0보다 큰) 물고기만 반환
            return fishDataMap.Values
                .Where(fish => !string.IsNullOrEmpty(fish.fishName) && fish.catchChance > 0)
                .ToList();
        }
    }
}
