using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 10 builder: sets the midday Farland Skies Cloudy Crown material as the
/// scene skybox. The material and its textures are imported from the asset pack
/// into Assets/Materials/Skyboxes, so the scene only needs the reference.
/// </summary>
public static class Task10Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string SkyboxPath = "Assets/Materials/Skyboxes/CloudyCrown_01_Midday/CloudyCrown_Midday.mat";
    private const string SkyboxShaderName = "Skybox/6 Sided";

    public static void Build()
    {
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
        if (skybox == null)
        {
            Debug.LogError("TASK10_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK10_SCENE_BROKEN: missing skybox material at " + SkyboxPath);
        }

        if (skybox.shader == null || skybox.shader.name != SkyboxShaderName)
        {
            Debug.LogError("TASK10_SHADER_UNRESOLVED");
            throw new System.InvalidOperationException(
                "TASK10_SHADER_UNRESOLVED: " + SkyboxPath + " uses shader "
                + (skybox.shader == null ? "null" : skybox.shader.name));
        }

        RenderSettings.skybox = skybox;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK10_BUILD_OK skybox=" + skybox.name
            + " shader=" + skybox.shader.name
            + " guid=" + AssetDatabase.AssetPathToGUID(SkyboxPath)
            + " sceneSkybox=" + (RenderSettings.skybox == null ? "none" : RenderSettings.skybox.name));
    }
}
