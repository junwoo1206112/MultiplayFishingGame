using UnityEngine;
using MultiplayFishing.Core;
using MultiplayFishing.UI; // 이제 참조 가능
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private bool autoLoadOnStart = true;

        private void Awake()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            // 1. DataService 등록 및 데이터 로드
            var dataService = new ExcelDataService();
            dataService.LoadData();
            DIContainer.Register<IDataService>(dataService);

            // 2. UserService 등록 및 세이브 로드
            var userService = new UserStorageService(dataService);
            if (autoLoadOnStart)
            {
                userService.Load();
            }
            DIContainer.Register<IUserService>(userService);

            // 3. NotificationUI 등록 (이제 UI 네임스페이스를 직접 참조)
            var notificationUI = Object.FindFirstObjectByType<NotificationUI>();
            if (notificationUI != null)
            {
                DIContainer.Register<INotificationService>(notificationUI);
            }

            Debug.Log("[GameInitializer] Core services initialized and registered in DIContainer.");
        }
    }
}
