# 01. 설치부터 동작 확인까지 (초보자용)

대상 환경: **Unity 6 (6000.3 계열)**, Windows 11, Git 설치, GitHub 계정.

## A. GitHub 저장소 만들기

1. GitHub에 로그인합니다.
2. 오른쪽 위 `+` 또는 GitHub의 새 저장소 생성 화면에서 `New repository`를 선택합니다.
3. **Repository name**에 `Unity-Package-Library`를 입력합니다.
4. **Public**을 선택합니다. (처음에는 공개 저장소로 검증하는 것이 간단합니다.)
5. `Create repository`를 누릅니다.
6. 이 압축 파일의 **Unity-Package-Library-Starter 폴더 내부 파일들**을 저장소 최상위에 업로드합니다. `README.md`가 저장소 루트에 와야 합니다.
7. 업로드 후 `main` 브랜치인지 확인합니다.
8. `https://github.com/WySong11/Unity-Package-Library` 페이지에서 `Packages`, `catalog.json`, `README.md`가 보이면 성공입니다.

> GitHub 사용자명이 `WySong11`이 아니면 `catalog.json`의 gitUrl, 패키지 Installer `.cs`의 `DefaultCatalogUrl`을 **GitHub에 올리기 전에** 자신의 계정/저장소 이름에 맞게 바꾸세요.

## B. 샘플 태그 만들기 (중요!)

카탈로그의 Hello World는 `#helloworld-v1.0.0` 태그로, Installer 자체는 `#installer-v1.0.0` 태그로 고정합니다.
태그를 만들지 않으면 설치가 실패합니다.

로컬 Git을 사용하는 경우 저장소 루트에서 다음을 실행합니다.

```powershell
git clone https://github.com/WySong11/Unity-Package-Library.git
cd Unity-Package-Library
git tag helloworld-v1.0.0
git tag installer-v1.0.0
git push origin helloworld-v1.0.0
git push origin installer-v1.0.0
```

GitHub 웹 UI에서 Release를 만들고 태그 이름을 `helloworld-v1.0.0`으로 지정해도 됩니다.
**중요:** 먼저 모든 파일을 업로드한 뒤 태그를 생성합니다.

## C. Unity에서 Installer 설치

1. Unity Hub에서 새 Unity 6 프로젝트(테스트용)를 생성합니다.
2. Windows 터미널에서 `git --version`을 실행해 Git 설치 여부를 확인합니다.
3. Unity 메뉴 `Window > Package Management > Package Manager`를 엽니다.
4. `+` 메뉴 → `Install package from git URL`을 선택합니다.
5. 다음 주소를 입력합니다.

```text
https://github.com/WySong11/Unity-Package-Library.git?path=/Packages/com.wysong.packageinstaller#installer-v1.0.0
```

6. 설치 완료 후 메뉴 `Tools > WySong > Package Installer`를 엽니다.

## D. Package Installer 테스트

1. Installer 창의 Catalog URL이 다음 주소인지 확인합니다.
   `https://raw.githubusercontent.com/WySong11/Unity-Package-Library/main/catalog.json`
2. `GitHub 목록 새로고침`을 누릅니다.
3. **Hello World (설치 테스트)** 항목이 표시되는지 확인합니다.
4. `설치` 버튼을 클릭합니다.
5. 설치 확인 창에 표시된 Git 주소를 확인하고 `설치`를 누릅니다.
6. 설치 완료 후 `Tools > WySong > Demo > Hello World` 메뉴를 클릭합니다.
7. Console에 `Hello! Unity Package Library 설치 성공.`가 뜨면 전체 흐름이 작동한 것입니다.

## E. Git 명령으로 처음 업로드하는 방법 (선택)

GitHub에서 빈 저장소를 만들고, 압축을 풀어 해당 폴더 안에서 터미널을 실행합니다.

```powershell
git init
git branch -M main
git add .
git commit -m "chore: initialize Unity Package Library"
git remote add origin https://github.com/WySong11/Unity-Package-Library.git
git push -u origin main
```

**주의:** 빈 저장소에 README를 자동 생성하지 않아야 가장 간단합니다. 위 명령은 아직 Git 저장소가 아닌 새 폴더에서만 사용합니다.

## F. 추가 패키지 등록

새 패키지 디렉터리 → package.json 작성 → Editor/Runtime 코드 추가 → Unity에서 .meta GUID 확인 →
커밋/푸시 → 고유 태그 생성 → catalog.json에 항목 추가 → Installer 새로고침.

세부 과정은 `02-PACKAGE-ADDING.md`를 참고하세요.
