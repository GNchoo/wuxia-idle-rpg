using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    public class HeroCardView
    {
        public GameObject Go;
        public Image Card;
        public Image Portrait;
        public TMP_Text Name;
        public TMP_Text Level;
        public TMP_Text Progress;
        public Button Button;
        public GameObject[] StarsOn;
    }

    /// <summary>
    /// GUI Pro demo Character_List hero card, rebuilt at native 415x585 from the
    /// exact prefab recipe (CardFrame03 + Slider_Level01 + StarGrade icons),
    /// then uniformly scaled into the grid cell so proportions never distort.
    /// </summary>
    public static class CasualCards
    {
        public const float NativeW = 415f;
        public const float NativeH = 585f;

        static readonly Color[] GradTint =
        {
            new Color(0.043f, 0.455f, 0.808f, 1f), // Blue
            new Color(0.180f, 0.620f, 0.075f, 1f), // Green
            new Color(0.560f, 0.235f, 0.800f, 1f), // Purple
            new Color(0.850f, 0.430f, 0.050f, 1f), // Orange
            new Color(0.32f, 0.36f, 0.44f, 1f),    // Dim
        };

        /// <summary>rarity → kit card color name (0 common..3+ top, -1 locked/dim).</summary>
        static string CardColor(int rarity, bool dim)
        {
            if (dim) return "Dim";
            switch (rarity)
            {
                case 0: return "Blue";
                case 1: return "Green";
                case 2: return "Purple";
                default: return "Orange";
            }
        }

        static Color Grad(int rarity, bool dim)
        {
            if (dim) return GradTint[4];
            return GradTint[Mathf.Clamp(rarity, 0, 3)];
        }

        public static HeroCardView HeroCard(Transform parent, string goName, string title,
            Sprite portrait, Color portraitTint, int rarity, int stars, int maxStars,
            string levelText, float progress, string progressText, bool dim,
            System.Action onClick, Vector2 cell)
        {
            // wrapper sized to the grid cell; native card scaled inside
            var wrap = new GameObject(goName, typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrapRt = (RectTransform)wrap.transform;
            wrapRt.sizeDelta = cell;
            var le = wrap.AddComponent<LayoutElement>();
            le.minWidth = cell.x; le.minHeight = cell.y;
            le.preferredWidth = cell.x; le.preferredHeight = cell.y;

            float scale = Mathf.Min(cell.x / NativeW, cell.y / NativeH);
            var root = new GameObject("Card", typeof(RectTransform));
            root.transform.SetParent(wrap.transform, false);
            var rootRt = (RectTransform)root.transform;
            rootRt.sizeDelta = new Vector2(NativeW, NativeH);
            rootRt.localScale = new Vector3(scale, scale, 1f);

            var card = root.AddComponent<Image>();
            var cardSprite = CasualArt.C("CardFrame03_Single_" + CardColor(rarity, dim));
            if (cardSprite != null) card.sprite = cardSprite;
            else { card.sprite = MapleLightTheme.RoundedSprite(16); card.color = Grad(rarity, dim); }
            card.type = Image.Type.Sliced;
            card.raycastTarget = true;

            // portrait zone (masked): glow + character + bottom gradient
            var mask = new GameObject("Mask", typeof(RectTransform), typeof(RectMask2D));
            mask.transform.SetParent(root.transform, false);
            var maskRt = (RectTransform)mask.transform;
            maskRt.anchorMin = new Vector2(0f, 0f);
            maskRt.anchorMax = new Vector2(1f, 1f);
            maskRt.offsetMin = new Vector2(4f, 150f);
            maskRt.offsetMax = new Vector2(-4f, -80f);

            var glow = UiKit.Img(mask.transform, "Glow", new Color(1f, 1f, 1f, 0.30f));
            glow.sprite = CasualArt.C("CardFrame02_Glow");
            glow.raycastTarget = false;
            var glowRt = glow.rectTransform;
            glowRt.anchorMin = glowRt.anchorMax = new Vector2(0.5f, 0.55f);
            glowRt.sizeDelta = new Vector2(440f, 440f);

            Image chara = null;
            if (portrait != null)
            {
                chara = UiKit.Img(mask.transform, "Character", Color.white);
                chara.sprite = portrait;
                chara.color = portraitTint;
                chara.preserveAspect = true;
                chara.raycastTarget = false;
                var chRt = chara.rectTransform;
                chRt.anchorMin = chRt.anchorMax = new Vector2(0.5f, 0.52f);
                chRt.sizeDelta = new Vector2(330f, 330f);
            }

            var grad = UiKit.Img(mask.transform, "Gradient", Grad(rarity, dim));
            grad.sprite = CasualArt.C("CardFrame03_Gradient");
            grad.raycastTarget = false;
            var gradRt = grad.rectTransform;
            gradRt.anchorMin = new Vector2(0f, 0f);
            gradRt.anchorMax = new Vector2(1f, 0f);
            gradRt.pivot = new Vector2(0.5f, 0f);
            gradRt.sizeDelta = new Vector2(0f, 165f);
            gradRt.anchoredPosition = Vector2.zero;

            // top level strip: navy bg + yellow fill + hex badge + progress text
            var strip = UiKit.Img(root.transform, "Slider", new Color(0.031f, 0.078f, 0.243f, 1f));
            strip.sprite = CasualArt.C("Slider_Level01_Bg");
            strip.type = Image.Type.Sliced;
            var stripRt = strip.rectTransform;
            stripRt.anchorMin = new Vector2(0f, 1f);
            stripRt.anchorMax = new Vector2(1f, 1f);
            stripRt.pivot = new Vector2(0.5f, 1f);
            stripRt.offsetMin = new Vector2(52f, -86f);
            stripRt.offsetMax = new Vector2(-16f, -14f);

            var fill = UiKit.Img(strip.transform, "Fill", Color.white);
            fill.sprite = CasualArt.C("Slider_Level01_Fill_Yellow");
            fill.type = Image.Type.Sliced;
            fill.raycastTarget = false;
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            fillRt.offsetMin = new Vector2(6f, 6f);
            fillRt.offsetMax = new Vector2(progress >= 0.999f ? -6f : 0f, -6f);
            if (progress <= 0.01f) fill.enabled = false;

            var progT = UiKit.TmpLabel(strip.transform, "Text", progressText ?? "", 30, Color.white,
                bold: true, TextAlignmentOptions.Center);
            progT.enableWordWrapping = false;
            UiKit.Fill(progT.rectTransform, 4f);

            var badge = UiKit.Img(root.transform, "Badge", Color.white);
            badge.sprite = CasualArt.C(progress >= 0.999f ? "Slider_Level01_Badge_Yellow" : "Slider_Level01_Badge_Blue");
            badge.raycastTarget = false;
            var badgeRt = badge.rectTransform;
            badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0f, 1f);
            badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.sizeDelta = new Vector2(107f, 119f);
            badgeRt.anchoredPosition = new Vector2(46f, -50f);

            var lvlT = UiKit.TmpLabel(badge.transform, "Text", levelText ?? "", 44, Color.white,
                bold: true, TextAlignmentOptions.Center);
            lvlT.enableWordWrapping = false;
            UiKit.Fill(lvlT.rectTransform, 8f);

            // stars
            var starsOn = new GameObject[Mathf.Max(0, maxStars)];
            if (maxStars > 0)
            {
                var starRow = UiKit.HStack(root.transform, "Stars", -4f, 0, 0, 0, 0, TextAnchor.MiddleCenter);
                var srRt = starRow.GetComponent<RectTransform>();
                srRt.anchorMin = new Vector2(0.5f, 0f);
                srRt.anchorMax = new Vector2(0.5f, 0f);
                srRt.pivot = new Vector2(0.5f, 0f);
                srRt.sizeDelta = new Vector2(maxStars * 42f, 48f);
                srRt.anchoredPosition = new Vector2(0f, 96f);
                var offS = CasualArt.C("Icon_ImageIcon_StarGrade_l_Off");
                var onS = CasualArt.C("Icon_ImageIcon_StarGrade_l_On");
                for (int i = 0; i < maxStars; i++)
                {
                    var off = UiKit.Img(starRow, "S" + i, Color.white);
                    if (offS != null) off.sprite = offS;
                    off.raycastTarget = false;
                    UiKit.Fix(off, 42f, 44f);
                    var on = UiKit.Img(off.transform, "On", Color.white);
                    if (onS != null) on.sprite = onS;
                    on.raycastTarget = false;
                    UiKit.Fill(on.rectTransform, 0f);
                    on.gameObject.SetActive(i < stars);
                    starsOn[i] = on.gameObject;
                }
            }

            // name band (bottom of card sprite has the colored band)
            var nameT = UiKit.TmpLabel(root.transform, "Name", title, 40, Color.white,
                bold: true, TextAlignmentOptions.Center);
            nameT.enableWordWrapping = false;
            var nameRt = nameT.rectTransform;
            nameRt.anchorMin = new Vector2(0f, 0f);
            nameRt.anchorMax = new Vector2(1f, 0f);
            nameRt.pivot = new Vector2(0.5f, 0f);
            nameRt.offsetMin = new Vector2(14f, 18f);
            nameRt.offsetMax = new Vector2(-14f, 86f);

            Button btn = null;
            if (onClick != null)
            {
                btn = root.AddComponent<Button>();
                btn.targetGraphic = card;
                UiKit.Press(btn);
                btn.onClick.AddListener(IdleMvp.Core.AudioService.Click);
                btn.onClick.AddListener(() => onClick());
            }

            return new HeroCardView
            {
                Go = wrap, Card = card, Portrait = chara, Name = nameT,
                Level = lvlT, Progress = progT, Button = btn, StarsOn = starsOn
            };
        }

        /// <summary>Demo stage-map hexagon node: hex + glow ring + icon + label under.</summary>
        public static ItemCardView HexNode(Transform parent, string name, Sprite icon, Color iconTint,
            System.Action onClick, Vector2 cell)
        {
            var wrap = new GameObject(name, typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var le = wrap.AddComponent<LayoutElement>();
            le.minWidth = cell.x; le.preferredWidth = cell.x;
            le.minHeight = cell.y; le.preferredHeight = cell.y;
            le.flexibleWidth = 0f;

            float hexSize = cell.x - 12f;
            var hex = UiKit.Img(wrap.transform, "Hex", Color.white);
            var hexSprite = CasualArt.C("Button_Hexagon199_Blue");
            if (hexSprite != null) hex.sprite = hexSprite;
            else { hex.sprite = MapleLightTheme.RoundedSprite(24); hex.color = new Color(0.13f, 0.42f, 0.85f, 1f); }
            var hexRt = hex.rectTransform;
            hexRt.anchorMin = hexRt.anchorMax = new Vector2(0.5f, 1f);
            hexRt.pivot = new Vector2(0.5f, 1f);
            hexRt.sizeDelta = new Vector2(hexSize, hexSize);
            hexRt.anchoredPosition = new Vector2(0f, -4f);

            // 강조는 슬롯과 '같은 육각형 실루엣'이어야 한다.
            // White_Shadow는 드롭섀도우 아트(161x102)라 모양이 안 맞고, null이면 스프라이트 없는
            // 맹물 사각형이 그려져 마름모처럼 보였다. 항상 육각형 스프라이트로 고정한다.
            var glow = UiKit.Img(hex.transform, "Glow", new Color(0.55f, 0.90f, 1f, 0.95f));
            glow.sprite = CasualArt.HexWhite ?? hexSprite ?? hex.sprite;
            glow.preserveAspect = false;
            glow.raycastTarget = false;
            var glowRt = glow.rectTransform;
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-10f, -10f);
            glowRt.offsetMax = new Vector2(10f, 10f);
            glow.transform.SetAsFirstSibling();
            glow.gameObject.SetActive(false);

            Image ic = null;
            if (icon != null)
            {
                ic = UiKit.Sprite(hex.transform, "Icon", icon);
                ic.color = iconTint;
                ic.preserveAspect = true;
                ic.raycastTarget = false;
                var icRt = ic.rectTransform;
                icRt.anchorMin = icRt.anchorMax = new Vector2(0.5f, 0.54f);
                icRt.sizeDelta = new Vector2(hexSize * 0.48f, hexSize * 0.48f);
            }

            // 섹션 박스가 남색이므로 밝은 글씨 (예전엔 흰 박스라 어두운 글씨였다)
            var sub = UiKit.TmpLabel(wrap.transform, "Sub", "", 22, new Color(0.88f, 0.92f, 0.98f, 1f),
                bold: true, TextAlignmentOptions.Center);
            sub.enableWordWrapping = true;
            var subRt = sub.rectTransform;
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0f);
            subRt.pivot = new Vector2(0.5f, 0f);
            subRt.offsetMin = new Vector2(-8f, 0f);
            subRt.offsetMax = new Vector2(8f, 56f);

            var hiddenTitle = UiKit.TmpLabel(wrap.transform, "Title", "", 10, Color.clear,
                bold: false, TextAlignmentOptions.Center);
            hiddenTitle.gameObject.SetActive(false);

            Button btn = null;
            if (onClick != null)
            {
                btn = hex.gameObject.AddComponent<Button>();
                btn.targetGraphic = hex;
                UiKit.Press(btn);
                btn.onClick.AddListener(IdleMvp.Core.AudioService.Click);
                btn.onClick.AddListener(() => onClick());
            }

            return new ItemCardView { Go = hex.gameObject, Icon = ic, Title = hiddenTitle, Sub = sub, Button = btn };
        }

        /// <summary>Connector line between hex nodes (demo stage map path).</summary>
        public static void HexLink(Transform parent, float hexTopOffset = 64f)
        {
            var wrap = new GameObject("Link", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var le = wrap.AddComponent<LayoutElement>();
            le.minWidth = 34f; le.preferredWidth = 34f;
            le.minHeight = 10f; le.flexibleWidth = 0f;
            var line = UiKit.Img(wrap.transform, "L", new Color(0.35f, 0.55f, 0.85f, 0.85f));
            line.sprite = MapleLightTheme.RoundedSprite(4);
            line.type = UnityEngine.UI.Image.Type.Sliced;
            line.raycastTarget = false;
            var lrt = line.rectTransform;
            lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f);
            lrt.pivot = new Vector2(0.5f, 1f);
            lrt.offsetMin = new Vector2(-8f, -hexTopOffset - 8f);
            lrt.offsetMax = new Vector2(8f, -hexTopOffset);
        }
    }
}
