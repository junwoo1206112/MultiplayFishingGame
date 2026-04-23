using System;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Core
{
    public interface IUserService
    {
        UserSaveData UserData { get; }
        event Action OnDataChanged; // 데이터 변경 알림 이벤트
        
        /// <summary>
        /// 물고기를 인벤토리에 추가 (개별 데이터) 및 도감 등록
        /// </summary>
        void AddFish(string fishId, float length);

        /// <summary>
        /// 특정 고유 ID(instanceId)를 가진 물고기를 판매
        /// </summary>
        void SellFish(string instanceId);

        /// <summary>
        /// 인벤토리의 모든 물고기를 일괄 판매
        /// </summary>
        void SellAllFish();

        void Save();
        void Load();
    }
}
