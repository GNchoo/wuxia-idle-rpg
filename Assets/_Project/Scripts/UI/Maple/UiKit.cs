using System.Collections;
using IdleMvp.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// Maple Idle–style design tokens + layout primitives.
    /// Dark translucent HUD chips, white modals, teal selection, orange CTA.
    /// </summary>
    public static class UiKit
    {
        public const float Space1 = 8f;
        public const float Space2 = 12f;
        public const float Space3 = 16f;
        public const float Space4 = 24f;

        public const int FontH1 = 32;
        public const int FontH2 = 25;
        public const int FontBody = 20;
        public const int FontCaption = 15;

        // TMP type scale (new screens; kit-derived, landscape 1080p)
        public const int TmpTitle = 34;
        public const int TmpHeader = 28;
        public const int TmpBody = 22;
        public const int TmpCaption = 17;

        // Maple Idle palette
        public static readonly Color Accent = new Color(0.188f, 0.816f, 0.808f, 1f);         // kit teal #30D0CE
        public static readonly Color Primary = new Color(0.35f, 0.82f, 0.45f, 1f);            // kit green CTA
        public static readonly Color Neutral = new Color(0.94f, 0.94f, 0.97f, 1f);
        public static readonly Color NeutralDark = new Color(0.32f, 0.34f, 0.40f, 1f);
        public static readonly Color Danger = new Color(0.90f, 0.30f, 0.30f, 1f);
        public static readonly Color Positive = new Color(0.45f, 0.88f, 0.35f, 1f);
        public static readonly Color Selected = Accent;

        public static readonly Color Surface = new Color(0.98f, 0.98f, 0.99f, 0.96f);     // white modal
        public static readonly Color SurfaceAlt = new Color(0.93f, 0.94f, 0.96f, 1f);
        public static readonly Color HudChip = new Color(0.08f, 0.09f, 0.12f, 0.72f);     // floating dark chip
        public static readonly Color HudStrip = new Color(0.08f, 0.09f, 0.12f, 0.82f);    // bottom dock
        public static readonly Color HeaderDark = new Color(0.18f, 0.19f, 0.22f, 1f);
        public static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color CardDark = new Color(0.14f, 0.18f, 0.22f, 1f);       // dark teal cards

        public static readonly Color TextPrimary = new Color(0.16f, 0.18f, 0.22f, 1f);
        public static readonly Color TextSecondary = new Color(0.46f, 0.49f, 0.55f, 1f);
        public static readonly Color TextInverse = new Color(0.24f, 0.15f, 0.09f, 1f);   // 무협 킷: 양피지 위 먹갈색
        public static readonly Color TextInverseDim = new Color(0.33f, 0.23f, 0.15f, 0.92f);
        public static readonly Color TextAccent = new Color(0.188f, 0.816f, 0.808f, 1f); // kit teal

        public static readonly Color GoldColor = new Color(1f, 0.85f, 0.28f, 1f);
        public static readonly Color GemColor = new Color(0.95f, 0.35f, 0.42f, 1f);
        public static readonly Color HpColor = new Color(0.95f, 0.38f, 0.55f, 1f);
        public static readonly Color MpColor = new Color(0.35f, 0.65f, 0.98f, 1f);
        public static readonly Color ExpColor = new Color(0.55f, 0.92f, 0.28f, 1f);
        public static readonly Color BarTrack = new Color(0.05f, 0.05f, 0.07f, 0.75f);

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            KoreanUiFont.Apply(t);
            t.text = text ?? "";
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            // Commercial-ish edge without TMP: soft outline.
            var outline = t.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            outline.effectDistance = new Vector2(1.1f, -1.1f);
            outline.useGraphicAlpha = true;
            return t;
        }

        // ---- TMP text (new screens) -------------------------------------------------

        static TMP_FontAsset _tmpFont;

        public static TMP_FontAsset TmpFont
        {
            get
            {
                if (_tmpFont == null)
                {
                    _tmpFont = Resources.Load<TMP_FontAsset>("Fonts/UIHangulSDF");
                    if (_tmpFont == null) _tmpFont = KoreanTmpFont.Get();
                }
                return _tmpFont;
            }
        }

        // Outline lives on the font asset's own material (baked in-editor) —
        // ponytail: material copies go stale when the dynamic atlas regenerates.
        /// <summary>Signature-compatible shim so legacy Label call sites convert by rename.</summary>
        public static TextMeshProUGUI TmpLabel(Transform parent, string name, string text, int size, Color color,
            FontStyle style, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            bool bold = style == FontStyle.Bold || style == FontStyle.BoldAndItalic;
            return TmpLabel(parent, name, text, size, color, bold, MapAnchor(anchor));
        }

        static TextAlignmentOptions MapAnchor(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.MidlineLeft;
            }
        }

        public static TextMeshProUGUI TmpLabel(Transform parent, string name, string text, int size, Color color,
            bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, bool heavyOutline = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            var font = TmpFont;
            if (font != null)
                t.font = font;
            t.text = text ?? "";
            t.fontSize = size;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.color = color;
            t.alignment = align;
            t.enableWordWrapping = true;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Image Img(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image Sprite(Transform parent, string name, Sprite sprite, bool sliced = false)
        {
            var img = Img(parent, name, Color.white);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
                img.preserveAspect = !sliced;
            }
            img.raycastTarget = false;
            return img;
        }

        public static RectTransform VStack(Transform parent, string name, float spacing,
            float padL = 0, float padR = 0, float padT = 0, float padB = 0,
            TextAnchor align = TextAnchor.UpperCenter, bool forceExpandHeight = false)
        {
            var rt = Rect(parent, name);
            var g = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset((int)padL, (int)padR, (int)padT, (int)padB);
            g.childAlignment = align;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = forceExpandHeight;
            return rt;
        }

        public static RectTransform HStack(Transform parent, string name, float spacing,
            float padL = 0, float padR = 0, float padT = 0, float padB = 0,
            TextAnchor align = TextAnchor.MiddleLeft, bool forceExpandWidth = false)
        {
            var rt = Rect(parent, name);
            var g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset((int)padL, (int)padR, (int)padT, (int)padB);
            g.childAlignment = align;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = forceExpandWidth;
            g.childForceExpandHeight = true;
            return rt;
        }

        public static void Fill(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public static RectTransform Grid(Transform parent, string name, Vector2 cell, Vector2 spacing,
            int columns, TextAnchor align = TextAnchor.UpperLeft)
        {
            var rt = Rect(parent, name);
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = cell;
            g.spacing = spacing;
            g.childAlignment = align;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            return rt;
        }

        /// <summary>
        /// Grid that fills pane width. Prefer availableWidth=0 — <see cref="FillGridFit"/>
        /// relayouts from the real Rect once the modal is visible (avoids L/R clipping).
        /// </summary>
        public static RectTransform FillGrid(Transform parent, string name, Vector2 preferredCell, Vector2 spacing,
            int minCols, int maxCols, float availableWidth = 0f, TextAnchor align = TextAnchor.UpperLeft)
        {
            minCols = Mathf.Max(1, minCols);
            maxCols = Mathf.Max(minCols, maxCols);
            float aw = availableWidth;
            if (aw < 80f)
            {
                var prt = parent as RectTransform;
                if (prt != null && prt.rect.width > 80f) aw = prt.rect.width;
                else aw = preferredCell.x * minCols + spacing.x * Mathf.Max(0, minCols - 1);
            }
            ComputeFillGrid(aw, preferredCell, spacing, minCols, maxCols, out int cols, out Vector2 cell);
            var rt = Grid(parent, name, cell, spacing, cols, align);
            var fit = rt.gameObject.GetComponent<FillGridFit>() ?? rt.gameObject.AddComponent<FillGridFit>();
            fit.PreferredCell = preferredCell;
            fit.Spacing = spacing;
            fit.MinCols = minCols;
            fit.MaxCols = maxCols;
            fit.SafetyPad = 10f;
            return rt;
        }

        /// <summary>Recompute FillGrid cell sizes after layout knows real width.</summary>
        public static void RelayoutFillGrid(RectTransform gridRt, Vector2 preferredCell, Vector2 spacing,
            int minCols, int maxCols, float availableWidth = 0f)
        {
            if (gridRt == null) return;
            var g = gridRt.GetComponent<GridLayoutGroup>();
            if (g == null) return;
            float aw = availableWidth;
            if (aw < 40f) aw = gridRt.rect.width;
            if (aw < 40f)
            {
                var parent = gridRt.parent as RectTransform;
                if (parent != null) aw = Mathf.Max(40f, parent.rect.width - 16f);
            }
            if (aw < 40f) return;

            minCols = Mathf.Max(1, minCols);
            maxCols = Mathf.Max(minCols, maxCols);
            ComputeFillGrid(aw, preferredCell, spacing, minCols, maxCols, out int cols, out Vector2 cell);
            g.constraintCount = cols;
            g.cellSize = cell;
            g.spacing = spacing;

            // SkillTile etc. bake LayoutElement min/preferred to the old cell — sync or they overflow the mask.
            for (int i = 0; i < gridRt.childCount; i++)
            {
                var le = gridRt.GetChild(i).GetComponent<LayoutElement>();
                if (le == null) continue;
                le.minWidth = cell.x;
                le.minHeight = cell.y;
                le.preferredWidth = cell.x;
                le.preferredHeight = cell.y;
            }
        }

        public static void RelayoutAllFillGrids(Transform root)
        {
            if (root == null) return;
            var fits = root.GetComponentsInChildren<FillGridFit>(true);
            for (int i = 0; i < fits.Length; i++)
                fits[i].ApplyNow();
        }

        static void ComputeFillGrid(float availableWidth, Vector2 preferredCell, Vector2 spacing,
            int minCols, int maxCols, out int cols, out Vector2 cell)
        {
            float aw = Mathf.Max(60f, availableWidth);
            float gapX = spacing.x;
            float aspect = preferredCell.y / Mathf.Max(1f, preferredCell.x);
            cols = minCols;
            cell = preferredCell;
            bool found = false;
            // Prefer more columns when they still fit — denser and less empty gutter.
            for (int c = maxCols; c >= minCols; c--)
            {
                float totalGap = gapX * Mathf.Max(0, c - 1);
                float cellW = (aw - totalGap) / c;
                if (cellW < preferredCell.x * 0.62f) continue;
                // Keep preferred height when width shrinks so action-strip tiles are not crushed.
                float cellH = Mathf.Max(preferredCell.y, cellW * aspect * 0.5f);
                if (cellW <= preferredCell.x * 1.35f)
                {
                    cols = c;
                    cell = new Vector2(cellW, cellH);
                    found = true;
                    break;
                }
            }
            if (!found && (aw - gapX * Mathf.Max(0, maxCols - 1)) / maxCols > preferredCell.x)
            {
                // Container wider than maxCols can use — cap cell size instead of ballooning.
                cols = maxCols;
                cell = new Vector2(preferredCell.x * 1.35f, Mathf.Max(preferredCell.y, preferredCell.x * 1.35f * aspect * 0.5f));
                found = true;
            }
            if (!found)
            {
                cols = minCols;
                float totalGap = gapX * Mathf.Max(0, cols - 1);
                float cellW = Mathf.Max(56f, (aw - totalGap) / cols);
                cell = new Vector2(cellW, Mathf.Max(preferredCell.y, cellW * aspect * 0.5f));
            }
        }

        public static LayoutElement Fix(Component c, float w = -1f, float h = -1f)
        {
            var le = c.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            return le;
        }

        public static LayoutElement Flex(Component c, float weight = 1f)
        {
            var le = c.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = weight;
            le.flexibleHeight = weight;
            return le;
        }

        public static RectTransform Spacer(Transform parent, float weight = 1f)
        {
            var rt = Rect(parent, "Spacer");
            Flex(rt, weight);
            return rt;
        }

        public static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2? offMin = null, Vector2? offMax = null)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offMin ?? Vector2.zero;
            rt.offsetMax = offMax ?? Vector2.zero;
        }

        public static void CenterSize(RectTransform rt, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
        }

        public static string Num(double v)
        {
            double a = System.Math.Abs(v);
            if (a >= 1e12) return Trim(v / 1e12) + "조";
            if (a >= 1e8) return Trim(v / 1e8) + "억";
            if (a >= 1e4) return Trim(v / 1e4) + "만";
            return v.ToString("N0");
        }

        static string Trim(double v)
        {
            if (v >= 100) return v.ToString("0");
            if (v >= 10) return v.ToString("0.#");
            return v.ToString("0.##");
        }

        public static void Press(Button b)
        {
            if (b == null) return;
            if (b.gameObject.GetComponent<UiPressEffect>() == null)
                b.gameObject.AddComponent<UiPressEffect>();
        }

        public static void SetEnabled(Button b, bool on)
        {
            if (b == null) return;
            b.interactable = on;
            var img = b.GetComponent<Image>();
            if (img != null)
            {
                var c = img.color;
                c.a = on ? 1f : 0.45f;
                img.color = c;
            }
            var t = b.GetComponentInChildren<Text>();
            if (t != null)
            {
                var c = t.color;
                c.a = on ? 1f : 0.5f;
                t.color = c;
            }
            var tmp = b.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                var c = tmp.color;
                c.a = on ? 1f : 0.5f;
                tmp.color = c;
            }
        }
    }

    public class UiPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Vector3 _base;
        bool _init;

        void EnsureInit()
        {
            if (_init) return;
            _base = transform.localScale;
            _init = true;
        }

        public void OnPointerDown(PointerEventData e)
        {
            EnsureInit();
            transform.localScale = _base * 0.95f;
        }

        public void OnPointerUp(PointerEventData e)
        {
            EnsureInit();
            transform.localScale = _base;
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_init) transform.localScale = _base;
        }
    }

    public class UiModalAnimator : MonoBehaviour
    {
        CanvasGroup _cg;

        public void PlayOpen()
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
            StopAllCoroutines();
            StartCoroutine(OpenCo());
        }

        IEnumerator OpenCo()
        {
            const float dur = 0.15f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                float e = 1f - (1f - u) * (1f - u);
                _cg.alpha = e;
                transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.94f, 1f, e);
                yield return null;
            }
            _cg.alpha = 1f;
            transform.localScale = Vector3.one;
        }
    }

    public class UiToast : MonoBehaviour
    {
        static UiToast _inst;
        Text _text;
        CanvasGroup _cg;
        Coroutine _co;

        public static void Ensure(Transform canvasRoot)
        {
            if (_inst != null) return;
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(canvasRoot, false);
            var rt = go.GetComponent<RectTransform>();
            // 화면 중앙(0.42)에 띄우면 전투/모달을 가린다 → 하단 네비 위쪽으로 내린다
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.22f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(640f, 56f);

            _inst = go.AddComponent<UiToast>();
            _inst._cg = go.GetComponent<CanvasGroup>();
            _inst._cg.alpha = 0f;
            _inst._cg.blocksRaycasts = false;

            var bg = UiKit.Img(go.transform, "Bg", new Color(0.06f, 0.07f, 0.10f, 0.92f));
            var toastSp = CasualArt.C("ToastMessage_Topbar_White") ?? CasualArt.CardRound;
            if (toastSp != null) { bg.sprite = toastSp; bg.type = Image.Type.Sliced; }
            UiKit.Fill(bg.rectTransform);
            bg.raycastTarget = false;

            _inst._text = UiKit.Label(go.transform, "Msg", "", UiKit.FontBody, UiKit.TextInverse,
                FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fill(_inst._text.rectTransform, 12f);
        }

        public static void Show(string msg)
        {
            if (_inst == null || string.IsNullOrEmpty(msg)) return;
            _inst.transform.SetAsLastSibling();
            _inst._text.text = msg;
            if (_inst._co != null) _inst.StopCoroutine(_inst._co);
            _inst._co = _inst.StartCoroutine(_inst.Run());
        }

        IEnumerator Run()
        {
            float t = 0f;
            while (t < 0.12f)
            {
                t += Time.unscaledDeltaTime;
                _cg.alpha = Mathf.Clamp01(t / 0.12f);
                yield return null;
            }
            _cg.alpha = 1f;
            yield return new WaitForSecondsRealtime(1.6f);
            t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                _cg.alpha = 1f - Mathf.Clamp01(t / 0.3f);
                yield return null;
            }
            _cg.alpha = 0f;
        }
    }

    /// <summary>
    /// Keeps a FillGrid's cellSize within the live Rect width so tiles are not clipped by ScrollRect masks.
    /// </summary>
    public sealed class FillGridFit : MonoBehaviour
    {
        public Vector2 PreferredCell = new Vector2(160f, 200f);
        public Vector2 Spacing = new Vector2(10f, 10f);
        public int MinCols = 2;
        public int MaxCols = 4;
        public float SafetyPad = 10f;
        bool _queued;

        void OnEnable() => Queue();

        void OnRectTransformDimensionsChange() => Queue();

        void Queue()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
            if (_queued) return;
            _queued = true;
            StartCoroutine(CoApply());
        }

        IEnumerator CoApply()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            _queued = false;
            ApplyNow();
        }

        public void ApplyNow()
        {
            var rt = transform as RectTransform;
            if (rt == null) return;
            float aw = rt.rect.width;
            if (aw < 40f)
            {
                var p = rt.parent as RectTransform;
                if (p != null) aw = p.rect.width;
            }
            aw = Mathf.Max(60f, aw - SafetyPad);
            UiKit.RelayoutFillGrid(rt, PreferredCell, Spacing, MinCols, MaxCols, aw);
        }
    }
}
