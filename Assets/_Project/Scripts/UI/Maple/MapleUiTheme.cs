using IdleMvp.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    public class StatRowView
    {
        public GameObject Go;
        public TMP_Text Label;
        public TMP_Text Value;
        public TMP_Text Level;
        public TMP_Text Bonus;
        public TMP_Text Cost;
        public Button Action;
        public BarView Progress;
    }

    public class ItemCardView
    {
        public GameObject Go;
        public Image Icon;
        public TMP_Text Title;
        public TMP_Text Sub;
        public Button Button;
        public TMP_Text ButtonLabel;
        /// <summary>Bottom HStack for tile actions (lock/deploy). Null if unused.</summary>
        public RectTransform ActionStrip;
    }

    public class TabBarView
    {
        public GameObject Go;
        public Button[] Tabs;
        Image[] _bgs;
        Image[] _underlines;
        Text[] _labels;
        int _selected = -1;

        public TabBarView(Button[] tabs, Image[] bgs, Image[] underlines, Text[] labels)
        {
            Tabs = tabs;
            _bgs = bgs;
            _underlines = underlines;
            _labels = labels;
        }

        public void Select(int idx)
        {
            _selected = idx;
            var tabOn = GrowArt.TabOn;
            var tabOff = GrowArt.TabOff;
            for (int i = 0; i < Tabs.Length; i++)
            {
                bool on = i == idx;
                if (_bgs[i] != null)
                {
                    if (tabOn != null && tabOff != null)
                    {
                        // Menu_TopBtn_Focus는 9슬라이스 세로 보더가 0(y:0,w:0)이라 탭 높이로 늘리면
                        // 마름모처럼 찌그러진다. 4면 슬라이스되는 Menu_TopBtn을 양쪽 상태에 쓰고
                        // 선택 여부는 '색'으로만 구분한다.
                        _bgs[i].sprite = tabOff;
                        _bgs[i].type = Image.Type.Sliced;
                        _bgs[i].pixelsPerUnitMultiplier = 1f;
                        // 키트 탭 원본은 흰색이다. white로 두면 파란 페이지 위에 흰 띠가 남으므로
                        // 선택=밝은 하늘, 비선택=짙은 남색으로 틴트해 블루 테마에 맞춘다.
                        _bgs[i].color = on
                            ? new Color(0.26f, 0.62f, 0.98f, 1f)
                            : new Color(0.10f, 0.17f, 0.36f, 0.95f);
                        if (_labels[i] != null)
                            _labels[i].color = on
                                ? new Color(1f, 1f, 1f, 1f)
                                : new Color(0.72f, 0.79f, 0.90f, 1f);
                    }
                    else
                    {
                        _bgs[i].color = on ? new Color(UiKit.Accent.r, UiKit.Accent.g, UiKit.Accent.b, 0.35f) : new Color(0f, 0f, 0f, 0.12f);
                    }
                }
                if (_underlines[i] != null) _underlines[i].enabled = on && tabOn == null;
                if (_labels[i] != null && (tabOn == null || tabOff == null))
                    _labels[i].color = on ? UiKit.Accent : UiKit.TextInverse;
            }
        }

        public int Selected => _selected;
    }

    public class BarView
    {
        public GameObject Go;
        public Image Fill;
        public TMP_Text Label;
        public float Inset = 4f;

        public void Set(float pct, string text = null)
        {
            if (Fill != null)
            {
                float p = Mathf.Clamp01(pct);
                var fr = Fill.rectTransform;
                fr.anchorMin = Vector2.zero;
                fr.anchorMax = new Vector2(p, 1f);
                fr.offsetMin = new Vector2(Inset, Inset);
                fr.offsetMax = new Vector2(p >= 0.999f ? -Inset : 0f, -Inset);
                // Keep Sliced/Simple — never Type.Filled (capsule sprites taper into a needle).
                if (Fill.type == Image.Type.Filled)
                    Fill.type = Fill.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            }
            if (Label != null && text != null) Label.text = text;
        }
    }

    public class ChipView
    {
        public GameObject Go;
        public TMP_Text Value;
    }

    /// <summary>Left-rail combat power + effective ATK/HP/DEF (Maple Idle style).</summary>
    public class StatSummaryView
    {
        public GameObject Go;
        public Image Avatar;
        public TMP_Text Cp;
        public TMP_Text Atk;
        public TMP_Text Hp;
        public TMP_Text Def;
        public Text Extra;
        public LayoutElement RootLayout;
        public float BasePreferredHeight;

        public void SetAvatar(Sprite sprite, Color? tint = null)
        {
            if (Avatar == null) return;
            if (sprite != null) Avatar.sprite = sprite;
            Avatar.color = tint ?? Color.white;
        }

        public void RefreshFromCombat()
        {
            var bd = IdleMvp.Progression.CombatPowerService.GetBreakdown();
            if (Cp != null) Cp.text = UiKit.Num(bd.TotalCp);
            if (Atk != null) Atk.text = bd.Atk.ToString("0.#");
            if (Hp != null) Hp.text = bd.MaxHp.ToString("0");
            if (Def != null) Def.text = bd.Def.ToString("0.#");
        }

        public void SetExtra(string text)
        {
            if (Extra == null) return;
            Extra.text = text ?? "";
            var host = Extra.transform.parent != null ? Extra.transform.parent.gameObject : Extra.gameObject;
            bool show = !string.IsNullOrEmpty(text);
            var le = host.GetComponent<LayoutElement>() ?? host.AddComponent<LayoutElement>();
            if (show)
            {
                host.SetActive(true);
                le.ignoreLayout = false;
                le.minHeight = 52f;
                le.preferredHeight = 56f;
                le.flexibleHeight = 0f;
            }
            else
            {
                Extra.text = "";
                le.minHeight = 0f;
                le.preferredHeight = 0f;
                le.flexibleHeight = 0f;
                host.SetActive(false);
            }
            SyncPreferredHeight(show);
        }

        public void SyncPreferredHeight(bool extraVisible)
        {
            if (RootLayout == null) return;
            float h = BasePreferredHeight + (extraVisible ? 64f : 0f);
            RootLayout.minHeight = h;
            RootLayout.preferredHeight = h;
            RootLayout.flexibleHeight = 0f;
        }
    }

    /// <summary>Kit Popup_Skill-style detail block (code-built).</summary>
    public class SkillDetailView
    {
        public GameObject Go;
        public Image Icon;
        public TMP_Text Title;
        public TMP_Text Rank;
        public TMP_Text Desc;
        public TMP_Text Effect;
        public TMP_Text Cost;
        public TMP_Text Reason;
        public BarView LevelBar;
        public Button Action;
        public TMP_Text ActionLabel;
    }

    /// <summary>
    /// Kit-faithful theme: dark Popup_Bg panels, green/mint CTA, PPU=1 sliced frames
    /// (matches Fantasy Idle DemoScene — never inflate pixelsPerUnitMultiplier).
    /// </summary>
    public static class MapleUiTheme
    {
        /// <summary>Kit DemoScene always uses pixelsPerUnitMultiplier = 1.</summary>
        static bool Skin(Image img, Sprite sprite, float ppu = 1f, Color? tint = null)
        {
            return FantasyKitSlots.Slice(img, sprite, tint);
        }

        public static void PanelShadow(GameObject go)
        {
            var s = go.GetComponent<Shadow>();
            if (s == null) s = go.AddComponent<Shadow>();
            s.effectColor = new Color(0f, 0f, 0f, 0.28f);
            s.effectDistance = new Vector2(0f, -4f);
            s.useGraphicAlpha = true;
        }

        public static void FieldTextOutline(Text t)
        {
            if (t == null) return;
            var o = t.GetComponent<Outline>();
            if (o == null) o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.75f);
            o.effectDistance = new Vector2(1.2f, -1.2f);
            o.useGraphicAlpha = true;
        }

        /// <summary>How much content must inset from Popup_Bg / rect rim so L/R strokes stay visible.</summary>
        public const float WindowFrameInset = 20f;
        /// <summary>Extra side inset for the title bar so sharp header corners clear Popup_Bg round corners.</summary>
        public const float WindowHeaderSideInset = 36f;
        public const float WindowRimThickness = 5f;

        /// <summary>Outer modal shell — crisp rounded rectangle (no cartoon blob sprites).</summary>
        public static Image Panel(Transform parent, string name, bool shadow = true)
        {
            var img = UiKit.Img(parent, name, Color.white);
            // 무협 스킨: 창은 통짜 일러스트(매달린 나무 보드) — 9-slice 조립이 아니라 한 장의 물체
            var wux = CasualArt.C("wux_window_large");
            var kit = wux ?? CasualArt.WoodBoard ?? CasualArt.C("Popup_FullWidth03_Single_Navy");
            if (kit != null)
            {
                img.sprite = kit;
                img.type = wux != null ? Image.Type.Simple : Image.Type.Sliced;
                img.preserveAspect = false;
                img.color = Color.white;
            }
            else
            {
                img.sprite = MapleLightTheme.RoundedSprite(16);
                img.type = Image.Type.Sliced;
                img.color = new Color(0.07f, 0.13f, 0.30f, 0.99f);
                if (shadow) PanelShadow(img.gameObject);
            }
            return img;
        }

        /// <summary>Inner dual-pane panel — list frame with real borders (not nested Popup_Bg).</summary>
        public static Image InnerPanel(Transform parent, string name)
        {
            var img = UiKit.Img(parent, name, FantasyKitSlots.KitPanel);
            FantasyKitSlots.FrameList(img, 220f);
            return img;
        }

        /// <summary>Dark HUD chip — kit rounded card when no explicit color requested.</summary>
        public static Image Chip(Transform parent, string name, Color? color = null)
        {
            var img = UiKit.Img(parent, name, color ?? UiKit.HudChip);
            // 색만 지정하면 스프라이트 없는 맹물 사각형이 된다. 항상 키트 카드 프레임을 깐다.
            img.sprite = CasualArt.CardRound != null ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(12);
            img.type = Image.Type.Sliced;
            if (color == null) img.color = new Color(0.10f, 0.11f, 0.15f, 0.85f);
            return img;
        }

        public static Image Strip(Transform parent, string name) => Chip(parent, name, UiKit.HudStrip);

        /// <summary>
        /// 알림 배지. 키트의 Alert_Dot(테두리+점)을 쓴다.
        /// 색만 칠한 네모는 품질이 떨어져 보이므로 직접 그리지 말 것.
        /// 반환된 Image의 enabled로 켜고 끄면 테두리도 같이 따라간다.
        /// </summary>
        public static Image AlertDot(Transform parent, string name = "Dot", float size = 16f)
        {
            // 테두리(흰 링)가 바깥, 점이 안쪽 — 어두운 배경에서도 또렷하게 보인다
            var border = UiKit.Img(parent, name, Color.white);
            border.sprite = CasualArt.AlertDotBorder;
            border.type = Image.Type.Simple;
            border.raycastTarget = false;
            border.rectTransform.sizeDelta = new Vector2(size, size);

            var dot = UiKit.Img(border.transform, "Fill", new Color(0.95f, 0.24f, 0.24f, 1f));
            dot.sprite = CasualArt.AlertDot;
            dot.type = Image.Type.Simple;
            dot.raycastTarget = false;
            UiKit.Fill(dot.rectTransform, size * 0.22f);

            // 스프라이트가 없으면(리소스 누락) 최소한 동그란 점으로라도 보이게
            if (border.sprite == null)
            {
                border.sprite = MapleLightTheme.RoundedSprite(999);
                border.color = new Color(1f, 1f, 1f, 0.9f);
                border.type = Image.Type.Sliced;
            }
            if (dot.sprite == null)
            {
                dot.sprite = MapleLightTheme.RoundedSprite(999);
                dot.type = Image.Type.Sliced;
            }
            return border;
        }

        static Button ButtonBase(Transform parent, string name, string label, Color bgTint, Color textColor, int fontSize, Sprite skin = null)
        {
            // GUI Pro casual candy button matched by tint; flat rounded rect fallback.
            var img = UiKit.Img(parent, name, bgTint);
            var kitKey = CasualArt.ButtonKeyForTint(bgTint);
            var kitBtn = kitKey != null ? CasualArt.Button(kitKey) : null;
            if (kitBtn != null)
            {
                img.sprite = kitBtn;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                // bright candy needs a dark label (kit demo: dark text on yellow Select)
                if (CasualArt.ButtonIsLight(kitKey))
                    textColor = new Color(0.09f, 0.17f, 0.10f, 1f);
            }
            else
            {
                img.sprite = MapleLightTheme.RoundedSprite(10);
                img.type = Image.Type.Sliced;
                img.color = bgTint;
            }
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.disabledColor = new Color(0.88f, 0.89f, 0.90f, 1f); // pale wash keeps any label color legible
            btn.colors = colors;
            btn.onClick.AddListener(IdleMvp.Core.AudioService.Click);
            UiKit.Press(btn);

            if (!string.IsNullOrEmpty(label))
            {
                var t = UiKit.TmpLabel(img.transform, "Label", label, fontSize, textColor,
                    bold: true, TextAlignmentOptions.Center);
                t.enableWordWrapping = false;
                UiKit.Fill(t.rectTransform, 6f);
            }
            // Default usable size for layout groups — callers override with UiKit.Fix.
            var ble = img.gameObject.GetComponent<LayoutElement>() ?? img.gameObject.AddComponent<LayoutElement>();
            if (ble.minWidth <= 0f) ble.minWidth = Mathf.Max(120f, (string.IsNullOrEmpty(label) ? 3 : label.Length) * fontSize * 0.9f + 48f);
            if (ble.minHeight <= 0f) ble.minHeight = 52f;
            // Image는 ILayoutElement라 스프라이트 원본 높이(CTA 195px)가 preferredHeight로 새어든다.
            // 명시하지 않으면 버튼이 그만큼 부풀어오르므로 기본값을 박아둔다 (Fix 호출 시 덮어씀).
            if (ble.preferredHeight <= 0f) ble.preferredHeight = 56f;
            return btn;
        }

        public static Button PrimaryButton(Transform parent, string name, string label, System.Action onClick, int fontSize = UiKit.FontBody)
        {
            // Kit primary = mint (equip / main CTA)
            var b = ButtonBase(parent, name, label, FantasyKitSlots.KitTeal, UiKit.TextInverse, fontSize, GrowArt.CtaButton);
            if (onClick != null) b.onClick.AddListener(() => onClick());
            return b;
        }

        public static Button AccentButton(Transform parent, string name, string label, System.Action onClick, int fontSize = UiKit.FontBody)
        {
            // Kit upgrade = green
            var b = ButtonBase(parent, name, label, UiKit.Positive, UiKit.TextInverse, fontSize, GrowArt.UpgradeButton);
            if (onClick != null) b.onClick.AddListener(() => onClick());
            return b;
        }

        /// <summary>Demo Select-style yellow CTA (dark label handled by ButtonBase).</summary>
        public static Button YellowButton(Transform parent, string name, string label, System.Action onClick, int fontSize = UiKit.FontBody)
        {
            var b = ButtonBase(parent, name, label, new Color(0.98f, 0.78f, 0.15f, 1f), UiKit.TextInverse, fontSize);
            if (onClick != null) b.onClick.AddListener(() => onClick());
            return b;
        }

        public static Button SecondaryButton(Transform parent, string name, string label, System.Action onClick, int fontSize = UiKit.FontBody)
        {
            var b = ButtonBase(parent, name, label, UiKit.NeutralDark, UiKit.TextInverse, fontSize, GrowArt.BtnNeutral);
            if (onClick != null) b.onClick.AddListener(() => onClick());
            return b;
        }

        public static Button IconButton(Transform parent, string name, string label, Sprite icon, System.Action onClick)
        {
            var img = UiKit.Img(parent, name, new Color(0f, 0f, 0f, 0.45f));
            if (!Skin(img, GrowArt.BtnNeutral))
                FantasyKitSlots.SharpPanel(img, img.color);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            UiKit.Press(btn);

            bool hasLabel = !string.IsNullOrEmpty(label);
            if (icon != null)
            {
                var ic = UiKit.Sprite(img.transform, "Icon", icon);
                var rt = ic.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, hasLabel ? 0.58f : 0.5f);
                rt.sizeDelta = hasLabel ? new Vector2(40f, 40f) : new Vector2(48f, 48f);
            }
            if (hasLabel)
            {
                var t = UiKit.TmpLabel(img.transform, "Label", label, UiKit.TmpCaption, UiKit.TextInverse,
                    bold: true, icon == null ? TextAlignmentOptions.Center : TextAlignmentOptions.Bottom);
                t.enableWordWrapping = false;
                UiKit.Fill(t.rectTransform, 3f);
            }
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static Text SectionHeader(Transform parent, string text)
        {
            var row = UiKit.HStack(parent, "Sec_" + text, UiKit.Space1, 0, 0, 4, 4);
            UiKit.Fix(row, -1f, 40f);
            var le = row.gameObject.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 40f;
            le.preferredHeight = 40f;
            // 통짜 창 일러스트 내부는 양피지 — 섹션 제목은 먹갈색
            var ink = new Color(0.30f, 0.18f, 0.09f, 1f);
            var t = UiKit.Label(row, "T", text, UiKit.FontH2, ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Flex(t);
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        /// <summary>Sub-container for section content (active/passive skill boxes, etc.).</summary>
        public static RectTransform SectionBox(Transform parent, string name, float minHeight = 0f)
        {
            // Same GO hosts Image + VLG + CSF so height follows children (no overflow into next section).
            var root = UiKit.VStack(parent, name, UiKit.Space2, 14, 14, 12, 14, TextAnchor.UpperLeft);
            var img = root.gameObject.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            img.sprite = CasualArt.CardRound != null ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(12);
            img.type = Image.Type.Sliced;
            // BasicFrame_Round12는 흰색 원본이라 white로 두면 파란 페이지 위에 흰 박스가 뜬다.
            // 형제 패널(InfoChip/FrameList/SkillDetailPanel)과 같은 남색으로 맞춘다.
            img.color = new Color(0.075f, 0.13f, 0.30f, 0.97f);
            img.raycastTarget = false;
            var le = root.gameObject.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = minHeight > 0f ? minHeight : 80f;
            var fit = root.gameObject.GetComponent<ContentSizeFitter>() ?? root.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return root;
        }

        public static Text InfoText(Transform parent, string text, int size = UiKit.FontBody)
        {
            var t = UiKit.Label(parent, "Info", text, size, new Color(1f, 1f, 1f, 0.74f), FontStyle.Normal, TextAnchor.UpperLeft);
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size * 1.6f;
            le.flexibleWidth = 1f;
            return t;
        }

        public static StatRowView StatRow(Transform parent, string name, string label, string actionLabel, System.Action onAction, Sprite icon = null)
        {
            // Delegate to kit Slot_Enhance layout (Frame_List + Frame_Img + green CTA).
            return FantasyKitSlots.EnhanceRow(parent, name, label, icon, actionLabel, onAction);
        }

        /// <summary>Dark teal core-stat card — kit Frame_Round_Black + green CTA.</summary>
        public static ItemCardView DarkStatCard(Transform parent, string name, string title, string value, string cta, System.Action onClick)
        {
            var card = UiKit.Img(parent, name, UiKit.CardDark);
            Skin(card, GrowArt.CardDark);
            var v = UiKit.VStack(card.transform, "V", 8f, 14, 14, 14, 14, TextAnchor.MiddleCenter);
            UiKit.Fill(v);

            var t = UiKit.TmpLabel(v, "Title", title, UiKit.TmpBody, UiKit.TextInverseDim,
                bold: true, TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            UiKit.Fix(t, -1f, 26f);
            var s = UiKit.TmpLabel(v, "Sub", value, UiKit.TmpTitle, FantasyKitSlots.KitTeal,
                bold: true, TextAlignmentOptions.Center);
            s.enableWordWrapping = false;
            UiKit.Fix(s, -1f, 44f);

            Button b = null;
            TMP_Text bl = null;
            if (cta != null)
            {
                b = AccentButton(v, "Cta", cta, onClick, UiKit.FontBody);
                UiKit.Fix(b, -1f, 64f);
                bl = b.GetComponentInChildren<TMP_Text>();
            }

            return new ItemCardView { Go = card.gameObject, Title = t, Sub = s, Button = b, ButtonLabel = bl };
        }

        public static ItemCardView ItemCard(Transform parent, string name, string title, string sub, string cta, System.Action onClick, Sprite iconSprite = null, Sprite frameOverride = null)
        {
            // Grid tiles (weapons / skills / gear) use Slot_Skill edge frames.
            if (frameOverride != null || (cta == null && iconSprite != null))
            {
                return FantasyKitSlots.SkillTile(parent, name, title, sub, iconSprite,
                    frameOverride != null ? frameOverride : GrowArt.Rarity(0), onClick, new Vector2(200f, 240f));
            }
            return FantasyKitSlots.PackageRow(parent, name, title, sub, iconSprite, cta, onClick,
                height: 140f);
        }

        public static TabBarView TabBar(Transform parent, string name, string[] labels, System.Action<int> onSelect, float height = 64f)
        {
            var bar = UiKit.Img(parent, name, FantasyKitSlots.KitPanel);
            Skin(bar, GrowArt.TabStrip);
            UiKit.Fix(bar, -1f, height);
            var h = UiKit.HStack(bar.transform, "H", 8f, 8, 8, 8, 8, TextAnchor.MiddleCenter, true);
            UiKit.Fill(h);

            int n = labels.Length;
            var tabs = new Button[n];
            var bgs = new Image[n];
            var lines = new Image[n];
            var texts = new Text[n];
            TabBarView view = null;

            for (int i = 0; i < n; i++)
            {
                int idx = i;
                var bg = UiKit.Img(h, "Tab" + i, new Color(0f, 0f, 0f, 0.08f));
                var btn = bg.gameObject.AddComponent<Button>();
                btn.targetGraphic = bg;
                UiKit.Press(btn);
                var t = UiKit.Label(bg.transform, "L", labels[i], UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleCenter);
                UiKit.Fill(t.rectTransform);
                t.rectTransform.offsetMax = new Vector2(0f, -4f);
                if (GrowArt.TabOn == null) FieldTextOutline(t); // kit tabs are light — no outline needed

                var line = UiKit.Img(bg.transform, "Under", FantasyKitSlots.KitTeal);
                var lr = line.rectTransform;
                lr.anchorMin = new Vector2(0.18f, 0f);
                lr.anchorMax = new Vector2(0.82f, 0f);
                lr.offsetMin = new Vector2(0f, 3f);
                lr.offsetMax = new Vector2(0f, 6f);
                line.enabled = false;
                line.raycastTarget = false;

                btn.onClick.AddListener(() =>
                {
                    view?.Select(idx);
                    onSelect?.Invoke(idx);
                });
                tabs[i] = btn;
                bgs[i] = bg;
                lines[i] = line;
                texts[i] = t;
            }

            view = new TabBarView(tabs, bgs, lines, texts);
            view.Go = bar.gameObject;
            view.Select(0);
            return view;
        }

        public static ChipView CurrencyChip(Transform parent, string name, Sprite icon, Color fallback)
        {
            var bg = UiKit.Img(parent, name, new Color(0.12f, 0.14f, 0.18f, 0.95f));
            if (CasualArt.ResourceBar != null)
            {
                bg.sprite = CasualArt.ResourceBar;
                bg.color = new Color(0.055f, 0.10f, 0.24f, 0.97f); // demo top-bar navy capsule
            }
            else
                bg.sprite = MapleLightTheme.RoundedSprite(10);
            bg.type = Image.Type.Sliced;
            UiKit.Fix(bg, -1f, 40f);
            var h = UiKit.HStack(bg.transform, "H", 4f, 8, 10, 4, 4, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            if (icon == null)
            {
                // kit currency icon fallback by chip name
                string kitIcon = name.Contains("Gold") ? "ResourceBar_Icon_Coin"
                    : name.Contains("Gem") || name.Contains("RD") ? "ResourceBar_Icon_Gem_Purple"
                    : name.Contains("Blue") ? "ResourceBar_Icon_Gem_Blue"
                    : name.Contains("Stone") || name.Contains("Sf") ? "ResourceBar_Icon_Energy"
                    : "ResourceBar_Icon_Gem_Green";
                icon = CasualArt.C(kitIcon);
            }
            if (icon != null)
            {
                var ic = UiKit.Sprite(h, "Icon", icon);
                ic.preserveAspect = true;
                UiKit.Fix(ic, 24f, 24f);
            }
            else
            {
                var dot = UiKit.Img(h, "Icon", fallback);
                UiKit.Fix(dot, 16f, 16f);
            }
            var val = UiKit.TmpLabel(h, "Val", "0", UiKit.TmpCaption, Color.white, bold: true);
            UiKit.Fix(val, -1f, 26f);
            val.enableWordWrapping = false;
            UiKit.Flex(val);
            return new ChipView { Go = bg.gameObject, Value = val };
        }

        public static BarView Bar(Transform parent, string name, Color fillColor, bool withLabel = true)
        {
            // GUI Pro slider track/fill (sliced, tintable); sharp rect fallback.
            var track = UiKit.Img(parent, name, UiKit.BarTrack);
            float inset = 3f;
            var fill = UiKit.Img(track.transform, "Fill", fillColor);
            if (CasualArt.BarBg != null && CasualArt.BarFillWhite != null)
            {
                track.sprite = CasualArt.BarBg;
                track.type = Image.Type.Sliced;
                track.color = Color.white;
                fill.sprite = CasualArt.BarFillWhite;
                fill.type = Image.Type.Sliced;
                inset = 2f;
            }
            else
            {
                track.sprite = null;
                track.type = Image.Type.Simple;
                fill.sprite = null;
                fill.type = Image.Type.Simple;
            }
            fill.color = fillColor;
            fill.raycastTarget = false;
            var fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(inset, inset);
            fr.offsetMax = new Vector2(-inset, -inset);

            TMP_Text label = null;
            if (withLabel)
            {
                label = UiKit.TmpLabel(track.transform, "T", "", UiKit.TmpCaption, UiKit.TextInverse,
                    bold: true, TextAlignmentOptions.Center);
                label.enableWordWrapping = false;
                UiKit.Fill(label.rectTransform);
            }
            var view = new BarView { Go = track.gameObject, Fill = fill, Label = label, Inset = inset };
            view.Set(1f);
            return view;
        }

        /// <summary>Content-top action strip (summon etc.) — keeps footer to 1–2 primaries.</summary>
        public static RectTransform ToolBar(Transform parent, string name)
        {
            var row = UiKit.HStack(parent, name, UiKit.Space2, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(row, -1f, 64f);
            return row;
        }

        /// <summary>Framed label|value summary line for left rails / wallets.</summary>
        public static StatRowView StatLine(Transform parent, string name, string label, Sprite icon, float height = 48f)
        {
            var card = UiKit.Img(parent, name, Color.white);
            float h = Mathf.Max(48f, height);
            var lineBanner = CasualArt.C("BannerFrame01_Single_Navy");
            if (lineBanner != null) { card.sprite = lineBanner; card.type = Image.Type.Sliced; }
            else { card.sprite = null; card.type = Image.Type.Simple; card.color = new Color(0.10f, 0.14f, 0.25f, 1f); }
            UiKit.Fix(card, -1f, h);
            StretchFullWidth(card);

            var row = UiKit.HStack(card.transform, "H", 8f, 12, 14, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(row);

            if (icon != null)
            {
                var ic = FantasyKitSlots.SimpleIcon(row, "Icon", icon, 28f);
                UiKit.Fix(ic, 28f, 28f);
                ic.preserveAspect = true;
            }

            var lbl = UiKit.TmpLabel(row, "Label", label, UiKit.FontBody, UiKit.TextInverseDim, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(lbl, 120f, 32f);
            lbl.enableWordWrapping = false;

            var val = UiKit.TmpLabel(row, "Val", "—", UiKit.FontBody + 2, FantasyKitSlots.KitTeal, FontStyle.Bold, TextAnchor.MiddleRight);
            UiKit.Flex(val);
            val.enableWordWrapping = false;

            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val };
        }

        /// <summary>Force layout children to share one left edge / full rail width.</summary>
        public static void StretchFullWidth(Component c)
        {
            if (c == null) return;
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minWidth = 0f;
            if (le.preferredWidth > 0f) le.preferredWidth = -1f;
        }

        /// <summary>Dark CP banner: label left, big number right.</summary>
        public static TMP_Text CpBanner(Transform parent, string name, out TMP_Text value)
        {
            var bg = UiKit.Img(parent, name, Color.white);
            if (CasualArt.PopupNavy != null) { bg.sprite = CasualArt.PopupNavy; bg.type = Image.Type.Sliced; }
            else { bg.sprite = MapleLightTheme.RoundedSprite(12); bg.type = Image.Type.Sliced; bg.color = new Color(0.09f, 0.14f, 0.30f, 1f); }
            UiKit.Fix(bg, -1f, 48f);
            StretchFullWidth(bg);
            var h = UiKit.HStack(bg.transform, "H", 8f, 12, 14, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(h);
            var lbl = UiKit.TmpLabel(h, "L", "전투력", UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(lbl, 90f, 32f);
            value = UiKit.TmpLabel(h, "V", "—", UiKit.FontH2, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleRight);
            UiKit.Flex(value);
            return value;
        }

        /// <summary>Compact multi-line info chip (points / wallet summaries).</summary>
        public static Text InfoChip(Transform parent, string name, string text, float height = 64f)
        {
            float h = Mathf.Max(48f, height);
            var bg = UiKit.Img(parent, name, new Color(0.075f, 0.13f, 0.30f, 0.97f));
            var chipCard = CasualArt.CardRound;
            if (chipCard != null) { bg.sprite = chipCard; bg.type = Image.Type.Sliced; }
            else { bg.sprite = MapleLightTheme.RoundedSprite(12); bg.type = Image.Type.Sliced; }
            UiKit.Fix(bg, -1f, h);
            StretchFullWidth(bg);
            var le = bg.gameObject.GetComponent<LayoutElement>() ?? bg.gameObject.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
            var t = UiKit.Label(bg.transform, "T", text, UiKit.FontBody, UiKit.TextInverse, FontStyle.Normal, TextAnchor.MiddleLeft);
            var tr = t.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(12f, 8f);
            tr.offsetMax = new Vector2(-12f, -8f);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            FieldTextOutline(t);
            return t;
        }

        /// <summary>Left-rail avatar + CP banner + framed effective stats.</summary>
        public static StatSummaryView StatSummary(Transform parent, string name, bool withAvatar = true)
        {
            // No horizontal pad — LeftRail padding owns the column edge.
            var root = UiKit.VStack(parent, name, UiKit.Space2, 0, 0, 0, 0);
            StretchFullWidth(root);
            float baseH = withAvatar ? 390f : 230f;
            var rootLe = root.gameObject.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
            rootLe.minHeight = baseH;
            rootLe.preferredHeight = baseH;
            rootLe.flexibleHeight = 0f;
            var oldFit = root.gameObject.GetComponent<ContentSizeFitter>();
            if (oldFit != null) UnityEngine.Object.Destroy(oldFit);

            Image avatar = null;
            if (withAvatar)
            {
                var host = UiKit.Img(root, "AvatarHost", FantasyKitSlots.KitPanel);
                host.sprite = null;
                host.type = Image.Type.Simple;
                host.color = FantasyKitSlots.KitPanel;
                UiKit.Fix(host, -1f, 150f);
                StretchFullWidth(host);
                avatar = UiKit.Img(host.transform, "Avatar", Color.white);
                FantasyKitSlots.Slice(avatar, GrowArt.CircleFrame, 100f);
                var art = avatar.rectTransform;
                art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
                art.sizeDelta = new Vector2(100f, 100f);
                art.anchoredPosition = Vector2.zero;
                avatar.sprite = GrowArt.Hero;
                avatar.preserveAspect = true;
            }

            CpBanner(root, "CpBanner", out var cp);

            var atkLine = StatLine(root, "Atk", "공격력", GrowArt.IconEnhance("Attack"));
            var hpLine = StatLine(root, "Hp", "최대 HP", GrowArt.IconEnhance("Hp"));
            var defLine = StatLine(root, "Def", "방어력", GrowArt.IconEnhance("Accuracy"));

            var extra = InfoChip(root, "Extra", "", 56f);
            if (extra.transform.parent != null)
                extra.transform.parent.gameObject.SetActive(false);

            var view = new StatSummaryView
            {
                Go = root.gameObject,
                Avatar = avatar,
                Cp = cp,
                Atk = atkLine.Value,
                Hp = hpLine.Value,
                Def = defLine.Value,
                Extra = extra,
                RootLayout = rootLe,
                BasePreferredHeight = baseH
            };
            view.SyncPreferredHeight(false);
            view.RefreshFromCombat();
            return view;
        }
    }
}
