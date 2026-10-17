using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Task 12 builder: dresses the platforms with trees, rocks and flowers under an
/// Environment root. Trees and rocks stay on the edges of the wider platforms and
/// the dead ends, flowers can sit on any top. Every item is kept clear of the
/// straight line between consecutive path platforms, the spawn and the flag.
/// </summary>
public static class Task12Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string ModelsFolder = "Assets/Models/";
    private const string EnvironmentName = "Environment";
    private const string PlatformsName = "Platforms";
    private const string TreesName = "Trees";
    private const string RocksName = "Rocks";
    private const string PlantsName = "Plants";
    private const string WinFlagName = "WinFlag";
    private const string FlagName = "Flag";
    private const int PlatformCount = 10;
    private const int PathPlatformCount = 8;
    private const int Seed = 12;
    private const int MaxTries = 400;
    private const int PathSamples = 400;
    private const float PathClearance = 1f;
    private const float PointClearance = 1.2f;
    private const float PlatformInset = 0.3f;
    private const float EdgeBand = 0.9f;
    private const float OverlapGap = 0.1f;

    private static readonly Vector2 SpawnXZ = new Vector2(0f, 0f);
    private static readonly Vector2 FlagXZ = new Vector2(30f, 0f);

    private static readonly string[] TreeModels =
    {
        "tree_default", "tree_oak", "tree_pineRoundA", "tree_fat", "tree_pineTallA", "tree_simple",
    };

    private static readonly string[] RockModels =
    {
        "rock_largeA", "rock_tallA", "stone_largeB", "rock_largeC", "stone_tallA",
    };

    private static readonly string[] PlantModels =
    {
        "flower_redA", "flower_purpleA", "grass", "grass_large", "mushroom_red",
        "plant_bush", "flower_yellowA", "mushroom_tan", "plant_bushSmall", "flower_redB",
    };

    // Wider platforms and dead ends. Trees and rocks never go on the path platforms 02 and 05.
    private static readonly int[] EdgePlatforms = { 1, 3, 4, 6, 7, 8, 9, 10 };
    private static readonly int[] AllPlatforms = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    // Items that stand at a hand-set spot, so they stay out of the follow camera's view.
    private static readonly Dictionary<string, FixedSpot> FixedSpots = new Dictionary<string, FixedSpot>
    {
        { "tree_default_01", new FixedSpot(1, -1.6f, 1.6f) },
        { "stone_largeB_09", new FixedSpot(1, 1.5f, 1.9f) },
        { "tree_pineRoundA_03", new FixedSpot(8, 31.9f, 1.7f) },
        { "rock_largeC_10", new FixedSpot(8, 31.9f, -1.9f) },
    };

    // Items taken out of the level. Their slot is kept so the other item names do not change.
    private static readonly string[] RemovedNames = { "tree_fat_04" };

    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var platforms = GameObject.Find(PlatformsName);
        if (platforms == null)
        {
            Debug.LogError("TASK12_SCENE_BROKEN");
            throw new System.InvalidOperationException("TASK12_SCENE_BROKEN: missing " + PlatformsName);
        }

        var layout = ReadLayout(platforms.transform);

        var environment = FindOrCreateRoot(EnvironmentName);
        var groups = new Dictionary<string, Transform>
        {
            { TreesName, FindOrCreateChild(environment.transform, TreesName) },
            { RocksName, FindOrCreateChild(environment.transform, RocksName) },
            { PlantsName, FindOrCreateChild(environment.transform, PlantsName) },
        };

        var slots = BuildSlots();
        var rng = new System.Random(Seed);
        var placed = new List<Bounds>();
        for (int n = 0; n < slots.Count; n++)
        {
            var slot = slots[n];
            var name = slot.Model + "_" + (n + 1).ToString("00");
            var group = groups[slot.Group];

            if (System.Array.IndexOf(RemovedNames, name) >= 0)
            {
                var stale = group.Find(name);
                if (stale != null)
                {
                    Object.DestroyImmediate(stale.gameObject);
                }
                Debug.Log("TASK12_REMOVED " + name);
                continue;
            }

            FixedSpot fixedSpot;
            bool isFixed = FixedSpots.TryGetValue(name, out fixedSpot);

            var existing = group.Find(name);
            if (existing != null)
            {
                if (isFixed)
                {
                    SeatAt(existing.gameObject, layout.Tops[fixedSpot.Platform], fixedSpot.Center);
                }

                var bounds = CombinedBounds(existing.gameObject);
                if (IsClear(bounds, layout, placed))
                {
                    ApplyColliders(existing.gameObject, slot);
                    placed.Add(bounds);
                    Debug.Log((isFixed ? "TASK12_FIXED " : "TASK12_KEPT ") + name + Describe(bounds));
                    continue;
                }

                // Moved from its old spot, so it is placed again below.
                Debug.Log("TASK12_MOVE " + name);
                Object.DestroyImmediate(existing.gameObject);
            }

            PlaceNew(slot, name, group, n, layout, placed, rng, isFixed ? (FixedSpot?)fixedSpot : null);
        }

        VerifyClearance(groups, layout, FlagBounds());

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK12_BUILD_OK items=" + (slots.Count - RemovedNames.Length));
    }

    private static Layout ReadLayout(Transform platforms)
    {
        var layout = new Layout();
        for (int i = 1; i <= PlatformCount; i++)
        {
            var child = platforms.Find(PlatformName(i));
            if (child == null)
            {
                Debug.LogError("TASK12_SCENE_BROKEN " + PlatformName(i));
                throw new System.InvalidOperationException("TASK12_SCENE_BROKEN: missing " + PlatformName(i));
            }

            layout.Tops[i] = CombinedBounds(child.gameObject);
            Debug.Log("TASK12_TOP " + PlatformName(i) + " y=" + layout.Tops[i].max.y.ToString("F3"));
        }

        for (int i = 1; i < PathPlatformCount; i++)
        {
            layout.Path.Add(new Segment(layout.Tops[i].center, layout.Tops[i + 1].center));
        }
        return layout;
    }

    private static List<Slot> BuildSlots()
    {
        var slots = new List<Slot>();
        foreach (var model in TreeModels)
        {
            slots.Add(new Slot(model, TreesName, 2.8f, 3.4f, 1.6f, true, true));
        }
        foreach (var model in RockModels)
        {
            slots.Add(new Slot(model, RocksName, 0.6f, 1.0f, 1.2f, true, true));
        }
        foreach (var model in PlantModels)
        {
            slots.Add(new Slot(model, PlantsName, 0.3f, 0.6f, 0.6f, false, false));
        }
        return slots;
    }

    private static void PlaceNew(Slot slot, string name, Transform group, int n, Layout layout,
        List<Bounds> placed, System.Random rng, FixedSpot? fixedSpot)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelsFolder + slot.Model + ".fbx");
        if (prefab == null)
        {
            Debug.LogError("TASK12_MISSING_MODEL " + slot.Model);
            throw new System.InvalidOperationException("TASK12_MISSING_MODEL: " + slot.Model);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group);
        instance.name = name;
        var root = instance.transform;
        var baseRotation = root.localRotation;
        var baseScale = root.localScale;

        // Rotates the starting platform so that items spread over the level.
        var order = slot.EdgeOnly ? EdgePlatforms : AllPlatforms;
        if (fixedSpot.HasValue)
        {
            order = new[] { fixedSpot.Value.Platform };
        }

        for (int k = 0; k < order.Length; k++)
        {
            int platform = order[(n + k) % order.Length];
            var top = layout.Tops[platform];

            for (int t = 0; t < MaxTries; t++)
            {
                var point = fixedSpot.HasValue ? fixedSpot.Value.Center : RandomPoint(top, rng, slot.EdgeOnly);
                float yaw = (float)rng.NextDouble() * 360f;
                float height = Mathf.Lerp(slot.MinHeight, slot.MaxHeight, (float)rng.NextDouble());

                root.localRotation = baseRotation * Quaternion.Euler(0f, yaw, 0f);
                root.localScale = baseScale;
                var bounds = CombinedBounds(instance);
                // Height sets the size of trees; the footprint limit keeps wide rock models on the small platforms.
                float horizontal = Mathf.Max(bounds.size.x, bounds.size.z);
                root.localScale = baseScale * Mathf.Min(height / bounds.size.y, slot.MaxFootprint / horizontal);

                SeatAt(instance, top, point);

                bounds = CombinedBounds(instance);
                if (IsClear(bounds, layout, placed))
                {
                    ApplyColliders(instance, slot);
                    placed.Add(bounds);
                    Debug.Log("TASK12_PLACED " + name
                        + " platform=" + PlatformName(platform)
                        + " height=" + height.ToString("F2")
                        + " min=" + bounds.min.ToString("F3")
                        + " max=" + bounds.max.ToString("F3"));
                    return;
                }
            }
        }

        Object.DestroyImmediate(instance);
        Debug.LogError("TASK12_NO_SPOT " + name);
        throw new System.InvalidOperationException("TASK12_NO_SPOT: " + name);
    }

    private static void VerifyClearance(Dictionary<string, Transform> groups, Layout layout, Bounds flag)
    {
        var items = new List<Transform>();
        foreach (var group in groups.Values)
        {
            for (int i = 0; i < group.childCount; i++)
            {
                items.Add(group.GetChild(i));
            }
        }

        var bounds = items.Select(item => CombinedBounds(item.gameObject)).ToList();
        bool allOk = true;
        for (int i = 0; i < items.Count; i++)
        {
            var others = bounds.Where((b, j) => j != i).ToList();
            bool ok = IsClear(bounds[i], layout, others) && !bounds[i].Intersects(flag);
            allOk &= ok;
            Debug.Log("TASK12_CLEAR " + items[i].name + " ok=" + (ok ? "True" : "False"));
        }

        if (!allOk)
        {
            Debug.LogError("TASK12_CLEAR_FAILED");
            throw new System.InvalidOperationException("TASK12_CLEAR_FAILED: see TASK12_CLEAR lines");
        }
    }

    // A point on the platform top, kept PlatformInset away from the rim. Solid items
    // are only allowed within EdgeBand of the rim.
    private static Vector2 RandomPoint(Bounds top, System.Random rng, bool edgeOnly)
    {
        float minX = top.min.x + PlatformInset;
        float maxX = top.max.x - PlatformInset;
        float minZ = top.min.z + PlatformInset;
        float maxZ = top.max.z - PlatformInset;

        Vector2 point = Vector2.zero;
        for (int k = 0; k < MaxTries; k++)
        {
            float x = Mathf.Lerp(minX, maxX, (float)rng.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)rng.NextDouble());
            point = new Vector2(x, z);
            if (!edgeOnly)
            {
                return point;
            }

            float rim = Mathf.Min(
                Mathf.Min(x - top.min.x, top.max.x - x),
                Mathf.Min(z - top.min.z, top.max.z - z));
            if (rim <= EdgeBand)
            {
                return point;
            }
        }
        return point;
    }

    private static bool IsClear(Bounds bounds, Layout layout, List<Bounds> others)
    {
        foreach (var segment in layout.Path)
        {
            if (SegmentDistance(segment, bounds) < PathClearance)
            {
                return false;
            }
        }

        if (RectDistance(bounds, SpawnXZ) < PointClearance || RectDistance(bounds, FlagXZ) < PointClearance)
        {
            return false;
        }

        foreach (var other in others)
        {
            if (RectsOverlap(bounds, other))
            {
                return false;
            }
        }
        return true;
    }

    // Smallest distance from the box to the segment, sampled along the segment.
    private static float SegmentDistance(Segment segment, Bounds box)
    {
        float best = float.MaxValue;
        for (int s = 0; s <= PathSamples; s++)
        {
            var point = Vector3.Lerp(segment.A, segment.B, s / (float)PathSamples);
            best = Mathf.Min(best, box.SqrDistance(point));
        }
        return Mathf.Sqrt(best);
    }

    private static float RectDistance(Bounds box, Vector2 point)
    {
        float dx = Mathf.Max(Mathf.Abs(point.x - box.center.x) - box.extents.x, 0f);
        float dz = Mathf.Max(Mathf.Abs(point.y - box.center.z) - box.extents.z, 0f);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static bool RectsOverlap(Bounds a, Bounds b)
    {
        float gapX = a.extents.x + b.extents.x + OverlapGap;
        float gapZ = a.extents.z + b.extents.z + OverlapGap;
        return Mathf.Abs(a.center.x - b.center.x) < gapX && Mathf.Abs(a.center.z - b.center.z) < gapZ;
    }

    // Trees and rocks get non-convex mesh colliders so they block the player; flowers get none.
    private static void ApplyColliders(GameObject instance, Slot slot)
    {
        if (!slot.Solid)
        {
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }
            return;
        }

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

    // Moves the instance so its bounds sit on the top with the centre at the given XZ point.
    private static void SeatAt(GameObject instance, Bounds top, Vector2 point)
    {
        var bounds = CombinedBounds(instance);
        instance.transform.position += new Vector3(
            point.x - bounds.center.x,
            top.max.y - bounds.min.y,
            point.y - bounds.center.z);
    }

    private static Bounds FlagBounds()
    {
        var winFlag = GameObject.Find(WinFlagName);
        Transform flag = winFlag != null ? winFlag.transform.Find(FlagName) : null;
        if (flag == null)
        {
            Debug.LogError("TASK12_FLAG_MISSING");
            throw new System.InvalidOperationException("TASK12_FLAG_MISSING: " + WinFlagName + "/" + FlagName);
        }

        var bounds = CombinedBounds(flag.gameObject);
        Debug.Log("TASK12_FLAG" + Describe(bounds));
        return bounds;
    }

    private static string Describe(Bounds bounds)
    {
        return " min=" + bounds.min.ToString("F3") + " max=" + bounds.max.ToString("F3");
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        var found = GameObject.Find(name);
        return found != null ? found : new GameObject(name);
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            child = new GameObject(name).transform;
            child.SetParent(parent, false);
        }
        return child;
    }

    private static string PlatformName(int index)
    {
        return "Platform" + index.ToString("00");
    }

    private static Bounds CombinedBounds(GameObject instance)
    {
        var renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            throw new System.InvalidOperationException("TASK12_NO_RENDERERS " + instance.name);
        }

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private sealed class Slot
    {
        public readonly string Model;
        public readonly string Group;
        public readonly float MinHeight;
        public readonly float MaxHeight;
        public readonly float MaxFootprint;
        public readonly bool Solid;
        public readonly bool EdgeOnly;

        public Slot(string model, string group, float minHeight, float maxHeight, float maxFootprint,
            bool solid, bool edgeOnly)
        {
            Model = model;
            Group = group;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            MaxFootprint = maxFootprint;
            Solid = solid;
            EdgeOnly = edgeOnly;
        }
    }

    private struct FixedSpot
    {
        public readonly int Platform;
        public readonly Vector2 Center;

        public FixedSpot(int platform, float x, float z)
        {
            Platform = platform;
            Center = new Vector2(x, z);
        }
    }

    private struct Segment
    {
        public readonly Vector3 A;
        public readonly Vector3 B;

        public Segment(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }
    }

    private sealed class Layout
    {
        // Indexed by platform number, 1 to 10. Bounds are the world bounds of each platform.
        public readonly Bounds[] Tops = new Bounds[PlatformCount + 1];
        public readonly List<Segment> Path = new List<Segment>();
    }
}
