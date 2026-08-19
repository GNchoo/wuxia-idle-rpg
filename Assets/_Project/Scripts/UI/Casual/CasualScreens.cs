using System.Collections.Generic;
using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using IdleMvp.UI.Maple;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Casual
{
    /// <summary>
    /// 구매 에셋(GUI Pro - Casual Game)의 완성 화면 프리팹을 그대로 띄우고
    /// 게임 데이터만 꽂아 넣는다. 손으로 그리지 않는다.
    ///
    /// 프리팹 공통 규칙:
    ///   Text_PanelName / Text_Title  = 제목
    ///   Button_Back / Button_Close   = 닫기
    ///   Text_Value (3개)             = 재화 칩 (에너지/골드/젬 순)
    ///   ScrollRect -> Content        = 목록. 첫 자식을 템플릿으로 복제해 채운다.
    /// </summary>
    public static class CasualScreens
    {
        static Transform _host;
        static readonly Dictionary<string, CasualPanel> _panels = new Dictionary<string, CasualPanel>(24);
        static System.Action<string> _toast;
        static System.Action _refresh;

        public static void Init(Transform host, System.Action<string> toast, System.Action refresh)
        {
            _host = host; _toast = toast; _refresh = refresh;
            // 프리팹을 만들자마자 한글화되도록 치환기를 넘긴다 (첫 열기 순서 버그 방지)
            CasualPanel.Localizer = LocalizeGo;
        }

        static void LocalizeGo(GameObject go)
        {
            if (go == null) return;
            var all = go.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || string.IsNullOrEmpty(t.text)) continue;
                string ko;
                if (EnKo.TryGetValue(t.text.Trim(), out ko)) t.text = ko;
            }
        }

        public static bool Ready => _host != null;

        static void Toast(string m) { if (!string.IsNullOrEmpty(m)) _toast?.Invoke(m); }

        /// <summary>
        /// 프리팹 인스턴스를 캐시해서 재사용한다.
        /// key를 주면 같은 프리팹을 용도별로 따로 띄울 수 있다
        /// (예: Character 프리팹을 동료 상세와 플레이어 캐릭터창에 각각).
        /// </summary>
        static CasualPanel P(string prefab, string key = null)
        {
            string k = key ?? prefab;
            CasualPanel p;
            if (_panels.TryGetValue(k, out p) && p != null && p.Valid) return p;
            p = CasualPanel.Load(prefab, _host);
            if (p != null && p.Go != null) p.Go.name = k;
            _panels[k] = p;
            return p;
        }

        public static void CloseAll()
        {
            foreach (var kv in _panels) if (kv.Value != null && kv.Value.Valid) kv.Value.Hide();
        }

        static System.Action _closeLegacy;
        /// <summary>손으로 그린 기존 모달 호스트를 닫는 콜백 (창 중복 방지).</summary>
        public static void BindCloseLegacy(System.Action a) => _closeLegacy = a;

        /// <summary>
        /// 이 id를 프리팹 화면으로 처리했으면 true.
        /// 열기 전에 다른 프리팹 화면과 기존 모달을 모두 닫는다 —
        /// 안 그러면 무기창(구버전)과 스킬창(프리팹)이 동시에 떠 있는다.
        /// </summary>
        public static bool Open(string id)
        {
            if (_host == null) return false;

            // 프리팹 화면은 '무조건' 먼저 전부 닫는다.
            // 프리팹이 없는 화면(무기 등)으로 넘어갈 때도 닫아야, 남아있던 프리팹이
            // 손그림 모달 위에 겹쳐 보이는 일이 없다.
            CloseAll();

            if (!Handles(id)) return false;   // 기존 모달이 열리도록 넘긴다
            _closeLegacy?.Invoke();
            return Dispatch(id);
        }

        static bool Handles(string id)
        {
            switch (id)
            {
                // 뽑기는 상점으로 옮겼으므로 무기창도 프리팹(가방)으로 전환한다.
                case "menu":
                case "char": case "skill": case "comp": case "compdetail": case "compselect":
                case "weapon":
                case "guild": case "equip": case "inventory": case "shop": case "settings":
                case "rune": case "mail": case "levelup": case "pass": case "hotdeal":
                case "map": case "dungeon": case "offline": case "arena":
                case "sect": case "realm":
                    return true;
                default: return false;
            }
        }

        static bool Dispatch(string id)
        {
            switch (id)
            {
                case "menu":       BuildMenu();           return true;
                case "char":       BuildPlayerChar();     return true;
                case "skill":      BuildSkillTree();      return true;
                case "comp":       BuildCompanionList();  return true;
                case "compdetail": BuildCompanionDetail(); return true;
                case "compselect": BuildCompanionSelect(); return true;
                case "guild":      BuildClan();           return true;
                case "equip":      BuildEquipment();      return true;
                case "inventory":
                case "weapon":     BuildInventory();      return true;   // 무기 목록 = 가방
                case "shop":       BuildShop();           return true;
                case "settings":   BuildSettings();       return true;
                case "rune":       BuildRuneFuse();       return true;
                case "mail":       BuildInbox();          return true;
                case "levelup":    BuildLevelUp();        return true;
                case "pass":       BuildPass();           return true;
                case "hotdeal":    BuildPassOffer();      return true;
                case "map":        BuildStageSelect();    return true;
                case "dungeon":    BuildStageDetail();    return true;
                case "offline":    BuildOffline();        return true;
                case "arena":      BuildRanking();        return true;
                case "sect":       BuildSectSelect();     return true;
                case "realm":      BuildRealm();          return true;
            }
            return false;
        }

        // ---- 공통 헬퍼 ------------------------------------------------------

        /// <summary>재화 칩 3개(Text_Value)를 에너지/골드/젬 순으로 채운다.</summary>
        static void FillCurrencies(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var all = p.Go.GetComponentsInChildren<TMP_Text>(true);
            var w = WalletAdapter.Instance;
            var cw = CurrencyWallet.Instance;
            int idx = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "Text_Value") continue;
                // 상단 재화 칩만 (스탯 표의 Text_Value는 건드리지 않도록 3개까지)
                if (idx == 0) all[i].text = cw != null ? UiKit.Num(cw.Get(CurrencyId.MonsterPoint)) : "0";
                else if (idx == 1) all[i].text = UiKit.Num(w != null ? w.Gold : 0);
                else if (idx == 2) all[i].text = UiKit.Num(w != null ? w.RedDiamond : 0);
                else break;
                all[i].color = new Color(0.96f, 0.93f, 0.84f);   // 어두운 칩 위 크림 고정
                idx++;
            }
        }

        /// <summary>
        /// 프리팹에 박혀있는 영문 문구 → 한글. 노드 이름이 아니라 '현재 텍스트'로 매칭하므로
        /// 화면마다 따로 지정하지 않아도 한 번에 정리된다.
        /// </summary>
        static readonly Dictionary<string, string> EnKo = new Dictionary<string, string>
        {
            // 공통 / 버튼
            { "Collect", "받기" }, { "x2 Collect", "2배 받기" },
            { "Ok", "확인" }, { "OK", "확인" }, { "Continue", "계속" },
            { "Ready", "준비 완료" }, { "Select", "선택" }, { "Equip", "장착" },
            { "UPGRADE", "강화" }, { "Upgrade", "강화" }, { "SELL", "판매" }, { "Sell", "판매" },
            { "Search", "검색" }, { "Join", "가입" }, { "Activate", "활성화" },
            { "Auto Select", "자동 장착" }, { "AutoSelect", "자동 장착" },
            { "Accept All", "전체 받기" }, { "Claim", "받기" },
            { "Fight", "도전" }, { "Finish Now", "즉시 완료" },
            // 섹션 헤더
            { "SKILLS", "무공" }, { "STATS", "능력치" }, { "RUNE", "비급" },
            { "REWARDS", "보상" }, { "BONUS", "보너스" }, { "LEVEL UP!", "레벨 업!" },
            { "ALL", "전체" }, { "Level", "레벨" }, { "Power", "전투력" }, { "Rarity", "등급" },
            { "Rewards", "보상" }, { "Enermy", "적" }, { "Reward", "보상" },
            { "Ranking", "순위" }, { "Members", "인원" }, { "Resets In", "갱신까지" },
            { "Resets In:", "갱신까지" }, { "Golbal", "전체" }, { "Country", "국가" },
            { "Friends", "친구" }, { "Settings", "설정" }, { "Inbox", "우편함" },
            { "Push Alarm", "푸시 알림" }, { "Sound Fx", "효과음" }, { "Music", "배경음" },
            { "Vibration", "진동" }, { "About", "정보" }, { "Support", "문의" },
            { "Like", "좋아요" }, { "Rate", "평가" }, { "Delete Account", "계정 삭제" },
            { "User ID", "사용자 ID" }, { "Rune Fusion", "합성" },
            { "Equipment", "장비" }, { "Inventory", "가방" }, { "Shop", "상점" },
            { "Heroes", "동료" }, { "Clan", "문파" },
            // 등급
            { "COMMON", "일반" }, { "NORMAL", "일반" }, { "RARE", "희귀" },
            { "EPIC", "영웅" }, { "LEGENDARY", "전설" }, { "LEGEND", "전설" },
            // 오프라인
            { "While you were away you earned", "접속하지 않은 동안 획득한 보상" },
            { "Offline Max Reward", "오프라인 최대 보상" },
            { "Attack Damage", "공격력" }, { "Defense", "방어력" }, { "Health", "체력" },
            { "Critical", "치명타" }, { "MOVE SPEED", "이동 속도" }, { "DAMAGE", "공격력" },
            { "DEFENSE", "방어력" }, { "ATTACK", "공격력" }, { "HEALTH", "체력" },
            { "CRITICAL", "치명타" }, { "Ground", "지상" },
            { "MAX+1", "최대 +1" }, { "Ryuens", "나" }, { "Soltjin", "검존" },
            // 패스 / 핫딜
            { "FREE\nPASS", "무료\n패스" }, { "GOLDEN\nPASS", "황금\n패스" },
            { "Season Ends In:", "시즌 종료까지" }, { "Season Ends In", "시즌 종료까지" },
            { "Unlock Exclusive Rewards!", "전용 보상을 해금하세요!" },
            { "Bonus gifts", "보너스 선물" }, { "+10 gem", "젬 +10" },
            { "40elements and 20 outfits", "원소 40종 · 외형 20종" },
            // 상점
            { "15 DAY GEM\nSUBSCRIPTION", "15일 젬\n구독" }, { "Gems", "젬" },
            { "DAILY", "일일" }, { "CHEST", "상자" },
            { "GOLD PACK", "골드 팩" }, { "GEM PACK", "젬 팩" },
            { "SPECIAL", "특별" }, { "Lucky Chest", "행운 상자" }, { "Epic Chest", "영웅 상자" },
            // 인벤토리 기본 문구
            { "Wood Shield", "목재 방패" },
            { "Equipment Component.\nOnly the Black Market Smiths.", "장비 재료입니다.\n좌측 목록에서 아이템을 선택하세요." },
            // 문파
            { "Dark Knight", "흑풍채" },
            { "We are long time players who enjoy the game!", "오래도록 무를 닦아온 이들의 모임." },
        };

        /// <summary>영문 잔재를 한글로 바꾼 뒤 표시한다. 모든 화면은 이걸 통해 열린다.</summary>
        static void ShowLocalized(CasualPanel p)
        {
            if (p == null) return;
            LocalizeByText(p, EnKo);
            p.Show();
        }

        /// <summary>현재 텍스트가 표에 있으면 한글로 바꾼다.</summary>
        static void LocalizeByText(CasualPanel p, Dictionary<string, string> table)
        {
            if (p == null || p.Go == null) return;
            var all = p.Go.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || string.IsNullOrEmpty(t.text)) continue;
                string key = t.text.Trim();
                string ko;
                if (table.TryGetValue(key, out ko)) t.text = ko;
            }
        }

        /// <summary>
        /// 목록 상단이 필터 바에 가려 잘리는 것 방지 — 뷰포트 위쪽을 내린다.
        /// </summary>
        static void PadScrollTop(CasualPanel p, float px)
        {
            var sr = p.Go != null ? p.Go.GetComponentInChildren<ScrollRect>(true) : null;
            if (sr == null) return;
            // 이 프리팹들은 viewport가 비어 있고 ScrollRect 자신이 마스크 역할을 한다
            var v = sr.viewport != null ? sr.viewport : sr.GetComponent<RectTransform>();
            if (v == null) return;
            if (v.offsetMax.y <= -px + 0.5f) return;   // 이미 내려가 있으면 건너뜀
            v.offsetMax = new Vector2(v.offsetMax.x, v.offsetMax.y - px);

            // 목록 컨테이너에도 위쪽 여백을 줘서 첫 줄이 필터에 물리지 않게
            var glg = sr.content != null ? sr.content.GetComponent<LayoutGroup>() : null;
            if (glg != null && glg.padding.top < (int)px)
                glg.padding = new RectOffset(glg.padding.left, glg.padding.right,
                    (int)px, glg.padding.bottom);
        }

        /// <summary>우측 상세 문구가 칸을 넘어 잘리는 것 방지.</summary>
        static void FitDetail(CasualPanel p, params string[] nodes)
        {
            if (p == null || p.Go == null) return;
            var all = p.Go.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < all.Length; i++)
            {
                for (int k = 0; k < nodes.Length; k++)
                {
                    if (all[i].name != nodes[k]) continue;
                    all[i].enableWordWrapping = true;
                    all[i].overflowMode = TMPro.TextOverflowModes.Overflow;
                    all[i].enableAutoSizing = true;
                    all[i].fontSizeMin = 16f;
                    break;
                }
            }
        }

        /// <summary>줄바꿈 금지 + 넘치면 축소 — 한글이 길어 다음 줄로 밀리는 것 방지.</summary>
        static void NoWrap(CasualPanel p, string node)
        {
            var all = p.Go != null ? p.Go.GetComponentsInChildren<TMP_Text>(true) : null;
            if (all == null) return;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != node) continue;
                all[i].enableWordWrapping = false;
                all[i].overflowMode = TMPro.TextOverflowModes.Overflow;
                all[i].enableAutoSizing = true;
                all[i].fontSizeMin = 14f;
            }
        }

        static void WireBack(CasualPanel p)
        {
            p.OnClick("Button_Back", () => { p.Hide(); _refresh?.Invoke(); });
            p.OnClick("Button_Close", () => { p.Hide(); _refresh?.Invoke(); });
            p.OnClick("Button_Home", () => { p.Hide(); _refresh?.Invoke(); });

            // 유저 목업(2026-08-18): 제목=판 중앙 단독, 재화 칩=화면 우상단 구석(창 밖)
            TitleOnPlank(p, "Text_PanelName");

            var wuxW = p.Go != null ? p.Go.transform.Find("WuxWindow") : null;

            // 화면 전용 레이아웃 배경 (구역·그리드가 그려진 배경, 유저 확정).
            // 없으면 공용 screen_dark 유지.
            if (wuxW != null)
            {
                string bgName = null;
                if (_screenBg.TryGetValue(p.Go.name, out bgName))
                {
                    var bgSp = Resources.Load<Sprite>("WuxiaUi/" + bgName);
                    var bgImg = wuxW.GetComponent<Image>();
                    if (bgSp != null && bgImg != null && bgImg.sprite != bgSp)
                    {
                        bgImg.sprite = bgSp;
                        bgImg.type = Image.Type.Simple;
                        bgImg.preserveAspect = false;
                    }
                }
            }

            // 풍경 배경 어둠막 — UI가 배경에 묻히지 않게 가라앉힌다 (AAA 캐릭터창 문법)
            if (wuxW != null && p.Go.transform.Find("WuxDim") == null)
            {
                var dgo = new GameObject("WuxDim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var drt = dgo.GetComponent<RectTransform>();
                drt.SetParent(p.Go.transform, false);
                drt.SetSiblingIndex(wuxW.GetSiblingIndex() + 1);
                drt.anchorMin = Vector2.zero;
                drt.anchorMax = Vector2.one;
                drt.offsetMin = drt.offsetMax = Vector2.zero;
                var di = dgo.GetComponent<Image>();
                // 배경이 이미 짙은 단색이라 어둠막은 약하게 (문양이 보이게)
                di.color = new Color(0f, 0f, 0f, 0.15f);
                di.raycastTarget = false;
            }

            // 창을 게임 화면 '전체'로 — 단, 프리팹 설계 단위(높이 1440)를 그대로 유지한다.
            // 스트레치+scale 1로 하면 캔버스 해상도에 따라 요소가 프리팹의 2배로 보인다
            // (프리팹≠인게임 이질감의 진짜 원인). 균등 스케일: 높이를 1440으로 맞추고
            // 폭은 화면 비율만큼 — 16:9에서 프리팹과 픽셀 단위로 동일해진다.
            if (wuxW != null && p.Root != null)
            {
                var canvasRt = p.Root.parent as RectTransform;
                float ch2 = canvasRt != null ? canvasRt.rect.height : 1440f;
                float cw2 = canvasRt != null ? canvasRt.rect.width : 2560f;
                float s = ch2 / 1440f;
                p.Root.anchorMin = p.Root.anchorMax = new Vector2(0.5f, 0.5f);
                p.Root.pivot = new Vector2(0.5f, 0.5f);
                p.Root.sizeDelta = new Vector2(cw2 / s, 1440f);
                p.Root.anchoredPosition = Vector2.zero;
                p.Root.localScale = new Vector3(s, s, 1f);
            }
            var sbG = wuxW != null ? p.Find("StatusBar_Group") : null;
            if (sbG != null)
            {
                var srt = sbG.GetComponent<RectTransform>();
                srt.anchorMin = srt.anchorMax = new Vector2(1f, 1f);
                srt.pivot = new Vector2(1f, 1f);
                srt.anchoredPosition = new Vector2(-48f, -8f);
                srt.sizeDelta = new Vector2(800f, 70f);
                srt.localScale = new Vector3(0.8f, 0.8f, 1f);
                var hlg = sbG.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null) hlg.childAlignment = TextAnchor.MiddleRight;
            }
        }

        /// <summary>
        /// 화면 제목을 나무 창 상단의 진홍 제목판 위로 올린다 (크림색·중앙).
        /// 프리팹마다 제목 노드명이 달라서(Text_PanelName / Text_Missions) 노드명을 받는다.
        /// </summary>
        /// <summary>패널 키 → 화면 전용 레이아웃 배경 (Resources/WuxiaUi/).</summary>
        static readonly Dictionary<string, string> _screenBg = new Dictionary<string, string>
        {
            { "PlayerChar", "bg_char" },
        };

        /// <summary>RectTransform을 부모 기준 비율 사각형에 앉힌다 (배경 그리드 정렬용).</summary>
        static void PlaceIn(Transform t, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = t as RectTransform;
            if (rt == null) return;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        /// <summary>제목 아래 먹선 한 줄 — 종이 위에 붓으로 그은 느낌(가운데가 굵다).</summary>
        static void TitleRule(Transform parent, float x0, float y, float x1)
        {
            var t = parent.Find("TitleRule") as RectTransform;
            if (t == null)
            {
                var go = new GameObject("TitleRule", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
                var im = go.GetComponent<Image>();
                im.color = new Color(0.36f, 0.22f, 0.12f, 0.55f);
                im.raycastTarget = false;
            }
            PlaceIn(t, x0, y, x1, y + 0.0035f);
        }

        static void TitleOnPlank(CasualPanel p, string node, float xOff = 0f, float width = 640f)
        {
            var pn = p.Get<TMP_Text>(node);
            if (pn == null) return;
            // 창 형태 화면은 제목을 이미 창 안 제목판에 붙였다 — 다시 끌어내지 않는다
            if (p.Go != null && (p.Go.transform.Find("WuxMain") != null
                || p.Go.transform.Find("WuxSide") != null)) return;
            pn.enableVertexGradient = false;
            // 어두운 풍경 배경 위 → 크림
            pn.color = new Color(0.95f, 0.90f, 0.78f);

            // 화면 전환식: 제목은 참고작처럼 좌상단, 뒤로가기 옆
            var wux = p.Go != null ? p.Go.transform.Find("WuxWindow") : null;
            if (wux != null && pn.rectTransform.parent != p.Go.transform)
            {
                var rt = pn.rectTransform;
                rt.SetParent(p.Go.transform, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(150f + xOff, -64f);
                rt.sizeDelta = new Vector2(width, 100f);
                pn.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                pn.fontSize = 54f;
            }
        }

        /// <summary>
        /// ScrollRect의 Content 첫 자식을 템플릿으로 count개 확보한다.
        /// 프리팹 Content에는 데모 행이 여러 개 들어있으므로, 첫 자식만 남기고 전부 치운 뒤
        /// 복제한다. (안 그러면 남은 데모 행에 Dunk/Soni 같은 샘플 데이터가 그대로 보인다)
        /// </summary>
        static List<Transform> Rows(CasualPanel p, int count)
        {
            var sr = p.Go != null ? p.Go.GetComponentInChildren<ScrollRect>(true) : null;
            if (sr == null || sr.content == null || sr.content.childCount == 0) return new List<Transform>();
            var content = sr.content;
            var tpl = content.GetChild(0);

            // 프리팹에 따라 content의 첫 자식이 '칸'이 아니라 칸들을 담은 그리드다.
            // (Inventory: Content/Slot_Yellow/Slot_Item x7)
            // 이걸 칸으로 착각해 복제하면 그리드가 늘어나고, 클릭 버튼도 그리드 하나에만
            // 붙어서 어느 칸을 눌러도 반응이 없었다 → 그리드면 그 안의 칸을 쓴다.
            int cellCount = 0;
            for (int i = 0; i < tpl.childCount; i++)
                if (tpl.GetChild(i).name.StartsWith("Slot_Item")) cellCount++;
            if (cellCount >= 2) return Cells(tpl, "Slot_Item", count);

            for (int i = content.childCount - 1; i >= 1; i--)
            {
                var ch = content.GetChild(i);
                if (ch == tpl) continue;
                ch.SetParent(null, false);
                Object.Destroy(ch.gameObject);
            }
            return p.Repeat(tpl.name, count);
        }

        /// <summary>그리드 안의 칸을 count개 확보한다 (모자라면 복제, 남으면 숨김).</summary>
        static List<Transform> Cells(Transform grid, string cellName, int count)
        {
            var cells = new List<Transform>();
            for (int i = 0; i < grid.childCount; i++)
                if (grid.GetChild(i).name.StartsWith(cellName)) cells.Add(grid.GetChild(i));
            if (cells.Count == 0) return cells;

            while (cells.Count < count)
            {
                var clone = Object.Instantiate(cells[0], grid);
                clone.name = cellName + "_c" + cells.Count;
                cells.Add(clone);
            }
            for (int i = 0; i < cells.Count; i++)
                cells[i].gameObject.SetActive(i < count);
            if (cells.Count > count) cells.RemoveRange(count, cells.Count - count);
            return cells;
        }

        static void SetIn(Transform row, string node, string value)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) { all[i].text = value; return; }
        }

        /// <summary>이름이 겹치는 프리팹 노드를 특정 가지 안에서만 찾는다.</summary>
        static T FindIn<T>(Transform root, string node) where T : Component
        {
            if (root == null) return null;
            var all = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) return all[i];
            return null;
        }

        /// <summary>
        /// 좁은 칸(숫자용 칩)에 한글을 넣으면 줄바꿈되어 아래 글자와 겹친다.
        /// 줄바꿈을 막고 글자를 줄여 한 줄에 들어가게 한다.
        /// </summary>
        static void FitChip(Transform root, string node, float min = 18f, float max = 32f)
        {
            var t = FindIn<TMP_Text>(root, node);
            if (t == null) return;
            t.enableWordWrapping = false;
            t.enableAutoSizing = true;
            t.fontSizeMin = min; t.fontSizeMax = max;
            t.overflowMode = TMPro.TextOverflowModes.Overflow;
        }

        /// <summary>행 안의 노드를 이름으로 끄고 켠다 (프리팹 데모 장식 정리용).</summary>
        static void ActiveIn(Transform row, string node, bool on)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) all[i].gameObject.SetActive(on);
        }

        static void SetInAt(Transform row, string node, int occurrence, string value)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<TMP_Text>(true);
            int seen = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != node) continue;
                if (seen == occurrence) { all[i].text = value; return; }
                seen++;
            }
        }

        static void ColorIn(Transform row, string node, Color c)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) { all[i].color = c; return; }
        }

        /// <summary>
        /// 육각형 노드 자체(눈에 보이는 부분)에만 버튼을 단다.
        /// 부모 행 rect는 육각형보다 훨씬 커서 다른 UI를 가로챈다.
        /// </summary>
        static void ClickHexOnly(Transform row, System.Action a)
        {
            if (row == null) return;
            // 행의 raycast를 끈다 (뒤에 있는 버튼이 눌리도록)
            var rowImg = row.GetComponent<Image>();
            if (rowImg != null) rowImg.raycastTarget = false;
            var rowBtn = row.GetComponent<Button>();
            if (rowBtn != null) rowBtn.enabled = false;

            // 가장 그럴듯한 육각형 이미지를 찾아 거기에만 버튼
            Image target = null;
            var imgs = row.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < imgs.Length; i++)
            {
                var r = imgs[i].rectTransform.rect;
                if (r.width < 60f || r.height < 60f) continue;
                if (r.width > 400f || r.height > 400f) continue;   // 행 배경 제외
                target = imgs[i]; break;
            }
            if (target == null) { ClickRow(row, a); return; }

            target.raycastTarget = true;
            var b = target.GetComponent<Button>() ?? target.gameObject.AddComponent<Button>();
            b.targetGraphic = target;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(AudioService.Click);
            if (a != null) b.onClick.AddListener(() => a());
        }

        static void ClickRow(Transform row, System.Action a)
        {
            if (row == null) return;
            var b = row.GetComponent<Button>() ?? row.GetComponentInChildren<Button>(true);
            if (b == null) b = row.gameObject.AddComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(AudioService.Click);
            if (a != null) b.onClick.AddListener(() => a());
        }

        static readonly Color MaxColor = new Color(1f, 0.83f, 0.25f, 1f);   // 6번: 맥스는 금색
        static readonly Color NormalColor = Color.white;
        // 동료 카드(밝은 양피지) 전용 — 흰색은 안 읽힌다
        static readonly Color CardNameColor = new Color(0.25f, 0.15f, 0.08f, 1f);
        static readonly Color CardMaxColor = new Color(0.62f, 0.40f, 0.05f, 1f);

        // ---- 6번: 동료 목록 (맥스 레벨 다른 색) ------------------------------

        static void BuildCompanionList()
        {
            var p = P("Character_List"); if (p == null) return;
            var ca = CompanionAdapter.Instance;
            // 상단 정렬 탭(레벨/전투력/등급)을 실제로 동작하게 한다 (기존엔 무동작)
            var owned = ca != null ? ca.GetSortedOwned(_compSort == 2 ? 2 : 0, _compSort == 1) : new List<CompanionItem>();
            WireCompSort(p);

            p.SetText("Text_PanelName", $"동료 {owned.Count}/{(ca != null ? ca.OwnedCount : 0)}");
            FillCurrencies(p);

            var rows = Rows(p, Mathf.Max(1, owned.Count));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < owned.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var it = owned[i];
                int max = 50;
                bool isMax = it.level >= max;

                SetIn(rows[i], "Text_Name", it.name);
                SetIn(rows[i], "Text_Level", it.level.ToString());
                SetIn(rows[i], "Text_Slider", isMax ? "MAX!" : $"{it.level}/{max}");
                ColorIn(rows[i], "Text_Name", isMax ? CardMaxColor : CardNameColor);

                // 맥스 레벨 배지 색 전환 — 프리팹에 파란(Level)/노란(Level_Full) 배지가
                // 둘 다 들어있다. 요청 6번은 이걸 그대로 쓰면 된다.
                SetActiveIn(rows[i], "Level", !isMax);
                SetActiveIn(rows[i], "Level_Full", isMax);

                // 초상화 노드 이름은 'Character' (가장 큰 이미지는 카드 프레임이라 오답)
                var icon = FindImage(rows[i], "Character");
                if (icon != null)
                {
                    var sp = GrowArt.IconCompanion(it.name, it.rarity);
                    if (sp != null) { icon.sprite = sp; icon.color = Color.white; icon.preserveAspect = true; }
                }

                // 각성 별: Active(켜진 별)를 awaken 개수만큼만 표시
                SetStars(rows[i], it.awaken);

                string id = it.id;
                ClickRow(rows[i], () => { _selectedCompanionId = id; BuildCompanionDetail(); });
            }
            LayoutCompanionWindow(p);
            WireBack(p);
            ShowLocalized(p);
        }

        static Image FindImage(Transform row, string name)
        {
            var all = row.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < all.Length; i++) if (all[i].name == name) return all[i];
            return null;
        }

        static void SetActiveIn(Transform row, string node, bool on)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) all[i].gameObject.SetActive(on);
        }

        /// <summary>이름이 같은 자식을 전부 숨긴다.</summary>
        static void HideAllNamed(Transform row, string node)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == node) all[i].gameObject.SetActive(false);
        }

        /// <summary>프리팹의 별 등급 표시: 켜진 별(Active)을 count개만 남긴다.</summary>
        static void SetStars(Transform row, int count)
        {
            if (row == null) return;
            var all = row.GetComponentsInChildren<Transform>(true);
            int seen = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "Active") continue;
                all[i].gameObject.SetActive(seen < count);
                seen++;
            }
        }

        /// <summary>
        /// 행/칸의 아이콘 이미지를 찾는다.
        /// '가장 큰 이미지'로 찾으면 카드 프레임(Slot)이 잡혀서 아이콘이 안 바뀌므로,
        /// 프리팹이 쓰는 이름을 우선 순서대로 본다.
        /// </summary>
        static readonly string[] IconNodeNames =
            { "Character", "Image_Item", "Item", "Icon_Item", "Image", "Icon" };

        /// <summary>
        /// 아이콘 뒤에 희귀도 프레임을 깐다. 키트에 무기 24종 전용 아트가 없어서
        /// 실루엣만으로는 등급이 안 읽히는데, 프레임 색이 그 역할을 한다.
        /// 아이콘 바로 앞 형제로 넣어 항상 뒤에 그려지게 한다.
        /// </summary>
        static void RarityBack(Image icon, int rarity)
        {
            if (icon == null) return;
            var parent = icon.transform.parent;
            if (parent == null) return;
            var frame = GrowArt.RarityFrame(rarity);
            if (frame == null) return;

            var t = parent.Find("RarityBg");
            Image bg;
            if (t != null) bg = t.GetComponent<Image>();
            else
            {
                var go = new GameObject("RarityBg", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                bg = go.GetComponent<Image>();
                bg.raycastTarget = false;
            }
            if (bg == null) return;
            bg.sprite = frame;
            bg.color = new Color(1f, 1f, 1f, 0.85f);
            bg.transform.SetSiblingIndex(icon.transform.GetSiblingIndex());

            // 아이콘 칸을 살짝 넘치게 덮어 '슬롯 배경'처럼 보이게 한다
            var ir = icon.rectTransform;
            var br = bg.rectTransform;
            br.anchorMin = ir.anchorMin; br.anchorMax = ir.anchorMax;
            br.pivot = ir.pivot;
            br.anchoredPosition = ir.anchoredPosition;
            br.sizeDelta = ir.sizeDelta + new Vector2(14f, 14f);
        }

        static Image FindIconImage(Transform row)
        {
            for (int n = 0; n < IconNodeNames.Length; n++)
            {
                var hit = FindImage(row, IconNodeNames[n]);
                if (hit != null) return hit;
            }
            // 이름으로 못 찾으면 프레임류를 제외하고 가장 큰 것
            var all = row.GetComponentsInChildren<Image>(true);
            Image best = null; float bestArea = 0f;
            for (int i = 0; i < all.Length; i++)
            {
                string nm = all[i].name;
                if (nm == "Slot" || nm == "Glow" || nm == "Gradient" || nm == "Slider" ||
                    nm == "Fill" || nm.StartsWith("Star") || nm == "Active") continue;
                var r = all[i].rectTransform.rect;
                float a = r.width * r.height;
                if (a > bestArea && a > 2000f) { bestArea = a; best = all[i]; }
            }
            return best;
        }

        static string _selectedCompanionId;

        // ---- 7번: 동료 상세 + 강화 ------------------------------------------

        static void BuildCompanionDetail()
        {
            var p = P("Character"); if (p == null) return;
            var ca = CompanionAdapter.Instance;
            CompanionItem it = null;
            if (ca != null)
            {
                var list = ca.GetSortedOwned(0, false);
                if (!string.IsNullOrEmpty(_selectedCompanionId))
                    it = list.Find(x => x.id == _selectedCompanionId);
                if (it == null && list.Count > 0) it = list[0];
            }
            if (it == null) { Toast("보유 동료 없음"); return; }
            _selectedCompanionId = it.id;

            string[] rank = { "COMMON", "RARE", "EPIC", "LEGEND" };
            p.SetText("Text_PanelName", "동료");
            p.SetText("Text_Name", it.name);
            p.SetText("Text_Rank", rank[Mathf.Clamp(it.rarity, 0, 3)]);
            p.SetText("Text_Stats", $"각성 ★{it.awaken}");
            p.SetText("Text_Exp", $"{it.count}/5");
            p.SetText("Text_Lv", it.level.ToString());
            p.SetText("Text_Trophy", it.awaken.ToString());
            p.SetText("Text_Info", $"{it.name}\n등급 {it.rarity} · 각성 ★{it.awaken}\n" +
                                   (it.main ? "메인 동료로 출전 중" : it.sub ? "서브 동료로 출전 중" : "대기 중"));
            FillCurrencies(p);

            // 초상화 = 필드에 소환되는 리그와 동일.
            // 프리팹의 데모 캐릭터 레이어가 남으면 겹쳐서 이상한 모양이 된다 → 전부 교체/숨김
            var portrait = GrowArt.IconCompanion(it.name, it.rarity);
            var big = p.Go.GetComponentsInChildren<Image>(true);
            bool placed = false;
            for (int i = 0; i < big.Length; i++)
            {
                string nodeName = big[i].name;
                string spName = big[i].sprite != null ? big[i].sprite.name : "";
                bool isDemoChar = spName.StartsWith("Character_Sample") || spName.StartsWith("Demo_Character");
                bool isSlot = nodeName == "Character" || nodeName == "Image_Character" || nodeName == "Image_Hero";
                if (!isSlot && !isDemoChar) continue;

                if (!placed && isSlot && portrait != null)
                {
                    big[i].sprite = portrait;
                    big[i].color = Color.white;
                    big[i].preserveAspect = true;
                    big[i].enabled = true;
                    placed = true;
                }
                else big[i].enabled = false;   // 나머지 데모 레이어는 끈다
            }
            // 글씨 깨짐(줄바꿈으로 밀림) 방지
            NoWrap(p, "Text_Name"); NoWrap(p, "Text_Rank"); NoWrap(p, "Text_Stats");

            // 강화 = 각성 (중복 동료를 소모해 ★ 상승)
            p.SetText("Text_Gold", it.count + " / 2");
            p.SetText("Text_Upgrade", $"★{it.awaken} → ★{it.awaken + 1} 각성");
            p.OnClick("Button_Upgrade", () =>
            {
                var costs = new List<CostLine> { CostLine.Of("중복 동료", 2, it.count) };
                CasualDialogs.Confirm("동료 각성", $"{it.name}  ★{it.awaken} → ★{it.awaken + 1}", costs, () =>
                {
                    Toast(CompanionAdapter.Instance?.TryAwaken(it.id));
                    CasualFx.EnhanceFlash(_host);
                    BuildCompanionDetail();
                    _refresh?.Invoke();
                });
            });

            // 선택 = 메인 동료로
            p.SetText("Text_Select", it.main ? "메인 중" : "메인 지정");
            p.OnClick("Button_Selet", () =>
            {
                CompanionAdapter.Instance?.SetMain(it.id);
                Toast(it.name + " 메인 지정");
                Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                BuildCompanionDetail();
                _refresh?.Invoke();
            });

            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 메뉴 (Popup_Setting 프리팹 재사용) ------------------------------

        static System.Action<string> _openById;
        public static void BindOpenById(System.Action<string> a) => _openById = a;

        static void BuildMenu()
        {
            // 설정 팝업은 '라벨 + 버튼' 행 구조라 메뉴로 그대로 쓸 수 있다
            var p = P("Popup_Setting", "MenuPanel"); if (p == null) return;
            p.SetText("Text_Title", "메뉴");

            // 토글·슬라이더와 그 라벨은 메뉴에선 필요 없다 (설정 화면에만 있으면 된다).
            // 토글 컴포넌트만 끄면 아이콘·구분선 행이 그대로 남아 반쪽짜리 줄이 보였다
            // → 행 컨테이너(List)째로 숨긴다.
            // 토글 행은 전부 'List'라는 같은 이름을 쓴다 → Find는 첫 개만 잡으므로 전부 순회
            var allNodes = p.Go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allNodes.Length; i++)
                if (allNodes[i].name == "List") allNodes[i].gameObject.SetActive(false);
            // 토글만 끄면 같은 줄의 아이콘·구분선이 남아 반쪽짜리 행이 보인다 → 행째 숨긴다
            var toggles = p.Go.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
            {
                var row = toggles[i].transform.parent != null
                    ? toggles[i].transform.parent.gameObject : toggles[i].gameObject;
                row.SetActive(false);
            }
            var sliders = p.Go.GetComponentsInChildren<Slider>(true);
            for (int i = 0; i < sliders.Length; i++)
            {
                var row = sliders[i].transform.parent != null
                    ? sliders[i].transform.parent.gameObject : sliders[i].gameObject;
                row.SetActive(false);
            }
            var leftover = new HashSet<string> { "푸시 알림", "효과음", "배경음", "진동",
                "Push Alarm", "Sound Fx", "Music", "Vibration" };
            var lt = p.Go.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < lt.Length; i++)
                if (leftover.Contains((lt[i].text ?? "").Trim())) lt[i].gameObject.SetActive(false);

            string[] labels = { "설정", "코스튬", "전직", "문파", "경지", "유물", "맵 선택", "성장 던전", "환생" };
            string[] ids = { "settings", "appearance", "job", "sect", "realm", "artifact", "map", "dungeon", "rebirth" };

            // 프리팹은 2열 × 4행 = 8칸인데 항목이 9개다 → 마지막 행을 복제해 10칸으로 늘린다
            var listRows = new List<Transform>();
            for (int i = 0; i < allNodes.Length; i++)
                if (allNodes[i] != null && allNodes[i].name == "Button_List") listRows.Add(allNodes[i]);
            while (listRows.Count * 2 < labels.Length && listRows.Count > 0)
            {
                var last = listRows[listRows.Count - 1];
                var extra = Object.Instantiate(last.gameObject, last.parent);
                extra.name = "Button_List_Extra";
                extra.transform.SetSiblingIndex(last.GetSiblingIndex() + 1);
                listRows.Add(extra.transform);
            }

            var slots = new List<Button>();
            for (int r = 0; r < listRows.Count; r++)
                slots.AddRange(listRows[r].GetComponentsInChildren<Button>(true));

            for (int i = 0; i < slots.Count; i++)
            {
                var b = slots[i];
                if (i >= labels.Length) { b.gameObject.SetActive(false); continue; }
                b.gameObject.SetActive(true);
                string id = ids[i];

                // 버튼 안에 글자가 둘(예: '계정 삭제' + 안내문)인 프리팹이 있어
                // 첫 글자만 라벨로 쓰고 나머지는 지운다 — 안 그러면 옛 문구가 남는다
                var ts = b.GetComponentsInChildren<TMP_Text>(true);
                for (int k = 0; k < ts.Length; k++)
                {
                    if (k == 0) { ts[k].text = labels[i]; ts[k].enableWordWrapping = false; ts[k].gameObject.SetActive(true); }
                    else ts[k].gameObject.SetActive(false);
                }

                // 프리팹 버튼에 박혀 있던 데모 장식(영어 국기 등)이 새 라벨 위에 겹쳤다.
                // 버튼 배경 이미지만 남기고 안쪽 장식 아이콘은 숨긴다.
                var deco = b.GetComponentsInChildren<Image>(true);
                for (int d = 0; d < deco.Length; d++)
                {
                    if (deco[d].gameObject == b.gameObject) continue;   // 버튼 배경은 유지
                    string dn = deco[d].name;
                    if (dn == "Flag" || dn.StartsWith("Icon_Language") || dn == "Icon_Flag")
                        deco[d].gameObject.SetActive(false);
                }

                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(AudioService.Click);
                b.onClick.AddListener(() => { CloseAll(); _openById?.Invoke(id); });
            }
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 2번: 캐릭터 창 (Character 프리팹 재사용) -------------------------

        static void BuildPlayerChar()
        {
            // 동료 상세와 같은 프리팹이지만 용도가 달라 별도 인스턴스로 띄운다
            var p = P("Character", "PlayerChar"); if (p == null) return;
            var g = PlayerGrowth.Instance;
            var jobDef = JobProgress.Current;
            string jobName = jobDef != null ? jobDef.name : "무사";

            p.SetText("Text_PanelName", "캐릭터");
            p.SetText("Text_Name", jobName);
            // 사다리꼴 Rank 뱃지는 이름 글자 위 20px를 덮어 '무사'가 잘려 보였다.
            // 세력은 바로 아래 Text_Stats가 '정파 · 내공 N단계'로 이미 보여주므로 중복이다.
            p.SetActive("Rank", false);

            // 데모 스킬 원형 버튼 2개(기능 없음)와 '무공' 라벨: 능력치 표와 겹쳐 어수선 → 숨김
            p.SetActive("Skill_Frist", false);
            p.SetActive("Skill_Second", false);
            p.SetActive("Text_Skills", false);

            // 배치는 프리팹(Character.prefab)에서 직접 편집한다 — 코드로 강제하지 않는다.
            // (Group_Right·Rune·BottomMenu·버튼 크기를 코드로 덮어쓰던 블록 제거, 2026-08-18)
            // 예외 1건: '공격 강화' 칩은 킷 원본 오프셋(+794)이 인게임에서 화면 밖으로
            // 날아간다 → 버튼 바로 위로 고정
            var lblUp = p.Find("Label_Upgrade");
            if (lblUp != null) ((RectTransform)lblUp).anchoredPosition = new Vector2(0f, 140f);

            // 이름 옆 원형 아이콘: 프리팹 기본이 흰 픽토그램(RoleIcon)이라 직업이 안 읽히고,
            // 레이아웃에 눌려 폭이 0이 돼 아예 안 보였다.
            var jobIcoRoot = p.Find("Icon_Ability");
            if (jobIcoRoot != null)
            {
                var le = jobIcoRoot.GetComponent<LayoutElement>();
                if (le == null) le = jobIcoRoot.gameObject.AddComponent<LayoutElement>();
                le.minWidth = le.preferredWidth = 77f;
                le.minHeight = le.preferredHeight = 77f;
                var jico = jobIcoRoot.Find("Icon");
                var jimg = jico != null ? jico.GetComponent<Image>() : null;
                if (jimg != null)
                {
                    var js = GrowArt.IconJob(jobDef != null ? jobDef.id : null);
                    if (js != null) { jimg.sprite = js; jimg.color = Color.white; jimg.preserveAspect = true; }
                }
            }
            // 'Text_Stats'는 STATS 표 라벨과 이름이 겹쳐서 여기서 건드리면 표가 밀린다.
            // 부제는 Text_Info 쪽에서만 표현한다.
            int sp0 = g != null ? g.StatPoints : 0;
            p.SetText("Text_Lv", g != null ? g.Level.ToString() : "1");
            p.SetText("Text_Exp", sp0 + " SP");
            p.SetText("Text_Trophy", StageProgress.Instance != null
                ? StageProgress.Instance.MaxWaveReached.ToString() : "0");
            // 프리팹 Info 칸은 3줄이 한계다. 길게 넣으면 아래 비급 아이콘 위로 넘친다.
            p.SetText("Text_Info",
                $"전투력 {UiKit.Num(CombatPowerService.GetTotalCp())}\n" +
                (FactionService.SynergyName != null
                    ? $"이형무공 {FactionService.SynergyName}" : $"잔여 스탯 {sp0} SP"));
            var info = p.Get<TMP_Text>("Text_Info");
            if (info != null)
            {
                info.enableWordWrapping = false;
                info.overflowMode = TMPro.TextOverflowModes.Truncate;
            }
            FillCurrencies(p);

            // 스탯 표는 '노드 이름'으로 채운다. 이전엔 TMP_Text를 순서대로 세서 채웠는데,
            // 레이아웃이 위젯 부모를 옮기면 순서가 바뀌어 라벨·값이 통째로 밀렸다.
            var statRows = new[]
            {
                new[] { "Damage",    "치명타",    CombatPowerService.GetCritRatePct().ToString("0.#") + "%" },
                new[] { "Defense",   "방어력",    UiKit.Num(CombatPowerService.GetDef()) },
                new[] { "Attack",    "공격력",    UiKit.Num(CombatPowerService.GetAtk()) },
                new[] { "Health",    "최대 HP",   UiKit.Num(CombatPowerService.GetMaxHp()) },
                new[] { "MoveSpeed", "공격 속도", CombatPowerService.GetAttackSpeedPct().ToString("0.#") + "%" },
            };
            var abRoot = FindDeep(p.Go.transform, "Ability");
            if (abRoot != null)
                foreach (var r in statRows)
                {
                    var row = abRoot.Find(r[0]);
                    if (row == null) continue;
                    var lb = row.Find("Text_Stats");
                    var lbt = lb != null ? lb.GetComponent<TMP_Text>() : null;
                    if (lbt != null) lbt.text = r[1];
                    var vl = row.Find("Text_Value");
                    var vlt = vl != null ? vl.GetComponent<TMP_Text>() : null;
                    if (vlt != null) vlt.text = r[2];
                }
            // 부제(세력·내공)는 Group_Left 직속 Text_Stats 하나뿐이다
            var glRoot = p.Find("Group_Left");
            var subLbl = glRoot != null ? glRoot.Find("Text_Stats") : null;
            var subTxt = subLbl != null ? subLbl.GetComponent<TMP_Text>() : null;
            if (subTxt != null)
                subTxt.text = $"{FactionService.DisplayName} · 내공 {(g != null ? g.Grade : 0)}단계";

            // 라벨 색은 배경 기준으로 명시 지정 (프리팹 캐시 방어 겸):
            // 스프라이트 판 위(종이·나무·칩) = 먹색, 맨 풍경 배경 위 = 크림.
            var inkL = new Color(0.25f, 0.15f, 0.08f);
            var creamL = new Color(0.95f, 0.91f, 0.80f);
            string[] fixNames = { "Text_Name", "Text_Info", "Text_Rune", "Text_Skills",
                "Text_Stats", "Text_Value", "Text_Exp", "Text_Lv", "Text_Trophy" };
            foreach (var tx in p.Go.GetComponentsInChildren<TMP_Text>(true))
            {
                if (System.Array.IndexOf(fixNames, tx.name) < 0) continue;
                // Top(재화 칩) 여부는 끝까지 올라가서 먼저 판정 — 칩 이미지에서 멈추면
                // Top 검사에 못 닿아 재화 숫자가 먹색이 되던 버그
                bool underTop = false;
                for (var a = tx.transform.parent; a != null; a = a.parent)
                    if (a.name == "Top") { underTop = true; break; }
                if (underTop) continue;   // 재화 칩 색은 FillCurrencies가 관리

                bool onSprite = false;
                for (var a = tx.transform.parent; a != null; a = a.parent)
                {
                    if (a.name == "WuxSafe") break;   // 그 위는 풍경 배경
                    var ai = a.GetComponent<Image>();
                    // 아이콘 스프라이트(트로피 등)는 '배경'이 아니다 — 오판하면
                    // 어두운 풍경 위 숫자가 먹색이 되어 사라진다
                    if (ai != null && ai.enabled && ai.sprite != null && ai.color.a > 0.4f
                        && !ai.sprite.name.StartsWith("Icon_"))
                    { onSprite = true; break; }
                }
                tx.color = onSprite ? inkL : creamL;
            }

            // ---- 다크+금 재스킨 (AAA 캐릭터창 문법: 반투명 먹색 행 + 금 포인트) ----
            var rowDim = Resources.Load<Sprite>("WuxiaUi/row_dim");
            var btnDark = Resources.Load<Sprite>("WuxiaUi/btn_dark");
            var slotDark = Resources.Load<Sprite>("WuxiaUi/slot_dark");
            var groundRing = Resources.Load<Sprite>("WuxiaUi/ground_ring");
            var gold = new Color(0.85f, 0.72f, 0.45f);
            var softWhite = new Color(0.96f, 0.94f, 0.90f);
            var grayLbl = new Color(0.80f, 0.77f, 0.72f);

            // 능력치 행: 먹색 반투명 행 + 라벨 회색·값 흰색
            var abil = p.Find("Ability");
            if (abil != null && rowDim != null)
            {
                foreach (Transform row in abil)
                {
                    var ri = row.GetComponent<Image>();
                    if (ri != null)
                    {
                        ri.sprite = rowDim;
                        ri.type = Image.Type.Sliced;
                        ri.color = new Color(1f, 1f, 1f, 0.92f);
                    }
                    var lt = row.Find("Text_Stats");
                    if (lt != null) lt.GetComponent<TMP_Text>().color = grayLbl;
                    var vt = row.Find("Text_Value");
                    if (vt != null) vt.GetComponent<TMP_Text>().color = softWhite;
                }
            }

            // 섹션 라벨(능력치·비급)은 금색
            foreach (var t2 in p.Go.GetComponentsInChildren<TMP_Text>(true))
            {
                var s2 = (t2.text ?? "").Trim();
                if (s2 == "능력치" || s2 == "비급") t2.color = gold;
            }

            // 버튼·강화 칩: 어두운 옻칠 + 금테
            foreach (var bn in new[] { "Button_Upgrade", "Button_Selet", "Label_Upgrade" })
            {
                var bb = p.Find(bn);
                var bi = bb != null ? bb.GetComponent<Image>() : null;
                if (bi != null && btnDark != null)
                {
                    bi.sprite = btnDark;
                    bi.type = Image.Type.Sliced;
                    bi.color = Color.white;
                }
            }
            var tSel = p.Get<TMP_Text>("Text_Select");
            if (tSel != null) tSel.color = softWhite;
            var tGoldTxt = p.Get<TMP_Text>("Text_Gold");
            if (tGoldTxt != null) tGoldTxt.color = softWhite;
            var tUp = p.Get<TMP_Text>("Text_Upgrade");
            if (tUp != null) tUp.color = gold;

            // 비급 슬롯·레벨칩·경험치바: 다크 슬롯/트랙 + 금 포인트
            foreach (var img3 in p.Go.GetComponentsInChildren<Image>(true))
            {
                if (img3.sprite == null) continue;
                string sn3 = img3.sprite.name;
                if (sn3 == "ItemFrame05_d" && slotDark != null)
                {
                    img3.sprite = slotDark;
                    img3.type = Image.Type.Sliced;
                    img3.color = Color.white;
                }
                else if (sn3 == "slot_frame")
                {
                    // 비급 아이콘의 밝은 나무 프레임은 다크 톤에서 이질적 → 숨김
                    img3.enabled = false;
                }
                else if (sn3 == "Character_Shadow01")
                {
                    img3.enabled = false;   // 발밑 문양으로 대체
                }
                else if (sn3 == "Slider_Level01_Bg")
                {
                    img3.color = new Color(0.07f, 0.07f, 0.07f, 0.92f);
                }
                else if (sn3 == "chip_gold" && img3.name == "Level" && slotDark != null)
                {
                    img3.sprite = slotDark;
                    img3.type = Image.Type.Sliced;
                    img3.color = Color.white;
                }
            }
            var tLv = p.Get<TMP_Text>("Text_Lv");
            if (tLv != null) tLv.color = gold;

            // 데모 로봇 대신 실제 플레이어 리그를 렌더해서 붙인다
            AttachPlayerPreview(p);

            // 캐릭터 발밑 원형 문양 (공중부양 해소) — 프리뷰 발끝 기준으로 매 빌드 정렬
            var chSlot = p.Find("Character");
            if (chSlot != null && groundRing != null)
            {
                var ringT = chSlot.Find("GroundRing") as RectTransform;
                if (ringT == null)
                {
                    var g2 = new GameObject("GroundRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    g2.transform.SetParent(chSlot, false);
                    g2.transform.SetAsFirstSibling();
                    ringT = g2.GetComponent<RectTransform>();
                    ringT.anchorMin = ringT.anchorMax = new Vector2(0.5f, 0.5f);
                    ringT.sizeDelta = new Vector2(500f, 160f);
                    var gi = g2.GetComponent<Image>();
                    gi.sprite = groundRing;
                    gi.preserveAspect = false;   // 정원 링을 눌러 바닥 타원으로
                    gi.raycastTarget = false;
                    gi.color = new Color(1f, 1f, 1f, 0.55f);
                }
                var prevRt2 = _charPrev != null ? _charPrev.Rect : null;
                ringT.anchoredPosition = prevRt2 != null
                    ? new Vector2(prevRt2.anchoredPosition.x,
                        prevRt2.anchoredPosition.y - prevRt2.sizeDelta.y * 0.5f + 25f)
                    : new Vector2(0f, -260f);
            }

            // 프리팹의 좌우 넘김 화살표는 '여러 캐릭터를 고르는' 데모용이다.
            // 우리는 플레이어 캐릭터가 하나뿐이라 눌러도 아무 일이 없어 혼란만 준다.
            p.SetActive("Button_Arrow_L", false);
            p.SetActive("Button_Arrow_R", false);

            // 강화 = 스탯 포인트 투자
            int sp2 = g != null ? g.StatPoints : 0;
            p.SetText("Text_Gold", sp2 + " SP");
            p.SetText("Text_Upgrade", "공격 강화");
            p.SetInteractable("Button_Upgrade", sp2 > 0);
            p.OnClick("Button_Upgrade", () =>
            {
                if (PlayerGrowth.Instance == null) return;
                // 키는 대문자다 — "atk"를 넘기면 default로 빠져 아무 일도 안 일어났다(버그)
                bool ok = PlayerGrowth.Instance.TrySpendStatPoint("ATK");
                Toast(ok ? "공격력 강화" : "스탯 포인트 부족");
                if (ok) CasualFx.EnhanceFlash(_host);
                BuildPlayerChar(); _refresh?.Invoke();
            });
            p.SetText("Text_Select", "외형 꾸미기");
            p.OnClick("Button_Selet", () => { p.Hide(); _openAppearance?.Invoke(); });

            LayoutCharGrid(p);

            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>
        /// 배경(bg_char)에 그려진 구역에 UI를 앉힌다 — 좌 정보판·중앙 무대·우 5행 그리드.
        /// 좌표는 아트 픽셀 실측값(1600×896 기준)을 비율로 환산한 것.
        /// </summary>
        static void LayoutCharGrid(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            if (p.Go.transform.Find("WuxWindow") == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            if (safe == null) return;
            // WuxSafe가 인셋을 주면 배경 좌표와 어긋난다 → 배경과 동일 영역으로
            PlaceIn(safe, 0f, 0f, 1f, 1f);

            LayoutCharWindow(p, safe);
            return;
#pragma warning disable 0162
            var gl = p.Find("Group_Left");
            var gc = p.Find("Group_Center");
            var gr = p.Find("Group_Right");

            // 배경 실측(카툰판 v2): 좌 패널 x 7.2~28.9%, 우 패널 x 69.5~92.3%,
            // 두 패널 y 13.5~83.0%, 무대 x 32.0~66.2% 상단 23.3%
            if (gl != null) PlaceIn(gl, 0.085f, 0.150f, 0.278f, 0.818f);
            if (gr != null) PlaceIn(gr, 0.706f, 0.150f, 0.913f, 0.818f);
            if (gc != null) PlaceIn(gc, 0.320f, 0.233f, 0.662f, 0.95f);

            // ---- 좌 패널 내부 (패널 로컬 비율) ----
            if (gl != null)
            {
                var nameG = gl.Find("Name_Group");
                if (nameG != null) PlaceIn(nameG, 0.06f, 0.89f, 0.94f, 0.99f);
                // 부제(정파·내공)는 Group_Left 직속 Text_Stats — 안 잡으면 레벨칩과 겹친다
                var subT = gl.Find("Text_Stats");
                if (subT != null) PlaceIn(subT, 0.06f, 0.825f, 0.94f, 0.885f);
                var lvInfo = gl.Find("Level_Info");
                if (lvInfo != null) PlaceIn(lvInfo, 0.06f, 0.705f, 0.94f, 0.815f);
                var infoT = gl.Find("Text_Info");
                if (infoT != null) PlaceIn(infoT, 0.06f, 0.60f, 0.94f, 0.70f);
                var runeL = gl.Find("Text_Rune");
                if (runeL != null) PlaceIn(runeL, 0.06f, 0.545f, 0.94f, 0.60f);
                var rune = gl.Find("Rune");
                if (rune != null) PlaceIn(rune, 0.06f, 0.35f, 0.94f, 0.53f);

                // 하단 버튼 2개를 좌 패널 안으로 옮겨 세로로 쌓는다 (창 어디에도 안 뜨게)
                var bm = p.Find("BottomMenu");
                if (bm != null)
                {
                    bm.SetParent(gl, false);
                    PlaceIn(bm, 0.04f, 0.03f, 0.96f, 0.34f);
                    // 레이아웃 그룹을 지우면 ContentSizeFitter가 크기를 0으로 만든다 → 같이 제거
                    var hl = bm.GetComponent<HorizontalLayoutGroup>();
                    if (hl != null) Object.DestroyImmediate(hl);
                    foreach (var csf in bm.GetComponentsInChildren<ContentSizeFitter>(true))
                        Object.DestroyImmediate(csf);
                    foreach (var le3 in bm.GetComponentsInChildren<LayoutElement>(true))
                        Object.DestroyImmediate(le3);
                    var up = bm.Find("Button_Upgrade");
                    if (up != null) PlaceIn(up, 0f, 0.54f, 1f, 1f);
                    var sel = bm.Find("Button_Selet");
                    if (sel != null) PlaceIn(sel, 0f, 0f, 1f, 0.46f);

                    // 버튼 내부: 킷 원본 오프셋(떠 있는 칩·코인)이 좁은 버튼에서 겹친다
                    // → 칩·코인은 숨기고 라벨 하나만 가운데 (한 줄로 정보 통합)
                    foreach (var img4 in bm.GetComponentsInChildren<Image>(true))
                    {
                        if (img4.name == "Label_Upgrade" || img4.name == "Gold"
                            || img4.name == "Icon_Gold" || img4.name == "Coin")
                            img4.gameObject.SetActive(false);
                    }
                    foreach (var t4 in bm.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (!t4.gameObject.activeInHierarchy) continue;
                        var trt4 = t4.rectTransform;
                        trt4.anchorMin = Vector2.zero;
                        trt4.anchorMax = Vector2.one;
                        trt4.offsetMin = new Vector2(12f, 0f);
                        trt4.offsetMax = new Vector2(-12f, 0f);
                        t4.alignment = TMPro.TextAlignmentOptions.Center;
                        t4.enableWordWrapping = false;
                        t4.fontSize = 38f;
                    }
                    var upTxt = up != null ? up.GetComponentInChildren<TMP_Text>(true) : null;
                    if (upTxt != null)
                    {
                        int sp3 = PlayerGrowth.Instance != null ? PlayerGrowth.Instance.StatPoints : 0;
                        upTxt.text = "공격 강화 · " + sp3 + " SP";
                        upTxt.gameObject.SetActive(true);
                    }
                }
            }

            // ---- 우 패널: 배경에 그려진 6칸 그리드 (맨 위=헤더, 아래 5칸=스탯) ----
            if (gr != null)
            {
                // 배경 구분선 실측(패널 로컬): 헤더 아래 5행
                float[] rowY = { 1f, 0.765f, 0.607f, 0.446f, 0.289f, 0.161f, 0f };
                var head = gr.Find("Text_Stats");
                if (head != null) PlaceIn(head, 0.08f, rowY[1], 0.92f, rowY[0]);
                var ab = gr.Find("Ability");
                if (ab != null)
                {
                    PlaceIn(ab, 0f, 0f, 1f, 1f);
                    var vg = ab.GetComponent<VerticalLayoutGroup>();
                    if (vg != null) Object.DestroyImmediate(vg);
                    foreach (var csf2 in ab.GetComponentsInChildren<ContentSizeFitter>(true))
                        Object.DestroyImmediate(csf2);
                    int ri = 0;
                    foreach (Transform row in ab)
                    {
                        if (ri >= 5) break;
                        PlaceIn(row, 0.06f, rowY[ri + 2], 0.94f, rowY[ri + 1]);
                        // 배경에 그리드가 그려져 있으니 행 판은 끈다
                        var ri2 = row.GetComponent<Image>();
                        if (ri2 != null) ri2.enabled = false;
                        ri++;
                    }
                }
            }

            // 캐릭터는 배경에 그려진 무대 위에 선다 (공중부양 해소)
            if (gc != null)
            {
                var chSlot2 = gc.Find("Character");
                if (chSlot2 != null) PlaceIn(chSlot2, 0f, 0f, 1f, 1f);
            }
            if (_charPrev != null && _charPrev.Rect != null)
            {
                var prt = _charPrev.Rect;
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
                prt.pivot = new Vector2(0.5f, 0f);
                prt.anchoredPosition = new Vector2(0f, -70f);   // 무대 상단에 발 (아트 하단 여백 보정)
            }

            // 무대가 그려져 있으니 별도 발밑 문양은 끈다
            foreach (var rt5 in p.Go.GetComponentsInChildren<RectTransform>(true))
                if (rt5.name == "GroundRing") rt5.gameObject.SetActive(false);

            // 배경 패널이 크림 종이 — 그 위 글씨는 전부 먹색 (버튼 위만 크림)
            var ink2 = new Color(0.24f, 0.15f, 0.08f);
            var deepRed = new Color(0.55f, 0.20f, 0.12f);
            var cream2 = new Color(0.96f, 0.93f, 0.86f);
            foreach (var side in new[] { gl, gr })
            {
                if (side == null) continue;
                foreach (var t6 in side.GetComponentsInChildren<TMP_Text>(true))
                {
                    bool onBtn = false;
                    for (var a = t6.transform; a != null && a != side; a = a.parent)
                        if (a.name == "BottomMenu" || a.name == "Level") { onBtn = true; break; }
                    t6.color = onBtn ? cream2 : ink2;
                }
                var hd = side.Find("Text_Stats");
                if (hd != null)
                {
                    var ht = hd.GetComponent<TMP_Text>();
                    if (ht != null) { ht.color = deepRed; ht.fontStyle |= TMPro.FontStyles.Bold; }
                }
            }
#pragma warning restore 0162
        }

        /// <summary>
        /// 목록형 창 배치 (문파·랭킹 공용): 단일 창 + 가로 5줄(좌측 문장 소켓) + 하단 판 2개.
        /// 좌표는 win_list_main(1360×1008) 픽셀 실측값.
        /// </summary>
        static void LayoutListWindow(CasualPanel p, string leftPlate, string rightPlate)
        {
            if (p == null || p.Go == null) return;
            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (wux == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            if (safe != null) PlaceIn(safe, 0f, 0f, 1f, 1f);

            var ink = new Color(0.26f, 0.16f, 0.08f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            PlaceIn(wux, 0f, 0f, 1f, 1f);
            var wi = wux.GetComponent<Image>();
            if (wi != null) { wi.sprite = null; wi.color = new Color(0.04f, 0.03f, 0.02f, 0.88f); }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);
            foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(true))
                ps.gameObject.SetActive(false);

            int baseIdx = wux.GetSiblingIndex();
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_list", baseIdx + 1);
            PlaceIn(main, 0.160f, 0.050f, 0.843f, 0.950f);
            WuxUiFx.PlayOpen(main);
            var sideOld = p.Go.transform.Find("WuxSide");
            if (sideOld != null) sideOld.gameObject.SetActive(false);
            if (safe != null) safe.SetSiblingIndex(baseIdx + 2);

            var pn = p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.300f, 0.888f, 0.700f, 0.925f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 46f; pn.color = ink; pn.enableVertexGradient = false;
                pn.enableWordWrapping = false;
            }

            // 목록: 배경에 그려진 5줄 위에 얹고, 더 많으면 스크롤
            var sr = p.Go.GetComponentInChildren<ScrollRect>(true);
            if (sr != null && sr.content != null)
            {
                var srt = sr.GetComponent<RectTransform>();
                srt.SetParent(main, false);
                PlaceIn(srt, 0.105f, 0.130f, 0.895f, 0.830f);
                var srImg = srt.GetComponent<Image>();
                if (srImg != null) srImg.enabled = false;
                if (srt.GetComponent<RectMask2D>() == null) srt.gameObject.AddComponent<RectMask2D>();
                sr.viewport = srt;
                sr.horizontal = false; sr.vertical = true;

                var content = sr.content;
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
                content.offsetMin = new Vector2(0f, content.offsetMin.y);
                content.offsetMax = new Vector2(0f, content.offsetMax.y);
                content.offsetMin = new Vector2(0f, content.offsetMin.y);
                content.offsetMax = new Vector2(0f, content.offsetMax.y);
                content.anchoredPosition = Vector2.zero;

                float rootH = p.Root != null ? p.Root.rect.height : 1440f;
                float mainH = (0.950f - 0.050f) * rootH;
                float pitch = 0.1045f * mainH;          // 줄 간격(실측)
                float rowH = 0.088f * mainH;            // 줄 높이(실측)

                var vg = content.GetComponent<VerticalLayoutGroup>();
                if (vg == null) vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                vg.spacing = pitch - rowH;
                vg.padding = new RectOffset(0, 0, 0, 0);
                // childControlHeight=false면 LayoutElement 높이가 무시돼 줄 간격이 2배가 된다
                vg.childControlHeight = true; vg.childControlWidth = true;
                vg.childForceExpandHeight = false; vg.childForceExpandWidth = true;
                var csf = content.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                foreach (Transform row in content)
                {
                    var le = row.GetComponent<LayoutElement>();
                    if (le == null) le = row.gameObject.AddComponent<LayoutElement>();
                    le.preferredHeight = rowH; le.minHeight = rowH;
                    // 목록이 배경의 줄 수(5)보다 길어질 수 있다 → 줄마다 자체 종이띠를 입힌다
                    var ri = row.GetComponent<Image>();
                    if (ri != null)
                    {
                        var strip = Resources.Load<Sprite>("WuxiaUi/row_dark");
                        if (strip != null)
                        {
                            ri.enabled = true; ri.sprite = strip;
                            ri.type = Image.Type.Sliced; ri.color = Color.white;
                        }
                        else ri.enabled = false;
                    }

                    // 줄 내부: 좌=문장 소켓 / 중=이름·레벨 / 우=수치 (줄 높이를 넘지 않게)
                    var emblem = FindDeep(row, "Icon_Clan") ?? FindDeep(row, "Icon")
                        ?? FindDeep(row, "Image_Clan");
                    if (emblem != null && emblem != row)
                    {
                        emblem.SetParent(row, false);
                        PlaceIn(emblem, 0.012f, 0.12f, 0.088f, 0.88f);
                        var ei = emblem.GetComponent<Image>();
                        if (ei != null) { ei.enabled = true; ei.preserveAspect = true; }
                    }
                    var texts = new List<TMP_Text>();
                    foreach (var t in row.GetComponentsInChildren<TMP_Text>(true))
                        if (t.gameObject.activeSelf && !string.IsNullOrEmpty(t.text)) texts.Add(t);
                    // 이름 칸은 노드 이름으로 특정한다 (글자 크기 추정은 빗나간다)
                    TMP_Text nameTx = null;
                    foreach (var t in texts)
                        if (t.name == "Text_OneofThem" || t.name == "Text_NickName"
                            || t.name == "Text_Name") { nameTx = t; break; }
                    if (nameTx == null)
                    {
                        float best = -1f;
                        foreach (var t in texts) if (t.fontSize > best) { best = t.fontSize; nameTx = t; }
                    }
                    int side = 0;
                    foreach (var t in texts)
                    {
                        t.color = ink;
                        t.enableWordWrapping = false;
                        var tr = t.rectTransform;
                        tr.SetParent(row, false);
                        if (t == nameTx)
                        {
                            PlaceIn(tr, 0.105f, 0.10f, 0.45f, 0.90f);
                            t.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                            t.enableAutoSizing = true; t.fontSizeMin = 18f; t.fontSizeMax = 34f;
                        }
                        else
                        {
                            float x0 = 0.47f + side * 0.175f;
                            PlaceIn(tr, x0, 0.10f, Mathf.Min(0.985f, x0 + 0.17f), 0.90f);
                            t.alignment = TMPro.TextAlignmentOptions.Center;
                            t.enableAutoSizing = true; t.fontSizeMin = 14f; t.fontSizeMax = 26f;
                            side++;
                        }
                    }
                    // 남은 킷 장식(파란 왕관 등)은 줄 밖으로 튀어나온다 → 정리
                    foreach (var im in row.GetComponentsInChildren<Image>(true))
                    {
                        if (im.transform == row) continue;
                        if (emblem != null && im.transform == emblem) continue;
                        im.enabled = false;
                    }
                }
            }

            // 하단 두 판
            var lbl1 = main.Find("ListPlateL") as RectTransform;
            if (lbl1 == null)
                lbl1 = UiKit.TmpLabel(main, "ListPlateL", "", 28, cream, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lbl1, 0.147f, 0.120f, 0.485f, 0.205f);
            var lbl2 = main.Find("ListPlateR") as RectTransform;
            if (lbl2 == null)
                lbl2 = UiKit.TmpLabel(main, "ListPlateR", "", 28, cream, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lbl2, 0.515f, 0.120f, 0.853f, 0.205f);
            var t1 = lbl1.GetComponent<TMP_Text>();
            if (t1 != null) { t1.text = leftPlate ?? ""; t1.color = cream; t1.fontSize = 30f; }
            var t2 = lbl2.GetComponent<TMP_Text>();
            if (t2 != null) { t2.text = rightPlate ?? ""; t2.color = cream; t2.fontSize = 30f; }

            foreach (var gname in new[] { "Group_Left", "Group_Right", "Tap", "Tap_Menu", "Bottom_Menu" })
            {
                var gt = p.Find(gname);
                if (gt != null && gt.parent != main) gt.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 동료창 배치: 단일 창 + 5×2 초상 액자(각 액자 하단에 이름 판).
        /// 좌표는 win_comp_main(1360×1008) 픽셀 실측값.
        /// </summary>
        static void LayoutCompanionWindow(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (wux == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            if (safe != null) PlaceIn(safe, 0f, 0f, 1f, 1f);

            var ink = new Color(0.26f, 0.16f, 0.08f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            PlaceIn(wux, 0f, 0f, 1f, 1f);
            var wi = wux.GetComponent<Image>();
            if (wi != null) { wi.sprite = null; wi.color = new Color(0.04f, 0.03f, 0.02f, 0.88f); }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);
            foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(true))
                ps.gameObject.SetActive(false);

            int baseIdx = wux.GetSiblingIndex();
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_slots10", baseIdx + 1);
            PlaceIn(main, 0.160f, 0.050f, 0.843f, 0.950f);
            WuxUiFx.PlayOpen(main);
            // 프리팹에 남은 탭·분류 글씨가 격자 위에 겹쳐 뜬다 (이 창은 탭을 쓰지 않는다)
            foreach (var strayName in new[] { "TapMenu", "Category" })
            {
                var stray = p.Find(strayName);
                if (stray != null) stray.gameObject.SetActive(false);
            }
            var sideOld = p.Go.transform.Find("WuxSide");
            if (sideOld != null) sideOld.gameObject.SetActive(false);
            if (safe != null) safe.SetSiblingIndex(baseIdx + 2);

            var pn = p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.340f, 0.860f, 0.660f, 0.925f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 44f; pn.color = ink; pn.enableVertexGradient = false;
                pn.enableWordWrapping = false;
            }

            // 좌표는 win_kit_slots10의 먹선을 실측한 값 (그림이 기준, 코드가 따라간다)
            float[,] cx = { { 0.095f, 0.219f }, { 0.267f, 0.390f }, { 0.439f, 0.561f },
                            { 0.610f, 0.732f }, { 0.781f, 0.902f } };
            float[,] cy = { { 0.480f, 0.808f }, { 0.100f, 0.418f } };

            var sr = p.Go.GetComponentInChildren<ScrollRect>(true);
            Transform content = sr != null ? (Transform)sr.content : null;
            if (content == null) return;
            var srt2 = sr.GetComponent<RectTransform>();
            srt2.SetParent(main, false);
            PlaceIn(srt2, 0f, 0f, 1f, 1f);
            var srImg2 = srt2.GetComponent<Image>();
            if (srImg2 != null) srImg2.enabled = false;
            sr.horizontal = false; sr.vertical = false;
            var mk = srt2.GetComponent<RectMask2D>();
            if (mk != null) Object.DestroyImmediate(mk);
            PlaceIn(content, 0f, 0f, 1f, 1f);
            foreach (var lg in content.GetComponents<LayoutGroup>()) Object.DestroyImmediate(lg);
            foreach (var csf in content.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(csf);

            int n = content.childCount;
            for (int i = 0; i < n; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (card == null) continue;
                if (i >= 10 || !card.gameObject.activeSelf) { card.gameObject.SetActive(false); continue; }
                int col = i % 5, row = i / 5;
                PlaceIn(card, cx[col, 0], cy[row, 0], cx[col, 1], cy[row, 1]);
                var ci = card.GetComponent<Image>();
                if (ci != null) ci.enabled = false;          // 액자는 배경에 그려져 있다

                var port = FindDeep(card, "Character");
                foreach (var im in card.GetComponentsInChildren<Image>(true))
                {
                    if (im.transform == card) continue;
                    if (port != null && im.transform == port) continue;
                    im.enabled = false;                       // 킷 카드 장식 제거
                }
                if (port != null)
                {
                    port.SetParent(card, false);
                    PlaceIn(port, 0.10f, 0.30f, 0.90f, 0.93f);
                    var pi = port.GetComponent<Image>();
                    if (pi != null) { pi.enabled = true; pi.preserveAspect = true; pi.color = Color.white; }
                }
                var nameT = FindDeep(card, "Text_Name");
                if (nameT != null)
                {
                    nameT.SetParent(card, false);
                    PlaceIn(nameT, 0.04f, 0.04f, 0.96f, 0.24f);
                    var t = nameT.GetComponent<TMP_Text>();
                    if (t != null)
                    {
                        // 데이터 이름이 "궁수 동료"처럼 접미사를 달고 있다 — 표시에서만 뗀다
                        string nmTxt = (t.text ?? "").Trim();
                        if (nmTxt.EndsWith(" 동료")) t.text = nmTxt.Substring(0, nmTxt.Length - 3);
                        t.alignment = TMPro.TextAlignmentOptions.Center;
                        t.color = cream; t.enableWordWrapping = false;
                        t.fontStyle |= TMPro.FontStyles.Bold;
                        t.enableAutoSizing = true; t.fontSizeMin = 12f; t.fontSizeMax = 24f;
                    }
                }
                var lvT = FindDeep(card, "Text_Slider") ?? FindDeep(card, "Text_Level");
                if (lvT != null)
                {
                    lvT.SetParent(card, false);
                    PlaceIn(lvT, 0.10f, 0.24f, 0.90f, 0.36f);
                    var t = lvT.GetComponent<TMP_Text>();
                    if (t != null)
                    {
                        t.alignment = TMPro.TextAlignmentOptions.Center;
                        t.color = ink; t.enableWordWrapping = false;
                        t.enableAutoSizing = true; t.fontSizeMin = 10f; t.fontSizeMax = 20f;
                    }
                }
                foreach (var t2 in card.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (t2.transform == nameT || t2.transform == lvT) continue;
                    t2.gameObject.SetActive(false);           // 남은 킷 문구 제거
                }
                // 액자 밖으로 새어 나온 킷 배지(레벨 원판 등)도 정리
                foreach (var badge in new[] { "Level", "Level_Full", "Focus", "Rank", "Star" })
                {
                    var bt = FindDeep(card, badge);
                    if (bt != null && bt != nameT && bt != lvT) bt.gameObject.SetActive(false);
                }
            }

            // 정렬 바(레벨/출전중/고등급)를 창 안 제목 아래로 — 창 밖 갈색 띠로 떠 있었다
            var sortBar = p.Find("Menu") ?? p.Find("Sort") ?? p.Find("Top_Menu");
            if (sortBar == null)
            {
                foreach (var t3 in p.Go.GetComponentsInChildren<TMP_Text>(true))
                    if (t3.name == "Text_Menu" && t3.transform.parent != null)
                    { sortBar = t3.transform.parent.parent ?? t3.transform.parent; break; }
            }
            if (sortBar != null && sortBar.parent != main)
            {
                sortBar.SetParent(main, false);
                PlaceIn(sortBar, 0.20f, 0.790f, 0.86f, 0.860f);
                foreach (var im2 in sortBar.GetComponentsInChildren<Image>(true))
                    im2.enabled = false;                       // 갈색 띠 제거, 글자만
                foreach (var t4 in sortBar.GetComponentsInChildren<TMP_Text>(true))
                {
                    t4.fontSize = 26f;
                    if (t4.color != MaxColor) t4.color = cream;
                }
            }

            // 하단 안내 판
            var lbl = main.Find("CompInfo") as RectTransform;
            if (lbl == null)
                lbl = UiKit.TmpLabel(main, "CompInfo", "", 28, cream, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            // 격자가 시트를 꽉 채워 안내문 자리가 없다. 보유 수는 현판 제목이 이미 말해 준다.
            lbl.gameObject.SetActive(false);
            PlaceIn(lbl, 0.16f, 0.812f, 0.84f, 0.852f);
            var lt = lbl.GetComponent<TMP_Text>();
            var ca2 = CompanionAdapter.Instance;
            if (lt != null)
            {
                lt.text = "동료를 눌러 상세를 확인하세요" +
                    (ca2 != null ? "   ·   보유 " + ca2.OwnedCount : "");
                lt.color = cream; lt.fontSize = 30f; lt.enableWordWrapping = false;
            }

            foreach (var gname in new[] { "Group_Left", "Group_Right", "Tap", "Tap_Menu", "Bottom_Menu" })
            {
                var gt = p.Find(gname);
                if (gt != null && gt.parent != main) gt.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 상점창 배치: 단일 창 + 3×2 좌판 상품 명패(각 명패 하단에 가격 판).
        /// 좌표는 win_shop_main(1360×1008) 픽셀 실측값.
        /// </summary>
        static void LayoutShopWindow(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (wux == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            if (safe != null) PlaceIn(safe, 0f, 0f, 1f, 1f);

            var ink = new Color(0.26f, 0.16f, 0.08f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            PlaceIn(wux, 0f, 0f, 1f, 1f);
            var wi = wux.GetComponent<Image>();
            if (wi != null) { wi.sprite = null; wi.color = new Color(0.04f, 0.03f, 0.02f, 0.88f); }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);

            int baseIdx = wux.GetSiblingIndex();
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_cards6", baseIdx + 1);
            PlaceIn(main, 0.160f, 0.050f, 0.843f, 0.950f);
            WuxUiFx.PlayOpen(main);
            var sideOld = p.Go.transform.Find("WuxSide");
            if (sideOld != null) sideOld.gameObject.SetActive(false);
            if (safe != null) safe.SetSiblingIndex(baseIdx + 2);

            var pn = p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.335f, 0.858f, 0.665f, 0.922f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 46f; pn.color = ink; pn.enableVertexGradient = false;
            }

            // 좌표는 win_kit_cards6의 먹선 실측값
            float[,] cx = { { 0.098f, 0.325f }, { 0.385f, 0.612f }, { 0.672f, 0.898f } };
            float[,] cy = { { 0.480f, 0.795f }, { 0.093f, 0.408f } };
            var sr = p.Go.GetComponentInChildren<ScrollRect>(true);
            Transform content = sr != null ? (Transform)sr.content : null;
            if (content == null) return;
            if (sr != null)
            {
                var srt = sr.GetComponent<RectTransform>();
                srt.SetParent(main, false);
                PlaceIn(srt, 0f, 0f, 1f, 1f);
                var srImg = srt.GetComponent<Image>();
                if (srImg != null) srImg.enabled = false;
                sr.horizontal = false; sr.vertical = false;
                var mask = srt.GetComponent<RectMask2D>();
                if (mask != null) Object.DestroyImmediate(mask);
            }
            PlaceIn(content, 0f, 0f, 1f, 1f);
            foreach (var lg in content.GetComponents<LayoutGroup>()) Object.DestroyImmediate(lg);
            foreach (var csf in content.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(csf);

            int n = content.childCount;
            for (int i = 0; i < n; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (card == null) continue;
                if (i >= 6) { card.gameObject.SetActive(false); continue; }
                card.gameObject.SetActive(true);
                int col = i % 3, row = i / 3;
                PlaceIn(card, cx[col, 0], cy[row, 0], cx[col, 1], cy[row, 1]);
                // 카드 판·장식은 배경에 그려져 있다 — 상품 그림 하나만 남긴다
                var ci = card.GetComponent<Image>();
                if (ci != null) ci.enabled = false;
                var prodIcon = FindIconImage(card);
                foreach (var im in card.GetComponentsInChildren<Image>(true))
                {
                    if (im.transform == card) continue;
                    if (prodIcon != null && im == prodIcon) continue;
                    im.enabled = false;
                }
                if (prodIcon != null)
                {
                    prodIcon.enabled = true;
                    prodIcon.transform.SetParent(card, false);
                    PlaceIn(prodIcon.transform, 0.22f, 0.34f, 0.78f, 0.86f);
                    prodIcon.preserveAspect = true;
                    prodIcon.color = Color.white;
                }
                // 글자는 부모를 카드로 통일해야 비율 좌표가 맞는다
                var tt = FindDeep(card, "Text_ItemTitle");
                if (tt != null)
                {
                    tt.SetParent(card, false);
                    PlaceIn(tt, 0.04f, 0.86f, 0.96f, 0.99f);
                    var t = tt.GetComponent<TMP_Text>();
                    if (t != null)
                    {
                        t.alignment = TMPro.TextAlignmentOptions.Center;
                        t.color = ink; t.enableWordWrapping = false;
                        t.fontStyle |= TMPro.FontStyles.Bold;
                        t.enableAutoSizing = true; t.fontSizeMin = 16f; t.fontSizeMax = 30f;
                    }
                }
                var vt = FindDeep(card, "Text_Value");
                if (vt != null)
                {
                    vt.SetParent(card, false);
                    PlaceIn(vt, 0.08f, 0.03f, 0.92f, 0.26f);   // 가격 판 위
                    var t = vt.GetComponent<TMP_Text>();
                    if (t != null)
                    {
                        t.alignment = TMPro.TextAlignmentOptions.Center;
                        t.color = ink; t.enableWordWrapping = false;   // 한지 위 — 크림은 안 읽힌다
                        t.fontStyle |= TMPro.FontStyles.Bold;
                        t.enableAutoSizing = true; t.fontSizeMin = 14f; t.fontSizeMax = 26f;
                    }
                }
            }

            // 하단 안내 판
            var infoT = p.Get<TMP_Text>("Text_Info");
            if (infoT != null)
            {
                infoT.rectTransform.SetParent(main, false);
                PlaceIn(infoT.rectTransform, 0.20f, 0.150f, 0.81f, 0.290f);
                infoT.alignment = TMPro.TextAlignmentOptions.Center;
                infoT.fontSize = 30f; infoT.color = cream; infoT.enableWordWrapping = false;
            }

            foreach (var gname in new[] { "Group_Left", "Group_Right", "Tap", "Tap_Menu", "Bottom_Menu" })
            {
                var gt = p.Find(gname);
                if (gt != null && gt.parent != main) gt.gameObject.SetActive(false);
            }
            // 분홍 광채의 정체는 킷 프리팹의 파티클(Fx_Rotate/Sparkle 등) — 창 톤과 안 맞는다
            foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(true))
                ps.gameObject.SetActive(false);

            // 킷 카드의 글로우 이미지도 끈다 (상단 재화칩은 제외)
            foreach (var im in p.Go.GetComponentsInChildren<Image>(true))
            {
                bool underTop = false;
                for (var a = im.transform; a != null; a = a.parent)
                    if (a.name == "Top") { underTop = true; break; }
                if (underTop) continue;
                string sn = im.sprite != null ? im.sprite.name : "";
                if (im.name.Contains("Glow") || sn.Contains("Glow") || sn.Contains("ScreenGlow"))
                    im.enabled = false;
            }
            // 카드 밖으로 새어 나온 가격 표시 정리
            foreach (var t in content.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.name != "Text_Value") continue;
                bool inCard = false;
                for (var a = t.transform.parent; a != null && a != content; a = a.parent)
                    if (a.parent == content) { inCard = true; break; }
                if (!inCard) t.gameObject.SetActive(false);
            }
        }

        /// <summary>이름으로 자손을 재귀 탐색 (첫 일치).</summary>
        static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        /// <summary>
        /// 무공창 배치: 좌=무공 상세(공용 상세 창), 우=서고(2단 책장에 비급 4권씩).
        /// 좌표는 win_skill_main(1360×1008) 픽셀 실측값.
        /// </summary>
        static void LayoutSkillWindow(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (wux == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            if (safe != null) PlaceIn(safe, 0f, 0f, 1f, 1f);

            var ink = new Color(0.26f, 0.16f, 0.08f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            PlaceIn(wux, 0f, 0f, 1f, 1f);
            var wi = wux.GetComponent<Image>();
            if (wi != null) { wi.sprite = null; wi.color = new Color(0.04f, 0.03f, 0.02f, 0.88f); }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);

            int baseIdx = wux.GetSiblingIndex();
            var side = EnsureArt(p.Go.transform, "WuxSide", "win_kit_side", baseIdx + 1);
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_grid", baseIdx + 2);
            PlaceIn(side, 0.020f, 0.080f, 0.283f, 0.920f);
            PlaceIn(main, 0.300f, 0.049f, 0.985f, 0.951f);
            WuxUiFx.PlayOpen(side); WuxUiFx.PlayOpen(main);
            if (safe != null) safe.SetSiblingIndex(baseIdx + 3);

            // 제목
            var pn = p.Get<TMP_Text>("Text_Missions") ?? p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.290f, 0.800f, 0.710f, 0.910f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 44f; pn.color = ink; pn.enableVertexGradient = false;
                pn.enableWordWrapping = false;
            }

            // 책장 2단에 비급 4권씩 (기존 Books 노드를 그대로 옮겨 쓴다)
            var shelfOld = p.Go.transform.Find("BookShelf");
            Transform books = null;
            if (shelfOld != null)
            {
                books = shelfOld.Find("Books");
                var si = shelfOld.GetComponent<Image>();
                if (si != null) si.enabled = false;      // 책장은 창 배경에 그려져 있다
            }
            if (books != null)
            {
                books.SetParent(main, false);
                PlaceIn(books, 0f, 0f, 1f, 1f);
                var hb = books.GetComponent<HorizontalLayoutGroup>();
                if (hb != null) Object.DestroyImmediate(hb);
                foreach (var csf in books.GetComponentsInChildren<ContentSizeFitter>(true))
                    Object.DestroyImmediate(csf);
                // 실측: 1단 y 0.584~0.733, 2단 0.430~0.578, 개구부 x 0.175~0.825
                float[,] shelf = { { 0.590f, 0.728f }, { 0.436f, 0.573f } };
                int n = books.childCount;
                for (int i = 0; i < n; i++)
                {
                    var b = books.GetChild(i) as RectTransform;
                    int rowIdx = i / 4, colIdx = i % 4;
                    if (rowIdx > 1) { b.gameObject.SetActive(false); continue; }
                    b.gameObject.SetActive(true);
                    float x0 = 0.185f, x1 = 0.815f;
                    float cw = (x1 - x0) / 4f;
                    float cx0 = x0 + cw * colIdx + 0.012f;
                    float cx1 = x0 + cw * (colIdx + 1) - 0.012f;
                    PlaceIn(b, cx0, shelf[rowIdx, 0], cx1, shelf[rowIdx, 1]);
                    var bi = b.GetComponent<Image>();
                    if (bi != null) bi.preserveAspect = true;
                    // 책등 글씨는 세로로 길게 — 칸에 맞춰 크기만 조정
                    var tt = b.Find("Title");
                    if (tt != null)
                    {
                        PlaceIn(tt, 0.06f, 0.16f, 0.94f, 0.95f);
                        var t = tt.GetComponent<TMP_Text>();
                        if (t != null)
                        {
                            // 책등을 넘지 않게 자동 축소 (세로 한 글자씩)
                            t.enableWordWrapping = false;
                            t.enableAutoSizing = true;
                            t.fontSizeMin = 12f; t.fontSizeMax = 24f;
                            t.alignment = TMPro.TextAlignmentOptions.Top;
                        }
                    }
                    var lv = b.Find("Lv");
                    if (lv != null)
                    {
                        PlaceIn(lv, 0.02f, 0.01f, 0.98f, 0.15f);
                        var t = lv.GetComponent<TMP_Text>();
                        if (t != null)
                        {
                            t.enableWordWrapping = false;
                            t.enableAutoSizing = true;
                            t.fontSizeMin = 10f; t.fontSizeMax = 18f;
                        }
                    }
                }
            }

            // 상세 카드(SkillDetail)를 좌측 상세 창으로
            var det = p.Go.transform.Find("SkillDetail");
            if (det != null)
            {
                det.SetParent(side, false);
                // 상단 액자(0.70~0.88)를 피해 아래 종이 영역에만 놓는다
                PlaceIn(det, 0.14f, 0.27f, 0.86f, 0.63f);
                var di = det.GetComponent<Image>();
                if (di != null) di.enabled = false;      // 종이는 창 배경
                var dt = det.Find("DTitle");
                if (dt != null)
                {
                    PlaceIn(dt, 0.04f, 0.86f, 0.96f, 0.99f);
                    var t = dt.GetComponent<TMP_Text>();
                    if (t != null)
                    { t.alignment = TMPro.TextAlignmentOptions.Center; t.fontSize = 32f; t.color = ink; }
                }
                var db = det.Find("DBody");
                if (db != null)
                {
                    PlaceIn(db, 0.06f, 0.20f, 0.94f, 0.84f);
                    var t = db.GetComponent<TMP_Text>();
                    if (t != null)
                    { t.alignment = TMPro.TextAlignmentOptions.Top; t.fontSize = 24f; t.color = ink; }
                }
                var da = det.Find("DAct");
                if (da != null)
                {
                    da.SetParent(side, false);
                    PlaceIn(da, 0.20f, 0.150f, 0.80f, 0.215f);
                    // 킷 원본(노란 버튼)이 창 톤과 겉돈다 → 배경의 나무 판을 쓴다
                    var dai = da.GetComponent<Image>();
                    if (dai != null) dai.enabled = false;
                    foreach (var im9 in da.GetComponentsInChildren<Image>(true))
                        if (im9.transform != da) im9.enabled = false;
                    foreach (var t in da.GetComponentsInChildren<TMP_Text>(true))
                    {
                        var tr = t.rectTransform;
                        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                        tr.offsetMin = tr.offsetMax = Vector2.zero;
                        t.alignment = TMPro.TextAlignmentOptions.Center;
                        t.fontSize = 30f; t.color = cream;
                    }
                }
            }

            // 하단 두 판: 세력 · 잔여 재화 안내
            var lblL = main.Find("SkillInfoL") as RectTransform;
            if (lblL == null)
                lblL = UiKit.TmpLabel(main, "SkillInfoL", "", 26, cream, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lblL, 0.175f, 0.150f, 0.478f, 0.243f);
            var lblR = main.Find("SkillInfoR") as RectTransform;
            if (lblR == null)
                lblR = UiKit.TmpLabel(main, "SkillInfoR", "", 26, cream, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lblR, 0.522f, 0.150f, 0.828f, 0.243f);
            var w2 = WalletAdapter.Instance;
            var es2 = EquipmentService.Instance;
            var l1 = lblL.GetComponent<TMP_Text>();
            if (l1 != null)
            { l1.text = FactionService.DisplayName + " 무공"; l1.color = cream; l1.fontSize = 30f; }
            var l2 = lblR.GetComponent<TMP_Text>();
            if (l2 != null)
            {
                l2.text = "강화석 " + (es2 != null ? UiKit.Num(es2.EnhanceStones) : "0");
                l2.color = cream; l2.fontSize = 30f;
            }

            // 남은 프리팹 틀 정리
            foreach (var gname in new[] { "Group_Left", "Group_Right", "Group_Center", "Bottom_Menu" })
            {
                var gt = p.Find(gname);
                if (gt != null && gt.parent != side && gt.parent != main)
                    gt.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 장비/가방 창 배치 (참고작 무기창 구조): 좌=아이템 상세, 우=등급 그리드.
        /// 좌표는 win_equip_side(560×1008)·win_equip_main(1360×1008) 픽셀 실측값.
        /// </summary>
        static void LayoutEquipWindow(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var safe = p.Go.transform.Find("WuxSafe");
            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (safe == null || wux == null) return;
            PlaceIn(safe, 0f, 0f, 1f, 1f);

            var ink = new Color(0.26f, 0.16f, 0.08f);
            var deepRed = new Color(0.60f, 0.20f, 0.11f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            PlaceIn(wux, 0f, 0f, 1f, 1f);
            var wi = wux.GetComponent<Image>();
            if (wi != null) { wi.sprite = null; wi.color = new Color(0.04f, 0.03f, 0.02f, 0.88f); }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);

            int baseIdx = wux.GetSiblingIndex();
            var side = EnsureArt(p.Go.transform, "WuxSide", "win_kit_side", baseIdx + 1);
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_grid", baseIdx + 2);
            PlaceIn(side, 0.020f, 0.080f, 0.283f, 0.920f);
            PlaceIn(main, 0.300f, 0.049f, 0.985f, 0.951f);
            WuxUiFx.PlayOpen(side); WuxUiFx.PlayOpen(main);
            safe.SetSiblingIndex(baseIdx + 3);

            // ---- 좌: 아이템 상세 ----
            // 아이템 액자 — 프리팹에 없으면 만들어 둔다 (가방 프리팹엔 Image_Item이 없다)
            var icon = p.Get<Image>("Image_Item");
            if (icon == null)
            {
                var go = new GameObject("Image_Item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(side, false);
                icon = go.GetComponent<Image>();
                icon.raycastTarget = false;
            }
            icon.transform.SetParent(side, false);
            PlaceIn(icon.transform, 0.316f, 0.700f, 0.702f, 0.875f);
            icon.preserveAspect = true;
            if (icon.sprite == null)
            {
                var eqNow = WeaponSummonAdapter.Instance != null ? WeaponSummonAdapter.Instance.Equipped : null;
                var sp0 = eqNow != null ? GrowArt.IconWeaponId(eqNow.catalogId, eqNow.kind) : null;
                if (sp0 != null) { icon.sprite = sp0; icon.color = Color.white; }
                else icon.color = new Color(1f, 1f, 1f, 0f);
            }
            var nameT = p.Get<TMP_Text>("Text_ItemName");
            if (nameT != null)
            {
                nameT.rectTransform.SetParent(side, false);
                PlaceIn(nameT.rectTransform, 0.20f, 0.560f, 0.80f, 0.625f);
                nameT.alignment = TMPro.TextAlignmentOptions.Center;
                nameT.fontSize = 34f; nameT.color = ink; nameT.enableWordWrapping = false;
            }
            // 양피지 4행: 등급 / 설명 / 공격력 / 방어력
            float[] er = { 0.520f, 0.462f, 0.404f, 0.346f, 0.292f };
            var rankT = p.Get<TMP_Text>("Text_Rank");
            if (rankT != null)
            {
                rankT.rectTransform.SetParent(side, false);
                PlaceIn(rankT.rectTransform, 0.20f, er[1], 0.80f, er[0]);
                rankT.alignment = TMPro.TextAlignmentOptions.Center;
                rankT.fontSize = 28f; rankT.color = deepRed;
            }
            var infoT = p.Get<TMP_Text>("Text_Info");
            if (infoT != null)
            {
                infoT.rectTransform.SetParent(side, false);
                PlaceIn(infoT.rectTransform, 0.16f, er[2], 0.84f, er[1]);
                infoT.alignment = TMPro.TextAlignmentOptions.Center;
                infoT.fontSize = 25f; infoT.color = ink; infoT.enableWordWrapping = true;
            }
            var stats = p.Find("Stats");
            if (stats != null)
            {
                stats.SetParent(side, false);
                PlaceIn(stats, 0.17f, er[4], 0.79f, er[2]);
                var vg2 = stats.GetComponent<VerticalLayoutGroup>();
                if (vg2 != null) Object.DestroyImmediate(vg2);
                foreach (var csf3 in stats.GetComponentsInChildren<ContentSizeFitter>(true))
                    Object.DestroyImmediate(csf3);
                int si2 = 0;
                foreach (Transform row in stats)
                {
                    if (si2 >= 2) break;
                    PlaceIn(row, 0f, si2 == 0 ? 0.5f : 0f, 1f, si2 == 0 ? 1f : 0.5f);
                    var im4 = row.GetComponent<Image>();
                    if (im4 != null) im4.enabled = false;
                    var ic2 = row.Find("Icon");
                    if (ic2 != null) PlaceIn(ic2, 0.01f, 0.15f, 0.14f, 0.85f);
                    var nm2 = row.Find("Text_Name");
                    if (nm2 != null)
                    {
                        PlaceIn(nm2, 0.17f, 0.05f, 0.62f, 0.95f);
                        var t = nm2.GetComponent<TMP_Text>();
                        if (t != null) { t.alignment = TMPro.TextAlignmentOptions.MidlineLeft; t.fontSize = 26f; t.color = ink; }
                    }
                    var vl2 = row.Find("Text_Value");
                    if (vl2 != null)
                    {
                        PlaceIn(vl2, 0.60f, 0.05f, 0.99f, 0.95f);
                        var t = vl2.GetComponent<TMP_Text>();
                        if (t != null) { t.alignment = TMPro.TextAlignmentOptions.MidlineRight; t.fontSize = 30f; t.color = ink; }
                    }
                    si2++;
                }
            }
            // 좌 하단 두 판: 분해 / 장착(강화)
            var bSell = p.Find("Button_Sell");
            if (bSell != null)
            {
                bSell.SetParent(side, false);
                PlaceIn(bSell, 0.20f, 0.185f, 0.80f, 0.240f);
                var bi2 = bSell.GetComponent<Image>();
                if (bi2 != null) bi2.enabled = false;
            }
            var bSel = p.Find("Button_Select");
            if (bSel != null)
            {
                bSel.SetParent(side, false);
                PlaceIn(bSel, 0.20f, 0.112f, 0.80f, 0.168f);
                var bi3 = bSel.GetComponent<Image>();
                if (bi3 != null) bi3.enabled = false;
            }
            foreach (var t9 in side.GetComponentsInChildren<TMP_Text>(true))
            {
                bool onBtn = false;
                for (var a = t9.transform; a != null && a != side; a = a.parent)
                    if (a.name == "Button_Sell" || a.name == "Button_Select") { onBtn = true; break; }
                if (onBtn)
                {
                    var tr = t9.rectTransform;
                    tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                    tr.offsetMin = tr.offsetMax = Vector2.zero;
                    t9.alignment = TMPro.TextAlignmentOptions.Center;
                    t9.fontSize = 30f; t9.color = cream;
                }
            }

            // ---- 우: 제목 / 탭 / 그리드 ----
            var pn = p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.340f, 0.865f, 0.660f, 0.928f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 46f; pn.color = ink; pn.enableVertexGradient = false;
            }
            var tap = p.Find("Tap_Menu") ?? p.Find("Tab_Menu");
            if (tap != null)
            {
                tap.SetParent(main, false);
                PlaceIn(tap, 0.100f, 0.655f, 0.900f, 0.810f);
                var ti = tap.GetComponent<Image>();
                if (ti != null) ti.enabled = false;      // 탭 판은 배경에 그려져 있다
                foreach (var img6 in tap.GetComponentsInChildren<Image>(true))
                    if (img6.transform != tap && img6.name != "Icon") img6.enabled = false;
                foreach (var t10 in tap.GetComponentsInChildren<TMP_Text>(true))
                { t10.color = ink; t10.fontSize = 26f; }   // 한지 위 — 크림은 안 읽힌다
            }
            var sr = p.Go.GetComponentInChildren<ScrollRect>(true);
            if (sr != null)
            {
                var srt = sr.GetComponent<RectTransform>();
                srt.SetParent(main, false);
                PlaceIn(srt, 0.100f, 0.333f, 0.900f, 0.588f);
                if (srt.GetComponent<RectMask2D>() == null) srt.gameObject.AddComponent<RectMask2D>();
                sr.viewport = srt;
                var grid2 = sr.content != null ? sr.content.GetComponent<GridLayoutGroup>() : null;
                if (grid2 != null)
                {
                    // 콘텐츠가 뷰포트보다 좁고 오른쪽으로 밀려 그리드가 어긋났다 → 폭 맞춤
                    var crt = sr.content;
                    crt.anchorMin = new Vector2(0f, 1f);
                    crt.anchorMax = new Vector2(1f, 1f);
                    crt.pivot = new Vector2(0.5f, 1f);
                    crt.offsetMin = new Vector2(0f, crt.offsetMin.y);
                    crt.offsetMax = new Vector2(0f, crt.offsetMax.y);
                    crt.anchoredPosition = new Vector2(0f, 0f);
                    // rect는 이번 프레임에 아직 갱신되지 않는다 → 비율로 직접 계산
                    float rootW = p.Root != null ? p.Root.rect.width : 2560f;
                    float rootH = p.Root != null ? p.Root.rect.height : 1440f;
                    float gw = (0.900f - 0.100f) * (0.985f - 0.300f) * rootW;
                    float gh = (0.588f - 0.333f) * (0.951f - 0.049f) * rootH;
                    // 칸은 정사각형(눌린 아이콘은 싸구려로 보인다). 높이로 칸 크기를 정하고,
                    // 칸 수는 폭에서 역산해 칸이 판을 꽉 채우게 한다.
                    grid2.spacing = new Vector2(6f, 6f);
                    grid2.padding = new RectOffset(0, 0, 0, 0);
                    grid2.childAlignment = TextAnchor.UpperLeft;
                    float cell = (gh - 6f * 3f) / 4f;
                    int cols = Mathf.Max(4, Mathf.FloorToInt((gw + 6f) / (cell + 6f)));
                    grid2.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid2.constraintCount = cols;
                    grid2.cellSize = new Vector2(cell, cell);
                    sr.horizontal = false;
                    sr.normalizedPosition = new Vector2(0f, 1f);
                    // 슬롯 아트가 셀보다 크면 넘친다 → 셀에 맞춰 늘린다
                    foreach (Transform cellT in crt)
                    {
                        foreach (var im8 in cellT.GetComponentsInChildren<Image>(true))
                        {
                            var irt = im8.rectTransform;
                            if (im8.transform == cellT) continue;
                            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
                            irt.offsetMin = new Vector2(2f, 2f); irt.offsetMax = new Vector2(-2f, -2f);
                        }
                    }
                }
            }
            // 하단 두 판: 보유 수 / 정렬 안내
            var lblL = main.Find("EquipInfoL") as RectTransform;
            if (lblL == null)
                lblL = UiKit.TmpLabel(main, "EquipInfoL", "", 26, ink, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lblL, 0.100f, 0.110f, 0.465f, 0.265f);
            var lblR = main.Find("EquipInfoR") as RectTransform;
            if (lblR == null)
                lblR = UiKit.TmpLabel(main, "EquipInfoR", "", 26, ink, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(lblR, 0.535f, 0.110f, 0.900f, 0.265f);
            // 위젯을 다 꺼냈으니 원래 컨테이너(상세 카드·목록 틀)는 치운다 —
            // 안 그러면 검은 카드가 창을 덮는다
            foreach (var gname in new[] { "Group_Left", "Group_Right", "Bottom_Menu", "Glow" })
            {
                var gt = p.Find(gname);
                if (gt != null && gt.parent != side && gt.parent != main)
                    gt.gameObject.SetActive(false);
            }
            // 분해 버튼의 코인 장식은 좁은 판에서 글자를 덮는다
            foreach (var img7 in side.GetComponentsInChildren<Image>(true))
                if (img7.name == "Gold" || img7.name == "Icon_Gold" || img7.name == "Coin")
                    img7.gameObject.SetActive(false);

            var wa2 = WeaponSummonAdapter.Instance;
            int ownCnt = wa2 != null ? wa2.GetSortedOwned(0, false).Count : 0;
            var lt3 = lblL.GetComponent<TMP_Text>();
            if (lt3 != null) { lt3.text = "보유 병기 " + ownCnt; lt3.color = cream; lt3.fontSize = 30f; }
            var rt3 = lblR.GetComponent<TMP_Text>();
            if (rt3 != null)
            {
                rt3.text = wa2 != null && wa2.Equipped != null ? "장착 · " + wa2.Equipped.name : "장착 없음";
                rt3.color = cream; rt3.fontSize = 30f; rt3.enableWordWrapping = false;
            }
        }

        /// <summary>세력(Faction) → 캐릭터판 배경. hero=정파 / bowmaster=사파 / archmage=마도.</summary>
        static string SideArtForFaction()
        {
            var s = SectService.Current;
            string f = s != null ? s.Faction : null;
            if (f == "bowmaster") return "win_side_sa";
            if (f == "archmage") return "win_side_ma";
            return "win_side_jeong";
        }

        /// <summary>
        /// 전직(세력)별 배경 종이 — 인물 없이 풍경만 그려진 한 장. 그 위에 실제 캐릭터가 선다.
        /// </summary>
        static string SceneForJob()
        {
            string tree = JobProgress.TreeId;
            if (string.IsNullOrEmpty(tree))
            {
                var sec = SectService.Current;
                tree = sec != null ? sec.Faction : null;
            }
            if (tree == "bowmaster") return "WuxiaUi/char_scene_bowmaster";
            if (tree == "archmage") return "WuxiaUi/char_scene_archmage";
            return "WuxiaUi/char_scene_hero";
        }

        /// <summary>프리팹 스탯 행 이름 → 새 고급 아이콘.</summary>
        static string StatIconFor(string rowName)
        {
            switch (rowName)
            {
                case "Damage": return "WuxiaUi/icon_crit";
                case "Defense": return "WuxiaUi/icon_def";
                case "Attack": return "WuxiaUi/icon_atk";
                case "Health": return "WuxiaUi/icon_hp";
                case "MoveSpeed": return "WuxiaUi/icon_spd";
            }
            return null;
        }

        // ---- 창 조립 키트 -------------------------------------------------
        // 캐릭터창(win_paper)이 기준이 됐지만, 통짜 아트는 칸 위치가 그림에 박혀 있어
        // 코드 배치와 1px만 어긋나도 깨진다. 그래서 나머지 창은 '한지 한 장 + 먹선 칸'으로
        // 조립한다 — 칸을 코드가 그리므로 내용과 절대 어긋나지 않고 크기도 자유롭다.

        /// <summary>창 바탕 한지. 기존 EnsureArt와 같은 자리에 끼워 넣어 쓴다.</summary>
        static RectTransform EnsureWuxSheet(Transform parent, string name, int sibling)
        {
            var t = EnsureArt(parent, name, "kit_paper_sheet", sibling);
            var img = t.GetComponent<Image>();
            img.type = Image.Type.Sliced;   // 찢긴 가장자리를 유지한 채 늘어난다
            WuxUiFx.PlayOpen(t);            // 열 때마다 두루마리처럼 펴진다
            return t;
        }

        /// <summary>한지 위에 먹선 칸 하나. 비율 좌표는 부모 시트 기준.</summary>
        static RectTransform InkPanel(RectTransform sheet, string name,
            float x0, float y0, float x1, float y1)
        {
            var t = sheet.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                t = go.GetComponent<RectTransform>();
                t.SetParent(sheet, false);
            }
            var img = t.GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("WuxiaUi/kit_ink_panel");
            img.type = Image.Type.Sliced;
            img.fillCenter = false;   // 속은 비워야 한지 바탕이 그대로 보인다
            img.color = Color.white;
            img.raycastTarget = false;
            PlaceIn(t, x0, y0, x1, y1);
            t.SetAsFirstSibling();    // 내용 위젯보다 뒤에
            return t;
        }

        /// <summary>창 제목 — 먹선 현판 + 그 위에 글씨. 캐릭터창 상단과 같은 모양.</summary>
        static TMP_Text InkTitle(RectTransform sheet, string text,
            float x0 = 0.30f, float y0 = 0.905f, float x1 = 0.70f, float y1 = 0.985f)
        {
            var plate = sheet.Find("InkTitle") as RectTransform;
            if (plate == null)
            {
                var go = new GameObject("InkTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                plate = go.GetComponent<RectTransform>();
                plate.SetParent(sheet, false);
            }
            var pi = plate.GetComponent<Image>();
            pi.sprite = Resources.Load<Sprite>("WuxiaUi/kit_ink_cartouche");
            pi.type = Image.Type.Simple;
            pi.preserveAspect = true;
            pi.raycastTarget = false;
            PlaceIn(plate, x0, y0, x1, y1);
            plate.SetAsFirstSibling();

            var label = plate.Find("T") as RectTransform;
            TMP_Text tt;
            if (label == null)
            {
                tt = UiKit.TmpLabel(plate, "T", text, 40, new Color(0.32f, 0.16f, 0.06f),
                    bold: true, TMPro.TextAlignmentOptions.Center);
                PlaceIn(tt.rectTransform, 0.14f, 0.10f, 0.86f, 0.90f);
            }
            else
            {
                tt = label.GetComponent<TMP_Text>();
                tt.text = text;
            }
            tt.enableWordWrapping = false;
            return tt;
        }

        static RectTransform EnsureArt(Transform parent, string name, string sprite, int sibling)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
            }
            t.SetSiblingIndex(sibling);
            var img = t.GetComponent<Image>();
            var sp = Resources.Load<Sprite>("WuxiaUi/" + sprite);
            if (sp != null) { img.sprite = sp; img.type = Image.Type.Simple; img.preserveAspect = false; }
            img.color = Color.white;
            img.raycastTarget = false;
            return t;
        }

        /// <summary>
        /// 창 형태 배치 (유저 확정): 좌=세력별 캐릭터판(캐릭터+9행 정보), 우=두루마리 3장+양피지 3행.
        /// 좌표는 win_side_*(560×1008)·win_char_main(1360×1008) 픽셀 실측을 비율로 환산한 값.
        /// </summary>
        // 게시판 아트(board_base)의 안쪽 판 실측: x .235~.816 / y .212~.758 (이미지 기준)
        const float BoardX0 = -0.16f, BoardY0 = -0.12f, BoardX1 = 1.16f, BoardY1 = 1.12f;
        static float InX(float f)   // 판 안쪽 가로 비율(0~1) → 루트 비율
        {
            float bx0 = BoardX0 + 0.235f * (BoardX1 - BoardX0);
            float bx1 = BoardX0 + 0.816f * (BoardX1 - BoardX0);
            return bx0 + f * (bx1 - bx0);
        }
        static float InY(float f)
        {
            float by0 = BoardY0 + 0.212f * (BoardY1 - BoardY0);
            float by1 = BoardY0 + 0.758f * (BoardY1 - BoardY0);
            return by0 + f * (by1 - by0);
        }

        // note_paper 스프라이트에서 실제 종이가 차지하는 영역 (실측). 나머지는 드롭섀도라
        // 내용물을 쪽지 rect 전체에 깔면 종이 밖으로 삐져나온다.
        const float PapX0 = 0.020f, PapX1 = 0.935f, PapY0 = 0.045f, PapY1 = 0.960f;
        static float PX(float f) { return PapX0 + f * (PapX1 - PapX0); }
        static float PY(float f) { return PapY0 + f * (PapY1 - PapY0); }

        /// <summary>쪽지 안(종이 위) 비율로 배치한다.</summary>
        static void OnPaper(Transform t, float x0, float y0, float x1, float y1)
        {
            PlaceIn(t, PX(x0), PY(y0), PX(x1), PY(y1));
        }

        /// <summary>판 위에 못으로 박은 쪽지 하나. 종이는 9-slice라 크기가 달라도 색·질감이 같다.</summary>
        static RectTransform Note(Transform parent, string name, float fx0, float fy0, float fx1, float fy1)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
                var im = go.GetComponent<Image>();
                im.sprite = Resources.Load<Sprite>("WuxiaUi/note_paper");
                im.type = Image.Type.Sliced;
                im.raycastTarget = false;
                var ng = new GameObject("Nail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ng.transform.SetParent(t, false);
                var ni = ng.GetComponent<Image>();
                ni.sprite = Resources.Load<Sprite>("WuxiaUi/nail_iron");
                ni.preserveAspect = true;
                ni.raycastTarget = false;
            }
            PlaceIn(t, InX(fx0), InY(fy0), InX(fx1), InY(fy1));
            var nail = t.Find("Nail") as RectTransform;
            if (nail != null)
            {
                // 못은 종이 폭에 비례하지 않고 항상 같은 크기 — 세 쪽지가 따로 놀지 않게
                float wPx = (InX(fx1) - InX(fx0)) * 2562f;
                float hPx = (InY(fy1) - InY(fy0)) * 1440f;
                float d = 58f;
                nail.anchorMin = nail.anchorMax = new Vector2((PapX0 + PapX1) * 0.5f, PapY1);
                nail.pivot = new Vector2(0.5f, 0.5f);
                nail.sizeDelta = new Vector2(d, d);
                nail.anchoredPosition = Vector2.zero;
                nail.SetSiblingIndex(t.childCount - 1);
            }
            return t;
        }

        /// <summary>쪽지 위에 그은 먹선 한 줄 (행 경계와 정확히 일치시킨다).</summary>
        static void Rule(Transform parent, string name, float x0, float y, float x1)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
                var im = go.GetComponent<Image>();
                im.color = new Color(0.45f, 0.33f, 0.20f, 0.32f);
                im.raycastTarget = false;
            }
            PlaceIn(t, x0, y, x1, y + 0.0022f);
        }

        static void LayoutCharWindow(CasualPanel p, Transform safe)
        {
            var ink = new Color(0.26f, 0.16f, 0.08f);
            var inkDim = new Color(0.42f, 0.29f, 0.17f);
            var deepRed = new Color(0.60f, 0.20f, 0.11f);

            var wux = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (wux != null)
            {
                PlaceIn(wux, 0f, 0f, 1f, 1f);
                var wi = wux.GetComponent<Image>();
                if (wi != null) { wi.sprite = null; wi.color = new Color(0.05f, 0.04f, 0.03f, 0.5f); }
            }
            var dimOld = p.Go.transform.Find("WuxDim");
            if (dimOld != null) dimOld.gameObject.SetActive(false);
            p.Go.transform.SetAsLastSibling();
            var mask = p.Go.GetComponent<HudMask>();
            if (mask == null)
            {
                var keep = new HashSet<string> { "Field", "Modals", "Toast", p.Go.name };
                var found = new List<GameObject>();
                var canvasT = p.Go.transform.parent;
                if (canvasT != null)
                    foreach (Transform t9 in canvasT)
                        if (!keep.Contains(t9.name) && t9.gameObject.activeSelf)
                            found.Add(t9.gameObject);
                mask = p.Go.AddComponent<HudMask>();
                mask.Targets = found.ToArray();
                foreach (var gm in found) gm.SetActive(false);
            }
            foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(true))
                ps.gameObject.SetActive(false);

            int baseIdx = wux != null ? wux.GetSiblingIndex() : 0;
            // 창 전체가 한 장의 종이 — 먹선 칸까지 그림에 들어 있다 (실측값이 아래 좌표)
            var main = EnsureArt(p.Go.transform, "WuxSide", "win_paper", baseIdx + 1);
            PlaceIn(main, 0f, 0f, 1f, 1f);
            var oldMain = p.Go.transform.Find("WuxMain");
            if (oldMain != null && oldMain != main) oldMain.gameObject.SetActive(false);
            safe.SetSiblingIndex(baseIdx + 2);
            var host = p.Go.transform;
            foreach (var gone in new[] { "NoteChar", "NoteStat", "NoteS1", "NoteS2", "NoteS3",
                                         "NoteSpec", "CharSheet", "Silhouette" })
            {
                var st = host.Find(gone);
                if (st != null) st.gameObject.SetActive(false);
            }

            var g = PlayerGrowth.Instance;

            // ---- 좌 칸: 이름 · 전투력 · 세력 풍경 · 캐릭터 (칸 실측 x .075~.275 / y .095~.830) ----
            var nameT = main.Find("NameLine") as RectTransform;
            if (nameT == null)
                nameT = UiKit.TmpLabel(main, "NameLine", "", 30, ink, bold: true,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(nameT, 0.078f, 0.762f, 0.272f, 0.826f);
            var nameTx = nameT.GetComponent<TMP_Text>();
            if (nameTx != null)
            {
                string job = JobProgress.Current != null ? JobProgress.Current.name : "무사";
                nameTx.text = job + "\n<size=54%><color=#6B4A2A>" + FactionService.DisplayName
                    + " · 내공 " + (g != null ? g.Grade : 0) + "단계</color></size>";
                nameTx.color = ink; nameTx.fontSize = 36f;
                nameTx.alignment = TMPro.TextAlignmentOptions.Center;
                nameTx.enableWordWrapping = false; nameTx.lineSpacing = 0f;
            }
            var infoT = p.Find("Text_Info");
            if (infoT != null)
            {
                infoT.SetParent(main, false);
                PlaceIn(infoT, 0.078f, 0.700f, 0.272f, 0.758f);
                var it = infoT.GetComponent<TMP_Text>();
                if (it != null)
                {
                    it.text = "<size=50%><color=#4A2A14>전투력</color></size>  <b>"
                        + UiKit.Num(CombatPowerService.GetTotalCp()) + "</b>";
                    it.alignment = TMPro.TextAlignmentOptions.Center;
                    it.fontSize = 42f; it.color = deepRed; it.enableWordWrapping = false;
                    it.lineSpacing = 0f;
                }
            }
            var scene = main.Find("Scene") as RectTransform;
            if (scene == null)
            {
                var sg = new GameObject("Scene", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                scene = sg.GetComponent<RectTransform>();
                scene.SetParent(main, false);
                var si = sg.GetComponent<Image>();
                si.preserveAspect = true;
                si.raycastTarget = false;
            }
            scene.SetSiblingIndex(0);
            PlaceIn(scene, 0.078f, 0.200f, 0.272f, 0.628f);
            var sceneImg = scene.GetComponent<Image>();
            if (sceneImg != null)
            {
                sceneImg.sprite = Resources.Load<Sprite>(SceneForJob());
                sceneImg.color = new Color(1f, 1f, 1f, 0.38f);
            }
            var gcT = p.Find("Group_Center");
            if (gcT != null)
            {
                gcT.gameObject.SetActive(true);
                gcT.SetParent(main, false);
                PlaceIn(gcT, 0.062f, 0.098f, 0.288f, 0.636f);
                if (gcT.GetComponent<RectMask2D>() == null)
                    gcT.gameObject.AddComponent<RectMask2D>();
            }
            if (_charPrev != null && _charPrev.Rect != null)
            {
                var prt = _charPrev.Rect;
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
                prt.pivot = new Vector2(0.5f, 0f);
                float boxH = (0.636f - 0.098f) * 1440f;
                float bodyH = boxH * 0.78f;
                prt.sizeDelta = new Vector2(bodyH * 300f / 380f, bodyH);
                prt.anchoredPosition = new Vector2(0f, boxH * 0.06f);
                var pim = prt.GetComponent<Image>();
                if (pim != null) pim.preserveAspect = true;
            }

            // ---- 중앙 칸: 잔여 포인트 띠 + 먹선 5줄에 스탯 5개 (칸 실측 x .318~.608) ----
            var ptRow = main.Find("PtRow") as RectTransform;
            if (ptRow == null)
                ptRow = UiKit.TmpLabel(main, "PtRow", "", 26, inkDim, bold: false,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(ptRow, 0.078f, 0.630f, 0.272f, 0.696f);
            var ptT = ptRow.GetComponent<TMP_Text>();
            if (ptT != null)
            {
                int perLv = IdleMvp.Core.BalanceConfig.Data.statPointsPerLevel;
                int perGrade = IdleMvp.Core.BalanceConfig.Data.pointsPerGrade;
                int specPer = IdleMvp.Core.BalanceConfig.Data.specialStatPerGrade;
                ptT.text = "잔여 <color=#9C3418><b>" + (g != null ? g.StatPoints : 0)
                    + "</b></color>   ·   특별 <color=#9C3418><b>"
                    + (g != null ? g.SpecialStatPoints : 0) + "</b></color>"
                    + "\n<size=76%><color=#8A6B49>레벨당 +" + perLv + " · " + perGrade
                    + "회 투자마다 내공 +1 (특별 +" + specPer + ")</color></size>";
                ptT.lineSpacing = -8f;
                ptT.color = inkDim; ptT.fontSize = 26f;
                ptT.alignment = TMPro.TextAlignmentOptions.Center;
                ptT.enableWordWrapping = false;
            }
            float[] ln = { 0.798f, 0.727f, 0.637f, 0.548f, 0.460f, 0.367f };
            var grT = p.Find("Group_Right");
            if (grT != null)
            {
                grT.SetParent(main, false);
                PlaceIn(grT, 0f, 0f, 1f, 1f);
                var head2 = grT.Find("Text_Stats");
                if (head2 != null) head2.gameObject.SetActive(false);
                var ab = grT.Find("Ability");
                if (ab != null)
                {
                    PlaceIn(ab, 0f, 0f, 1f, 1f);
                    var vg = ab.GetComponent<VerticalLayoutGroup>();
                    if (vg != null) Object.DestroyImmediate(vg);
                    foreach (var csf in ab.GetComponentsInChildren<ContentSizeFitter>(true))
                        Object.DestroyImmediate(csf);
                    int i = 0;
                    foreach (Transform row in ab)
                    {
                        if (i >= 5) break;
                        PlaceIn(row, 0.330f, ln[i + 1], 0.596f, ln[i]);
                        var im = row.GetComponent<Image>();
                        if (im != null) im.enabled = false;
                        var ico = row.Find("Icon");
                        if (ico != null)
                        {
                            PlaceIn(ico, 0f, 0.10f, 0.135f, 0.90f);
                            var icoImg = ico.GetComponent<Image>();
                            var newIco = Resources.Load<Sprite>(StatIconFor(row.name));
                            if (icoImg != null && newIco != null)
                            {
                                icoImg.sprite = newIco; icoImg.color = Color.white;
                                icoImg.preserveAspect = true;
                            }
                        }
                        var lab = row.Find("Text_Stats");
                        if (lab != null)
                        {
                            PlaceIn(lab, 0.165f, 0.08f, 0.60f, 0.92f);
                            var lt = lab.GetComponent<TMP_Text>();
                            if (lt != null)
                            {
                                lt.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                                lt.fontSize = 28f; lt.color = inkDim; lt.characterSpacing = 2f;
                            }
                        }
                        var val = row.Find("Text_Value");
                        if (val != null)
                        {
                            PlaceIn(val, 0.58f, 0.08f, 1f, 0.92f);
                            var vt = val.GetComponent<TMP_Text>();
                            if (vt != null)
                            {
                                vt.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                                vt.fontSize = 32f; vt.color = ink;
                                vt.fontStyle |= TMPro.FontStyles.Bold;
                            }
                        }
                        i++;
                    }
                }
            }

            var sbG2 = p.Find("StatusBar_Group");
            if (sbG2 != null) sbG2.gameObject.SetActive(false);
            foreach (var t0 in p.Go.GetComponentsInChildren<TMP_Text>(true))
                if (t0.name == "Text_Stats" && (t0.text ?? "").Contains("내공"))
                { t0.gameObject.SetActive(false); break; }
            foreach (var im0 in p.Go.GetComponentsInChildren<Image>(true))
                if (im0.name == "Line" || im0.name == "Divider") im0.enabled = false;
            var nameG = p.Find("Name_Group");
            if (nameG != null) nameG.gameObject.SetActive(false);
            var lvInfo = p.Find("Level_Info");
            if (lvInfo != null) lvInfo.gameObject.SetActive(false);
            var runeOld = p.Find("Rune");
            if (runeOld != null) runeOld.gameObject.SetActive(false);
            var runeLbl = p.Find("Text_Rune");
            if (runeLbl != null) runeLbl.gameObject.SetActive(false);

            // ---- 제목: 그림 속 카투슈 ----
            var pn = p.Get<TMP_Text>("Text_PanelName");
            if (pn != null)
            {
                pn.rectTransform.SetParent(main, false);
                PlaceIn(pn.rectTransform, 0.395f, 0.858f, 0.620f, 0.932f);
                pn.alignment = TMPro.TextAlignmentOptions.Center;
                pn.fontSize = 44f; pn.color = new Color(0.32f, 0.16f, 0.06f);
                pn.enableVertexGradient = false; pn.enableWordWrapping = false;
                pn.text = "능력치";
            }

            // ---- 우 칸 3개: 능력치 강화 (실측 x .636~.713 / .741~.817 / .846~.924, y .365~.830) ----
            var bm = p.Find("BottomMenu");
            var upBtn = bm != null ? bm.Find("Button_Upgrade") : null;
            float[,] sx = { { 0.636f, 0.713f }, { 0.741f, 0.817f }, { 0.846f, 0.924f } };
            string[] keys = { "ATK", "DEF", "HP" };
            string[] nm = { "주 스탯", "방어력", "최대 HP" };
            for (int c = 0; c < 3; c++)
            {
                string cn = "Card_" + keys[c];
                var card = main.Find(cn) as RectTransform;
                if (card == null)
                {
                    var go = new GameObject(cn, typeof(RectTransform), typeof(CanvasRenderer),
                        typeof(Image), typeof(Button));
                    card = go.GetComponent<RectTransform>();
                    card.SetParent(main, false);
                    go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                    UiKit.TmpLabel(card, "Title", "", 26, inkDim, bold: true, TMPro.TextAlignmentOptions.Center);
                    UiKit.TmpLabel(card, "Val", "", 34, deepRed, bold: true, TMPro.TextAlignmentOptions.Center);
                    MakeChip(card, "Chip", "강화");
                    HoverOutline(go, card);
                }
                PlaceIn(card, sx[c, 0], 0.365f, sx[c, 1], 0.830f);
                var tt = card.Find("Title") as RectTransform;
                if (tt != null) PlaceIn(tt, 0.02f, 0.740f, 0.98f, 0.880f);
                var vv = card.Find("Val") as RectTransform;
                if (vv != null) PlaceIn(vv, 0.02f, 0.520f, 0.98f, 0.700f);
                var ch = card.Find("Chip") as RectTransform;
                if (ch != null) PlaceIn(ch, 0.10f, 0.260f, 0.90f, 0.420f);
                int cur = g == null ? 0 : (c == 0 ? g.Atk : c == 1 ? g.Def : g.Hp);
                var t1 = tt != null ? tt.GetComponent<TMP_Text>() : null;
                if (t1 != null)
                {
                    t1.text = nm[c]; t1.color = inkDim; t1.enableWordWrapping = false;
                    t1.enableAutoSizing = true; t1.fontSizeMin = 16f; t1.fontSizeMax = 27f;
                }
                var t2 = vv != null ? vv.GetComponent<TMP_Text>() : null;
                if (t2 != null)
                {
                    t2.text = "+" + cur; t2.color = deepRed; t2.enableWordWrapping = false;
                    t2.enableAutoSizing = true; t2.fontSizeMin = 20f; t2.fontSizeMax = 38f;
                }
                bool canUp = g != null && g.StatPoints > 0;
                SetChip(ch, canUp ? "강화" : "부족", canUp);
                var cb = card.GetComponent<Button>();
                if (cb != null)
                {
                    string key = keys[c];
                    cb.transition = Selectable.Transition.None;
                    cb.interactable = canUp;
                    cb.onClick.RemoveAllListeners();
                    cb.onClick.AddListener(AudioService.Click);
                    cb.onClick.AddListener(() =>
                    {
                        var g3 = PlayerGrowth.Instance;
                        if (g3 == null) return;
                        bool ok = g3.TrySpendStatPoint(key);
                        Toast(ok ? "강화 완료" : "스탯 포인트 부족");
                        if (ok) CasualFx.EnhanceFlash(_host);
                        BuildPlayerChar(); _refresh?.Invoke();
                    });
                }
            }
            if (upBtn != null) upBtn.gameObject.SetActive(false);

            // ---- 아래 넓은 칸: 특별 능력치 (레벨로 계속 해금되는 스크롤 목록) ----
            var sr = main.Find("SpecScroll") as RectTransform;
            RectTransform content;
            if (sr == null)
            {
                var sg = new GameObject("SpecScroll", typeof(RectTransform), typeof(CanvasRenderer),
                    typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
                sr = sg.GetComponent<RectTransform>();
                sr.SetParent(main, false);
                sg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                var cg = new GameObject("Content", typeof(RectTransform));
                content = cg.GetComponent<RectTransform>();
                content.SetParent(sr, false);
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
                var vg2 = cg.AddComponent<VerticalLayoutGroup>();
                vg2.spacing = 6f;
                vg2.childControlHeight = true; vg2.childControlWidth = true;
                vg2.childForceExpandHeight = false; vg2.childForceExpandWidth = true;
                var csf2 = cg.AddComponent<ContentSizeFitter>();
                csf2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var scroll = sg.GetComponent<ScrollRect>();
                scroll.content = content;
                scroll.viewport = sr;
                scroll.horizontal = false; scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Elastic;
                scroll.scrollSensitivity = 28f;
            }
            content = sr.Find("Content") as RectTransform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = new Vector2(0f, content.offsetMin.y);
            content.offsetMax = new Vector2(0f, content.offsetMax.y);
            content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
            PlaceIn(sr, 0.330f, 0.100f, 0.920f, 0.322f);

            var catalog = PlayerGrowth.SpecCatalog;
            for (int c = 0; c < catalog.Length; c++)
            {
                var d = catalog[c];
                var row = content.Find("Spec_" + d.Id) as RectTransform;
                if (row == null)
                {
                    var go = new GameObject("Spec_" + d.Id, typeof(RectTransform), typeof(CanvasRenderer),
                        typeof(Image), typeof(Button), typeof(LayoutElement));
                    row = go.GetComponent<RectTransform>();
                    row.SetParent(content, false);
                    go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                    UiKit.TmpLabel(row, "L", "", 26, inkDim, bold: false, TMPro.TextAlignmentOptions.MidlineLeft);
                    UiKit.TmpLabel(row, "V", "", 24, ink, bold: false, TMPro.TextAlignmentOptions.MidlineRight);
                    MakeChip(row, "Chip", "강화");
                    HoverOutline(go, row);
                }
                var le = row.GetComponent<LayoutElement>();
                le.preferredHeight = 64f; le.minHeight = 64f;
                var lr = row.Find("L") as RectTransform;
                if (lr != null) PlaceIn(lr, 0.005f, 0.05f, 0.46f, 0.95f);
                var vr = row.Find("V") as RectTransform;
                if (vr != null) PlaceIn(vr, 0.46f, 0.05f, 0.80f, 0.95f);
                var ch2 = row.Find("Chip") as RectTransform;
                if (ch2 != null) PlaceIn(ch2, 0.815f, 0.12f, 0.995f, 0.88f);

                int lv = g != null ? g.SpecLevel(d.Id) : 0;
                int cap = g != null ? g.SpecMax(d.Id) : d.MaxLevel;
                bool unlocked = g != null && g.SpecUnlocked(d.Id);
                bool maxed = g != null && g.SpecMaxed(d.Id);
                bool canBuy = unlocked && !maxed && g != null && g.SpecialStatPoints > 0;

                var lt3 = lr != null ? lr.GetComponent<TMP_Text>() : null;
                if (lt3 != null)
                {
                    lt3.text = unlocked
                        ? d.Name + "  <size=80%><color=#8A6B49>" + lv + "/" + cap + "</color></size>"
                        : "<color=#9A8B7A>" + d.Name + "</color>";
                    lt3.color = unlocked ? inkDim : new Color(0.62f, 0.55f, 0.47f);
                    lt3.fontSize = 27f; lt3.enableWordWrapping = false;
                }
                var vt3 = vr != null ? vr.GetComponent<TMP_Text>() : null;
                if (vt3 != null)
                {
                    float cur = g != null ? g.SpecValue(d.Id) : 0f;
                    if (!unlocked)
                        vt3.text = "<color=#9A8B7A>" + d.UnlockLevel + " 레벨 달성 필요</color>";
                    else if (maxed)
                    {
                        int nextCap = g != null ? g.SpecNextCapLevel(d.Id) : 0;
                        vt3.text = "<color=#9C3418><b>+" + cur.ToString("0.#") + d.Unit + "</b></color>"
                            + (nextCap > 0
                                ? "<size=82%><color=#8A6B49>  " + nextCap + " Lv 해금</color></size>"
                                : "");
                    }
                    else
                        vt3.text = "<color=#9C3418><b>+" + cur.ToString("0.#") + d.Unit + "</b></color>"
                            + "<size=85%><color=#8A6B49>  ▶ +" + (cur + d.PerLevel).ToString("0.#") + d.Unit + "</color></size>";
                    vt3.fontSize = 25f; vt3.enableWordWrapping = false;
                    vt3.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                }
                SetChip(ch2, !unlocked ? "잠김" : (maxed ? "최대" : "강화"), canBuy);

                var sb2 = row.GetComponent<Button>();
                if (sb2 != null)
                {
                    string key = d.Id;
                    sb2.transition = Selectable.Transition.None;
                    sb2.interactable = canBuy;
                    sb2.onClick.RemoveAllListeners();
                    sb2.onClick.AddListener(AudioService.Click);
                    sb2.onClick.AddListener(() =>
                    {
                        var g4 = PlayerGrowth.Instance;
                        if (g4 == null) return;
                        bool ok = g4.TryUpgradeSpecial(key);
                        Toast(ok ? "특별 능력치 강화" : "특별 포인트 부족");
                        if (ok) CasualFx.EnhanceFlash(_host);
                        BuildPlayerChar(); _refresh?.Invoke();
                    });
                }
            }
            // 구버전 고정 3줄 제거
            foreach (var gone2 in new[] { "SpecRow_DMG", "SpecRow_GOLD", "SpecRow_IDLE" })
            {
                var st2 = main.Find(gone2);
                if (st2 != null) st2.gameObject.SetActive(false);
            }

            var selBtn = bm != null ? bm.Find("Button_Selet") : null;
            if (selBtn != null) selBtn.gameObject.SetActive(false);
            if (bm != null) PlaceIn(bm, 0f, 0f, 0.001f, 0.001f);
            foreach (var rt9 in p.Go.GetComponentsInChildren<RectTransform>(true))
                if (rt9.name == "GroundRing") rt9.gameObject.SetActive(false);

            var closeOld = p.Go.transform.Find("WuxClose");
            if (closeOld != null) closeOld.gameObject.SetActive(false);
            var backT = p.Find("Button_Back");
            if (backT != null)
            {
                backT.gameObject.SetActive(true);
                backT.SetParent(main, false);
                PlaceIn(backT, 0.048f, 0.862f, 0.108f, 0.958f);
                var plate = backT.GetComponent<Image>();
                if (plate != null) plate.color = new Color(1f, 1f, 1f, 0f);
                var arrow = FindDeep(backT, "Image_Arrow");
                if (arrow != null)
                {
                    PlaceIn(arrow, 0.12f, 0.12f, 0.88f, 0.88f);
                    var ai = arrow.GetComponent<Image>();
                    if (ai != null)
                    {
                        ai.enabled = true; ai.preserveAspect = true;
                        ai.color = new Color(0.32f, 0.18f, 0.08f);
                    }
                }
                foreach (var t8 in backT.GetComponentsInChildren<TMP_Text>(true))
                    t8.gameObject.SetActive(false);
                HoverOutline(backT.gameObject, backT as RectTransform);
                var bb2 = backT.GetComponent<Button>();
                if (bb2 != null)
                {
                    bb2.transition = Selectable.Transition.None;
                    bb2.onClick.RemoveAllListeners();
                    bb2.onClick.AddListener(AudioService.Click);
                    bb2.onClick.AddListener(() => { p.Hide(); _refresh?.Invoke(); });
                }
            }
        }

        /// <summary>종이 위에 얹는 작은 버튼 칩 — 옅은 나무판 + 먹 글씨(요란한 판은 유저가 기각).</summary>
        static void MakeChip(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var im = go.GetComponent<Image>();
            im.sprite = Resources.Load<Sprite>("WuxiaUi/kit_btn_upgrade");
            im.type = Image.Type.Sliced;
            im.raycastTarget = false;
            UiKit.TmpLabel(go.transform, "T", label, 22, new Color(0.30f, 0.19f, 0.10f), bold: true,
                TMPro.TextAlignmentOptions.Center);
        }

        static void SetChip(RectTransform chip, string label, bool on)
        {
            if (chip == null) return;
            var t = chip.Find("T") as RectTransform;
            if (t != null)
            {
                PlaceIn(t, 0.06f, 0.02f, 0.94f, 0.98f);
                var tt = t.GetComponent<TMP_Text>();
                if (tt != null)
                {
                    tt.text = label; tt.fontSize = 22f; tt.enableWordWrapping = false;
                    tt.color = on ? new Color(1f, 0.98f, 0.92f) : new Color(0.92f, 0.92f, 0.92f);
                    tt.fontStyle |= TMPro.FontStyles.Bold;
                }
            }
            var im = chip.GetComponent<Image>();
            if (im != null)
            {
                im.sprite = Resources.Load<Sprite>(on ? "WuxiaUi/kit_btn_upgrade" : "WuxiaUi/kit_btn_off");
                im.color = Color.white;
            }
        }

        /// <summary>마우스를 올리면 금색 테두리 — 9-slice 프레임은 이 크기에서 통판이 되어 선 4개로 그린다.</summary>
        static void HoverOutline(GameObject target, RectTransform area)
        {
            if (area == null || area.Find("Ring") != null) return;
            var ring = new GameObject("Ring", typeof(RectTransform));
            var rrt = ring.GetComponent<RectTransform>();
            rrt.SetParent(area, false);
            rrt.anchorMin = new Vector2(-0.04f, -0.02f);
            rrt.anchorMax = new Vector2(1.04f, 1.02f);
            rrt.offsetMin = rrt.offsetMax = Vector2.zero;
            var gold = new Color(0.78f, 0.56f, 0.20f);
            float[,] edge = { { 0f, 0f, 1f, 0.012f }, { 0f, 0.988f, 1f, 1f },
                              { 0f, 0f, 0.006f, 1f }, { 0.994f, 0f, 1f, 1f } };
            for (int e = 0; e < 4; e++)
            {
                var l = new GameObject("L" + e, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                l.transform.SetParent(rrt, false);
                var li = l.GetComponent<Image>();
                li.color = gold; li.raycastTarget = false;
                PlaceIn(l.transform, edge[e, 0], edge[e, 1], edge[e, 2], edge[e, 3]);
            }
            ring.SetActive(false);
            var et = target.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (et == null) et = target.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var enter = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => ring.SetActive(true));
            var exit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => ring.SetActive(false));
            et.triggers.Add(enter); et.triggers.Add(exit);
        }

        /// <summary>
        /// 메이플키우기 캐릭터창 구조를 그대로 옮긴 배치 (구버전, 전체화면판).
        /// 좌: 이름·전투력·캐릭터(발판 위)·5행 스탯판 / 우: 등급바·3강화카드·비급 6칸·버튼.
        /// 좌표는 bg_char 픽셀 실측(1600×896)을 루트 비율로 환산한 값.
        /// </summary>
        static void LayoutCharBenchmark(CasualPanel p, Transform safe)
        {
            var ink = new Color(0.24f, 0.15f, 0.08f);
            var sky = new Color(1f, 1f, 1f);
            var deepRed = new Color(0.62f, 0.22f, 0.12f);

            // 위젯을 안전영역 직속으로 올려 루트 비율로 직접 배치한다
            Transform Grab(string n)
            {
                var t = p.Find(n);
                if (t != null && t.parent != safe) t.SetParent(safe, false);
                return t;
            }
            var nameG = Grab("Name_Group");
            var infoT = Grab("Text_Info");
            var lvInfo = Grab("Level_Info");
            var rune = Grab("Rune");
            var bm = Grab("BottomMenu");
            var gcT = Grab("Group_Center");
            var grT = Grab("Group_Right");
            var subT = p.Find("Text_Stats");
            if (subT != null && subT.parent != safe && subT.parent != null
                && subT.parent.name == "Group_Left") subT.SetParent(safe, false);

            // ---- 좌: 이름 / 전투력 / 캐릭터 / 스탯판 ----
            if (nameG != null) PlaceIn(nameG, 0.075f, 0.875f, 0.465f, 0.960f);
            if (subT != null) PlaceIn(subT, 0.075f, 0.820f, 0.465f, 0.875f);
            if (infoT != null) PlaceIn(infoT, 0.075f, 0.700f, 0.465f, 0.810f);
            if (gcT != null) PlaceIn(gcT, 0.060f, 0.554f, 0.465f, 0.700f);
            if (_charPrev != null && _charPrev.Rect != null)
            {
                var prt = _charPrev.Rect;
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
                prt.pivot = new Vector2(0.5f, 0f);
                prt.sizeDelta = new Vector2(340f, 430f);
                prt.anchoredPosition = new Vector2(0f, 6f);   // 발판 위에 정확히
            }
            if (grT != null)
            {
                // 스탯판: 배경 실측 밴드 5개 (경계 y=522/591/662/732, 판 480~815)
                PlaceIn(grT, 0.060f, 0.090f, 0.465f, 0.464f);
                float[] band = { 1f, 0.875f, 0.669f, 0.457f, 0.248f, 0f };
                var head2 = grT.Find("Text_Stats");
                if (head2 != null) head2.gameObject.SetActive(false);   // 판에 제목 칸이 없다
                var ab = grT.Find("Ability");
                if (ab != null)
                {
                    PlaceIn(ab, 0f, 0f, 1f, 1f);
                    var vg = ab.GetComponent<VerticalLayoutGroup>();
                    if (vg != null) Object.DestroyImmediate(vg);
                    foreach (var csf in ab.GetComponentsInChildren<ContentSizeFitter>(true))
                        Object.DestroyImmediate(csf);
                    int i = 0;
                    foreach (Transform row in ab)
                    {
                        if (i >= 5) break;
                        PlaceIn(row, 0.05f, band[i + 1], 0.95f, band[i]);
                        var im = row.GetComponent<Image>();
                        if (im != null) im.enabled = false;         // 줄은 배경에 그려져 있다
                        // 행 내부: 아이콘 왼쪽 · 이름 · 값 오른쪽 (겹침 해소)
                        var ico = row.Find("Icon");
                        if (ico != null) PlaceIn(ico, 0.02f, 0.15f, 0.13f, 0.85f);
                        var lab = row.Find("Text_Stats");
                        if (lab != null)
                        {
                            PlaceIn(lab, 0.16f, 0.1f, 0.60f, 0.9f);
                            var lt2 = lab.GetComponent<TMP_Text>();
                            if (lt2 != null)
                            { lt2.alignment = TMPro.TextAlignmentOptions.MidlineLeft; lt2.fontSize = 30f; }
                        }
                        var val = row.Find("Text_Value");
                        if (val != null)
                        {
                            PlaceIn(val, 0.60f, 0.1f, 0.97f, 0.9f);
                            var vt2 = val.GetComponent<TMP_Text>();
                            if (vt2 != null)
                            { vt2.alignment = TMPro.TextAlignmentOptions.MidlineRight; vt2.fontSize = 38f; }
                        }
                        i++;
                    }
                }
            }

            // ---- 우: 등급바 / 3강화카드 / 비급 6칸 / 버튼 ----
            if (lvInfo != null) PlaceIn(lvInfo, 0.540f, 0.848f, 0.920f, 0.902f);
            if (rune != null)
            {
                PlaceIn(rune, 0.545f, 0.175f, 0.920f, 0.505f);
                // 배경의 3열×2행 칸을 채우도록 셀 크기를 늘린다 (기본 134px는 너무 작다)
                var grid = rune.GetComponent<GridLayoutGroup>();
                var rrt = rune as RectTransform;
                if (grid != null && rrt != null)
                {
                    float cw2 = (0.920f - 0.545f) * 2560f, chh = (0.505f - 0.175f) * 1440f;
                    grid.spacing = new Vector2(18f, 14f);
                    grid.cellSize = new Vector2((cw2 - grid.spacing.x * 2f) / 3f - 6f,
                        (chh - grid.spacing.y) / 2f - 6f);
                    grid.childAlignment = TextAnchor.MiddleCenter;
                }
                foreach (var im3 in rune.GetComponentsInChildren<Image>(true))
                    if (im3.transform.parent == rune) im3.preserveAspect = true;
            }
            var runeLbl = p.Find("Text_Rune");
            if (runeLbl != null) runeLbl.gameObject.SetActive(false);

            // 강화 카드 3장 (공격/방어/최대HP) — Button_Upgrade를 복제해 만든다
            var upBtn = bm != null ? bm.Find("Button_Upgrade") : null;
            if (upBtn != null)
            {
                float[,] cardX = { { 0.536f, 0.649f }, { 0.673f, 0.786f }, { 0.811f, 0.923f } };
                string[] keys = { "ATK", "DEF", "HP" };
                string[] names = { "공격력", "방어력", "최대 HP" };
                var g2 = PlayerGrowth.Instance;
                for (int c = 0; c < 3; c++)
                {
                    string cn = "Card_" + keys[c];
                    var card = safe.Find(cn);
                    if (card == null)
                    {
                        var clone = Object.Instantiate(upBtn.gameObject, safe);
                        clone.name = cn;
                        card = clone.transform;
                    }
                    PlaceIn(card, cardX[c, 0], 0.528f, cardX[c, 1], 0.802f);
                    foreach (var im2 in card.GetComponentsInChildren<Image>(true))
                        if (im2.gameObject != card.gameObject) im2.gameObject.SetActive(false);
                    var txs = card.GetComponentsInChildren<TMP_Text>(true);
                    for (int t = 0; t < txs.Length; t++)
                    {
                        var tr = txs[t].rectTransform;
                        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                        tr.offsetMin = new Vector2(8f, 8f); tr.offsetMax = new Vector2(-8f, -8f);
                        txs[t].gameObject.SetActive(t == 0);
                        txs[t].alignment = TMPro.TextAlignmentOptions.Center;
                        txs[t].enableWordWrapping = true;
                        txs[t].fontSize = 34f;
                        txs[t].color = ink;
                    }
                    if (txs.Length > 0)
                    {
                        int val = g2 == null ? 0 : (c == 0 ? g2.Atk : c == 1 ? g2.Def : g2.Hp);
                        txs[0].text = names[c] + "\n<size=130%><b>+" + val + "</b></size>\n"
                            + "<size=80%><color=#9E5A2A>1 SP 강화</color></size>";
                    }
                    var cb = card.GetComponent<Button>();
                    if (cb != null)
                    {
                        var img5 = card.GetComponent<Image>();
                        if (img5 != null) img5.enabled = false;   // 카드는 배경에 그려져 있다
                        string key = keys[c];
                        cb.transition = Selectable.Transition.None;
                        cb.onClick.RemoveAllListeners();
                        cb.onClick.AddListener(AudioService.Click);
                        cb.onClick.AddListener(() =>
                        {
                            var g3 = PlayerGrowth.Instance;
                            if (g3 == null) return;
                            bool ok = g3.TrySpendStatPoint(key);
                            Toast(ok ? "강화 완료" : "스탯 포인트 부족");
                            if (ok) CasualFx.EnhanceFlash(_host);
                            BuildPlayerChar(); _refresh?.Invoke();
                        });
                    }
                }
                upBtn.gameObject.SetActive(false);   // 원본 버튼은 카드로 대체
            }

            // 외형 꾸미기 = 하단 띠
            var selBtn = bm != null ? bm.Find("Button_Selet") : null;
            if (selBtn != null)
            {
                selBtn.SetParent(safe, false);
                PlaceIn(selBtn, 0.536f, 0.092f, 0.928f, 0.150f);
                var si = selBtn.GetComponent<Image>();
                if (si != null) si.enabled = false;   // 배경 띠 사용
                foreach (var t7 in selBtn.GetComponentsInChildren<TMP_Text>(true))
                {
                    var tr = t7.rectTransform;
                    tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                    tr.offsetMin = tr.offsetMax = Vector2.zero;
                    t7.alignment = TMPro.TextAlignmentOptions.Center;
                    t7.fontSize = 40f;
                    t7.color = deepRed;
                    t7.fontStyle |= TMPro.FontStyles.Bold;
                }
            }
            if (bm != null) PlaceIn(bm, 0f, 0f, 0.001f, 0.001f);   // 빈 컨테이너 치우기

            // ---- 글씨색: 크림 판 위=먹색, 하늘 위=흰색 ----
            foreach (var t8 in p.Go.GetComponentsInChildren<TMP_Text>(true))
            {
                bool underTop = false;
                for (var a = t8.transform; a != null; a = a.parent)
                    if (a.name == "Top") { underTop = true; break; }
                if (underTop) continue;
                bool onPanel = false;
                for (var a = t8.transform; a != null && a != safe; a = a.parent)
                    if (a.name == "Group_Right" || a.name == "Rune" || a.name.StartsWith("Card_")
                        || a.name == "Button_Selet" || a.name == "Level_Info")
                    { onPanel = true; break; }
                if (t8.transform == nameG || (nameG != null && t8.transform.IsChildOf(nameG))
                    || t8.transform == infoT || t8.transform == subT)
                    { t8.color = sky; t8.fontStyle |= TMPro.FontStyles.Bold; }
                else if (onPanel && t8.color != deepRed) t8.color = ink;
            }

            foreach (var rt9 in p.Go.GetComponentsInChildren<RectTransform>(true))
                if (rt9.name == "GroundRing") rt9.gameObject.SetActive(false);
        }

        static System.Action _openAppearance;
        public static void BindAppearance(System.Action a) => _openAppearance = a;

        /// <summary>
        /// 스킬 노드 아래에 이름 라벨을 붙인다. 스테이지 프리팹에는 이름 칸이 없어서
        /// (스테이지는 숫자만 쓰므로) 한 번 만들어 재사용한다.
        /// </summary>
        /// <summary>'골드 5,500/279만' 한 줄. 부족하면 빨갛게. 비용 0이면 빈 문자열.</summary>
        static string CostLineText(string label, double need, double have)
        {
            if (need <= 0) return "";
            string c = have >= need ? "#B8E0FF" : "#FF6B6B";
            // <nobr>로 묶어 '강화/석'처럼 단어 중간에서 줄이 끊기지 않게 한다
            return $"<size=62%><color={c}><nobr>{label} {UiKit.Num(need)}/{UiKit.Num(have)}</nobr></color></size>  ";
        }

        static void EnsureNodeLabel(Transform node, string text, Color color, string sub = null,
            float width = 300f)
        {
            if (node == null) return;
            const string N = "SkillName";
            TMP_Text label = null;
            var found = node.Find(N);
            if (found != null) label = found.GetComponent<TMP_Text>();

            if (label == null)
            {
                label = UiKit.TmpLabel(node, N, text, 26, color, bold: true,
                    TMPro.TextAlignmentOptions.Center);
                var rt = label.rectTransform;
                // 노드 RectTransform은 육각형보다 훨씬 커서 위/아래 끝에 붙이면 패널 밖으로 나간다.
                // 중앙 기준 고정 오프셋으로 육각형 바로 아래에 놓는다.
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(240f, 60f);
                rt.anchoredPosition = new Vector2(0f, -78f);
                label.enableWordWrapping = true;
                label.overflowMode = TMPro.TextOverflowModes.Overflow;
                label.raycastTarget = false;
            }
            // 이름 + (둘째 줄) 필요/보유 재화 또는 해금 레벨
            label.text = string.IsNullOrEmpty(sub) ? text : text + "\n" + sub;
            label.color = color;
            // 라벨 폭이 칸 폭보다 넓으면 옆 노드 이름과 겹친다 → 칸 폭에 맞춘다
            label.rectTransform.sizeDelta = new Vector2(width, string.IsNullOrEmpty(sub) ? 60f : 104f);
        }

        /// <summary>
        /// 눌러도 아무 동작이 없는 데모 '+' 슬롯을 숨긴다.
        /// 우리 게임의 장비 슬롯은 우측 목록이라, 캐릭터 주변 +는 장식일 뿐이다.
        /// (상단 재화칩의 + 는 남겨둔다 — 재화 충전 동선)
        /// </summary>
        static void HideDeadAddButtons(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var btns = p.Go.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < btns.Length; i++)
            {
                if (btns[i].name != "Button_Add") continue;
                // 리스너가 붙어 있으면 실제 기능이 있는 것이므로 건드리지 않는다
                if (btns[i].onClick.GetPersistentEventCount() > 0) continue;
                var rt = btns[i].GetComponent<RectTransform>();
                // 상단 재화 줄(화면 위쪽)에 있는 +는 유지
                if (rt != null && rt.anchoredPosition.y > -140f) continue;
                btns[i].gameObject.SetActive(false);
            }
        }

        static CharacterPreview _charPrev;
        static CharacterPreview _equipPrev;

        /// <summary>
        /// 장비창 좌측의 데모 캐릭터를 실제 플레이어 리그로 바꾼다.
        /// 장착 무기가 바로 반영되므로 '무기만 장착한 화면'도 여기서 확인된다.
        /// </summary>
        static void AttachEquipPreview(CasualPanel p)
        {
            var slot = p.Find("Character") ?? p.Find("Image_Character");
            if (slot == null) return;

            var demo = slot.GetComponent<Image>();
            if (demo != null) demo.enabled = false;
            // 남은 데모 레이어도 정리
            var all = p.Go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var sp = all[i].sprite;
                if (sp != null && sp.name.StartsWith("Character_Sample")) all[i].enabled = false;
            }

            if (_equipPrev == null || _equipPrev.Rect == null || _equipPrev.Rect.parent != slot)
            {
                const int w = 300, h = 380;
                _equipPrev = CharacterPreview.Attach(slot, "EquipBody", w, h, 1.5f, 0.9f, live: true);
                var rt = _equipPrev.Rect;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                float boxH = Mathf.Max(220f, slot.GetComponent<RectTransform>().rect.height);
                rt.sizeDelta = new Vector2(boxH * w / h, boxH);   // 비율 유지 (늘리면 찌부러진다)
                rt.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// 캐릭터창의 데모 초상화(Opti 로봇)를 실제 플레이어 리그 렌더로 교체한다.
        /// 프리팹의 'Character' 이미지는 숨기고 그 자리에 라이브 프리뷰를 얹는다.
        /// </summary>
        static void AttachPlayerPreview(CasualPanel p)
        {
            var slot = p.Find("Character");
            if (slot == null) return;

            // 데모 캐릭터 아트를 전부 끈다. 'Character' 하나만 끄면 프리팹의 다른
            // 샘플 레이어가 남아 실제 캐릭터와 겹쳐 보인다.
            var demo = slot.GetComponent<Image>();
            if (demo != null) demo.enabled = false;
            var all = p.Go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var sp = all[i].sprite;
                if (sp == null) continue;
                string n = sp.name;
                if (n.StartsWith("Character_Sample") || n.StartsWith("Demo_Character") ||
                    n.StartsWith("Image_Character") || n.StartsWith("Hero_Sample"))
                    all[i].enabled = false;
            }

            if (_charPrev == null || _charPrev.Rect == null ||
                _charPrev.Rect.parent != slot)
            {
                const int w = 300, h = 380;
                // orthoSize 1.5는 모자 끝·무기 끝이 렌더 텍스처 밖으로 잘렸다.
                // 시야를 넓혀(2.4) 전신이 들어오게 하고, 대신 화면에서 작게 보인다 (유저 지시)
                _charPrev = CharacterPreview.Attach(slot, "PlayerBody", w, h, 2.4f, 1.55f, live: true);
                var rt = _charPrev.Rect;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                // 슬롯 높이에 맞추되 원본 비율(300:380) 유지 — 늘리면 찌부러진다.
                // 세로 110px 깎기 제거 후 여유 복구 → 프리팹 데모 로봇처럼 슬롯을 채운다
                float boxH = Mathf.Max(200f, slot.GetComponent<RectTransform>().rect.height * 0.8f);
                rt.sizeDelta = new Vector2(boxH * w / h, boxH);
                rt.anchoredPosition = new Vector2(0f, -40f);
            }
        }

        // ---- 스킬트리 (육각 노드 맵 프리팹 재사용) ---------------------------

        static void BuildSkillTree()
        {
            // 사용자 제안: 스테이지 육각 노드 구성을 스킬트리로 쓴다
            var p = P("Stage_Select_Type2", "SkillTree"); if (p == null) return;
            var sk = SkillAdapter.Instance;
            var nodes = SkillTreeDef.Nodes;

            p.SetText("Text_Missions", $"무공 · {FactionService.DisplayName}");
            TitleOnPlank(p, "Text_Missions");
            FillCurrencies(p);

            // ---- 무공 서고: 육각 노드 대신 '책장에 꽂힌 비급' (유저 확정 컨셉) ----
            // 봉인된 비급 = 밧줄로 묶인 책등, 해금 = 낡은 천 표지. 누르면 위 양피지에 상세.
            var shelfSpr = Resources.Load<Sprite>("WuxiaUi/bookshelf");
            var srS = p.Go.GetComponentInChildren<ScrollRect>(true);
            if (srS != null) srS.gameObject.SetActive(false);

            var shelfHost = p.Go.transform.Find("BookShelf") as RectTransform;
            if (shelfHost == null)
            {
                var shelfImg = UiKit.Img(p.Go.transform, "BookShelf", Color.white);
                shelfHost = shelfImg.rectTransform;
                shelfHost.anchorMin = new Vector2(0.08f, 0.10f);
                shelfHost.anchorMax = new Vector2(0.92f, 0.50f);
                shelfHost.offsetMin = shelfHost.offsetMax = Vector2.zero;
                shelfImg.sprite = shelfSpr;
                shelfImg.enabled = shelfSpr != null;
                shelfImg.type = Image.Type.Simple;
                shelfImg.preserveAspect = false;
                shelfImg.raycastTarget = false;

                var inner = new GameObject("Books", typeof(RectTransform)).GetComponent<RectTransform>();
                inner.SetParent(shelfHost, false);
                inner.offsetMin = inner.offsetMax = Vector2.zero;
                var hb = inner.gameObject.AddComponent<HorizontalLayoutGroup>();
                hb.childAlignment = TextAnchor.LowerCenter;
                hb.childControlWidth = false;
                hb.childControlHeight = false;
                hb.childForceExpandWidth = false;
                hb.childForceExpandHeight = false;
            }
            var bookRoot = shelfHost.Find("Books");
            // 개구부 픽셀 실측(v2 아트): 좌 5%·우 93%·바닥 21.4%·상단 90% — 책이 바닥에 선다
            var bookRootRt = (RectTransform)bookRoot;
            bookRootRt.anchorMin = new Vector2(0.05f, 0.214f);
            bookRootRt.anchorMax = new Vector2(0.93f, 0.90f);
            bookRootRt.offsetMin = bookRootRt.offsetMax = Vector2.zero;
            string[] spineNames = { "book_a", "book_b", "book_c" };
            for (int i = 0; i < nodes.Length; i++)
            {
                var nd = nodes[i];
                int lv = sk != null && i < sk.NodeLevel.Length ? sk.NodeLevel[i] : 0;
                bool locked = sk != null && !sk.IsNodeUnlocked(i);
                bool canAct = sk != null && sk.CanPerformAction(i);

                Transform b = i < bookRoot.childCount ? bookRoot.GetChild(i) : null;
                if (b == null)
                {
                    var go = new GameObject("Book" + i, typeof(RectTransform),
                        typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    go.transform.SetParent(bookRoot, false);
                    b = go.transform;
                    // 세로 제목 (한 글자씩 줄바꿈 — 옛 책등처럼)
                    var t = UiKit.TmpLabel(b, "Title", "", 34, Color.white, bold: true,
                        TMPro.TextAlignmentOptions.Center);
                    var trt = t.rectTransform;
                    trt.anchorMin = Vector2.zero;
                    trt.anchorMax = Vector2.one;
                    trt.offsetMin = new Vector2(6f, 60f);
                    trt.offsetMax = new Vector2(-6f, -46f);
                    t.raycastTarget = false;
                    // 하단 레벨 표기
                    var lvl = UiKit.TmpLabel(b, "Lv", "", 24, Color.white, bold: false,
                        TMPro.TextAlignmentOptions.Center);
                    var lrt = lvl.rectTransform;
                    lrt.anchorMin = new Vector2(0f, 0f);
                    lrt.anchorMax = new Vector2(1f, 0f);
                    lrt.pivot = new Vector2(0.5f, 0f);
                    lrt.offsetMin = new Vector2(0f, 14f);
                    lrt.offsetMax = new Vector2(0f, 54f);
                    lvl.raycastTarget = false;
                }
                var bImg = b.GetComponent<Image>();
                var spine = Resources.Load<Sprite>("WuxiaUi/" + (locked ? "book_locked" : spineNames[i % spineNames.Length]));
                if (spine != null) { bImg.sprite = spine; bImg.type = Image.Type.Simple; bImg.preserveAspect = false; }
                bImg.color = Color.white;

                var title = b.Find("Title").GetComponent<TMP_Text>();
                var chars = nd.Name.Replace(" ", "");
                var vsb = new System.Text.StringBuilder();
                for (int c = 0; c < chars.Length; c++) { if (c > 0) vsb.Append('\n'); vsb.Append(chars[c]); }
                title.text = vsb.ToString();
                // 표지별 대비: book_c(양피지 표지)만 먹색, 나머지(어두운 천·봉인 죽간)는 크림.
                // 강화 가능이면 금빛.
                // 한적 v2 아트: 표지 내지가 전부 밝은 크림 — 제목은 일괄 먹색,
                // 강화 가능만 진한 호박색 (밝은 금빛은 씻겨 보인다)
                title.color = canAct ? new Color(0.62f, 0.40f, 0.05f)
                    : new Color(0.28f, 0.17f, 0.09f);

                var lvTxt = b.Find("Lv").GetComponent<TMP_Text>();
                lvTxt.text = locked ? $"봉인 Lv.{nd.ReqLevel}" : lv > 0 ? $"Lv.{lv}" : "미습득";
                lvTxt.color = locked ? new Color(0.55f, 0.20f, 0.12f)
                    : new Color(0.35f, 0.22f, 0.12f);

                int idx = i;
                var btn = b.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Core.AudioService.Click);
                btn.onClick.AddListener(() => ShowSkillDetail(p, idx));
            }

            // 책 크기: 개구부 실측값 기준 매 빌드 재계산 — 8권이 폭에 맞고 바닥에 선다
            {
                float shelfW2 = p.Root.rect.width * (0.92f - 0.08f);
                float shelfH2 = p.Root.rect.height * (0.50f - 0.10f);
                float availW2 = shelfW2 * (0.93f - 0.05f);
                float availH2 = shelfH2 * (0.90f - 0.214f);
                const float gapB = 24f;
                const float aspAvg = 0.72f;   // 한적 표지 평균 종횡비 (w/h)
                int nB = nodes.Length;
                float bh2 = Mathf.Min(availH2 * 0.95f,
                    (availW2 - gapB * (nB - 1)) / (nB * aspAvg));
                for (int bi = 0; bi < bookRoot.childCount && bi < nB; bi++)
                {
                    var brt2 = (RectTransform)bookRoot.GetChild(bi);
                    var img2 = brt2.GetComponent<Image>();
                    float asp2 = img2 != null && img2.sprite != null
                        ? img2.sprite.rect.width / img2.sprite.rect.height : aspAvg;
                    brt2.sizeDelta = new Vector2(bh2 * asp2, bh2);
                }
                var hb2 = bookRoot.GetComponent<HorizontalLayoutGroup>();
                if (hb2 != null) hb2.spacing = gapB;
            }

            // 창 위쪽 절반이 통째로 비어 있었고, 스킬 설명이 어디에도 없었다 → 상세 카드
            int firstShown = 0;
            for (int i = 0; i < nodes.Length; i++)
                if (sk != null && sk.IsNodeUnlocked(i)) { firstShown = i; break; }
            ShowSkillDetail(p, firstShown);

            LayoutSkillWindow(p);
            WireBack(p);
            ShowLocalized(p);
        }

        static TMP_Text _skillDetailTitle, _skillDetailBody;
        static Button _skillDetailBtn;
        static TMP_Text _skillDetailBtnLabel;

        /// <summary>스킬트리 상단 상세 카드. 없으면 만들고, 있으면 내용만 갈아끼운다.</summary>
        static void ShowSkillDetail(CasualPanel p, int idx)
        {
            if (p == null || p.Go == null) return;
            var nodes = SkillTreeDef.Nodes;
            if (idx < 0 || idx >= nodes.Length) return;
            var nd = nodes[idx];
            var sk = SkillAdapter.Instance;

            var host = p.Go.transform.Find("SkillDetail") as RectTransform;
            if (host == null)
            {
                // 무협: 남색 카드가 아니라 나무 벽에 붙인 양피지
                var card = UiKit.Img(p.Go.transform, "SkillDetail", Color.white);
                card.sprite = CasualArt.PaperSheet
                    ?? CasualArt.C("BasicFrame_Round20") ?? CasualArt.C("BasicFrame_Round12");
                card.type = Image.Type.Sliced;
                host = card.rectTransform;
                host.anchorMin = new Vector2(0.10f, 0.52f);
                host.anchorMax = new Vector2(0.90f, 0.80f);
                host.offsetMin = Vector2.zero;
                host.offsetMax = Vector2.zero;

                _skillDetailTitle = UiKit.TmpLabel(host, "DTitle", "", 34, UiKit.TextInverse, bold: true,
                    TMPro.TextAlignmentOptions.MidlineLeft);
                var tr = _skillDetailTitle.rectTransform;
                tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
                tr.pivot = new Vector2(0.5f, 1f);
                tr.offsetMin = new Vector2(28f, -70f); tr.offsetMax = new Vector2(-300f, -14f);

                _skillDetailBody = UiKit.TmpLabel(host, "DBody", "", 26,
                    UiKit.TextInverseDim, bold: false, TMPro.TextAlignmentOptions.TopLeft);
                var br = _skillDetailBody.rectTransform;
                br.anchorMin = new Vector2(0f, 0f); br.anchorMax = new Vector2(1f, 1f);
                br.offsetMin = new Vector2(28f, 16f); br.offsetMax = new Vector2(-300f, -78f);
                _skillDetailBody.enableWordWrapping = true;

                _skillDetailBtn = MapleUiTheme.PrimaryButton(host, "DAct", "강화", null, UiKit.FontH2);
                var rr = _skillDetailBtn.GetComponent<RectTransform>();
                rr.anchorMin = new Vector2(1f, 0.5f); rr.anchorMax = new Vector2(1f, 0.5f);
                rr.pivot = new Vector2(1f, 0.5f);
                rr.sizeDelta = new Vector2(250f, 96f);
                rr.anchoredPosition = new Vector2(-28f, 0f);
                _skillDetailBtnLabel = _skillDetailBtn.GetComponentInChildren<TMP_Text>(true);
            }
            host.gameObject.SetActive(true);

            // 좌 창 액자에 선택한 무공 아이콘 (창 형태에서만)
            var sideT = p.Go.transform.Find("WuxSide");
            if (sideT != null)
            {
                var fi = sideT.Find("SkillFrameIcon") as RectTransform;
                if (fi == null)
                {
                    var go2 = new GameObject("SkillFrameIcon", typeof(RectTransform),
                        typeof(CanvasRenderer), typeof(Image));
                    fi = go2.GetComponent<RectTransform>();
                    fi.SetParent(sideT, false);
                    var im = go2.GetComponent<Image>();
                    im.raycastTarget = false;
                    im.preserveAspect = true;
                }
                PlaceIn(fi, 0.335f, 0.715f, 0.685f, 0.862f);
                var fim = fi.GetComponent<Image>();
                var icoSp = GrowArt.SkillIcon(idx);
                if (fim != null)
                {
                    fim.sprite = icoSp;
                    fim.color = icoSp != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                }
            }

            int lv = sk != null && idx < sk.NodeLevel.Length ? sk.NodeLevel[idx] : 0;
            bool locked = sk != null && !sk.IsNodeUnlocked(idx);
            string kind = nd.IsPassive ? "패시브" : "액티브";

            if (_skillDetailTitle != null)
                _skillDetailTitle.text = nd.Name + "  <size=64%><color=#7A5C3A>" + kind
                    + " · Lv." + lv + "/" + nd.MaxLevel + "</color></size>";

            if (_skillDetailBody != null)
            {
                string desc = !string.IsNullOrEmpty(nd.Description) ? nd.Description : "";
                string eff = !string.IsNullOrEmpty(nd.EffectHint) ? "\n<color=#3E7A3E>" + nd.EffectHint + "</color>" : "";
                string req = locked ? "\n<color=#A03828>레벨 " + nd.ReqLevel + " 이상 필요</color>" : "";
                _skillDetailBody.text = desc + eff + req;
            }

            if (_skillDetailBtn != null)
            {
                _skillDetailBtn.interactable = !locked;
                if (_skillDetailBtnLabel != null)
                    _skillDetailBtnLabel.text = locked ? "잠김"
                        : (sk != null ? sk.ActionButtonLabel(idx) : "강화");
                _skillDetailBtn.onClick.RemoveAllListeners();
                _skillDetailBtn.onClick.AddListener(Core.AudioService.Click);
                int captured = idx;
                string nm = nd.Name;
                _skillDetailBtn.onClick.AddListener(() =>
                {
                    var s2 = SkillAdapter.Instance;
                    if (s2 == null) return;
                    if (!s2.IsNodeUnlocked(captured)) { Toast(nm + " — " + s2.ActionReason(captured)); return; }

                    // 실제 소모 재화를 확인창에 그대로 보여준다
                    double gold, stone; int rd;
                    s2.GetActionCosts(captured, out gold, out stone, out rd);
                    var costs = new List<CostLine>();
                    var w = WalletAdapter.Instance;
                    if (gold > 0) costs.Add(CostLine.Of("골드", gold, w != null ? w.Gold : 0));
                    if (stone > 0) costs.Add(CostLine.Of("강화석", stone,
                        EquipmentService.Instance != null ? EquipmentService.Instance.EnhanceStones : 0));
                    if (rd > 0) costs.Add(CostLine.Of("레드다이아", rd, w != null ? w.RedDiamond : 0));

                    CasualDialogs.Confirm(nm, s2.ActionButtonLabel(captured), costs, () =>
                    {
                        Toast(SkillAdapter.Instance?.PerformNodeAction(captured));
                        CasualFx.EnhanceFlash(_host);
                        BuildSkillTree(); _refresh?.Invoke();
                    });
                });
            }
        }

        // ---- 8번: 동료 구성(출전) -------------------------------------------

        static void BuildCompanionSelect()
        {
            var p = P("Character_Select"); if (p == null) return;
            var ca = CompanionAdapter.Instance;
            var owned = ca != null ? ca.GetSortedOwned(0, false) : new List<CompanionItem>();

            p.SetText("Text_PanelName", "동료 편성");
            var main = ca?.Main;
            if (main != null)
            {
                p.SetText("Text_Name", main.name);
                p.SetText("Text_Rate", main.rarity >= 3 ? "LEGEND" : main.rarity == 2 ? "EPIC" : "NORMAL");
                p.SetText("Text_Level", main.level.ToString());
                p.SetText("Text_Exp", $"{main.count}/5");
            }
            p.SetText("Text_Ready", "확인");
            p.OnClick("Button_Ready", () => { p.Hide(); _refresh?.Invoke(); });

            var rows = Rows(p, Mathf.Max(1, owned.Count));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < owned.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var it = owned[i];
                SetIn(rows[i], "Text_Name", it.name);
                SetIn(rows[i], "Text_Level", it.level.ToString());
                string id = it.id;
                ClickRow(rows[i], () =>
                {
                    CompanionAdapter.Instance?.ToggleSub(id);
                    Toast("편성 변경");
                    Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                    BuildCompanionSelect();
                });
            }
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 9번: 길드 ------------------------------------------------------

        // ---- 문파 입문 (Clan 프리팹 재사용) ---------------------------------

        static int _sectPick;

        static void BuildSectSelect()
        {
            var p = P("Clan", "SectSelect"); if (p == null) return;
            p.SetText("Text_PanelName", "문파");
            p.SetText("Text_JoinTheClan", "문파 입문");
            p.SetText("Text_Search", "검색");
            p.SetText("Text_Join", SectService.HasSect ? "탈퇴" : "입문");
            NoWrap(p, "Text_JoinTheClan"); NoWrap(p, "Text_Info");
            FillCurrencies(p);

            // 세력을 정해야 문파를 고를 수 있다. 세력 없이 문파부터 갈 수는 없다
            if (!FactionService.HasSelected)
            {
                p.SetText("Text_Info", "먼저 세력을 정하세요");
                var empty = Rows(p, 1);
                if (empty.Count > 0)
                {
                    SetIn(empty[0], "Text_OneofThem", "세력 미선택");
                    SetIn(empty[0], "Text_Lv47", "");
                    SetIn(empty[0], "Text_16/20", "");
                    SetIn(empty[0], "Text_Members", "레벨 6에 세력이 열린다");
                    SetIn(empty[0], "Text_14,323", "");
                    ClickRow(empty[0], () => Toast("레벨 6에 세력 선택이 열립니다"));
                }
                WireBack(p); ShowLocalized(p);
                return;
            }

            var sects = SectService.ForFaction(FactionService.Selected);
            p.SetText("Text_Info", FactionService.DisplayName + " 문파");
            _sectPick = Mathf.Clamp(_sectPick, 0, Mathf.Max(0, sects.Count - 1));

            var rows = Rows(p, Mathf.Max(1, sects.Count));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < sects.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var s = sects[i];
                bool joined = SectService.SelectedId == s.Id;

                // 프리팹 행은 길드용이라 Lv·순위 칸이 있다. 문파엔 없는 개념이라 지운다
                // 이름 뒤에 붙이면 칸을 넘어 옆 글자와 겹친다 → 비워둔 Lv 칸을 소속 표시로 쓴다
                SetIn(rows[i], "Text_OneofThem", s.Name);
                SetIn(rows[i], "Text_Lv47", joined ? "소속" : "");
                SetIn(rows[i], "Text_16/20", WeaponsOf(s));
                SetIn(rows[i], "Text_Members", s.Signature);
                SetIn(rows[i], "Text_14,323", "");
                ActiveIn(rows[i], "Icon_ranking", false);
                FitChip(rows[i], "Text_OneofThem", 26f, 44f);
                FitChip(rows[i], "Text_16/20"); FitChip(rows[i], "Text_Members", 16f, 26f);

                // 문파 상징색으로 아이콘을 물들여 한눈에 구분되게
                var img = FindIconImage(rows[i]);
                if (img != null)
                {
                    var sp = GrowArt.IconWeaponKind(
                        s.AllowedKinds != null && s.AllowedKinds.Length > 0 ? s.AllowedKinds[0] : 0);
                    if (sp != null) { img.sprite = sp; img.preserveAspect = true; }
                    img.color = s.RobeMain;
                }

                int idx = i;
                ClickRow(rows[i], () => { _sectPick = idx; ShowSectDetail(p, sects[idx]); });
            }

            ShowSectDetail(p, sects.Count > 0 ? sects[_sectPick] : null);
            LayoutListWindow(p, FactionService.DisplayName + " 문파",
                SectService.HasSect ? "소속 · " + SectService.DisplayName : "미가입");
            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>정파만 병기가 정해져 있다. 사파·마도는 가리지 않는다.</summary>
        static string WeaponsOf(SectService.SectDef s)
        {
            if (s == null || s.AllowedKinds == null || s.AllowedKinds.Length == 0) return "모든 병기";
            return string.Join(" · ", System.Array.ConvertAll(s.AllowedKinds, SectService.KindName));
        }

        static void ShowSectDetail(CasualPanel p, SectService.SectDef s)
        {
            if (p == null || s == null) return;
            bool joined = SectService.SelectedId == s.Id;

            // 오른쪽 카드에도 Text_Info·Text_Members 가 또 있어서 이름으로 찾으면
            // 왼쪽 목록 것이 먼저 잡힌다 → 카드 안에서만 찾는다
            var card = p.Find("Group_Right");
            if (card != null)
            {
                SetIn(card, "Text_DarkKnight", s.Name);
                SetIn(card, "Text_Info", s.Desc);
                SetIn(card, "Text_Level", FactionService.DisplayName);
                SetIn(card, "Text_Members", "병기");
                SetIn(card, "Text_16/20", WeaponsOf(s));
                SetIn(card, "Text_Ranking", "의상");
                SetIn(card, "Text_25,490", s.Robe);
                FitChip(card, "Text_16/20"); FitChip(card, "Text_25,490");

                // 이름과 설명이 같은 자리에 겹쳐 있다 → 이름은 위로, 설명은 아래로.
                // 위로 올리면 문파 문양 이미지에 가려지므로 맨 앞으로 끌어올린다
                // 문양이 카드 위쪽을 크게 차지해 이름 자리를 덮는다 → 문양을 줄여 위로 올린다
                var mark = FindIn<RectTransform>(card, "Clan_Mark_Large");
                if (mark != null && mark.localScale.x > 0.9f)
                {
                    mark.localScale = new Vector3(0.6f, 0.6f, 1f);
                    mark.anchoredPosition += new Vector2(0f, 40f);
                }

                var nameT = FindIn<TMP_Text>(card, "Text_DarkKnight");
                if (nameT != null)
                {
                    nameT.rectTransform.anchoredPosition = new Vector2(-33f, 46f);
                    nameT.enableWordWrapping = false;
                    nameT.transform.SetAsLastSibling();
                }
                var descT = FindIn<TMP_Text>(card, "Text_Info");
                if (descT != null)
                {
                    descT.rectTransform.anchoredPosition = new Vector2(-33f, -36f);
                    descT.enableWordWrapping = true;
                    descT.enableAutoSizing = true;
                    descT.fontSizeMin = 22f; descT.fontSizeMax = 34f;
                }
            }

            p.SetText("Text_Join", joined ? "탈퇴" : "입문");
            FitChip(p.Find("Button_Join"), "Text_Join", 24f, 40f);
            p.OnClick("Button_Join", () =>
            {
                if (joined)
                {
                    SectService.Leave();
                    Toast(s.Name + "에서 나왔습니다");
                }
                else
                {
                    Toast(SectService.Join(s.Id));
                    // 문파를 바꾸면 못 쓰는 무기를 들고 있을 수 있다
                    var w = WeaponSummonAdapter.Instance;
                    if (w != null && w.Equipped != null && !SectService.CanUseKind(w.Equipped.kind))
                        Toast("지금 든 " + SectService.KindName(w.Equipped.kind) + "은(는) 쓸 수 없습니다");
                }
                BuildSectSelect(); _refresh?.Invoke();
            });
        }

        // ---- 경지 (깨달음) --------------------------------------------------

        static void BuildRealm()
        {
            // 문파 화면과 같은 '목록 + 상세' 구조라 Clan 프리팹을 그대로 쓴다
            var p = P("Clan", "Realm"); if (p == null) return;

            p.SetText("Text_PanelName", "경지");
            p.SetText("Text_JoinTheClan", "무력 경지");
            p.SetText("Text_Info", RealmService.DisplayName);
            p.SetText("Text_Search", "안내");
            NoWrap(p, "Text_JoinTheClan"); NoWrap(p, "Text_Info");
            FillCurrencies(p);
            p.OnClick("Button_Search", () => Toast("레벨을 채우면 깨달음의 시련이 열린다"));

            var g = PlayerGrowth.Instance;
            int lv = g != null ? g.Level : 1;
            var all = RealmService.All;

            var rows = Rows(p, all.Length);
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < all.Length;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var d = all[i];
                bool reached = i <= RealmService.Index;
                bool here = i == RealmService.Index;

                SetIn(rows[i], "Text_OneofThem", RealmService.NameOf(d));
                SetIn(rows[i], "Text_Lv47", here ? "현재" : "");
                SetIn(rows[i], "Text_16/20", reached ? "달성" : "Lv " + d.ReqLevel);
                SetIn(rows[i], "Text_Members", d.Insight);
                SetIn(rows[i], "Text_14,323", "");
                ActiveIn(rows[i], "Icon_ranking", false);
                FitChip(rows[i], "Text_OneofThem", 26f, 44f);
                FitChip(rows[i], "Text_16/20"); FitChip(rows[i], "Text_Members", 16f, 26f);

                // 경지 색이 그대로 전투 이펙트 색이다 — 목록에서 미리 보여준다
                var img = FindIconImage(rows[i]);
                if (img != null) img.color = reached ? d.Aura : new Color(0.35f, 0.35f, 0.40f);

                ClickRow(rows[i], () => Toast(RealmService.NameOf(d) + " — " + d.Insight));
            }

            var trial = RealmService.CurrentTrial();
            var card = p.Find("Group_Right");
            string title, body, btn;
            bool can;
            System.Action act;

            if (RealmService.IsMax)
            {
                title = "생사를 넘다";
                body = "더 오를 곳이 없다.";
                btn = "완성"; can = false; act = null;
            }
            else if (RealmService.QuestActive && trial != null)
            {
                int done = Mathf.Min(RealmService.QuestKills, trial.KillCount);
                bool cleared = RealmService.TrialCleared;
                title = trial.Title;
                body = trial.Story + "\n\n" + trial.Objective + "  (" + done + "/" + trial.KillCount + ")";
                btn = cleared ? "경지 돌파" : "수련 중"; can = cleared;
                act = () =>
                {
                    Toast(RealmService.CompleteAwakening());
                    RealmService.ResetQuestProgress();
                    CasualFx.EnhanceFlash(_host);
                    BuildRealm(); _refresh?.Invoke();
                };
            }
            else if (RealmService.CanAwaken && trial != null)
            {
                title = trial.Title;
                body = trial.Story + "\n\n" + trial.Objective;
                btn = "깨달음 시작"; can = true;
                act = () =>
                {
                    RealmService.ResetQuestProgress();
                    RealmService.BeginAwakening();
                    Toast("깨달음의 시련이 시작되었습니다");
                    BuildRealm(); _refresh?.Invoke();
                };
            }
            else
            {
                title = "아직 벽이 보이지 않는다";
                body = "다음 경지 " + RealmService.NextName + "은 Lv " + RealmService.Next.ReqLevel
                     + "부터. 레벨이 모자라면 벽은 모습을 드러내지 않는다.";
                btn = "레벨 부족"; can = false; act = null;
            }

            if (card != null)
            {
                SetIn(card, "Text_DarkKnight", title);
                SetIn(card, "Text_Info", body);
                SetIn(card, "Text_Level", "Lv" + lv);
                SetIn(card, "Text_Members", "전투력");
                SetIn(card, "Text_16/20", "x" + RealmService.PowerMul.ToString("0.00"));
                SetIn(card, "Text_Ranking", "다음");
                SetIn(card, "Text_25,490", RealmService.IsMax ? "—" : RealmService.NextName);
                FitChip(card, "Text_16/20"); FitChip(card, "Text_25,490");

                // 문양이 카드 위쪽을 크게 차지해 이름 자리를 덮는다 → 문양을 줄여 위로 올린다
                var mark = FindIn<RectTransform>(card, "Clan_Mark_Large");
                if (mark != null && mark.localScale.x > 0.9f)
                {
                    mark.localScale = new Vector3(0.6f, 0.6f, 1f);
                    mark.anchoredPosition += new Vector2(0f, 40f);
                }

                var nameT = FindIn<TMP_Text>(card, "Text_DarkKnight");
                if (nameT != null)
                {
                    nameT.rectTransform.anchoredPosition = new Vector2(-33f, 46f);
                    nameT.enableWordWrapping = false;
                    nameT.transform.SetAsLastSibling();
                }
                var descT = FindIn<TMP_Text>(card, "Text_Info");
                if (descT != null)
                {
                    descT.rectTransform.anchoredPosition = new Vector2(-33f, -40f);
                    descT.enableWordWrapping = true;
                    descT.enableAutoSizing = true;
                    descT.fontSizeMin = 20f; descT.fontSizeMax = 30f;
                }
            }

            p.SetText("Text_Join", btn);
            FitChip(p.Find("Button_Join"), "Text_Join", 24f, 40f);
            p.SetInteractable("Button_Join", can);
            p.OnClick("Button_Join", () => { if (act != null) act(); });

            WireBack(p);
            ShowLocalized(p);
        }

        static void BuildClan()
        {
            var p = P("Clan"); if (p == null) return;
            p.SetText("Text_PanelName", "문파");
            // 칸이 좁아 길면 다음 줄로 밀린다 → 짧게 + 줄바꿈 금지
            p.SetText("Text_JoinTheClan", "문파 가입");
            // 부제가 길어 오른쪽 검색 버튼 밑으로 들어가 뒷글자가 가려졌다 → 짧게 + 폭 제한
            p.SetText("Text_Info", "동료를 찾아보세요");
            p.SetText("Text_Search", "검색");
            p.SetText("Text_Join", "가입");
            NoWrap(p, "Text_JoinTheClan"); NoWrap(p, "Text_Info");
            NoWrap(p, "Text_OneofThem"); NoWrap(p, "Text_DarkKnight");
            var clanInfo = p.Get<TMP_Text>("Text_Info");
            if (clanInfo != null)
            {
                clanInfo.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                clanInfo.color = new Color(0.30f, 0.18f, 0.10f);   // 밝은 나무 창 위 먹갈색 (킷 기본은 파랑)
                var cr = clanInfo.rectTransform;
                cr.sizeDelta = new Vector2(Mathf.Min(cr.sizeDelta.x, 400f), cr.sizeDelta.y);
            }
            FillCurrencies(p);

            var g = GuildAdapter.Instance;
            string[] names = { "정의문", "흑풍채", "천마신교", "녹림맹", "무당파", "소림사", "화산파" };
            _clanPick = Mathf.Clamp(_clanPick, 0, names.Length - 1);
            var rows = Rows(p, names.Length);
            int[] lvs = { 47, 45, 45, 44, 43, 42, 41 };
            int[] mem = { 16, 19, 17, 13, 17, 15, 12 };
            int[] rank = { 14323, 25490, 10050, 8314, 8035, 7420, 6980 };
            for (int i = 0; i < rows.Count && i < names.Length; i++)
            {
                SetIn(rows[i], "Text_OneofThem", names[i]);
                SetIn(rows[i], "Text_DarkKnight", names[i]);
                SetIn(rows[i], "Text_Members", "인원");
                // 데모 영문 리치텍스트(<size=38>Lv</size>47)를 실제 값으로
                SetIn(rows[i], "Text_Lv47", $"<size=38>Lv</size>{lvs[i]}");
                SetIn(rows[i], "Text_Lv45", $"<size=38>Lv</size>{lvs[i]}");
                SetIn(rows[i], "Text_16/20", $"{mem[i]}/20");
                SetIn(rows[i], "Text_19/20", $"{mem[i]}/20");
                SetIn(rows[i], "Text_14,323", UiKit.Num(rank[i]));
                SetIn(rows[i], "Text_25,490", UiKit.Num(rank[i]));
                int idx = i;
                ClickRow(rows[i], () =>
                {
                    _clanPick = idx;
                    p.SetText("Text_DarkKnight", names[idx]);
                    p.SetText("Text_Level", $"<size=35>Lv</size>{lvs[idx]}");
                    p.SetText("Text_16/20", $"{mem[idx]}/20");
                    p.SetText("Text_25,490", UiKit.Num(rank[idx]));
                    Toast(names[idx] + " 선택");
                });
            }
            // 우측 상세 기본값
            p.SetText("Text_DarkKnight", names[_clanPick]);
            p.SetText("Text_Level", $"<size=35>Lv</size>{lvs[_clanPick]}");
            p.SetText("Text_Ranking", "명성");
            var gi = p.Get<TMP_Text>("Text_Info");
            p.OnClick("Button_Join", () =>
                Toast(g != null ? g.CreateOrJoin(names[_clanPick]) : "문파 시스템 준비중"));
            p.OnClick("Button_Search", () => Toast("문파 검색은 백엔드 연동 후 지원"));
            LayoutListWindow(p, FactionService.DisplayName + " 문파", "문파에 들어가면 무공이 열린다");
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 17번: 장비 장착 -------------------------------------------------

        static readonly string[] EquipSlotNames = { "무기", "투구", "갑옷", "장신구", "하의", "신발" };

        static void BuildEquipment()
        {
            // Equipment 프리팹은 종이인형(캐릭터 주변 슬롯) 레이아웃이라 우리 6슬롯과 안 맞고
            // 눌러도 반응 없는 '+' 슬롯만 남았다. 무기창과 같은 Inventory(그리드+상세) 레이아웃을
            // 쓰면 깔끔하게 6슬롯을 목록으로 보여줄 수 있다.
            var p = P("Inventory", "EquipInv"); if (p == null) return;
            var inv = InventoryAdapter.Instance;
            int n = inv != null ? inv.Slots.Length : 6;

            p.SetText("Text_PanelName", "장비");
            p.SetText("Text_Sell", "강화");
            p.SetText("Text_AutoSelect", "장착");
            WireSlotFilter(p, () => BuildEquipment());
            FillCurrencies(p);
            PadScrollTop(p, 46f);

            // 부위별 티어 목록. 예전엔 '장착 중인 6칸'만 보여서 강화 말고는 볼 게 없었다.
            // 이제 레벨 10당 한 단계씩 올라가는 티어 장비를 전부 늘어놓고,
            // 도달한 것은 또렷하게 / 아직인 것은 흐리게 + 요구 레벨을 보여준다.
            var list = new List<EquipDef>();
            for (int s = 0; s < n; s++)
            {
                if (_equipFilter >= 0 && s != _equipFilter) continue;
                list.AddRange(ContentCatalog.EquipsForSlot(s));
            }

            // 기본 상세 = 지금 장착 중인 티어 (예전 슬롯 요약이 아니라 실제 장비를 보여준다)
            int detailSlot = _equipFilter >= 0 ? _equipFilter : 0;
            int detailLv = inv != null && detailSlot < inv.Slots.Length ? inv.Slots[detailSlot].level : 1;
            var cur = ContentCatalog.GetEquip(detailSlot, detailLv);
            if (cur != null) ShowEquipItem(p, cur);
            else ShowEquipDetail(p, detailSlot);

            var rows = Rows(p, Mathf.Max(1, list.Count));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < list.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var def = list[i];
                int lv = inv != null && def.slot < inv.Slots.Length ? inv.Slots[def.slot].level : 1;
                bool reached = lv >= def.reqLevel;

                // 자물쇠 이모지는 한글 폰트에 글리프가 없어 □로 깨진다 → 글자로 표시
                SetIn(rows[i], "Item_Count", (reached ? "" : "잠금 ") + "Lv." + def.reqLevel);
                var img = FindIconImage(rows[i]);
                if (img != null)
                {
                    bool dedicated;
                    var sp = GrowArt.IconEquipDef(def, out dedicated);
                    if (sp != null) { img.sprite = sp; img.preserveAspect = true; }
                    // 전용 아트가 있으면 원색 그대로, 없으면 티어 색으로 구분한다
                    Color tc = Color.white;
                    if (!dedicated && !ColorUtility.TryParseHtmlString(def.tint, out tc)) tc = Color.white;
                    img.color = reached ? tc : new Color(tc.r, tc.g, tc.b, 0.35f);
                    RarityBack(img, def.rarity);
                }
                var captured = def;
                ClickRow(rows[i], () => { _equipSlot = captured.slot; ShowEquipItem(p, captured); });
            }
            LayoutEquipWindow(p);
            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>장비 목록에서 고른 티어 장비의 상세.</summary>
        static void ShowEquipItem(CasualPanel p, EquipDef def)
        {
            if (p == null || def == null) return;
            var inv = InventoryAdapter.Instance;
            int lv = inv != null && def.slot < inv.Slots.Length ? inv.Slots[def.slot].level : 1;
            bool reached = lv >= def.reqLevel;
            string[] rank = { "일반", "희귀", "영웅", "전설" };

            LayoutDetailHeader(p);
            p.SetText("Text_Rank", rank[Mathf.Clamp(def.rarity, 0, 3)]);
            p.SetText("Text_ItemName", def.name);
            p.SetText("Text_Info", reached
                ? EquipSlotNames[Mathf.Clamp(def.slot, 0, 5)] + " · " + def.tier + "단계 (보유)"
                : "레벨 " + def.reqLevel + " 필요");
            p.SetText("Text_Name", "공격력");

            var dt = p.Get<Image>("Image_Item");
            if (dt == null)
            {
                var right = p.Find("Group_Right");
                var card = right != null ? right.Find("Background") : null;
                var icoT = card != null ? card.Find("Slot_Item") : null;
                if (icoT != null) dt = icoT.GetComponentInChildren<Image>(true);
            }
            if (dt != null)
            {
                bool dedicated;
                var sp = GrowArt.IconEquipDef(def, out dedicated);
                if (sp != null) { dt.sprite = sp; dt.preserveAspect = true; }
                Color tc = Color.white;
                if (!dedicated && !ColorUtility.TryParseHtmlString(def.tint, out tc)) tc = Color.white;
                dt.color = tc;
            }

            SetDetailStats(p, def.atk, def.hp, def.def);

            double cost = 500 + lv * 300;
            p.SetText("Text_AutoSelect", reached ? "강화" : "잠김");
            p.SetInteractable("Button_Select", reached);
            p.OnClick("Button_Select", () =>
            {
                var w = WalletAdapter.Instance;
                var costs = new List<CostLine> { CostLine.Of("골드", cost, w != null ? w.Gold : 0) };
                CasualDialogs.Confirm(def.name + " 강화", "강화", costs, () =>
                {
                    bool ok = InventoryAdapter.Instance != null
                        && InventoryAdapter.Instance.TryUpgradeSlot(def.slot);
                    Toast(ok ? def.name + " 강화 성공" : "골드가 부족합니다");
                    if (ok) CasualFx.EnhanceFlash(_host);
                    BuildEquipment(); _refresh?.Invoke();
                });
            });
        }

        /// <summary>상세 카드의 능력치 두 줄(공격/방어)에 값을 꽂는다.</summary>
        static void SetDetailStats(CasualPanel p, int atk, int hp, int def)
        {
            var right = p.Find("Group_Right");
            var stats = right != null ? right.Find("Background/Stats") : null;
            if (stats == null) return;
            var texts = stats.GetComponentsInChildren<TMP_Text>(true);
            int seen = 0;
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name != "Text_Value" && !texts[i].name.StartsWith("Text_+")) continue;
                if (seen == 0) texts[i].text = "+" + atk;
                else if (seen == 1) texts[i].text = "+" + def;
                seen++;
                if (seen >= 2) break;
            }
        }

        static int _equipFilter = -1;

        /// <summary>
        /// 장비창 상단 부위 필터 (전체 + 6부위).
        /// 프리팹 탭은 5칸(전체+4)뿐이라 하의·신발이 빠졌다 → 모자란 만큼 복제해 채운다.
        /// </summary>
        static void WireSlotFilter(CasualPanel p, System.Action rebuild)
        {
            if (p == null || p.Go == null) return;
            var tap = p.Find("Tap_Menu");
            if (tap == null) return;
            tap.gameObject.SetActive(true);
            var host = tap.childCount > 0 ? tap.GetChild(0) : tap;

            // 부위 6개를 담으려면 Menu 탭이 6개 있어야 한다 (프리팹 기본은 4개)
            var menus = new List<Transform>();
            for (int i = 0; i < host.childCount; i++)
                if (host.GetChild(i).name == "Menu") menus.Add(host.GetChild(i));
            while (menus.Count > 0 && menus.Count < EquipSlotNames.Length)
            {
                var clone = Object.Instantiate(menus[0], host);
                clone.name = "Menu";                 // 이름을 유지해야 다음 열기에도 같이 잡힌다
                menus.Add(clone);
            }

            var tabs = new List<Transform>();
            var allTab = host.Find("Text_All");
            if (allTab != null) tabs.Add(allTab);
            tabs.AddRange(menus);
            if (tabs.Count == 0) return;

            // 탭 순서: 전체 / 무기(0) / 투구(1) / 갑옷(2) / 장신구(3) / 하의(4) / 신발(5)
            for (int i = 0; i < tabs.Count; i++)
            {
                var t = tabs[i];
                int slot = i - 1;
                bool picked = _equipFilter == slot;

                var img = t.GetComponent<Image>();
                if (img != null)
                {
                    if (i > 0)
                    {
                        var s = GrowArt.IconGear(slot);
                        if (s != null) { img.sprite = s; img.preserveAspect = true; }
                    }
                    img.color = picked ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                    img.raycastTarget = true;
                }
                var tmp = t.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.color = picked ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                    tmp.raycastTarget = true;
                }

                var b = t.GetComponent<Button>();
                if (b == null) b = t.gameObject.AddComponent<Button>();
                b.transition = Selectable.Transition.None;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(AudioService.Click);
                b.onClick.AddListener(() => { _equipFilter = slot; rebuild(); });
            }
            var focus = allTab != null ? allTab.Find("Focus") : null;
            if (focus != null) focus.gameObject.SetActive(_equipFilter < 0);

            // 탭이 7개(전체+6부위)로 늘었으니 프리팹 절대좌표 대신 폭에 맞춰 다시 편다
            SpreadTabs(p, "Tap_Menu");
        }

        /// <summary>
        /// Inventory 프리팹의 우측 상세는 아이콘이 카드 좌상단에 있고,
        /// 등급·이름이 그 '위'에 놓여 카드 밖으로 튀어나가 잘렸다.
        /// 설명(Text_Info)은 아이콘과 겹쳐 글자가 아이콘에 물렸다.
        /// → 등급·이름은 아이콘 오른쪽, 설명은 아이콘 아래로 앵커를 다시 잡는다.
        /// </summary>
        static void LayoutDetailHeader(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            // Slot_Item은 좌측 목록에도 여러 개 있어서 이름으로 찾으면 엉뚱한 걸 잡는다.
            // 우측 상세 카드는 Group_Right/Background 로 특정한다.
            var right = p.Find("Group_Right");
            var card = right != null ? right.Find("Background") as RectTransform : null;
            if (card == null) return;

            System.Action<string, float, float, float, float, TextAlignmentOptions> place =
                (name, xMin, xMax, yMin, yMax, align) =>
                {
                    Transform found = null;
                    var all = card.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < all.Length; i++)
                        if (all[i].name == name && all[i].parent == card) { found = all[i]; break; }
                    if (found == null) return;
                    var rt = (RectTransform)found;
                    rt.anchorMin = new Vector2(xMin, yMin);
                    rt.anchorMax = new Vector2(xMax, yMax);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    var tmp = found.GetComponent<TMP_Text>();
                    if (tmp != null)
                    {
                        tmp.alignment = align;
                        tmp.enableWordWrapping = true;
                        tmp.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                        // 프리팹 폰트가 커서 두 줄로 넘치면 아래 스탯 줄을 덮는다 → 칸에 맞춰 축소
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 18f;
                        tmp.fontSizeMax = tmp.fontSize > 0 ? tmp.fontSize : 40f;
                    }
                };

            // 아이콘은 좌상단(x 0.06~0.35, y 0.77~1.0)에 있다
            place("Text_Rank",     0.38f, 0.97f, 0.90f, 0.99f, TextAlignmentOptions.MidlineLeft);
            place("Text_ItemName", 0.38f, 0.97f, 0.76f, 0.90f, TextAlignmentOptions.MidlineLeft);
            place("Text_Info",     0.06f, 0.97f, 0.56f, 0.74f, TextAlignmentOptions.TopLeft);
        }

        /// <summary>우측 상세 패널에 선택 슬롯 정보 + 강화 버튼.</summary>
        static void ShowEquipDetail(CasualPanel p, int slot)
        {
            var inv = InventoryAdapter.Instance;
            if (inv == null || slot < 0 || slot >= inv.Slots.Length) return;
            var s = inv.Slots[slot];
            string[] rank = { "일반", "희귀", "영웅", "전설" };
            string label = slot < EquipSlotNames.Length ? EquipSlotNames[slot] : "슬롯";

            LayoutDetailHeader(p);
            p.SetText("Text_Rank", rank[Mathf.Clamp(s.rarity, 0, 3)]);
            p.SetText("Text_ItemName", label + " · Lv." + s.level);
            // 이름에 이미 부위가 들어가므로 설명에서 한 번 더 쓰면 중복이다
            p.SetText("Text_Info", "아래 버튼으로 강화");
            p.SetText("Text_Name", "공격력");
            var dt = p.Get<Image>("Image_Item");
            if (dt != null)
            {
                var sp = slot == 0 ? GrowArt.IconWeapon(
                            WeaponSummonAdapter.Instance != null && WeaponSummonAdapter.Instance.Equipped != null ? WeaponSummonAdapter.Instance.Equipped.kind : 0, s.rarity)
                        : GrowArt.IconGear(slot);
                if (sp != null) { dt.sprite = sp; dt.preserveAspect = true; }
            }

            double cost = 500 + s.level * 300;
            p.SetText("Text_AutoSelect", "강화");
            p.OnClick("Button_Select", () =>
            {
                var costs = new List<CostLine>
                {
                    CostLine.Of("골드", cost, WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0),
                };
                CasualDialogs.Confirm($"{label} 강화", $"Lv.{s.level} → Lv.{s.level + 1}", costs, () =>
                {
                    EquipmentService.Instance?.TryUpgrade(slot, PlayerWallet.Instance);
                    CasualFx.EnhanceFlash(_host);
                    BuildEquipment(); _refresh?.Invoke();
                });
            });
            // 분해 버튼은 자동 장착으로 (전체 자동 장착)
            p.OnClick("Button_Sell", () =>
            {
                Toast(WeaponSummonAdapter.Instance?.EquipBest() ?? "무기 없음");
                Combat.FieldAutoHuntController.Instance?.RefreshHeroAppearance();
                BuildEquipment(); _refresh?.Invoke();
            });
        }

        static int _equipSlot;
        static int _clanPick;
        static int _invFilter = -1;   // -1=전체, 0~3=무기 종류
        static int _compSort;         // 0=레벨 1=출전중 2=고등급

        /// <summary>동료 목록 상단 정렬 탭(레벨/전투력/등급) 연결.</summary>
        static void WireCompSort(CasualPanel p)
        {
            if (p == null || p.Go == null) return;
            var txs = p.Go.GetComponentsInChildren<TMP_Text>(true);
            int n = 0;
            for (int i = 0; i < txs.Length && n < 3; i++)
            {
                if (txs[i].name != "Text_Menu") continue;
                string[] labels = { "레벨", "출전중", "고등급" };
                txs[i].text = labels[n];
                txs[i].color = (n == _compSort) ? MaxColor : NormalColor;
                // GetComponentInParent는 비활성 계층에서 null을 준다 — 첫 방문 때
                // 자신에게 붙인 Button을 두 번째 열 때 못 찾아 AddComponent가
                // 터졌다(이미 있음 → b=null → NRE). 자기 자신부터 본다.
                var b = txs[i].GetComponent<Button>();
                if (b == null) b = txs[i].GetComponentInParent<Button>(true);
                if (b == null) b = txs[i].gameObject.AddComponent<Button>();
                int pick = n;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(AudioService.Click);
                b.onClick.AddListener(() => { _compSort = pick; BuildCompanionList(); });
                n++;
            }
        }

        /// <summary>
        /// 프리팹 상단의 종류 탭(전체/검/도/창곤/기병)을 실제 필터로 연결한다.
        /// 프리팹엔 탭 5개가 이미 있는데 아무 동작도 없었다.
        /// </summary>
        /// <summary>
        /// 무기 가방 상단 종류 탭 (전체 / 검 / 도 / 창곤 / 기병).
        ///
        /// 예전 구현은 이름이 Tab*/Filter* 인 Button을 찾았는데, 프리팹의 탭은
        /// 'Text_All' 과 'Menu' 라는 이름이고 **Button 컴포넌트가 아예 없다.**
        /// 그래서 한 개도 배선되지 않아 필터가 클릭조차 안 됐다.
        /// → 실제 노드를 찾아 Button을 직접 붙이고, 아이콘도 무기 종류로 갈아끼운다.
        /// (프리팹 기본은 방패·신발·장신구라 무기 가방에는 맞지 않는다)
        /// </summary>
        static void WireKindFilter(CasualPanel p, System.Action rebuild)
        {
            if (p == null || p.Go == null) return;
            var tap = p.Find("Tap_Menu");
            if (tap == null) return;
            var host = tap.childCount > 0 ? tap.GetChild(0) : tap;

            var tabs = new List<Transform>();
            var allTab = host.Find("Text_All");
            if (allTab != null) tabs.Add(allTab);
            for (int i = 0; i < host.childCount; i++)
                if (host.GetChild(i).name == "Menu") tabs.Add(host.GetChild(i));
            if (tabs.Count == 0) return;

            string[] kindIcon = { null, "IcoC_Sword", "IcoC_Blades", "IcoC_Arrow", "IcoC_Dagger" };

            for (int i = 0; i < tabs.Count && i < kindIcon.Length; i++)
            {
                var t = tabs[i];
                int kind = i - 1;                       // 0번 탭 = 전체(-1)
                bool picked = _invFilter == kind;

                var img = t.GetComponent<Image>();
                if (img != null)
                {
                    if (i > 0 && kindIcon[i] != null)
                    {
                        var s = CasualArt.C(kindIcon[i]);
                        if (s != null) { img.sprite = s; img.preserveAspect = true; }
                    }
                    // 고른 탭만 또렷하게 (어느 종류를 보고 있는지 안 보였다)
                    img.color = picked ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                    img.raycastTarget = true;
                }
                var tmp = t.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.color = picked ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                    tmp.raycastTarget = true;
                }

                var b = t.GetComponent<Button>();
                if (b == null) b = t.gameObject.AddComponent<Button>();
                b.transition = Selectable.Transition.None;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(AudioService.Click);
                b.onClick.AddListener(() => { _invFilter = kind; rebuild(); });
            }

            // '전체' 밑줄 강조는 전체 탭일 때만
            var focus = allTab != null ? allTab.Find("Focus") : null;
            if (focus != null) focus.gameObject.SetActive(_invFilter < 0);
        }

        // ---- 10번: 장비 업그레이드 팝업 --------------------------------------

        static void BuildEquipDetail(int slot)
        {
            var p = P("Equipment_Popup_Detail1"); if (p == null) return;
            var inv = InventoryAdapter.Instance;
            if (inv == null || slot < 0 || slot >= inv.Slots.Length) return;
            var s = inv.Slots[slot];
            string label = SlotEnhanceService.SlotLabel(slot);
            string[] rank = { "COMMON", "RARE", "EPIC", "LEGEND" };

            p.SetText("Text_Legendary", rank[Mathf.Clamp(s.rarity, 0, 3)]);
            p.SetTextStartsWith("Text_Tiitle", label);
            p.SetText("Text_3", s.level.ToString());
            p.SetText("Text_AttackDamage", "공격력");
            p.SetText("Text_Health", "체력");
            p.SetTextStartsWith("Text_+5", "+" + (s.level * 8));
            p.SetTextStartsWith("Text_+3", "+" + (s.level * 25));

            var img = p.Get<Image>("Image_Glove");
            if (img != null)
            {
                var sp = GrowArt.IconGear(slot);
                if (sp != null) { img.sprite = sp; img.preserveAspect = true; }
            }

            double cost = 500 + s.level * 300;
            p.SetText("Text_300", UiKit.Num(cost));
            p.SetText("Text_Upgrade", "강화");
            p.OnClick("Button_Upgrade", () =>
            {
                var costs = new List<CostLine>
                {
                    CostLine.Of("골드", cost, WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0),
                };
                CasualDialogs.Confirm($"{label} 강화", $"Lv.{s.level} → Lv.{s.level + 1}", costs, () =>
                {
                    EquipmentService.Instance?.TryUpgrade(slot, PlayerWallet.Instance);
                    CasualFx.EnhanceFlash(_host);
                    BuildEquipDetail(slot);
                    _refresh?.Invoke();
                });
            });
            p.SetText("Text_Equip", "장착");
            p.OnClick("Button_Equip", () => { p.Hide(); _refresh?.Invoke(); });
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 18번: 인벤토리 --------------------------------------------------

        static void BuildInventory()
        {
            var p = P("Inventory"); if (p == null) return;
            var wa = WeaponSummonAdapter.Instance;
            var all0 = wa != null ? wa.GetSortedOwned(0, false) : new List<WeaponItem>();
            // 상단 필터 탭: 전체 / 검(0) / 도(1) / 창곤(2) / 기병(3)
            var owned = new List<WeaponItem>(all0.Count);
            for (int i = 0; i < all0.Count; i++)
                if (_invFilter < 0 || all0[i].kind == _invFilter) owned.Add(all0[i]);
            WireKindFilter(p, () => BuildInventory());

            p.SetText("Text_PanelName", "가방");
            p.SetText("Text_All", "전체");
            p.SetText("Text_Sell", "분해");
            p.SetText("Text_AutoSelect", "장착");
            LayoutDetailHeader(p);
            // 우측 상세 기본값 (아이템 선택 전)
            var eq0 = wa != null ? wa.Equipped : null;
            string[] rk0 = { "일반", "희귀", "영웅", "전설" };
            p.SetText("Text_Rank", eq0 != null ? rk0[Mathf.Clamp(eq0.rarity, 0, 3)] : "일반");
            p.SetText("Text_ItemName", eq0 != null ? eq0.name : "장착 무기 없음");
            p.SetText("Text_Info", "목록에서 무기를 선택하면\n상세 정보가 표시됩니다.");
            p.SetText("Text_Name", "공격력");
            FillCurrencies(p);
            PadScrollTop(p, 46f);                                   // 필터 바에 목록이 가리던 문제
            FitDetail(p, "Text_Info", "Text_ItemName", "Text_Rank"); // 우측 설명 잘림

            var rows = Rows(p, Mathf.Max(1, owned.Count));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < owned.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var it = owned[i];
                SetIn(rows[i], "Item_Count", it.level.ToString());
                var img = FindIconImage(rows[i]);
                if (img != null)
                {
                    // catalogId가 weapons.json의 id다 (it.id는 소환 때 찍은 GUID)
                    var sp = GrowArt.IconWeaponId(it.catalogId, it.kind);
                    if (sp != null) { img.sprite = sp; img.preserveAspect = true; }
                    // 문파가 안 다루는 병기는 흐리게 — 목록에서 바로 구분되게
                    img.color = SectService.CanUseKind(it.kind)
                        ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    RarityBack(img, it.rarity);
                }
                string id = it.id; string cat = it.catalogId; string nm = it.name; int rar = it.rarity;
                int lv = it.level; int kind = it.kind; bool equipped = it.equipped;
                ClickRow(rows[i], () =>
                {
                    string[] rank = { "일반", "희귀", "영웅", "전설" };
                    p.SetText("Text_Rank", rank[Mathf.Clamp(rar, 0, 3)]);
                    p.SetText("Text_ItemName", nm);
                    p.SetText("Text_Info", equipped
                        ? $"장착 중 · Lv.{lv}\n강화하려면 아래 버튼을 누르세요."
                        : $"등급 {rar} · Lv.{lv}");
                    p.SetText("Text_Name", "공격력");
                    var dt = p.Get<Image>("Image_Item");
                    var sp2 = GrowArt.IconWeaponId(cat, kind);
                    if (dt != null && sp2 != null) { dt.sprite = sp2; RarityBack(dt, rar); }

                    // 문파가 안 다루는 병기는 아예 못 든다 (정파 한정, 사파·마도는 자유)
                    string block = SectService.WhyCannotUse(kind);
                    if (block != null && !equipped)
                    {
                        p.SetText("Text_Info", "<color=#FF8A8A>" + block + "</color>");
                        p.SetText("Text_AutoSelect", "불가");
                        p.SetInteractable("Button_Select", false);
                    }
                    else
                    {
                        p.SetInteractable("Button_Select", true);
                        // 장착 중인 무기면 같은 버튼이 '강화'가 된다 (프리팹 버튼이 2개뿐)
                        p.SetText("Text_AutoSelect", equipped ? "강화" : "장착");
                    }
                    p.OnClick("Button_Select", () =>
                    {
                        if (!equipped)
                        {
                            var why = SectService.WhyCannotUse(kind);
                            if (why != null) { Toast(why); return; }
                            WeaponSummonAdapter.Instance?.Equip(id);
                            Combat.FieldAutoHuntController.Instance?.RefreshHeroAppearance();
                            Toast(nm + " 장착"); BuildInventory(); _refresh?.Invoke();
                            return;
                        }
                        ConfirmWeaponUpgrade();
                    });
                });
            }
            p.OnClick("Button_Sell", () =>
            {
                Toast(InventoryAdapter.Instance?.DisassembleJunk(1) ?? "분해 대상 없음");
                BuildInventory(); _refresh?.Invoke();
            });
            LayoutEquipWindow(p);
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 뽑기 / 강화 (상점·가방에서 호출) --------------------------------

        static void ConfirmWeaponSummon(int pulls)
        {
            var cw = CurrencyWallet.Instance;
            var costs = new List<CostLine>
            {
                CostLine.Of("무기 소환권", pulls, cw != null ? cw.Get(CurrencyId.WeaponTicket) : 0),
            };
            CasualDialogs.Confirm(pulls > 1 ? $"무기 {pulls}연차 소환" : "무기 소환",
                pulls > 1 ? "소환권 10장을 사용합니다." : "소환권 1장을 사용합니다.", costs, () =>
                {
                    var wa = WeaponSummonAdapter.Instance; if (wa == null) return;
                    Toast(pulls > 1 ? wa.SummonTen() : wa.SummonOne());
                    CasualFx.SummonBurst(_host, pulls > 1);
                    BuildShop(); _refresh?.Invoke();
                });
        }

        static void ConfirmCompanionSummon(int pulls)
        {
            var cw = CurrencyWallet.Instance;
            var costs = new List<CostLine>
            {
                CostLine.Of("동료 소환권", pulls, cw != null ? cw.Get(CurrencyId.CompanionTicket) : 0),
            };
            CasualDialogs.Confirm(pulls > 1 ? $"동료 {pulls}연차 소환" : "동료 소환",
                pulls > 1 ? "소환권 10장을 사용합니다." : "소환권 1장을 사용합니다.", costs, () =>
                {
                    var ca = CompanionAdapter.Instance; if (ca == null) return;
                    Toast(pulls > 1 ? ca.SummonTen() : ca.SummonOne());
                    CasualFx.SummonBurst(_host, pulls > 1);
                    Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                    BuildShop(); _refresh?.Invoke();
                });
        }

        static void ConfirmWeaponUpgrade()
        {
            var wa = WeaponSummonAdapter.Instance;
            var w = wa != null ? wa.Equipped : null;
            if (w == null) { Toast("장착 무기 없음"); return; }
            int max = 20 + w.awaken * 20;
            if (w.level >= max) { Toast("최대 레벨 (각성 필요)"); return; }

            var cw = CurrencyWallet.Instance;
            double need = 1 + w.level * 0.2f;
            var costs = new List<CostLine>
            {
                CostLine.Of("무기 강화석", need,
                    cw != null ? cw.Get(CurrencyId.WeaponEnhanceStone) : 0),
            };
            CasualDialogs.Confirm("무기 강화", $"{w.name}  Lv.{w.level} → Lv.{w.level + 1}", costs, () =>
            {
                Toast(wa.LevelUpEquipped());
                CasualFx.EnhanceFlash(_host);
                BuildInventory(); _refresh?.Invoke();
            });
        }

        // ---- 13번: 상점 -----------------------------------------------------

        /// <summary>
        /// 프리팹 탭은 2560 설계 기준 절대좌표(±960)로 박혀 있어서, 2040 폭 패널에서는
        /// 첫 탭('일일')과 끝 탭('젬 팩')이 창 좌우로 잘려 나갔다.
        /// 레이아웃 그룹이 없으므로 실제 폭에 맞춰 균등 재배치한다.
        /// </summary>
        static void SpreadTabs(CasualPanel p, string containerName)
        {
            if (p == null) return;
            var box = p.Find(containerName) as RectTransform;
            if (box == null) return;
            // 프리팹에 따라 탭이 컨테이너 직속이 아니라 래퍼 한 겹 아래에 있다
            if (box.childCount == 1 && box.GetChild(0).childCount > 1)
                box = box.GetChild(0) as RectTransform;
            if (box == null) return;

            var tabs = new List<RectTransform>();
            RectTransform focus = null;
            for (int i = 0; i < box.childCount; i++)
            {
                var c = box.GetChild(i) as RectTransform;
                if (c == null) continue;
                if (c.name == "Focus") { focus = c; continue; }
                tabs.Add(c);
            }
            if (tabs.Count == 0) return;

            float usable = box.rect.width - 160f;          // 좌우 여백
            float step = usable / tabs.Count;
            float start = -usable * 0.5f + step * 0.5f;
            for (int i = 0; i < tabs.Count; i++)
            {
                var t = tabs[i];
                t.anchoredPosition = new Vector2(start + step * i, t.anchoredPosition.y);
                if (t.rect.width > step) t.sizeDelta = new Vector2(step - 8f, t.sizeDelta.y);
            }
            if (focus != null)
                focus.anchoredPosition = new Vector2(tabs[0].anchoredPosition.x, focus.anchoredPosition.y);
        }

        static void BuildShop()
        {
            var p = P("Shop"); if (p == null) return;
            p.SetText("Text_PanelName", "상점");
            p.SetText("Text_Daily", "일일");
            p.SetText("Text_Chest", "상자");
            p.SetTextStartsWith("Text_Gld", "골드 팩");
            p.SetTextStartsWith("Text_Gem", "젬 팩");
            p.SetText("Text_Info", "매일 갱신되는 상품을 확인하세요.");
            SpreadTabs(p, "Tap");
            FillCurrencies(p);

            // 뽑기(소환)를 상점으로 옮겼다. 앞 4칸이 소환, 마지막이 유료 상품.
            var cw = CurrencyWallet.Instance;
            string[] titles =
            {
                "무기 소환", "무기 10연차", "동료 소환", "동료 10연차", "특별 상품",
            };
            // 이 카드는 텍스트 칸 5개가 상·하 두 군데에 몰려 있어 다 쓰면 겹친다.
            // 위(Text_ItemTitle) = 상품명, 아래(Text_Value) = 가격. 나머지는 숨긴다.
            // 카드 폭이 좁아 길면 잘린다. 보유량은 확인창에서 보여주므로 여기선 비용만.
            string[] prices = { "소환권 1", "소환권 10", "소환권 1", "소환권 10", "US $4.99" };
            // 상품 그림: 무기 뽑기는 검, 동료 뽑기는 동료 아이콘.
            // (전부 보석 상자로 두면 무엇을 뽑는지 알 수 없다)
            Sprite[] icons =
            {
                GrowArt.IconSummonWeapon, GrowArt.IconSummonWeapon,
                GrowArt.IconSummonAcc,    GrowArt.IconSummonAcc,
                GrowArt.IconChest,
            };
            var rows = Rows(p, titles.Length);
            for (int i = 0; i < rows.Count && i < titles.Length; i++)
            {
                SetIn(rows[i], "Text_ItemTitle", titles[i]);
                SetIn(rows[i], "Text_Value", prices[i]);
                HideAllNamed(rows[i], "Text_Info");
                HideAllNamed(rows[i], "Text_Gems");
                HideAllNamed(rows[i], "Text_Cost");
                var pic = FindIconImage(rows[i]);
                if (pic != null && icons[i] != null)
                {
                    pic.sprite = icons[i];
                    pic.color = Color.white;
                    pic.preserveAspect = true;
                }
                int idx = i;
                ClickRow(rows[i], () =>
                {
                    switch (idx)
                    {
                        case 0: ConfirmWeaponSummon(1); break;
                        case 1: ConfirmWeaponSummon(10); break;
                        case 2: ConfirmCompanionSummon(1); break;
                        case 3: ConfirmCompanionSummon(10); break;
                        default: Toast("실제 결제는 스토어 연동 후 지원"); break;
                    }
                });
            }
            LayoutShopWindow(p);
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 20번: 설정 -----------------------------------------------------

        static void BuildSettings()
        {
            var p = P("Popup_Setting"); if (p == null) return;
            p.SetText("Text_Title", "설정");
            p.SetText("Text_Laguage", "한국어");
            p.SetText("Text_Like", "평가");
            p.SetText("Text_Rate", "리뷰");
            p.SetText("Text_About", "확률 정보");
            p.SetText("Text_Support", "문의");

            // 토글: 프리팹의 Toggle을 그대로 쓰고 값만 연결
            var toggles = p.Go.GetComponentsInChildren<Toggle>(true);
            if (toggles.Length > 0)
            {
                toggles[0].onValueChanged.RemoveAllListeners();
                toggles[0].isOn = GameSettings.SfxEnabled;
                toggles[0].onValueChanged.AddListener(v =>
                    { GameSettings.SfxEnabled = v; GameSettings.ApplyAudio(); });
            }
            if (toggles.Length > 1)
            {
                toggles[1].onValueChanged.RemoveAllListeners();
                toggles[1].isOn = GameSettings.BgmEnabled;
                toggles[1].onValueChanged.AddListener(v =>
                    { GameSettings.BgmEnabled = v; GameSettings.ApplyAudio(); });
            }
            p.OnClick("Button_Language", () => Toast("한국어 (기본)"));
            p.OnClick("Button_UserID", () => Toast("사용자 ID: 로컬 저장"));
            p.OnClick("Button_Like", () => Toast("스토어 출시 후 지원"));
            p.OnClick("Button_Rate", () => Toast("스토어 출시 후 지원"));
            p.OnClick("Button_About", ShowRateDisclosure);
            p.OnClick("Button_Support", () => Toast("문의는 스토어 출시 후 지원"));
            p.OnClick("Button_Delete", () => Toast("계정 삭제는 백엔드 연동 후 지원"));
            p.SetText("Text_PanelName", "설정");
            // 이 프리팹엔 WuxWindow 노드가 없어 목록창 배치가 통째로 건너뛰어졌다
            // (그래서 혼자 구매 에셋 검은 창으로 남아 있었다). 판때기만 한지로 갈아 끼운다.
            var popup = p.Go.transform.Find("Popup") as RectTransform;
            if (popup != null)
            {
                var pi2 = popup.GetComponent<Image>();
                if (pi2 != null)
                {
                    pi2.sprite = Resources.Load<Sprite>("WuxiaUi/kit_paper_sheet");
                    pi2.type = Image.Type.Sliced;
                    pi2.color = Color.white;
                }
                var line = popup.Find("TitleLine");           // 검은 창용 제목 줄 — 종이엔 안 맞는다
                if (line != null) line.gameObject.SetActive(false);
                var tt = p.Get<TMP_Text>("Text_Title");
                if (tt != null)
                {
                    tt.rectTransform.SetParent(popup, false);
                    PlaceIn(tt.rectTransform, 0.30f, 0.855f, 0.70f, 0.960f);
                    tt.alignment = TMPro.TextAlignmentOptions.Center;
                }
                InkTitle(popup, "", 0.285f, 0.850f, 0.715f, 0.968f);
                WuxUiFx.PlayOpen(popup);
            }
            LayoutListWindow(p, "무협 방치 RPG", "v0.9 · 로컬 저장");
            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>
        /// 확률형 아이템 공시 (스토어 심사 요건). 수치는 각 서비스의 상수를 직접 읽어
        /// 조율 후에도 공시가 저절로 맞는다.
        /// </summary>
        static void ShowRateDisclosure()
        {
            var host = _host != null ? _host : null;
            if (host == null) { Toast("화면 없음"); return; }
            var old = host.Find("RateDisclosure");
            if (old != null) { old.gameObject.SetActive(true); old.SetAsLastSibling(); return; }

            var go = new GameObject("RateDisclosure", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(host, false);
            PlaceIn(rt, 0f, 0f, 1f, 1f);
            go.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.82f);

            var paperGo = new GameObject("Paper", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var paper = paperGo.GetComponent<RectTransform>();
            paper.SetParent(rt, false);
            PlaceIn(paper, 0.16f, 0.06f, 0.84f, 0.94f);
            var pi = paperGo.GetComponent<Image>();
            // win_paper는 캐릭터창 칸 테두리가 그려져 있어 글이 겹친다 → 빈 한지 패널 사용
            pi.sprite = Resources.Load<Sprite>("WuxiaUi/panel_hanji");
            pi.type = Image.Type.Sliced;
            pi.preserveAspect = false;

            var title = UiKit.TmpLabel(paper, "Title", "확률 정보", 40,
                new Color(0.32f, 0.16f, 0.06f), bold: true, TMPro.TextAlignmentOptions.Center);
            PlaceIn(title.rectTransform, 0.1f, 0.90f, 0.9f, 0.97f);

            var viewGo = new GameObject("View", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            var view = viewGo.GetComponent<RectTransform>();
            view.SetParent(paper, false);
            PlaceIn(view, 0.08f, 0.08f, 0.92f, 0.88f);
            viewGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            var body = UiKit.TmpLabel(view, "Body", BuildRateText(), 25,
                new Color(0.30f, 0.20f, 0.10f), bold: false, TMPro.TextAlignmentOptions.TopLeft);
            var brt = body.rectTransform;
            brt.anchorMin = new Vector2(0f, 1f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.offsetMin = new Vector2(0f, brt.offsetMin.y);
            brt.offsetMax = new Vector2(0f, brt.offsetMax.y);
            body.enableWordWrapping = true;
            var fit = body.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewGo.GetComponent<ScrollRect>();
            scroll.content = brt;
            scroll.viewport = view;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            var closeGo = new GameObject("Close", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            var close = closeGo.GetComponent<RectTransform>();
            close.SetParent(paper, false);
            PlaceIn(close, 0.35f, 0.012f, 0.65f, 0.068f);
            var ci = closeGo.GetComponent<Image>();
            ci.sprite = Resources.Load<Sprite>("WuxiaUi/kit_btn_upgrade");
            ci.type = Image.Type.Sliced;
            var cl = UiKit.TmpLabel(close, "T", "닫기", 26, Color.white, bold: true,
                TMPro.TextAlignmentOptions.Center);
            PlaceIn(cl.rectTransform, 0f, 0f, 1f, 1f);
            var cb = closeGo.GetComponent<Button>();
            cb.transition = Selectable.Transition.None;
            cb.onClick.AddListener(AudioService.Click);
            cb.onClick.AddListener(() => go.SetActive(false));
        }

        static string BuildRateText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>■ 소환(뽑기) 등급 확률</b>  — 무기·동료 공통");
            sb.AppendLine("  일반 70%  ·  희귀 20%  ·  영웅 8%  ·  전설 2%");
            sb.AppendLine("  천장: " + Adapters.GachaRoll.SoftPityEpic + "회 연속 영웅 미획득 시 영웅 확정, "
                + Adapters.GachaRoll.HardPityLegendary + "회 연속 전설 미획득 시 전설 확정");
            sb.AppendLine();
            sb.AppendLine("<b>■ 사냥 드랍 확률</b> (처치 1회당)");
            sb.AppendLine("  무기 " + (Core.DropService.WeaponRate * 100f).ToString("0.#") + "%"
                + "  ·  물약 " + (Core.DropService.PotionRate * 100f).ToString("0.#") + "%"
                + "  ·  강화석 " + (Core.DropService.StoneRate * 100f).ToString("0.#") + "%"
                + "  ·  유물조각 " + (Core.DropService.ArtifactRate * 100f).ToString("0.#") + "%");
            sb.AppendLine("  보스 처치 시 전 항목 ×" + Core.DropService.BossDropMul.ToString("0") + " 배");
            sb.AppendLine("  장비 등급: 챕터가 오를수록 상위 등급 확률 상승 (영웅 2%+챕터×0.8%, 희귀 12%+챕터×2%), 전설은 필드 드랍 제외");
            sb.AppendLine();
            sb.AppendLine("<b>■ 주문서 강화 성공 확률</b>");
            sb.AppendLine("  기본 70% · 5번째 시도 80% · 10번째 시도 90% (슬롯당 최대 10회, 실패 시 소멸·하락 없음)");
            sb.AppendLine();
            sb.AppendLine("<b>■ 스타포스 성공 확률</b>");
            sb.AppendLine("  0성 70%에서 1성마다 4%p 감소, 10성부터 30% 고정");
            sb.AppendLine("  실패 시: 13성 이상 1성 하락 · 20성 이상은 5% 확률로 0성 초기화(장비 유지)");
            sb.AppendLine();
            sb.AppendLine("<b>■ 잠재·비전 감정 등급 상승 확률</b>");
            for (int r = 0; r < 5; r++)
                sb.AppendLine("  " + Progression.SlotEnhanceService.RankName(r) + " → "
                    + Progression.SlotEnhanceService.RankName(r + 1) + " : "
                    + (Progression.SlotEnhanceService.RankUpChance(r) * 100f).ToString("0.##") + "%");
            sb.AppendLine();
            sb.AppendLine("모든 확률은 개별 시행 기준이며 이전 결과의 영향을 받지 않습니다 (천장 제외).");
            return sb.ToString();
        }

        // ---- 19번: 룬/장비 합성 (미리 구현) ---------------------------------

        static void BuildRuneFuse()
        {
            var p = P("RuneFuse"); if (p == null) return;
            p.SetText("Text_PanelName", "합성");
            p.SetText("Text_RuneFusion", "장비 합성");
            p.SetText("Text_Sell", "분해");
            var cw = CurrencyWallet.Instance;
            double fuseCost = 9000;
            p.SetText("Text_Gold", UiKit.Num(fuseCost));
            p.SetText("Text_SlotCount", $"{(cw != null ? cw.Get(CurrencyId.WeaponEnhanceStone) : 0)} / 200");
            FillCurrencies(p);

            var rows = Rows(p, 12);
            for (int i = 0; i < rows.Count; i++)
            {
                int idx = i;
                ClickRow(rows[i], () => Toast($"재료 {idx + 1} 선택"));
            }
            p.OnClick("Button_Upgrade", () =>
            {
                var costs = new List<CostLine>
                {
                    CostLine.Of("골드", fuseCost, WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0),
                };
                CasualDialogs.Confirm("장비 합성", "같은 등급 재료 3개를 상위 등급으로 합성합니다.", costs, () =>
                {
                    Toast("합성 시스템은 재료 3종 선택 후 동작합니다 (틀 구현 완료)");
                    CasualFx.EnhanceFlash(_host);
                });
            });
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 12번: 우편함 ---------------------------------------------------

        static void BuildInbox()
        {
            var p = P("Popup_Inbox"); if (p == null) return;
            var ms = MailService.Instance;
            p.SetText("Text_Title", "우편함");
            p.SetText("Text_Info", "<color=#ffe00d>전체 받기</color>로 모두 수령합니다.");

            int n = ms != null ? ms.Inbox.Count : 0;
            var rows = Rows(p, Mathf.Max(1, n));
            for (int i = 0; i < rows.Count; i++)
            {
                bool has = i < n;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var m = ms.Inbox[i];
                double amt = m.attach != null
                    ? (m.attach.gold > 0 ? m.attach.gold
                       : m.attach.redDiamond > 0 ? m.attach.redDiamond : m.attach.amount)
                    : 0;
                SetIn(rows[i], "Text_Message1", m.title);
                SetIn(rows[i], "Text_Message2", m.body);
                SetIn(rows[i], "Text_500", UiKit.Num(amt));
                SetIn(rows[i], "Text_2,500", m.claimed ? "수령완료" : "받기");
                string mid = m.id;
                var claim = rows[i].GetComponentsInChildren<Button>(true);
                for (int b = 0; b < claim.Length; b++)
                {
                    if (claim[b].name != "Button_Claim") continue;
                    claim[b].interactable = !m.claimed;
                    claim[b].onClick.RemoveAllListeners();
                    claim[b].onClick.AddListener(AudioService.Click);
                    claim[b].onClick.AddListener(() =>
                    {
                        Toast(MailService.Instance?.Claim(mid));
                        BuildInbox(); _refresh?.Invoke();
                    });
                }
            }
            p.SetText("Text", "전체 받기");
            p.OnClick("Button_AcceptAll", () =>
            {
                Toast(MailService.Instance?.ClaimAll());
                BuildInbox(); _refresh?.Invoke();
            });
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 14번: 레벨업 축하 ----------------------------------------------

        public static void BuildLevelUp()
        {
            var p = P("LevelUp"); if (p == null) return;
            var g = PlayerGrowth.Instance;
            p.SetText("Text_Level", g != null ? g.Level.ToString() : "1");
            p.SetTextStartsWith("Text_LevelUp", "레벨 업!");
            p.SetText("Text_Rewards", "보상");
            p.SetText("Text_Continue", "확인");
            p.OnClick("Button_Continue", () => { p.Hide(); _refresh?.Invoke(); });
            ShowLocalized(p);
        }

        // ---- 16번: 시즌 패스 / 15번: 핫딜 -----------------------------------

        static void BuildPass()
        {
            var p = P("Pass"); if (p == null) return;
            var sp = SeasonPassService.Instance;
            p.SetText("Text_PanelName", sp != null ? $"시즌 {sp.CurrentSeason}" : "시즌 패스");
            // Text_GoldenPass가 2개 (황금/무료)
            var gp = p.Go.GetComponentsInChildren<TMP_Text>(true);
            int gi2 = 0;
            for (int i = 0; i < gp.Length; i++)
                if (gp[i].name == "Text_GoldenPass")
                    gp[i].text = (gi2++ == 0) ? "황금\n패스" : "무료\n패스";
            p.SetText("Text_Time", sp != null ? $"{sp.DaysRemaining}일 남음" : "-");
            p.SetText("Text_Ativate", sp != null && sp.IsPremium ? "보상 받기" : "활성화");
            FillCurrencies(p);
            p.OnClick("Button_Activate", () =>
            {
                if (sp != null && sp.IsPremium) { Toast(sp.ClaimNextTier()); _refresh?.Invoke(); }
                else BuildPassOffer();
            });
            var rows = Rows(p, 3);
            for (int i = 0; i < rows.Count; i++)
            {
                int idx = i;
                ClickRow(rows[i], () => Toast(SeasonPassService.Instance?.ClaimNextTier() ?? "패스 없음"));
            }
            WireBack(p);
            ShowLocalized(p);
        }

        static void BuildPassOffer()
        {
            var p = P("Popup_Pass"); if (p == null) return;
            p.SetTextStartsWith("Text_Title", "황금 패스");
            p.SetText("Text_USD", "₩6,600");
            // Text_Info가 3개(혜택 목록)라 순서대로 채운다
            string[] perks = { "전용 외형 · 무공서 해금", "매일 젬 +10 · 골드 +1,000", "보너스 선물 상자" };
            var infos = p.Go.GetComponentsInChildren<TMP_Text>(true);
            int pi = 0;
            for (int i = 0; i < infos.Length; i++)
                if (infos[i].name == "Text_Info" && pi < perks.Length) infos[i].text = perks[pi++];
            p.OnClick("Button_Buy", () => Toast("실제 결제는 스토어 연동 후 지원 (현재 mock)"));
            WireBack(p);
            ShowLocalized(p);
        }

        // ---- 3·5번: 스테이지 선택 / 상세 ------------------------------------

        static void BuildStageSelect()
        {
            var p = P("Stage_Select_Type2"); if (p == null) return;
            var st = StageProgress.Instance;
            p.SetText("Text_Missions", st != null ? st.GetDisplayLabel() : "강호");
            TitleOnPlank(p, "Text_Missions");
            FillCurrencies(p);
            // 스킬트리와 같은 프리팹 — viewport가 없어 카드가 창 밖까지 그려진다.
            // 창 안쪽으로 가두고 RectMask2D로 잘라낸다 (BuildSkillTree와 동일 처리).
            var srM = p.Go.GetComponentInChildren<ScrollRect>(true);
            if (srM != null)
            {
                var mrt = srM.GetComponent<RectTransform>();
                mrt.anchorMin = Vector2.zero;
                mrt.anchorMax = Vector2.one;
                mrt.pivot = new Vector2(0.5f, 0.5f);
                mrt.offsetMin = new Vector2(70f, 70f);
                mrt.offsetMax = new Vector2(-70f, -230f);
                if (srM.viewport == null)
                {
                    if (mrt.GetComponent<RectMask2D>() == null) mrt.gameObject.AddComponent<RectMask2D>();
                    srM.viewport = mrt;
                }
            }
            int maxWave = st != null ? st.MaxWaveReached : 1;
            var rows = Rows(p, 8);
            for (int i = 0; i < rows.Count; i++)
            {
                int stage = i + 1;
                SetIn(rows[i], "Text_" + stage, stage.ToString());
                int captured = stage;
                ClickRow(rows[i], () => { _stagePick = captured; BuildStageDetail(); });
            }
            WireBack(p);
            ShowLocalized(p);
        }

        static int _stagePick = 1;
        static DungeonDifficulty _dungDiff = DungeonDifficulty.Easy;

        static void BuildStageDetail()
        {
            var p = P("Stage_Select_Type2_Detail"); if (p == null) return;
            var st = StageProgress.Instance;
            p.SetText("Text_Missions", st != null ? st.GetDisplayLabel() : "강호");
            TitleOnPlank(p, "Text_Missions");
            p.SetText("Text_Title", $"성장 던전");
            p.SetText("Text_Enermy", "적");
            p.SetText("Text_Reward", "보상");
            p.SetText("Text_FinishNOw", "즉시 소탕");
            p.SetTextStartsWith("Text_Fight", "도전");
            FillCurrencies(p);

            BuildDungeonDifficulty(p);
            p.OnClick("Button_Fight", () => { RunDungeon(); p.Hide(); _refresh?.Invoke(); });
            p.OnClick("Button_FinishNow", () => { RunDungeon(); _refresh?.Invoke(); });
            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>난이도 3단계 선택 줄 — 요구 전투력을 못 넘기면 눌리지 않는다.</summary>
        static void BuildDungeonDifficulty(CasualPanel p)
        {
            var host = p.Go.transform;
            var row = host.Find("DiffRow") as RectTransform;
            if (row == null)
            {
                var go = new GameObject("DiffRow", typeof(RectTransform));
                row = go.GetComponent<RectTransform>();
                row.SetParent(host, false);
                for (int i = 0; i < 3; i++)
                {
                    var bg = new GameObject("D" + i, typeof(RectTransform), typeof(CanvasRenderer),
                        typeof(Image), typeof(Button));
                    bg.transform.SetParent(row, false);
                    var bi = bg.GetComponent<Image>();
                    bi.sprite = Resources.Load<Sprite>("WuxiaUi/kit_btn_upgrade");
                    bi.type = Image.Type.Sliced;
                    UiKit.TmpLabel(bg.transform, "T", "", 24, Color.white, bold: true,
                        TMPro.TextAlignmentOptions.Center);
                }
            }
            row.SetSiblingIndex(host.childCount - 1);
            // 던전 다이얼로그는 화면 우측 절반에 뜬다 — 보상 줄과 도전 버튼 사이에 끼운다
            PlaceIn(row, 0.515f, 0.130f, 0.855f, 0.197f);

            for (int i = 0; i < 3; i++)
            {
                var b = row.Find("D" + i) as RectTransform;
                if (b == null) continue;
                PlaceIn(b, i / 3f + 0.01f, 0f, (i + 1) / 3f - 0.01f, 1f);
                var lab = b.Find("T") as RectTransform;
                if (lab != null) PlaceIn(lab, 0.04f, 0.05f, 0.96f, 0.95f);
                var d = (DungeonDifficulty)i;
                bool can = DungeonService.CanEnter(d);
                bool sel = _dungDiff == d;
                var img = b.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = Resources.Load<Sprite>(can ? "WuxiaUi/kit_btn_upgrade" : "WuxiaUi/kit_btn_off");
                    img.color = sel ? Color.white : new Color(0.78f, 0.78f, 0.78f, 0.9f);
                }
                var lt = lab != null ? lab.GetComponent<TMP_Text>() : null;
                if (lt != null)
                {
                    lt.text = DungeonService.DifficultyNames[i]
                        + "  <size=76%>×" + DungeonService.DifficultyReward[i].ToString("0.#") + "</size>";
                    lt.color = new Color(1f, 0.97f, 0.92f);
                    lt.enableWordWrapping = false;
                }
                int captured = i;
                var btn = b.GetComponent<Button>();
                if (btn != null)
                {
                    btn.transition = Selectable.Transition.None;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(AudioService.Click);
                    btn.onClick.AddListener(() =>
                    {
                        var pick = (DungeonDifficulty)captured;
                        if (!DungeonService.CanEnter(pick))
                        {
                            Toast(DungeonService.DifficultyName(pick) + " 난이도는 전투력이 부족합니다");
                            return;
                        }
                        _dungDiff = pick;
                        BuildStageDetail();
                    });
                }
            }
        }

        /// <summary>선택된 던전을 실행한다 (DungeonService는 out 메시지 방식).</summary>
        static void RunDungeon()
        {
            var ds = DungeonService.Instance;
            if (ds == null) { Toast("던전 없음"); return; }
            var id = (DungeonId)Mathf.Clamp(_stagePick - 1, 0, 3);   // 4번 = 수련장
            string msg;
            ds.TryRun(id, PlayerGrowth.Instance, PlayerWallet.Instance,
                EquipmentService.Instance, out msg, _dungDiff);
            Toast(msg);
        }

        // ---- 11번: 오프라인 보상 --------------------------------------------

        public static void BuildOffline()
        {
            var p = P("Popup_Offline"); if (p == null) return;
            var lb = LootBoxService.Instance;
            p.SetTextStartsWith("Text_Title", "오프라인 보상");
            if (lb != null)
            {
                // 프리팹 데모값('40,736B')이 남지 않도록, 숫자처럼 보이는 칸을 순서대로 덮는다
                string[] vals = { UiKit.Num(lb.PendingGold), UiKit.Num(lb.PendingEnhanceStone) };
                var tx = p.Go.GetComponentsInChildren<TMP_Text>(true);
                int vi = 0;
                for (int i = 0; i < tx.Length && vi < vals.Length; i++)
                {
                    string s = tx[i].text;
                    if (string.IsNullOrEmpty(s)) continue;
                    // 콤마/영문 접미사가 붙은 순수 수치 칸만 (제목·버튼은 건드리지 않는다)
                    if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^[\d,\.]+[BMK]?$")) continue;
                    tx[i].text = vals[vi++];
                }
            }
            // 최대 보상 눈금도 한국 단위로 (프리팹 기본값이 30M/120M)
            double cap = lb != null ? lb.PendingGold : 0;
            p.SetText("Text_Free", UiKit.Num(cap));
            p.SetText("Text_120M", UiKit.Num(System.Math.Max(cap * 4, 1000)));

            // 색을 칠하기 전에 먼저 한글화한다.
            // (한글화가 나중에 돌면 이 시점 텍스트는 아직 'x2 Collect'라 아래 조건이 안 맞는다)
            LocalizeByText(p, EnKo);

            // 버튼 글씨가 배경에 묻히지 않게
            var tx2 = p.Go.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < tx2.Length; i++)
            {
                string s = tx2[i].text;
                // '2배 받기'는 노란 버튼 위라 흰 글씨면 안 보인다 → 진한 갈색
                // '받기'는 파란 버튼이라 흰 글씨가 맞다
                if (s == "2배 받기")
                {
                    tx2[i].color = new Color(0.28f, 0.16f, 0.02f, 1f);
                    tx2[i].fontStyle |= TMPro.FontStyles.Bold;
                    tx2[i].enableWordWrapping = false;
                }
                else if (s == "받기")
                {
                    tx2[i].color = Color.white;
                    tx2[i].fontStyle |= TMPro.FontStyles.Bold;
                    tx2[i].enableWordWrapping = false;
                }
            }
            // 프리팹에 남아있는 영문 문구를 전부 한글로
            LocalizeByText(p, EnKo);
            p.OnClick("Button_Collect", () =>
            {
                Toast(lb != null ? lb.ClaimBonus(1f, 0) : "보상 없음");
                AudioService.Gold(); p.Hide(); _refresh?.Invoke();
            });
            p.OnClick("Button_Collect_x2", () =>
            {
                Toast(lb != null ? lb.ClaimBonus(2f, 50) : "보상 없음");
                AudioService.Gold(); p.Hide(); _refresh?.Invoke();
            });
            LayoutOfflineWindow(p, lb);
            WireBack(p);
            ShowLocalized(p);
        }

        /// <summary>
        /// 오프라인 보상 창 배치 (유저 지시): 밧줄 프레임 팝업 + 보상 2칸 + 진행 홈 + 버튼 2개.
        /// 좌표는 win_offline(1008×768) 픽셀 실측값.
        /// </summary>
        static void LayoutOfflineWindow(CasualPanel p, LootBoxService lb)
        {
            if (p == null || p.Go == null) return;
            var ink = new Color(0.26f, 0.16f, 0.08f);
            var cream = new Color(0.96f, 0.93f, 0.85f);

            // 스크림 + 창
            var scrim = p.Go.transform.Find("WuxWindow") as RectTransform;
            if (scrim == null)
            {
                var sg = new GameObject("WuxWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                scrim = sg.GetComponent<RectTransform>();
                scrim.SetParent(p.Go.transform, false);
                scrim.SetSiblingIndex(0);
            }
            PlaceIn(scrim, 0f, 0f, 1f, 1f);
            // 오프라인 창은 팝업이다 — 뒤 화면을 가리지 않는다 (유저 지시: 완전 투명)
            var scrimImg = scrim.GetComponent<Image>();
            if (scrimImg != null)
            {
                scrimImg.sprite = null;
                scrimImg.color = new Color(0f, 0f, 0f, 0f);
                scrimImg.raycastTarget = true;      // 창 밖 오클릭만 막는다
            }
            // 프리팹의 어두운 판(Dimed/Background)도 함께 투명화
            foreach (var im0 in p.Go.GetComponentsInChildren<Image>(true))
            {
                if (im0.transform == scrim) continue;
                if (im0.name != "Dimed" && im0.name != "Dim" && im0.name != "Background") continue;
                im0.color = new Color(im0.color.r, im0.color.g, im0.color.b, 0f);
            }
            var main = EnsureArt(p.Go.transform, "WuxMain", "win_kit_reward", scrim.GetSiblingIndex() + 1);
            PlaceIn(main, 0.285f, 0.170f, 0.715f, 0.830f);   // 1008×768 비율(1.31) 유지
            WuxUiFx.PlayOpen(main);

            foreach (var ps in p.Go.GetComponentsInChildren<ParticleSystem>(true))
                ps.gameObject.SetActive(false);

            // 제목
            var title = p.Get<TMP_Text>("Text_Title");
            if (title != null)
            {
                title.rectTransform.SetParent(main, false);
                PlaceIn(title.rectTransform, 0.31f, 0.858f, 0.69f, 0.920f);
                title.alignment = TMPro.TextAlignmentOptions.Center;
                title.fontSize = 40f; title.color = new Color(0.32f, 0.16f, 0.06f);
                title.enableVertexGradient = false;
                title.text = "오프라인 보상"; title.enableWordWrapping = false;
            }

            // 보상 2칸: 아이콘 + 수치 (실측 x 0.20~0.46 / 0.55~0.81, y 0.41~0.74)
            var golds = new[] { UiKit.Num(lb != null ? lb.PendingGold : 0),
                                UiKit.Num(lb != null ? lb.PendingEnhanceStone : 0) };
            var names = new[] { "금전", "강화석" };
            for (int i = 0; i < 2; i++)
            {
                string cn = "OffSlot" + i;
                var slot = main.Find(cn) as RectTransform;
                if (slot == null)
                {
                    var go = new GameObject(cn, typeof(RectTransform));
                    slot = go.GetComponent<RectTransform>();
                    slot.SetParent(main, false);
                    UiKit.TmpLabel(slot, "V", "", 30, ink, bold: true, TMPro.TextAlignmentOptions.Center);
                    UiKit.TmpLabel(slot, "N", "", 22, ink, bold: false, TMPro.TextAlignmentOptions.Center);
                }
                PlaceIn(slot, i == 0 ? 0.106f : 0.538f, 0.468f, i == 0 ? 0.484f : 0.902f, 0.805f);
                // 쌓인 보상 자리에 기운이 한 번 튄다 — 창이 열릴 때만
                WuxUiFx.Sparkle(slot, i == 0 ? new Color(1f, 0.86f, 0.42f, 0.85f)
                                             : new Color(0.68f, 0.86f, 1f, 0.85f), 0.85f);
                var vT = slot.Find("V");
                if (vT != null)
                {
                    PlaceIn(vT, 0.05f, 0.10f, 0.95f, 0.40f);
                    var t = vT.GetComponent<TMP_Text>();
                    if (t != null)
                    {
                        t.text = golds[i]; t.color = ink; t.fontSize = 34f;
                        t.enableWordWrapping = false;
                    }
                }
                var nT = slot.Find("N");
                if (nT != null)
                {
                    PlaceIn(nT, 0.05f, 0.72f, 0.95f, 0.96f);
                    var t = nT.GetComponent<TMP_Text>();
                    if (t != null)
                    { t.text = names[i]; t.color = ink; t.fontSize = 24f; t.enableWordWrapping = false; }
                }
            }

            // 진행 홈 위 안내
            var bar = main.Find("OffBar") as RectTransform;
            if (bar == null)
                bar = UiKit.TmpLabel(main, "OffBar", "", 24, ink, bold: false,
                    TMPro.TextAlignmentOptions.Center).rectTransform;
            PlaceIn(bar, 0.20f, 0.285f, 0.83f, 0.365f);
            var bt = bar.GetComponent<TMP_Text>();
            if (bt != null)
            {
                bt.text = "접속하지 않은 동안 쌓인 보상";
                bt.color = ink; bt.fontSize = 26f; bt.enableWordWrapping = false;
            }

            // 버튼 두 판
            // 창이 통짜 아트에서 한지+먹선 조립으로 바뀌면서 배경에 그려져 있던 버튼 판이 사라졌다
            // → 버튼에 실제 아트를 붙인다
            // 시트에 먹선 칸이 이미 그려져 있다 — 버튼 판을 또 얹으면 칸을 덮어 튄다.
            // 칸 자체를 버튼으로 쓰고, 이미지는 눌림 판정용으로만 옅게 남긴다.
            var btnFace = new Color(1f, 0.97f, 0.88f, 0.001f);
            var b1 = p.Find("Button_Collect");
            if (b1 != null)
            {
                b1.SetParent(main, false);
                PlaceIn(b1, 0.118f, 0.115f, 0.472f, 0.395f);
                var bi = b1.GetComponent<Image>();
                if (bi != null)
                {
                    bi.enabled = true; bi.sprite = null;
                    bi.type = Image.Type.Simple; bi.color = btnFace;
                }
            }
            // 프리팹엔 '2배 받기' 버튼이 없다 — 배경 그림이 버튼처럼 보이게 그려놨을 뿐,
            // 눌러도 아무 일도 없었다. 통짜 아트를 걷어내며 드러났으므로 진짜 버튼을 만든다.
            var b2 = p.Find("Button_Collect_x2");
            if (b2 == null)
            {
                var go2 = new GameObject("Button_Collect_x2",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                b2 = go2.GetComponent<RectTransform>();
                b2.SetParent(main, false);
                var btn2 = go2.GetComponent<Button>();
                btn2.transition = Selectable.Transition.None;
                btn2.onClick.AddListener(AudioService.Click);
                btn2.onClick.AddListener(() =>
                {
                    Toast(lb != null ? lb.ClaimBonus(2f, 50) : "보상 없음");
                    AudioService.Gold(); p.Hide(); _refresh?.Invoke();
                });
            }
            b2.SetParent(main, false);
            PlaceIn(b2, 0.550f, 0.115f, 0.890f, 0.395f);
            var bi2Img = b2.GetComponent<Image>();
            if (bi2Img != null)
            {
                bi2Img.enabled = true; bi2Img.sprite = null;
                bi2Img.type = Image.Type.Simple; bi2Img.color = btnFace;
            }
            // 킷 버튼 안의 글자·아이콘은 위치가 제각각이라 숨기고, 판 위에 직접 라벨을 얹는다
            for (int bi2 = 0; bi2 < 2; bi2++)
            {
                var btn = bi2 == 0 ? b1 : b2;
                if (btn != null)
                    foreach (var g2 in btn.GetComponentsInChildren<Graphic>(true))
                        if (g2.transform != btn) g2.gameObject.SetActive(false);
                string ln = "OffBtnLabel" + bi2;
                var lbT = main.Find(ln) as RectTransform;
                if (lbT == null)
                    lbT = UiKit.TmpLabel(main, ln, "", 28, cream, bold: true,
                        TMPro.TextAlignmentOptions.Center).rectTransform;
                lbT.SetSiblingIndex(main.childCount - 1);
                PlaceIn(lbT, bi2 == 0 ? 0.118f : 0.550f, 0.115f,
                    bi2 == 0 ? 0.472f : 0.890f, 0.395f);
                var lt2 = lbT.GetComponent<TMP_Text>();
                if (lt2 != null)
                {
                    lt2.text = bi2 == 0 ? "받기" : "2배 받기";
                    lt2.color = cream; lt2.fontSize = 30f; lt2.enableWordWrapping = false;
                    lt2.raycastTarget = false;
                }
            }

            // 남은 킷 요소(옛 팝업 판·아이콘·게이지)는 창 밖으로 새어 나온다 → 정리
            foreach (Transform ch in p.Go.transform)
            {
                if (ch == scrim || ch == main) continue;
                if (ch.name == "PointTop" || ch.name == "SidePlank" || ch.name == "MainPlank") continue;
                ch.gameObject.SetActive(false);
            }
        }

        // ---- 랭킹 (아레나) ---------------------------------------------------

        static void BuildRanking()
        {
            var p = P("Ranking"); if (p == null) return;
            p.SetText("Text_Title_Ranking", "순위");
            p.SetText("Text_Reset", "갱신까지");
            p.SetText("Text_Golbal", "전체");
            p.SetText("Text_Country", "국가");
            p.SetText("Text_Friends", "친구");
            var ar = ArenaAdapter.Instance;
            p.SetText("Text_Time", ar != null ? ar.TierName : "-");
            string[] rivals = { "검존", "나", "혈랑", "무영객", "패도", "청풍" };
            var rows = Rows(p, rivals.Length);
            for (int i = 0; i < rows.Count && i < rivals.Length; i++)
            {
                SetIn(rows[i], "Text_NickName", rivals[i]);
                SetIn(rows[i], "Text_Score", UiKit.Num(9000 - i * 640));
                SetIn(rows[i], "Text_3,524", UiKit.Num(9000 - i * 640));
                SetIn(rows[i], "Text_999", (i + 1).ToString());
                SetIn(rows[i], "Text_4", (i + 1).ToString());
            }
            p.SetText("Text_PanelName", "순위");
            LayoutListWindow(p, ar != null ? "경지 · " + ar.TierName : "순위",
                "매주 갱신 · 상위 보상 지급");
            WireBack(p);
            ShowLocalized(p);
        }
    }

    /// <summary>
    /// 창이 열려 있는 동안 지정한 HUD 노드를 감춘다. 창이 꺼지면(OnDisable) 원상 복구 —
    /// 닫기 경로가 여러 개라도 복구가 새지 않는다.
    /// </summary>
    public class HudMask : MonoBehaviour
    {
        public GameObject[] Targets;

        void OnEnable() { Set(false); }
        void OnDisable() { Set(true); }

        void Set(bool on)
        {
            if (Targets == null) return;
            foreach (var g in Targets) if (g != null) g.SetActive(on);
        }
    }
}
