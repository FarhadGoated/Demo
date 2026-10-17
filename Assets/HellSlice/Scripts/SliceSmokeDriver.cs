using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace HellSlice
{
    // Explicit test entry only: editor menu or standalone -sliceSmoke. Not used in ordinary play.
    public sealed class SliceSmokeDriver : MonoBehaviour
    {
        public static bool IsRunning { get; private set; }
        readonly List<string> results = new List<string>();
        bool failed;
        bool finished, cleaned;
        Keyboard testKeyboard;
        Mouse testMouse;
        bool previousRunInBackground;
        InputSettings previousInputSettings, temporaryInputSettings;
        float startedAt;
        IEnumerator Hold(Key key, float seconds)
        {
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(key));
            yield return null;
            yield return new WaitForSecondsRealtime(seconds);
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
            yield return null;
        }
        void Update()
        {
            if (startedAt > 0 && Time.realtimeSinceStartup - startedAt > 90)
            { Check(false, "Test run timed out"); Finish(); }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StandaloneChecks()
        {
            if (!Application.isEditor && Array.IndexOf(Environment.GetCommandLineArgs(), "-sliceSmoke") >= 0 && FindAnyObjectByType<SliceSmokeDriver>() == null)
                new GameObject("Standalone smoke checks").AddComponent<SliceSmokeDriver>();
        }
        void Check(bool value, string name)
        {
            results.Add((value ? "PASS: " : "FAIL: ") + name); failed |= !value;
            if (!value) Debug.LogError("Smoke check failed: " + name +
                " [focused=" + Application.isFocused + ", running=" + SliceSession.Current?.Running +
                ", keyboardEnabled=" + testKeyboard?.enabled + "]");
        }
        void Warp(SlicePlayer p, Vector3 position)
        {
            p.Body.enabled = false; p.transform.position = position; p.Body.enabled = true; Physics.SyncTransforms();
        }
        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            IsRunning = true;
            startedAt = Time.realtimeSinceStartup;
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            // The default editor routing sends keyboard/mouse events to the editor when
            // Game view lacks focus. Clone settings for this test; never modify the user's asset.
            previousInputSettings = InputSystem.settings;
            temporaryInputSettings = Instantiate(previousInputSettings);
            temporaryInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            temporaryInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = temporaryInputSettings;
            testKeyboard = InputSystem.AddDevice<Keyboard>("Slice smoke keyboard");
            testMouse = InputSystem.AddDevice<Mouse>("Slice smoke mouse");
            yield return null; yield return null;
            var s = SliceSession.Current;
            Check(s != null && !s.landing, "FirstPlayable initializes");
            if (s == null) { Finish(); yield break; }
            Check(!s.useLegacyConstruction && FindObjectsByType<SlicePlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 1 &&
                FindObjectsByType<SliceWorld>(FindObjectsSortMode.None).Length == 1 && s.World.SceneRoot.name == "Level", "Saved scene used without duplicate player or world");
            Check(s.GetComponent<SliceHud>().GameplayCanvas != null && s.CameraRig.FollowPlayer == s.Player, "Authored HUD and camera references resolve");
            s.Begin();
            Warp(s.Player, new Vector3(-4, 0, 0));
            float initialZ = s.Player.transform.position.z;
            yield return Hold(Key.W, .35f);
            float normalTravel = s.Player.transform.position.z - initialZ;
            Check(normalTravel > .7f, "W input moves the body through Input Actions");
            yield return Hold(Key.Q, .01f);
            Check(!s.SlowTimeAvailable && s.Clock.Remaining == 0, "Slow time begins locked");
            s.SlowTimeUnlockStage = s.StageNumber;
            yield return Hold(Key.Q, .01f);
            Check(s.Clock.Remaining > 0, "Q input activates slow time at configured stage");
            initialZ = s.Player.transform.position.z;
            yield return Hold(Key.W, .35f);
            float slowedTravel = s.Player.transform.position.z - initialZ;
            Check(slowedTravel > normalTravel * .65f && slowedTravel < normalTravel * 1.5f, "Player movement stays responsive during slow time");
            float groundY = s.Player.transform.position.y;
            yield return Hold(Key.Space, .1f);
            Check(s.Player.transform.position.y > groundY + .15f && !s.Player.Dodging, "Space input starts a grounded jump instead of dodge");
            yield return new WaitForSecondsRealtime(.8f);
            s.Clock.ResetState();
            var enemyController = s.Enemy.GetComponent<CharacterController>();
            enemyController.enabled = false; s.Enemy.transform.position = new Vector3(0, 0, 8); enemyController.enabled = true;
            Warp(s.Player, new Vector3(0, 0, 6)); s.Player.transform.forward = Vector3.forward;
            InputSystem.QueueStateEvent(testMouse, new MouseState().WithButton(MouseButton.Left)); yield return null; yield return null;
            InputSystem.QueueStateEvent(testMouse, new MouseState()); yield return null;
            Check(s.Enemy.Health == 3, "Mouse attack hits an enemy in reach and facing cone");
            s.Begin(); s.SetPaused(true);
            Check(s.Player.Health == 5 && !s.GateOpen, "Fresh health and gate state");
            Check(Resources.Load<TextAsset>("SliceControls") != null && Resources.Load<Material>("SliceBase") != null, "Input and shader material are included");
            var clock = s.Clock;
            Check(clock.Activate() && !clock.Activate(), "Slow time prevents stacking");
            clock.Tick(2); Check(Mathf.Approximately(clock.Remaining, 3) && clock.Factor == .25f, "Slow duration and factor");
            clock.Tick(3); Check(clock.Remaining == 0 && clock.Recharge == 8 && !clock.Activate(), "Expiry begins recharge");
            clock.Tick(8); Check(clock.Recharge == 0 && clock.Activate(), "Recharge completes");
            clock.ResetState(); Check(clock.Factor == 1 && clock.Recharge == 0, "Reset clears slow-time state");
            Check(!SliceHazard.IsBurningAt(.5f) && SliceHazard.IsBurningAt(1.5f) && !SliceHazard.IsBurningAt(3.5f), "Flame telegraph / burn / cooling phases");
            float phase = FindAnyObjectByType<SliceHazard>().Elapsed;
            yield return new WaitForSecondsRealtime(.15f);
            Check(phase == FindAnyObjectByType<SliceHazard>().Elapsed && clock.WorldDelta == 0, "Pause freezes world timers");
            Check(!s.Assist() && !s.GateOpen, "Assistance cannot bypass puzzle prerequisites");
            for (int i = 0; i < 4; i++) s.Enemy.Hit();
            Check(s.Enemy.Defeated && !s.GateOpen && !s.Assisted, "Enemy defeat alone cannot open narrative gate");
            s.SetPaused(false); Vector3 coreStart = s.World.Piece.transform.position;
            Warp(s.Player, coreStart + Vector3.back);
            yield return Hold(Key.E, .01f);
            Check(s.Player.Carrying, "E input picks up released core");
            yield return Hold(Key.E, .01f);
            Check(!s.Player.Carrying && Vector3.Distance(s.World.Piece.transform.position, coreStart) < .1f, "E safely returns core to pickup location");
            yield return Hold(Key.E, .01f);
            Warp(s.Player, s.World.SocketPosition + Vector3.back * .4f); yield return Hold(Key.E, .01f);
            s.SetPaused(true); s.World.UpdateMechanisms(s); Physics.SyncTransforms();
            Check(s.PiecePlaced && !s.GateOpen, "Placed core opens crossing but not exit");
            s.SetPaused(false);
            var firstMechanism = s.World.PuzzleSwitches[0];
            Warp(s.Player, s.World.RescueSwitch.transform.position + Vector3.back * .3f);
            Check(!s.World.RescueSwitch.Interact(s.Player), "Rescue lock requires first mechanism");
            Warp(s.Player, firstMechanism.transform.position + Vector3.back * .3f);
            yield return Hold(Key.E, .01f);
            Check(firstMechanism.Completed && !firstMechanism.OpensGate.activeSelf, "Body interaction opens the route gate");
            Warp(s.Player, s.World.RescueSwitch.transform.position + Vector3.back * .3f);
            yield return Hold(Key.E, .01f);
            Check(s.SwitchLatched, "Body interaction releases rescue lock");
            Check(s.Assist() && s.GateOpen && !s.Assist(), "Assistance opens gate exactly once after prerequisites");
            s.World.UpdateMechanisms(s); Warp(s.Player, new Vector3(0, 0, 45));
            yield return new WaitUntil(() => SliceSession.Current != s && SliceSession.Current != null);
            Check(SceneManager.GetActiveScene().name == "WeightOfPossession" && SliceSession.Current.Possession != null, "Exit loads separate Stage 2 scene");
            s = SliceSession.Current;
            int completed = s.EarlierCompletedStages;
            s.Restart();
            yield return new WaitUntil(() => SliceSession.Current != s && SliceSession.Current != null);
            Check(SceneManager.GetActiveScene().name == "WeightOfPossession" && SliceSession.Current.EarlierCompletedStages == completed, "Restart keeps current stage and earlier progress");
            yield return SceneManager.LoadSceneAsync("FirstPlayable"); yield return null;
            s = SliceSession.Current; s.Begin();
            s.PlacePiece(); s.Player.Carrying = true;
            s.KillPlayer(); s.KillPlayer();
            Check(s.Dead && s.Transitioning && !s.Running && s.Clock.Factor == 1, "Death queues one stage reload and stops simulation");
            yield return new WaitUntil(() => SliceSession.Current != s && SliceSession.Current != null);
            s = SliceSession.Current;
            Check(!s.landing && !s.PiecePlaced && !s.SwitchLatched && !s.Assisted && !s.Player.Carrying && s.Enemy.Health == 4 && s.Player.Health == 5,
                "Death restores current stage from authored initial state");
            Check(!s.SlowTimeAvailable && s.Clock.Remaining == 0, "Restart restores authored slow-time lock");
            Finish();
        }
        void Finish()
        {
            if (finished) return;
            finished = true;
            startedAt = -1;
            StopAllCoroutines();
            Cleanup();
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/phase-1-smoke-" + (Application.isEditor ? "editor" : "standalone") + ".txt", results);
            Debug.Log("Slice smoke checks: " + (failed ? "FAILED" : "PASSED") + " (" + results.Count + ")");
#if UNITY_EDITOR
            if (Application.isEditor) { StartCoroutine(ReturnToPlayable()); return; }
#endif
            Application.Quit(failed ? 1 : 0);
        }
        IEnumerator ReturnToPlayable()
        {
            yield return SceneManager.LoadSceneAsync("FirstPlayable");
            yield return null;
            var session = SliceSession.Current;
            if (session != null) { session.Begin(); session.SetPaused(true); }
            Debug.Log("Smoke checks finished. The game is ready: click Resume. For ordinary play, use Unity's Play button.");
            Destroy(gameObject);
        }
        void Cleanup()
        {
            if (cleaned) return;
            cleaned = true;
            IsRunning = false;
            if (testKeyboard != null && testKeyboard.added) InputSystem.RemoveDevice(testKeyboard);
            if (testMouse != null && testMouse.added) InputSystem.RemoveDevice(testMouse);
            if (previousInputSettings != null) InputSystem.settings = previousInputSettings;
            if (temporaryInputSettings != null) Destroy(temporaryInputSettings);
            Application.runInBackground = previousRunInBackground;
        }
        void OnDestroy() { Cleanup(); }
    }
}

