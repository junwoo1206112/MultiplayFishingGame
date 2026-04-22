## Why

외부 NPOI 라이브러리 저장소의 불안정성(404 에러)으로 인해 프로젝트 빌드 및 환경 설정에 차질이 발생함. 이를 해결하기 위해 외부 DLL 의존성이 없는 표준 CSV 기반 데이터 시스템으로 전환하여 안정성을 확보함.

## What Changes

- **Dependency Removal**: NPOI DLL 의존성 제거. `System.IO` 및 표준 문자열 파싱을 사용하는 커스텀 CSV 핸들러 도입.
- **Excel Compatibility**: 사용자는 엑셀에서 데이터를 편집하고 `.csv` (쉼표로 구분) 형식으로 저장하여 사용.
- **Automated Tooling**: 에디터 메뉴를 통해 CSV 템플릿 생성 및 ScriptableObject 변환 기능 제공.
- **DI & Persistence**: 기존에 설계된 DI 아키텍처 및 유저 데이터 저장 로직은 동일하게 유지.

## Capabilities

### New Capabilities
- `csv-data-converter`: 외부 라이브러리 없이 작동하는 데이터 변환 툴.
- `di-service-framework`: (기존과 동일)
- `user-data-persistence`: (기존과 동일)

## Impact

- `Assets/Plugins/NPOI`: 삭제 (더 이상 필요 없음).
- `Assets/Editor/ExcelDataConverter.cs`: CSV 대응 로직으로 전면 수정.
- `Assets/ExcelData/`: `.xlsx` 대신 `.csv` 파일 관리.
