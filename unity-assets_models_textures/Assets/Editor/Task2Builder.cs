using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 2 builder: adds the WinFlag placeholder cylinder at the end of the
/// platform path. The flag is a scene root, never a child of Platforms, and its
/// capsule collider is a trigger so it only detects the player.
/// </summary>
public static class Task2Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string FlagName = "WinFlag";

    /// <summary>
    /// Top-centre of Platform08, the end platform: localPosition (30, 2.5, 0)
    /// with localScale (5, 1, 5) puts its top face at y = 3. The collider is 6
    /// units tall and centred here, spanning y = 0..6, which covers a player
    /// standing on that platform.
    /// </summary>
    private static readonly Vector3 FlagPosition = new Vector3(30f, 3f, 0f);

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var existing = GameObject.Find(FlagName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        // CreatePrimitive already attaches a CapsuleCollider, so configure that
        // component rather than adding a second one.
        var flag = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flag.name = FlagName;
        flag.transform.position = FlagPosition;
        flag.transform.rotation = Quaternion.identity;

        var trigger = flag.GetComponent<CapsuleCollider>();
        trigger.isTrigger = true;
        trigger.direction = 1;
        trigger.radius = 0.7f;
        trigger.height = 6f;

        if (GameObject.Find("Player") == null || GameObject.Find("Platforms") == null)
        {
            Debug.LogError("TASK2_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK2_SCENE_BROKEN: Level01.unity is missing Player or Platforms");
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK2_BUILD_OK flag=" + FlagName
            + " position=" + FlagPosition
            + " isTrigger=" + trigger.isTrigger
            + " radius=" + trigger.radius
            + " height=" + trigger.height
            + " direction=" + trigger.direction);
    }
}