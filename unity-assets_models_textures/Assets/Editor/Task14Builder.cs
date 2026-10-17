using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 14 builder: puts the flag texture on the flag cloth and the wood material on
/// the pole. The texture is imported as a repeating 2D texture, and the flag material
/// uses the same shader as the Kenney materials.
/// </summary>
public static class Task14Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string TexturePath = "Assets/Textures/Flag.png";
    private const string MaterialPath = "Assets/Materials/Flag.mat";
    private const string ReferenceMaterialPath = "Assets/Materials/grass.mat";
    private const string WoodPath = "Assets/Materials/Wood.mat";
    private const string FlagModelPath = "Assets/Models/Flag.fbx";
    private const string WinFlagName = "WinFlag";
    private const string FlagName = "Flag";
    private const string ClothName = "Flag";
    private const string PoleName = "Flagpost";

    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var texture = ImportTexture();
        var reference = AssetDatabase.LoadAssetAtPath<Material>(ReferenceMaterialPath);
        var wood = AssetDatabase.LoadAssetAtPath<Material>(WoodPath);
        if (reference == null || wood == null)
        {
            Debug.LogError("TASK14_MISSING_MATERIAL");
            throw new System.InvalidOperationException("TASK14_MISSING_MATERIAL: grass or Wood");
        }

        var flagMaterial = FindOrCreateFlagMaterial(reference.shader, texture);
        Debug.Log("TASK14_MATERIAL Flag guid=" + AssetDatabase.AssetPathToGUID(MaterialPath)
            + " shader=" + flagMaterial.shader.name + " referenceShader=" + reference.shader.name);
        Debug.Log("TASK14_MATERIAL Wood guid=" + AssetDatabase.AssetPathToGUID(WoodPath));
        Debug.Log("TASK14_FBX_EMBEDDED_MATERIALS count="
            + AssetDatabase.LoadAllAssetsAtPath(FlagModelPath).OfType<Material>().Count());

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var winFlag = GameObject.Find(WinFlagName);
        var flag = winFlag != null ? winFlag.transform.Find(FlagName) : null;
        if (flag == null)
        {
            Debug.LogError("TASK14_SCENE_BROKEN");
            throw new System.InvalidOperationException("TASK14_SCENE_BROKEN: no " + WinFlagName + "/" + FlagName);
        }

        var cloth = FindPart(flag, ClothName);
        var pole = FindPart(flag, PoleName);
        AssignMaterial(cloth, flagMaterial);
        AssignMaterial(pole, wood);

        Debug.Log("TASK14_RENDERERS cloth=" + cloth.name + " material=" + cloth.sharedMaterial.name
            + " pole=" + pole.name + " material=" + pole.sharedMaterial.name);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK14_BUILD_OK");
    }

    private static Texture2D ImportTexture()
    {
        var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("TASK14_NO_TEXTURE " + TexturePath);
            throw new System.InvalidOperationException("TASK14_NO_TEXTURE: " + TexturePath);
        }

        importer.textureType = TextureImporterType.Default;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.SaveAndReimport();

        Debug.Log("TASK14_TEXTURE type=" + importer.textureType
            + " shape=" + importer.textureShape
            + " wrap=" + importer.wrapMode);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
    }

    private static Material FindOrCreateFlagMaterial(Shader shader, Texture2D texture)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "Flag" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        // The built-in Standard shader names its albedo _MainTex; the newer shaders use _BaseMap.
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static Renderer FindPart(Transform flag, string name)
    {
        var part = flag.GetComponentsInChildren<Renderer>(true)
            .FirstOrDefault(renderer => renderer.name == name && renderer.transform != flag);
        if (part == null)
        {
            Debug.LogError("TASK14_NO_PART " + name);
            throw new System.InvalidOperationException("TASK14_NO_PART: " + name);
        }
        return part;
    }

    private static void AssignMaterial(Renderer part, Material material)
    {
        var materials = part.sharedMaterials;
        if (materials.Length == 0)
        {
            Debug.LogError("TASK14_NO_SLOTS " + part.name);
            throw new System.InvalidOperationException("TASK14_NO_SLOTS: " + part.name);
        }

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = material;
        }
        part.sharedMaterials = materials;
    }
}
