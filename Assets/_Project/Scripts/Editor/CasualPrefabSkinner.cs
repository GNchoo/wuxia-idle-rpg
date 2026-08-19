using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// 구매 프리팹 화면(CasualPrefabs)의 킷 스프라이트를 무협 킷(WuxiaUi)으로 일괄 교체.
    /// 스프라이트 '이름 패턴' 매칭이라 프리팹 구조 무관 — 재실행 안전(이미 교체된 건 그대로).
    /// 되돌리기는 git checkout.
    /// </summary>
    public static class CasualPrefabSkinner
    {
        const string Dir = "Assets/_Project/Resources/CasualPrefabs";
        const string Wux = "Assets/_Project/Resources/WuxiaUi/";

        // 스프라이트 이름 포함 패턴 → 무협 에셋 (순서 중요: 구체적인 것 먼저)
        static readonly (string pattern, string wux)[] Map =
        {
            // 2층 구조 재매핑: 이전 패스 산출물(panel_hanji/panel_dark) → 크기별 나무/종이.
            // panel_hanji/panel_dark는 아래 특수 규칙(크기 휴리스틱)이 처리한다.
            // 다크+금 언어 확산 (v12): 이전 패스의 밝은 오크 산출물 → 다크 세트
            ("tab_on", "btn_dark"),
            ("tab_off", "btn_dark"),
            ("wood_board", "panel_dg"),
            ("row_dark", "row_dim"),
            ("btn_primary", "btn_dark"),
            ("btn_secondary", "btn_dark"),
            ("chip_gold", "btn_dark"),
            ("Button01_195_Green", "btn_primary"),
            ("Button01_195_Yellow", "btn_primary"),
            ("Button01_195_Red", "btn_primary"),
            ("Button01_195_Orange", "btn_primary"),
            ("Button01_195", "btn_secondary"),          // 나머지 색 전부
            ("Button_FlushLeft_Gray", "btn_secondary"), // 뒤로가기 보라회색 사다리꼴
            ("Menu_TopBtn_Focus", "tab_on"),
            ("Menu_TopBtn", "tab_off"),
            ("Slider_Basic03_Fill", "bar_fill"),
            ("Slider_Basic03_Bg", "bar_bg"),
            ("Slider_Level01_Fill", "bar_fill"),        // 파란 경험치바 (아트 자체가 파랑)
            ("Slider_Level01_Badge", "chip_gold"),      // 파란 레벨 뱃지 오각형
            ("Popup_FullWidth", "wood_board"),          // 창 몸체 = 통짜 나무 판자
            ("Popoup01-03_White_Bg", "wood_board"),
            ("Popup01_Single_Navy", "wood_board"),
            ("PanelFrame01_Round_Bg", "wood_board"),
            ("Label_Round", "chip_gold"),
            ("BasicFrame_Round12", "row_dark"),
            ("Button01_145_Green", "btn_primary"),
            ("Button01_145_Yellow", "btn_primary"),
            ("Button01_145", "btn_secondary"),
            ("PanelFrame01_Round_Line", "frame_gold"),
            ("ItemFrame01_Empty", "slot_empty"),
            ("Background_06", "bg_ink"),
            ("Background_01", "bg_ink"),
            ("Background_02", "bg_ink"),
            ("Background_03", "bg_ink"),
            // 2차 커버리지 — 오프라인 보상·캐릭터 화면 잔여물 (유저 스크린샷 보고)
            ("Title_Ribbon", "header_cloud"),
            ("Title_Flag", "header_cloud"),
            ("CardFrame03_Single", "paper_sheet"),      // 콘텐츠 카드 = 핀 꽂힌 양피지
            ("CardFrame05_Bg", "paper_sheet"),          // 상점 상품 카드 (네온 보라/초록)
            ("CardFrame06_Bg", "paper_sheet"),
            ("Label_Ribbon", "chip_gold"),              // 가격 리본
            ("BannerFrame", "paper_sheet"),
            ("BorderFrame_Round01", "frame_gold"),
            ("Label_Trapezoid", "chip_gold"),
            ("Button_Hexagon199", "slot_empty"),
            ("ItemFrame07", "row_dark"),
            ("ListFrame02_Single", "row_dark"),         // 문파 목록 남색 행
            ("ListFrame03_Single_Inner", "slot_empty"), // 랭킹 아바타 원판 (파랑/초록)
            ("ListFrame03_Single_Bg", "row_dark"),      // 랭킹 행 (하늘/파랑)
            ("Slider_Basic01_Fill", "bar_fill"),        // 파란 게이지 (문파 상세 등)
            ("StageFrame_Single_Bg", "slot_empty"),     // 파란 육각 노드 (스킬·스테이지)
            ("Slider_Icon05_Fill", "bar_fill"),
            ("Slider_Icon05_Bg", "bar_bg"),
        };

        // ItemFrame01_Single_<색> — 무채색 slot_frame에 등급색 틴트 (희귀도 구분 유지)
        static readonly (string key, Color tint)[] SlotTints =
        {
            ("Yellow", new Color(0.98f, 0.78f, 0.25f)),
            ("Orange", new Color(0.95f, 0.58f, 0.20f)),
            ("Green", new Color(0.45f, 0.80f, 0.35f)),
            ("Blue", new Color(0.35f, 0.60f, 0.95f)),
            ("Sky", new Color(0.45f, 0.75f, 0.95f)),
            ("Purple", new Color(0.72f, 0.45f, 0.95f)),
            ("Red", new Color(0.90f, 0.32f, 0.28f)),
            ("Gray", new Color(0.72f, 0.72f, 0.76f)),
            ("White", new Color(0.90f, 0.90f, 0.92f)),
        };

        static int ProcessRoot(GameObject root, Dictionary<string, Sprite> sprites)
        {
            int changed = 0;
            bool dirty = false;

            // 콘텐츠를 창 내부로 — 비정형 테두리 밖으로 삐져나가지 않게 '가로만' 살짝
            // 인셋한 WuxSafe로 감싼다. 세로까지 압축하면 고정 크기 요소가 겹친다(실사고 롤백).
            // WuxSafe는 Top '뒤'에 둔다: FillCurrencies 등이 텍스트 열거 순서(재화 칩 먼저)에
            // 의존하므로 계층 순서를 프리팹 원본과 같게 유지해야 한다.
            var wuxT = root.transform.Find("WuxWindow");
            if (wuxT != null)
            {
                var safeT = root.transform.Find("WuxSafe") as RectTransform;
                if (safeT == null)
                {
                    var sgo = new GameObject("WuxSafe", typeof(RectTransform));
                    safeT = sgo.GetComponent<RectTransform>();
                    safeT.SetParent(root.transform, false);
                    safeT.offsetMin = safeT.offsetMax = Vector2.zero;

                    var skip = new HashSet<string> { "Background", "Backgroud", "WuxWindow",
                        "WuxSafe", "Top", "Dimed", "Dim" };
                    var toMove = new List<Transform>();
                    for (int ci = 0; ci < root.transform.childCount; ci++)
                    {
                        var ch = root.transform.GetChild(ci);
                        if (skip.Contains(ch.name)) continue;
                        toMove.Add(ch);
                    }
                    var top = root.transform.Find("Top");
                    safeT.SetSiblingIndex(top != null ? top.GetSiblingIndex() + 1
                        : wuxT.GetSiblingIndex() + 1);
                    foreach (var ch in toMove) ch.SetParent(safeT, false);
                    dirty = true;
                    changed++;
                }
                // 인셋 튜닝은 재실행으로 반영되게 항상 갱신
                // 종이 전면 배경엔 프레임이 없다 → 인셋 최소화 (콘텐츠 공간 최대)
                var amin = new Vector2(0.03f, 0.02f);
                var amax = new Vector2(0.97f, 1f);
                if (safeT.anchorMin != amin || safeT.anchorMax != amax)
                {
                    safeT.anchorMin = amin;
                    safeT.anchorMax = amax;
                    dirty = true;
                    changed++;
                }
            }
            var slotFrame = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + "slot_frame.png");
            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite == null)
                {
                    var c0 = img.color;
                    // 이전 패스의 반투명 스크림도 직사각형 잔상 → 완전 투명 (클릭 차단만 유지)
                    if ((img.name == "Background" || img.name == "Backgroud")
                        && img.rectTransform.anchorMin == Vector2.zero
                        && img.rectTransform.anchorMax == Vector2.one
                        && c0.a > 0.01f && c0.r < 0.1f && c0.g < 0.1f && c0.b < 0.1f)
                    {
                        img.color = new Color(0f, 0f, 0f, 0f);
                        img.raycastTarget = true;
                        dirty = true;
                        changed++;
                        continue;
                    }
                    // 스크롤 가장자리 페이드 판(Glow·ScrollShadow):
                    // 나무 창 위에서는 검은 상자로 보인다 → 투명
                    if ((img.name.Contains("Glow") || img.name.Contains("ScrollShadow")) && c0.a > 0.05f)
                    {
                        img.color = new Color(c0.r, c0.g, c0.b, 0f);
                        dirty = true;
                        changed++;
                        continue;
                    }
                    // 스프라이트 없는 파랑 계열 무지 판 → 먹빛 (무협 톤)
                    if (c0.b > 0.3f && c0.b > c0.r * 1.8f && c0.a > 0.5f)
                    {
                        img.color = new Color(0.10f, 0.09f, 0.12f, c0.a);
                        dirty = true;
                        changed++;
                    }
                    continue;
                }
                string sn = img.sprite.name;
                // 화면 전환식 전면 배경 — v3: 거의 단색 다크+희미한 문양 (풍경도 기각, 유저 확정)
                if (img.name == "WuxWindow")
                {
                    var paper = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + "screen_dark.png");
                    var wrt2 = img.rectTransform;
                    if (paper != null && (img.sprite != paper || wrt2.offsetMin != Vector2.zero))
                    {
                        img.sprite = paper;
                        img.type = Image.Type.Simple;
                        img.preserveAspect = false;
                        img.color = Color.white;
                        wrt2.offsetMin = wrt2.offsetMax = Vector2.zero;
                        dirty = true;
                        changed++;
                    }
                    continue;
                }
                // 풀스크린 화면: 몸체=어두운 벽(bg_ink), 그 위에 비정형 실루엣의
                // '나무판 창'(screen_wood, 크로마 투명)을 WuxWindow 노드로 띄운다.
                // 유저 확정 — 반듯한 직사각형이 아니라 구불구불한 수제 판자 실루엣.
                if ((sn == "bg_ink" || sn == "screen_wood") && img.name != "WuxWindow")
                {
                    var rt = img.rectTransform;
                    if (rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one)
                    {
                        var scr = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + "screen_paper.png");
                        // 유저 확정: 나무판 밖은 완전 투명 — 스크림 어둠도 직사각형 잔상을 만든다.
                        // 투명 이미지는 raycastTarget만 살려 창 밖 오클릭을 막는다.
                        {
                            img.sprite = null;
                            img.color = new Color(0f, 0f, 0f, 0f);
                            img.raycastTarget = true;
                            dirty = true;
                            changed++;
                        }
                        var parent = img.transform.parent;
                        if (scr != null && parent != null && parent.Find("WuxWindow") == null)
                        {
                            var go = new GameObject("WuxWindow", typeof(RectTransform), typeof(Image));
                            var wrt = go.GetComponent<RectTransform>();
                            wrt.SetParent(parent, false);
                            go.transform.SetSiblingIndex(img.transform.GetSiblingIndex() + 1);
                            wrt.anchorMin = Vector2.zero;
                            wrt.anchorMax = Vector2.one;
                            wrt.offsetMin = Vector2.zero;
                            wrt.offsetMax = Vector2.zero;
                            var wi = go.GetComponent<Image>();
                            wi.sprite = scr;
                            wi.type = Image.Type.Simple;
                            wi.preserveAspect = false;
                            wi.raycastTarget = false;
                            dirty = true;
                            changed++;
                        }
                    }
                    continue;
                }
                // 파란 광원 이펙트 → 은은한 금빛 (무협 톤)
                if (sn == "Glow_Cirlce" || sn == "Background_ScreenGlow"
                    || sn == "Glow_Oval" || sn == "CardFrame01_Glow"
                    || sn == "CardFrame05_Glow" || sn == "CardFrame06_Glow")
                {
                    // 다크 테마: 글로우가 어두운 배경에서 흰 광원처럼 타오른다 → 아주 은은하게
                    var gold = new Color(0.85f, 0.65f, 0.25f, Mathf.Min(img.color.a, 0.08f));
                    if (img.color != gold) { img.color = gold; dirty = true; changed++; }
                    continue;
                }
                // 프리팹 몸체는 끝까지 채우는 나무판(9-slice) — 통짜 일러스트는 구역이
                // 어긋난다(이중 판·흰 틈 실사고). 판/종이 장식은 프리팹 자체 요소가 담당.
                if (sn == "panel_hanji" || sn == "panel_dark" || sn == "window_popup" || sn == "window_large")
                {
                    var rt = img.rectTransform;
                    bool stretch = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
                    float wpx = Mathf.Max(Mathf.Abs(rt.sizeDelta.x), rt.rect.width);
                    float hpx = Mathf.Max(Mathf.Abs(rt.sizeDelta.y), rt.rect.height);
                    string target = (stretch || (wpx > 520f && hpx > 380f)) ? "wood_board" : "paper_sheet";
                    var repl = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + target + ".png");
                    if (repl != null && img.sprite != repl)
                    {
                        img.sprite = repl;
                        // 통짜 창 일러스트는 슬라이스 금지 — 그림 전체를 그대로 편다
                        img.type = target.StartsWith("window") ? Image.Type.Simple : Image.Type.Sliced;
                        img.preserveAspect = false;
                        img.color = Color.white;
                        dirty = true;
                        changed++;
                    }
                    continue;
                }
                // 등급색 슬롯: 무채색 프레임 + 등급 틴트
                if ((sn.StartsWith("ItemFrame01_Single_") || sn.StartsWith("ItemFrame05_Single_"))
                    && slotFrame != null)
                {
                    string variant = sn.Substring("ItemFrame0X_Single_".Length);
                    var tint = new Color(0.72f, 0.72f, 0.76f);
                    // 05 계열(비급·룬 슬롯)의 색은 등급이 아니라 그냥 아트색 → 따뜻한 중립
                    if (sn.StartsWith("ItemFrame05_"))
                        tint = new Color(0.88f, 0.76f, 0.55f);
                    else
                        foreach (var (key, c) in SlotTints)
                            if (variant.StartsWith(key)) { tint = c; break; }
                    if (img.sprite != slotFrame || img.color != tint)
                    {
                        img.sprite = slotFrame;
                        img.type = Image.Type.Sliced;
                        img.color = tint;
                        dirty = true;
                        changed++;
                    }
                    continue;
                }
                foreach (var (pattern, w) in Map)
                {
                    if (!sn.Contains(pattern)) continue;
                    var rep = sprites[w];
                    if (rep != null && img.sprite != rep)
                    {
                        img.sprite = rep;
                        img.type = Image.Type.Sliced;
                        // 킷은 흰 베이스 + 진한 틴트를 쓰기도 한다 — 프리컬러 아트는 틴트 제거
                        if (img.color != Color.white) img.color = Color.white;
                        dirty = true;
                        changed++;
                    }
                    break;
                }
                // 이전 패스에서 비급(Rune) slot_frame이 등급표에 걸려 파랗게 저장됐다 → 복구
                if (sn == "slot_frame")
                {
                    bool underRune = false;
                    for (var pp = img.transform.parent; pp != null; pp = pp.parent)
                        if (pp.name == "Rune") { underRune = true; break; }
                    var warm = new Color(0.88f, 0.76f, 0.55f);
                    if (underRune && img.color != warm)
                    { img.color = warm; dirty = true; changed++; }
                }
                // 매핑에 안 걸린 잔여 한색 틴트(남색 레벨칩·청록 기력바 등) → 무협 갈색.
                // slot_frame은 등급색 틴트(파랑·하늘 포함)라 제외.
                if (sn != "slot_frame")
                {
                    var cc = img.color;
                    bool coolish = (cc.b > 0.2f && cc.b > cc.r * 1.7f)
                        || (cc.g > 0.08f && cc.g > cc.r * 2f && cc.b > cc.r);
                    if (coolish)
                    {
                        img.color = new Color(0.24f, 0.15f, 0.09f, cc.a);
                        dirty = true;
                        changed++;
                    }
                }
            }
            // 종이띠 스탯 행: 프리팹 기본은 라벨이 띠 위 경계에 걸려 잘린다
            // → '라벨 왼쪽 · 값 오른쪽' 한 줄로 프리팹 자체를 고정 (장비 상세·캐릭터 능력치 공용)
            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite == null
                    || (img.sprite.name != "paper_sheet" && img.sprite.name != "row_dark")) continue;
                TMPro.TMP_Text lab = null, val = null;
                foreach (Transform ch in img.transform)
                {
                    var tt = ch.GetComponent<TMPro.TMP_Text>();
                    if (tt == null) continue;
                    if (ch.name == "Text_Name" || ch.name == "Text_Stats") lab = tt;
                    else if (ch.name == "Text_Value") val = tt;
                }
                if (lab == null || val == null) continue;
                var lrt = lab.rectTransform;
                // 라벨이 행 세로중심을 벗어나 있으면(=띠 경계에 걸침) 재배치
                if (lrt.anchorMin != new Vector2(0f, 0.5f)
                    || Mathf.Abs(lrt.anchoredPosition.y) > 4f)
                {
                    lrt.anchorMin = lrt.anchorMax = new Vector2(0f, 0.5f);
                    lrt.pivot = new Vector2(0f, 0.5f);
                    lrt.anchoredPosition = new Vector2(104f, 0f);
                    lrt.sizeDelta = new Vector2(240f, 70f);
                    lab.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                    lab.fontSize = 32f;
                    var vrt = val.rectTransform;
                    vrt.anchorMin = vrt.anchorMax = new Vector2(1f, 0.5f);
                    vrt.pivot = new Vector2(1f, 0.5f);
                    vrt.anchoredPosition = new Vector2(-28f, 0f);
                    vrt.sizeDelta = new Vector2(280f, 76f);
                    val.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                    val.fontSize = 46f;
                    val.fontStyle = TMPro.FontStyles.Bold;
                    dirty = true;
                    changed++;
                }
            }

            // 재질별 글씨색: 종이(양피지) 위=먹갈색, 나무 창틀 위=크림색.
            var ink = new Color(0.25f, 0.15f, 0.08f, 1f);
            var cream = new Color(0.93f, 0.87f, 0.72f, 1f);
            foreach (var txt in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                var c = txt.color;
                float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                var bg = NearestBgSprite(txt.transform);
                // 창(WuxWindow)은 형제 노드라 부모 체인에 안 잡힌다 —
                // 배경 없는 텍스트는 창 배경 위에 떠 있는 것으로 간주 (실제 스프라이트명 사용)
                if (bg == null)
                {
                    var wuxBg = root.transform.Find("WuxWindow");
                    var wuxImg = wuxBg != null ? wuxBg.GetComponent<Image>() : null;
                    if (wuxImg != null && wuxImg.sprite != null) bg = wuxImg.sprite.name;
                }
                // 밝은 팔레트 v2 (유저 확정): 킷 대부분이 연한 오크·양피지 → 먹색 글씨.
                // 어두운 면은 중간 갈색 제목판(chip_gold·header_cloud)과 짙게 틴트된 탑바뿐.
                bool paperBg = bg == "window_large" || bg == "window_popup"
                    || bg == "paper_sheet" || bg == "panel_hanji" || bg == "row_dark"
                    || bg == "btn_primary" || bg == "btn_secondary" || bg == "tab_on"
                    || bg == "bar_bg" || bg == "slot_empty" || bg == "tab_off"
                    || bg == "wood_board" || bg == "panel_dark" || bg == "screen_wood"
                    || bg == "screen_paper"
                    || bg == "bg_ink" || bg == "chip_gold" || bg == "header_cloud";
                // 어두운 배경·다크 패널 위 텍스트는 밝은 크림
                bool woodBg = bg == "PanelFrame03_Topbar" || bg == "screen_scene"
                    || bg == "screen_dark" || bg == "btn_dark" || bg == "panel_dg"
                    || bg == "row_dim" || bg == "slot_dark";
                // 파랑 계열 글씨(킷 기본)는 명도와 무관하게 무협 톤으로
                bool coolTxt = c.b > 0.25f && c.b > c.r * 1.4f;
                if (paperBg && (lum >= 0.72f || coolTxt))
                { txt.color = ink; dirty = true; changed++; }
                else if (woodBg && (lum < 0.45f || coolTxt))
                { txt.color = cream; dirty = true; changed++; }
            }
            return changed;
        }

        [MenuItem("IdleMvp/아트/프리팹 화면 무협 스킨", priority = 124)]
        public static void Apply()
        {
            var sprites = new Dictionary<string, Sprite>();
            foreach (var (_, w) in Map)
                if (!sprites.ContainsKey(w))
                    sprites[w] = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + w + ".png");

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { Dir });
            int changedPrefabs = 0, changedImages = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                int c0 = ProcessRoot(root, sprites);
                bool dirty = c0 > 0;
                changedImages += c0;
                if (dirty)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changedPrefabs++;
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
            // 도메인 리로드 없는 플레이에서 Resources가 낡은 프리팹을 물고 오지 않게 즉시 플러시
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PrefabSkin] {changedPrefabs}개 프리팹, {changedImages}개 이미지 교체 완료");
        }

        [MenuItem("IdleMvp/아트/씬 무협 스킨 (열린 씬)", priority = 125)]
        public static void ApplyScene()
        {
            var sprites = new Dictionary<string, Sprite>();
            foreach (var (_, w) in Map)
                if (!sprites.ContainsKey(w))
                    sprites[w] = AssetDatabase.LoadAssetAtPath<Sprite>(Wux + w + ".png");
            int changed = 0;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var sc = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!sc.isLoaded) continue;
                foreach (var root in sc.GetRootGameObjects())
                    changed += ProcessRoot(root, sprites);
                if (changed > 0) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc);
            }
            if (changed > 0) UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[PrefabSkin/Scene] {changed}개 이미지 교체 완료");
        }

        /// <summary>부모로 올라가며 처음 만나는 '스프라이트 있는 Image'의 스프라이트 이름.</summary>
        static string NearestBgSprite(Transform t)
        {
            for (var p = t.parent; p != null; p = p.parent)
            {
                var img = p.GetComponent<Image>();
                if (img != null && img.sprite != null && img.color.a > 0.4f)
                    return img.sprite.name;
            }
            return null;
        }
    }
}
