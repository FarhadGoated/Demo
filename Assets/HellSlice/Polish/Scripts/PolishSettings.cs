using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HellSlice.Polish
{
    /// <summary>
    /// Settings screen in the approved "Cinematic Minimal" style (design 5), with only the options that
    /// matter for this game: Volume, Mouse Sensitivity, Invert Look, Fullscreen, Graphics Quality, Brightness.
    /// Values persist in PlayerPrefs and are re-applied on every scene load.
    /// Mouse: hover a row, click arrows or bar segments. Keyboard: W/S select, A/D adjust, Enter act, R reset, Esc resume.
    /// </summary>
    public sealed class PolishSettings : MonoBehaviour
    {
        public static PolishSettings Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance.root != null && Instance.root.gameObject.activeSelf;

        // ---- saved values (steps keep the segmented bars exact) ----
        int volumeStep, sensitivityStep, brightnessStep, quality; bool invert, fullscreen;
        const string P = "hellslice.";
        static readonly Vector2 BaseSensitivity = new Vector2(.12f, .10f);
        float Volume => volumeStep / 10f;
        float Sensitivity => sensitivityStep * .2f;          // 0.2x .. 2.0x, default 1.0x
        float Brightness => (brightnessStep - 5) / 5f;        // -1 .. +1, default 0

        sealed class Row
        {
            public int Section; public RectTransform Rt; public Image Highlight, Edge; public Action<int> Step; public Action Refresh, Activate;
            public float Glow;
        }
        readonly List<Row> rows = new List<Row>();
        readonly List<(Text num, Text label, Image bar, RectTransform holder)> tabs = new List<(Text, Text, Image, RectTransform)>();
        readonly Dictionary<Volume, float> baseExposure = new Dictionary<Volume, float>();
        Canvas canvas; CanvasScaler scaler; RectTransform root, content; CanvasGroup group; int focus;
        static readonly string[] Sections = { "AUDIO", "CONTROLS", "DISPLAY", "CHAMBER" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("Polish settings"); DontDestroyOnLoad(go);
            Instance = go.AddComponent<PolishSettings>();
        }
        public static void Open()
        {
            var session = SliceSession.Current;
            if (session == null || session.Dead || session.Transitioning) return;
            if (Instance == null) Boot();
            session.SetPaused(true);
            Instance.Show(true);
        }
        public static void Close() { if (Instance != null) Instance.Back(); }
        // SliceSession owns Escape input so one press cannot both open and close this screen.
        public static void Toggle() { if (IsOpen) Close(); else Open(); }
        public static void Respawn()
        {
            var session = SliceSession.Current;
            if (session == null || session.Dead || session.Transitioning) return;
            if (Instance != null) Instance.Show(false);
            session.Restart();
        }

        void Awake()
        {
            Load();
            QualitySettings.SetQualityLevel(Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1), true);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsOpen) Show(false);
            ApplySceneSettings();
        }
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            if (Instance == this) Instance = null;
        }

        // ================= persistence & application =================
        void Load()
        {
            volumeStep = PlayerPrefs.GetInt(P + "volumeStep", 8);
            sensitivityStep = PlayerPrefs.GetInt(P + "sensitivityStep", 5);
            brightnessStep = PlayerPrefs.GetInt(P + "brightnessStep", 5);
            invert = PlayerPrefs.GetInt(P + "invert", 0) == 1;
            fullscreen = PlayerPrefs.GetInt(P + "fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            quality = PlayerPrefs.GetInt(P + "quality", QualitySettings.GetQualityLevel());
        }
        void Save()
        {
            PlayerPrefs.SetInt(P + "volumeStep", volumeStep); PlayerPrefs.SetInt(P + "sensitivityStep", sensitivityStep);
            PlayerPrefs.SetInt(P + "brightnessStep", brightnessStep); PlayerPrefs.SetInt(P + "invert", invert ? 1 : 0);
            PlayerPrefs.SetInt(P + "fullscreen", fullscreen ? 1 : 0); PlayerPrefs.SetInt(P + "quality", quality);
            PlayerPrefs.Save();
        }
        void ResetDefaults()
        {
            volumeStep = 8; sensitivityStep = 5; brightnessStep = 5; invert = false;
            quality = QualitySettings.names.Length - 1; QualitySettings.SetQualityLevel(quality, true);
            ApplySceneSettings(); Save(); RefreshAll(); Cue(330);
        }
        /// <summary>Camera, volume and listener are per scene, so re-apply after every load.</summary>
        void ApplySceneSettings()
        {
            AudioListener.volume = Volume;
            foreach (var dead in new List<Volume>(baseExposure.Keys)) if (dead == null) baseExposure.Remove(dead); // drop unloaded scenes' volumes
            foreach (var cam in FindObjectsByType<SliceCamera>(FindObjectsSortMode.None))
                cam.Sensitivity = new Vector2(BaseSensitivity.x * Sensitivity, BaseSensitivity.y * Sensitivity * (invert ? -1 : 1));
            foreach (var v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (!v.isGlobal || v.sharedProfile == null || !v.sharedProfile.Has<ColorAdjustments>()) continue;
                if (!baseExposure.ContainsKey(v)) { v.sharedProfile.TryGet<ColorAdjustments>(out var shared); baseExposure[v] = shared.postExposure.value; }
                if (v.profile.TryGet<ColorAdjustments>(out var ca)) ca.postExposure.Override(baseExposure[v] + Brightness);
            }
        }
        void ApplyFullscreen()
        {
            if (fullscreen) { var r = Screen.currentResolution; Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow); }
            else Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
        static string QualityLabel(int level)
        {
            string n = QualitySettings.names[Mathf.Clamp(level, 0, QualitySettings.names.Length - 1)].ToLowerInvariant();
            return n.Contains("mobile") || n.Contains("low") ? "PERFORMANCE" : n.Contains("pc") || n.Contains("high") ? "HIGH" : n.ToUpperInvariant();
        }
        static void Cue(int hz) => SliceSession.Current?.Feedback?.Cue(hz, .04f);

        // ================= open / close / input =================
        void Show(bool open)
        {
            if (root == null) Build();
            if (open) { fullscreen = Screen.fullScreenMode != FullScreenMode.Windowed; RefreshAll(); SetFocus(0); group.alpha = 0; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            else Save();
            root.gameObject.SetActive(open);
        }

        void Update()
        {
            UpdateLauncher();
            if (!IsOpen) return;
            float dt = Time.unscaledDeltaTime;
            // Narrower than 16:9 (4:3, 16:10 laptops): fit width; wider (ultrawide): fit height. Nothing gets cut off.
            scaler.matchWidthOrHeight = (float)Screen.width / Mathf.Max(1, Screen.height) < 1.77f ? 0 : 1;
            group.alpha = Mathf.MoveTowards(group.alpha, 1, dt * 5);
            // Rows slide in from the right as the screen fades up.
            content.anchoredPosition = new Vector2(Mathf.Lerp(60, 0, Mathf.SmoothStep(0, 1, group.alpha)), 0);
            var s = SliceSession.Current;
            if (s == null || s.Dead || s.Transitioning) { Show(false); return; }
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) { SetFocus((focus + rows.Count - 1) % rows.Count); Cue(440); }
                if (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame) { SetFocus((focus + 1) % rows.Count); Cue(440); }
                if (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) StepFocused(-1);
                if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) StepFocused(1);
                if ((k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame) && rows[focus].Activate != null)
                { rows[focus].Activate(); return; }
                if (k.rKey.wasPressedThisFrame) ResetDefaults();
            }
            // Animate row highlight and rail.
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; r.Glow = Mathf.MoveTowards(r.Glow, i == focus ? 1 : 0, dt * 7);
                r.Highlight.color = new Color(PolishCine.Accent.r, PolishCine.Accent.g, PolishCine.Accent.b, .24f * r.Glow);
                r.Edge.color = new Color(PolishCine.Accent.r, PolishCine.Accent.g, PolishCine.Accent.b, r.Glow);
            }
            int active = rows[focus].Section;
            for (int t = 0; t < tabs.Count; t++)
            {
                bool on = t == active;
                var (num, label, bar, holder) = tabs[t];
                float x = Mathf.MoveTowards(holder.anchoredPosition.x, on ? 60 : 0, dt * 500);
                holder.anchoredPosition = new Vector2(x, holder.anchoredPosition.y);
                float a = Mathf.Clamp01(x / 60f);
                bar.color = new Color(PolishCine.Accent.r, PolishCine.Accent.g, PolishCine.Accent.b, a);
                num.color = Color.Lerp(PolishCine.TextMuted, PolishCine.Accent, a);
                label.color = Color.Lerp(PolishCine.TextMuted, PolishCine.TextMain, a);
            }
        }
        void Back() { var s = SliceSession.Current; Show(false); if (s != null && s.Started && !s.Dead && !s.Transitioning) s.SetPaused(false); }
        void SetFocus(int i) { focus = Mathf.Clamp(i, 0, rows.Count - 1); }
        void StepFocused(int d) { rows[focus].Step(d); rows[focus].Refresh(); Save(); Cue(d > 0 ? 620 : 520); }
        void RefreshAll() { foreach (var r in rows) r.Refresh(); }

        // ================= launcher (works with the scenes' minimal UI, which draws no menu buttons) =================
        RectTransform hint, pauseButton;
        CanvasGroup hintGroup, pauseGroup;

        void EnsureCanvas()
        {
            if (canvas != null) return;
            if (FindFirstObjectByType<EventSystem>() == null)
            { var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); DontDestroyOnLoad(es); }
            var cgo = new GameObject("Settings canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            cgo.transform.SetParent(transform, false);
            canvas = cgo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var sc = scaler = cgo.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = .5f;

            // Keep the existing hint styling with the single pause/settings key.
            hint = PolishCine.Rect(canvas.transform, "Settings hint", new Vector2(1, 1), new Vector2(-40, -34), new Vector2(260, 40), new Vector2(1, 1));
            hintGroup = hint.gameObject.AddComponent<CanvasGroup>(); hintGroup.blocksRaycasts = false; hintGroup.interactable = false;
            var hcap = PolishCine.Rect(hint, "Key", new Vector2(1, .5f), new Vector2(-146, 0), new Vector2(52, 34), new Vector2(1, .5f));
            PolishCine.Img(hcap, PolishCine.Keycap, new Color(.9f, .9f, .9f), sliced: true);
            PolishCine.Label(PolishCine.Stretch(hcap, "ESC"), "ESC", PolishCine.BodyBold, 19, Color.white, TextAnchor.MiddleCenter);
            PolishCine.Label(PolishCine.Rect(hint, "Label", new Vector2(1, .5f), Vector2.zero, new Vector2(132, 34), new Vector2(1, .5f)), "SETTINGS", PolishCine.Body, 22, new Color(.9f, .9f, .9f), TextAnchor.MiddleRight, 2);

            // Big clickable button on the pause screen.
            pauseButton = PolishCine.Rect(canvas.transform, "Settings button", new Vector2(.5f, .5f), new Vector2(0, -110), new Vector2(380, 76), new Vector2(.5f, .5f));
            pauseGroup = pauseButton.gameObject.AddComponent<CanvasGroup>();
            var bg = PolishCine.Img(pauseButton, PolishCine.White, Color.white, raycast: true);
            var b = Clickable(pauseButton.gameObject, bg, Selectable.Transition.ColorTint, () => { Open(); Cue(440); });
            var c = b.colors; c.normalColor = new Color(0, 0, 0, .55f); c.highlightedColor = new Color(1, .42f, .17f, .35f); c.pressedColor = new Color(1, .42f, .17f, .55f); c.selectedColor = c.normalColor; b.colors = c;
            var edge = PolishCine.Rect(pauseButton, "Edge", new Vector2(0, 0), Vector2.zero, new Vector2(4, 0), new Vector2(0, 0)); edge.anchorMax = new Vector2(0, 1);
            PolishCine.Img(edge, PolishCine.White, PolishCine.Accent);
            var pcap = PolishCine.Rect(pauseButton, "Key", new Vector2(0, .5f), new Vector2(30, 0), new Vector2(60, 40), new Vector2(0, .5f));
            PolishCine.Img(pcap, PolishCine.Keycap, new Color(.9f, .9f, .9f), sliced: true);
            PolishCine.Label(PolishCine.Stretch(pcap, "ESC"), "ESC", PolishCine.BodyBold, 22, Color.white, TextAnchor.MiddleCenter);
            PolishCine.Label(PolishCine.Rect(pauseButton, "Label", new Vector2(0, .5f), new Vector2(112, 2), new Vector2(250, 70), new Vector2(0, .5f)), "SETTINGS", PolishCine.Display, 50, Color.white, TextAnchor.MiddleLeft, 4);
        }

        void UpdateLauncher()
        {
            EnsureCanvas();
            var s = SliceSession.Current;
            bool usable = s != null && !s.Dead && !s.Transitioning && !IsOpen;
            bool paused = usable && (s.Paused || !s.Started);
            bool playing = usable && s.Running;
            scaler.matchWidthOrHeight = (float)Screen.width / Mathf.Max(1, Screen.height) < 1.77f ? 0 : 1;
            hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, playing ? .55f : 0, Time.unscaledDeltaTime * 4);
            pauseGroup.alpha = paused ? 1 : 0; pauseGroup.blocksRaycasts = paused; pauseGroup.interactable = paused;
        }

        // ================= construction =================
        void Build()
        {
            EnsureCanvas();
            root = PolishCine.Stretch(canvas.transform, "Settings screen");
            group = root.gameObject.AddComponent<CanvasGroup>();
            // Darkness from the left, letting the paused game show through on the right.
            PolishCine.Img(PolishCine.Stretch(root, "Base dim"), PolishCine.White, new Color(0, 0, 0, .42f), raycast: true);
            PolishCine.Img(PolishCine.Stretch(root, "Left fade"), PolishCine.HGradient, new Color(0, 0, 0, .9f));
            content = PolishCine.Stretch(root, "Content");

            // Left rail.
            var tl = new Vector2(0, 1);
            PolishCine.Label(PolishCine.Rect(root, "Title", tl, new Vector2(108, -138), new Vector2(700, 140)), "SETTINGS", PolishCine.Display, 132, PolishCine.TextMain, TextAnchor.UpperLeft, 6);
            PolishCine.Label(PolishCine.Rect(root, "Subtitle", tl, new Vector2(112, -284), new Vector2(700, 40)), "A DAY THROUGH HELL", PolishCine.BodyMedium, 24, new Color(.73f, .73f, .73f), TextAnchor.UpperLeft, 6);
            for (int t = 0; t < Sections.Length; t++)
            {
                int section = t;
                var tab = PolishCine.Rect(root, "Tab " + Sections[t], tl, new Vector2(110, -380 - t * 100), new Vector2(460, 76));
                var hit = PolishCine.Img(tab, PolishCine.White, new Color(0, 0, 0, 0), raycast: true);
                Clickable(tab.gameObject, hit, Selectable.Transition.None, () => { SetFocus(rows.FindIndex(r => r.Section == section)); Cue(440); });
                var bar = PolishCine.Img(PolishCine.Rect(tab, "Bar", new Vector2(0, .5f), new Vector2(0, -2), new Vector2(46, 4), new Vector2(0, .5f)), PolishCine.White, PolishCine.Accent);
                var holder = PolishCine.Rect(tab, "Holder", new Vector2(0, .5f), Vector2.zero, new Vector2(400, 76), new Vector2(0, .5f));
                var num = PolishCine.Label(PolishCine.Rect(holder, "Num", new Vector2(0, .5f), new Vector2(0, -10), new Vector2(50, 30), new Vector2(0, .5f)), "0" + (t + 1), PolishCine.Body, 22, PolishCine.TextMuted, TextAnchor.MiddleLeft, 2);
                var label = PolishCine.Label(PolishCine.Rect(holder, "Label", new Vector2(0, .5f), new Vector2(50, 0), new Vector2(360, 76), new Vector2(0, .5f)), Sections[t], PolishCine.Display, 62, PolishCine.TextMuted, TextAnchor.MiddleLeft, 4);
                tabs.Add((num, label, bar, holder));
            }
            PolishCine.Img(PolishCine.Rect(root, "Divider", tl, new Vector2(640, -110), new Vector2(2, 860)), PolishCine.VFade, new Color(1, 1, 1, .6f));

            // Rows.
            float y = -170;
            y = BarRow(y, 0, "Volume", "Overall loudness of the game", 0, 10, () => volumeStep, v => { volumeStep = v; ApplySceneSettings(); }, () => volumeStep * 10 + "%");
            y -= 30;
            y = BarRow(y, 1, "Mouse Sensitivity", "How fast the camera turns", 1, 10, () => sensitivityStep, v => { sensitivityStep = v; ApplySceneSettings(); }, () => Sensitivity.ToString("0.0") + "x");
            y = PickRow(y, 1, "Invert Look", "Flip vertical mouse look", () => invert ? "ON" : "OFF", d => { invert = !invert; ApplySceneSettings(); });
            y -= 30;
            y = PickRow(y, 2, "Fullscreen", "Fill the whole screen", () => fullscreen ? "ON" : "OFF", d => { fullscreen = !fullscreen; ApplyFullscreen(); });
            y = PickRow(y, 2, "Graphics Quality", "Lower for smoother play on laptops", () => QualityLabel(QualitySettings.GetQualityLevel()),
                d => { int n = QualitySettings.names.Length; quality = (QualitySettings.GetQualityLevel() + d + n) % n; QualitySettings.SetQualityLevel(quality, true); });
            y = BarRow(y, 2, "Brightness", "Lift the darkness without washing out the fire", 0, 10, () => brightnessStep, v => { brightnessStep = v; ApplySceneSettings(); },
                () => (Brightness >= 0 ? "+" : "") + Brightness.ToString("0.0"));
            ActionRow(y - 24, 3, "Respawn", "Restart this chamber; keep earlier stage progress.", Respawn);

            // Key-hint footer (clickable for mouse players).
            var foot = PolishCine.Rect(root, "Hint bar", new Vector2(0, 0), Vector2.zero, new Vector2(0, 90), new Vector2(0, 0));
            foot.anchorMax = new Vector2(1, 0); foot.sizeDelta = new Vector2(0, 90);
            PolishCine.Img(foot, PolishCine.White, new Color(0, 0, 0, .6f));
            float hx = -110;
            hx = Hint(foot, hx, "ESC", "RESUME", Back);
            hx = Hint(foot, hx, "R", "RESET DEFAULTS", ResetDefaults);
            hx = Hint(foot, hx, "A  D", "ADJUST", null);
            Hint(foot, hx, "W  S", "SELECT", null);

            root.gameObject.SetActive(false);
        }

        void ActionRow(float y, int section, string name, string description, Action action)
        {
            var row = NewRow(y, section, name, description);
            PolishCine.Label(PolishCine.Rect(row.Rt, "Action", new Vector2(1, .5f), new Vector2(-24, 2),
                new Vector2(360, 60), new Vector2(1, .5f)), "ENTER / RESPAWN", PolishCine.Display, 40,
                PolishCine.Accent, TextAnchor.MiddleRight, 1);
            row.Step = _ => { }; // Adjusting settings must never trigger a stage reset.
            row.Refresh = () => { };
            row.Activate = action;
            Clickable(row.Rt.gameObject, row.Highlight, Selectable.Transition.None, action);
        }

        Row NewRow(float y, int section, string name, string desc)
        {
            var rt = PolishCine.Rect(content, name, new Vector2(0, 1), new Vector2(720, y), new Vector2(1080, 96));
            var row = new Row { Section = section, Rt = rt };
            row.Highlight = PolishCine.Img(PolishCine.Stretch(rt, "Highlight"), PolishCine.HGradient, new Color(1, 1, 1, 0), raycast: true);
            row.Edge = PolishCine.Img(PolishCine.Rect(rt, "Edge", new Vector2(0, 0), Vector2.zero, new Vector2(4, 0), new Vector2(0, 0)), PolishCine.White, new Color(1, 1, 1, 0));
            row.Edge.rectTransform.anchorMax = new Vector2(0, 1);
            var line = PolishCine.Rect(rt, "Hairline", new Vector2(0, 0), Vector2.zero, new Vector2(0, 2), new Vector2(0, 0)); // 2px survives 720p scaling line.anchorMax = new Vector2(1, 0);
            PolishCine.Img(line, PolishCine.White, PolishCine.Hairline);
            PolishCine.Label(PolishCine.Rect(rt, "Name", new Vector2(0, 1), new Vector2(24, -14), new Vector2(600, 44)), name, PolishCine.Body, 34, PolishCine.TextMain, TextAnchor.UpperLeft, .5f);
            PolishCine.Label(PolishCine.Rect(rt, "Description", new Vector2(0, 1), new Vector2(25, -58), new Vector2(620, 30)), desc, PolishCine.BodyMedium, 20, PolishCine.TextDim, TextAnchor.UpperLeft, .8f);
            int index = rows.Count;
            rt.gameObject.AddComponent<PolishHover>().Enter = () => SetFocus(index);
            rows.Add(row);
            return row;
        }

        float BarRow(float y, int section, string name, string desc, int min, int max, Func<int> get, Action<int> set, Func<string> value)
        {
            var row = NewRow(y, section, name, desc);
            var segs = new List<Image>();
            var bars = PolishCine.Rect(row.Rt, "Bars", new Vector2(1, .5f), new Vector2(-170, 0), new Vector2(10 * 30 + 9 * 7, 22), new Vector2(1, .5f));
            for (int i = 0; i < 10; i++)
            {
                int step = i + 1;
                var seg = PolishCine.Img(PolishCine.Rect(bars, "Segment " + step, new Vector2(0, .5f), new Vector2(i * 37, 0), new Vector2(34, 18), new Vector2(0, .5f)), PolishCine.Segment, PolishCine.BarOff, raycast: true);
                // Clicking the top lit segment steps down once, so the mouse can reach 0% / minimum too.
                Clickable(seg.gameObject, seg, Selectable.Transition.None, () => { set(Mathf.Clamp(get() == step ? step - 1 : step, min, max)); row.Refresh(); Save(); Cue(560); });
                segs.Add(seg);
            }
            var val = PolishCine.Label(PolishCine.Rect(row.Rt, "Value", new Vector2(1, .5f), new Vector2(-24, 2), new Vector2(130, 60), new Vector2(1, .5f)), "", PolishCine.Display, 50, PolishCine.TextMain, TextAnchor.MiddleRight, 1);
            row.Step = d => set(Mathf.Clamp(get() + d, min, max));
            row.Refresh = () =>
            {
                int v = get();
                for (int i = 0; i < segs.Count; i++) segs[i].color = i < v ? (row.Glow > .5f ? PolishCine.Accent : Color.white) : PolishCine.BarOff;
                val.text = value();
            };
            // Bars turn ember while the row is focused.
            row.Rt.gameObject.AddComponent<PolishHover>().Tick = () =>
            {
                int v = get();
                for (int i = 0; i < segs.Count; i++) segs[i].color = i < v ? Color.Lerp(Color.white, PolishCine.Accent, row.Glow) : PolishCine.BarOff;
            };
            return y - 96;
        }

        float PickRow(float y, int section, string name, string desc, Func<string> get, Action<int> step)
        {
            var row = NewRow(y, section, name, desc);
            var val = PolishCine.Label(PolishCine.Rect(row.Rt, "Value", new Vector2(1, .5f), new Vector2(-90, 2), new Vector2(340, 60), new Vector2(.5f, .5f)), "", PolishCine.Display, 50, PolishCine.TextMain, TextAnchor.MiddleCenter, 3);
            val.rectTransform.anchoredPosition = new Vector2(-260, 2);
            Arrow(row.Rt, PolishCine.ArrowLeft, new Vector2(-460, 0), () => { step(-1); row.Refresh(); Save(); Cue(520); });
            Arrow(row.Rt, PolishCine.ArrowRight, new Vector2(-60, 0), () => { step(1); row.Refresh(); Save(); Cue(620); });
            row.Step = step; row.Refresh = () => val.text = get();
            return y - 96;
        }

        void Arrow(Transform parent, Sprite sprite, Vector2 pos, Action click)
        {
            var img = PolishCine.Img(PolishCine.Rect(parent, "Arrow", new Vector2(1, .5f), pos, new Vector2(30, 30), new Vector2(.5f, .5f)), sprite, PolishCine.Accent, raycast: true);
            var b = Clickable(img.gameObject, img, Selectable.Transition.ColorTint, click);
            var c = b.colors; c.normalColor = Color.white; c.highlightedColor = new Color(1, .85f, .75f); c.pressedColor = new Color(.7f, .7f, .7f); c.selectedColor = Color.white; b.colors = c;
        }

        /// <summary>Mouse-only button: no keyboard/gamepad navigation and never left selected, so W/A/S/D/Enter
        /// only ever drive the highlighted row through this screen's own handler.</summary>
        static Button Clickable(GameObject go, Graphic target, Selectable.Transition transition, Action click)
        {
            var b = go.AddComponent<Button>(); b.targetGraphic = target; b.transition = transition;
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            b.onClick.AddListener(() => { click(); EventSystem.current?.SetSelectedGameObject(null); });
            return b;
        }

        float Hint(Transform foot, float x, string key, string label, Action click)
        {
            var labelText = PolishCine.Label(PolishCine.Rect(foot, label, new Vector2(1, .5f), new Vector2(x, 0), new Vector2(400, 40), new Vector2(1, .5f)), label, PolishCine.Body, 26, new Color(.87f, .87f, .87f), TextAnchor.MiddleRight, 2);
            float labelWidth = labelText.preferredWidth + label.Length * 2;
            labelText.rectTransform.sizeDelta = new Vector2(labelWidth, 40);
            float capWidth = Mathf.Max(56, key.Length * 16 + 26);
            var cap = PolishCine.Rect(foot, key + " key", new Vector2(1, .5f), new Vector2(x - labelWidth - 14, 0), new Vector2(capWidth, 40), new Vector2(1, .5f));
            PolishCine.Img(cap, PolishCine.Keycap, new Color(.87f, .87f, .87f), sliced: true);
            PolishCine.Label(PolishCine.Stretch(cap, "Key"), key, PolishCine.BodyBold, 22, new Color(.9f, .9f, .9f), TextAnchor.MiddleCenter);
            if (click != null)
            {
                var hit = PolishCine.Rect(foot, label + " button", new Vector2(1, .5f), new Vector2(x + 8, 0), new Vector2(labelWidth + capWidth + 30, 56), new Vector2(1, .5f));
                // Opaque base colour: the tint supplies the alpha (a transparent base would hide every state).
                var img = PolishCine.Img(hit, PolishCine.White, Color.white, raycast: true);
                var b = Clickable(hit.gameObject, img, Selectable.Transition.ColorTint, click);
                var c = b.colors; c.normalColor = new Color(1, 1, 1, 0); c.highlightedColor = new Color(1, .42f, .17f, .18f); c.pressedColor = new Color(1, .42f, .17f, .3f); c.selectedColor = c.normalColor; b.colors = c;
            }
            return x - labelWidth - capWidth - 60;
        }
    }

    /// <summary>Small pointer helper: Enter on hover, optional per-frame Tick.</summary>
    public sealed class PolishHover : MonoBehaviour, IPointerEnterHandler
    {
        public Action Enter, Tick;
        public void OnPointerEnter(PointerEventData e) => Enter?.Invoke();
        void Update() => Tick?.Invoke();
    }
}
