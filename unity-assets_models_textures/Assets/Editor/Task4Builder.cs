using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 4 builder: places the Main Camera behind and above the player at the
/// authored tilt, framing the start of the platform path. Only the transform is
/// touched: the camera keeps its scale and carries no script yet, the follow
/// behaviour comes in task 5.
/// </summary>
public static class Task4Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string CameraName = "Main Camera";

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var mainCamera = GameObject.Find(CameraName);
        if (mainCamera == null)
        {
            Debug.LogError("TASK4_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK4_SCENE_BROKEN: Level01.unity is missing " + CameraName);
        }

        // 6.25 units behind the player at 2.5 units high, tilted 9 degrees down
        // so the platforms ahead stay in frame.
        mainCamera.transform.position = new Vector3(0f, 2.5f, -6.25f);
        mainCamera.transform.rotation = Quaternion.Euler(9f, 0f, 0f);

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK4_BUILD_OK camera=" + CameraName
            + " position=" + mainCamera.transform.position
            + " euler=" + mainCamera.transform.eulerAngles
            + " scale=" + mainCamera.transform.localScale);
    }
}