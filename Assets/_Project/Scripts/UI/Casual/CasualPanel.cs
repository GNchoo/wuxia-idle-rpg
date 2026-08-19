using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Casual
{
    /// <summary>
    /// 구매 에셋(GUI Pro - Casual Game)의 완성 패널 프리팹을 그대로 띄우는 래퍼.
    ///
    /// 이 에셋에는 Prefabs_DemoScene_Panels 아래에 55개의 완성된 화면 프리팹이 들어있고,
    /// 그중 필요한 것들을 Resources/CasualPrefabs 로 복사해 뒀다. 지금까지는 이걸 안 쓰고
    /// 런타임에 uGUI를 손으로 그려서 품질이 떨어졌다. 앞으로 새 화면은 손으로 그리지 말고
    /// 프리팹을 띄운 뒤 이름으로 찾아 값만 꽂는다.
    ///
    /// 프리팹은 2560x1440 기준으로 제작돼 있어 1920x1080 캔버스에선 1.33배 크게 나온다.
    /// Load()가 캔버스 논리 크기에 맞춰 자동으로 스케일을 보정한다.
    /// </summary>
    public class CasualPanel
    {
        /// <summary>프리팹 제작 기준 해상도.</summary>
        public static readonly Vector2 DesignSize = new Vector2(2560f, 1440f);

        public GameObject Go { get; private set; }
        public RectTransform Root { get; private set; }

        readonly Dictionary<string, Transform> _cache = new Dictionary<string, Transform>(32);

        public bool Valid => Go != null;

        public static CasualPanel Load(string prefabName, Transform parent)
        {
            var prefab = Resources.Load<GameObject>("CasualPrefabs/" + prefabName);
            if (prefab == null)
            {
                Debug.LogWarning("[CasualPanel] 프리팹 없음: CasualPrefabs/" + prefabName);
                return null;
            }

            var go = UnityEngine.Object.Instantiate(prefab, parent);
            go.name = prefabName;
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();

            // 캔버스 전체를 덮되, 내부 팝업이 설계 비율대로 보이도록 스케일 보정
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localPosition = Vector3.zero;

            var canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
            float scale = 1f;
            if (canvas != null)
            {
                var canvasRt = canvas.GetComponent<RectTransform>();
                if (canvasRt != null && canvasRt.rect.height > 1f)
                    scale = canvasRt.rect.height / DesignSize.y;
            }
            if (scale <= 0.01f || float.IsNaN(scale)) scale = 1f;
            rt.localScale = new Vector3(scale, scale, 1f);

            ApplyKoreanFont(go);
            StretchDim(go, scale);
            HideDemoBadges(go);
            EnsureScrollMasks(go);
            DisableDecorRaycast(go);
            // 한글화를 '띄우기 직전'이 아니라 '만들자마자' 돌린다.
            // 예전엔 맨 마지막에 돌아서, 텍스트 내용으로 조건 분기하는 빌더 코드가
            // 첫 열기 때 영문을 보고 조건이 빗나갔다(2배 받기 색이 안 먹던 원인).
            Localizer?.Invoke(go);
            return new CasualPanel { Go = go, Root = rt };
        }

        /// <summary>CasualScreens가 주입하는 영문→한글 치환기.</summary>
        public static System.Action<GameObject> Localizer;

        /// <summary>
        /// 글로우·배경 같은 장식 이미지가 raycastTarget을 켠 채 목록 위를 덮고 있어
        /// 클릭을 먹어버린다. 순수 장식은 클릭 대상에서 빼 준다.
        /// </summary>
        static readonly string[] DecorNames = { "Glow", "Glow_Circle", "Gradient", "Shadow", "Light", "Background" };

        static void DisableDecorRaycast(GameObject root)
        {
            if (root == null) return;
            var imgs = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < imgs.Length; i++)
            {
                if (!imgs[i].raycastTarget) continue;
                if (imgs[i].GetComponent<Button>() != null) continue;   // 버튼 배경은 유지
                string n = imgs[i].name;
                for (int k = 0; k < DecorNames.Length; k++)
                {
                    if (n != DecorNames[k]) continue;
                    imgs[i].raycastTarget = false;
                    break;
                }
            }
        }

        /// <summary>
        /// 키트 프리팹의 ScrollRect는 viewport가 비어 있는 게 많다.
        /// 그러면 마스킹이 없어서 목록이 창 밖으로 그대로 삐져나와 그려진다
        /// (스킬 노드가 패널 왼쪽 밖으로 잘려 보이던 것, 상점 카드가 오른쪽으로 넘치던 것).
        /// viewport가 지정돼 있으면 프리팹 의도를 존중하고 건드리지 않는다.
        /// </summary>
        static void EnsureScrollMasks(GameObject root)
        {
            if (root == null) return;
            var scrolls = root.GetComponentsInChildren<ScrollRect>(true);
            for (int i = 0; i < scrolls.Length; i++)
            {
                var sr = scrolls[i];
                if (sr == null || sr.viewport != null) continue;
                var rt = sr.GetComponent<RectTransform>();
                if (rt == null) continue;
                if (rt.GetComponent<RectMask2D>() == null) rt.gameObject.AddComponent<RectMask2D>();
                sr.viewport = rt;
            }
        }

        /// <summary>
        /// 프리팹 우상단의 '홈(2)' 같은 데모 알림 뱃지는 우리 게임에 대응 기능이 없다.
        /// 그냥 배경처럼 떠 있어 혼란만 주므로 숨긴다.
        /// </summary>
        static void HideDemoBadges(GameObject root)
        {
            if (root == null) return;
            string[] names = { "Button_Home", "Alarm", "Text_Alram", "Icon_Alarm", "Home" };
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                for (int k = 0; k < names.Length; k++)
                {
                    if (all[i].name != names[k]) continue;
                    all[i].gameObject.SetActive(false);
                    break;
                }
            }
        }

        /// <summary>
        /// Dimed(뒷배경 어둡게)는 프리팹에 고정 크기로 들어있어서, 루트를 축소하면
        /// 화면을 못 덮고 검은 박스처럼 보인다. 화면 전체를 덮도록 넉넉히 늘린다.
        /// </summary>
        static void StretchDim(GameObject root, float scale)
        {
            if (root == null) return;
            var all = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "Dimed" && all[i].name != "Dim") continue;
                all[i].anchorMin = Vector2.zero;
                all[i].anchorMax = Vector2.one;
                all[i].pivot = new Vector2(0.5f, 0.5f);
                all[i].anchoredPosition = Vector2.zero;
                // 루트가 scale배 축소돼 있으니 그만큼 더 크게 잡아야 화면을 덮는다
                float pad = scale > 0.01f ? (1f / scale) * 2000f : 2000f;
                all[i].sizeDelta = new Vector2(pad, pad);
            }
        }

        /// <summary>
        /// 키트 프리팹의 TMP 폰트는 LiberationSans SDF라 한글 글리프가 없어서 전부 □로 나온다.
        /// 프리팹을 띄울 때마다 프로젝트 한글 폰트로 갈아준다. (프리팹 원본은 건드리지 않는다)
        /// </summary>
        public static void ApplyKoreanFont(GameObject root)
        {
            if (root == null) return;
            var font = Maple.UiKit.TmpFont;
            if (font == null) return;
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null || texts[i].font == font) continue;
                texts[i].font = font;
                // 폰트를 바꾸면 원본 머티리얼 프리셋이 안 맞아 아웃라인이 깨진다 → 기본으로
                texts[i].fontSharedMaterial = font.material;
            }
        }

        // ---- 이름으로 찾기 (프리팹 노드 이름이 명확해서 경로 없이 충분하다) --------

        public Transform Find(string name)
        {
            if (Go == null || string.IsNullOrEmpty(name)) return null;
            Transform hit;
            if (_cache.TryGetValue(name, out hit)) return hit;

            var all = Go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == name) { hit = all[i]; break; }
            }
            _cache[name] = hit;   // 못 찾은 것도 캐시 (매번 전체 순회 방지)
            if (hit == null) Debug.LogWarning("[CasualPanel] " + Go.name + " 에 '" + name + "' 없음");
            return hit;
        }

        /// <summary>이름 앞부분만 일치해도 찾는다 (Text_+50 처럼 값이 이름에 박힌 노드 대응).</summary>
        public Transform FindStartsWith(string prefix)
        {
            if (Go == null || string.IsNullOrEmpty(prefix)) return null;
            var all = Go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name.StartsWith(prefix)) return all[i];
            return null;
        }

        public T Get<T>(string name) where T : Component
        {
            var t = Find(name);
            return t != null ? t.GetComponent<T>() : null;
        }

        // ---- 값 꽂기 -----------------------------------------------------------

        public void SetText(string node, string value)
        {
            var t = Find(node);
            if (t == null) return;
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp != null) { tmp.text = value; return; }
            var legacy = t.GetComponent<Text>();
            if (legacy != null) legacy.text = value;
        }

        public void SetTextStartsWith(string prefix, string value)
        {
            var t = FindStartsWith(prefix);
            if (t == null) return;
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp != null) { tmp.text = value; return; }
            var legacy = t.GetComponent<Text>();
            if (legacy != null) legacy.text = value;
        }

        public void SetSprite(string node, Sprite sprite, bool keepIfNull = true)
        {
            if (sprite == null && keepIfNull) return;
            var img = Get<Image>(node);
            if (img != null) img.sprite = sprite;
        }

        public void SetColor(string node, Color c)
        {
            var g = Find(node);
            if (g == null) return;
            var img = g.GetComponent<Image>();
            if (img != null) { img.color = c; return; }
            var tmp = g.GetComponent<TMP_Text>();
            if (tmp != null) tmp.color = c;
        }

        public void SetActive(string node, bool on)
        {
            var t = Find(node);
            if (t != null && t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        }

        /// <summary>기존 리스너를 지우고 새로 연결한다 (재사용 시 중복 방지).</summary>
        public void OnClick(string node, Action action)
        {
            var b = Get<Button>(node);
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(Core.AudioService.Click);
            if (action != null) b.onClick.AddListener(() => action());
        }

        public void SetInteractable(string node, bool on)
        {
            var b = Get<Button>(node);
            if (b != null) b.interactable = on;
        }

        /// <summary>자식 노드를 템플릿으로 N개 복제한다 (목록/그리드 채우기).</summary>
        public List<Transform> Repeat(string templateNode, int count)
        {
            var list = new List<Transform>(Mathf.Max(0, count));
            var tpl = Find(templateNode);
            if (tpl == null || count <= 0) return list;
            var parent = tpl.parent;

            // 기존 복제본 제거 (원본은 남긴다)
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var ch = parent.GetChild(i);
                if (ch != tpl && ch.name.StartsWith(tpl.name + "_c"))
                {
                    ch.SetParent(null, false);
                    UnityEngine.Object.Destroy(ch.gameObject);
                }
            }

            tpl.gameObject.SetActive(true);
            list.Add(tpl);
            for (int i = 1; i < count; i++)
            {
                var clone = UnityEngine.Object.Instantiate(tpl, parent);
                clone.name = tpl.name + "_c" + i;
                list.Add(clone);
            }
            return list;
        }

        // ---- 표시 ------------------------------------------------------------

        public void Show()
        {
            if (Go == null) return;
            Go.SetActive(true);
            Go.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            if (Go != null) Go.SetActive(false);
        }

        public bool IsShown => Go != null && Go.activeSelf;

        /// <summary>Button_Close / Dimed 클릭을 닫기로 연결한다.</summary>
        public void WireClose(Action onClose = null)
        {
            OnClick("Button_Close", () => { Hide(); onClose?.Invoke(); });
        }
    }
}
