using UnityEditor;

namespace WySong.HelloWorld.Editor
{
    /// <summary>
    /// 샘플 패키지가 실제로 설치되었는지 메뉴로 확인합니다.
    /// </summary>
    public static class HelloWorldMenu
    {
        [MenuItem("Tools/WySong/Demo/Hello World")]
        private static void Run()
        {
            WySong.HelloWorld.HelloWorld.Print();
        }
    }
}
