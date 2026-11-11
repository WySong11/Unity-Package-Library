# Unity-Package-Library

Unity 6용 **GitHub 기반 UPM·.unitypackage 라이브러리 + UI Toolkit Package Installer** 스타터입니다.

## 구성

```text
Unity-Package-Library/
├── README.md
├── catalog.json
├── Packages/
│   ├── com.wysong.packageinstaller/
│   │   ├── package.json
│   │   └── Editor/                  # UI Toolkit 패키지 목록/설치 창
│   └── com.wysong.helloworld/
│       ├── package.json
│       ├── Runtime/
│       └── Editor/                  # 설치 성공 확인 메뉴
├── UnityPackages/                # 별도 .unitypackage 보관
└── Documentation/
    ├── 01-START.md
    ├── 02-PACKAGE-ADDING.md
    ├── 03-TROUBLESHOOTING.md
    └── 04-PUBLISHING-AND-SECURITY.md
```

## 시작하기

1. [01-START.md](Documentation/01-START.md)에 따라 `Unity-Package-Library` 공개 GitHub 저장소를 생성하고 **이 폴더의 내용물**을 루트에 업로드합니다.
2. Git 태그 `helloworld-v1.0.0`과 `installer-v1.0.0`을 만듭니다.
3. Unity > `Window > Package Management > Package Manager` > `+` > `Install package from git URL`에 입력합니다.

```text
https://github.com/WySong11/Unity-Package-Library.git?path=/Packages/com.wysong.packageinstaller#installer-v1.0.0
```

4. `Tools > WySong > Package Installer`에서 GitHub 목록을 새로고침하고 Hello World를 설치합니다.
5. `Tools > WySong > Demo > Hello World`로 설치 결과를 확인합니다.

> **중요:** 위 Git URL과 `catalog.json`은 **해당 GitHub 저장소를 만들고 파일과 태그를 업로드해야만 작동하는 예시**입니다. 이 압축 파일 자체는 GitHub에 게시되어 있지 않습니다.

## 패키지 등록 규칙

- 새 기능: `Packages/com.wysong.<기능>/` 아래 신규 UPM 패키지로 만듭니다. 기존 `.unitypackage`는 GitHub Releases에 올린 뒤 카탈로그에 등록합니다.
- Unity `package.json`의 `name`, `version`과 카탈로그 `id`, `version`을 일치시킵니다.
- Git 태그는 패키지별 `helloworld-v1.0.0` 식으로 고유하게 만듭니다.
- `.cs`는 UTF-8 BOM, 그 외 Markdown/JSON/.asmdef/.meta는 UTF-8 BOM 없음, 줄바꿈 LF 정책을 예제에 적용했습니다.
- `.meta`의 GUID는 추적해야 하며 버전마다 변경하지 않습니다.

## 공식 근거 (Unity 6000.3 계열)

- Git URL 패키지 하위 경로와 태그: https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/upm-git
- Unity Package Manager `Client.Add`: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/packagemanager/client/add
- Package Manager 작업은 순차 실행: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/packagemanager/client
- `package.json` 매니페스트: https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/cus-pkg-development/upm-manifest-pkg

## 검증 범위

이 패키지는 카탈로그 파싱/파일 구성/문법 패턴에 대한 로컬 정적 점검을 거칩니다.
실제 Unity Editor나 외부 GitHub 설치를 실행하지 않았으므로, 게시 후 Unity 6000.3에서 컴파일 및 설치 검증이 필요합니다.
