using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 9 builder: attaches the goal trigger to the win flag and aims it at the
/// player. The flag keeps the transform, collider and scene-root placement built
/// in task 2, so the new script is the only thing added to it.
/// </summary>
public static class Task9Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string WinFlagName = "WinFlag";
    private const string PlayerName = "Player";

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var winFlag = GameObject.Find(WinFlagName);
        var player = GameObject.Find(PlayerName);
        if (winFlag == null || player == null)
        {
            Debug.LogError("TASK9_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK9_SCENE_BROKEN: Level01.unity is missing " + WinFlagName + " or " + PlayerName);
        }

        var winTrigger = winFlag.GetComponent<WinTrigger>();
        if (winTrigger == null)
        {
            winTrigger = winFlag.AddComponent<WinTrigger>();
        }

        // Assigned through SerializedObject so the reference is written into the
        // saved scene file; a plain field assignment would not be serialised. It is
        // also required at runtime, since the script caches the player's Timer in
        // Awake and would throw on a null player.
        var serialized = new SerializedObject(winTrigger);
        serialized.FindProperty("player").objectReferenceValue = player;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK9_BUILD_OK winTriggerOn=" + WinFlagName
            + " player=" + winTrigger.player.name
            + " flagPosition=" + winFlag.transform.position
            + " flagScale=" + winFlag.transform.localScale
            + " flagParent=" + (winFlag.transform.parent == null ? "none (scene root)" : winFlag.transform.parent.name));
    }
}