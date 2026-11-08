using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

// GitHub Actions (game-ci/unity-builder) から呼ばれる Windows 向けビルド入口。
// Addressables のコンテンツビルドを行ってからプレイヤーをビルドする。
public static class CIBuild
{
    public static void BuildWindows()
    {
        // Addressables のコンテンツを先にビルドする
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception("Addressables build failed: " + result.Error);
            }
        }

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            throw new Exception("No enabled scenes in EditorBuildSettings.");
        }

        string outputDir = "Builds/StandaloneWindows64";
        Directory.CreateDirectory(outputDir);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputDir + "/bunkasai-2023-otoge.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None, // Development Build ではない配布用ビルド
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Player build failed: " + report.summary.result
                + " (errors: " + report.summary.totalErrors + ")");
        }

        Debug.Log("Build succeeded: " + options.locationPathName
            + " (" + report.summary.totalSize + " bytes)");
    }
}
