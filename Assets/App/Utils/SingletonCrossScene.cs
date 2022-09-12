using UnityEngine;

namespace ZFramework
{
    /// <summary>
    /// A singleton template that lives for the duration of the application, if the object is not on the stage, it will create itself.
    /// </summary>
    public class SingletonCrossScene<T> : MonoBehaviour where T : MonoBehaviour
    {
        // TODO: R&D Singleton pattern using scriptable objects (to protect against domain reloading after scripts compiling)
        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Private fields (static)
        // ---------------------------------------------------------------------------------------------------------

        private static T _instance;
        private static bool _applicationIsQuitting = false;
        private static object _lock = new();

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods (static)
        // ---------------------------------------------------------------------------------------------------------

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

        #endregion

        #region Object lifecycle

        /// <summary>
        /// When the application closes, the objects are destroyed in a random order.
        /// To prevent the creation of a new object after destruction, setting this flag is used.
        /// </summary>
        public virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }

        #endregion
    }
}
