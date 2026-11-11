# 기존 .unitypackage 보관 영역

`Assets > Export Package...`로 내보낸 `.unitypackage` 파일을 여기에 저장할 수 있습니다.
이 파일은 **UPM Git 패키지와 다른 형식**으로 `Client.Add`가 아닌 `AssetDatabase.ImportPackage`가 필요합니다.
파일을 직접 다운로드해 `Assets > Import Package > Custom Package...`에서 가져오거나,
아래 예시처럼 `catalog.json`에 `type: "unitypackage"`로 등록하면 전용 Installer에서 다운로드 후 Import 창을 열 수 있습니다.
실제로는 GitHub Releases에 파일을 게시한 다음 그 URL을 지정하는 것을 권장합니다.
대용량(100 MiB 이상) 파일은 일반 Git 업로드가 차단될 수 있으므로 GitHub Releases 또는 Git LFS 사용을 검토하세요.
재배포 권한이 없는 상용 Asset Store 파일은 올리지 마세요.
