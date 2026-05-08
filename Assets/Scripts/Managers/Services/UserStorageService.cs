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
            OnDataChanged?.Invoke();
        }

        public bool BuyItem(ShopItemType itemType, string itemId)
        {
            int price = 0;

            if (itemType == ShopItemType.Rod)
            {
                if (IsRodOwned(itemId))
                {
                    Debug.LogWarning($"[UserStorageService] Rod {itemId} already owned.");
                    return false;
                }
                var rodData = dataService.GetRodData(itemId);
                if (rodData == null) return false;
                price = rodData.price;
            }
            else if (itemType == ShopItemType.Bait)
            {
                if (IsBaitOwned(itemId))
                {
                    Debug.LogWarning($"[UserStorageService] Bait {itemId} already owned.");
                    return false;
                }
                var baitData = dataService.GetBaitData(itemId);
                if (baitData == null) return false;
                price = baitData.price;
            }

            if (userData.gold < price)
            {
                Debug.Log($"[UserStorageService] Not enough gold. Need {price}, have {userData.gold}.");
                return false;
            }

            userData.gold -= price;

            if (itemType == ShopItemType.Rod)
                userData.ownedRodIds.Add(itemId);
            else
                userData.ownedBaitIds.Add(itemId);

            Debug.Log($"[UserStorageService] Purchased {itemType} {itemId}. Gold left: {userData.gold}");
            Save();
            OnDataChanged?.Invoke();
            return true;
        }

        public bool EquipRod(string rodId)
        {
            if (!IsRodOwned(rodId)) return false;
            userData.equippedRodId = rodId;
            Debug.Log($"[UserStorageService] Equipped rod: {rodId}");
            Save();
            OnDataChanged?.Invoke();
            return true;
        }

        public bool EquipBait(string baitId)
        {
            if (!IsBaitOwned(baitId)) return false;
            userData.equippedBaitId = baitId;
            Debug.Log($"[UserStorageService] Equipped bait: {baitId}");
            Save();
            OnDataChanged?.Invoke();
            return true;
        }

        public void UnequipRod()
        {
            userData.equippedRodId = "";
            Debug.Log($"[UserStorageService] Unequipped rod.");
            Save();
            OnDataChanged?.Invoke();
        }

        public void UnequipBait()
        {
            userData.equippedBaitId = "";
            Debug.Log($"[UserStorageService] Unequipped bait.");
            Save();
            OnDataChanged?.Invoke();
        }

        public bool IsRodOwned(string rodId)
        {
            return userData.ownedRodIds.Contains(rodId);
        }

        public bool IsBaitOwned(string baitId)
        {
            return userData.ownedBaitIds.Contains(baitId);
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
