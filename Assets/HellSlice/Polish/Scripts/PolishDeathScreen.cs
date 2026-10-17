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
    /// Death screen in the settings screen's "Cinematic Minimal" style. SliceSession.KillPlayer hands death here
    /// instead of reloading at once. The world drains of colour, embers rise, and the verse catches fire letter by letter;
    /// then the player chooses Rise Again (reload this chamber) or Leave Hell (quit, asked twice).
    /// Any key or click skips the reveal. Mouse: hover and click rows. Keyboard: W/S select, Enter choose.
    /// </summary>
    public sealed class PolishDeathScreen : MonoBehaviour
    {
        public enum Cause { Fire, Fall, Caught, Wounds, Unknown }

        public static PolishDeathScreen Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance.root != null && Instance.root.gameObject.activeSelf;

        // ---- words ----
        const string Title = "DEATH, AGAIN";
        const string Line = "Even here, where nothing lives, you found a way to die.";
        static string Verse(Cause c) => c switch
        {
            Cause.Fire => "The fire did not hate you. It only kept its promise.",
            Cause.Fall => "The dark below has no floor. Only patience.",
            Cause.Caught => "You ran, but what follows you here wears your own footsteps.",
            Cause.Wounds => "Your body gave way long before your will did.",
            _ => "The flame forgets nothing, and forgives slowly.",
        };

        // ---- timeline (seconds since death, unscaled) ----
        const float TEyebrow = .5f, TTitle = .9f, TLine = 2.15f, TVerse = 3.0f, TMenu = 3.7f, TEnd = 4.8f;

        /// <summary>Review capture only: drives the reveal clock by hand so frames are evenly spaced.</summary>
        [NonSerialized] public float ClockOverride = -1;

        static readonly Dictionary<string, int> deaths = new Dictionary<string, int>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void NewRun() { deaths.Clear(); Instance = null; }

        sealed class Row { public RectTransform Rt; public Image Highlight, Edge; public Text Name, Desc, Action; public Action Activate; public float Glow; }
        readonly List<Row> rows = new List<Row>();
        struct Ember { public RectTransform Rt; public Image Img; public float Speed, Sway, Phase, Life, Age; }
        readonly List<Ember> embers = new List<Ember>();
        readonly Dictionary<Volume, float> baseSaturation = new Dictionary<Volume, float>();

        Canvas canvas; CanvasScaler scaler;
        RectTransform root, emberLayer, burnLine, burnHead, menu;
        CanvasGroup sceneGroup, group, menuGroup, footGroup;
        Image flash, dim, vignette, floor, black;
        Text eyebrow, title, titleGlow, line, verse;
        UIGlyphReveal eyebrowFx, titleFx, titleGlowFx, lineFx, verseFx;
        UILetterSpacing titleSpacing;
        SliceSession session;
        float clock, quitArmed, fadeOut = -1, fadeIn = -1;
        int focus; bool rising;

        // ================= entry points =================
        /// <summary>Called by SliceSession.KillPlayer. Returns false if the screen cannot show (the session then reloads at once).</summary>
        public static bool Show(SliceSession s) => s != null && Show(s, Infer(s));
        public static bool Show(SliceSession s, Cause cause)
        {
            if (s == null) return false;
            if (Instance == null) { var go = new GameObject("Polish death screen"); DontDestroyOnLoad(go); Instance = go.AddComponent<PolishDeathScreen>(); }
            Instance.Open(s, cause);
            return true;
        }

        static Cause Infer(SliceSession s)
        {
            var p = s.Player;
            if (p == null) return Cause.Unknown;
            if (p.transform.position.y < -3) return Cause.Fall;
            if (p.Health <= 0) return Cause.Wounds;
            foreach (var lava in FindObjectsByType<SliceLava>(FindObjectsSortMode.None))
                if (lava.Volume != null && lava.Volume.enabled && p.Body != null && lava.Volume.bounds.Intersects(p.Body.bounds)) return Cause.Fire;
            return s.Enemy != null || s.Chase != null ? Cause.Caught : Cause.Unknown;
        }

        void Awake() { SceneManager.sceneLoaded += SceneLoaded; }
        void OnDestroy() { SceneManager.sceneLoaded -= SceneLoaded; if (Instance == this) Instance = null; }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // The chamber comes back out of the dark.
            baseSaturation.Clear();
            if (IsOpen && rising) { rising = false; fadeIn = 0; }
            else if (IsOpen) root.gameObject.SetActive(false);
        }

        void Open(SliceSession s, Cause cause)
        {
            if (root == null) Build();
            session = s;
            string stage = s.gameObject.scene.path;
            deaths.TryGetValue(stage, out int count); deaths[stage] = ++count;
            // Wide tracking swallows single spaces, so words are set apart with doubles.
            eyebrow.text = "CHAMBER  " + Roman(s.StageNumber) + "    ·    THE  " + Ordinal(count) + "  DEATH";
            verse.text = Verse(cause);
            clock = 0; quitArmed = 0; fadeOut = fadeIn = -1; rising = false; focus = 0;
            foreach (var r in rows) r.Glow = 0;
            RefreshQuit();
            black.color = Color.clear; sceneGroup.alpha = 1;
            for (int i = 0; i < embers.Count; i++) { var e = embers[i]; Respawn(ref e, true); embers[i] = e; }
            baseSaturation.Clear();
            foreach (var v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (v.isGlobal && v.profile != null && v.profile.TryGet<ColorAdjustments>(out var ca))
                    baseSaturation[v] = ca.saturation.overrideState ? ca.saturation.value : 0;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            s.Feedback?.Cue(62, .7f);
            root.gameObject.SetActive(true);
            Apply(0);
        }

        // ================= update =================
        void Update()
        {
            if (!IsOpen) return;
            float dt = ClockOverride >= 0 ? Mathf.Max(0, ClockOverride - clock) : Time.unscaledDeltaTime;
            clock = ClockOverride >= 0 ? ClockOverride : clock + dt;
            scaler.matchWidthOrHeight = (float)Screen.width / Mathf.Max(1, Screen.height) < 1.77f ? 0 : 1;

            if (fadeIn >= 0)
            {
                // After Rise Again: the reloaded chamber fades in from black, then the screen closes.
                fadeIn += Time.unscaledDeltaTime;
                sceneGroup.alpha = 0; black.color = new Color(0, 0, 0, 1 - Mathf.SmoothStep(0, 1, fadeIn / .9f));
                if (fadeIn >= .9f) { fadeIn = -1; root.gameObject.SetActive(false); }
                return;
            }
            if (fadeOut >= 0)
            {
                fadeOut += Time.unscaledDeltaTime;
                black.color = new Color(0, 0, 0, Mathf.SmoothStep(0, 1, fadeOut / .7f));
                if (fadeOut >= .7f && !rising) { rising = true; session?.Restart(); }
                Apply(clock); Embers(dt); return;
            }

            HandleInput();
            Apply(clock);
            Embers(dt);
            if (quitArmed > 0 && (quitArmed -= Time.unscaledDeltaTime) <= 0) RefreshQuit();
        }

        void HandleInput()
        {
            var k = Keyboard.current; var m = Mouse.current;
            bool menuLive = clock >= TMenu + .25f;
            if (!menuLive)
            {
                // Any fresh key or click skips the reveal (short grace so the killing input does not skip it).
                bool any = (k != null && k.anyKey.wasPressedThisFrame) || (m != null && (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame));
                if (any && clock > .45f && ClockOverride < 0) clock = TEnd;
                return;
            }
            if (k == null) return;
            if (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) { focus = (focus + rows.Count - 1) % rows.Count; Cue(440); }
            if (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame) { focus = (focus + 1) % rows.Count; Cue(440); }
            if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame) rows[focus].Activate();
        }

        /// <summary>Lays out every animated element for reveal time t.</summary>
        void Apply(float t)
        {
            float dark = Mathf.SmoothStep(0, 1, t / 1.4f);
            // A red pulse at the moment of death, then the darkness closes from the edges.
            flash.color = new Color(.7f, .06f, .02f, .3f * Mathf.Clamp01(1 - t / .3f));
            dim.color = new Color(0, 0, 0, .58f * dark);
            vignette.color = new Color(.02f, 0, 0, .96f * dark);
            floor.color = new Color(0, 0, 0, .85f * dark);
            Drain(Mathf.SmoothStep(0, 1, t / 1.8f));

            Fade(eyebrow, t, TEyebrow, .6f); eyebrowFx.Set(t - TEyebrow);
            titleFx.Set(t - TTitle); titleGlowFx.Set(t - TTitle);
            // The title breathes in: wide tracking that tightens as the letters take.
            titleSpacing.Spacing = Mathf.Lerp(34, 12, Mathf.SmoothStep(0, 1, (t - TTitle) / 2.2f)); title.SetVerticesDirty();
            var glowSpacing = titleGlow.GetComponent<UILetterSpacing>(); glowSpacing.Spacing = titleSpacing.Spacing; titleGlow.SetVerticesDirty();
            // Ember breathing on the halo once the title has formed.
            titleGlow.color = new Color(1, 1, 1, t > TTitle + 2 ? .75f + .25f * Mathf.Sin(t * 2.1f) : 1);

            // A burning line draws out from the centre under the title.
            float burn = Mathf.SmoothStep(0, 1, (t - (TTitle + .9f)) / 1.1f);
            burnLine.sizeDelta = new Vector2(980 * burn, 2);
            burnHead.anchoredPosition = new Vector2(490 * burn, 0);
            burnHead.GetComponent<Image>().color = new Color(1, .78f, .45f, burn > 0 && burn < 1 ? 1 : 0);
            burnHead.localScale = Vector3.one * (.8f + .3f * Mathf.Sin(t * 30));

            lineFx.Set(t - TLine); verseFx.Set(t - TVerse);

            float m = Mathf.SmoothStep(0, 1, (t - TMenu) / .8f);
            menuGroup.alpha = m; menuGroup.blocksRaycasts = menuGroup.interactable = m > .5f;
            menu.anchoredPosition = new Vector2(0, Mathf.Lerp(-40, 0, m));
            footGroup.alpha = Mathf.SmoothStep(0, 1, (t - TMenu - .4f) / .6f);
            float fade = fadeOut >= 0 ? 1 - Mathf.Clamp01(fadeOut / .5f) : 1;
            group.alpha = fade;

            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i]; r.Glow = ClockOverride >= 0 ? (i == focus ? 1 : 0) : Mathf.MoveTowards(r.Glow, i == focus ? 1 : 0, dt * 7);
                r.Highlight.color = new Color(PolishCine.Accent.r, PolishCine.Accent.g, PolishCine.Accent.b, .24f * r.Glow);
                r.Edge.color = new Color(PolishCine.Accent.r, PolishCine.Accent.g, PolishCine.Accent.b, r.Glow);
                r.Name.color = Color.Lerp(new Color(.82f, .82f, .82f), Color.white, r.Glow);
            }
        }

        static void Fade(Graphic g, float t, float start, float seconds) => g.canvasRenderer.SetAlpha(Mathf.SmoothStep(0, 1, (t - start) / seconds));

        /// <summary>The world loses its colour while the fire keeps its own (UI embers are not affected).</summary>
        void Drain(float k)
        {
            foreach (var pair in baseSaturation)
                if (pair.Key != null && pair.Key.profile.TryGet<ColorAdjustments>(out var ca)) ca.saturation.Override(Mathf.Lerp(pair.Value, -78, k));
        }

        void Embers(float dt)
        {
            float glow = Mathf.SmoothStep(0, 1, clock / 1.2f);
            for (int i = 0; i < embers.Count; i++)
            {
                var e = embers[i];
                e.Age += dt;
                if (e.Age > e.Life) Respawn(ref e, false);
                float k = e.Age / e.Life;
                var p = e.Rt.anchoredPosition;
                p.y += e.Speed * dt; p.x += Mathf.Sin(e.Age * e.Sway + e.Phase) * 22 * dt;
                e.Rt.anchoredPosition = p;
                float flicker = .65f + .35f * Mathf.Sin(clock * 9 + e.Phase * 7);
                var c = Color.Lerp(new Color(1, .84f, .5f), PolishCine.Accent, k);
                c.a = glow * flicker * Mathf.Sin(k * Mathf.PI) * .9f;
                e.Img.color = c;
                embers[i] = e;
            }
        }

        void Respawn(ref Ember e, bool scatter)
        {
            e.Life = UnityEngine.Random.Range(3.5f, 7f);
            e.Age = scatter ? UnityEngine.Random.Range(0, e.Life) : 0;
            e.Speed = UnityEngine.Random.Range(45f, 150f);
            e.Sway = UnityEngine.Random.Range(.6f, 2.2f); e.Phase = UnityEngine.Random.Range(0, 6.3f);
            float size = UnityEngine.Random.Range(4f, 11f); e.Rt.sizeDelta = new Vector2(size, size);
            e.Rt.anchoredPosition = new Vector2(UnityEngine.Random.Range(-1000f, 1000f), -560 + (scatter ? e.Speed * e.Age : UnityEngine.Random.Range(-40f, 0)));
        }

        // ================= choices =================
        void Rise()
        {
            if (fadeOut >= 0 || session == null) return;
            Cue(330); fadeOut = 0;
        }
        void Leave()
        {
            if (fadeOut >= 0) return;
            if (quitArmed <= 0) { quitArmed = 3; RefreshQuit(); Cue(520); return; }
            Cue(220);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void RefreshQuit()
        {
            if (rows.Count < 2) return;
            rows[1].Action.text = quitArmed > 0 ? "PRESS AGAIN" : "QUIT TO DESKTOP";
            rows[1].Desc.text = quitArmed > 0 ? "Are you sure? Choose it again to leave." : "Close the game. The fire will keep your place.";
        }
        /// <summary>Review capture only.</summary>
        public void PreviewFocus(int i) { focus = Mathf.Clamp(i, 0, rows.Count - 1); }
        public void PreviewChoose() => rows[focus].Activate();

        static void Cue(int hz) => SliceSession.Current?.Feedback?.Cue(hz, .05f);
        static string Roman(int n) => n >= 1 && n <= 10 ? new[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" }[n - 1] : n.ToString();
        static string Ordinal(int n) => n >= 1 && n <= 10
            ? new[] { "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH", "SEVENTH", "EIGHTH", "NINTH", "TENTH" }[n - 1]
            : n + (n % 100 is 11 or 12 or 13 ? "TH" : (n % 10) switch { 1 => "ST", 2 => "ND", 3 => "RD", _ => "TH" });

        // ================= construction =================
        void Build()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            { var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); DontDestroyOnLoad(es); }
            var cgo = new GameObject("Death canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            cgo.transform.SetParent(transform, false);
            canvas = cgo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 110; // above settings
            scaler = cgo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            root = PolishCine.Stretch(canvas.transform, "Death screen");
            var stage = PolishCine.Stretch(root, "Screen");
            sceneGroup = stage.gameObject.AddComponent<CanvasGroup>();
            flash = PolishCine.Img(PolishCine.Stretch(stage, "Death flash"), PolishCine.White, Color.clear, raycast: true);
            dim = PolishCine.Img(PolishCine.Stretch(stage, "Dim"), PolishCine.White, Color.clear);
            vignette = PolishCine.Img(PolishCine.Stretch(stage, "Vignette"), Vignette, Color.clear);
            floor = PolishCine.Img(PolishCine.Rect(stage, "Floor fade", new Vector2(.5f, 0), Vector2.zero, new Vector2(0, 520), new Vector2(.5f, 0)), FloorFade, Color.clear);
            floor.rectTransform.anchorMin = new Vector2(0, 0); floor.rectTransform.anchorMax = new Vector2(1, 0);

            emberLayer = PolishCine.Stretch(stage, "Embers");
            for (int i = 0; i < 46; i++)
            {
                var rt = PolishCine.Rect(emberLayer, "Ember", new Vector2(.5f, .5f), Vector2.zero, new Vector2(6, 6));
                var e = new Ember { Rt = rt, Img = PolishCine.Img(rt, Dot, Color.clear) };
                Respawn(ref e, true); embers.Add(e);
            }

            var content = PolishCine.Stretch(stage, "Content");
            group = content.gameObject.AddComponent<CanvasGroup>();
            var c = new Vector2(.5f, .5f);

            eyebrow = Reveal(PolishCine.Rect(content, "Eyebrow", c, new Vector2(0, 300), new Vector2(1400, 40)), "", PolishCine.Body, 24, new Color(.72f, .66f, .62f), 9, .03f, .5f, 6, out eyebrowFx);

            // Halo behind the title: the same letters in ember, spread by outlines; it flares as each letter catches.
            titleGlow = Reveal(PolishCine.Rect(content, "Title glow", c, new Vector2(0, 175), new Vector2(1600, 220)), Title, PolishCine.Display, 210, PolishCine.Accent, 12, .11f, .8f, 26, out titleGlowFx, shadow: false);
            titleGlowFx.Flare = true;
            foreach (var d in new[] { 3f, 6f, 10f })
            { var o = titleGlow.gameObject.AddComponent<Outline>(); o.effectColor = new Color(1, .35f, .08f, .16f); o.effectDistance = new Vector2(d, d); }
            title = Reveal(PolishCine.Rect(content, "Title", c, new Vector2(0, 175), new Vector2(1600, 220)), Title, PolishCine.Display, 210, Color.white, 12, .11f, .8f, 26, out titleFx);
            titleSpacing = title.GetComponent<UILetterSpacing>();

            burnLine = PolishCine.Rect(content, "Burn line", c, new Vector2(0, 52), new Vector2(0, 2));
            PolishCine.Img(burnLine, LineFade, new Color(1, .55f, .25f, .85f));
            burnHead = PolishCine.Rect(burnLine, "Burn head", c, Vector2.zero, new Vector2(26, 26));
            PolishCine.Img(burnHead, Dot, Color.clear);

            line = Reveal(PolishCine.Rect(content, "Line", c, new Vector2(0, -8), new Vector2(1500, 50)), Line, PolishCine.BodyMedium, 38, new Color(.94f, .92f, .9f), .5f, .022f, .45f, 10, out lineFx);
            verse = Reveal(PolishCine.Rect(content, "Verse", c, new Vector2(0, -62), new Vector2(1500, 40)), "", PolishCine.BodyMedium, 26, new Color(.95f, .62f, .42f), .8f, .02f, .45f, 8, out verseFx);
            verse.fontStyle = FontStyle.Italic;

            // Choices, in the settings screen's row style.
            menu = PolishCine.Rect(content, "Choices", c, Vector2.zero, new Vector2(960, 0));
            menuGroup = menu.gameObject.AddComponent<CanvasGroup>();
            NewRow(-178, "Rise Again", "Hell is not an ending. It is a door that keeps opening.", "RESTART CHAMBER", Rise);
            NewRow(-274, "Leave Hell", "", "QUIT TO DESKTOP", Leave);
            RefreshQuit();

            // Key-hint footer, as on the settings screen.
            var foot = PolishCine.Rect(content, "Hint bar", new Vector2(0, 0), Vector2.zero, new Vector2(0, 90), new Vector2(0, 0));
            foot.anchorMax = new Vector2(1, 0); foot.sizeDelta = new Vector2(0, 90);
            footGroup = foot.gameObject.AddComponent<CanvasGroup>();
            PolishCine.Img(foot, PolishCine.White, new Color(0, 0, 0, .6f));
            float hx = -110;
            hx = Hint(foot, hx, "ENTER", "CHOOSE");
            Hint(foot, hx, "W  S", "SELECT");
            PolishCine.Label(PolishCine.Rect(foot, "Tagline", new Vector2(0, .5f), new Vector2(110, 0), new Vector2(700, 40), new Vector2(0, .5f)), "A DAY THROUGH HELL", PolishCine.BodyMedium, 22, new Color(.55f, .55f, .55f), TextAnchor.MiddleLeft, 6);

            black = PolishCine.Img(PolishCine.Stretch(root, "Black"), PolishCine.White, Color.clear);
            root.gameObject.SetActive(false);
        }

        Text Reveal(RectTransform rt, string text, Font font, int size, Color color, float spacing, float stagger, float duration, float rise, out UIGlyphReveal fx, bool shadow = true)
        {
            var tx = rt.gameObject.AddComponent<Text>();
            tx.font = font; tx.text = text; tx.fontSize = size; tx.color = color; tx.alignment = TextAnchor.MiddleCenter; tx.raycastTarget = false;
            tx.horizontalOverflow = HorizontalWrapMode.Overflow; tx.verticalOverflow = VerticalWrapMode.Overflow; tx.supportRichText = false;
            if (spacing != 0) rt.gameObject.AddComponent<UILetterSpacing>().Spacing = spacing;
            fx = rt.gameObject.AddComponent<UIGlyphReveal>(); fx.Stagger = stagger; fx.Duration = duration; fx.Rise = rise;
            // Shadow after the reveal so it copies each letter's fading alpha.
            if (shadow) { var sh = rt.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, .7f); sh.effectDistance = new Vector2(0, -3); }
            return tx;
        }

        void NewRow(float y, string name, string desc, string action, Action activate)
        {
            var rt = PolishCine.Rect(menu, name, new Vector2(.5f, .5f), new Vector2(0, y), new Vector2(960, 96));
            var row = new Row { Rt = rt, Activate = activate };
            row.Highlight = PolishCine.Img(PolishCine.Stretch(rt, "Highlight"), PolishCine.HGradient, new Color(1, 1, 1, 0), raycast: true);
            row.Edge = PolishCine.Img(PolishCine.Rect(rt, "Edge", new Vector2(0, 0), Vector2.zero, new Vector2(4, 0), new Vector2(0, 0)), PolishCine.White, new Color(1, 1, 1, 0));
            row.Edge.rectTransform.anchorMax = new Vector2(0, 1);
            var hair = PolishCine.Rect(rt, "Hairline", new Vector2(0, 0), Vector2.zero, new Vector2(0, 2), new Vector2(0, 0)); hair.anchorMax = new Vector2(1, 0);
            PolishCine.Img(hair, PolishCine.White, PolishCine.Hairline);
            row.Name = PolishCine.Label(PolishCine.Rect(rt, "Name", new Vector2(0, 1), new Vector2(24, -14), new Vector2(600, 44)), name, PolishCine.Body, 34, PolishCine.TextMain, TextAnchor.UpperLeft, .5f);
            row.Desc = PolishCine.Label(PolishCine.Rect(rt, "Description", new Vector2(0, 1), new Vector2(25, -58), new Vector2(640, 30)), desc, PolishCine.BodyMedium, 20, PolishCine.TextDim, TextAnchor.UpperLeft, .8f);
            row.Action = PolishCine.Label(PolishCine.Rect(rt, "Action", new Vector2(1, .5f), new Vector2(-24, 2), new Vector2(360, 60), new Vector2(1, .5f)), action, PolishCine.Display, 40, PolishCine.Accent, TextAnchor.MiddleRight, 1);
            int index = rows.Count;
            rt.gameObject.AddComponent<PolishHover>().Enter = () => { if (focus != index) Cue(440); focus = index; };
            Clickable(rt.gameObject, row.Highlight, () => { focus = index; activate(); });
            rows.Add(row);
        }

        static void Clickable(GameObject go, Graphic target, Action click)
        {
            var b = go.AddComponent<Button>(); b.targetGraphic = target; b.transition = Selectable.Transition.None;
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            b.onClick.AddListener(() => { click(); EventSystem.current?.SetSelectedGameObject(null); });
        }

        static float Hint(Transform foot, float x, string key, string label)
        {
            var labelText = PolishCine.Label(PolishCine.Rect(foot, label, new Vector2(1, .5f), new Vector2(x, 0), new Vector2(400, 40), new Vector2(1, .5f)), label, PolishCine.Body, 26, new Color(.87f, .87f, .87f), TextAnchor.MiddleRight, 2);
            float labelWidth = labelText.preferredWidth + label.Length * 2;
            labelText.rectTransform.sizeDelta = new Vector2(labelWidth, 40);
            float capWidth = Mathf.Max(56, key.Length * 16 + 26);
            var cap = PolishCine.Rect(foot, key + " key", new Vector2(1, .5f), new Vector2(x - labelWidth - 14, 0), new Vector2(capWidth, 40), new Vector2(1, .5f));
            PolishCine.Img(cap, PolishCine.Keycap, new Color(.87f, .87f, .87f), sliced: true);
            PolishCine.Label(PolishCine.Stretch(cap, "Key"), key, PolishCine.BodyBold, 22, new Color(.9f, .9f, .9f), TextAnchor.MiddleCenter);
            return x - labelWidth - capWidth - 60;
        }

        // ---------- generated shapes ----------
        static Sprite vignetteSprite, floorSprite, lineSprite, dotSprite;
        static Sprite Make(int w, int h, Func<float, float, float> a)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "PolishDeath" };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = new Color(1, 1, 1, Mathf.Clamp01(a((x + .5f) / w, (y + .5f) / h)));
            t.SetPixels(px); t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, w, h), Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
        }
        /// <summary>Clear centre, dark edges.</summary>
        static Sprite Vignette => vignetteSprite != null ? vignetteSprite : vignetteSprite = Make(256, 256, (x, y) =>
        {
            float dx = (x - .5f) * 2, dy = (y - .5f) * 2.3f;
            return Mathf.SmoothStep(.25f, 1.25f, Mathf.Sqrt(dx * dx + dy * dy));
        });
        static Sprite FloorFade => floorSprite != null ? floorSprite : floorSprite = Make(4, 256, (x, y) => Mathf.Pow(1 - y, 1.6f));
        static Sprite LineFade => lineSprite != null ? lineSprite : lineSprite = Make(256, 4, (x, y) => Mathf.Pow(Mathf.Sin(x * Mathf.PI), .7f));
        static Sprite Dot => dotSprite != null ? dotSprite : dotSprite = Make(32, 32, (x, y) =>
        {
            float d = Mathf.Sqrt((x - .5f) * (x - .5f) + (y - .5f) * (y - .5f)) * 2;
            return Mathf.Pow(Mathf.Clamp01(1 - d), 1.8f);
        });
    }

    /// <summary>
    /// Letter-by-letter reveal for legacy uGUI Text: each glyph rises into place and fades in, starting as an ember
    /// and cooling to the text colour. With Flare set (the halo layer) each glyph flares bright and settles low.
    /// Add it before any Shadow/Outline so those copy the per-glyph alpha.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class UIGlyphReveal : BaseMeshEffect
    {
        public float Time = -1, Stagger = .05f, Duration = .6f, Rise = 12;
        public bool Flare;
        static readonly Color Ember = new Color(1f, .58f, .22f);

        public void Set(float time)
        {
            if (Mathf.Approximately(time, Time) || (time > 0 && Time > 0 && Done(time) && Done(Time))) return;
            Time = time; graphic.SetVerticesDirty();
        }
        bool Done(float t) => graphic is Text tx && t > Duration + Stagger * tx.text.Length;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            int quads = vh.currentVertCount / 4;
            var v = new UIVertex();
            for (int q = 0; q < quads; q++)
            {
                float p = Mathf.Clamp01((Time - q * Stagger) / Duration);
                float a = Mathf.SmoothStep(0, 1, p);
                float cool = Mathf.SmoothStep(0, 1, (p - .3f) / .7f);
                // Halo: flares as the letter catches, then settles to a low ember.
                float alpha = Flare ? Mathf.Clamp01(a * (1.6f - 1.25f * cool)) : a;
                for (int i = 0; i < 4; i++)
                {
                    vh.PopulateUIVertex(ref v, q * 4 + i);
                    Color c = v.color;
                    Color tint = Flare ? c : Color.Lerp(Ember, c, cool);
                    tint.a = c.a * alpha;
                    v.color = tint;
                    v.position.y -= (1 - Mathf.SmoothStep(0, 1, p)) * Rise;
                    vh.SetUIVertex(v, q * 4 + i);
                }
            }
        }
    }
}
