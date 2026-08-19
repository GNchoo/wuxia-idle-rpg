using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI
{
    /// <summary>
    /// MVP top strip parented under template Content_Panel (phone-safe frame).
    /// Reuses sprites already referenced by WalletView / template UI when possible.
    /// </summary>
    public class MvpHudController : MonoBehaviour
    {
        static readonly Color Cream = new Color(0.98f, 0.94f, 0.82f, 1f);
        static readonly Color Gold = new Color(1f, 0.86f, 0.42f, 1f);
        static readonly Color SoftGreen = new Color(0.72f, 0.95f, 0.7f, 1f);

        Text _stageText;
        Text _lootText;
        Text _growthText;
        Text _claimResultText;
        Button _claimButton;

        void Start()
        {
            StartCoroutine(BootHud());
        }

        System.Collections.IEnumerator BootHud()
        {
            KoreanUiFont.Get();
            // Wait until template phone-safe Content_Panel exists.
            for (int i = 0; i < 60 && GameObject.Find("Content_Panel") == null; i++)
                yield return null;

            BuildHud();
            Refresh();

            if (StageProgress.Instance != null)
                StageProgress.Instance.OnChanged += Refresh;
            if (LootBoxService.Instance != null)
                LootBoxService.Instance.OnChanged += Refresh;
            if (PlayerGrowth.Instance != null)
                PlayerGrowth.Instance.OnChanged += Refresh;
        }

        void OnDestroy()
        {
            if (StageProgress.Instance != null)
                StageProgress.Instance.OnChanged -= Refresh;
            if (LootBoxService.Instance != null)
                LootBoxService.Instance.OnChanged -= Refresh;
            if (PlayerGrowth.Instance != null)
                PlayerGrowth.Instance.OnChanged -= Refresh;
        }

        void BuildHud()
        {
            var parent = ResolvePhoneSafeParent();
            if (parent == null)
            {
                Debug.LogError("[IdleMvp] Content_Panel not found — cannot place HUD in phone frame.");
                return;
            }

            // Destroy previous overlay HUD if any (from older builds).
            var old = transform.Find("IdleMvp_HUD_Canvas");
            if (old != null)
                Destroy(old.gameObject);
            var oldOverlay = GameObject.Find("IdleMvp_HUD_Canvas");
            if (oldOverlay != null)
                Destroy(oldOverlay);

            var panel = CreatePanel(parent, "IdleMvp_Panel",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -210f), new Vector2(980f, 150f));
            panel.SetAsLastSibling();

            var frameSprite = FindTemplateSprite("BACKGROUND (frame)", "BACKGROUND POPUP SQUARE with frame", "Empty Square Frame")
                              ?? LoadResourceSprite("MvpUi/PanelFrame")
                              ?? LoadResourceSprite("MvpUi/SquareFrame");
            var fogSprite = FindTemplateSprite("TOP BAR - FOG", "FOG behind currency")
                            ?? LoadResourceSprite("MvpUi/TopBarFog");
            var btnSprite = FindTemplateSprite("upgrade button", "Multiply button")
                            ?? LoadResourceSprite("MvpUi/UpgradeButton");

            if (fogSprite != null)
            {
                var fogGo = new GameObject("TopBarFog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fogGo.transform.SetParent(panel, false);
                var fogRt = fogGo.GetComponent<RectTransform>();
                StretchFull(fogRt, new Vector2(-8, -4), new Vector2(8, 6));
                var fogImg = fogGo.GetComponent<Image>();
                fogImg.sprite = fogSprite;
                fogImg.type = Image.Type.Simple;
                fogImg.color = new Color(1f, 1f, 1f, 0.9f);
                fogImg.raycastTarget = false;
            }

            var bg = panel.gameObject.AddComponent<Image>();
            if (frameSprite != null)
            {
                bg.sprite = frameSprite;
                bg.type = frameSprite.border.sqrMagnitude > 0.01f ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = Color.white;
            }
            else
            {
                // Last resort: match wallet chip dark fill, not flat debug black.
                bg.color = new Color(0.18f, 0.12f, 0.08f, 0.92f);
                Debug.LogWarning("[IdleMvp] Template frame sprite not found; using tinted fallback.");
            }

            _stageText = CreateUiText(panel, "StageText", 24, FontStyle.Bold,
                new Vector2(0, 40), new Vector2(900, 34), Cream);
            _lootText = CreateUiText(panel, "LootText", 18, FontStyle.Normal,
                new Vector2(0, 8), new Vector2(900, 28), Gold);
            _growthText = CreateUiText(panel, "GrowthText", 17, FontStyle.Normal,
                new Vector2(0, -22), new Vector2(900, 26), Cream);
            _claimResultText = CreateUiText(panel, "ClaimResult", 15, FontStyle.Italic,
                new Vector2(-90, -50), new Vector2(560, 24), SoftGreen);

            var btnGo = new GameObject("ClaimButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = btnRt.anchorMax = new Vector2(1f, 0f);
            btnRt.pivot = new Vector2(1f, 0f);
            btnRt.anchoredPosition = new Vector2(-18, 12);
            btnRt.sizeDelta = new Vector2(200, 52);

            var btnImg = btnGo.GetComponent<Image>();
            if (btnSprite != null)
            {
                btnImg.sprite = btnSprite;
                btnImg.type = btnSprite.border.sqrMagnitude > 0.01f ? Image.Type.Sliced : Image.Type.Simple;
                btnImg.color = Color.white;
            }
            else
            {
                btnImg.color = new Color(0.85f, 0.65f, 0.15f, 0.95f);
            }

            _claimButton = btnGo.GetComponent<Button>();
            var btnLabel = CreateUiText(btnGo.transform, "Label", 20, FontStyle.Bold,
                Vector2.zero, new Vector2(190, 44), Cream);
            btnLabel.text = "전리품 수령";
            btnLabel.alignment = TextAnchor.MiddleCenter;
            _claimButton.onClick.AddListener(OnClaimClicked);

            Debug.Log($"[IdleMvp] HUD parented under {parent.name}. frame={(frameSprite != null ? frameSprite.name : "null")} btn={(btnSprite != null ? btnSprite.name : "null")}");
        }

        static Transform ResolvePhoneSafeParent()
        {
            var content = GameObject.Find("Content_Panel");
            if (content != null) return content.transform;

            var wallet = GameObject.Find("WalletView");
            if (wallet != null) return wallet.transform;

            var ui = GameObject.Find("UI");
            return ui != null ? ui.transform : null;
        }

        /// <summary>Pull sprites already used by template UI (Wallet / buttons / frames).</summary>
        static Sprite FindTemplateSprite(params string[] nameHints)
        {
            var images = Object.FindObjectsOfType<Image>(true);
            foreach (var hint in nameHints)
            {
                string h = hint.ToLowerInvariant();
                foreach (var img in images)
                {
                    if (img == null || img.sprite == null) continue;
                    if (img.sprite.name.ToLowerInvariant().Contains(h))
                        return img.sprite;
                }
            }

            var renderers = Object.FindObjectsOfType<SpriteRenderer>(true);
            foreach (var hint in nameHints)
            {
                string h = hint.ToLowerInvariant();
                foreach (var sr in renderers)
                {
                    if (sr == null || sr.sprite == null) continue;
                    if (sr.sprite.name.ToLowerInvariant().Contains(h))
                        return sr.sprite;
                }
            }

            return null;
        }

        static Sprite LoadResourceSprite(string resourcesPath)
        {
            var sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite != null) return sprite;
            var tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        void OnClaimClicked()
        {
            if (LootBoxService.Instance == null) return;
            var (g, x, s) = LootBoxService.Instance.Claim();
            _claimResultText.text = $"수령: Gold {g:0} / XP {x:0} / 강화석 {s:0.#}";
            Refresh();
        }

        void Refresh()
        {
            if (_stageText != null && StageProgress.Instance != null)
            {
                var sp = StageProgress.Instance;
                string cpHint = $"CP {CombatPowerService.GetTotalCp():0} / 권장 {sp.RecommendedCp:0}";
                _stageText.text = $"{sp.GetDisplayLabel()}   {cpHint}";
            }

            if (_lootText != null && LootBoxService.Instance != null)
            {
                var lb = LootBoxService.Instance;
                _lootText.text =
                    $"상자  Gold {lb.PendingGold:0} | XP {lb.PendingXp:0} | 강화석 {lb.PendingEnhanceStone:0.#}";
            }

            if (_growthText != null && PlayerGrowth.Instance != null)
            {
                var bd = CombatPowerService.GetBreakdown();
                var p = PlayerGrowth.Instance;
                _growthText.text =
                    $"Lv.{p.Level}  XP {p.CurrentXp}/{p.XpToNext}  ATK{bd.Atk:0.#}/HP{bd.MaxHp:0}/DEF{bd.Def:0.#}";
            }
        }

        static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null)
                Destroy(existing.gameObject);

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        static void StretchFull(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        static Text CreateUiText(Transform parent, string name, int size, FontStyle style,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var text = go.GetComponent<Text>();
            KoreanUiFont.Apply(text);
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = string.Empty;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.08f, 0.05f, 0.02f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }
    }
}
