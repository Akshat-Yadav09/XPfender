using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds a separate Main Fight Area test player without altering production Build Settings.
public static class DesktopOverlayValidationBuild
{
    [MenuItem("Tools/Desktop Overlay/Build Main Fight Area Validation Player")]
    public static void Build()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save or discard your unsaved scene changes before building the validation player.");
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/DesktopOverlayValidation/XPDefender.exe"));
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-desktopValidationOutput");
        if (index >= 0 && index + 1 < args.Length) output = Path.GetFullPath(args[index + 1]);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        string bootstrapPath = "Assets/Editor/DesktopOverlayValidationBootstrap-" + Guid.NewGuid().ToString("N") + ".unity";
        Scene bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
            Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
        EditorSceneManager.SaveScene(bootstrap, bootstrapPath);
        if (!Application.isBatchMode) EditorSceneManager.CloseScene(bootstrap, true);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { bootstrapPath, "Assets/Scenes/Main Fight Area.unity" },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };
        BuildReport report;
        try { report = BuildPipeline.BuildPlayer(options); }
        finally { AssetDatabase.DeleteAsset(bootstrapPath); }
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(output), "BuildResult.txt"), report.summary.result + "; errors=" + report.summary.totalErrors
            + "; warnings=" + report.summary.totalWarnings + "; " + options.locationPathName);
    }
}
