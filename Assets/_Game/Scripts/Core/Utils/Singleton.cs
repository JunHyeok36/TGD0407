using UnityEngine; 

namespace TDG0407.Core.Utils
{
    
    /// <summary>
    /// MonoBehaviour을 상속받는 싱글톤 클래스를 구현합니다.
    /// </summary>
    /// <typeparam name="T">MonoBehaviour를 상속받는 클래스이어야 합니다.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;
        public static T Instance => instance;

        protected virtual void Awake()
        {
            instance ??= FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }
    }

}