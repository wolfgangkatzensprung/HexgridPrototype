using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get;
        private set;
    }

    protected void Awake()
    {
        if (Instance != this)
            Instance = this as T;

    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationQuit()
    {
        if (Instance == this)
            Instance = null;
    }
}