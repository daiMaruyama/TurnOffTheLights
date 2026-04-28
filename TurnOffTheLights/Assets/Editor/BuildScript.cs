using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    const string ProductName = "TurnOffTheLights";
    const string BuildRoot = "Builds";

    [MenuItem("Build/Build macOS")]
    public static void BuildMacOs()
    {
        BuildStandalone(BuildTarget.StandaloneOSX, Path.Combine(BuildRoot, "macOS", ProductName + ".app"));
    }

    public static void BuildFromCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        string targetArg = GetArgument(args, "-buildTargetName") ?? "macOS";

        switch (targetArg.ToLowerInvariant())
        {
            case "mac":
            case "macos":
            case "osx":
                BuildMacOs();
                break;

            default:
                throw new ArgumentException($"Unsupported build target: {targetArg}");
        }
    }

    static void BuildStandalone(BuildTarget target, string locationPathName)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in Build Settings.");
        }

        string fullOutputPath = Path.GetFullPath(locationPathName);
        string outputDirectory = Path.GetDirectoryName(fullOutputPath);

        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new InvalidOperationException("Failed to resolve build output directory.");
        }

        Directory.CreateDirectory(outputDirectory);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = target,
            locationPathName = fullOutputPath,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception($"Build failed: {report.summary.result}");
        }

        UnityEngine.Debug.Log($"Build succeeded: {fullOutputPath}");
    }

    static string GetArgument(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
