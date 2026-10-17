using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 3 builder: attaches the player movement script to the Player capsule
/// and gives it the CharacterController that script drives. The capsule
/// primitive's own CapsuleCollider is left in place, so the player keeps
/// colliding exactly as it did before.
/// </summary>
public static class Task3Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string PlayerName = "Player";

    /// <summary>
    /// The capsule primitive is a 2 unit tall, 1 unit wide capsule, so the
    /// CharacterController matches the visible mesh: radius 0.5, height 2,
    /// centred on the transform origin at the player's mid-height.
    /// </summary>
    private static void Configure(CharacterController controller)
    {
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0f, 0f, 0f);
    }

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var player = GameObject.Find(PlayerName);
        if (player == null)
        {
            Debug.LogError("TASK3_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK3_SCENE_BROKEN: Level01.unity is missing " + PlayerName);
        }

        if (player.GetComponent<PlayerController>() == null)
        {
            player.AddComponent<PlayerController>();
        }

        // CreatePrimitive supplies a CapsuleCollider, never a CharacterController,
        // so this is normally added here rather than found.
        var controller = player.GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = player.AddComponent<CharacterController>();
        }

        Configure(controller);

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK3_BUILD_OK player=" + PlayerName
            + " script=" + player.GetComponent<PlayerController>().GetType().Name
            + " height=" + controller.height
            + " radius=" + controller.radius
            + " center=" + controller.center);
    }
}