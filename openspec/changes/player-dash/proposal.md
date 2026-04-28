## Why

플레이어가 Space 키를 눌러 짧은 거리를 빠르게 대시할 수 있는 기능이 필요합니다. 현재 플레이어는 걷기만 가능하여 이동이 답답하고, 긴 거리를 이동할 때 지루함이 있습니다. 대시를 통해 이동에 변화를 주고 긴급 상황(낚시 실패 후 재시도 등)에서 빠르게 대처할 수 있게 합니다.

## What Changes

- `FishingPlayer`에 대시 기능 추가 (Space 키 입력 → 짧은 거리 빠른 이동)
- `SampleSceneLocalPlayerController`에도 동일한 대시 기능 추가 (싱글 플레이어 씬)
- 대시 중에는 낚시 불가 (FishingController와 연동)
- 대시 쿨타임 시스템 (남용 방지)
- 대시 애니메이션 파라미터 연동 (선택 사항)

## Capabilities

### New Capabilities
- `player-dash`: 플레이어가 Space 키를 눌러 쿨타임 기반으로 짧은 거리를 빠르게 이동할 수 있는 대시 기능

### Modified Capabilities
<!-- None - existing specs are not affected -->

## Impact

- `FishingPlayer.cs` — 대시 입력 처리 및 서버 동기화 로직 추가
- `FishingController.cs` — 대시 중 낚시 입력 차단 로직 추가
- `SampleSceneLocalPlayerController.cs` — 싱글 플레이어 씬 대시 처리 추가
- `PlayerAnimController` (있을 경우) — 대시 애니메이션 파라미터 추가
