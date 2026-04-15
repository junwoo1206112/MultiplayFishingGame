## ADDED Requirements

### Requirement: Room Creation
방장은 공개 또는 비공개 방을 생성할 수 있어야 한다. 방에는 이름, 최대 인원, 공개/비공개 설정, 선택적 비밀번호가 포함된다.

#### Scenario: Create public room
- **WHEN** 방장이 공개 방 생성을 요청함 (방 이름, 최대 인원 지정)
- **THEN** 서버에 새 방이 생성되고, 로비 방 목록에 표시됨

#### Scenario: Create private room with password
- **WHEN** 방장이 비밀번호가 있는 비공개 방 생성을 요청함
- **THEN** 방이 생성되지만 로비 방 목록에는 잠금 아이콘과 함께 표시됨

#### Scenario: Room name validation
- **WHEN** 플레이어가 빈 이름이나 20자 초과 이름으로 방 생성을 요청함
- **THEN** 서버가 요청을 거부하고 클라이언트에 에러 메시지를 전송함

### Requirement: Room Joining
플레이어는 방 목록에서 방을 선택하여 참여할 수 있어야 한다.

#### Scenario: Join public room
- **WHEN** 플레이어가 공개 방을 선택하여 참여함
- **THEN** 플레이어가 대기실에 입장함

#### Scenario: Join private room with correct password
- **WHEN** 플레이어가 비밀번호가 있는 방에 올바른 비밀번호를 입력함
- **THEN** 플레이어가 대기실에 입장함

#### Scenario: Join private room with wrong password
- **WHEN** 플레이어가 비밀번호가 있는 방에 잘못된 비밀번호를 입력함
- **THEN** 서버가 참여를 거부하고 클라이언트에 "비밀번호가 틀렸습니다" 메시지를 표시함

#### Scenario: Join full room
- **WHEN** 플레이어가 최대 인원이 찬 방에 참여를 요청함
- **THEN** 서버가 참여를 거부하고 "방이 가득 찼습니다" 메시지를 표시함

### Requirement: Room Leaving
플레이어는 대기실에서 방을 나갈 수 있어야 한다.

#### Scenario: Player leaves room
- **WHEN** 플레이어가 대기실에서 "나가기" 버튼을 누름
- **THEN** 플레이어가 로비(방 목록)로 돌아감

#### Scenario: Room host leaves
- **WHEN** 방장이 대기실에서 나감
- **THEN** 방장 권한이 다른 플레이어에게 이양됨. 남은 플레이어가 없으면 방이 삭제됨

### Requirement: Room List Display
로비에 있는 플레이어는 현재 채널의 방 목록을 볼 수 있어야 한다.

#### Scenario: Display available rooms
- **WHEN** 플레이어가 로비에 입장함
- **THEN** 현재 채널의 방 목록(방 이름, 현재 인원, 최대 인원, 공개/비공개, 진행 상태)이 표시됨

#### Scenario: Room list real-time update
- **WHEN** 새 방이 생성되거나 기존 방의 상태가 변경됨
- **THEN** 로비에 있는 모든 플레이어의 방 목록이 실시간으로 업데이트됨

### Requirement: Room Configuration
방장은 방 설정을 변경할 수 있어야 한다.

#### Scenario: Change room settings
- **WHEN** 방장이 대기실에서 방 설정(최대 인원, 공개/비공개 전환)을 변경함
- **THEN** 서버가 변경을 검증하고 방 설정이 업데이트됨