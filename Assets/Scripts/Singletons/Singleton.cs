using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : Singleton<T>
{
    private static T instance;
    private bool isInitialized = false; // Track if THIS instance is initialized

    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        // Check if there's already a different instance
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // If this instance is already initialized, don't initialize again
        if (isInitialized)
            return;

        instance = (T)this;
        isInitialized = true;

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void OnDisable()
    {
        if (instance != this)
            return;
    }
}
