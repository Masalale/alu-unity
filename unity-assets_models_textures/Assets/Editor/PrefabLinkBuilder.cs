using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step 0 builder: links the plain Player and TimerCanvas objects in Level01 to
/// their prefab assets. The scene components and references are kept as overrides
/// on the Player instance, while the TimerCanvas instance is applied back to its
/// prefab so the two match exactly.
/// </summary>
public static class PrefabLinkBuilder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string TimerCanvasPrefabPath = "Assets/Prefabs/TimerCanvas.prefab";
    private const string PlayerName = "Player";
    private const string TimerCanvasName = "TimerCanvas";
    private const string TimerTextName = "TimerText";
    private const string TriggerName = "TimerTrigger";
    private const string WinFlagName = "WinFlag";
    private const string CameraName = "Main Camera";

    private static readonly Vector3 PlayerRootPosition = new Vector3(0f, 1.25f, 0f);

    private static int failedChecks;

    public static void Build()
    {
        failedChecks = 0;

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var player = GameObject.Find(PlayerName);
        var timerCanvas = GameObject.Find(TimerCanvasName);
        var timerText = GameObject.Find(TimerTextName);
        var camera = GameObject.Find(CameraName);
        var trigger = GameObject.Find(TriggerName);
        var winFlag = GameObject.Find(WinFlagName);
        if (player == null || timerCanvas == null || timerText == null
            || camera == null || trigger == null || winFlag == null)
        {
            Debug.LogError("PREFAB_LINK_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "PREFAB_LINK_SCENE_BROKEN: Level01.unity is missing a required scene object");
        }

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        var timerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TimerCanvasPrefabPath);
        if (playerPrefab == null || timerCanvasPrefab == null)
        {
            Debug.LogError("PREFAB_LINK_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "PREFAB_LINK_SCENE_BROKEN: Player or TimerCanvas prefab asset is missing");
        }

        EnsurePlayerPrefabRoot();

        if (PrefabUtility.IsPartOfPrefabInstance(player))
        {
            Debug.Log("PREFAB_LINK_SKIP Player already linked");
        }
        else
        {
            PrefabUtility.ConvertToPrefabInstance(player, playerPrefab, CreateSettings(), InteractionMode.AutomatedAction);
            Debug.Log("PREFAB_LINK_PLAYER_CONVERTED");
        }

        if (PrefabUtility.IsPartOfPrefabInstance(timerCanvas))
        {
            Debug.Log("PREFAB_LINK_SKIP TimerCanvas already linked");
        }
        else
        {
            PrefabUtility.ConvertToPrefabInstance(timerCanvas, timerCanvasPrefab, CreateSettings(), InteractionMode.AutomatedAction);
            Debug.Log("PREFAB_LINK_TIMER_CONVERTED");
        }

        // The TimerCanvas prefab is brought up to date with the scene so both match.
        PrefabUtility.ApplyPrefabInstance(timerCanvas, InteractionMode.AutomatedAction);
        Debug.Log("PREFAB_LINK_TIMER_APPLIED");

        CheckLinks(player, playerPrefab, timerCanvas, timerCanvasPrefab);
        CheckReferences(player, camera, timerText, trigger, winFlag);
        CheckTimerDisabled(player);
        CheckTimerCanvasValues(timerCanvasPrefab, "PREFAB_LINK_CHECK_PREFAB_TIMER");
        CheckTimerCanvasValues(timerCanvas, "PREFAB_LINK_CHECK_SCENE_TIMER");

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (failedChecks > 0)
        {
            throw new System.InvalidOperationException(
                "PREFAB_LINK_CHECKS_FAILED: " + failedChecks + " check(s) failed");
        }

        Debug.Log("PREFAB_LINK_BUILD_OK");
    }

    private static ConvertToPrefabInstanceSettings CreateSettings()
    {
        return new ConvertToPrefabInstanceSettings
        {
            objectMatchMode = ObjectMatchMode.ByHierarchy,
            componentsNotMatchedBecomesOverride = true,
            gameObjectsNotMatchedBecomesOverride = true,
            changeRootNameToAssetName = false,
            recordPropertyOverridesOfMatches = true,
            logInfo = true,
        };
    }

    // The Player prefab root must sit at the spawn point, so it is fixed if it drifted.
    private static void EnsurePlayerPrefabRoot()
    {
        var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        var rootTransform = contents.transform;
        bool matches = rootTransform.localPosition == PlayerRootPosition
            && rootTransform.localRotation == Quaternion.identity
            && rootTransform.localScale == Vector3.one;

        if (!matches)
        {
            rootTransform.localPosition = PlayerRootPosition;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
        }

        PrefabUtility.UnloadPrefabContents(contents);
        Debug.Log("PREFAB_LINK_PLAYER_ROOT_" + (matches ? "OK" : "FIXED"));
    }

    private static void CheckLinks(GameObject player, GameObject playerPrefab,
        GameObject timerCanvas, GameObject timerCanvasPrefab)
    {
        Check("PREFAB_LINK_CHECK_PLAYER_INSTANCE",
            PrefabUtility.GetNearestPrefabInstanceRoot(player) == player
            && PrefabUtility.GetCorrespondingObjectFromSource(player) == playerPrefab);
        Check("PREFAB_LINK_CHECK_TIMER_INSTANCE",
            PrefabUtility.GetNearestPrefabInstanceRoot(timerCanvas) == timerCanvas
            && PrefabUtility.GetCorrespondingObjectFromSource(timerCanvas) == timerCanvasPrefab);
    }

    private static void CheckReferences(GameObject player, GameObject camera,
        GameObject timerText, GameObject trigger, GameObject winFlag)
    {
        var cameraController = camera.GetComponent<CameraController>();
        var timer = player.GetComponent<Timer>();
        var timerTrigger = trigger.GetComponent<TimerTrigger>();
        var winTrigger = winFlag.GetComponent<WinTrigger>();

        Check("PREFAB_LINK_CHECK_REF_CAMERA_PLAYER",
            cameraController != null && IsReference(cameraController, "player", player.transform));
        Check("PREFAB_LINK_CHECK_REF_TIMER_TEXT",
            timer != null && IsReference(timer, "timerText", timerText.GetComponent<Text>()));
        Check("PREFAB_LINK_CHECK_REF_TIMER_TRIGGER_PLAYER",
            timerTrigger != null && IsReference(timerTrigger, "player", player));
        Check("PREFAB_LINK_CHECK_REF_WIN_TRIGGER_PLAYER",
            winTrigger != null && IsReference(winTrigger, "player", player));
    }

    private static void CheckTimerDisabled(GameObject player)
    {
        var timer = player.GetComponent<Timer>();
        Check("PREFAB_LINK_CHECK_TIMER_DISABLED", timer != null && !timer.enabled);
    }

    private static void CheckTimerCanvasValues(GameObject root, string label)
    {
        var canvas = root.GetComponent<Canvas>();
        var scaler = root.GetComponent<CanvasScaler>();
        var text = root.GetComponentInChildren<Text>(true);
        var rect = text != null ? text.rectTransform : null;

        Check(label + "_CANVAS",
            canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay && !canvas.pixelPerfect);
        Check(label + "_SCALER",
            scaler != null
            && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize
            && scaler.referenceResolution == new Vector2(1280f, 800f)
            && scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
            && scaler.matchWidthOrHeight == 1f);
        Check(label + "_RECT",
            rect != null
            && rect.sizeDelta == new Vector2(160f, 30f)
            && rect.anchoredPosition == new Vector2(0f, -40f)
            && rect.anchorMin == new Vector2(0.5f, 1f)
            && rect.anchorMax == new Vector2(0.5f, 1f));
        Check(label + "_TEXT",
            text != null
            && text.fontSize == 48
            && text.color == Color.white
            && text.alignment == TextAnchor.MiddleCenter
            && text.horizontalOverflow == HorizontalWrapMode.Overflow
            && text.verticalOverflow == VerticalWrapMode.Overflow
            && !text.raycastTarget
            && text.text == "0:00.00");
    }

    private static bool IsReference(Object component, string propertyName, Object expected)
    {
        var serialized = new SerializedObject(component);
        var property = serialized.FindProperty(propertyName);
        return property != null && expected != null && property.objectReferenceValue == expected;
    }

    private static void Check(string name, bool passed)
    {
        Debug.Log(name + " ok=" + passed);
        if (!passed)
        {
            failedChecks++;
        }
    }
}
