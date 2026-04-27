using UnityEngine;
using MultiplayFishing.Core;

namespace MultiplayFishing.Managers
{
    /// <summary>
    /// 게임 시작 시 DIContainer에 모든 필수 서비스를 등록합니다.
    /// </summary>
    public static class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Debug.Log("<color=cyan><b>[GameInitializer]</b> Initializing core services...</color>");

            // 1. DataService 등록 (Excel/Master Data)
            var dataService = new ExcelDataService();
            dataService.LoadData();
            DIContainer.Register<IDataService>(dataService);

            // 2. UserService 등록 (Save/Storage)
            var userService = new UserStorageService(dataService);
            // userService.Load(); // UserStorageService 생성자에서 이미 호출함
            DIContainer.Register<IUserService>(userService);

            Debug.Log("<color=cyan><b>[GameInitializer]</b> All services registered successfully.</color>");
        }
    }
}
