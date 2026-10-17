using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 0 builder: creates the Level01 scene containing a Player capsule
/// placeholder and saves the Player as a prefab.
/// </summary>
public static class Task0Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/Player.prefab";

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.25f, 0f);
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;

        EditorSceneManager.SaveScene(scene, ScenePath);

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK0_BUILD_OK");
    }
}