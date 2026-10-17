using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Task 8 builder: gives the player the run clock and drops the start line that
/// arms it. The clock is left disabled so it does not start counting until the
/// player actually walks off the start platform, which is what the start line
/// trigger is for.
/// </summary>
public static class Task8Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string PlayerName = "Player";
    private const string TextName = "TimerText";
    private const string TriggerName = "TimerTrigger";

    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var player = GameObject.Find(PlayerName);
        var textObject = GameObject.Find(TextName);
        if (player == null || textObject == null)
        {
            Debug.LogError("TASK8_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK8_SCENE_BROKEN: Level01.unity is missing " + PlayerName + " or " + TextName);
        }

        var timerText = textObject.GetComponent<Text>();
        if (timerText == null)
        {
            Debug.LogError("TASK8_SCENE_BROKEN");
            throw new System.InvalidOperationException(
                "TASK8_SCENE_BROKEN: " + TextName + " has no Text component");
        }

        var timer = player.GetComponent<Timer>();
        if (timer == null)
        {
            timer = player.AddComponent<Timer>();
        }

        // Assigned through SerializedObject so the reference is written into the
        // saved scene file; a plain field assignment would not be serialised.
        var timerSerialized = new SerializedObject(timer);
        timerSerialized.FindProperty("timerText").objectReferenceValue = timerText;
        timerSerialized.ApplyModifiedPropertiesWithoutUndo();

        timer.enabled = false;

        var triggerObject = GameObject.Find(TriggerName);
        if (triggerObject == null)
        {
            triggerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        triggerObject.name = TriggerName;
        triggerObject.transform.position = new Vector3(0f, 1.25f, 0f);
        triggerObject.transform.localScale = new Vector3(0f, 2f, 0f);

        // Flattened onto the start line, so it is never seen or lit.
        triggerObject.GetComponent<MeshRenderer>().enabled = false;

        // Is Trigger is what makes OnTriggerExit fire at all; a solid collider would
        // just block the player on the spawn platform instead.
        triggerObject.GetComponent<BoxCollider>().isTrigger = true;

        var trigger = triggerObject.GetComponent<TimerTrigger>();
        if (trigger == null)
        {
            trigger = triggerObject.AddComponent<TimerTrigger>();
        }

        var triggerSerialized = new SerializedObject(trigger);
        triggerSerialized.FindProperty("player").objectReferenceValue = player;
        triggerSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TASK8_BUILD_OK timerOn=" + PlayerName
            + " timerText=" + textObject.name
            + " timerEnabled=" + timer.enabled
            + " trigger=" + triggerObject.name
            + " triggerPosition=" + triggerObject.transform.position
            + " triggerScale=" + triggerObject.transform.localScale
            + " triggerIsTrigger=" + triggerObject.GetComponent<BoxCollider>().isTrigger
            + " triggerPlayer=" + trigger.player.name
            + " meshRendererEnabled=" + triggerObject.GetComponent<MeshRenderer>().enabled);
    }
}