# WySong Package Installer

Unity 6용 UI Toolkit 기반 Editor 창입니다. UPM 및 기존 `.unitypackage`를 모두 지원합니다.

- 메뉴: `Tools > WySong > Package Installer`
- `catalog.json`을 HTTPS로 가져와 목록 표시
- 검색, 현재 설치 버전 확인, Git URL 선택 설치
- 설치 전 Git URL 확인 팝업
- UnityEditor.PackageManager.Client.Add 호출
- `.unitypackage` 항목은 다운로드 후 `AssetDatabase.ImportPackage(path, true)`로 가져옵니다.
- `.unitypackage`는 버전 설치 여부를 자동으로 판별하지 않습니다.

기본 카탈로그: `https://raw.githubusercontent.com/WySong11/Unity-Package-Library/main/catalog.json`

테스트는 Unity 6000.3 환경에서 수행하세요. 이 배포 파일은 Unity Editor 실제 실행 검증을 수행하지 않은 소스 예제입니다.
