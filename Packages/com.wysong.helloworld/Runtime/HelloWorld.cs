using UnityEngine;

namespace WySong.HelloWorld
{
    /// <summary>
    /// 설치 후 어디에서든 호출 가능한 간단한 런타임 기능입니다.
    /// </summary>
    public static class HelloWorld
    {
        public const string Message = "Hello! Unity Package Library 설치 성공.";

        public static void Print()
        {
            Debug.Log(Message);
        }
    }
}
