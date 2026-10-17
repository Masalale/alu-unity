using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 5 builder: attaches the follow camera script to Main Camera and aims
/// its player field at the Player transform. The camera keeps the position and
/// tilt authored in task 4, which the script reads as its starting framing, and
/// starts tracking the capsule from there.
/// </summary>
public static class Task5Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string CameraName = "Main Camera";
    private const string PlayerName = "Player";

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var mainCamera = GameObject.Find(CameraName);
        var player = GameObject.Find(PlayerName);
        if (mainCamera == null || player == null)
        {
            Debug.LogError("TASK5_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK5_SCENE_BROKEN: Level01.unity is missing " + CameraName + " or " + PlayerName);
        }

        var camera = mainCamera.GetComponent<CameraController>();
        if (camera == null)
        {
            camera = mainCamera.AddComponent<CameraController>();
        }

        // Assigned through SerializedObject so the reference is written into the
        // saved scene file; a plain field assignment would not be serialised.
        var serialized = new SerializedObject(camera);
        serialized.FindProperty("player").objectReferenceValue = player.transform;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK5_BUILD_OK camera=" + CameraName
            + " script=" + camera.GetType().Name
            + " player=" + camera.player.name
            + " playerPosition=" + camera.player.position
            + " cameraPosition=" + camera.transform.position);
    }
}