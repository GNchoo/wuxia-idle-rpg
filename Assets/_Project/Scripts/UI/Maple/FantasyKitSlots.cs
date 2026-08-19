using IdleMvp.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// Rebuilds Mobile Fantasy Idle UI Kit slot layouts in code.
    /// Matches DemoScene Slot_Enhance / Slot_Skill / Slot_Gear / Slot_Summon sizes
    /// and hierarchy (Frame_List + Frame_Img + icon Simple + CTA Sliced at PPU=1).
    /// Landscape modals scale ~0.55 from the kit's 1440x2560 portrait reference.
    /// </summary>
    public static class FantasyKitSlots
    {
        public static readonly Color KitTeal = new Color(0.55f, 0.32f, 0.12f, 1f);   // 무협 킷: 진갈색 강조
        public static readonly Color KitPanel = new Color(0.22f, 0.255f, 0.306f, 1f);

        static readonly Color[] RarityEdges =
        {
            new Color(0.55f, 0.58f, 0.62f, 1f),
            new Color(0.30f, 0.82f, 0.42f, 1f),
            new Color(0.32f, 0.62f, 0.95f, 1f),
            new Color(0.72f, 0.38f, 0.92f, 1f),
            new Color(0.95f, 0.82f, 0.28f, 1f),
            new Color(0.92f, 0.32f, 0.32f, 1f)
        };

        /// <summary>Window chrome only ? sharp corners (modal shell / header).</summary>
        public static void SharpPanel(Image img, Color? fill = null, Color? edge = null)
        {
            if (img == null) return;
            // 키트의 각진 프레임을 우선 사용한다. 손으로 그린 테두리는 스프라이트가 없을 때만.
            var kit = CasualArt.CardSquare;
            if (kit != null)
            {
                img.sprite = kit;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color = fill ?? KitPanel;
                DisableOutline(img);
                return;
            }
            ApplyRectBorder(img, fill ?? KitPanel, 4f, edge);
        }

        /// <summary>
        /// Visible rectangular border on all 4 sides (child fill inset).
        /// Does not rely on Outline/Shadow mesh offsets that get clipped.
        /// </summary>
        public static void ApplyRectBorder(Image shell, Color fill, float thickness = 4f, Color? rim = null)
        {
            if (shell == null) return;
            DisableOutline(shell);
            shell.sprite = null;
            shell.type = Image.Type.Simple;
            shell.preserveAspect = false;
            shell.pixelsPerUnitMultiplier = 1f;
            shell.color = rim ?? new Color(0.02f, 0.025f, 0.04f, 1f);

            const string fillName = "PanelFill";
            Image fillImg = null;
            for (int i = 0; i < shell.transform.childCount; i++)
            {
                var ch = shell.transform.GetChild(i);
                if (ch.name == fillName)
                {
                    fillImg = ch.GetComponent<Image>();
                    break;
                }
            }
            if (fillImg == null)
            {
                fillImg = UiKit.Img(shell.transform, fillName, fill);
            }
            fillImg.sprite = null;
            fillImg.type = Image.Type.Simple;
            fillImg.color = fill;
            fillImg.raycastTarget = false;
            fillImg.transform.SetAsFirstSibling();
            float t = Mathf.Max(2f, thickness);
            UiKit.Fill(fillImg.rectTransform, t);
        }

        static void DisableOutline(Image img)
        {
            var o = img != null ? img.GetComponent<Outline>() : null;
            if (o != null) o.enabled = false;
        }

        static float GuessMinSide(Image img, float fallback)
        {
            if (img == null) return fallback;
            var rt = img.rectTransform;
            float w = rt.rect.width;
            float h = rt.rect.height;
            if (w > 1f && h > 1f) return Mathf.Min(w, h);
            var le = img.GetComponent<LayoutElement>();
            if (le != null)
            {
                float pw = le.preferredWidth > 0f ? le.preferredWidth : fallback;
                float ph = le.preferredHeight > 0f ? le.preferredHeight : fallback;
                return Mathf.Min(pw, ph);
            }
            return fallback;
        }

        /// <summary>Rounded list/card frame (InvSlot). Fallback: flat panel.</summary>
        public static void FrameList(Image img, float minSideHint = 120f)
        {
            if (img == null) return;
            // 무협 행 판(비단)이 있으면 틴트 없이 그대로 — 없으면 킷 남색 카드
            var wux = CasualArt.RowDark;
            if (wux != null)
            {
                img.sprite = wux;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color = Color.white;
                DisableOutline(img);
                return;
            }
            var kit = CasualArt.CardRound;
            if (kit != null)
            {
                img.sprite = kit;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color = new Color(0.10f, 0.16f, 0.32f, 0.97f);
                DisableOutline(img);
                return;
            }
            ApplyRectBorder(img, KitPanel, 3f);
        }

        /// <summary>Rounded rarity edge for tiles. Fallback: colored outline.</summary>
        public static void FrameRarity(Image img, int rarity, float minSideHint = 140f)
        {
            if (img == null) return;
            int g = Mathf.Clamp(rarity, 0, RarityEdges.Length - 1);
            // GUI Pro colored hero-card by rarity (demo palette: blue/green/purple/orange)
            string col = g <= 1 ? "Blue" : g == 2 ? "Green" : g == 3 ? "Purple" : "Orange";
            var kit = CasualArt.C("CardFrame03_Single_" + col);
            if (kit != null)
            {
                img.sprite = kit;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color = Color.white;
                DisableOutline(img);
                return;
            }
            var spr = GrowArt.Rarity(g);
            float side = GuessMinSide(img, minSideHint);
            if (spr != null && Slice(img, spr, side))
            {
                DisableOutline(img);
                return;
            }
            ApplyRectBorder(img, KitPanel, 3f, RarityEdges[g]);
        }

        public static void FrameRarity(Image img, Sprite raritySprite, float minSideHint = 140f)
        {
            // Map legacy rarity sprite → grade index, then always use the kit colored card.
            int g = 0;
            if (raritySprite != null)
            {
                for (int i = 0; i <= 5; i++)
                {
                    if (raritySprite == GrowArt.Rarity(i)) { g = i; break; }
                }
            }
            FrameRarity(img, g, minSideHint);
        }

        /// <summary>Compat ? routes to rounded FrameRarity.</summary>
        public static void SharpRarity(Image img, int rarity) => FrameRarity(img, rarity);

        public static void SharpRarity(Image img, Sprite raritySprite) => FrameRarity(img, raritySprite);

        /// <summary>
        /// Apply frame sprite. Large panels keep PPU=1 Sliced.
        /// Modal shells use kit Popup_Bg or ApplyRectBorder; tiles use FrameList/FrameRarity.
        /// </summary>
        public static bool Slice(Image img, Sprite sprite, Color? tint = null)
        {
            return Slice(img, sprite, -1f, tint);
        }

        public static bool Slice(Image img, Sprite sprite, float rectMinSide, Color? tint = null)
        {
            if (img == null || sprite == null) return false;
            img.sprite = sprite;
            img.color = tint ?? Color.white;

            float borderNeed = Mathf.Max(
                sprite.border.x + sprite.border.z,
                sprite.border.y + sprite.border.w);

            if (rectMinSide > 0f && borderNeed > 1f && rectMinSide < borderNeed + 12f)
            {
                // Too small for 9-slice ? flat color, never Simple+preserveAspect (shows floating ovals/squares).
                img.sprite = null;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
                img.pixelsPerUnitMultiplier = 1f;
                if (tint.HasValue) img.color = tint.Value;
                else img.color = KitPanel;
                return false;
            }

            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            if (rectMinSide > 0f && rectMinSide < 180f)
                img.pixelsPerUnitMultiplier = 2f;
            else
                img.pixelsPerUnitMultiplier = 1f;
            return true;
        }

        static void EnsureCardMask(GameObject go)
        {
            if (go == null) return;
            if (go.GetComponent<RectMask2D>() == null)
                go.AddComponent<RectMask2D>();
        }

        public static Image SimpleIcon(Transform parent, string name, Sprite sprite, float size)
        {
            var img = UiKit.Sprite(parent, name, sprite, sliced: false);
            UiKit.Fix(img, size, size);
            if (img.sprite != null) img.color = Color.white;
            return img;
        }

        /// <summary>
        /// Slot_Enhance style row: list frame + icon frame + title/value + green CTA.
        /// Demo root ~1380x200 ? landscape ~760x110.
        /// </summary>
        public static StatRowView EnhanceRow(Transform parent, string name, string title, Sprite icon,
            string actionLabel, System.Action onAction, float height = 110f)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card, height);
            UiKit.Fix(card, -1f, height);

            var h = UiKit.HStack(card.transform, "Row", 12f, 12, 12, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            // IconFrame only when large enough (>=70). Smaller sizes become empty blue squares.
            if (GrowArt.IconFrame != null && height >= 70f)
            {
                var iconFrame = UiKit.Img(h, "IconFrame", Color.white);
                Slice(iconFrame, GrowArt.IconFrame, 72f);
                UiKit.Fix(iconFrame, 72f, 72f);
                if (icon != null)
                {
                    var ic = SimpleIcon(iconFrame.transform, "Icon", icon, 52f);
                    UiKit.Fill(ic.rectTransform, 10f);
                    ic.preserveAspect = true;
                }
            }
            else if (icon != null)
            {
                var ic = SimpleIcon(h, "Icon", icon, 52f);
                UiKit.Fix(ic, 52f, 52f);
            }

            var mid = UiKit.VStack(h, "Mid", 2f, 0, 0, 2, 2, TextAnchor.MiddleLeft);
            UiKit.Flex(mid);
            // Content-sized heights: empty Value/Cost collapse so short rows keep the title visible.
            var lbl = UiKit.TmpLabel(mid, "Label", title, UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold);
            lbl.enableWordWrapping = false; // never letter-stack in narrow rails
            var val = UiKit.TmpLabel(mid, "Value", "", UiKit.FontH2, KitTeal, FontStyle.Bold);
            val.enableWordWrapping = false;
            // Cost under preview so it isn't crushed by the CTA (was 80px sibling).
            var cost = UiKit.TmpLabel(mid, "Cost", "", UiKit.FontBody, UiKit.TextInverse, FontStyle.Normal, TextAnchor.MiddleLeft);

            Button act = null;
            if (actionLabel != null)
            {
                // Compact rows (left-rail lists) get a narrower CTA so the label keeps width.
                bool compact = height < 90f;
                act = MapleUiTheme.AccentButton(h, "Act", actionLabel, onAction,
                    compact ? UiKit.FontCaption : UiKit.FontBody);
                UiKit.Fix(act, compact ? 90f : 180f, compact ? 48f : 64f);
            }

            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val, Cost = cost, Action = act };
        }

        /// <summary>
        /// Maple Idle invest card: title, growth level, big bonus, cost + CTA.
        /// </summary>
        public static StatRowView InvestCard(Transform parent, string name, string title, Sprite icon,
            string actionLabel, System.Action onAction, float height = 128f)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card, height);
            UiKit.Fix(card, -1f, height);

            var h = UiKit.HStack(card.transform, "Row", 12f, 14, 14, 12, 12, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            var iconFrame = UiKit.Img(h, "IconFrame", Color.white);
            Slice(iconFrame, GrowArt.IconFrame, 80f);
            UiKit.Fix(iconFrame, 80f, 80f);
            if (icon != null)
            {
                var ic = SimpleIcon(iconFrame.transform, "Icon", icon, 56f);
                UiKit.Fill(ic.rectTransform, 12f);
                ic.preserveAspect = true;
            }

            var mid = UiKit.VStack(h, "Mid", 4f, 0, 0, 2, 2, TextAnchor.MiddleLeft);
            UiKit.Flex(mid);

            var top = UiKit.HStack(mid, "Top", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft);
            UiKit.Fix(top, -1f, 26f);
            var lbl = UiKit.TmpLabel(top, "Label", title, UiKit.FontBody + 2, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Flex(lbl);
            var level = UiKit.TmpLabel(top, "Level", "?? Lv.0", UiKit.FontCaption + 2, UiKit.TextInverseDim, FontStyle.Bold, TextAnchor.MiddleRight);
            UiKit.Fix(level, 140f, 24f);

            var bonus = UiKit.TmpLabel(mid, "Bonus", "+0", UiKit.FontH1, KitTeal, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(bonus, -1f, 36f);

            var preview = UiKit.TmpLabel(mid, "Preview", "", UiKit.FontCaption + 1, UiKit.TextInverseDim, FontStyle.Normal, TextAnchor.MiddleLeft);
            UiKit.Fix(preview, -1f, 22f);

            var right = UiKit.VStack(h, "Right", 6f, 0, 0, 0, 0, TextAnchor.MiddleCenter);
            UiKit.Fix(right, 200f, -1f);

            var cost = UiKit.TmpLabel(right, "Cost", "", UiKit.FontCaption + 1, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(cost, -1f, 28f);

            Button act = null;
            if (actionLabel != null)
            {
                act = MapleUiTheme.AccentButton(right, "Act", actionLabel, onAction, UiKit.FontBody);
                UiKit.Fix(act, -1f, 56f);
            }

            return new StatRowView
            {
                Go = card.gameObject,
                Label = lbl,
                Level = level,
                Bonus = bonus,
                Value = preview,
                Cost = cost,
                Action = act
            };
        }

        /// <summary>Original Maple Idle vertical primary-stat card (title / big bonus / Lv / CTA).</summary>
        public static StatRowView VerticalInvestCard(Transform parent, string name, string title, Sprite icon,
            string actionLabel, System.Action onAction, float width = 220f, float height = 320f)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card, width > 0f ? Mathf.Min(width, height) : height);
            UiKit.Fix(card, width, height);
            EnsureCardMask(card.gameObject);

            var v = UiKit.VStack(card.transform, "V", 6f, 12, 12, 10, 10, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            var lbl = UiKit.TmpLabel(v, "Label", title, UiKit.FontBody, UiKit.TextInverseDim, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(lbl, -1f, 24f);

            if (icon != null)
            {
                var ic = SimpleIcon(v, "Icon", icon, 44f);
                UiKit.Fix(ic, 44f, 44f);
                ic.preserveAspect = true;
            }

            var bonus = UiKit.TmpLabel(v, "Bonus", "+0", UiKit.FontH1 + 4, KitTeal, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(bonus, -1f, 36f);

            var level = UiKit.TmpLabel(v, "Level", "0/10", UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(level, -1f, 22f);

            var preview = UiKit.TmpLabel(v, "Preview", "", UiKit.FontCaption + 1, UiKit.TextInverseDim, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiKit.Fix(preview, -1f, 20f);
            preview.enableWordWrapping = true;
            preview.overflowMode = TextOverflowModes.Truncate;

            var cost = UiKit.TmpLabel(v, "Cost", "?? 1", UiKit.FontCaption + 1, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(cost, -1f, 20f);

            Button act = null;
            if (actionLabel != null)
            {
                act = MapleUiTheme.AccentButton(v, "Act", actionLabel, onAction, UiKit.FontBody);
                UiKit.Fix(act, -1f, 48f);
            }

            return new StatRowView
            {
                Go = card.gameObject,
                Label = lbl,
                Level = level,
                Bonus = bonus,
                Value = preview,
                Cost = cost,
                Action = act
            };
        }

        /// <summary>Special-stat grid cell ? unlocked invest or locked requirement.</summary>
        public static StatRowView SpecialStatCard(Transform parent, string name, string title, Sprite icon,
            string actionLabel, System.Action onAction, bool locked, string lockReason, Vector2 cell)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card);
            var le = card.gameObject.GetComponent<LayoutElement>() ?? card.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = cell.x;
            le.preferredHeight = cell.y;
            le.minWidth = cell.x;
            le.minHeight = cell.y;
            // No RectMask2D ? CTA must not be sliced by the card edge.

            var v = UiKit.VStack(card.transform, "V", 4f, 12, 12, 10, 14, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            var lbl = UiKit.TmpLabel(v, "Label", title, UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(lbl, -1f, 22f);
            lbl.enableWordWrapping = true;
            lbl.overflowMode = TextOverflowModes.Truncate;

            if (locked)
            {
                if (GrowArt.IconLock != null)
                {
                    var lockIc = SimpleIcon(v, "Lock", GrowArt.IconLock, 32f);
                    UiKit.Fix(lockIc, 32f, 32f);
                }
                var reason = UiKit.TmpLabel(v, "Reason", lockReason ?? "??", UiKit.FontCaption + 1, UiKit.TextInverseDim, FontStyle.Normal, TextAnchor.MiddleCenter);
                UiKit.Fix(reason, -1f, 36f);
                reason.enableWordWrapping = true;
                reason.overflowMode = TextOverflowModes.Truncate;
                var actLocked = MapleUiTheme.SecondaryButton(v, "Act", "??", null, UiKit.FontCaption + 1);
                UiKit.Fix(actLocked, -1f, 36f);
                UiKit.SetEnabled(actLocked, false);
                return new StatRowView
                {
                    Go = card.gameObject,
                    Label = lbl,
                    Value = reason,
                    Action = actLocked
                };
            }

            if (icon != null)
            {
                var ic = SimpleIcon(v, "Icon", icon, 36f);
                UiKit.Fix(ic, 36f, 36f);
            }

            var level = UiKit.TmpLabel(v, "Level", "0/20", UiKit.FontCaption + 2, UiKit.TextInverseDim, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(level, -1f, 20f);
            var bonus = UiKit.TmpLabel(v, "Bonus", "+0%", UiKit.FontH2, KitTeal, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(bonus, -1f, 26f);
            // Preview + cost share one line to fit cell >=260.
            var meta = UiKit.TmpLabel(v, "Meta", "", UiKit.FontCaption, UiKit.TextInverseDim, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiKit.Fix(meta, -1f, 22f);
            meta.enableWordWrapping = true;
            meta.overflowMode = TextOverflowModes.Truncate;
            var act = MapleUiTheme.AccentButton(v, "Act", actionLabel ?? "+1", onAction, UiKit.FontCaption + 1);
            UiKit.Fix(act, -1f, 36f);

            return new StatRowView
            {
                Go = card.gameObject,
                Label = lbl,
                Level = level,
                Bonus = bonus,
                Value = meta,
                Cost = meta,
                Action = act
            };
        }

        /// <summary>Grade progress header with bar (Maple Idle style).</summary>
        public static StatRowView GradeProgress(Transform parent, string name, string title = "??")
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card, 88f);
            UiKit.Fix(card, -1f, 88f);
            MapleUiTheme.StretchFullWidth(card);

            var v = UiKit.VStack(card.transform, "V", 6f, 12, 12, 10, 10, TextAnchor.UpperLeft);
            UiKit.Fill(v);

            var top = UiKit.HStack(v, "Top", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft);
            UiKit.Fix(top, -1f, 28f);
            var lbl = UiKit.TmpLabel(top, "Label", title, UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(lbl, 100f, 26f);
            var val = UiKit.TmpLabel(top, "Val", "0 ??", UiKit.FontH2, KitTeal, FontStyle.Bold, TextAnchor.MiddleRight);
            UiKit.Flex(val);

            var bar = MapleUiTheme.Bar(v, "Bar", KitTeal, true);
            UiKit.Fix(bar.Go.transform, -1f, 32f);

            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val, Progress = bar };
        }

        /// <summary>Compact info row (summary) with icon frame ? still kit framed, not a flat line.</summary>
        public static StatRowView InfoRow(Transform parent, string name, string title, Sprite icon, float height = 72f)
        {
            var card = UiKit.Img(parent, name, Color.white);
            var rowBanner = CasualArt.C("BannerFrame01_Single_Navy");
            if (rowBanner != null) { card.sprite = rowBanner; card.type = Image.Type.Sliced; }
            else { card.sprite = null; card.type = Image.Type.Simple; card.color = new Color(0.10f, 0.14f, 0.25f, 1f); }
            UiKit.Fix(card, -1f, height);
            MapleUiTheme.StretchFullWidth(card);

            var h = UiKit.HStack(card.transform, "Row", 8f, 12, 14, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            if (icon != null && height >= 88f && GrowArt.IconFrame != null)
            {
                var iconFrame = UiKit.Img(h, "IconFrame", Color.white);
                Slice(iconFrame, GrowArt.IconFrame, 72f);
                UiKit.Fix(iconFrame, 72f, 72f);
                var ic = SimpleIcon(iconFrame.transform, "Icon", icon, 48f);
                UiKit.Fill(ic.rectTransform, 10f);
                ic.preserveAspect = true;
            }
            else if (icon != null)
            {
                var ic = SimpleIcon(h, "Icon", icon, 40f);
                UiKit.Fix(ic, 40f, 40f);
            }

            var lbl = UiKit.TmpLabel(h, "Label", title, UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold);
            UiKit.Fix(lbl, 160f);

            var val = UiKit.TmpLabel(h, "Value", "", UiKit.FontH2, KitTeal, FontStyle.Bold, TextAnchor.MiddleRight);
            val.enableWordWrapping = false; // "ATK +124.3" must never wrap into a second line
            val.overflowMode = TMPro.TextOverflowModes.Overflow;
            UiKit.Flex(val);

            return new StatRowView { Go = card.gameObject, Label = lbl, Value = val, Cost = null, Action = null };
        }

        /// <summary>Slot_Skill / Slot_Gear tile: rarity edge + icon + title + sub + optional action strip.</summary>
        public static ItemCardView SkillTile(Transform parent, string name, string title, string sub,
            Sprite icon, Sprite rarityFrame, System.Action onClick, Vector2 cell, bool locked = false,
            bool withActionStrip = false, int actionRows = 1)
        {
            // UIHangulSDF line height = 1.33em → 17pt needs 23px, 16pt needs 22px.
            const float titleH = 26f;
            float subH = withActionStrip ? 26f : 46f;
            float rowH = 36f;
            float stripH = withActionStrip ? (actionRows * (rowH + 4f) + 4f) : 0f;
            // Rarity 9-slice eats edges ? keep content inside the visible fill.
            float padX = withActionStrip ? 14f : 12f;
            float padT = withActionStrip ? 14f : 10f;
            float padB = withActionStrip ? 14f : 12f;
            float gaps = 4f * (withActionStrip ? 3f : 2f);
            float minIcon = withActionStrip ? 36f : 40f;
            float minH = padT + padB + titleH + subH + stripH + gaps + minIcon;
            if (cell.y < minH) cell.y = minH;

            var card = UiKit.Img(parent, name, KitPanel);
            FrameRarity(card, rarityFrame, Mathf.Min(cell.x, cell.y));
            var le = card.gameObject.GetComponent<LayoutElement>() ?? card.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = cell.x;
            le.preferredHeight = cell.y;
            le.minWidth = cell.x;
            le.minHeight = cell.y;

            var v = UiKit.VStack(card.transform, "V", 4f, padX, padX, padT, padB, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            Image iconImg = null;
            float textBudget = titleH + subH + stripH + padT + padB + gaps;
            float iconBudget = Mathf.Max(minIcon, cell.y - textBudget);
            float iconSize = Mathf.Min(cell.x - padX * 2f - 8f, iconBudget);
            if (icon != null)
            {
                iconImg = SimpleIcon(v, "Icon", icon, iconSize);
                UiKit.Fix(iconImg, -1f, iconSize);
                if (locked) iconImg.color = new Color(0.45f, 0.45f, 0.5f, 1f);
            }

            if (locked && GrowArt.IconLock != null)
            {
                var lockIc = SimpleIcon(card.transform, "Lock", GrowArt.IconLock, 22f);
                var lr = lockIc.rectTransform;
                lr.anchorMin = lr.anchorMax = new Vector2(1f, 1f);
                lr.pivot = new Vector2(1f, 1f);
                lr.anchoredPosition = new Vector2(-8f, -8f);
            }

            var t = UiKit.TmpLabel(v, "Title", title, UiKit.TmpCaption, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            UiKit.Fix(t, -1f, titleH);

            var s = UiKit.TmpLabel(v, "Sub", sub, UiKit.TmpCaption - 1, Color.white,
                bold: true, TextAlignmentOptions.Center);
            s.overflowMode = TextOverflowModes.Truncate;
            UiKit.Fix(s, -1f, subH);

            RectTransform actionStrip = null;
            if (withActionStrip)
            {
                actionStrip = UiKit.VStack(v, "Actions", 4f, 0, 0, 0, 0, TextAnchor.MiddleCenter);
                UiKit.Fix(actionStrip, -1f, stripH);
            }

            Button b = null;
            if (onClick != null && !locked)
            {
                b = card.gameObject.AddComponent<Button>();
                b.targetGraphic = card;
                UiKit.Press(b);
                b.onClick.AddListener(() => onClick());
            }

            return new ItemCardView
            {
                Go = card.gameObject,
                Icon = iconImg,
                Title = t,
                Sub = s,
                Button = b,
                ButtonLabel = null,
                ActionStrip = actionStrip
            };
        }

        /// <summary>Reward readability tile: icon + bold name + dark quantity bar.</summary>
        public static ItemCardView RewardTile(Transform parent, string name, string title, string amount,
            Sprite icon, Sprite rarityFrame, Vector2 cell, string badge = null, bool withActionStrip = false)
        {
            float stripH = withActionStrip ? 40f : 0f;
            var card = UiKit.Img(parent, name, KitPanel);
            FrameRarity(card, rarityFrame, Mathf.Min(cell.x, cell.y));
            var le = card.gameObject.GetComponent<LayoutElement>() ?? card.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = cell.x;
            le.preferredHeight = cell.y;
            le.minWidth = cell.x;
            le.minHeight = cell.y;

            var v = UiKit.VStack(card.transform, "V", 6f, 12, 12, 10, 12, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            float iconSize = Mathf.Min(cell.x - 40f, Mathf.Max(40f, cell.y - 96f - stripH));
            Image iconImg = null;
            if (icon != null)
            {
                iconImg = SimpleIcon(v, "Icon", icon, iconSize);
                UiKit.Fix(iconImg, -1f, iconSize);
            }

            string titleText = string.IsNullOrEmpty(badge) ? title : $"[{badge}] {title}";
            var t = UiKit.TmpLabel(v, "Title", titleText, UiKit.TmpCaption, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            UiKit.Fix(t, -1f, 26f);

            // Host + sibling label (Text nested under Image often ends up 0-size under VLG).
            var qtyHost = UiKit.Rect(v, "QtyHost");
            UiKit.Fix(qtyHost, -1f, 34f);
            var qtyBg = UiKit.Img(qtyHost, "QtyBar", new Color(0.08f, 0.1f, 0.14f, 0.95f));
            // 명시적으로 sprite=null이라 맹물 사각형이었다 → 키트 카드 프레임
            qtyBg.sprite = CasualArt.CardRound;
            qtyBg.type = qtyBg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            qtyBg.raycastTarget = false;
            UiKit.Fill(qtyBg.rectTransform);
            string qtyText = string.IsNullOrEmpty(amount) ? "0" : amount;
            var qty = UiKit.TmpLabel(qtyHost, "Amount", qtyText, UiKit.TmpBody, KitTeal,
                bold: true, TextAlignmentOptions.Center);
            qty.enableWordWrapping = false;
            UiKit.Fill(qty.rectTransform, 2f);

            RectTransform actionStrip = null;
            if (withActionStrip)
            {
                actionStrip = UiKit.HStack(v, "Actions", 4f, 0, 0, 0, 0, TextAnchor.MiddleCenter, true);
                UiKit.Fix(actionStrip, -1f, stripH);
            }

            return new ItemCardView
            {
                Go = card.gameObject,
                Icon = iconImg,
                Title = t,
                Sub = qty,
                Button = null,
                ButtonLabel = null,
                ActionStrip = actionStrip
            };
        }

        /// <summary>Add a compact button into a SkillTile ActionStrip row.</summary>
        public static Button TileAction(Transform strip, string name, string label, System.Action onClick, bool accent = false)
        {
            var btn = accent
                ? MapleUiTheme.AccentButton(strip, name, label, onClick, UiKit.FontCaption)
                : MapleUiTheme.SecondaryButton(strip, name, label, onClick, UiKit.FontCaption);
            UiKit.Fix(btn, -1f, 32f);
            var le = btn.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.flexibleWidth = 1f;
                le.minHeight = 32f;
                le.preferredHeight = 32f;
            }
            return btn;
        }

        /// <summary>Row of tile actions inside ActionStrip.</summary>
        public static RectTransform TileActionRow(Transform strip, string name)
        {
            var row = UiKit.HStack(strip, name, 4f, 0, 0, 0, 0, TextAnchor.MiddleCenter, true);
            UiKit.Fix(row, -1f, 36f);
            return row;
        }

        /// <summary>Slot_Summon / shop package row with dual CTAs.</summary>
        public static ItemCardView PackageRow(Transform parent, string name, string title, string sub,
            Sprite icon, string cta, System.Action onClick, float height = 150f)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            FrameList(card, height);
            UiKit.Fix(card, -1f, height);

            var h = UiKit.HStack(card.transform, "Row", 14f, 18, 18, 14, 14);
            UiKit.Fill(h);

            var iconFrame = UiKit.Img(h, "IconFrame", Color.white);
            Slice(iconFrame, GrowArt.IconFrame, 100f);
            UiKit.Fix(iconFrame, 100f, 100f);
            Image iconImg = null;
            if (icon != null)
            {
                iconImg = SimpleIcon(iconFrame.transform, "Icon", icon, 72f);
                UiKit.Fill(iconImg.rectTransform, 14f);
                iconImg.preserveAspect = true;
            }

            var mid = UiKit.VStack(h, "Mid", 8f, 0, 0, 4, 4, TextAnchor.MiddleLeft);
            UiKit.Flex(mid);
            var t = UiKit.TmpLabel(mid, "Title", title, UiKit.TmpBody + 2, UiKit.TextInverse, bold: true);
            t.enableWordWrapping = false;
            UiKit.Fix(t, -1f, 30f);
            var s = UiKit.TmpLabel(mid, "Sub", sub, UiKit.TmpCaption, UiKit.TextInverseDim);
            UiKit.Fix(s, -1f, 40f);

            Button b = null;
            TMPro.TMP_Text bl = null;
            if (cta != null)
            {
                b = MapleUiTheme.AccentButton(h, "Cta", cta, onClick, UiKit.FontBody);
                UiKit.Fix(b, 180f, 72f);
                bl = b.GetComponentInChildren<TMPro.TMP_Text>();
            }

            return new ItemCardView { Go = card.gameObject, Icon = iconImg, Title = t, Sub = s, Button = b, ButtonLabel = bl };
        }

        /// <summary>Equipped / companion slot card with large rarity edge + portrait area.</summary>
        public static ItemCardView PortraitCard(Transform parent, string name, string title, string sub,
            Sprite icon, Sprite rarity, System.Action onClick, float width = 220f, float height = 140f)
        {
            var card = UiKit.Img(parent, name, KitPanel);
            float sideHint = width > 0f ? Mathf.Min(width, height) : height;
            FrameRarity(card, rarity, sideHint);
            if (width < 0f)
            {
                var le = card.gameObject.GetComponent<LayoutElement>() ?? card.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = -1f;
                le.minWidth = 0f;
                le.flexibleWidth = 1f;
                le.preferredHeight = height;
                le.minHeight = height;
                MapleUiTheme.StretchFullWidth(card);
            }
            else
            {
                UiKit.Fix(card, width, height);
                var fixLe = card.gameObject.GetComponent<LayoutElement>();
                if (fixLe != null) { fixLe.flexibleWidth = 0f; fixLe.flexibleHeight = 0f; }
            }

            // Compact slots (sub companions) need tight padding so labels stay inside.
            bool compact = height < 100f || (width > 0f && width < 100f);
            float pad = compact ? 4f : 14f;
            float gap = compact ? 2f : 4f;
            var v = UiKit.VStack(card.transform, "V", gap, pad, pad, pad, pad + 2f, TextAnchor.UpperCenter);
            UiKit.Fill(v);

            float iconSize = compact
                ? Mathf.Clamp(Mathf.Min(width > 0f ? width : height, height) - 36f, 22f, 40f)
                : Mathf.Clamp(height * 0.45f, 48f, 80f);
            Image iconImg = null;
            if (icon != null)
            {
                iconImg = SimpleIcon(v, "Icon", icon, iconSize);
                UiKit.Fix(iconImg, -1f, iconSize);
            }
            else
            {
                var ph = UiKit.Img(v, "IconPh", new Color(1f, 1f, 1f, 0.08f));
                UiKit.Fix(ph, -1f, iconSize);
            }

            int titleSize = compact ? UiKit.TmpCaption - 3 : UiKit.TmpCaption;
            float titleH = compact ? 20f : 24f;
            float subH = compact ? 19f : 24f;
            var t = UiKit.TmpLabel(v, "Title", title, titleSize, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            UiKit.Fix(t, -1f, titleH);
            var s = UiKit.TmpLabel(v, "Sub", sub, compact ? UiKit.TmpCaption - 3 : UiKit.TmpCaption - 1, Color.white,
                bold: true, TextAlignmentOptions.Center);
            s.enableWordWrapping = false;
            s.overflowMode = TextOverflowModes.Ellipsis;
            UiKit.Fix(s, -1f, subH);

            Button b = null;
            if (onClick != null)
            {
                b = card.gameObject.AddComponent<Button>();
                b.targetGraphic = card;
                UiKit.Press(b);
                b.onClick.AddListener(() => onClick());
            }

            return new ItemCardView { Go = card.gameObject, Icon = iconImg, Title = t, Sub = s, Button = b };
        }

        /// <summary>Popup_Skill-style detail: vertical stack for left-rail width.</summary>
        public static SkillDetailView SkillDetailPanel(Transform parent, string name, System.Action onAction)
        {
            var card = UiKit.Img(parent, name, new Color(0.075f, 0.13f, 0.30f, 0.97f));
            var dtCard = CasualArt.CardRound;
            if (dtCard != null) { card.sprite = dtCard; card.type = Image.Type.Sliced; }
            else { card.sprite = null; card.type = Image.Type.Simple; }
            // Tall enough for icon / effect / desc / cost / reason / CTA without crushing children.
            UiKit.Fix(card, -1f, 480f);
            MapleUiTheme.StretchFullWidth(card);

            // Order: icon row ? effect ? desc ? cost ? reason ? CTA (bottom).
            var root = UiKit.VStack(card.transform, "Col", 8f, 12, 12, 12, 12, TextAnchor.UpperCenter);
            UiKit.Fill(root);

            var head = UiKit.HStack(root, "Head", 12f, 0, 0, 0, 0, TextAnchor.MiddleLeft);
            UiKit.Fix(head, -1f, 100f);

            var iconFrame = UiKit.Img(head, "IconFrame", Color.white);
            Slice(iconFrame, GrowArt.IconFrame, 88f);
            UiKit.Fix(iconFrame, 88f, 88f);
            var icon = SimpleIcon(iconFrame.transform, "Icon", GrowArt.SkillIcon(0), 64f);
            UiKit.Fill(icon.rectTransform, 10f);
            icon.preserveAspect = true;

            var mid = UiKit.VStack(head, "Mid", 4f, 0, 0, 2, 2, TextAnchor.MiddleLeft);
            UiKit.Flex(mid);

            var title = UiKit.TmpLabel(mid, "Title", "??", UiKit.FontH2, UiKit.TextInverse, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(title, -1f, 28f);
            title.enableWordWrapping = true;
            title.overflowMode = TextOverflowModes.Truncate;

            var rank = UiKit.TmpLabel(mid, "Rank", "Lv.0/10", UiKit.FontBody, KitTeal, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Fix(rank, -1f, 24f);

            var levelBar = MapleUiTheme.Bar(mid, "LvBar", KitTeal, true);
            UiKit.Fix(levelBar.Go.transform, -1f, 28f);

            var effect = UiKit.TmpLabel(root, "Effect", "", UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.UpperLeft);
            UiKit.Fix(effect, -1f, 32f);
            effect.enableWordWrapping = true;
            effect.overflowMode = TextOverflowModes.Truncate;

            var desc = UiKit.TmpLabel(root, "Desc", "", UiKit.FontBody + 1, UiKit.TextInverse, FontStyle.Normal, TextAnchor.UpperLeft);
            UiKit.Fix(desc, -1f, 80f);
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Truncate;

            var cost = UiKit.TmpLabel(root, "Cost", "", UiKit.FontBody + 1, KitTeal, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(cost, -1f, 44f);
            cost.enableWordWrapping = true;
            cost.overflowMode = TextOverflowModes.Truncate;

            var reason = UiKit.TmpLabel(root, "Reason", "", UiKit.FontBody, UiKit.Danger, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(reason, -1f, 32f);
            reason.enableWordWrapping = true;
            reason.overflowMode = TextOverflowModes.Truncate;

            var act = MapleUiTheme.AccentButton(root, "Act", "??", onAction, UiKit.FontBody);
            UiKit.Fix(act, -1f, 64f);
            var actLabel = act.GetComponentInChildren<TMPro.TMP_Text>();

            return new SkillDetailView
            {
                Go = card.gameObject,
                Icon = icon,
                Title = title,
                Rank = rank,
                Desc = desc,
                Effect = effect,
                Cost = cost,
                Reason = reason,
                LevelBar = levelBar,
                Action = act,
                ActionLabel = actLabel
            };
        }
    }
}
