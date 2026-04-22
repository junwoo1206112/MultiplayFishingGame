using MultiplayFishing.Data.Models;
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

        public UserStorageService(IDataService dataService)
        {
            this.dataService = dataService;
            savePath = Path.Combine(Application.persistentDataPath, "UserData.json");
        }

        public void AddFish(string fishId)
        {
            // 인벤토리에 추가
            userData.inventory.Add(fishId);
            
            // 도감에 등록 (이미 있으면 무시됨)
            userData.AddToEncyclopedia(fishId);
            
            Debug.Log($"[UserStorageService] Fish {fishId} added to inventory and encyclopedia.");
            Save();
        }

        public void SellFish(string fishId)
        {
            if (userData.inventory.Remove(fishId))
            {
                var fishInfo = dataService.GetFishData(fishId);
                if (fishInfo != null)
                {
                    userData.gold += fishInfo.sellPrice;
                    Debug.Log($"[UserStorageService] Sold {fishId} for {fishInfo.sellPrice}G. Current Gold: {userData.gold}");
                }
                // 도감 정보는 유지함 (삭제하지 않음)
                Save();
            }
            else
            {
                Debug.LogWarning($"[UserStorageService] Fish {fishId} not found in inventory.");
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(userData, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[UserStorageService] Data saved to: {savePath}");
        }

        public void Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                userData = JsonUtility.FromJson<UserSaveData>(json);
                Debug.Log("[UserStorageService] Data loaded.");
            }
            else
            {
                userData = new UserSaveData();
                Debug.Log("[UserStorageService] No save file found. Created new data.");
            }
        }
    }
}
