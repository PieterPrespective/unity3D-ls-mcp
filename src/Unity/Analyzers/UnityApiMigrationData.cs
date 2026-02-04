namespace ULSM.Unity.Analyzers;

/// <summary>
/// Database of Unity API deprecations and migrations.
/// Covers Unity 2022.x to Unity 6.x (6000.x) transitions.
/// </summary>
public static class UnityApiMigrationData
{
    /// <summary>
    /// Represents an API migration entry.
    /// </summary>
    /// <param name="OldApi">The deprecated API (fully qualified or short name).</param>
    /// <param name="NewApi">The recommended replacement API.</param>
    /// <param name="MinVersion">Unity version where the API was deprecated.</param>
    /// <param name="RemovedVersion">Unity version where removed, or "Not Removed".</param>
    /// <param name="Category">Migration category (Input, Networking, Rendering, etc.).</param>
    /// <param name="Notes">Additional migration guidance.</param>
    public record ApiMigration(
        string OldApi,
        string NewApi,
        string MinVersion,
        string RemovedVersion,
        string Category,
        string Notes
    );

    /// <summary>
    /// Get all known API deprecations/migrations.
    /// </summary>
    /// <returns>Read-only list of all migration entries.</returns>
    public static IReadOnlyList<ApiMigration> GetAllMigrations() => _migrations;

    /// <summary>
    /// Get migrations relevant for a specific Unity version.
    /// Returns all migrations deprecated at or before the target version.
    /// </summary>
    /// <param name="targetVersion">Unity version to check (e.g., "6000.0", "2022.3").</param>
    /// <returns>Migrations applicable to the specified version.</returns>
    public static IEnumerable<ApiMigration> GetMigrationsForVersion(string targetVersion)
    {
        var targetParsed = ParseUnityVersion(targetVersion);

        return _migrations.Where(m =>
        {
            var minParsed = ParseUnityVersion(m.MinVersion);
            return CompareVersions(targetParsed, minParsed) >= 0;
        });
    }

    /// <summary>
    /// Search migrations by old API pattern (case-insensitive).
    /// </summary>
    /// <param name="pattern">Partial API name to search for.</param>
    /// <returns>Migrations matching the pattern.</returns>
    public static IEnumerable<ApiMigration> SearchByOldApi(string pattern)
    {
        return _migrations.Where(m =>
            m.OldApi.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all unique migration categories.
    /// </summary>
    /// <returns>Distinct category names.</returns>
    public static IEnumerable<string> GetCategories()
    {
        return _migrations.Select(m => m.Category).Distinct().OrderBy(c => c);
    }

    /// <summary>
    /// Get migrations filtered by category.
    /// </summary>
    /// <param name="category">Category to filter by (case-insensitive).</param>
    /// <returns>Migrations in the specified category.</returns>
    public static IEnumerable<ApiMigration> GetByCategory(string category)
    {
        return _migrations.Where(m =>
            m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses a Unity version string into comparable components.
    /// Handles formats: "6000.0.0", "2022.3", "6000", "2022.3.1f1"
    /// </summary>
    private static (int major, int minor, int patch) ParseUnityVersion(string version)
    {
        var parts = version.Split('.', '-');
        int major = 0, minor = 0, patch = 0;

        if (parts.Length > 0) int.TryParse(parts[0], out major);
        if (parts.Length > 1) int.TryParse(parts[1], out minor);
        if (parts.Length > 2)
        {
            // Strip suffixes like 'f1', 'p1', 'a1', 'b1'
            var patchStr = new string(parts[2].TakeWhile(char.IsDigit).ToArray());
            int.TryParse(patchStr, out patch);
        }

        return (major, minor, patch);
    }

    /// <summary>
    /// Compares two version tuples.
    /// </summary>
    /// <returns>Negative if a &lt; b, zero if equal, positive if a &gt; b.</returns>
    private static int CompareVersions(
        (int major, int minor, int patch) a,
        (int major, int minor, int patch) b)
    {
        if (a.major != b.major) return a.major.CompareTo(b.major);
        if (a.minor != b.minor) return a.minor.CompareTo(b.minor);
        return a.patch.CompareTo(b.patch);
    }

    /// <summary>
    /// Database of Unity API migrations.
    /// Sources: Unity docs, upgrade guides, release notes.
    /// </summary>
    private static readonly List<ApiMigration> _migrations = new()
    {
        // ============ Input System ============
        new ApiMigration(
            "UnityEngine.Input.GetAxis",
            "UnityEngine.InputSystem.InputAction",
            "2019.4",
            "Not Removed",
            "Input",
            "Legacy Input Manager is deprecated. Use the new Input System package."
        ),
        new ApiMigration(
            "UnityEngine.Input.GetButton",
            "UnityEngine.InputSystem.InputAction",
            "2019.4",
            "Not Removed",
            "Input",
            "Legacy Input Manager is deprecated. Use the new Input System package."
        ),
        new ApiMigration(
            "UnityEngine.Input.GetKey",
            "UnityEngine.InputSystem.Keyboard.current",
            "2019.4",
            "Not Removed",
            "Input",
            "Legacy Input Manager is deprecated. Use the new Input System package."
        ),
        new ApiMigration(
            "UnityEngine.Input.GetMouseButton",
            "UnityEngine.InputSystem.Mouse.current",
            "2019.4",
            "Not Removed",
            "Input",
            "Legacy Input Manager is deprecated. Use the new Input System package."
        ),
        new ApiMigration(
            "UnityEngine.Input.mousePosition",
            "UnityEngine.InputSystem.Mouse.current.position",
            "2019.4",
            "Not Removed",
            "Input",
            "Legacy Input Manager is deprecated. Use the new Input System package."
        ),

        // ============ Physics ============
        new ApiMigration(
            "Physics.IgnoreCollision (layer overloads)",
            "Physics.IgnoreLayerCollision",
            "2022.3",
            "Not Removed",
            "Physics",
            "Use layer-based collision matrix settings instead"
        ),
        new ApiMigration(
            "Physics.autoSyncTransforms",
            "Physics.simulationMode",
            "2022.3",
            "Not Removed",
            "Physics",
            "Use Physics.simulationMode for controlling physics simulation"
        ),

        // ============ Rendering ============
        new ApiMigration(
            "Camera.SetTargetBuffers",
            "RenderTexture.SetRenderTarget",
            "2022.3",
            "Not Removed",
            "Rendering",
            "Use RenderTexture API or Scriptable Render Pipeline"
        ),
        new ApiMigration(
            "RenderTexture.GetTemporary (with int parameters)",
            "RenderTexture.GetTemporary (with RenderTextureDescriptor)",
            "2022.3",
            "Not Removed",
            "Rendering",
            "Use RenderTextureDescriptor overload for clarity"
        ),
        new ApiMigration(
            "Graphics.Blit (obsolete overloads)",
            "CommandBuffer.Blit",
            "2022.3",
            "Not Removed",
            "Rendering",
            "Use CommandBuffer API for better control and batching"
        ),
        new ApiMigration(
            "OnRenderImage",
            "ScriptableRenderPass",
            "2022.3",
            "Not Removed",
            "Rendering",
            "OnRenderImage not supported in URP/HDRP. Use custom render passes."
        ),

        // ============ UI ============
        new ApiMigration(
            "UnityEngine.UI (legacy)",
            "UnityEngine.UIElements",
            "2022.3",
            "Not Removed",
            "UI",
            "UI Toolkit (UIElements) is the recommended UI system for editor and runtime"
        ),

        // ============ Networking ============
        new ApiMigration(
            "UnityEngine.Networking.NetworkTransport",
            "Unity.Netcode.NetworkManager",
            "2022.3",
            "6000.0",
            "Networking",
            "UNet is removed in Unity 6. Use Netcode for GameObjects or third-party solution"
        ),
        new ApiMigration(
            "UnityEngine.Networking.NetworkManager",
            "Unity.Netcode.NetworkManager",
            "2022.3",
            "6000.0",
            "Networking",
            "UNet is removed in Unity 6. Use Netcode for GameObjects"
        ),
        new ApiMigration(
            "UnityEngine.Networking.NetworkBehaviour",
            "Unity.Netcode.NetworkBehaviour",
            "2022.3",
            "6000.0",
            "Networking",
            "UNet is removed in Unity 6. Use Netcode for GameObjects"
        ),

        // ============ WWW/Web ============
        new ApiMigration(
            "UnityEngine.WWW",
            "UnityEngine.Networking.UnityWebRequest",
            "2018.4",
            "2022.3",
            "Networking",
            "WWW class is removed. Use UnityWebRequest"
        ),

        // ============ ScriptableRenderPipeline ============
        new ApiMigration(
            "UnityEngine.Rendering.SRPBatcher (manual calls)",
            "Automatic batching",
            "2022.3",
            "Not Removed",
            "Rendering",
            "SRP Batcher is automatic in URP/HDRP; manual calls are deprecated"
        ),

        // ============ Unity 6 Specific ============
        new ApiMigration(
            "EditorUtility.DisplayDialog (some overloads)",
            "EditorUtility.DisplayDialog (updated signature)",
            "6000.0",
            "Not Removed",
            "Editor",
            "Some dialog overloads updated in Unity 6"
        ),
        new ApiMigration(
            "AssetDatabase.Refresh (synchronous)",
            "AssetDatabase.RefreshSettings",
            "6000.0",
            "Not Removed",
            "Editor",
            "Consider async refresh patterns in Unity 6"
        ),

        // ============ XR ============
        new ApiMigration(
            "UnityEngine.XR.WSA",
            "UnityEngine.XR.OpenXR",
            "2022.3",
            "6000.0",
            "XR",
            "Windows Mixed Reality API deprecated. Use OpenXR"
        ),
        new ApiMigration(
            "UnityEngine.VR.VRSettings",
            "UnityEngine.XR.XRSettings",
            "2019.4",
            "2022.3",
            "XR",
            "Legacy VR namespace removed. Use XR namespace"
        ),
        new ApiMigration(
            "UnityEngine.XR.InputTracking",
            "UnityEngine.XR.InputDevices",
            "2019.4",
            "Not Removed",
            "XR",
            "InputTracking deprecated. Use InputDevices API"
        ),

        // ============ Coroutines/Async ============
        new ApiMigration(
            "StartCoroutine (string method name)",
            "StartCoroutine (IEnumerator)",
            "2019.4",
            "Not Removed",
            "Coroutines",
            "String-based coroutine start is slower due to reflection. Use IEnumerator overload"
        ),

        // ============ Audio ============
        new ApiMigration(
            "AudioSource.PlayScheduled",
            "AudioSource.PlayScheduled (with DSP time)",
            "2022.3",
            "Not Removed",
            "Audio",
            "Ensure using DSP time for accurate audio scheduling"
        ),

        // ============ Animation ============
        new ApiMigration(
            "Animation component (legacy)",
            "Animator component",
            "2017.4",
            "Not Removed",
            "Animation",
            "Legacy Animation component is deprecated. Use Animator with AnimatorController"
        ),

        // ============ Particle System ============
        new ApiMigration(
            "ParticleSystem.Emit (obsolete overloads)",
            "ParticleSystem.Emit (EmitParams)",
            "2022.3",
            "Not Removed",
            "Particles",
            "Use EmitParams struct for more control over emitted particles"
        ),

        // ============ Scene Management ============
        new ApiMigration(
            "Application.LoadLevel",
            "SceneManager.LoadScene",
            "2017.4",
            "2019.4",
            "SceneManagement",
            "Application.LoadLevel removed. Use SceneManager.LoadScene"
        ),
        new ApiMigration(
            "Application.LoadLevelAsync",
            "SceneManager.LoadSceneAsync",
            "2017.4",
            "2019.4",
            "SceneManagement",
            "Application.LoadLevelAsync removed. Use SceneManager.LoadSceneAsync"
        ),

        // ============ PlayerPrefs ============
        new ApiMigration(
            "PlayerPrefs (for complex data)",
            "JsonUtility + File IO",
            "2019.4",
            "Not Removed",
            "Storage",
            "PlayerPrefs is not suitable for complex data. Use serialization to files."
        ),

        // ============ GUI ============
        new ApiMigration(
            "GUI.* and GUILayout.*",
            "UnityEngine.UIElements",
            "2022.3",
            "Not Removed",
            "UI",
            "IMGUI is legacy. Use UI Toolkit for editor UI and runtime UI."
        ),
    };
}
