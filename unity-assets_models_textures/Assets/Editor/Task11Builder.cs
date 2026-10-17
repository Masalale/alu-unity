using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 11 builder: moves the embedded materials of the Kenney nature models into
/// Assets/Materials, then swaps each cube platform for a cliff block model. Every
/// model is fitted to the exact bounds of the cube it replaces, so the platform
/// tops and the jump path keep their heights.
/// </summary>
public static class Task11Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string ModelsFolder = "Assets/Models";
    private const string MaterialsFolder = "Assets/Materials";
    private const string PlatformModelPath = "Assets/Models/cliff_block_rock.fbx";
    private const string PlatformsName = "Platforms";
    private const string WoodPath = "Assets/Materials/wood.mat";
    private const string WoodPathCased = "Assets/Materials/Wood.mat";
    private const string WoodName = "Wood";
    private const int PlatformCount = 10;
    private const float BoundsTolerance = 0.001f;

    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ExtractModelMaterials();
        RenameWoodMaterial();
        CheckNoLooseMaterialsInModels();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ReplacePlatforms();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK11_BUILD_OK");
    }

    private static void ExtractModelMaterials()
    {
        var guids = AssetDatabase.FindAssets("t:Model", new[] { ModelsFolder });
        int modelCount = 0;
        int remapCount = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                continue;
            }
            modelCount++;

            var embedded = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>().ToList();
            if (embedded.Count == 0)
            {
                continue;
            }

            foreach (var material in embedded)
            {
                var name = material.name;
                var target = MaterialsFolder + "/" + name + ".mat";

                // Shared names such as grass or dirt become one material used by every model.
                if (AssetDatabase.LoadAssetAtPath<Material>(target) == null)
                {
                    AssetDatabase.ExtractAsset(material, target);
                }

                var external = AssetDatabase.LoadAssetAtPath<Material>(target);
                if (external == null)
                {
                    Debug.LogError("TASK11_EXTRACT_FAILED " + target);
                    throw new System.InvalidOperationException("TASK11_EXTRACT_FAILED: " + target);
                }

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), external);
                remapCount++;
            }

            importer.SaveAndReimport();
        }

        Debug.Log("TASK11_MATERIALS models=" + modelCount + " remaps=" + remapCount);
    }

    private static void RenameWoodMaterial()
    {
        var wood = AssetDatabase.LoadAssetAtPath<Material>(WoodPath);
        if (wood != null && wood.name != WoodName)
        {
            var error = AssetDatabase.RenameAsset(WoodPath, WoodName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("TASK11_WOOD_RENAME_FAILED " + error);
                throw new System.InvalidOperationException("TASK11_WOOD_RENAME_FAILED: " + error);
            }
            Debug.Log("TASK11_WOOD_RENAMED to " + WoodPathCased);
        }
        else if (AssetDatabase.AssetPathToGUID(WoodPathCased) == string.Empty)
        {
            Debug.LogError("TASK11_WOOD_MISSING");
            throw new System.InvalidOperationException("TASK11_WOOD_MISSING: no wood material to rename");
        }
        else
        {
            Debug.Log("TASK11_WOOD_OK already " + WoodPathCased);
        }
    }

    private static void CheckNoLooseMaterialsInModels()
    {
        var loose = AssetDatabase.FindAssets("t:Material", new[] { ModelsFolder });
        Debug.Log("TASK11_LOOSE_MATERIALS_IN_MODELS count=" + loose.Length);
        if (loose.Length > 0)
        {
            throw new System.InvalidOperationException("TASK11_LOOSE_MATERIALS_IN_MODELS: " + loose.Length);
        }
    }

    private static void ReplacePlatforms()
    {
        var platforms = GameObject.Find(PlatformsName);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformModelPath);
        if (platforms == null || model == null)
        {
            Debug.LogError("TASK11_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK11_SCENE_BROKEN: missing " + PlatformsName + " or " + PlatformModelPath);
        }

        var parent = platforms.transform;
        for (int i = 1; i <= PlatformCount; i++)
        {
            var name = "Platform" + i.ToString("00");
            var child = parent.Find(name);
            if (child == null)
            {
                Debug.LogError("TASK11_SCENE_BROKEN " + name);
                throw new System.InvalidOperationException("TASK11_SCENE_BROKEN: missing " + name);
            }

            if (IsModelInstance(child.gameObject))
            {
                Debug.Log("TASK11_SKIP " + name + " already a model instance");
                LogPlatform(child.gameObject, null);
                continue;
            }

            var cubeRenderer = child.GetComponent<MeshRenderer>();
            if (cubeRenderer == null)
            {
                Debug.LogError("TASK11_SCENE_BROKEN " + name + " has no MeshRenderer");
                throw new System.InvalidOperationException("TASK11_SCENE_BROKEN: " + name + " has no MeshRenderer");
            }

            var target = cubeRenderer.bounds;
            var siblingIndex = child.GetSiblingIndex();
            child.name = name + "_cube";

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            instance.name = name;
            instance.transform.SetSiblingIndex(siblingIndex);

            FitToBounds(instance, target);
            AddMeshColliders(instance);
            Object.DestroyImmediate(child.gameObject);

            LogPlatform(instance, target);
        }
    }

    private static bool IsModelInstance(GameObject candidate)
    {
        if (!PrefabUtility.IsPartOfPrefabInstance(candidate))
        {
            return false;
        }

        var source = PrefabUtility.GetCorrespondingObjectFromSource(candidate);
        return source != null && AssetDatabase.GetAssetPath(source) == PlatformModelPath;
    }

    // Scales the instance per axis and then moves it, repeated so rotated child meshes converge.
    private static void FitToBounds(GameObject instance, Bounds target)
    {
        var root = instance.transform;
        for (int pass = 0; pass < 3; pass++)
        {
            var current = CombinedBounds(instance);
            var ratio = new Vector3(
                target.size.x / current.size.x,
                target.size.y / current.size.y,
                target.size.z / current.size.z);
            root.localScale = Vector3.Scale(root.localScale, ratio);

            var scaled = CombinedBounds(instance);
            root.position += target.center - scaled.center;
        }
    }

    private static void AddMeshColliders(GameObject instance)
    {
        foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
        {
            var collider = filter.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = filter.gameObject.AddComponent<MeshCollider>();
            }
            collider.sharedMesh = filter.sharedMesh;
            collider.convex = false;
        }
    }

    private static void LogPlatform(GameObject platform, Bounds? expected)
    {
        var bounds = CombinedBounds(platform);
        var line = "TASK11_PLATFORM " + platform.name
            + " min=" + bounds.min.ToString("F3")
            + " max=" + bounds.max.ToString("F3");

        if (expected == null)
        {
            Debug.Log(line);
            return;
        }

        var target = expected.Value;
        bool matches = Vector3.Distance(bounds.min, target.min) <= BoundsTolerance
            && Vector3.Distance(bounds.max, target.max) <= BoundsTolerance;
        Debug.Log(line
            + " expectedMin=" + target.min.ToString("F3")
            + " expectedMax=" + target.max.ToString("F3")
            + " matches=" + matches);

        if (!matches)
        {
            Debug.LogError("TASK11_BOUNDS_MISMATCH " + platform.name);
            throw new System.InvalidOperationException("TASK11_BOUNDS_MISMATCH: " + platform.name);
        }
    }

    private static Bounds CombinedBounds(GameObject instance)
    {
        var renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            throw new System.InvalidOperationException("TASK11_NO_RENDERERS " + instance.name);
        }

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
