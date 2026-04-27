using MultiplayFishing.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MultiplayFishing.Core
{
    public class UserStorageService : IUserService
    {
        private UserSaveData userData = new UserSaveData();
        private readonly string savePath;
        private readonly IDataService dataService;

        public UserSaveData UserData => userData;
        public event Action OnDataChanged;

        public UserStorageService(IDataService dataService)
        {
            this.dataService = dataService;
            savePath = Path.Combine(Application.persistentDataPath, "UserData.json");
            Load();
        }

        public void AddFish(string fishId, float length)
        {
            // 1. 인벤토리 추가
            userData.AddToInventory(fishId, length);
            
            // 2. 경험치 획득 및 레벨업 체크
            var fishInfo = dataService.GetFishData(fishId);
            if (fishInfo != null)
            {
                bool levelUp = userData.AddExp(fishInfo.expReward);
                Debug.Log($"[UserStorageService] Gained {fishInfo.expReward} EXP.");
                if (levelUp) Debug.Log($"<color=yellow><b>[LEVEL UP!]</b></color> Tier {userData.currentTier}");
            }
            
            Save();
            OnDataChanged?.Invoke(); // UI에 알림
        }

        public void SellFish(string instanceId)
        {
            InventoryItem item = userData.inventory.Find(x => x.instanceId == instanceId);
            if (item != null)
            {
                var fishInfo = dataService.GetFishData(item.fishId);
                if (fishInfo != null)
                {
                    userData.gold += fishInfo.sellPrice;
                    userData.inventory.Remove(item);
                    Debug.Log($"[UserStorageService] Sold {fishInfo.fishName}. Current Gold: {userData.gold}");
                    Save();
                    OnDataChanged?.Invoke(); // UI에 알림
                }
            }
        }

        public void SellAllFish()
        {
            if (userData.inventory.Count == 0) return;

            int totalGain = 0;
            foreach (var item in userData.inventory)
            {
                var fishInfo = dataService.GetFishData(item.fishId);
                if (fishInfo != null)
                {
                    totalGain += fishInfo.sellPrice;
                }
            }

            userData.gold += totalGain;
            userData.inventory.Clear();
            
            Debug.Log($"[UserStorageService] Bulk sold all fish. Gained {totalGain}G. Total Gold: {userData.gold}");
            
            Save();
            OnDataChanged?.Invoke(); // UI에 알림
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(userData, true);
            File.WriteAllText(savePath, json);
        }

        public void Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                userData = JsonUtility.FromJson<UserSaveData>(json);
            }
            else
            {
                userData = new UserSaveData();
            }
        }
    }
}
