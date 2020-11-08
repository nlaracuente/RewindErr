using UnityEngine;

/// <summary>
/// A generic singleton base class
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// An instance of self
    /// </summary>
    static T _instance;

    /// <summary>
    /// True: prevents the object from being destroyed.
    /// <note>
    /// Singletons can be auto-instantiated therefore if you want to keep the persistent key
    /// then you must set it within the child class by overriding this flag.
    /// If you setup the class as a prefab then the option is available in the inspector
    /// </note> 
    /// </summary>
    [SerializeField, Tooltip("Enable this to prevent the object from being destroyed")]
    protected bool isPersistent = false;

    /// <summary>
    /// The current instance if one exists or creates a new one 
    /// </summary>
    public static T instance
    {
        get
        {
            _instance = _instance ?? FindObjectOfType<T>();

            // Create
            if (_instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name, typeof(T));
                _instance = go.GetComponent<T>();
            }

            return _instance;
        }
    }

    /// <summary>
    /// Sets this object as the current instance if one does not exist
    /// Destroys this object if it is not the current instance
    /// Prevents this object from being destroyed if the <see cref="isPersistent"/> is enabled
    /// </summary>
    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (isPersistent)
            {
                // Object cannot be a child or else Unity won't let us make it persistent
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

        }
        else if (_instance != this)
        {
            DestroyImmediate(gameObject);
        }
    }
}