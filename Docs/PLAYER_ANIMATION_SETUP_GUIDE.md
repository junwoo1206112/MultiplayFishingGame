# Player Animation Setup Guide

작성일: 2026-05-11

이 문서는 플레이어 캐릭터에 낚시 관련 애니메이션을 수동으로 연결하는 방법을 정리한 가이드입니다.

## 핵심 개념

플레이어 프리팹과 Animator Controller는 서로 다른 파일입니다.

```text
플레이어 프리팹
Assets/Prefabs/Player/Player.prefab
```

```text
Animator Controller
Assets/Zimni/Fantasy character/animations/Fantasy_character_AnimatorController.controller
```

`Player.prefab`은 실제 게임에 생성되는 플레이어 오브젝트입니다.  
`Fantasy_character_AnimatorController.controller`는 플레이어가 어떤 조건에서 어떤 애니메이션을 재생할지 정하는 애니메이션 규칙표입니다.

즉, 순서는 아래와 같습니다.

```text
Player.prefab 안의 Animator 컴포넌트
-> Fantasy_character_AnimatorController.controller 사용
-> Controller 안에서 Idle, Walk, Fishing_Cast 같은 상태 관리
```

## 1. Animator Controller 열기

Unity Project 창에서 아래 파일을 엽니다.

```text
Assets/Zimni/Fantasy character/animations/Fantasy_character_AnimatorController.controller
```

이 파일을 더블클릭하면 Animator 창이 열립니다.

주의: 아래 파일은 플레이어 프리팹입니다. 파라미터와 상태를 추가하는 곳이 아니라, Animator Controller가 연결되어 있는지 확인하는 곳입니다.

```text
Assets/Prefabs/Player/Player.prefab
```

## 2. Parameters 추가

Animator 창 왼쪽의 `Parameters` 탭을 엽니다.

기존에 있던 아래 두 개는 그대로 둡니다.

```text
Bool Walk
Bool fishing
```

지우거나 이름을 바꾸면 안 됩니다. 현재 코드가 이미 이 이름을 사용합니다.

새로 추가할 파라미터는 아래와 같습니다.

```text
Bool    HasFish
Bool    RodEquipped
Trigger RodTakeOut
Trigger RodPutAway
Trigger FishingCast
```

최종 Parameters 목록은 대략 이렇게 됩니다.

```text
Walk
fishing
HasFish
RodEquipped
RodTakeOut
RodPutAway
FishingCast
```

대소문자를 정확히 맞춰야 합니다.

```text
fishing      -> 소문자 f
FishingCast  -> F와 C 대문자
```

`Bool`은 켜짐/꺼짐 값입니다.

```text
HasFish = true   -> 물고기를 들고 있음
HasFish = false  -> 물고기를 안 들고 있음
```

`Trigger`는 버튼처럼 한 번 발동되는 값입니다.

```text
FishingCast 발동 -> 캐스팅 애니메이션 한 번 재생
RodTakeOut 발동  -> 낚싯대 꺼내기 한 번 재생
RodPutAway 발동  -> 낚싯대 넣기 한 번 재생
```

## 3. 애니메이션 클립을 상태로 추가

Project 창에서 아래 애니메이션 클립들을 Animator 창의 빈 공간으로 드래그합니다.

```text
Assets/Zimni/Fantasy character/animations/fishing Idle.anim
Assets/Zimni/Fantasy character/animations/Fishing_Cast.anim
Assets/Zimni/Fantasy character/animations/rod-in.anim
Assets/rod-out.anim
```

Animator 창에 네모 박스가 생기면 성공입니다.

추천 상태 이름:

```text
fishing Idle
Fishing_Cast
rod-in
rod-out
```

주의: Parameters 탭에 추가하는 것이 아니라, `Idle`, `wallk`, `Fishing Start` 같은 네모 박스들이 보이는 Animator 그래프 화면의 빈 공간에 드래그해야 합니다.

## 4. 캐스팅 전환 연결

목표 구조:

```text
Any State -> Fishing_Cast -> fishing Idle -> Idle
```

### Any State -> Fishing_Cast

1. Animator 창에서 초록색 `Any State` 박스를 찾습니다.
2. `Any State`를 우클릭합니다.
3. `Make Transition`을 누릅니다.
4. `Fishing_Cast` 박스를 클릭합니다.
5. `Any State`에서 `Fishing_Cast`로 화살표가 생깁니다.
6. 그 화살표를 클릭합니다.
7. Inspector에서 `Has Exit Time` 체크를 끕니다.
8. `Transition Duration`을 `0.05` 정도로 둡니다.
9. Inspector 아래쪽 `Conditions`에서 `+`를 누릅니다.
10. 조건을 `FishingCast`로 선택합니다.

의미:

```text
코드에서 FishingCast 트리거 발동
-> Fishing_Cast 애니메이션 재생
```

### Fishing_Cast -> fishing Idle

1. `Fishing_Cast` 박스를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `fishing Idle` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. Inspector에서 `Has Exit Time`을 켭니다.
6. `Exit Time`을 `0.9` 정도로 둡니다.
7. `Transition Duration`은 `0.05` 정도로 둡니다.
8. `Conditions`는 비워둡니다.

의미:

```text
Fishing_Cast 애니메이션이 거의 끝남
-> fishing Idle로 이동
```

### fishing Idle -> Idle

1. `fishing Idle` 박스를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `Idle` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`은 끕니다.
6. `Transition Duration`은 `0.05` 정도로 둡니다.
7. `Conditions`에서 `+`를 누릅니다.
8. 조건을 `fishing`으로 선택합니다.
9. 조건값을 `false`로 설정합니다.

조건이 아래처럼 보이면 맞습니다.

```text
fishing false
```

의미:

```text
낚시 상태가 끝남
-> Idle로 돌아감
```

## 5. 낚싯대 꺼내기 / 넣기 연결

목표 구조:

```text
Any State -> rod-in -> Idle
Any State -> rod-out -> Idle
```

### Any State -> rod-in

1. `Any State`를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `rod-in` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`을 끕니다.
6. `Transition Duration`은 `0.05` 정도로 둡니다.
7. `Conditions`에서 `+`를 누릅니다.
8. 조건을 `RodTakeOut`으로 선택합니다.

### rod-in -> Idle

1. `rod-in` 박스를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `Idle` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`을 켭니다.
6. `Exit Time`은 `0.95` 정도로 둡니다.
7. `Transition Duration`은 `0.05` 정도로 둡니다.
8. `Conditions`는 비워둡니다.

### Any State -> rod-out

1. `Any State`를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `rod-out` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`을 끕니다.
6. `Transition Duration`은 `0.05` 정도로 둡니다.
7. `Conditions`에서 `+`를 누릅니다.
8. 조건을 `RodPutAway`로 선택합니다.

### rod-out -> Idle

1. `rod-out` 박스를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `Idle` 박스를 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`을 켭니다.
6. `Exit Time`은 `0.95` 정도로 둡니다.
7. `Transition Duration`은 `0.05` 정도로 둡니다.
8. `Conditions`는 비워둡니다.

## 6. Animation Event 넣기

캐스팅 애니메이션이 재생될 때 실제 낚싯줄이 날아가도록 함수 호출 지점을 넣어야 합니다.

가장 중요한 이벤트:

```text
Fishing_Cast.anim -> OnCastRelease
```

### Fishing_Cast.anim 이벤트

1. Project 창에서 아래 클립을 선택합니다.

```text
Assets/Zimni/Fantasy character/animations/Fishing_Cast.anim
```

2. 상단 메뉴에서 `Window > Animation > Animation`을 엽니다.
3. `Fishing_Cast.anim` 클립이 선택된 상태인지 확인합니다.
4. 타임라인 중간보다 살짝 뒤, 대략 `60~70%` 지점을 클릭합니다.
5. Animation 창의 `Add Event` 버튼을 누릅니다.
6. 이벤트가 생기면 Inspector에서 함수 이름을 선택합니다.
7. 함수 이름을 아래로 설정합니다.

```text
OnCastRelease
```

이 이벤트가 있어야 캐스팅 포즈 중간에 실제 낚싯바늘이 날아갑니다.

### rod-in.anim 이벤트

`rod-in.anim` 첫 부분에 이벤트를 추가할 수 있습니다.

```text
ShowRodEvent
```

### rod-out.anim 이벤트

`rod-out.anim` 끝부분에 이벤트를 추가할 수 있습니다.

```text
HideRodEvent
```

참고: `ShowRodEvent`와 `HideRodEvent`는 `FishingRodVisibility` 컴포넌트가 플레이어의 Animator와 같은 GameObject에 붙어 있을 때 의미가 있습니다. 우선 필수는 `Fishing_Cast.anim`의 `OnCastRelease`입니다.

## 7. 물고기 들기 애니메이션 연결

이미 컨트롤러에 `LIfting`, `Carrying` 상태가 있습니다. 새로 추가한 `HasFish`를 사용하려면 아래처럼 연결할 수 있습니다.

목표 구조:

```text
Any State -> LIfting -> Carrying
Carrying -> Idle
```

### Any State -> LIfting

1. `Any State`를 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `LIfting`을 클릭합니다.
4. 생긴 화살표를 클릭합니다.
5. `Has Exit Time`을 끕니다.
6. `Conditions`에 `HasFish true`를 추가합니다.

### LIfting -> Carrying

이미 있을 가능성이 큽니다. 없으면 아래처럼 만듭니다.

1. `LIfting`을 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `Carrying`을 클릭합니다.
4. `Has Exit Time`을 켭니다.
5. `Exit Time`을 `0.9` 정도로 둡니다.
6. 조건은 비워둡니다.

### Carrying -> Idle

1. `Carrying`을 우클릭합니다.
2. `Make Transition`을 누릅니다.
3. `Idle`을 클릭합니다.
4. `Has Exit Time`을 끕니다.
5. `Conditions`에 `HasFish false`를 추가합니다.

이 부분은 물고기를 잡았을 때 들어 올리고 들고 있기 용도입니다. 현재 낚시 성공 로직과 완전히 자연스럽게 연결하려면 추가 코드 연결이 필요할 수 있습니다.

## 8. 플레이어 프리팹에서 연결 확인

Project 창에서 아래 프리팹을 엽니다.

```text
Assets/Prefabs/Player/Player.prefab
```

프리팹 안에서 캐릭터 모델 오브젝트를 찾습니다. 이름은 보통 아래 중 하나일 수 있습니다.

```text
Player
model
Fantasy_character
```

Inspector에서 `Animator` 컴포넌트를 찾습니다.

`Controller` 칸에 아래 컨트롤러가 들어가 있어야 합니다.

```text
Fantasy_character_AnimatorController
```

정확한 파일:

```text
Assets/Zimni/Fantasy character/animations/Fantasy_character_AnimatorController.controller
```

만약 `Controller` 칸이 비어 있거나 다른 컨트롤러라면, Project 창에서 `Fantasy_character_AnimatorController.controller`를 끌어다가 `Controller` 칸에 넣습니다.

`Avatar` 칸에는 캐릭터 FBX Avatar가 들어가 있어야 합니다. 보통 아래 모델의 Avatar입니다.

```text
Assets/Zimni/Fantasy character/mesh/Fantasy_character.fbx
```

## 9. Play Mode 확인

Play 모드에서 아래를 확인합니다.

```text
WASD 이동 -> Walk 애니메이션
낚시 시작 -> fishing 관련 상태
마우스 뗌 / 캐스팅 -> Fishing_Cast
캐스팅 후 -> fishing Idle
낚시 끝 -> Idle
```

가장 중요한 체크포인트:

```text
Any State -> Fishing_Cast 조건이 FishingCast인지
Fishing_Cast.anim에 OnCastRelease 이벤트가 들어갔는지
기존 Walk, fishing 파라미터를 지우지 않았는지
```

