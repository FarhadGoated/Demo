using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HellSlice.Polish
{
    /// <summary>
    /// "Cinematic Minimal" UI kit (approved design 5): Bebas Neue display type, Barlow body type,
    /// one ember accent, hairlines, skewed bar segments and key-cap hints. Shapes are generated in code.
    /// </summary>
    public static class PolishCine
    {
        public static readonly Color Accent = new Color(1f, .416f, .165f);          // #ff6a2a
        public static readonly Color TextMain = Color.white;
        public static readonly Color TextDim = new Color(.6f, .6f, .6f);
        public static readonly Color TextMuted = new Color(.44f, .44f, .44f);
        public static readonly Color Hairline = new Color(1, 1, 1, .14f);
        public static readonly Color BarOff = new Color(1, 1, 1, .18f);

        static readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        static Sprite white, hGradient, vFade, segment, arrowL, arrowR, keycap;

        public static Font Display => F("BebasNeue-Regular");
        public static Font Body => F("Barlow-SemiBold");
        public static Font BodyMedium => F("Barlow-Medium");
        public static Font BodyBold => F("Barlow-Bold");

        static Font F(string n)
        {
            if (!fonts.TryGetValue(n, out var f) || f == null)
            {
                f = Resources.Load<Font>("PolishFonts/" + n);
                if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                fonts[n] = f;
            }
            return f;
        }

        // ---------- generated shapes ----------
        delegate float Alpha(float x, float y);
        static Sprite Make(int w, int h, Alpha a, Vector4 border = default)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "PolishCine" };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = new Color(1, 1, 1, Mathf.Clamp01(a((x + .5f) / w, (y + .5f) / h)));
            t.SetPixels(px); t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, w, h), Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect, border);
        }
        public static Sprite White => white != null ? white : white = Make(4, 4, (x, y) => 1, new Vector4(1, 1, 1, 1));
        /// <summary>Opaque at left, transparent at right.</summary>
        public static Sprite HGradient => hGradient != null ? hGradient : hGradient = Make(256, 4, (x, y) => 1 - x);
        /// <summary>Fades in and out vertically (for the vertical hairline).</summary>
        public static Sprite VFade => vFade != null ? vFade : vFade = Make(4, 256, (x, y) => Mathf.Sin(y * Mathf.PI));
        /// <summary>Parallelogram bar segment, skewed like the mockup.</summary>
        public static Sprite Segment => segment != null ? segment : segment = Make(64, 32, (x, y) =>
        {
            float skew = .32f; float left = skew * y, right = 1 - skew * (1 - y);
            return Mathf.Clamp01(Mathf.Min(x - left, right - x) * 64);
        });
        public static Sprite ArrowLeft => arrowL != null ? arrowL : arrowL = Make(48, 48, (x, y) => Mathf.Clamp01((x - Mathf.Abs(y - .5f) * 1.3f - .18f) * 48) * (x < .82f ? 1 : 0));
        public static Sprite ArrowRight => arrowR != null ? arrowR : arrowR = Make(48, 48, (x, y) => Mathf.Clamp01(((1 - x) - Mathf.Abs(y - .5f) * 1.3f - .18f) * 48) * (x > .18f ? 1 : 0));
        /// <summary>Rounded outline for key caps (9-sliced).</summary>
        public static Sprite Keycap => keycap != null ? keycap : keycap = Make(48, 48, (x, y) =>
        {
            float px = x * 48, py = y * 48, r = 8, inset = 1.5f;
            float dx = Mathf.Max(Mathf.Max(r + inset - px, px - (48 - r - inset)), 0);
            float dy = Mathf.Max(Mathf.Max(r + inset - py, py - (48 - r - inset)), 0);
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float outer = r - d; // distance inside rounded rect
            return Mathf.Clamp01(outer + 1) * Mathf.Clamp01(2.5f - outer);
        }, new Vector4(14, 14, 14, 14));

        // ---------- builders ----------
        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot ?? anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size; return rt;
        }
        public static RectTransform Stretch(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; return rt;
        }
        public static Image Img(Transform t, Sprite s, Color c, bool raycast = false, bool sliced = false)
        {
            var i = t.gameObject.AddComponent<Image>(); i.sprite = s; i.color = c; i.raycastTarget = raycast;
            if (sliced) i.type = Image.Type.Sliced; return i;
        }
        public static Text Label(Transform t, string text, Font font, int size, Color color, TextAnchor align, float spacing = 0)
        {
            var tx = t.gameObject.AddComponent<Text>();
            tx.font = font; tx.text = text; tx.fontSize = size; tx.color = color; tx.alignment = align; tx.raycastTarget = false;
            tx.horizontalOverflow = HorizontalWrapMode.Overflow; tx.verticalOverflow = VerticalWrapMode.Overflow;
            if (spacing != 0) t.gameObject.AddComponent<UILetterSpacing>().Spacing = spacing;
            var sh = t.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, .6f); sh.effectDistance = new Vector2(0, -2);
            return tx;
        }
    }

    /// <summary>Tracking (letter spacing) for legacy uGUI Text; alignment-aware, per line.</summary>
    [RequireComponent(typeof(Text))]
    public sealed class UILetterSpacing : BaseMeshEffect
    {
        public float Spacing;
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || Spacing == 0 || vh.currentVertCount < 4) return;
            int quads = vh.currentVertCount / 4;
            var v = new UIVertex();
            var line = new int[quads]; var index = new int[quads];
            var counts = new List<int>();
            float prevX = float.MinValue, prevY = float.MaxValue; int l = -1, k = 0;
            for (int q = 0; q < quads; q++)
            {
                vh.PopulateUIVertex(ref v, q * 4);
                float x = v.position.x, y = v.position.y;
                if (l < 0 || x < prevX - 1f) { l++; counts.Add(0); k = 0; } // x jumping back = next line
                line[q] = l; index[q] = k++; counts[l]++; prevX = x; prevY = y;
            }
            var anchor = ((Text)graphic).alignment;
            float factor = anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.LowerCenter ? .5f
                : anchor == TextAnchor.UpperRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.LowerRight ? 1f : 0f;
            for (int q = 0; q < quads; q++)
            {
                float shift = index[q] * Spacing - (counts[line[q]] - 1) * Spacing * factor;
                for (int i = 0; i < 4; i++) { vh.PopulateUIVertex(ref v, q * 4 + i); v.position.x += shift; vh.SetUIVertex(v, q * 4 + i); }
            }
        }
    }
}
