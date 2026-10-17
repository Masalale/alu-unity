using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 1 builder: adds the floating cube platform layout to the existing
/// Level01 scene. The player must survive untouched, so the scene is loaded
/// and extended, never recreated.
/// </summary>
public static class Task1Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string ParentName = "Platforms";

    /// <summary>
    /// Platform layout, verbatim: name, local position, local scale. The parent
    /// sits at the origin, so local coordinates are world coordinates. Gaps stay
    /// between 1.0 and 1.5 units and every rise is exactly 1.0 unit, which stays
    /// inside a CharacterController jump with gravity -20, jumpHeight 2, speed 5.
    /// </summary>
    private static readonly (string Name, Vector3 Position, Vector3 Scale)[] Platforms =
    {
        ("Platform01", new Vector3(0f, -0.5f, 0f), new Vector3(5f, 1f, 5f)),
        ("Platform02", new Vector3(5f, -0.5f, 0f), new Vector3(3f, 1f, 3f)),
        ("Platform03", new Vector3(9f, 0.5f, 0f), new Vector3(3f, 1f, 3f)),
        ("Platform04", new Vector3(13f, 0.5f, 2f), new Vector3(3f, 1f, 3f)),
        ("Platform05", new Vector3(17f, 1.5f, 0f), new Vector3(2f, 1f, 2f)),
        ("Platform06", new Vector3(21f, 1.5f, -2f), new Vector3(3f, 1f, 3f)),
        ("Platform07", new Vector3(25f, 2.5f, 0f), new Vector3(3f, 1f, 3f)),
        ("Platform08", new Vector3(30f, 2.5f, 0f), new Vector3(5f, 1f, 5f)),
        ("Platform09", new Vector3(13f, 0.5f, 6f), new Vector3(2f, 1f, 2f)),
        ("Platform10", new Vector3(25f, 2.5f, -6f), new Vector3(2f, 1f, 2f)),
    };

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var existing = GameObject.Find(ParentName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var parent = new GameObject(ParentName);
        parent.transform.position = Vector3.zero;

        foreach (var (name, position, scale) in Platforms)
        {
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.SetParent(parent.transform);
            platform.transform.localPosition = position;
            platform.transform.localScale = scale;
        }

        if (GameObject.Find("Player") == null)
        {
            Debug.LogError("TASK1_PLAYER_MISSING");
            throw new System.InvalidOperationException(
                "TASK1_PLAYER_MISSING: no GameObject named Player in " + ScenePath);
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK1_BUILD_OK platforms=" + Platforms.Length);
    }
}
