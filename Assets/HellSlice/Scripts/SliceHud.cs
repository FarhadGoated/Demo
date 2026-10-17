using UnityEngine;

namespace HellSlice
{
    public sealed class SliceHud : MonoBehaviour
    {
        public Canvas GameplayCanvas;
        public UnityEngine.UI.Text ObjectiveText, StatusText, PromptText;
        public bool MinimalUI;
        GUIStyle title, heading, body, small, button;
        readonly Color ink = new Color(.035f, .045f, .065f, .94f);
        readonly Color muted = new Color(.65f, .72f, .8f);
        readonly Color gold = new Color(1, .72f, .36f);
        float fps;
        bool showControls, showPerformance;
        void Update()
        {
            if (Time.unscaledDeltaTime > 0) fps = Mathf.Lerp(fps, 1 / Time.unscaledDeltaTime, .03f);
            if (SliceSession.Current != null && SliceSession.Current.Running) showControls = false;
            var s = SliceSession.Current;
            if (GameplayCanvas != null && s != null && s.Player != null)
            {
                GameplayCanvas.enabled = s.Running;
                ObjectiveText.text = s.Objective;
                string time = !s.SlowTimeAvailable ? "" : s.Clock.Remaining > 0 ? "Slow time: " + s.Clock.Remaining.ToString("0.0") + "s" : s.Clock.Recharge > 0 ? "Time recharge: " + s.Clock.Recharge.ToString("0.0") + "s" : "Q / Slow time ready";
                StatusText.text = "Health: " + s.Player.Health + " / 5    " + time;
                PromptText.text = s.Interaction.Prompt;
                if (!string.IsNullOrEmpty(s.CurrentMessage)) PromptText.text += "\n" + s.CurrentMessage;
                if (MinimalUI) PromptText.text = s.Interaction.Prompt;
            }
        }
        void Init()
        {
            title = Style(42, Color.white, FontStyle.Bold); heading = Style(21, gold, FontStyle.Bold);
            body = Style(18, Color.white); small = Style(14, muted);
            button = new GUIStyle(GUI.skin.button) { fontSize = 19, fixedHeight = 48 };
        }
        GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, wordWrap = true };
            style.normal.textColor = color; return style;
        }
        void Panel(Rect rect, Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        void Bar(float x, float y, float width, float ratio, Color color)
        {
            Panel(new Rect(x, y, width, 6), new Color(.19f, .23f, .29f)); Panel(new Rect(x, y, width * Mathf.Clamp01(ratio), 6), color);
        }
        void OnGUI()
        {
            var s = SliceSession.Current; if (s == null || s.Player == null || Polish.PolishSettings.IsOpen || Polish.PolishDeathScreen.IsOpen) return;
            if (title == null) Init();
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            if (MinimalUI)
            {
                if (s.Dead) GUI.Label(new Rect(420, 320, 500, 60), "Restarting stage...", body);
                GUI.matrix = Matrix4x4.identity; return;
            }
            if (GameplayCanvas != null)
            {
                if (!s.Started || s.Paused || s.Dead) Menu(s);
                GUI.matrix = Matrix4x4.identity;
                return;
            }
            Panel(new Rect(24, 22, 740, 85), ink);
            GUI.Label(new Rect(44, 32, 690, 22), s.landing ? "A DAY THROUGH HELL / NEXT LANDING" : "A DAY THROUGH HELL / THE UNANSWERED CALL", small);
            GUI.Label(new Rect(44, 60, 690, 42), s.Objective, body);
            Panel(new Rect(970, 22, 286, 157), ink);
            GUI.Label(new Rect(990, 32, 240, 23), "BODY  " + s.Player.Health + " / 5", small);
            Bar(990, 59, 240, s.Player.Health / 5f, gold);
            string time = !s.SlowTimeAvailable ? "" : s.Clock.Remaining > 0 ? "SLOW TIME  " + s.Clock.Remaining.ToString("0.0") + "s" : s.Clock.Recharge > 0 ? "RECHARGING  " + s.Clock.Recharge.ToString("0.0") + "s" : "TIME READY";
            if (s.SlowTimeAvailable) GUI.Label(new Rect(990, 74, 240, 24), time, small);
            if (s.SlowTimeAvailable) Bar(990, 104, 240, s.Clock.Remaining > 0 ? s.Clock.Remaining / SliceTime.Duration : 1 - s.Clock.Recharge / SliceTime.RechargeDuration, new Color(.2f, .75f, .9f));
            if (s.Player.AttackFlash > 0) GUI.Label(new Rect(590, 330, 180, 35), "— STRIKE —", heading);
            if (!string.IsNullOrEmpty(s.Interaction.Prompt)) { Panel(new Rect(390, 595, 500, 44), ink); GUI.Label(new Rect(410, 603, 470, 32), s.Interaction.Prompt, body); }
            if (!string.IsNullOrEmpty(s.CurrentMessage)) { Panel(new Rect(180, 115, 920, 48), ink); GUI.Label(new Rect(200, 125, 880, 35), s.CurrentMessage, body); }
            if (showPerformance) GUI.Label(new Rect(1170, 674, 80, 24), Mathf.RoundToInt(fps) + " FPS", small);
            if (!s.Started || s.Paused || s.Dead) Menu(s);
            else if (s.landing)
            {
                Panel(new Rect(240, 430, 800, 150), ink);
                GUI.Label(new Rect(270, 450, 740, 35), "ONE CHAMBER. ONE ACT OF RESPONSIBILITY.", heading);
                GUI.Label(new Rect(270, 493, 740, 65), "You chose to stop and help. The journey continues beyond this threshold. Explore the landing, or press Esc to pause.", body);
            }
            GUI.matrix = Matrix4x4.identity;
        }
        void Menu(SliceSession s)
        {
            Panel(new Rect(0, 0, 1280, 720), new Color(.01f, .015f, .025f, .78f));
            Panel(new Rect(290, 130, 700, 530), ink);
            if (showControls)
            {
                GUI.Label(new Rect(330, 163, 620, 70), "CONTROLS", title);
                GUI.Label(new Rect(330, 240, 620, 310),
                    "WASD - Move / Mouse - Look\n\nSpace - Jump / Shift - Dodge\n\nLeft mouse - Attack\n\nE - Interact" + (s.SlowTimeAvailable ? "\n\nQ - Slow time" : "") + "\n\nEsc - Pause / resume", body);
                if (GUI.Button(new Rect(330, 575, 620, 48), "Back", button)) showControls = false;
                return;
            }
            GUI.Label(new Rect(330, 163, 620, 70), s.Dead ? "THE DREAM REPEATS" : s.Paused ? "PAUSED" : s.landing ? "A FIRST STEP" : "THE UNANSWERED CALL", title);
            string description = s.Dead ? "Your body fell. The current stage is restarting." :
                s.Paused ? "Take a moment. The world and your abilities are paused." : s.landing ? "You helped someone instead of walking away. A small part of the shadow has lifted." :
                s.Chase != null ? "A fictional nightmare. The guardian is behind you. Run forward, take the core to its socket, and escape through the gate. Being caught restarts the stage." :
                "A fictional dream about responsibility. Defeat the manifestation, restore the crossing, and help the figure who is still waiting. Combat alone cannot open the exit.";
            GUI.Label(new Rect(330, 240, 620, 105), description, body);
            if (!s.Started && !SliceSession.InstructionsSeen)
                GUI.Label(new Rect(330, 345, 620, 65), "WASD + mouse / E interact / Left-click attack\nSpace jump / Shift dodge / Esc pause", small);
            if (!s.Dead && GUI.Button(new Rect(330, 420, 300, 48), s.Started ? "Resume" : "Begin", button)) s.Begin();
            if (GUI.Button(new Rect(s.Dead ? 330 : 650, 420, 300, 48), "Restart chamber", button)) s.Restart();
            if (GUI.Button(new Rect(330, 487, 300, 48), "Controls", button)) showControls = true;
            if (GUI.Button(new Rect(650, 487, 300, 48), s.Feedback.Muted ? "Sound: OFF" : "Sound: ON", button)) s.Feedback.Muted = !s.Feedback.Muted;
            if (!s.Dead && GUI.Button(new Rect(330, 553, 300, 48), "Settings", button)) Polish.PolishSettings.Open();
            if (GUI.Button(new Rect(650, 553, 300, 48), "Quit game", button))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            GUI.Label(new Rect(330, 620, 620, 30), "A fictional dream about responsibility and change.", small);
        }
    }
}

