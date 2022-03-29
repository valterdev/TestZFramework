using UnityEngine;

namespace ZFramework
{
    /// <summary>
    /// Шаблон синглтона, который живет в течении работы приложения, если объекта нет на сцене - создаст себя.
    /// </summary>
    public class SingletonCrossScene<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting = false;
        private static object _lock = new object();

        public static T Instance()
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[Singleton] Something went wrong, there is more than 1 singleton!");
#endif
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                        DontDestroyOnLoad(singleton);
#if UNITY_EDITOR
                        Debug.Log("[Singleton] instance of " + typeof(T) + " : " + singleton + "' was created.");
#endif
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// При закрытии приложения объекты уничтожаются в случайной последовательности.
        /// Для защиты от создания нового объекта после уничтожения используется установка этого флага.
        /// </summary>
        public void OnDestroy()
        {
            _applicationIsQuitting = true;
        }

    }
}