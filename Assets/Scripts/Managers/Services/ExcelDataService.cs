using MultiplayFishing.Data.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayFishing.Core
{
    public class ExcelDataService : IDataService
    {
        private Dictionary<string, FishDataSO> fishDataMap = new Dictionary<string, FishDataSO>();
        private Dictionary<string, RodDataSO> rodDataMap = new Dictionary<string, RodDataSO>();
        private Dictionary<string, BaitDataSO> baitDataMap = new Dictionary<string, BaitDataSO>();

        public void LoadData()
        {
            fishDataMap.Clear();
            rodDataMap.Clear();
            baitDataMap.Clear();

            var loadedFishes = Resources.LoadAll<FishDataSO>("Data/Fish");
            foreach (var fish in loadedFishes)
            {
                if (fishDataMap.ContainsKey(fish.id)) continue;

                if (string.IsNullOrEmpty(fish.rank) || fish.catchChance <= 0f)
                {
                    Debug.LogWarning($"[ExcelDataService] Skipping ghost fish: '{fish.fishName}' (id: {fish.id}) — not in Excel data.");
                    continue;
                }

                fishDataMap.Add(fish.id, fish);

                if (fish.fishIcon == null)
                    Debug.LogWarning($"[ExcelDataService] Fish '{fish.fishName}' (id: {fish.id}) has NO icon sprite. Run Tools > Fish > Match Icons to Assets.");
            }

            var loadedRods = Resources.LoadAll<RodDataSO>("Data/Rods");
            foreach (var rod in loadedRods)
            {
                if (!rodDataMap.ContainsKey(rod.id))
                    rodDataMap.Add(rod.id, rod);
            }

            var loadedBaits = Resources.LoadAll<BaitDataSO>("Data/Baits");
            foreach (var bait in loadedBaits)
            {
                if (!baitDataMap.ContainsKey(bait.id))
                    baitDataMap.Add(bait.id, bait);
            }

            Debug.Log($"[ExcelDataService] Loaded {fishDataMap.Count} fish, {rodDataMap.Count} rods, {baitDataMap.Count} baits.");
        }

        public FishDataSO GetFishData(string id)
        {
            fishDataMap.TryGetValue(id, out var data);
            return data;
        }

        public List<FishDataSO> GetAllFishData()
        {
            return fishDataMap.Values
                .Where(fish => !string.IsNullOrEmpty(fish.fishName) && fish.catchChance > 0)
                .ToList();
        }

        public RodDataSO GetRodData(string id)
        {
            rodDataMap.TryGetValue(id, out var data);
            return data;
        }

        public List<RodDataSO> GetAllRodData()
        {
            return rodDataMap.Values
                .Where(rod => !string.IsNullOrEmpty(rod.rodName))
                .ToList();
        }

        public BaitDataSO GetBaitData(string id)
        {
            baitDataMap.TryGetValue(id, out var data);
            return data;
        }

        public List<BaitDataSO> GetAllBaitData()
        {
            return baitDataMap.Values
                .Where(bait => !string.IsNullOrEmpty(bait.baitName))
                .ToList();
        }
    }
}
