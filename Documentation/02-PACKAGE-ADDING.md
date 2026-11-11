# 02. 새 UPM 패키지 추가하기

예제: `com.wysong.androidlog` 패키지를 만드는 경우

## 1) 폴더 생성

```text
Packages/
  com.wysong.androidlog/
    package.json
    Runtime/
      AndroidLogManager.cs
    Editor/
      AndroidLogEditorWindow.cs
    README.md
```

`.asmdef`로 Runtime과 Editor 어셈블리를 나누는 것을 권장합니다.
Editor asmdef는 `includePlatforms: ["Editor"]`로 구성하고 Runtime 어셈블리를 참조하세요.

## 2) package.json 작성

```json
{
  "name": "com.wysong.androidlog",
  "version": "1.0.0",
  "displayName": "Android Log System",
  "description": "Reusable Android logging utilities",
  "unity": "6000.0"
}
```

`name`은 소문자·숫자·마침표 중심으로 고유하게 만듭니다.
코드나 폴더를 Unity 프로젝트에서 만든 뒤 `.meta`를 포함해 Git에 커밋하면 GUID를 안정적으로 유지할 수 있습니다.

## 3) Git에 업로드하고 태그 만들기

```powershell
git add .
git commit -m "feat: add Android Log package 1.0.0"
git push origin main
git tag androidlog-v1.0.0
git push origin androidlog-v1.0.0
```

> 멀티 패키지(모노레포)에서는 태그가 저장소 **전체의 특정 커밋**을 가리킵니다. 예전 태그를 이동하거나 삭제하지 마세요.

## 4) catalog.json에 패키지 추가

기존 `packages` 배열에 다음 객체를 **추가**합니다.

```json
{
  "id": "com.wysong.androidlog",
  "displayName": "Android Log System",
  "description": "Android Logcat 및 Unity 로그 수집",
  "category": "Debugging",
  "version": "1.0.0",
  "gitUrl": "https://github.com/WySong11/Unity-Package-Library.git?path=/Packages/com.wysong.androidlog#androidlog-v1.0.0"
}
```

**주의:** `package.json`과 `catalog.json`의 버전을 일치시키세요.

## 5) 패키지 수정/업데이트

`package.json`을 1.1.0으로 변경하고 코드를 수정한 뒤:

```powershell
git add .
git commit -m "feat: update Android Log to 1.1.0"
git push origin main
git tag androidlog-v1.1.0
git push origin androidlog-v1.1.0
```

`catalog.json`의 `version`을 `1.1.0`, gitUrl의 태그를 `#androidlog-v1.1.0`으로 변경하여 커밋합니다.
설치 사용자들은 목록을 새로고침한 뒤 `선택 버전 설치`를 누르면 됩니다.

**제약:** 설치 여부 판정은 패키지 ID와 버전 기준이며, 같은 버전의 다른 Git 원본까지 구분하지는 않습니다.

## 6) 기존 `.unitypackage`를 Installer 목록에 추가하기

1. Unity Editor에서 `Assets > Export Package...`로 원하는 에셋을 내보냅니다.
2. GitHub 저장소의 **Releases**에서 새 릴리스를 만들고 `.unitypackage` 파일을 업로드합니다.
3. 릴리스 태그(예: `assetui-v1.0.0`)를 지정합니다.
4. `catalog.json`의 `packages` 배열에 다음 항목을 추가합니다.

```json
{
  "id": "com.wysong.assetui",
  "displayName": "UI 예제 에셋",
  "description": "수업에서 사용하는 UI 프리팹 예제",
  "category": "Assets",
  "version": "1.0.0",
  "type": "unitypackage",
  "downloadUrl": "https://github.com/WySong11/Unity-Package-Library/releases/download/assetui-v1.0.0/AssetUI.unitypackage"
}
```

5. GitHub에 카탈로그를 커밋한 뒤 Installer에서 `GitHub 목록 새로고침`을 누릅니다.
6. `다운로드 후 Import` 버튼을 누르면 파일을 프로젝트의 `Library/WySongPackageInstaller` 아래로 다운로드하고 Unity의 Import 선택 창을 엽니다.

`AssetUI.unitypackage`는 **실제로 릴리스에 업로드한 파일 이름**과 일치해야 합니다. 이 저장소 샘플에는 실제 `.unitypackage` 파일이 포함되어 있지 않습니다.

`.unitypackage`는 프로젝트의 `Assets`에 파일을 복사하는 방식이라 UPM과 달리 설치한 패키지의 버전을 자동 추적하지 않으며, 기존 파일을 덮어쓸 수 있습니다.
