# Project Progress Report (Multiplay Fishing Game)

## 📌 1. 현재 진행 상황 요약

### ✅ 완료된 작업 (Done)
1. **캐릭터 지면 박힘 문제 해결**
   - `Player.prefab`의 콜라이더 오프셋 정렬 (바닥을 0에 맞춤).
   - `Walking.fbx` 애니메이션 설정 수정 (Bake Into Pose Y 활성화, 발 기준 고정).
   - `SampleSceneLocalPlayerController.cs` 중력 로직 개선 (isGrounded 시 과도한 하강 압력 제거).
2. **맵 에셋 통합**
   - `Map` 브랜치의 최신 에셋들(IslandMap, 바다 쉐이더 등)을 `test` 브랜치로 머지 완료.
   - `Island` 씬의 주요 오브젝트들을 `PlayScene`으로 이관 완료.
3. **DI 기반 데이터 시스템 구축**
   - `IDataService`, `IUserService` 인터페이스 정의 및 `DIContainer` 구현.
   - `NotificationUI` 싱글톤 제거 및 DI 구조로 리팩토링 완료.
   - NPOI 기반 엑셀 컨버터 에디터 툴(`ExcelDataConverter.cs`) 구현 완료.
   - 유저 데이터(인벤토리, 도감) JSON 저장 시스템 구현 완료.

---

## ⚠️ 2. 새로운 PC에서 작업 시작 시 주의사항 (필수!)

현재 **NPOI 라이브러리**와 유니티 6 간의 충돌로 인해 컴파일 에러가 발생할 수 있습니다. 다음 순서대로 환경을 세팅하세요.

1. **NuGetForUnity 설치**
   - `https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity` 주소를 UPM에서 입력하거나 `.unitypackage`를 받아 설치.
2. **NPOI 패키지 설치**
   - `NuGet > Manage NuGet Packages`에서 **NPOI** 검색 후 설치.
3. **SkiaSharp 에러 해결 (중요)**
   - 설치 후 `SkiaSharp.DotNet.Interactive.dll` 관련 에러가 뜨면 해당 파일을 삭제해야 합니다.
   - 혹은 `Assets/Packages/NPOI...` 하위 폴더 중 `netstandard2.0`, `net6.0` 등 비호환 폴더를 정리해야 할 수 있습니다. (현재 `test` 브랜치에 최대한 정리해서 올렸으나 다시 생길 수 있음)
4. **Library 폴더 삭제 후 재시작**
   - 에러가 지속되면 유니티를 끄고 `Library` 폴더를 삭제한 뒤 다시 켜세요.

---

## 🚀 3. 다음에 이어서 해야 할 일 (Next Steps)

1. **마스터 데이터 입력**
   - `Tools/Excel/Create Blank Fish Data` 메뉴로 엑셀 양식 생성.
   - `FishData.xlsx`에 물고기 정보(ID, 이름, 확률, 가격 등) 입력.
   - `Tools/Excel/Convert Fish Data to SO`를 눌러 데이터 에셋 생성.
2. **낚시 로직 연동**
   - 플레이어가 물고기를 낚았을 때 `DIContainer.Resolve<IUserService>().AddFish(fishId)` 호출하도록 연결.
   - 판매 상점 UI를 만들어 `SellFish(fishId)` 기능 구현.
3. **네트워크 동기화**
   - Mirror를 통해 플레이어의 물고기 획득 정보를 다른 유저에게 브로드캐스팅.
   - 방 입장 시 자신의 유저 데이터(`UserData.json`) 정보를 서버에 전달.
4. **DI 원칙 준수**
   - 앞으로 추가할 `SoundManager`, `ItemManager` 등도 반드시 인터페이스를 만들고 `GameInitializer`에서 등록하여 사용할 것. (싱글톤 금지)

---
*최종 업데이트: 2026-04-22*
