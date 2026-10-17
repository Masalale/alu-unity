using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Task 7 builder: creates the overlay canvas that carries the run clock label and
/// saves it as a prefab. The canvas is left in the scene as well, so the label is
/// on screen before the timer script exists. Re-running the builder replaces any
/// canvas a previous run left behind instead of stacking a second one on top.
/// </summary>
public static class Task7Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string CanvasName = "TimerCanvas";
    private const string TextName = "TimerText";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/TimerCanvas.prefab";

    // Design resolution the label is authored against, and the top-centre offset it
    // sits at below the top edge of the screen.
    private static readonly Vector2 ReferenceResolution = new Vector2(1280f, 800f);

    private static readonly Vector2 TextSize = new Vector2(160f, 30f);
    private static readonly Vector2 TopCentreAnchor = new Vector2(0.5f, 1f);

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var existing = FindInScene(scene, CanvasName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var timerCanvas = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas));

        var canvas = timerCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        // Added explicitly: AddComponent<Canvas>() on its own leaves the canvas
        // unscaled, so the scaler is what makes the label resolution independent.
        var scaler = timerCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        // Screen Match is Expand rather than the Match Width Or Height default, so the
        // label keeps its authored pixel size and the canvas grows into the extra
        // space on whichever axis the aspect ratio has to spare.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 1f;

        var textObject = new GameObject(TextName, typeof(RectTransform));
        textObject.transform.SetParent(timerCanvas.transform, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = TopCentreAnchor;
        textRect.anchorMax = TopCentreAnchor;
        textRect.sizeDelta = TextSize;
        textRect.anchoredPosition = new Vector3(0f, -40f, 0f);
        textRect.localScale = Vector3.one;

        var label = textObject.AddComponent<Text>();
        // The builtin Arial.ttf was renamed in Unity 6; LegacyRuntime.ttf is the
        // replacement that ships with the editor.
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "0:00.00";
        label.fontSize = 48;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        // Wrap horizontally and Truncate vertically, both against the Text component's
        // Overflow default: the clock reading is a fixed m:ss.ff shape that must
        // stay on one line, so the label is laid out by its own rect rather than
        // reflowed by the label box.
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        // The AssetDatabase has to know the destination folder exists before a prefab
        // can be written into it, whether or not the folder is already on disk.
        AssetDatabase.Refresh();
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        PrefabUtility.SaveAsPrefabAsset(timerCanvas, PrefabPath);

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK7_BUILD_OK canvas=" + CanvasName
            + " renderMode=" + canvas.renderMode
            + " pixelPerfect=" + canvas.pixelPerfect
            + " scaleMode=" + scaler.uiScaleMode
            + " referenceResolution=" + scaler.referenceResolution
            + " matchWidthOrHeight=" + scaler.matchWidthOrHeight
            + " text=" + TextName
            + " fontSize=" + label.fontSize
            + " font=" + label.font.name
            + " textValue=" + label.text
            + " prefab=" + PrefabPath);
    }

    // Finds a GameObject by name anywhere in the scene, including inactive ones,
    // so a previous run's canvas is always found and replaced.
    private static Transform FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var match = FindByName(root.transform, name);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static Transform FindByName(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            var match = FindByName(child, name);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}