# 03. 오류 및 점검표

| 증상 | 원인 | 확인 방법 |
| --- | --- | --- |
| Git executable not found | Git 미설치 또는 PATH 누락 | `git --version` / Unity 재실행 |
| Repository not found | 저장소 주소 오류 또는 비공개 권한 문제 | 브라우저에서 GitHub 주소 확인 |
| Couldn't read Username | 비공개 Git 저장소 인증 문제 | 처음에는 Public 사용, 비공개는 Git 인증 별도 설정 |
| catalog.json 다운로드 실패 | raw URL 오류, 브랜치명 또는 네트워크 문제 | URL을 브라우저에서 직접 열기 |
| 목록은 보이는데 설치 실패 | 태그가 없음, 하위 path 오류, package.json 누락 | GitHub `Tags`, `Packages/.../package.json` 확인 |
| URL 확인 필요 | 설치 관리자가 허용하는 Git URL 규칙과 불일치 | `/Packages/<ID>#<태그>` 형식 점검 |
| 컴파일 오류 | asmdef 참조 누락, Unity 버전/의존성 불일치 | Console의 첫 번째 에러부터 수정 |
| 목록은 업데이트됐지만 코드가 그대로 | Git 태그를 재사용했거나 잠금 파일이 이전 커밋 가리킴 | 새 버전·새 태그 발행 후 설치 |
| .unitypackage 다운로드 후 Import가 안 뜸 | Release 파일 URL 오류 또는 패키지 파일 손상 | 브라우저에서 릴리스 파일 다운로드 확인 |
| Hello World 메뉴가 안 보임 | 설치 또는 컴파일 실패 | Package Manager/My Assets 아닌 In Project와 Console 확인 |

## 필수 주의 사항

- UPM Git 설치에는 Git 클라이언트가 필요합니다.
- UPM Git URL은 **`?path=`가 `#태그`보다 먼저** 나와야 합니다.
- `Client.Add`는 순차 처리해야 하므로 이 예제에서는 중복 요청을 막았습니다.
- 설치 중 창이 닫히거나 도메인 재로드가 발생하면 표시 상태가 사라질 수 있으나 Package Manager 동작은 계속될 수 있습니다. 다시 창을 열어 `설치 상태 확인`을 누르세요.
- 카탈로그는 공개 `raw.githubusercontent.com` 주소에서 가져옵니다. 비공개 저장소는 별도 인증/카탈로그 호스팅 설계가 필요합니다.
- `.unitypackage`는 UPM Git URL로 설치할 수 없지만 이 Installer는 다운로드 후 Unity Import 선택 창을 열도록 지원합니다.
- 코드는 Unity 6000.3 실제 Editor에서 테스트해야 합니다. 로컬 정적 검증만 수행한 스타터입니다.
