using UnityEngine;

namespace _Project.Scripts.Utility
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        private static T _instance;

        private protected bool DontDestroy = true;
        
        private static bool _applicationIsQuitting;

        public static T GetInstance()
        {
            if (_applicationIsQuitting) { return null; }

            // If instance isn't null, return it.
            if (_instance != null) return _instance;
            
            // If we have no instance, search for one
            _instance = FindObjectOfType<T>();
            if (_instance != null) return _instance;
            
            // If we still have no instance, we create one and return it.
            GameObject obj = new GameObject();
            obj.name = typeof(T).Name;
            _instance = obj.AddComponent<T>();

            return _instance;
        }
        
        /* IMPORTANT!!! To use Awake in a derived class you need to do it this way
         * protected override void Awake()
         * {
         *      base.Awake();
         *      --Code--
         * }
        */
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (DontDestroy)
                {
                    transform.parent = null;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this as T)
            {
                Destroy(gameObject);
            }
            else if (DontDestroy)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
        }

        /* IMPORTANT!!! To use OnApplicationQuit in a derived class you need to do it this way
         * protected override void OnApplicationQuit()
         * {
         *      base.OnApplicationQuit();
         *      --Code--
         * }
         */
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}