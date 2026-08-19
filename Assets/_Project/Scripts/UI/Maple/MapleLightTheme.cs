using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// Light-surface widget set (ref-layout modals: bright panels, dark text,
    /// orange progress, teal gradient invest cards). Returns the same view types
    /// as the dark theme so Refresh logic stays untouched.
    /// </summary>
    public static class MapleLightTheme
    {
        public static readonly Color Bg = new Color(0.955f, 0.955f, 0.965f, 1f);
        public static readonly Color BgAlt = new Color(0.90f, 0.905f, 0.92f, 1f);
        public static readonly Color TextDark = new Color(0.17f, 0.18f, 0.22f, 1f);
        public static readonly Color TextGray = new Color(0.48f, 0.50f, 0.55f, 1f);
        public static readonly Color Orange = new Color(0.98f, 0.62f, 0.12f, 1f);
        public static readonly Color Separator = new Color(0f, 0f, 0f, 0.10f);
        public static readonly Color LockedBg = new Color(0.86f, 0.86f, 0.88f, 1f);

        static readonly System.Collections.Generic.Dictionary<int, Sprite> _rounded =
            new System.Collections.Generic.Dictionary<int, Sprite>();

        /// <summary>
        /// Anti-aliased 9-slice rounded rectangle (white — tint via Image.color).
        /// The universal crisp surface: windows radius 16, buttons radius 10.
        /// </summary>
        public static Sprite RoundedSprite(int radius)
        {
            if (_rounded.TryGetValue(radius, out var cached)) return cached;
            int size = radius * 2 + 20; // flat center band for slicing
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // signed distance to rounded-rect edge (1px inset for AA room)
                    float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - r - 1f), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - r - 1f), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float a = Mathf.Clamp01(0.5f - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            float b = radius + 4;
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
            _rounded[radius] = sp;
            return sp;
        }

        /// <summary>Flat button: kit card frame + TMP label.</summary>
        public static Button FlatButton(Transform parent, string name, string label,
            Color bg, System.Action onClick, int fontSize = 0)
        {
            var img = UiKit.Img(parent, name, bg);
            img.sprite = CasualArt.CardRound != null ? CasualArt.CardRound : RoundedSprite(10);
            img.type = Image.Type.Sliced;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
            btn.colors = colors;
            btn.onClick.AddListener(Core.AudioService.Click);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            UiKit.Press(btn);
            if (!string.IsNullOrEmpty(label))
            {
                var t = UiKit.TmpLabel(img.transform, "Label", label,
                    fontSize > 0 ? fontSize : UiKit.TmpCaption, Color.white,
                    bold: true, TextAlignmentOptions.Center);
                t.enableWordWrapping = false;
                UiKit.Fill(t.rectTransform, 4f);
            }
            var ble = img.gameObject.AddComponent<LayoutElement>();
            ble.minWidth = 96f;
            ble.minHeight = 44f;
            return btn;
        }


        /// <summary>
        /// Kit demo-page mode: full navy background page (demo Character screen),
        /// docked panels turn transparent so content floats on the page.
        /// Page spans between top HUD (currency row stays visible) and bottom nav.
        /// </summary>
        public static void SkinDemoPage(ModalView modal, string baseName, bool wideContent = false)
        {
            if (modal == null || modal.Go == null) return;
            float leftW = 400f;
            var leftT = modal.Go.transform.Find(baseName + "Left") as RectTransform;
            if (leftT != null) leftW = leftT.sizeDelta.x;
            var panel = modal.Go.GetComponent<Image>();
            var bg = CasualArt.C("Background_07"); // demo blue vertical gradient
            if (panel != null && bg != null)
            {
                panel.sprite = bg;
                panel.type = Image.Type.Simple;
                panel.color = Color.white;
                panel.raycastTarget = true;
                var prt = panel.rectTransform;
                prt.anchorMin = Vector2.zero;
                prt.anchorMax = Vector2.one;
                prt.offsetMin = new Vector2(0f, 106f);   // keep bottom nav tappable
                prt.offsetMax = new Vector2(0f, 0f);     // true fullscreen (covers HUD)

                // soft radial glow behind the hero (demo look)
                if (panel.transform.Find("PageGlow") == null)
                {
                    var glow = UiKit.Img(panel.transform, "PageGlow", new Color(1f, 1f, 1f, 0.55f));
                    var gs = CasualArt.C("Background_ScreenGlow");
                    if (gs != null) glow.sprite = gs; else glow.color = new Color(1f, 1f, 1f, 0.06f);
                    glow.raycastTarget = false;
                    var grt = glow.rectTransform;
                    grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f);
                    grt.sizeDelta = new Vector2(1400f, 900f);
                    grt.anchoredPosition = new Vector2(-30f, 0f);
                    glow.transform.SetAsFirstSibling();
                }
            }
            foreach (var suffix in new[] { "Left", "Right" })
            {
                var t = modal.Go.transform.Find(baseName + suffix);
                if (t == null) continue;
                var img = t.GetComponent<Image>();
                if (img == null) continue;
                img.sprite = null;
                img.color = new Color(0f, 0f, 0f, 0f);
                var o = img.GetComponent<Outline>();
                if (o != null) o.enabled = false;
                var rt2 = (RectTransform)t;
                rt2.sizeDelta = new Vector2(rt2.sizeDelta.x, suffix == "Left" ? -190f : -110f);
                rt2.anchoredPosition = new Vector2(rt2.anchoredPosition.x, suffix == "Left" ? -30f : 30f);
                // List/manage pages: content stretches to fill the middle (anchor-based —
                // canvas rect width is unreliable during boot before the scaler runs).
                if (wideContent && suffix == "Right")
                {
                    rt2.anchorMin = new Vector2(0f, 0f);
                    rt2.anchorMax = new Vector2(1f, 1f);
                    rt2.pivot = new Vector2(0.5f, 0.5f);
                    rt2.offsetMin = new Vector2(leftW + 44f, 85f);
                    rt2.offsetMax = new Vector2(-12f, -25f);
                }

                if (suffix == "Right" && panel != null)
                {
                    var header = t.Find("Header") as RectTransform;
                    if (header != null)
                    {
                        var close = header.Find("Close") as RectTransform;
                        header.SetParent(panel.transform, false);
                        header.anchorMin = header.anchorMax = new Vector2(0.5f, 1f);
                        header.pivot = new Vector2(0.5f, 1f);
                        header.sizeDelta = new Vector2(520f, 60f);
                        header.anchoredPosition = new Vector2(0f, -14f);
                        if (close != null)
                        {
                            close.SetParent(panel.transform, false);
                            close.anchorMin = close.anchorMax = Vector2.one;
                            close.pivot = Vector2.one;
                            close.sizeDelta = new Vector2(52f, 52f);
                            close.anchoredPosition = new Vector2(-20f, -18f);
                        }
                    }
                }
            }
        }

        /// <summary>Swap a dual-modal's docked panels to light surfaces.</summary>
        public static void SkinLightPanels(ModalView modal, string baseName)
        {
            if (modal == null || modal.Go == null) return;
            foreach (var suffix in new[] { "Left", "Right" })
            {
                var t = modal.Go.transform.Find(baseName + suffix);
                if (t == null) continue;
                var img = t.GetComponent<Image>();
                if (img == null) continue;
                // GUI Pro white popup surface (baked shadow); rounded-rect fallback.
                var kit = CasualArt.PopupWhite;
                img.sprite = kit != null ? kit : RoundedSprite(16);
                img.type = Image.Type.Sliced;
                img.color = kit != null ? Color.white : Bg;
                var o = img.GetComponent<Outline>();
                if (o != null) o.enabled = false;
            }
        }

        public static TextMeshProUGUI DarkLabel(Transform parent, string name, string text, int size,
            bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, Color? color = null)
        {
            var t = UiKit.TmpLabel(parent, name, text, size, color ?? TextDark, bold, align);
            t.enableWordWrapping = false;
            return t;
        }

        /// <summary>Section title — kit demo style: sky blue bold caps.</summary>
        public static readonly Color SkyTitle = new Color(0.25f, 0.56f, 0.86f, 1f);
        public static TextMeshProUGUI Section(Transform parent, string text)
        {
            var t = DarkLabel(parent, "Sec_" + text, text, UiKit.TmpBody, bold: true, color: SkyTitle);
            UiKit.Fix(t, -1f, 36f);
            return t;
        }

        /// <summary>Kit demo stat row: navy banner, optional icon circle, white label + bold value.</summary>
        public static StatRowView NavyRow(Transform parent, string name, string label, float height = 46f, Sprite icon = null)
        {
            var card = UiKit.Img(parent, name, Color.white);
            var banner = CasualArt.C("BannerFrame01_Single_Navy");
            if (banner != null) { card.sprite = banner; card.type = Image.Type.Sliced; }
            else card.color = new Color(0.10f, 0.14f, 0.25f, 1f);
            UiKit.Fix(card, -1f, height);
            MapleUiTheme.StretchFullWidth(card);
            var h = UiKit.HStack(card.transform, "H", 10f, 12, 16, 6, 6, TextAnchor.MiddleLeft);
            UiKit.Fill(h);
            if (icon != null)
            {
                float d = height - 14f;
                var circle = UiKit.Img(h, "IconBg", new Color(0f, 0f, 0f, 0.35f));
                var cs = CasualArt.C("BasicFrame_Circle77");
                if (cs != null) circle.sprite = cs;
                UiKit.Fix(circle, d, d);
                var ic = UiKit.Sprite(circle.transform, "Icon", icon);
                ic.preserveAspect = true;
                UiKit.Fill(ic.rectTransform, 6f);
            }
            var lbl = UiKit.TmpLabel(h, "Label", label, UiKit.TmpCaption, new Color(1f, 1f, 1f, 0.82f), bold: true);
            lbl.enableWordWrapping = false;
            UiKit.Fix(lbl, 150f, 30f);
            var val = UiKit.TmpLabel(h, "Value", "-", UiKit.TmpBody + 2, Color.white, bold: true, TextAlignmentOptions.MidlineRight);
            val.enableWordWrapping = false;
            UiKit.Flex(val);
            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val };
        }

        /// <summary>Grade row: dark label | orange bar (count inside) | "N 단계" value.</summary>
        public static StatRowView GradeBar(Transform parent, string name, string label)
        {
            var card = UiKit.Img(parent, name, new Color(0f, 0f, 0f, 0f));
            UiKit.Fix(card, -1f, 60f);
            MapleUiTheme.StretchFullWidth(card);
            var h = UiKit.HStack(card.transform, "H", 10f, 14, 14, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            var lbl = DarkLabel(h, "Label", label, UiKit.TmpBody, bold: true, color: Color.white);
            UiKit.Fix(lbl, 130f, 32f);

            var bar = new BarView();
            var track = UiKit.Img(h, "Bar", BgAlt);
            var kitTrack = CasualArt.C("Slider_Basic01_Bg");
            if (kitTrack != null)
            {
                track.sprite = kitTrack;
                track.type = Image.Type.Sliced;
                track.color = new Color(0.09f, 0.13f, 0.26f, 1f); // navy (demo grade bar)
            }
            else
            {
                track.sprite = null;
                track.type = Image.Type.Simple;
            }
            UiKit.Flex(track);
            UiKit.Fix(track, -1f, 26f);
            var fill = UiKit.Img(track.transform, "Fill", Orange);
            var kitFill = CasualArt.C("Slider_Basic01_Fill_Blue");
            if (kitFill != null)
            {
                fill.sprite = kitFill;
                fill.type = Image.Type.Sliced;
                fill.color = new Color(0.30f, 0.52f, 0.95f, 1f); // deeper blue so white label reads
            }
            fill.raycastTarget = false;
            var fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0.5f, 1f);
            fr.offsetMin = new Vector2(2f, 2f); fr.offsetMax = new Vector2(0f, -2f);
            var barLabel = UiKit.TmpLabel(track.transform, "T", "", UiKit.TmpCaption - 2, Color.white,
                bold: true, TextAlignmentOptions.Center);
            barLabel.enableWordWrapping = false;
            UiKit.Fill(barLabel.rectTransform);
            bar.Go = track.gameObject; bar.Fill = fill; bar.Label = barLabel; bar.Inset = 2f;

            var val = DarkLabel(h, "Val", "-", UiKit.TmpBody, bold: true, TextAlignmentOptions.MidlineRight, new Color(1f, 0.78f, 0.25f, 1f));
            UiKit.Fix(val, 96f, 32f);

            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val, Progress = bar };
        }

        /// <summary>
        /// Wide invest card: teal gradient, corner count badge, centered title,
        /// big bonus value, small preview+cost, CTA button.
        /// </summary>
        public static StatRowView InvestCardWide(Transform parent, string name, string title,
            string cta, System.Action onClick)
        {
            // 키트 카드 프레임 + 위쪽 그라데이션 하이라이트 + 테두리 (밋밋한 단색 박스 대신)
            var card = UiKit.Img(parent, name, new Color(0.12f, 0.19f, 0.38f, 1f));
            var kitCard = CasualArt.C("CardFrame01_Bg") ?? CasualArt.CardRound;
            card.sprite = kitCard != null ? kitCard : RoundedSprite(12);
            card.type = Image.Type.Sliced;
            UiKit.Flex(card);

            var grad = UiKit.Img(card.transform, "Grad", new Color(0.45f, 0.70f, 1f, 0.20f));
            grad.sprite = CasualArt.C("CardFrame01_Gradient");
            grad.type = Image.Type.Sliced;
            grad.raycastTarget = false;
            UiKit.Fill(grad.rectTransform, 2f);

            var rim = UiKit.Img(card.transform, "Rim", new Color(0.40f, 0.66f, 1f, 0.55f));
            rim.sprite = CasualArt.BorderSky ?? CasualArt.C("BorderFrame_Round01_Blue");
            rim.type = Image.Type.Sliced;
            rim.raycastTarget = false;
            UiKit.Fill(rim.rectTransform);

            var v = UiKit.VStack(card.transform, "V", 3f, 10, 10, 8, 10, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            // 값 배지. Label_Trapezoid는 9슬라이스 세로 보더가 0이라(y=0,w=0) 60→30으로 눌리면
            // 사다리꼴 빗변이 찌그러져 '오각형'처럼 보였다. 4면 슬라이스가 되는 카드 프레임을 쓴다.
            var badgeBg = UiKit.Img(card.transform, "BadgeBg", SkyTitle);
            badgeBg.sprite = CasualArt.CardRound != null ? CasualArt.CardRound : RoundedSprite(8);
            badgeBg.type = Image.Type.Sliced;
            badgeBg.raycastTarget = false;
            var bgrt = badgeBg.rectTransform;
            bgrt.anchorMin = new Vector2(1f, 1f); bgrt.anchorMax = Vector2.one;
            bgrt.pivot = new Vector2(1f, 1f);
            bgrt.anchoredPosition = new Vector2(-8f, -6f);
            bgrt.sizeDelta = new Vector2(96f, 38f);   // '235/∞'가 들어갈 폭 + 9슬라이스 최소 높이
            var badge = UiKit.TmpLabel(badgeBg.transform, "Badge", "", UiKit.TmpCaption - 2,
                Color.white, bold: true, TextAlignmentOptions.Center);
            badge.enableWordWrapping = false;
            UiKit.Fill(badge.rectTransform, 2f);

            var spacerTop = UiKit.Rect(v, "Sp"); UiKit.Fix(spacerTop, -1f, 26f);
            var t = UiKit.TmpLabel(v, "Title", title, UiKit.TmpBody, new Color(1f, 1f, 1f, 0.88f), bold: true, TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            UiKit.Fix(t, -1f, 30f);
            var big = UiKit.TmpLabel(v, "Big", "+0", UiKit.TmpTitle + 4, Color.white,
                bold: true, TextAlignmentOptions.Center);
            big.enableWordWrapping = false;
            UiKit.Fix(big, -1f, 48f);
            var small = UiKit.TmpLabel(v, "Preview", "", UiKit.TmpCaption - 1,
                new Color(0.55f, 0.80f, 1f, 1f), bold: false, TextAlignmentOptions.Center);
            small.enableWordWrapping = false;
            UiKit.Fix(small, -1f, 24f);

            var btn = MapleUiTheme.AccentButton(v, "Act", cta, onClick, UiKit.FontCaption);
            UiKit.Fix(btn, -1f, 50f);
            var cost = UiKit.TmpLabel(v, "Cost", "", UiKit.TmpCaption - 2,
                new Color(1f, 1f, 1f, 0.62f), bold: false, TextAlignmentOptions.Center);
            cost.enableWordWrapping = false;
            UiKit.Fix(cost, -1f, 18f);

            return new StatRowView
            {
                Go = card.gameObject, Label = t, Level = badge, Bonus = big,
                Value = small, Cost = cost, Action = btn
            };
        }

        /// <summary>Special-stat row: title+val/max top, % below, CTA right. Locked = gray + requirement.</summary>
        public static StatRowView SpecialRow(Transform parent, string name, string title,
            string cta, System.Action onClick, bool locked, string lockReason)
        {
            var lockedNavy = new Color(0.62f, 0.66f, 0.76f, 1f);
            var card = UiKit.Img(parent, name, Color.white);
            var navyCard = CasualArt.CardRound;
            if (navyCard != null)
            {
                card.sprite = navyCard;
                card.type = Image.Type.Sliced;
                card.color = locked ? new Color(0.16f, 0.20f, 0.32f, 1f) : new Color(0.10f, 0.16f, 0.32f, 1f);
            }
            else
                FantasyKitSlots.ApplyRectBorder(card, locked ? LockedBg : Color.white, 2f, Separator);
            var h = UiKit.HStack(card.transform, "H", 8f, 14, 12, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            var mid = UiKit.VStack(h, "Mid", 2f, 0, 0, 2, 2, TextAnchor.MiddleLeft);
            UiKit.Flex(mid);
            // 앵커 고정 배치 — 레이아웃그룹 플렉스가 TMP 폭을 붕괴시키는 문제 회피
            var titleRow = UiKit.Rect(mid, "TR");
            UiKit.Fix(titleRow, -1f, 26f);
            var lbl = DarkLabel(titleRow, "Label", title, UiKit.TmpCaption - 2, bold: true,
                color: locked ? new Color(1f, 1f, 1f, 0.75f) : Color.white);
            var lrt = lbl.rectTransform;
            lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 1f);
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = new Vector2(-102f, 0f);
            var lvl = DarkLabel(titleRow, "Level", locked ? "" : "0/20", UiKit.TmpCaption - 2, bold: true,
                TextAlignmentOptions.MidlineRight, locked ? new Color(1f, 1f, 1f, 0.75f) : Color.white);
            var vrt = lvl.rectTransform;
            vrt.anchorMin = new Vector2(1f, 0f); vrt.anchorMax = new Vector2(1f, 1f);
            vrt.pivot = new Vector2(1f, 0.5f);
            vrt.offsetMin = new Vector2(-98f, 0f); vrt.offsetMax = Vector2.zero;
            var pct = DarkLabel(mid, "Bonus", locked ? (lockReason ?? "잠금") : "0%", UiKit.TmpCaption,
                bold: false, color: locked ? new Color(1f, 1f, 1f, 0.70f) : new Color(0.55f, 0.85f, 1f, 1f));
            UiKit.Fix(pct, -1f, 24f);

            Button act = null;
            if (!locked && cta != null)
            {
                act = MapleUiTheme.AccentButton(h, "Act", cta, onClick, UiKit.FontCaption - 2);
                UiKit.Fix(act, 104f, 44f);
            }
            else if (locked && GrowArt.IconLock != null)
            {
                var ic = FantasyKitSlots.SimpleIcon(h, "Lock", GrowArt.IconLock, 26f);
                UiKit.Fix(ic, 26f, 26f);
            }

            return new StatRowView { Go = card.gameObject, Label = lbl, Level = lvl, Bonus = pct, Value = pct, Action = act };
        }

        /// <summary>Left-panel stat line: dark label | dark value, hairline separator below.</summary>
        public static StatRowView StatLine(Transform parent, string name, string label)
        {
            var row = UiKit.Rect(parent, name);
            UiKit.Fix(row, -1f, 40f);
            MapleUiTheme.StretchFullWidth(row);
            var h = UiKit.HStack(row, "H", 8f, 6, 6, 4, 4, TextAnchor.MiddleLeft);
            UiKit.Fill(h);
            var lbl = DarkLabel(h, "Label", label, UiKit.TmpCaption, bold: false, color: TextGray);
            UiKit.Flex(lbl);
            var val = DarkLabel(h, "Val", "-", UiKit.TmpCaption, bold: true, TextAlignmentOptions.MidlineRight);
            UiKit.Fix(val, 130f, 28f);
            var sep = UiKit.Img(row, "Sep", Separator);
            var srt = sep.rectTransform;
            srt.anchorMin = new Vector2(0f, 0f); srt.anchorMax = new Vector2(1f, 0f);
            srt.offsetMin = new Vector2(4f, 0f); srt.offsetMax = new Vector2(-4f, 1.5f);
            sep.raycastTarget = false;
            return new StatRowView { Go = row.gameObject, Label = lbl, Value = val };
        }

        /// <summary>Dark rounded CP pill on light surface: label + big number.</summary>
        public static TMP_Text CpPill(Transform parent, string name, out TMP_Text value)
        {
            var bg = UiKit.Img(parent, name, new Color(0.15f, 0.16f, 0.20f, 1f));
            if (CasualArt.PopupNavy != null)
            {
                bg.sprite = CasualArt.PopupNavy;
                bg.color = Color.white;
            }
            else
                bg.sprite = RoundedSprite(12);
            bg.type = Image.Type.Sliced;
            UiKit.Fix(bg, -1f, 52f);
            MapleUiTheme.StretchFullWidth(bg);
            var h = UiKit.HStack(bg.transform, "H", 8f, 16, 16, 6, 6, TextAnchor.MiddleLeft);
            UiKit.Fill(h);
            var lbl = UiKit.TmpLabel(h, "L", "전투력", UiKit.TmpCaption, new Color(1f, 1f, 1f, 0.85f), bold: true);
            lbl.enableWordWrapping = false;
            UiKit.Fix(lbl, 90f, 30f);
            value = UiKit.TmpLabel(h, "V", "-", UiKit.TmpHeader, Color.white, bold: true, TextAlignmentOptions.MidlineRight);
            value.enableWordWrapping = false;
            UiKit.Flex(value);
            return value;
        }
    }
}
