using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Review helper. Inactive unless the player is launched with: -polish-capture "C:\path\to\folder"
    /// It starts the stage, photographs fixed viewpoints for before/after comparison, then quits.
    /// Add -polish-scene <SceneName> to capture another stage (default: the first scene).
    /// </summary>
    public sealed class PolishCapture : MonoBehaviour
    {
        string folder, scene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-polish-capture" && FindFirstObjectByType<PolishCapture>() == null)
                {
                    var go = new GameObject("Polish capture"); DontDestroyOnLoad(go);
                    var capture = go.AddComponent<PolishCapture>(); capture.folder = args[i + 1];
                    for (int j = 0; j < args.Length - 1; j++) if (args[j] == "-polish-scene") capture.scene = args[j + 1];
                }
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            StartCoroutine(HoldEnemies());
            if (!string.IsNullOrEmpty(scene)) yield return SceneManager.LoadSceneAsync(scene);
            yield return new WaitForSecondsRealtime(1.5f);
            var s = SliceSession.Current;
            if (s == null) { Application.Quit(); yield break; }
            s.Begin();
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Shot("1-gameplay-view");

            // Look the camera up directly: Stage 1's session reference can be empty when the capture runs.
            var rig = s.CameraRig != null ? s.CameraRig : FindFirstObjectByType<SliceCamera>();
            if (rig != null) rig.enabled = false;
            var cam = rig != null ? rig.transform : Camera.main.transform;
            yield return View(cam, new Vector3(1.4f, 1.55f, 2.3f), new Vector3(0, 1.0f, 0), "2-protagonist");
            yield return View(cam, new Vector3(2.2f, 2.0f, 4.6f), new Vector3(0, 1.3f, 8), "3-enemy");
            yield return View(cam, new Vector3(-6, 7.5f, -4.5f), new Vector3(0, 0, 20), "4-wide-vista");
            // Wait until a flame row is burning for the crossing shot.
            float wait = 0;
            while (wait < 8 && !AnyBurning()) { wait += Time.unscaledDeltaTime; yield return null; }
            yield return new WaitForSecondsRealtime(.35f);
            yield return View(cam, new Vector3(0, 6.2f, 12.5f), new Vector3(0, .6f, 21.5f), "5-flame-crossing");
            yield return View(cam, new Vector3(-6.5f, 2.2f, 26), new Vector3(-14, -1.5f, 30), "6-lava-edge");
            // Close eruption view from the lane entrance, timed just after ignition.
            wait = 0;
            while (wait < 8 && AnyBurning()) { wait += Time.unscaledDeltaTime; yield return null; }
            wait = 0;
            while (wait < 8 && !AnyBurning()) { wait += Time.unscaledDeltaTime; yield return null; }
            yield return new WaitForSecondsRealtime(.25f);
            yield return View(cam, new Vector3(1.9f, 1.2f, 15.6f), new Vector3(0, 4.2f, 20.5f), "7-eruption-close");
            // Stage 1 cage, when present: closed close-up, wide with its support, then the door lift preview.
            var cage = FindFirstObjectByType<PolishCageRescue>();
            if (cage != null)
            {
                Vector3 c = cage.transform.position;
                yield return View(cam, c + new Vector3(1.6f, 1.7f, -4.2f), c + new Vector3(0, 1.2f, 0), "9a-cage-closed");
                yield return View(cam, c + new Vector3(5.5f, 4.2f, -7.5f), c + new Vector3(-.8f, 3.2f, 0), "9b-cage-wide");
                cage.PreviewRelease();
                yield return new WaitForSecondsRealtime(2f);
                yield return View(cam, c + new Vector3(1.6f, 1.7f, -4.2f), c + new Vector3(0, 1.2f, 0), "9c-cage-open");
            }
            // Settings screen over the paused game.
            if (rig != null) rig.enabled = true;
            s.SetPaused(true);
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot("8a-paused-with-settings-button");
            PolishSettings.Open();
            yield return new WaitForSecondsRealtime(.8f);
            yield return Shot("8-settings");
            Application.Quit();
        }

        // Every frame, from the first one: the Stage 1 guardian would otherwise catch the standing player and reload the stage.
        static IEnumerator HoldEnemies()
        {
            while (true)
            {
                foreach (var e in FindObjectsByType<SliceEnemy>(FindObjectsSortMode.None)) { e.StopChase(); e.enabled = false; }
                yield return null;
            }
        }

        static bool AnyBurning()
        {
            foreach (var h in FindObjectsByType<SliceHazard>(FindObjectsSortMode.None)) if (h.Burning) return true;
            return false;
        }

        IEnumerator View(Transform cam, Vector3 position, Vector3 target, string name)
        {
            cam.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot(name);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
            yield return new WaitForSecondsRealtime(.3f);
        }
    }
}
