using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HellSlice.Editor
{
    // Explicit, one-time conversion of the existing builder output. Never runs on Play or import.
    public static class SliceSceneMigration
    {
        const string Prefabs = "Assets/HellSlice/Prefabs";
        const string Materials = "Assets/HellSlice/Materials";

        [MenuItem("Tools/A Day Through Hell/Migrate Existing Layout Once")]
        public static void Migrate()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before migration.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string backup = "LocalOnly/SceneMigration-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(backup); Directory.CreateDirectory(Prefabs); Directory.CreateDirectory(Materials);
            AssetDatabase.Refresh();
            foreach (string path in new[] { SliceBuild.First, SliceBuild.Landing })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var sessions = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<SliceSession>(true)).ToArray();
                if (sessions.Length != 1) throw new InvalidOperationException("Expected exactly one session in " + path);
                var session = sessions[0];
                if (!session.useLegacyConstruction) { Debug.Log("Preserved already authored scene: " + path); continue; }
                if (session.GetComponent<SliceWorld>() != null || scene.GetRootGameObjects().Any(o => o.GetComponentInChildren<SlicePlayer>(true) != null))
                    throw new InvalidOperationException("Existing gameplay objects found. Conversion stopped to preserve manual work: " + path);
                File.Copy(path, backup + "/" + Path.GetFileName(path));
                var world = session.gameObject.AddComponent<SliceWorld>(); world.Build(session);
                session.gameObject.AddComponent<SliceTime>();
                session.gameObject.AddComponent<SliceInteraction>();
                session.gameObject.AddComponent<SliceFeedback>();
                var source = session.gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.volume = .12f;
                var hud = session.gameObject.AddComponent<SliceHud>();
                var root = world.SceneRoot.transform; root.name = "Level";
                var groups = new Dictionary<string, Transform>();
                Transform Group(string name)
                {
                    if (!groups.TryGetValue(name, out var group))
                    { group = new GameObject(name).transform; group.SetParent(root, false); groups.Add(name, group); }
                    return group;
                }
                // Keep all unrelated scene roots intact. Only group the builder's own output.
                var children = root.Cast<Transform>().ToArray();
                foreach (var item in children)
                {
                    string group = item.GetComponent<SlicePlayer>() != null ? "Player" :
                        item.GetComponent<Camera>() != null ? "Camera" :
                        item.GetComponent<Light>() != null ? "Lighting" :
                        item.GetComponent<SliceEnemy>() != null ? "Enemies" :
                        item.GetComponent<SliceHazard>() != null ? "Hazards" :
                        item == world.ExitVolume?.transform || item.name == "Assistance gate" || item.name == "Core-locked crossing" ? "Progression" :
                        item.GetComponent<SlicePuzzleSwitch>() != null ||
                        item == world.SocketAnchor || item == world.FigureAnchor || item.gameObject == world.Piece ||
                        item.name == "Rescue lock bars" || item.name == "Shadow-only barrier collider" || item.name == "Blue shadow bars" ? "Puzzles" : "Environment";
                    item.SetParent(Group(group), true);
                }
                session.name = "Gameplay Session"; session.transform.SetParent(Group("Systems"), true);
                CreateUI(Group("UI"), hud);
                // Existing primitive art is retained under replaceable visual roots.
                GroupVisuals(world.Player.transform);
                if (session.Enemy != null) GroupVisuals(session.Enemy.transform);
                var seen = new HashSet<Material>(); int materialIndex = 0;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    foreach (var material in renderer.sharedMaterials)
                        if (material != null && !AssetDatabase.Contains(material) && seen.Add(material))
                            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(Materials + "/" + scene.name + "-" + (++materialIndex) + ".mat"));
                PrefabUtility.SaveAsPrefabAssetAndConnect(world.Player.gameObject,
                    AssetDatabase.GenerateUniqueAssetPath(Prefabs + "/" + scene.name + "-Player.prefab"), InteractionMode.AutomatedAction);
                if (session.Enemy != null) PrefabUtility.SaveAsPrefabAssetAndConnect(session.Enemy.gameObject,
                    AssetDatabase.GenerateUniqueAssetPath(Prefabs + "/Manifestation.prefab"), InteractionMode.AutomatedAction);
                var hazards = root.GetComponentsInChildren<SliceHazard>(true);
                if (hazards.Length > 0)
                {
                    // Each existing hazard remains linked; no replacement of scene identities or tuning.
                    foreach (var hazard in hazards) PrefabUtility.SaveAsPrefabAssetAndConnect(hazard.gameObject,
                        AssetDatabase.GenerateUniqueAssetPath(Prefabs + "/" + hazard.name + ".prefab"), InteractionMode.AutomatedAction);
                }
                session.useLegacyConstruction = false;
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + path);
            }
            AssetDatabase.SaveAssets();
            VerifySavedScenes();
        }

        static void GroupVisuals(Transform owner)
        {
            var parts = owner.Cast<Transform>().Where(t => t.GetComponent<MeshRenderer>() != null).ToArray();
            if (parts.Length == 0) return;
            var visual = new GameObject("Visuals - replace artwork here").transform; visual.SetParent(owner, false);
            foreach (var part in parts) part.SetParent(visual, true);
        }

        static void CreateUI(Transform parent, SliceHud hud)
        {
            var canvasObject = new GameObject("Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(parent, false);
            hud.GameplayCanvas = canvasObject.GetComponent<Canvas>(); hud.GameplayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
            Text Label(string name, Vector2 position, Vector2 size, string initial)
            {
                var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(canvasObject.transform, false);
                var rect = (RectTransform)obj.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
                var text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 22; text.color = Color.white; text.raycastTarget = false; text.text = initial;
                return text;
            }
            hud.ObjectiveText = Label("Current Objective", new Vector2(28, -24), new Vector2(1000, 65), "Current objective appears here during play.");
            hud.StatusText = Label("Health and Ability", new Vector2(28, -92), new Vector2(1000, 70), "Health: 5 / 5");
            hud.PromptText = Label("Interaction and Recovery Prompt", new Vector2(240, -590), new Vector2(1000, 110), "Contextual interaction appears here.");
        }

        [MenuItem("Tools/A Day Through Hell/Verify Saved Scene Migration")]
        public static void VerifySavedScenes()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var lines = new List<string>();
            foreach (string path in new[] { SliceBuild.First, SliceBuild.Landing })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var session = UnityEngine.Object.FindFirstObjectByType<SliceSession>();
                var world = session.GetComponent<SliceWorld>();
                if (session.useLegacyConstruction || world == null || world.Player == null || world.CameraRig == null ||
                    world.Player.Body == null || world.CameraRig.FollowPlayer != world.Player ||
                    session.GetComponent<SliceHud>().ObjectiveText == null)
                    throw new InvalidOperationException("Incomplete saved references: " + path);
                if (!session.landing && (session.Enemy == null || world.RescueSwitch == null || world.PuzzleSwitches.Length == 0 || world.ExitVolume == null || world.SocketAnchor == null || world.FigureAnchor == null))
                    throw new InvalidOperationException("Missing puzzle/progression references: " + path);
                foreach (var obj in scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Transform>(true)))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj.gameObject) > 0 || (obj.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                        throw new InvalidOperationException("Hidden object or missing script: " + obj.name);
                if (!PrefabUtility.IsPartOfPrefabInstance(world.Player)) throw new InvalidOperationException("Player prefab link missing.");
                int count = world.SceneRoot.GetComponentsInChildren<Transform>(true).Length;
                // Exercise a real serialized Inspector property through save + reload, then restore it.
                float original = world.CameraRig.FollowDistance;
                try
                {
                    world.CameraRig.FollowDistance = original + .125f;
                    EditorUtility.SetDirty(world.CameraRig); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
                    scene = EditorSceneManager.OpenScene(path);
                    world = UnityEngine.Object.FindFirstObjectByType<SliceWorld>();
                    if (world.CameraRig.FollowDistance != original + .125f || world.SceneRoot.GetComponentsInChildren<Transform>(true).Length != count)
                        throw new InvalidOperationException("Save/reopen did not preserve scene data.");
                }
                finally
                {
                    world.CameraRig.FollowDistance = original; EditorUtility.SetDirty(world.CameraRig);
                    EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
                }
                lines.Add("PASS: " + path + " / " + count + " authored objects; references, visibility, prefab link, property edit/save/reopen verified.");
            }
            Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/editability-scenes.txt", lines);
            EditorSceneManager.OpenScene(SliceBuild.First);
            Selection.activeGameObject = UnityEngine.Object.FindFirstObjectByType<SlicePlayer>().gameObject;
            Debug.Log("Saved-scene migration verified. " + string.Join("\n", lines));
        }
    }
}
