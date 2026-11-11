using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace WySong.PackageInstaller
{
    // JsonUtility로 읽을 수 있도록 JSON 최상위는 객체여야 합니다.
    [Serializable]
    internal sealed class PackageCatalog
    {
        public int schemaVersion;
        public PackageCatalogEntry[] packages;
    }

    [Serializable]
    internal sealed class PackageCatalogEntry
    {
        public string id;
        public string displayName;
        public string description;
        public string category;
        public string version;
        public string type;  // "upm" 또는 "unitypackage"
        public string gitUrl;
        public string downloadUrl;
    }

    /// <summary>
    /// GitHub의 catalog.json을 읽어 패키지 목록을 보여주고
    /// Git UPM 패키지는 Client.Add, .unitypackage는 ImportPackage로 처리합니다.
    /// </summary>
    public sealed class WySongPackageInstallerWindow : EditorWindow
    {
        private const string CatalogEditorPrefsKey = "WySong.PackageInstaller.CatalogUrl";
        private const string DefaultCatalogUrl =
            "https://raw.githubusercontent.com/WySong11/Unity-Package-Library/main/catalog.json";

        private TextField _catalogUrlField;
        private TextField _searchField;
        private Label _statusLabel;
        private VisualElement _packageContainer;
        private Button _refreshCatalogButton;
        private Button _refreshInstalledButton;

        private UnityWebRequest _catalogRequest;
        private UnityWebRequestAsyncOperation _catalogOperation;
        private ListRequest _listRequest;
        private AddRequest _addRequest;
        private UnityWebRequest _assetDownloadRequest;
        private UnityWebRequestAsyncOperation _assetDownloadOperation;
        private string _assetDownloadPath;
        private PackageCatalog _catalog;
        private readonly Dictionary<string, string> _installed =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private bool _installedListReady;
        private string _workingPackageId;
        private string _statusText = "먼저 GitHub 패키지 목록을 불러옵니다.";

        [MenuItem("Tools/WySong/Package Installer")]
        public static void Open()
        {
            var window = GetWindow<WySongPackageInstallerWindow>();
            window.titleContent = new GUIContent("Package Installer");
            window.minSize = new Vector2(560, 420);
            window.Show();
        }

        private void OnEnable()
        {
            // Unity Editor의 업데이트 루프에서 비동기 요청 완료 여부를 확인합니다.
            EditorApplication.update += PollRequests;
        }

        private void OnDisable()
        {
            EditorApplication.update -= PollRequests;
            if (_catalogRequest != null)
            {
                _catalogRequest.Abort();
                _catalogRequest.Dispose();
                _catalogRequest = null;
                _catalogOperation = null;
            }
            if (_assetDownloadRequest != null)
            {
                _assetDownloadRequest.Abort();
                _assetDownloadRequest.Dispose();
                _assetDownloadRequest = null;
                _assetDownloadOperation = null;
            }
            // UPM 요청은 Editor가 처리합니다. 창이 닫혀도 설치 자체는 진행될 수 있습니다.
        }

        // Unity 6의 Editor UI Toolkit 진입점입니다.
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            var title = new Label("WySong Package Library");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            root.Add(title);

            var guide = new Label("GitHub catalog.json 목록에서 원하는 UPM 패키지를 설치합니다.");
            guide.style.marginBottom = 10;
            root.Add(guide);

            _catalogUrlField = new TextField("Catalog URL");
            _catalogUrlField.value = EditorPrefs.GetString(CatalogEditorPrefsKey, DefaultCatalogUrl);
            _catalogUrlField.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetString(CatalogEditorPrefsKey, evt.newValue);
            });
            root.Add(_catalogUrlField);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 8;
            buttons.style.marginBottom = 8;
            root.Add(buttons);

            _refreshCatalogButton = new Button(LoadCatalog) { text = "GitHub 목록 새로고침" };
            _refreshCatalogButton.style.marginRight = 8;
            buttons.Add(_refreshCatalogButton);

            _refreshInstalledButton = new Button(LoadInstalled) { text = "설치 상태 확인" };
            buttons.Add(_refreshInstalledButton);

            _searchField = new TextField("패키지 검색");
            _searchField.RegisterValueChangedCallback(_ => RenderPackages());
            _searchField.style.marginBottom = 6;
            root.Add(_searchField);

            _statusLabel = new Label(_statusText);
            _statusLabel.style.whiteSpace = WhiteSpace.Normal;
            _statusLabel.style.marginBottom = 8;
            root.Add(_statusLabel);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            root.Add(scroll);
            _packageContainer = new VisualElement();
            scroll.Add(_packageContainer);

            RenderPackages();
            LoadInstalled();
            LoadCatalog();
        }

        private void LoadCatalog()
        {
            if (_catalogRequest != null)
                return;

            string url = _catalogUrlField == null ? DefaultCatalogUrl : _catalogUrlField.value.Trim();
            Uri parsed;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsed) ||
                parsed.Scheme != Uri.UriSchemeHttps)
            {
                ShowStatus("Catalog URL에는 HTTPS 주소를 입력하세요.", true);
                return;
            }

            try
            {
                _catalogRequest = UnityWebRequest.Get(url);
                _catalogRequest.timeout = 20;
                _catalogOperation = _catalogRequest.SendWebRequest();
                ShowStatus("GitHub 패키지 목록을 가져오는 중...");
                UpdateButtons();
            }
            catch (Exception ex)
            {
                if (_catalogRequest != null)
                    _catalogRequest.Dispose();
                _catalogRequest = null;
                _catalogOperation = null;
                ShowStatus("목록 요청 실패: " + ex.Message, true);
            }
        }

        private void LoadInstalled()
        {
            // Package Manager Client API는 한 번에 하나의 작업만 요청합니다.
            if (_listRequest != null || _addRequest != null || _assetDownloadRequest != null)
                return;

            try
            {
                _installedListReady = false;
                // 로컬 캐시로 설치 상태만 확인합니다. 직접 의존성만 조회합니다.
                _listRequest = Client.List(true, false);
                UpdateButtons();
            }
            catch (Exception ex)
            {
                ShowStatus("설치 상태 조회 실패: " + ex.Message, true);
            }
        }

        private void PollRequests()
        {
            if (_catalogRequest != null && _catalogOperation != null && _catalogOperation.isDone)
            {
                try
                {
                    if (_catalogRequest.result != UnityWebRequest.Result.Success)
                    {
                        ShowStatus("목록 다운로드 실패: " + _catalogRequest.error, true);
                    }
                    else
                    {
                        var parsed = JsonUtility.FromJson<PackageCatalog>(
                            _catalogRequest.downloadHandler.text);
                        if (parsed == null || parsed.schemaVersion != 1 || parsed.packages == null)
                            throw new FormatException("catalog.json의 schemaVersion 또는 packages가 잘못되었습니다.");

                        _catalog = parsed;
                        ShowStatus("패키지 " + _catalog.packages.Length + "개를 읽었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus("catalog.json 읽기 오류: " + ex.Message, true);
                }
                finally
                {
                    _catalogRequest.Dispose();
                    _catalogRequest = null;
                    _catalogOperation = null;
                    RenderPackages();
                }
            }

            if (_assetDownloadRequest != null && _assetDownloadOperation != null &&
                _assetDownloadOperation.isDone)
            {
                bool downloaded = _assetDownloadRequest.result == UnityWebRequest.Result.Success;
                string error = _assetDownloadRequest.error;
                _assetDownloadRequest.Dispose();
                _assetDownloadRequest = null;
                _assetDownloadOperation = null;

                if (!downloaded)
                {
                    ShowStatus(".unitypackage 다운로드 실패: " + error, true);
                }
                else
                {
                    try
                    {
                        // true: Import 창에서 파일 선택 및 변경 사항을 최종 확인합니다.
                        AssetDatabase.ImportPackage(_assetDownloadPath, true);
                        ShowStatus("Import 선택 창을 열었습니다. 실제 가져오기는 선택 후 완료됩니다.");
                    }
                    catch (Exception ex)
                    {
                        ShowStatus(".unitypackage Import 요청 실패: " + ex.Message, true);
                    }
                }
                _assetDownloadPath = null;
                RenderPackages();
            }

            if (_listRequest != null && _listRequest.IsCompleted)
            {
                if (_listRequest.Status == StatusCode.Success)
                {
                    _installed.Clear();
                    foreach (var package in _listRequest.Result)
                        _installed[package.name] = package.version;
                    _installedListReady = true;
                }
                else
                {
                    _installedListReady = false;
                    ShowStatus("설치 상태 조회 실패: " +
                        (_listRequest.Error != null ? _listRequest.Error.message : "알 수 없는 오류"), true);
                }

                _listRequest = null;
                RenderPackages();
            }

            if (_addRequest != null && _addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    ShowStatus("설치 완료: " + _workingPackageId);
                }
                else
                {
                    ShowStatus("설치 실패: " +
                        (_addRequest.Error != null ? _addRequest.Error.message : "알 수 없는 오류"), true);
                }
                _workingPackageId = null;
                _addRequest = null;
                LoadInstalled();
                RenderPackages();
            }
        }

        private void RenderPackages()
        {
            if (_packageContainer == null)
                return;

            _packageContainer.Clear();
            UpdateButtons();
            if (_catalog == null || _catalog.packages == null)
            {
                _packageContainer.Add(new Label("목록 새로고침을 눌러 catalog.json을 가져오세요."));
                return;
            }

            string keyword = _searchField == null ? "" : _searchField.value.Trim();
            int visible = 0;

            foreach (var entry in _catalog.packages)
            {
                if (entry == null)
                    continue;
                if (!Contains(entry.displayName, keyword) && !Contains(entry.id, keyword) &&
                    !Contains(entry.description, keyword) && !Contains(entry.category, keyword))
                    continue;

                visible++;
                var box = new VisualElement();
                box.style.borderBottomWidth = 1;
                box.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                box.style.paddingTop = 9;
                box.style.paddingBottom = 10;
                _packageContainer.Add(box);

                bool isAssetPackage = string.Equals(entry.type, "unitypackage", StringComparison.OrdinalIgnoreCase);
                var name = new Label((entry.displayName ?? entry.id ?? "이름 없음") +
                                     "  ·  v" + (entry.version ?? "?") +
                                     (isAssetPackage ? "  [.unitypackage]" : "  [UPM]"));
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.fontSize = 13;
                box.Add(name);
                box.Add(new Label(entry.id ?? "ID 없음"));

                if (!string.IsNullOrWhiteSpace(entry.description))
                {
                    var description = new Label(entry.description);
                    description.style.whiteSpace = WhiteSpace.Normal;
                    description.style.marginTop = 3;
                    box.Add(description);
                }

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginTop = 5;
                box.Add(row);

                string installedVersion = null;
                bool installed = !isAssetPackage && _installed.TryGetValue(entry.id ?? "", out installedVersion);
                bool sameVersion = installed && string.Equals(
                    installedVersion, entry.version, StringComparison.Ordinal);

                var state = new Label(isAssetPackage ? "Assets로 가져오기 · 설치 여부 자동 확인 불가" :
                    !_installedListReady ? "설치 상태 확인 중" :
                    installed ? "현재 설치: v" + installedVersion : "미설치");
                state.style.flexGrow = 1;
                row.Add(state);

                bool valid = isAssetPackage ? IsSupportedAssetPackage(entry) : IsSupportedGitPackage(entry);
                string buttonText = isAssetPackage ? "다운로드 후 Import" :
                    sameVersion ? "설치됨" : installed ? "선택 버전 설치" : "설치";
                var capturedEntry = entry;
                var installButton = new Button(() =>
                {
                    if (isAssetPackage)
                        ImportAssetPackage(capturedEntry);
                    else
                        InstallPackage(capturedEntry);
                }) { text = valid ? buttonText : "URL 확인 필요" };
                installButton.SetEnabled(valid && (isAssetPackage || _installedListReady) &&
                    !sameVersion && _listRequest == null && _addRequest == null &&
                    _assetDownloadRequest == null);
                row.Add(installButton);

                if (!valid)
                {
                    var warning = new Label(isAssetPackage
                        ? "허용된 형식: GitHub Releases 또는 raw.githubusercontent.com의 .unitypackage HTTPS"
                        : "허용된 형식: GitHub HTTPS + /Packages/<패키지ID> + #태그");
                    warning.style.color = new Color(0.95f, 0.45f, 0.35f);
                    box.Add(warning);
                }
            }
            if (visible == 0)
                _packageContainer.Add(new Label("검색 결과가 없습니다."));
        }

        private void InstallPackage(PackageCatalogEntry entry)
        {
            if (_listRequest != null || _addRequest != null || _assetDownloadRequest != null ||
                !_installedListReady || !IsSupportedGitPackage(entry))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Git 패키지 설치 확인",
                "패키지: " + entry.displayName + "\n" +
                "버전: " + entry.version + "\n\n" +
                "Git 주소:\n" + entry.gitUrl + "\n\n" +
                "신뢰할 수 있는 소스인지 확인하고 설치하세요.",
                "설치", "취소");
            if (!confirmed)
                return;

            try
            {
                _workingPackageId = entry.id;
                _addRequest = Client.Add(entry.gitUrl);
                ShowStatus("설치 중: " + entry.id);
                RenderPackages();
            }
            catch (Exception ex)
            {
                _workingPackageId = null;
                _addRequest = null;
                ShowStatus("설치 요청 실패: " + ex.Message, true);
            }
        }

        private void ImportAssetPackage(PackageCatalogEntry entry)
        {
            if (_listRequest != null || _addRequest != null || _assetDownloadRequest != null ||
                !IsSupportedAssetPackage(entry))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                ".unitypackage 다운로드 및 Import 확인",
                "에셋: " + entry.displayName + "\n" +
                "버전: " + entry.version + "\n\n" +
                "다운로드 주소:\n" + entry.downloadUrl + "\n\n" +
                "다운로드 후 Unity의 Import 창에서 파일을 선택합니다.\n" +
                "프로젝트 Assets의 기존 파일과 충돌할 수 있으므로 백업을 권장합니다.",
                "다운로드", "취소");
            if (!confirmed)
                return;

            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string targetDir = Path.Combine(projectRoot, "Library", "WySongPackageInstaller");
                Directory.CreateDirectory(targetDir);
                string filename = entry.id + "-" + entry.version + ".unitypackage";
                _assetDownloadPath = Path.Combine(targetDir, filename);

                _assetDownloadRequest = new UnityWebRequest(entry.downloadUrl, UnityWebRequest.kHttpVerbGET);
                var handler = new DownloadHandlerFile(_assetDownloadPath);
                handler.removeFileOnAbort = true;
                _assetDownloadRequest.downloadHandler = handler;
                _assetDownloadRequest.timeout = 120;
                _assetDownloadOperation = _assetDownloadRequest.SendWebRequest();
                ShowStatus(".unitypackage 다운로드 중: " + entry.displayName);
                RenderPackages();
            }
            catch (Exception ex)
            {
                if (_assetDownloadRequest != null)
                    _assetDownloadRequest.Dispose();
                _assetDownloadRequest = null;
                _assetDownloadOperation = null;
                _assetDownloadPath = null;
                ShowStatus("Import 다운로드 요청 실패: " + ex.Message, true);
            }
        }

        private static bool IsSupportedAssetPackage(PackageCatalogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id) ||
                string.IsNullOrEmpty(entry.version) || string.IsNullOrEmpty(entry.downloadUrl))
                return false;
            if (!Regex.IsMatch(entry.id, @"^com\.[a-z0-9]+(?:[.-][a-z0-9]+)+$"))
                return false;
            if (!Regex.IsMatch(entry.version, @"^[a-zA-Z0-9_.-]+$"))
                return false;

            Uri url;
            if (!Uri.TryCreate(entry.downloadUrl, UriKind.Absolute, out url))
                return false;
            bool release = string.Equals(url.Host, "github.com", StringComparison.OrdinalIgnoreCase) &&
                           url.AbsolutePath.Contains("/releases/download/");
            bool raw = string.Equals(url.Host, "raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase);
            return url.Scheme == Uri.UriSchemeHttps && (release || raw) &&
                   url.AbsolutePath.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase) &&
                   string.IsNullOrEmpty(url.Query) && string.IsNullOrEmpty(url.Fragment);
        }

        private static bool IsSupportedGitPackage(PackageCatalogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id) ||
                string.IsNullOrEmpty(entry.version) || string.IsNullOrEmpty(entry.gitUrl))
                return false;

            if (!Regex.IsMatch(entry.id, @"^com\.[a-z0-9]+(?:[.-][a-z0-9]+)+$"))
                return false;

            Uri url;
            if (!Uri.TryCreate(entry.gitUrl, UriKind.Absolute, out url))
                return false;

            if (url.Scheme != Uri.UriSchemeHttps ||
                !string.Equals(url.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
                !url.AbsolutePath.EndsWith(".git", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(url.Query, "?path=/Packages/" + entry.id, StringComparison.Ordinal))
                return false;

            string revision = url.Fragment.TrimStart('#');
            return revision.Length > 0 && Regex.IsMatch(revision, @"^[a-zA-Z0-9][a-zA-Z0-9_.-]*$");
        }

        private static bool Contains(string text, string keyword)
        {
            return string.IsNullOrEmpty(keyword) ||
                   (!string.IsNullOrEmpty(text) &&
                    text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ShowStatus(string message, bool error = false)
        {
            _statusText = message;
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
                if (error)
                    _statusLabel.style.color = new Color(0.95f, 0.45f, 0.35f);
                else
                    _statusLabel.style.color = StyleKeyword.Null;
            }
        }

        private void UpdateButtons()
        {
            if (_refreshCatalogButton != null)
                _refreshCatalogButton.SetEnabled(_catalogRequest == null);
            if (_refreshInstalledButton != null)
                _refreshInstalledButton.SetEnabled(_listRequest == null && _addRequest == null &&
                                                    _assetDownloadRequest == null);
        }
    }
}
