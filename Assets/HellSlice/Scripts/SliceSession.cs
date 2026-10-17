using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HellSlice
{
    [DefaultExecutionOrder(-100)]
    public sealed class SliceSession : MonoBehaviour
    {
        public static SliceSession Current { get; private set; }
        public static bool InstructionsSeen { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetPlaySession() { InstructionsSeen = false; Current = null; }
        public bool landing;
        [Tooltip("Legacy bootstrap only. Saved gameplay scenes keep this OFF.")]
        public bool useLegacyConstruction = true;
        public bool StartImmediately;
        [Min(1)] public int StageNumber = 1;
        [Tooltip("0 keeps slow time locked until its placement is agreed. Set the intended first available stage in each scene.")]
        [Min(0)] public int SlowTimeUnlockStage;
        public bool SlowTimeAvailable => SlowTimeUnlockStage > 0 && StageNumber >= SlowTimeUnlockStage;
        public int EarlierCompletedStages => SliceStageProgress.CountBefore(StageNumber);
        public SlicePlayer Player { get; private set; }
        public SliceCamera CameraRig { get; private set; }
        public SliceTime Clock { get; private set; }
        public SliceInput Input { get; private set; }
        public SliceInteraction Interaction { get; private set; }
        public SliceWorld World { get; private set; }
        public SliceFeedback Feedback { get; private set; }
        public SlicePossessionStage Possession { get; private set; }
        public SliceFeedingFlameStage FeedingFlame { get; private set; }
        public SliceShadowTruthStage ShadowTruth { get; private set; }
        public SliceBorrowedTimeStage BorrowedTime { get; private set; }
        public SlicePrideStage Pride { get; private set; }
        public SliceChaseStage Chase { get; private set; }
        public SliceReturnStage TheReturn { get; private set; }
        public bool StageCompleted { get; private set; }
        [field: SerializeField] public SliceEnemy Enemy { get; set; }
        public bool Started { get; private set; }
        public bool Paused { get; private set; }
        public bool Dead { get; private set; }
        public bool Transitioning { get; private set; }
        public bool PiecePlaced { get; private set; }
        public bool SwitchLatched { get; private set; }
        public bool Assisted { get; private set; }
        public bool Running => Started && !Paused && !Dead && !Transitioning;
        public bool GateOpen => PiecePlaced && SwitchLatched && Assisted;
        public string Message { get; private set; }
        float messageTime;
        public string Objective => TheReturn != null ? TheReturn.Objective : Chase != null && !Chase.ChaseComplete ? Chase.Objective : Pride != null ? Pride.Objective : BorrowedTime != null ? BorrowedTime.Objective : ShadowTruth != null ? ShadowTruth.Objective : FeedingFlame != null ? FeedingFlame.Objective : Possession != null ? Possession.Objective : landing ? "A FIRST STEP / The next chamber awaits" :
            StageNumber != 1 && Enemy != null && !Enemy.Defeated ? "01 / Face the manifestation. Move clear of its red warning." :
            !PiecePlaced ? (StageNumber == 1 ? "Run forward. Stay ahead of the guardian." : "02 / Carry the released ember core to the socket.") :
            !SwitchLatched ? (StageNumber == 1 ? "Cross the flame lane. Release the rescue lock." : "03 / Cross the flame lane. Operate the rescue mechanisms.") :
            !Assisted ? "04 / Help the trapped figure." : "05 / The way opens through assistance. Enter the light.";

        void Awake()
        {
            Current = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = 32;
            Input = new SliceInput();
            Clock = GetComponent<SliceTime>();
            Feedback = GetComponent<SliceFeedback>();
            World = GetComponent<SliceWorld>();
            Interaction = GetComponent<SliceInteraction>();
            Possession = GetComponent<SlicePossessionStage>();
            FeedingFlame = GetComponent<SliceFeedingFlameStage>();
            ShadowTruth = GetComponent<SliceShadowTruthStage>();
            BorrowedTime = GetComponent<SliceBorrowedTimeStage>();
            Pride = GetComponent<SlicePrideStage>();
            Chase = GetComponent<SliceChaseStage>();
            TheReturn = GetComponent<SliceReturnStage>();
            if (useLegacyConstruction)
            {
                if (Clock == null) Clock = gameObject.AddComponent<SliceTime>();
                if (Feedback == null) Feedback = gameObject.AddComponent<SliceFeedback>();
                if (World == null) World = gameObject.AddComponent<SliceWorld>();
                World.Build(this);
                if (Interaction == null) Interaction = gameObject.AddComponent<SliceInteraction>();
                if (GetComponent<SliceHud>() == null) gameObject.AddComponent<SliceHud>();
            }
            if (World == null || World.Player == null || World.CameraRig == null || Clock == null || Feedback == null || Interaction == null)
            {
                Debug.LogError("Saved level is missing gameplay references. Repair them in the Inspector; the scene will not be rebuilt.", this);
                enabled = false; return;
            }
            Player = World.Player;
            CameraRig = World.CameraRig;
            World.InitializeRuntime();
            if (landing) PiecePlaced = SwitchLatched = Assisted = true;
            Player.SetReveal(EarlierCompletedStages / 6f, true);
            UnlockCursor();
            if (StartImmediately || InstructionsSeen) Begin();
        }
        void Update()
        {
            if (Input.Pause && !Dead && !Transitioning) Polish.PolishSettings.Toggle();
            if (!Running) return;
            if (!SlowTimeAvailable) Clock.ResetState();
            Clock.Tick(Time.unscaledDeltaTime);
            if (messageTime > 0) messageTime -= Time.unscaledDeltaTime;
            if (Input.Slow && SlowTimeAvailable && Clock.Activate()) { Notify("TIME BENDS / Move while the world hesitates."); Feedback.Cue(300); }
            if (Possession != null) { Possession.Tick(this); return; }
            if (FeedingFlame != null) { FeedingFlame.Tick(this); return; }
            if (ShadowTruth != null) { ShadowTruth.Tick(this); return; }
            if (BorrowedTime != null) { BorrowedTime.Tick(this); return; }
            if (Pride != null) { Pride.Tick(this); return; }
            if (Chase != null && !Chase.ChaseComplete) { Chase.Tick(this); return; }
            if (TheReturn != null) { TheReturn.Tick(this); return; }
            if (landing) return;
            if (World.RescueSwitch != null)
            {
                if (!SwitchLatched && World.RescueSwitch.Completed)
                {
                    SwitchLatched = true; Feedback.Cue(660);
                    Notify("The rescue lock is released. Help the waiting figure.");
                }
            }
            World.UpdateMechanisms(this);
            if (GateOpen && World.ExitContains(Player.transform.position))
            {
                CompleteStage();
            }
        }
        public void CompleteStage()
        {
            if (!Running || StageCompleted) return;
            StageCompleted = true;
            SliceStageProgress.Complete(StageNumber);
            Clock.ResetState();
            // An unfinished next chamber is a completion state, never an invalid scene load.
            if (string.IsNullOrWhiteSpace(World.NextScene) || !Application.CanStreamedLevelBeLoaded(World.NextScene)) return;
            Transitioning = true; UnlockCursor();
            StartCoroutine(LoadStage(World.NextScene));
        }
        public void Begin() { InstructionsSeen = true; Started = true; SetPaused(false); }
        public void SetPaused(bool value)
        {
            Paused = value;
            if (value) UnlockCursor(); else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
        public void PlacePiece() { PiecePlaced = true; Feedback.Cue(470); Notify("Crossing unlocked. Wait for the flames to cool."); }
        public void UnlockRescueProgression()
        {
            if (StageNumber != 1 || Chase == null || !Chase.DoorClosed) return;
            Player.Carrying = false;
            PiecePlaced = true;
        }
        public bool Assist()
        {
            if (!PiecePlaced || !SwitchLatched || Assisted) return false;
            Assisted = true; Player.SetReveal((EarlierCompletedStages + 1) / 6f, false); Feedback.Cue(880);
            Notify("You stopped to help. A part of yourself becomes visible.", 7f); return true;
        }
        public void KillPlayer()
        {
            if (!Running || Transitioning) return;
            Dead = true;
            // The death screen offers the restart; automated tests (and a missing screen) reload at once.
            if (SliceSmokeDriver.IsRunning || !Polish.PolishDeathScreen.Show(this)) Restart();
        }
        public void Restart()
        {
            if (Transitioning) return;
            Transitioning = true;
            Clock.ResetState(); UnlockCursor();
            // The scene asset is the stage's initial state. Completed earlier stages live separately.
            string stage = gameObject.scene.path;
            if (string.IsNullOrEmpty(stage)) stage = gameObject.scene.name;
            StartCoroutine(LoadStage(stage));
        }
        IEnumerator LoadStage(string stage)
        {
            // Let this frame finish; every death source sees Transitioning and cannot queue another load.
            yield return null;
            yield return SceneManager.LoadSceneAsync(stage, LoadSceneMode.Single);
        }
        public void Notify(string text, float seconds = 4f) { Message = text; messageTime = seconds; }
        public string CurrentMessage => messageTime > 0 ? Message : "";
        void OnApplicationFocus(bool focus)
        {
            // Automated input tests run without Game-view focus. Normal play still pauses on focus loss.
            if (!focus && Started && !Dead && !SliceSmokeDriver.IsRunning) SetPaused(true);
        }
        void OnDestroy() { Input?.Dispose(); if (Current == this) Current = null; UnlockCursor(); }
        static void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}

