using System;
using System.Collections.Generic;

namespace MultiplayFishing.Data.Models
{
    [Serializable]
    public class UserSaveData
    {
        public List<string> inventory = new List<string>();
        public List<string> encyclopedia = new List<string>(); // 도감 (ID 목록)
        public int gold = 0;

        public void AddToEncyclopedia(string fishId)
        {
            if (!encyclopedia.Contains(fishId))
            {
                encyclopedia.Add(fishId);
            }
        }
    }
}
