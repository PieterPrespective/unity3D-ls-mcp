#if !UNITY_EDITOR
// Using simulated types from TestMonoBehaviour.cs
#endif

using UnityEngine;

/// <summary>
/// Test ScriptableObject for testing Unity-specific patterns.
/// </summary>
public class TestScriptableObject : ScriptableObject
{
    [SerializeField]
    private string itemName = "Default";

    [SerializeField]
    private int itemValue = 100;

    /// <summary>
    /// Gets the item name.
    /// </summary>
    public string ItemName => itemName;

    /// <summary>
    /// Gets the item value.
    /// </summary>
    public int ItemValue => itemValue;
}
