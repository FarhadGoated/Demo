using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HellSlice.Editor
{
    public static class SliceBuild
    {
        public const string First = "Assets/HellSlice/Scenes/FirstPlayable.unity";
        public const string Landing = "Assets/HellSlice/Scenes/NextLanding.unity";
        public const string Possession = "Assets/HellSlice/Scenes/WeightOfPossession.unity";
        public const string FeedingFlame = "Assets/HellSlice/Scenes/FeedingFlame.unity";
        public const string ShadowOfTruth = "Assets/HellSlice/Scenes/ShadowOfTruth.unity";
        public const string Pride = "Assets/HellSlice/Scenes/WeightOfPride.unity";
        public const string BorrowedTime = "Assets/HellSlice/Scenes/BorrowedTime.unity";
        public const string TheReturn = "Assets/HellSlice/Scenes/TheReturn.unity";

        [MenuItem("Tools/A Day Through Hell/Open First Playable")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(First);
        }
        [MenuItem("Tools/A Day Through Hell/Build Windows")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building.");
            Directory.CreateDirectory("Builds/Windows"); Directory.CreateDirectory("Logs");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { First, Possession, FeedingFlame, ShadowOfTruth, Pride, TheReturn, Landing },
                locationPathName = "Builds/Windows/A day through Hell.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            string result = $"{report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\n";
            File.WriteAllText("Logs/phase-1-build.txt", result);
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + result);
            Debug.Log("Windows build complete: " + Path.GetFullPath(options.locationPathName));
        }
        [MenuItem("Tools/A Day Through Hell/Run Smoke Checks")]
        public static void RunSmokeChecks()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before starting smoke checks.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(First);
            SessionState.SetBool("HellSlice.RunSmoke", true);
            EditorApplication.isPlaying = true;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartChecks()
        {
            if (!SessionState.GetBool("HellSlice.RunSmoke", false)) return;
            SessionState.SetBool("HellSlice.RunSmoke", false);
            new GameObject("Phase 1 automated verification").AddComponent<SliceSmokeDriver>();
        }
    }
}
