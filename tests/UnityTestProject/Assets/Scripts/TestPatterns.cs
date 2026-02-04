#if !UNITY_EDITOR
// Using simulated types from TestMonoBehaviour.cs
#endif

using UnityEngine;

/// <summary>
/// Test file containing INTENTIONAL Unity anti-patterns for analyzer testing.
/// DO NOT use as reference code - these are examples of what NOT to do.
/// Each anti-pattern is documented with the expected diagnostic ID.
/// </summary>
public class TestPatterns : MonoBehaviour
{
    private Transform? cachedTransform;

    /// <summary>
    /// UNT0002: Inefficient tag comparison.
    /// Should use CompareTag() instead of == operator.
    /// </summary>
    void CheckTagBad()
    {
        // BAD: Direct string comparison (UNT0002)
        if (gameObject.tag == "Player") { }

        // GOOD: Using CompareTag
        // if (gameObject.CompareTag("Player")) { }
    }

    /// <summary>
    /// UNT0010: MonoBehaviour created with new.
    /// MonoBehaviours should be added with AddComponent.
    /// </summary>
    void CreateBehaviourBad()
    {
        // BAD: Using new to create MonoBehaviour (UNT0010)
        var mb = new TestPatterns();

        // GOOD: Using AddComponent
        // var mb = gameObject.AddComponent<TestPatterns>();
    }

    /// <summary>
    /// ULSM0001: Expensive call in hot path (Update).
    /// GetComponent should be cached in Awake/Start.
    /// </summary>
    void Update()
    {
        // BAD: GetComponent called every frame (ULSM0001)
        var rb = GetComponent<Transform>();

        // BAD: Camera.main called every frame (ULSM0004)
        var cam = Camera.main;

        // BAD: String interpolation in Update (ULSM0002)
        var message = $"Position: {transform}";

        // BAD: Debug.Log in Update (ULSM0003)
        Debug.Log(message);

        // GOOD: Use cached references
        // var rb = cachedTransform;
    }

    /// <summary>
    /// ULSM0002: String concatenation in hot path.
    /// String operations allocate memory.
    /// </summary>
    void FixedUpdate()
    {
        // BAD: String concatenation (ULSM0002)
        var text = "Value: " + Time.ToString();

        // GOOD: Use StringBuilder or avoid in hot paths
    }

    /// <summary>
    /// Test method containing Input API (for migration testing).
    /// Legacy Input system deprecated in favor of new Input System.
    /// </summary>
    void CheckInput()
    {
        // Legacy Input API (deprecated)
        // var horizontal = Input.GetAxis("Horizontal");
        // var fire = Input.GetButton("Fire1");
        // var keyDown = Input.GetKey(KeyCode.Space);
    }

    /// <summary>
    /// UNT0001: Empty Unity message.
    /// Empty Update/FixedUpdate/etc methods waste performance.
    /// </summary>
    void LateUpdate()
    {
        // BAD: Empty Unity message (UNT0001)
    }

    /// <summary>
    /// Example with Find methods (expensive operations).
    /// </summary>
    void FindObjectsBad()
    {
        // BAD: Find methods are expensive (ULSM0001 in hot paths)
        // var player = GameObject.Find("Player");
        // var enemies = GameObject.FindObjectsOfType<TestPatterns>();
    }

    // Placeholder for simulated Time class
    private static class Time
    {
        public static new string ToString() => "0.0";
    }
}
