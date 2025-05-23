using UnityEngine;

namespace com.rotovr.sdk
{
    /// <summary>
    /// Singleton pattern implementation.
    /// Can be used with classes extended from a MonoBehaviour.
    /// Once instance is found or created, game object will be marked as DontDestroyOnLoad.
    /// </summary>
    public abstract class MonoSingleton<T> 
#if !NO_UNITY
        : MonoBehaviour where T : MonoBehaviour
#else
     where T : class, new()
#endif
    {
        static T s_Instance;
        static bool s_ApplicationIsQuitting;
        static bool s_IsDestroyed;


#if !NO_UNITY
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.transform.SetParent(SingletonService.Parent);
        }
#endif

        /// <summary>
        /// Returns a singleton class instance
        /// If current instance is not assigned it will try to find an object of the instance type,
        /// in case instance already exists on a scene. If not, new instance will be created
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_ApplicationIsQuitting || s_IsDestroyed)
                {
                    Debug.LogError(
                        $"{typeof(T)} [MonoSingleton] is already destroyed. " +
                        $"Please check {nameof(HasInstance)} or {nameof(IsDestroyed)} before accessing instance in the destructor.");
                    return default(T);
                }

                if (s_Instance == null)
                {
#if !NO_UNITY
                    s_Instance = FindObjectOfType(typeof(T)) as T;
#endif
                    if (s_Instance == null)
                        Instantiate();
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Methods will create new object Instantiate
        /// Normally method is called automatically when you referring to and Instance getter
        /// for a first time.
        /// But it may be useful if you want manually control when the instance is created,
        /// even if you do not this specific instance at the moment
        /// </summary>
        static void Instantiate()
        {
            if (HasInstance)
            {
                Debug.LogWarning($"You are trying to Instantiate {typeof(T).FullName}, but it already has an Instance. Please use Instance property instead.");
                return;
            }
            
#if !NO_UNITY
            var name = typeof(T).FullName;
            s_Instance = new GameObject(name).AddComponent<T>();
#else
            s_Instance = new T();
#endif
        }

        /// <summary>
        /// Returns `true` if Singleton Instance exists.
        /// </summary>
        public static bool HasInstance => s_Instance != null;

        /// <summary>
        /// If this property returns `true` it means that object with explicitly destroyed.
        /// This could happen if Destroy function  was called for this object or if it was
        /// automatically destroyed during the `ApplicationQuit`.
        /// </summary>
        public static bool IsDestroyed => s_IsDestroyed;

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed,
        /// it will create a buggy ghost object that will stay on the Editor scene
        /// even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            s_Instance = null;
            s_IsDestroyed = true;
        }

        protected virtual void OnApplicationQuit()
        {
            s_Instance = null;
            s_IsDestroyed = true;
            s_ApplicationIsQuitting = true;
        }
    }
}