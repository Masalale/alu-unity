using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 13 builder: swaps the cylinder placeholder on the win flag for the flag model.
/// The model is a child of WinFlag and its pole stands on the top of Platform08 inside
/// the win trigger. The cylinder renderer is turned off, but its collider and trigger
/// stay in place so the finish still works.
/// </summary>
public static class Task13Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string FlagModelPath = "Assets/Models/Flag.fbx";
    private const string WinFlagName = "WinFlag";
    private const string PlatformName = "Platform08";
    private const string FlagName = "Flag";
    private const string PoleName = "Flagpost";
    private const float PoleHeight = 4f;
    private const float TriggerRadius = 0.7f;
    private const float TriggerHeight = 6f;
    private const float Tolerance = 0.001f;
    private const float BaseBand = 0.05f;

    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var winFlag = GameObject.Find(WinFlagName);
        var platform = GameObject.Find(PlatformName);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(FlagModelPath);
        if (winFlag == null || platform == null || model == null)
        {
            Debug.LogError("TASK13_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK13_SCENE_BROKEN: missing " + WinFlagName + ", " + PlatformName + " or " + FlagModelPath);
        }

        Debug.Log("TASK13_ROOT " + WinFlagName + " isSceneRoot=" + (winFlag.transform.parent == null));

        var capsule = winFlag.GetComponent<CapsuleCollider>();
        var trigger = winFlag.GetComponent<WinTrigger>();
        var placeholder = winFlag.GetComponent<MeshRenderer>();
        if (capsule == null || trigger == null || placeholder == null)
        {
            Debug.LogError("TASK13_SCENE_BROKEN " + WinFlagName);
            throw new System.InvalidOperationException("TASK13_SCENE_BROKEN: " + WinFlagName + " lacks a component");
        }

        // The trigger is kept as it is; only the cylinder is hidden.
        placeholder.enabled = false;
        Debug.Log("TASK13_TRIGGER isTrigger=" + capsule.isTrigger
            + " radius=" + capsule.radius.ToString("F2")
            + " height=" + capsule.height.ToString("F2")
            + " winTrigger=" + (trigger != null)
            + " placeholderRendererEnabled=" + placeholder.enabled);
        if (!capsule.isTrigger
            || Mathf.Abs(capsule.radius - TriggerRadius) > Tolerance
            || Mathf.Abs(capsule.height - TriggerHeight) > Tolerance)
        {
            throw new System.InvalidOperationException("TASK13_TRIGGER_CHANGED: capsule no longer matches");
        }

        var flag = FindOrCreateFlag(winFlag.transform, model);
        RemoveColliders(flag.gameObject);

        // Scale from the model's own pose, so a re-run gives the same result.
        flag.localRotation = model.transform.localRotation;
        flag.localScale = model.transform.localScale;

        var pole = FindPole(flag);
        flag.localScale *= PoleHeight / pole.bounds.size.y;

        // The top crossbar makes the pole's bounds off-centre, so the post is aligned by its base vertices.
        var platformTop = CombinedBounds(platform).max.y;
        var target = winFlag.transform.position;
        var post = PostBase(pole);
        flag.position += new Vector3(target.x - post.x, platformTop - post.y, target.z - post.z);

        var postNow = PostBase(pole);
        float radius = RadialExtentBelow(pole, platformTop + PoleHeight * 0.5f, target);
        var poleBounds = pole.bounds;
        var flagBounds = CombinedBounds(flag.gameObject);
        bool insideTrigger = radius <= TriggerRadius;
        bool onTop = Mathf.Abs(postNow.y - platformTop) <= Tolerance;

        Debug.Log("TASK13_POLE bounds=" + poleBounds.min.ToString("F3") + ".." + poleBounds.max.ToString("F3")
            + " post=" + postNow.ToString("F3")
            + " radius=" + radius.ToString("F3")
            + " platformTop=" + platformTop.ToString("F3")
            + " insideTrigger=" + insideTrigger);
        Debug.Log("TASK13_FLAG bounds=" + flagBounds.min.ToString("F3") + ".." + flagBounds.max.ToString("F3")
            + " scale=" + flag.localScale.ToString("F3"));

        if (!insideTrigger || !onTop)
        {
            throw new System.InvalidOperationException("TASK13_POLE_MISPLACED");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK13_BUILD_OK");
    }

    private static Transform FindOrCreateFlag(Transform winFlag, GameObject model)
    {
        var existing = winFlag.Find(FlagName);
        if (existing != null)
        {
            if (IsModelInstance(existing.gameObject))
            {
                return existing;
            }
            Object.DestroyImmediate(existing.gameObject);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, winFlag);
        instance.name = FlagName;
        return instance.transform;
    }

    private static bool IsModelInstance(GameObject candidate)
    {
        if (!PrefabUtility.IsPartOfPrefabInstance(candidate))
        {
            return false;
        }

        var source = PrefabUtility.GetCorrespondingObjectFromSource(candidate);
        return source != null && AssetDatabase.GetAssetPath(source) == FlagModelPath;
    }

    private static Renderer FindPole(Transform flag)
    {
        var pole = flag.GetComponentsInChildren<Renderer>(true)
            .FirstOrDefault(renderer => renderer.name == PoleName);
        if (pole == null)
        {
            Debug.LogError("TASK13_NO_POLE");
            throw new System.InvalidOperationException("TASK13_NO_POLE: no " + PoleName + " renderer in the model");
        }
        return pole;
    }

    // Centre of the lowest vertices of the pole, which is the footprint of the post.
    private static Vector3 PostBase(Renderer pole)
    {
        var worlds = WorldVertices(pole);
        float lowest = worlds.Min(w => w.y);
        var lowVertices = worlds.Where(w => w.y <= lowest + BaseBand).ToList();
        var sum = Vector3.zero;
        foreach (var w in lowVertices)
        {
            sum += w;
        }
        return sum / lowVertices.Count;
    }

    // Largest distance from the win flag axis among pole vertices below the given height.
    private static float RadialExtentBelow(Renderer pole, float height, Vector3 axis)
    {
        float radius = 0f;
        foreach (var w in WorldVertices(pole))
        {
            if (w.y > height)
            {
                continue;
            }
            var dx = w.x - axis.x;
            var dz = w.z - axis.z;
            radius = Mathf.Max(radius, Mathf.Sqrt(dx * dx + dz * dz));
        }
        return radius;
    }

    private static Vector3[] WorldVertices(Renderer renderer)
    {
        var filter = renderer.GetComponent<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
        {
            throw new System.InvalidOperationException("TASK13_NO_MESH " + renderer.name);
        }
        return filter.sharedMesh.vertices.Select(v => renderer.transform.TransformPoint(v)).ToArray();
    }

    private static void RemoveColliders(GameObject instance)
    {
        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static Bounds CombinedBounds(GameObject instance)
    {
        var renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            throw new System.InvalidOperationException("TASK13_NO_RENDERERS " + instance.name);
        }

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
