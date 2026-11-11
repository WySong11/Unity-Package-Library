# 04. 운영, 안전, 라이선스

## 운영 권장 사항

1. 운영 저장소와 개발 중인 패키지를 구분하고, 검증된 버전만 catalog.json에 공개합니다.
2. 설치 가능한 패키지는 반드시 Git 태그 또는 전체 커밋 해시로 고정합니다. 예제는 태그를 사용합니다.
3. 모든 패키지 버전의 `package.json`과 카탈로그 `version`을 일치시킵니다.
4. 공용 프로젝트에서는 `Packages/manifest.json` 및 `Packages/packages-lock.json`을 Git에 포함하여 재현성을 높입니다.
5. Assembly Definition File과 고정 GUID의 `.meta`를 관리합니다.
6. Runtime/Editor를 분리하여 Editor 코드가 플레이어 빌드에 포함되지 않게 합니다.
7. 다른 사람이 만든 Asset Store 패키지의 라이선스를 검토하고, 허가받지 않은 에셋을 공개 저장소에 올리지 않습니다.

## Installer 보안 정책

- 다운로드할 Catalog URL은 HTTPS만 허용합니다.
- Installer의 원클릭 설치 대상은 `https://github.com/<owner>/<repo>.git?path=/Packages/com.xxx#tag` 형식만 허용합니다.
- 설치 전 확인 창에 전체 Git URL 또는 `.unitypackage` 다운로드 주소를 표시합니다.
- `.unitypackage` 다운로드는 파일로 저장한 후 **대화형 Import 선택 창**을 열며, UPM처럼 설치 상태를 자동 추적하지 않습니다.
- 원격 코드는 Editor 안에서 실행될 수 있으므로, 신뢰한 GitHub 저장소와 변경 이력을 검토한 후 설치합니다.
- 공개 GitHub raw 카탈로그만 기본 지원합니다. 개인 액세스 토큰을 EditorPrefs나 catalog.json에 넣지 마세요.
- 태그는 변경 불가능한 릴리스 지점으로 취급합니다. 엄격한 공급망 재현성이 필요하면 전체 Git commit SHA를 사용하세요.
- `Client.Add`를 통한 설치는 기존 설치 패키지나 의존성 버전을 변경할 수 있으므로, 작업 전에 Git 커밋으로 안전 지점을 만들어두세요.

## 수업 환경 배포 추천

- 학원 강의용 패키지는 교육 과제와 의존성을 명시한 README.md를 작성합니다.
- GitHub 릴리스마다 Unity 테스트 버전/지원 플랫폼/변경사항(CHANGELOG.md)을 남깁니다.
- `Unity-Codex-TemplateMD`는 Codex 작업 규칙과 CLI 검증을, 이 저장소는 실사용 코드 패키지를 관리합니다.
