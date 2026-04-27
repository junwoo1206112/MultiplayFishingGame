# 🤖 Gemini CLI 프로젝트 가이드라인

## 🚨 핵심 원칙 (Core Mandates)
1. **OpenSpec 기반 개발**: 모든 새로운 기능 추가나 대규모 수정 시 반드시 `.openspec` 폴더 아래에 `proposal.md`, `design.md`, `tasks.md`를 작성하여 계획을 수립한 뒤 실행한다.
2. **Skill 활용**: `.opencode/skills`에 정의된 전문 스킬들을 적극 활용하여 프로젝트 컨벤션(이름 규칙, Mirror 패턴 등)을 엄수한다.
3. **Plan-Act-Validate**: 모든 작업은 '계획-실행-검증'의 사이클을 거치며, 완료 후에는 반드시 `feature_documentation.md`나 관련 문서에 업데이트한다.

---
## 📝 현재 진행 중인 스펙
- `ui-status-and-inventory`: 플레이어 상태 및 인벤토리 UI 연동 (진행 중)
- `system-refinement-and-cleanup`: 시스템 로직 고도화 및 데이터 클리닝 (예정)
