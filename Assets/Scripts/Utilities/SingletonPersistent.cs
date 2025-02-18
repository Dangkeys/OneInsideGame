using UnityEngine;

public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isInitialized = false;
    
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                
                if (instance == null)
                {
                    Debug.LogWarning($"[Singleton] An instance of {typeof(T)} is needed in the scene, but there is none.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (!isInitialized)
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnAwakeInitialization();
                isInitialized = true;
                Debug.Log($"[Singleton] {typeof(T)} initialized in Awake");
            }
        }
        else
        {
            // If we already have an initialized instance, destroy this one
            if (this != instance)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already exists, destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }

    // New method for initialization logic
    protected virtual void OnAwakeInitialization()
    {
        // Override this in derived classes to add initialization logic
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            isInitialized = false;
            Debug.Log($"[Singleton] {typeof(T)} instance destroyed");
        }
    }

#if UNITY_EDITOR
    // This will help clean up the singleton when stopping play mode in the editor
    protected virtual void OnApplicationQuit()
    {
        instance = null;
        isInitialized = false;
    }
#endif
}