using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Task 15 builder: makes the Windows, Linux and Mac standalone builds of Level01 under
/// Builds/. Every build is x86_64 (the Mac build is universal x64 and ARM64), and all
/// three use the Mono backend because IL2CPP cannot be cross compiled from Windows.
/// </summary>
public static class Task15Builder
{
    private const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string ProductName = "Platformer";
    private const string BuildRoot = "Builds";

    public static void Build()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = ProductName;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        SetMacArchitecture();
        AssetDatabase.SaveAssets();

        var failures = new List<string>();
        BuildPlatform(BuildTarget.StandaloneWindows64, "Windows", "Platformer.exe", failures);
        BuildPlatform(BuildTarget.StandaloneLinux64, "Linux", "Platformer.x86_64", failures);
        BuildPlatform(BuildTarget.StandaloneOSX, "Mac", "Platformer.app", failures);

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("TASK15_BUILD_FAILED: " + string.Join(", ", failures));
        }

        Debug.Log("TASK15_BUILD_OK");
    }

    private static void BuildPlatform(BuildTarget target, string folder, string executable, List<string> failures)
    {
        var location = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BuildRoot, folder, executable));
        Directory.CreateDirectory(Path.GetDirectoryName(location));

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = location,
            target = target,
            options = BuildOptions.None,
        };

        var summary = BuildPipeline.BuildPlayer(options).summary;
        Debug.Log("TASK15_BUILD " + target + " " + summary.result + " " + summary.totalSize + " " + summary.totalErrors);

        if (summary.result != BuildResult.Succeeded)
        {
            failures.Add(target.ToString());
        }
    }

    private static void SetMacArchitecture()
    {
        // Looked up by name so the builder still runs on editors that lack the setting.
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("UnityEditor.OSXStandalone.UserBuildSettings"))
            .FirstOrDefault(found => found != null);
        var property = type == null ? null : type.GetProperty("architecture", BindingFlags.Public | BindingFlags.Static);
        if (property == null || !Enum.IsDefined(property.PropertyType, "x64ARM64"))
        {
            Debug.Log("TASK15_MAC_ARCH unavailable");
            return;
        }

        property.SetValue(null, Enum.Parse(property.PropertyType, "x64ARM64"));
        Debug.Log("TASK15_MAC_ARCH " + property.GetValue(null));
    }
}
