# Multiplay Fishing Game

Unity 6와 Mirror Networking을 사용해 만든 멀티플레이 낚시 게임 프로젝트입니다.
로컬/호스트 기반 멀티플레이 흐름, 낚시 상태머신, 물 표면 판정, 캐스팅/입질/낚아올리기 연출을 하나의 플레이 루프로 구성했습니다.

## 포트폴리오 핵심

- Unity C# 기반 3D 낚시 게임
- Mirror Networking 기반 Host/Client 접속 흐름
- 최대 플레이어 수 제한 및 접속/퇴장 이벤트 처리
- 낚시 상태머신: `Idle -> Charging -> Casting -> Waiting -> Nibble -> Catching -> Success/Failure`
- 물 표면 Raycast, 찌/줄/스플래시 연출, 낚시 사운드 처리
- UI, 인벤토리, 상점, 도감, 데이터 변환 도구까지 확장된 구조

## 주요 구현

| 영역 | 구현 내용 |
|---|---|
| Network | `FishingNetworkManager`, Mirror Host/Client, 접속 상태 이벤트 |
| Gameplay | `FishingController`, 캐스팅 충전, 입질 반응, 연타 판정, 성공/실패 처리 |
| Visual | 낚싯줄, 로프, 물 스플래시, 카메라/애니메이션 이벤트 연동 |
| UI | 로비, 네트워크 메뉴, 낚시 UI, 인벤토리/상점/도감 |
| Data | 낚싯대/미끼/어종 데이터, Excel 데이터 변환 Editor Tool |
| Test | 낚시 컨트롤러 EditMode 테스트 포함 |

## 대표 코드

- `Assets/Scripts/Network/FishingNetworkManager.cs`
  Mirror `NetworkManager`를 확장해 접속 수 제한, Host/Client 상태 이벤트, 플레이어 입장/퇴장 처리를 담당합니다.

- `Assets/Scripts/Gameplay/FishingController.cs`
  낚시의 핵심 상태머신입니다. 캐스팅 거리 충전, 물 표면 탐지, 입질 타이밍, 연타 기반 낚아올리기, 결과 연출을 관리합니다.

- `Assets/Scripts/UI/`
  네트워크 메뉴, 낚시 HUD, 인벤토리, 상점, 도감 UI를 구성합니다.

- `Assets/Editor/ExcelDataConverter.cs`
  게임 데이터를 Unity 에디터에서 변환/관리하기 위한 도구입니다.

## 실행 방법

1. Unity Hub에서 Unity `6000.3.10f1` 이상으로 프로젝트를 엽니다.
2. `Assets/Scenes` 안의 시작/로비 씬을 엽니다.
3. 한 인스턴스는 Host로 실행합니다.
4. 다른 인스턴스는 Client로 실행해 Host IP와 포트로 접속합니다.
5. 접속 후 낚시 캐스팅, 입질, 낚아올리기 루프를 확인합니다.

## 검증 포인트

- Host 실행 후 Client 접속 흐름 확인
- 플레이어 접속 수 제한 동작 확인
- 캐스팅 중 물 표면 탐지와 찌 위치 보정 확인
- 입질 반응 시간 내 클릭 시 Catching 상태 전환 확인
- 연타 수 달성 시 성공 처리, 실패 시 실패 연출 확인

## 포트폴리오에서 강조할 점

이 프로젝트는 단순 미니게임보다 **네트워크 게임 구조와 실제 플레이 루프 구현 경험**을 보여주는 데 적합합니다.
게임 회사 지원 시에는 `Mirror Networking`, `상태머신`, `멀티플레이 동기화`, `게임 데이터/UI 확장 구조`를 중심으로 설명하는 것이 좋습니다.

## 기술 스택

- Unity 6
- C#
- Mirror Networking
- Unity Input System
- UGUI
- Editor Tooling
