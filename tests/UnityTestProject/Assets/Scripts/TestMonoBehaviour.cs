// Simulated Unity types for testing (when real Unity DLLs are not available)
#if !UNITY_EDITOR
namespace UnityEngine
{
    public class Object { }
    public class Component : Object { }
    public class Behaviour : Component { }
    public class MonoBehaviour : Behaviour
    {
        public GameObject gameObject { get; set; } = null!;
        public Transform transform { get; set; } = null!;
        public T GetComponent<T>() where T : Component => default!;
    }
    public class ScriptableObject : Object { }
    public class GameObject : Object
    {
        public string tag { get; set; } = "";
        public bool CompareTag(string tag) => false;
    }
    public class Transform : Component { }
    public class Camera : Behaviour
    {
        public static Camera main { get; } = null!;
    }
    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }
    public class SerializeFieldAttribute : System.Attribute { }
}
#endif

using UnityEngine;

/// <summary>
/// Valid MonoBehaviour for testing symbol resolution and references.
/// This class demonstrates proper Unity patterns.
/// </summary>
public class TestMonoBehaviour : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private string playerName = "Player";

    private Transform? cachedTransform;
    private Camera? mainCamera;

    /// <summary>
    /// Gets or sets the player name.
    /// </summary>
    public string PlayerName
    {
        get => playerName;
        set => playerName = value;
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Demonstrates proper caching pattern.
    /// </summary>
    private void Awake()
    {
        // Good pattern: cache references in Awake
        cachedTransform = transform;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Called on the frame when a script is enabled.
    /// </summary>
    private void Start()
    {
        Debug.Log($"TestMonoBehaviour started with speed {speed}");
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    private void Update()
    {
        // Uses cached reference - good pattern
        if (cachedTransform != null)
        {
            // Movement logic would go here
        }
    }

    /// <summary>
    /// Example public method for testing find references.
    /// </summary>
    /// <param name="value">A test value.</param>
    public void DoSomething(int value)
    {
        Debug.Log($"DoSomething called with {value}");
    }

    /// <summary>
    /// Example method with multiple parameters.
    /// </summary>
    /// <param name="name">The name parameter.</param>
    /// <param name="count">The count parameter.</param>
    /// <returns>A formatted string.</returns>
    public string FormatMessage(string name, int count)
    {
        return $"{name}: {count}";
    }
}
