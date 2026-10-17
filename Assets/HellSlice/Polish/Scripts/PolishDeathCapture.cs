using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HellSlice.Polish
{
    /// <summary>
    /// Review helper for the death screen. Inactive unless the player is launched with:
    /// -polish-death-capture "C:\path\to\folder" [-polish-scene SceneName]
    /// It kills the player through the real KillPlayer path, records the reveal frame by frame (frames/), takes
    /// stills of each state and death cause, chooses Rise Again, photographs the reloaded chamber, then quits.
    /// </summary>
    public sealed class PolishDeathCapture : MonoBehaviour
    {
        string folder, scene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            string folder = null, scene = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-polish-death-capture") folder = args[i + 1];
                if (args[i] == "-polish-scene") scene = args[i + 1];
            }
            if (folder == null || FindFirstObjectByType<PolishDeathCapture>() != null) return;
            var go = new GameObject("Polish death capture"); DontDestroyOnLoad(go);
            var c = go.AddComponent<PolishDeathCapture>(); c.folder = folder; c.scene = scene;
        }

        IEnumerator Start()
        {
            string frames = Path.Combine(folder, "frames");
            Directory.CreateDirectory(frames);
            if (!string.IsNullOrEmpty(scene)) { SceneManager.LoadScene(scene); yield return new WaitForSecondsRealtime(1f); }
            yield return new WaitForSecondsRealtime(1.5f);
            var s = SliceSession.Current;
            if (s == null) { Application.Quit(); yield break; }
            StartCoroutine(HoldEnemies());
            yield return null;
            s.Begin();
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Shot("d0-alive");

            s.KillPlayer();
            var ds = PolishDeathScreen.Instance;
            if (ds == null) { Debug.LogError("[DeathCapture] death screen did not open"); Application.Quit(); yield break; }
            // The reveal at 20 fps for the video.
            for (int i = 0; i <= 130; i++)
            {
                ds.ClockOverride = i / 20f;
                yield return Shot(Path.Combine("frames", "f" + i.ToString("000")), false);
            }
            foreach (var t in new[] { .2f, 1.0f, 1.8f, 2.6f, 3.4f, 6.5f })
            {
                ds.ClockOverride = t;
                yield return Shot("d1-reveal-" + t.ToString("0.0") + "s");
            }
            ds.PreviewFocus(1);
            yield return Shot("d2-leave-focused");
            ds.PreviewChoose(); // arms the quit confirmation only
            yield return Shot("d3-leave-confirm");

            ds.ClockOverride = 6.5f;
            foreach (var cause in new[] { PolishDeathScreen.Cause.Fire, PolishDeathScreen.Cause.Fall, PolishDeathScreen.Cause.Wounds })
            {
                PolishDeathScreen.Show(s, cause);
                yield return Shot("d4-cause-" + cause.ToString().ToLowerInvariant());
            }

            // Rise Again: fade to black, reload, fade back in.
            ds.ClockOverride = -1;
            ds.PreviewFocus(0); ds.PreviewChoose();
            yield return new WaitForSecondsRealtime(.45f);
            yield return Shot("d5-rising");
            yield return new WaitForSecondsRealtime(3f);
            yield return Shot("d6-risen");
            Debug.Log("[DeathCapture] done, death screen open: " + PolishDeathScreen.IsOpen);
            Application.Quit();
        }

        // The guardian would otherwise catch the standing player before the scripted death.
        IEnumerator HoldEnemies()
        {
            while (true)
            {
                foreach (var e in FindObjectsByType<SliceEnemy>(FindObjectsSortMode.None)) if (e.enabled) { e.StopChase(); e.enabled = false; }
                yield return null;
            }
        }

        IEnumerator Shot(string name, bool settle = true)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
            if (settle) yield return new WaitForSecondsRealtime(.3f);
            else yield return null;
        }
    }
}
