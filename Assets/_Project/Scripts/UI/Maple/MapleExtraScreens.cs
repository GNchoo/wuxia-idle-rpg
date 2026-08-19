using IdleMvp.Adapters;
using IdleMvp.Combat;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using IdleMvp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// Original-layout shells for job / server / offline / hotdeal / event / dungeon / chat.
    /// Stub data where adapters are thin; wired where services exist.
    /// </summary>
    public class MapleExtraScreens
    {
        readonly MapleModalHost _modals;
        readonly System.Action<string> _toast;
        readonly System.Action _refresh;

        public ModalView JobModal { get; private set; }
        public ModalView ServerModal { get; private set; }
        public ModalView OfflineModal { get; private set; }
        public ModalView HotDealModal { get; private set; }
        public ModalView EventModal { get; private set; }
        public ModalView DungeonModal { get; private set; }
        public GameObject ChatPanel { get; private set; }

        Image[] _serverBgs;
        string[] _servers;
        int _serverSel;
        bool _serverMineOnly;
        Text _offlineTime, _offlineLevel, _offlineNote;
        RectTransform _offlineGrid;
        BarView _offlineBar;
        int _jobSel;
        int _gender; // 0 male 1 female
        Text _jobTitle, _jobRole, _jobDesc, _jobStat;
        Image _jobHero;
        Text _hotDealTimer;
        public ModalView ArtifactModal { get; private set; }
        public ModalView FactionModal { get; private set; }
        public ModalView FatedModal { get; private set; }
        public ModalView RebirthModal { get; private set; }
        Text _artifactInfo;
        int _eventTab;
        GameObject _eventAttend, _eventMission, _eventAchv, _eventCollection;
        Text _dungeonDetail;
        RectTransform _dungeonRewardHost;
        int _dungeonSel;
        RectTransform _chatLog;
        InputField _chatInput;

        public MapleExtraScreens(MapleModalHost modals, System.Action<string> toast, System.Action refresh)
        {
            _modals = modals;
            _toast = toast;
            _refresh = refresh;
        }

        public void BuildAll(Transform hudRoot)
        {
            BuildJob();
            BuildServer();
            BuildOffline();
            BuildHotDeal();
            BuildArtifact();
            BuildEvent();
            BuildDungeon();
            BuildChat(hudRoot);
            BuildFaction();
            BuildFated();
            BuildRebirth();
        }

        void BuildJob()
        {
            JobModal = _modals.Create("Job", "전직", ModalSize.Large, footer: false);
            var body = UiKit.HStack(JobModal.Content, "JobBody", 12f, 0, 0, 0, 0, TextAnchor.UpperLeft, true);
            UiKit.Fix(body, -1f, 720f);

            var info = UiKit.VStack(body, "Info", 8f, 8, 8, 8, 8);
            UiKit.Fix(info, 360f, -1f);
            _jobRole = UiKit.Label(info, "Role", "전사", UiKit.FontBody, FantasyKitSlots.KitTeal, FontStyle.Bold);
            UiKit.Fix(_jobRole.rectTransform, -1f, 24f);
            _jobTitle = UiKit.Label(info, "Title", "무사", UiKit.FontH1 + 4, UiKit.TextInverse, FontStyle.Bold);
            UiKit.Fix(_jobTitle.rectTransform, -1f, 40f);
            MapleUiTheme.FieldTextOutline(_jobTitle);
            _jobStat = MapleUiTheme.InfoChip(info, "Stat", "주 스탯 : STR", 40f);
            _jobDesc = MapleUiTheme.InfoChip(info, "Desc", "근접 전투에 특화된 모험가입니다.", 80f);
            MapleUiTheme.SectionHeader(info, "대표 스킬");
            var skillRow = UiKit.HStack(info, "Skills", 8f, 0, 0, 0, 0);
            UiKit.Fix(skillRow, -1f, 72f);
            for (int i = 0; i < 4; i++)
            {
                var ic = FantasyKitSlots.SimpleIcon(skillRow, "Sk" + i, GrowArt.SkillIcon(i), 56f);
                UiKit.Fix(ic, 56f, 56f);
            }

            var mid = UiKit.VStack(body, "Mid", 8f, 8, 8, 8, 8, TextAnchor.MiddleCenter);
            UiKit.Flex(mid);
            _jobHero = UiKit.Img(mid, "Hero", new Color(0.075f, 0.13f, 0.30f, 0.97f));
            _jobHero.sprite = CasualArt.CardRound;
            _jobHero.type = UnityEngine.UI.Image.Type.Sliced;
            UiKit.Fix(_jobHero, 280f, 360f);
            var face = UiKit.Img(_jobHero.transform, "Face", Color.white);
            face.sprite = GrowArt.Hero;
            face.preserveAspect = true;
            UiKit.Fill(face.rectTransform, 20f);

            var right = UiKit.VStack(body, "Right", 8f, 8, 8, 8, 8);
            UiKit.Fix(right, 420f, -1f);
            MapleUiTheme.SectionHeader(right, "직업 선택 · 모험가");
            var grid = UiKit.Grid(right, "Jobs", new Vector2(190f, 72f), new Vector2(8f, 8f), 2);
            var jobs = ContentCatalog.Jobs;
            string curId = JobProgress.JobId;
            for (int i = 0; i < jobs.Length; i++)
            {
                if (jobs[i].id == curId) _jobSel = i;
            }
            for (int i = 0; i < jobs.Length; i++)
            {
                int idx = i;
                var job = jobs[i];
                bool unlocked = JobProgress.IsUnlocked(job);
                var card = FantasyKitSlots.PortraitCard(grid, "J" + i, job.name, unlocked ? job.role : $"Lv.{job.unlockLevel}",
                    GrowArt.IconJob(job.id), GrowArt.Rarity(unlocked ? 2 : 0),
                    () =>
                    {
                        if (!unlocked) { _toast($"잠금 · {job.name}은(는) Lv.{job.unlockLevel} 해금"); return; }
                        _jobSel = idx;
                        RefreshJob();
                    }, 190f, 72f);
                // 직업 아이콘은 이제 키트 컬러 아이콘이라 어둡게 칠하면 그림이 죽는다
                if (card.Icon != null)
                {
                    card.Icon.color = unlocked ? Color.white : new Color(0.55f, 0.58f, 0.65f, 0.9f);
                    card.Icon.preserveAspect = true;
                }
                if (!unlocked && card.Sub != null) card.Sub.text = "잠금";
            }

            MapleUiTheme.SectionHeader(right, "성별 선택");
            var gen = UiKit.HStack(right, "Gender", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(gen, -1f, 48f);
            var male = MapleUiTheme.SecondaryButton(gen, "M", "남자", () => { _gender = 0; RefreshJob(); });
            UiKit.Fix(male, -1f, 44f);
            var female = MapleUiTheme.PrimaryButton(gen, "F", "여자", () => { _gender = 1; RefreshJob(); });
            UiKit.Fix(female, -1f, 44f);

            var go = MapleUiTheme.AccentButton(right, "Go", "전직하러 가기", () =>
            {
                var job = ContentCatalog.GetJobByIndex(_jobSel);
                if (job == null) { _toast("직업 데이터 없음"); return; }
                PlayerPrefs.SetInt("IdleGrow.Maple.Gender", _gender);
                string msg = JobProgress.SetJob(job.id);
                _toast($"{msg} ({(_gender == 0 ? "남자" : "여자")})");
                FieldAutoHuntController.Instance?.RefreshHeroAppearance();
                _refresh?.Invoke();
                _modals.Close();
            }, UiKit.FontH2);
            UiKit.Fix(go, -1f, 64f);

            JobModal.Refresh = RefreshJob;
            _gender = PlayerPrefs.GetInt("IdleGrow.Maple.Gender", 0);
            RefreshJob();
        }

        void RefreshJob()
        {
            var jobs = ContentCatalog.Jobs;
            int i = Mathf.Clamp(_jobSel, 0, Mathf.Max(0, jobs.Length - 1));
            var job = jobs.Length > 0 ? jobs[i] : null;
            if (job == null) return;
            if (_jobTitle != null) _jobTitle.text = job.name;
            if (_jobRole != null) _jobRole.text = job.role;
            if (_jobStat != null) _jobStat.text = $"주 스탯 : {job.primaryStat} · ATK×{job.atkMul:0.##}";
            if (_jobDesc != null)
            {
                var tree = SkillTreeDef.GetNodes(job.treeId);
                string sk = tree.Length > 0 ? tree[0].Name : "";
                _jobDesc.text = $"{job.desc}\n대표 스킬: {sk} · 성별: {(_gender == 0 ? "남자" : "여자")}";
            }
            if (_jobHero != null && GrowArt.Hero != null)
            {
                var face = _jobHero.transform.Find("Face")?.GetComponent<Image>();
                if (face != null) face.sprite = GrowArt.Hero;
            }
        }

        void BuildServer()
        {
            ServerModal = _modals.Create("Server", "서버선택", ModalSize.Large);
            var c = ServerModal.Content;
            _servers = new[]
            {
                "천산 1", "천산 2", "천산 3", "천산 4",
                "천산 5", "천산 6", "천산 7", "천산 8",
                "천산 9", "천산 10", "천산 11", "천산 12",
                "천산 13", "천산 14", "화산 1", "화산 2"
            };
            _serverBgs = new Image[_servers.Length];
            var grid = UiKit.Grid(c, "Servers", new Vector2(240f, 80f), new Vector2(10f, 10f), 4);
            string mine = PlayerPrefs.GetString("IdleGrow.Maple.Server", "천산 6");
            for (int i = 0; i < _servers.Length; i++)
            {
                int idx = i;
                bool hasChar = _servers[i] == mine || i == 5;
                var bg = UiKit.Img(grid, "S" + i, FantasyKitSlots.KitPanel);
                FantasyKitSlots.Slice(bg, GrowArt.InvSlot);
                UiKit.Fix(bg, 240f, 80f);
                _serverBgs[i] = bg;
                var btn = bg.gameObject.AddComponent<Button>();
                btn.targetGraphic = bg;
                UiKit.Press(btn);
                var title = UiKit.Label(bg.transform, "T", _servers[i], UiKit.FontBody, UiKit.TextInverse, FontStyle.Bold, TextAnchor.UpperLeft);
                var tr = title.rectTransform;
                tr.anchorMin = new Vector2(0.05f, 0.45f);
                tr.anchorMax = new Vector2(0.95f, 0.95f);
                tr.offsetMin = Vector2.zero;
                tr.offsetMax = Vector2.zero;
                MapleUiTheme.FieldTextOutline(title);
                var st = UiKit.Label(bg.transform, "St", hasChar ? "캐릭터 있음" : "표시용 · 혼잡", UiKit.FontCaption + 1,
                    hasChar ? FantasyKitSlots.KitTeal : new Color(1f, 0.55f, 0.2f), FontStyle.Bold, TextAnchor.LowerRight);
                var sr = st.rectTransform;
                sr.anchorMin = new Vector2(0.05f, 0.05f);
                sr.anchorMax = new Vector2(0.95f, 0.45f);
                sr.offsetMin = Vector2.zero;
                sr.offsetMax = Vector2.zero;
                btn.onClick.AddListener(() =>
                {
                    _serverSel = idx;
                    HighlightServer();
                    PlayerPrefs.SetString("IdleGrow.Maple.Server", _servers[idx]);
                });
                if (_servers[i] == mine) _serverSel = i;
            }

            Button filterBtn = null;
            filterBtn = MapleUiTheme.SecondaryButton(ServerModal.Footer, "Filter", "내 캐릭터만 OFF", () =>
            {
                _serverMineOnly = !_serverMineOnly;
                var t = filterBtn != null ? filterBtn.GetComponentInChildren<TMPro.TMP_Text>() : null;
                if (t != null) t.text = _serverMineOnly ? "내 캐릭터만 ON" : "내 캐릭터만 OFF";
                for (int i = 0; i < _serverBgs.Length; i++)
                {
                    bool show = !_serverMineOnly || i == 5 || _servers[i] == mine;
                    _serverBgs[i].gameObject.SetActive(show);
                }
            });
            UiKit.Fix(filterBtn, 220f, 56f);

            var ok = MapleUiTheme.PrimaryButton(ServerModal.Footer, "Ok", "선택 완료", null, UiKit.FontH2);
            UiKit.Fix(ok, 240f, 60f);
            // onClick set by host after Build

            HighlightServer();
            ServerModal.Refresh = HighlightServer;
        }

        public void BindServerComplete(System.Action onComplete)
        {
            if (ServerModal?.Footer == null) return;
            var ok = ServerModal.Footer.Find("Ok")?.GetComponent<Button>();
            if (ok == null) return;
            ok.onClick.RemoveAllListeners();
            ok.onClick.AddListener(() => onComplete?.Invoke());
        }

        void HighlightServer()
        {
            if (_serverBgs == null) return;
            for (int i = 0; i < _serverBgs.Length; i++)
            {
                if (_serverBgs[i] == null) continue;
                bool on = i == _serverSel;
                _serverBgs[i].color = on ? new Color(0.55f, 0.9f, 0.95f, 1f) : Color.white;
            }
        }

        void BuildOffline()
        {
            OfflineModal = _modals.Create("Offline", "오프라인 보상", ModalSize.Small);
            var c = OfflineModal.Content;
            if (OfflineModal.Footer != null)
            {
                var fg = OfflineModal.Footer.GetComponent<HorizontalLayoutGroup>();
                if (fg != null)
                {
                    fg.childAlignment = TextAnchor.MiddleCenter;
                    fg.childForceExpandWidth = true;
                }
            }
            var top = UiKit.HStack(c, "Top", 12f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(top, -1f, 48f);
            _offlineTime = UiKit.Label(top, "Time", "OFFLINE 00:00 / 8h", UiKit.FontBody, FantasyKitSlots.KitTeal, FontStyle.Bold);
            UiKit.Flex(_offlineTime);
            _offlineLevel = UiKit.Label(top, "Lv", "Lv.1 >> Lv.1", UiKit.FontBody, UiKit.ExpColor, FontStyle.Bold, TextAnchor.MiddleRight);
            UiKit.Flex(_offlineLevel);

            _offlineBar = MapleUiTheme.Bar(c, "OfflineBar", FantasyKitSlots.KitTeal, true);
            UiKit.Fix(_offlineBar.Go.transform, -1f, 32f);

            MapleUiTheme.SectionHeader(c, "아이템 획득");
            _offlineGrid = UiKit.FillGrid(c, "Items", new Vector2(120f, 148f), new Vector2(8f, 8f), 3, 3);

            _offlineNote = MapleUiTheme.InfoChip(c, "Note", "※ 미수령 보상은 우편함에서도 확인할 수 있습니다.", 48f);

            var x15 = MapleUiTheme.AccentButton(OfflineModal.Footer, "X15", "1.5배 보상받기", () =>
            {
                var lb = LootBoxService.Instance;
                if (lb == null) { _toast("보상 없음"); return; }
                string msg = lb.ClaimBonus(1.5f, 50);
                _toast(msg);
                if (msg != null && msg.IndexOf("필요", System.StringComparison.Ordinal) < 0 &&
                    msg.IndexOf("없습니다", System.StringComparison.Ordinal) < 0)
                {
                    IdleMvp.Core.AudioService.Gold();
                    _modals.Close();
                    _refresh?.Invoke();
                }
            });
            UiKit.Fix(x15, -1f, 60f);
            var x15Le = x15.GetComponent<LayoutElement>() ?? x15.gameObject.AddComponent<LayoutElement>();
            x15Le.flexibleWidth = 1f;
            x15Le.minWidth = 180f;

            var claim = MapleUiTheme.PrimaryButton(OfflineModal.Footer, "Claim", "확인", () =>
            {
                var lb = LootBoxService.Instance;
                if (lb == null) { _toast("보상 없음"); return; }
                if (lb.PendingGold + lb.PendingXp + lb.PendingEnhanceStone <= 0)
                {
                    _toast("수령할 보상이 없습니다");
                    return;
                }
                var got = lb.Claim(1f);
                IdleMvp.Core.AudioService.Gold();
                _toast($"보상 수령 · 골드 {got.gold:0} · XP {got.xp:0}");
                _modals.Close();
                _refresh?.Invoke();
            });
            UiKit.Fix(claim, -1f, 60f);
            var claimLe = claim.GetComponent<LayoutElement>() ?? claim.gameObject.AddComponent<LayoutElement>();
            claimLe.flexibleWidth = 1f;
            claimLe.minWidth = 140f;

            OfflineModal.Refresh = () =>
            {
                var lb = LootBoxService.Instance;
                float cap = lb != null ? Mathf.Max(0.01f, lb.CapHours) : 8f;
                float hours = lb != null ? Mathf.Clamp(lb.PendingHours, 0f, cap) : 0f;
                float pct = Mathf.Clamp01(hours / cap);
                int h = Mathf.FloorToInt(hours);
                int m = Mathf.FloorToInt((hours - h) * 60f);
                if (_offlineTime != null)
                    _offlineTime.text = $"OFFLINE  {h:00}:{m:00} / {cap:0}h";
                _offlineBar?.Set(pct, $"{hours:0.#}h / {cap:0}h");
                int lv = PlayerGrowth.Instance != null ? PlayerGrowth.Instance.Level : 1;
                int xpGain = lb != null ? Mathf.FloorToInt((float)lb.PendingXp) : 0;
                if (_offlineLevel != null)
                    _offlineLevel.text = xpGain > 0 ? $"Lv.{lv}  ·  XP +{xpGain}" : $"Lv.{lv}";
                for (int i = _offlineGrid.childCount - 1; i >= 0; i--)
                    Object.Destroy(_offlineGrid.GetChild(i).gameObject);
                if (lb == null) return;
                var gl = _offlineGrid.GetComponent<GridLayoutGroup>();
                var cell = gl != null ? gl.cellSize : new Vector2(108f, 148f);
                // Only real pending rewards (no fake ticket/diamond zeros).
                FantasyKitSlots.RewardTile(_offlineGrid, "G", "골드", UiKit.Num(lb.PendingGold), GrowArt.IconGold, GrowArt.Rarity(1), cell);
                FantasyKitSlots.RewardTile(_offlineGrid, "X", "경험치", UiKit.Num(lb.PendingXp), GrowArt.IconXp, GrowArt.Rarity(1), cell);
                FantasyKitSlots.RewardTile(_offlineGrid, "S", "강화석", lb.PendingEnhanceStone.ToString("0.#"), GrowArt.IconStone, GrowArt.Rarity(0), cell);
            };
        }

        void BuildHotDeal()
        {
            HotDealModal = _modals.Create("HotDeal", "핫딜", ModalSize.Medium, footer: false);
            var body = UiKit.HStack(HotDealModal.Content, "Body", 16f, 8, 8, 8, 8, TextAnchor.MiddleLeft, true);
            UiKit.Fix(body, -1f, 280f);

            FantasyKitSlots.PortraitCard(body, "Art", "최상급 무기", "핫딜", GrowArt.IconSummonWeapon, GrowArt.Rarity(3), null, 250f, 240f);
            var right = UiKit.VStack(body, "R", 10f, 0, 0, 0, 0);
            UiKit.Flex(right);
            var title = UiKit.Label(right, "T", "최상급 무기 핫딜", UiKit.FontH1, new Color(1f, 0.7f, 0.2f), FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Fix(title.rectTransform, -1f, 40f);
            MapleUiTheme.FieldTextOutline(title);
            MapleUiTheme.InfoChip(right, "Sub", "더욱 강력한 무기와 함께 빠르게 성장해보세요!", 48f);
            var slots = UiKit.HStack(right, "Slots", 12f, 8, 8, 8, 8, TextAnchor.MiddleCenter);
            UiKit.Fix(slots, -1f, 100f);
            FantasyKitSlots.PortraitCard(slots, "Gem", "다이아", "1,500", GrowArt.IconGem, GrowArt.Rarity(2), null, 100f, 90f);
            FantasyKitSlots.PortraitCard(slots, "Wep", "최상급", "1", GrowArt.IconSummonWeapon, GrowArt.Rarity(3), null, 100f, 90f);
            _hotDealTimer = MapleUiTheme.InfoChip(right, "Timer", "남은 시간 —", 36f);
            EnsureHotDealWindow();
            var buy = MapleUiTheme.AccentButton(right, "Buy", "블루다이아 팩 구매", () =>
            {
                if (IsHotDealExpired())
                {
                    _toast("핫딜이 종료되었습니다");
                    EnsureHotDealWindow();
                    RefreshHotDeal();
                    return;
                }
                IapBridge.Instance?.Purchase(IapProductCatalog.BlueDiamondPack0, () =>
                {
                    string msg = ShopAdapter.Instance?.BuyBlueDiamondPack(0) ?? "지급 실패";
                    _toast(msg);
                    _refresh?.Invoke();
                    _modals.Close();
                }, err => _toast(err ?? "구매 실패"));
            }, UiKit.FontH2);
            UiKit.Fix(buy, -1f, 64f);
            HotDealModal.Refresh = RefreshHotDeal;
            RefreshHotDeal();
        }

        const string PrefHotDealEnd = "IdleGrow.Maple.HotDealEndUtc";

        void EnsureHotDealWindow()
        {
            long end = 0;
            long.TryParse(PlayerPrefs.GetString(PrefHotDealEnd, "0"), out end);
            long now = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            if (end <= now)
            {
                end = now + 24 * 3600;
                PlayerPrefs.SetString(PrefHotDealEnd, end.ToString());
                PlayerPrefs.Save();
            }
        }

        bool IsHotDealExpired()
        {
            long end = 0;
            long.TryParse(PlayerPrefs.GetString(PrefHotDealEnd, "0"), out end);
            return end <= (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        void RefreshHotDeal()
        {
            EnsureHotDealWindow();
            long end = 0;
            long.TryParse(PlayerPrefs.GetString(PrefHotDealEnd, "0"), out end);
            long left = System.Math.Max(0, end - (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
            int h = (int)(left / 3600);
            int m = (int)((left % 3600) / 60);
            int s = (int)(left % 60);
            if (_hotDealTimer != null)
                _hotDealTimer.text = left <= 0 ? "핫딜 종료 · 새로고침 시 재개" : $"한정 · 남은 {h:00}:{m:00}:{s:00}";
        }

        void BuildArtifact()
        {
            ArtifactModal = _modals.CreateDual("Artifact", "유물", footer: false, leftWidth: 260f);
            MapleLightTheme.SkinDemoPage(ArtifactModal, "Artifact", wideContent: true);
            var left = ArtifactModal.LeftRail;
            var c = ArtifactModal.Content;
            MapleUiTheme.SectionHeader(left, "장착 효과");
            _artifactInfo = MapleUiTheme.InfoChip(left, "Info", "", 140f);
            MapleUiTheme.SectionHeader(c, "보유 유물");
            var grid = UiKit.FillGrid(c, "Arts", new Vector2(170f, 150f), new Vector2(8f, 8f), 3, 4);
            ArtifactModal.Refresh = () =>
            {
                var svc = ArtifactService.Instance;
                if (_artifactInfo != null)
                    _artifactInfo.text = svc != null ? svc.StatusLine() : "유물 서비스 없음";
                for (int i = grid.childCount - 1; i >= 0; i--)
                    Object.Destroy(grid.GetChild(i).gameObject);
                if (svc == null) return;
                var cell = grid.GetComponent<GridLayoutGroup>() != null
                    ? grid.GetComponent<GridLayoutGroup>().cellSize
                    : new Vector2(170f, 150f);
                if (svc.Owned.Count == 0)
                {
                    MapleUiTheme.InfoChip(grid, "Empty", "스테이지 클리어·사냥으로 유물 조각을 모으세요", 80f);
                    return;
                }
                foreach (var o in svc.Owned)
                {
                    var def = ContentCatalog.GetArtifact(o.defId);
                    string title = def != null ? def.name : o.defId;
                    string sub = o.equipped ? "장착중" : $"조각 {o.fragments}";
                    string id = o.defId;
                    // 전용 무협 아이콘(ArtifactIcons/{id}) 우선 — 없으면 기존 보석 아이콘
                    var artSp = Resources.Load<Sprite>("ArtifactIcons/" + id);
                    FantasyKitSlots.SkillTile(grid, id, title, sub, artSp != null ? artSp : GrowArt.IconGem, GrowArt.Rarity(o.equipped ? 3 : 1),
                        () =>
                        {
                            _toast(ArtifactService.Instance?.ToggleEquip(id));
                            ArtifactModal.Refresh?.Invoke();
                            _refresh?.Invoke();
                        }, cell);
                }
            };
            ArtifactModal.Refresh();
        }

        void BuildEvent()
        {
            EventModal = _modals.CreateDual("Event", "이벤트", footer: false, leftWidth: 280f);
            MapleLightTheme.SkinDemoPage(EventModal, "Event", wideContent: true);
            var left = EventModal.LeftRail;
            var c = EventModal.Content;

            string[] tabs = { "10일 출석판", "일일 미션", "업적", "도감" };
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;
                var b = MapleUiTheme.SecondaryButton(left, "Tab" + i, tabs[i], () =>
                {
                    _eventTab = idx;
                    ShowEventTab();
                });
                UiKit.Fix(b, -1f, 48f);
            }

            _eventAttend = UiKit.VStack(c, "Attend", 8f, 0, 0, 0, 0).gameObject;
            MapleUiTheme.SectionHeader(_eventAttend.transform, "10일 출석판");
            int streak = IdleMvp.Core.LoginRewardService.Streak;
            float mul = IdleMvp.Core.LoginRewardService.StreakMultiplier;
            string streakInfo = streak >= 3
                ? $"연속 {streak}일 접속 중! 출석 보상 x{mul:0.#}"
                : $"연속 {streak}일 접속 중 (3일부터 보너스!)";
            if (IdleMvp.Core.LoginRewardService.IsReturnLogin)
                streakInfo = "복귀를 환영합니다! 우편함을 확인하세요.";
            MapleUiTheme.InfoChip(_eventAttend.transform, "Hint", streakInfo, 48f);
            var attendPref = new Vector2(150f, 160f);
            var grid = UiKit.FillGrid(_eventAttend.transform, "Days", attendPref, new Vector2(10f, 10f), 4, 5);
            var attendCell = grid.GetComponent<UnityEngine.UI.GridLayoutGroup>().cellSize;
            int claimed = PlayerPrefs.GetInt("IdleGrow.Maple.AttendDay", 0);
            for (int d = 1; d <= 10; d++)
            {
                int day = d;
                bool done = day <= claimed;
                FantasyKitSlots.RewardTile(grid, "D" + d, $"{d}일 차", done ? "완료" : $"{50 * d}",
                    GrowArt.IconGem, GrowArt.Rarity(done ? 2 : 1), attendCell, done ? "OK" : null);
                // Make claimable via SkillTile-style click: wrap with button on last child
                var go = grid.GetChild(grid.childCount - 1);
                var btn = go.gameObject.GetComponent<UnityEngine.UI.Button>() ?? go.gameObject.AddComponent<UnityEngine.UI.Button>();
                var img = go.GetComponent<UnityEngine.UI.Image>();
                if (img != null) btn.targetGraphic = img;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
                    string last = PlayerPrefs.GetString("IdleGrow.Maple.AttendLastDay", "");
                    if (last == today) { _toast("오늘은 이미 출석했습니다"); return; }
                    int cur = PlayerPrefs.GetInt("IdleGrow.Maple.AttendDay", 0);
                    if (day != cur + 1) { _toast(day <= cur ? "이미 수령함" : "순서대로 출석하세요"); return; }
                    PlayerPrefs.SetInt("IdleGrow.Maple.AttendDay", day);
                    PlayerPrefs.SetString("IdleGrow.Maple.AttendLastDay", today);
                    PlayerPrefs.Save();
                    float mul = IdleMvp.Core.LoginRewardService.StreakMultiplier;
                    CurrencyWallet.Instance?.Add(CurrencyId.RedDiamond, (10 + day * 2) * mul);
                    CurrencyWallet.Instance?.Add(CurrencyId.BlueDiamond, day * mul);
                    WalletAdapter.Instance?.AddGold(50 * day * mul);
                    string bonus = mul > 1f ? $" (연속{IdleMvp.Core.LoginRewardService.Streak}일 x{mul:0.#})" : "";
                    _toast($"{day}일 차 보상 수령!{bonus}");
                    _refresh?.Invoke();
                    _modals.Close();
                    _modals.Open(EventModal);
                });
            }

            _eventMission = UiKit.VStack(c, "Mission", 8f, 12, 12, 8, 8).gameObject;
            BuildDailyMissionRows(_eventMission.transform);
            _eventMission.SetActive(false);

            _eventAchv = UiKit.VStack(c, "Achv", 8f, 12, 12, 8, 8).gameObject;
            BuildAchievementRows(_eventAchv.transform);
            _eventAchv.SetActive(false);

            _eventCollection = UiKit.VStack(c, "Collection", 8f, 12, 12, 8, 8).gameObject;
            BuildCollectionRows(_eventCollection.transform);
            _eventCollection.SetActive(false);

            EventModal.Refresh = ShowEventTab;
            ShowEventTab();
        }

        void ShowEventTab()
        {
            if (_eventAttend != null) _eventAttend.SetActive(_eventTab == 0);
            if (_eventMission != null)
            {
                _eventMission.SetActive(_eventTab == 1);
                if (_eventTab == 1) RefreshDailyMissions();
            }
            if (_eventAchv != null)
            {
                _eventAchv.SetActive(_eventTab == 2);
                if (_eventTab == 2) RefreshAchievements();
            }
            if (_eventCollection != null)
            {
                _eventCollection.SetActive(_eventTab == 3);
                if (_eventTab == 3) RefreshCollection();
            }
        }

        TMPro.TMP_Text[] _missionProgLabels;
        UnityEngine.UI.Button[] _missionClaimBtns;
        UnityEngine.UI.Image[] _missionBars;
        UnityEngine.UI.Button _missionAllBtn;
        TMPro.TMP_Text _missionAllLabel;

        void BuildDailyMissionRows(Transform parent)
        {
            MapleUiTheme.SectionHeader(parent, "일일 미션");
            var missions = IdleMvp.Core.DailyMissionService.Missions;
            _missionProgLabels = new TMPro.TMP_Text[missions.Length];
            _missionClaimBtns = new UnityEngine.UI.Button[missions.Length];
            _missionBars = new UnityEngine.UI.Image[missions.Length];

            for (int i = 0; i < missions.Length; i++)
            {
                int idx = i;
                var m = missions[i];
                var chipT = MapleUiTheme.InfoChip(parent, "M" + i, "", 82f);
                var row = chipT.transform.parent; // navy card container
                chipT.gameObject.SetActive(false); // hide default label; we'll add custom layout

                var title = UiKit.TmpLabel(row, "Title", m.Title, UiKit.TmpBody,
                    new Color(1f, 0.85f, 0.35f, 1f), bold: true, TMPro.TextAlignmentOptions.TopLeft);
                title.enableWordWrapping = false;
                var trt = title.rectTransform;
                trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(0.65f, 1f);
                trt.pivot = new Vector2(0f, 1f);
                trt.offsetMin = new Vector2(14f, -30f); trt.offsetMax = new Vector2(0f, -6f);

                var desc = UiKit.TmpLabel(row, "Desc", m.Desc, UiKit.TmpCaption - 1,
                    new Color(1f, 1f, 1f, 0.72f), bold: false, TMPro.TextAlignmentOptions.TopLeft);
                desc.enableWordWrapping = true;
                var drt = desc.rectTransform;
                drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(0.65f, 1f);
                drt.offsetMin = new Vector2(14f, 22f); drt.offsetMax = new Vector2(0f, -32f);

                var progT = UiKit.TmpLabel(row, "Prog", "0/" + m.Goal, UiKit.TmpCaption - 2,
                    new Color(0.55f, 0.85f, 1f, 1f), bold: true, TMPro.TextAlignmentOptions.BottomLeft);
                progT.enableWordWrapping = false;
                var prt = progT.rectTransform;
                prt.anchorMin = new Vector2(0f, 0f); prt.anchorMax = new Vector2(0.4f, 0f);
                prt.pivot = new Vector2(0f, 0f);
                prt.offsetMin = new Vector2(14f, 4f); prt.offsetMax = new Vector2(0f, 22f);
                _missionProgLabels[i] = progT;

                var bar = MapleUiTheme.Bar(row, "Bar", UiKit.ExpColor, withLabel: false);
                var brt = bar.Go.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0.28f, 0f); brt.anchorMax = new Vector2(0.65f, 0f);
                brt.offsetMin = new Vector2(0f, 6f); brt.offsetMax = new Vector2(0f, 16f);
                _missionBars[i] = bar.Fill;

                var claimBtn = MapleUiTheme.YellowButton(row, "Claim", "받기", () =>
                {
                    if (IdleMvp.Core.DailyMissionService.TryClaim(missions[idx].Id))
                    {
                        _toast($"{missions[idx].Title} 보상 수령!");
                        IdleMvp.Core.AudioService.Gold();
                        RefreshDailyMissions();
                        _refresh?.Invoke();
                    }
                }, UiKit.FontCaption);
                var crt = claimBtn.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(1f, 0.5f); crt.anchorMax = new Vector2(1f, 0.5f);
                crt.pivot = new Vector2(1f, 0.5f);
                crt.sizeDelta = new Vector2(90f, 40f);
                crt.anchoredPosition = new Vector2(-10f, 0f);
                _missionClaimBtns[i] = claimBtn;
            }

            MapleUiTheme.SectionHeader(parent, "전체 완료 보상");
            var allT = MapleUiTheme.InfoChip(parent, "AllReward",
                "5개 미션 모두 완료 시 RD 20 추가 지급", 52f);
            var allRow = allT.transform.parent;
            _missionAllBtn = MapleUiTheme.YellowButton(allRow, "AllClaim", "전체 받기", () =>
            {
                if (IdleMvp.Core.DailyMissionService.TryClaimAll())
                {
                    _toast("전체 완료 보상! RD 20 지급");
                    IdleMvp.Core.AudioService.Gold();
                    RefreshDailyMissions();
                    _refresh?.Invoke();
                }
            }, UiKit.FontCaption);
            var art = _missionAllBtn.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(1f, 0.5f); art.anchorMax = new Vector2(1f, 0.5f);
            art.pivot = new Vector2(1f, 0.5f);
            art.sizeDelta = new Vector2(110f, 40f);
            art.anchoredPosition = new Vector2(-10f, 0f);
            _missionAllLabel = _missionAllBtn.GetComponentInChildren<TMPro.TMP_Text>();
        }

        void RefreshDailyMissions()
        {
            var missions = IdleMvp.Core.DailyMissionService.Missions;
            for (int i = 0; i < missions.Length; i++)
            {
                var m = missions[i];
                int prog = IdleMvp.Core.DailyMissionService.Progress(m.Id);
                bool done = prog >= m.Goal;
                bool claimed = IdleMvp.Core.DailyMissionService.IsClaimed(m.Id);
                if (_missionProgLabels[i] != null)
                    _missionProgLabels[i].text = $"{prog}/{m.Goal}";
                if (_missionBars[i] != null)
                    _missionBars[i].fillAmount = Mathf.Clamp01((float)prog / m.Goal);
                if (_missionClaimBtns[i] != null)
                {
                    _missionClaimBtns[i].interactable = done && !claimed;
                    var t = _missionClaimBtns[i].GetComponentInChildren<TMPro.TMP_Text>();
                    if (t != null) t.text = claimed ? "완료" : "받기";
                }
            }
            if (_missionAllBtn != null)
            {
                bool canAll = !IdleMvp.Core.DailyMissionService.AllClaimed
                    && IdleMvp.Core.DailyMissionService.CompletedCount >= missions.Length;
                bool allIndiv = true;
                foreach (var m in missions)
                    if (!IdleMvp.Core.DailyMissionService.IsClaimed(m.Id)) { allIndiv = false; break; }
                _missionAllBtn.interactable = canAll && allIndiv;
                if (_missionAllLabel != null)
                    _missionAllLabel.text = IdleMvp.Core.DailyMissionService.AllClaimed ? "완료" : "전체 받기";
            }
        }

        TMPro.TMP_Text[] _achvProgLabels;
        UnityEngine.UI.Button[] _achvClaimBtns;
        UnityEngine.UI.Image[] _achvBars;
        TMPro.TMP_Text _achvTitleLabel;

        void BuildAchievementRows(Transform parent)
        {
            MapleUiTheme.SectionHeader(parent, "업적");
            var achvs = AchievementService.List;
            _achvProgLabels = new TMPro.TMP_Text[achvs.Length];
            _achvClaimBtns = new UnityEngine.UI.Button[achvs.Length];
            _achvBars = new UnityEngine.UI.Image[achvs.Length];

            AchievementService.Category? lastCat = null;
            for (int i = 0; i < achvs.Length; i++)
            {
                int idx = i;
                var a = achvs[i];
                if (lastCat == null || lastCat.Value != a.Cat)
                {
                    lastCat = a.Cat;
                    string catName = a.Cat switch
                    {
                        AchievementService.Category.Kill => "처치",
                        AchievementService.Category.Stage => "스테이지",
                        AchievementService.Category.Summon => "소환",
                        AchievementService.Category.Enhance => "강화",
                        AchievementService.Category.Level => "레벨",
                        AchievementService.Category.Dungeon => "던전",
                        AchievementService.Category.Arena => "아레나",
                        _ => ""
                    };
                    MapleUiTheme.SectionHeader(parent, catName);
                }

                var chipT = MapleUiTheme.InfoChip(parent, "A" + i, "", 72f);
                var row = chipT.transform.parent;
                chipT.gameObject.SetActive(false);

                var title = UiKit.TmpLabel(row, "Title", a.Title, UiKit.TmpBody,
                    new Color(1f, 0.85f, 0.35f, 1f), bold: true, TMPro.TextAlignmentOptions.TopLeft);
                title.enableWordWrapping = false;
                var trt = title.rectTransform;
                trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(0.6f, 1f);
                trt.pivot = new Vector2(0f, 1f);
                trt.offsetMin = new Vector2(14f, -28f); trt.offsetMax = new Vector2(0f, -4f);

                string rewardDesc = a.RewardAmount + " " + CurrencyName(a.RewardCurrency);
                if (!string.IsNullOrEmpty(a.TitleReward)) rewardDesc += $" + 칭호 \"{a.TitleReward}\"";
                var desc = UiKit.TmpLabel(row, "Desc", $"목표: {a.Goal} · 보상: {rewardDesc}", UiKit.TmpCaption - 2,
                    new Color(1f, 1f, 1f, 0.65f), bold: false, TMPro.TextAlignmentOptions.TopLeft);
                desc.enableWordWrapping = true;
                var drt = desc.rectTransform;
                drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(0.6f, 1f);
                drt.offsetMin = new Vector2(14f, 16f); drt.offsetMax = new Vector2(0f, -30f);

                var progT = UiKit.TmpLabel(row, "Prog", "0/" + a.Goal, UiKit.TmpCaption - 2,
                    new Color(0.55f, 0.85f, 1f, 1f), bold: true, TMPro.TextAlignmentOptions.BottomLeft);
                progT.enableWordWrapping = false;
                var prt = progT.rectTransform;
                prt.anchorMin = new Vector2(0f, 0f); prt.anchorMax = new Vector2(0.4f, 0f);
                prt.pivot = new Vector2(0f, 0f);
                prt.offsetMin = new Vector2(14f, 2f); prt.offsetMax = new Vector2(0f, 18f);
                _achvProgLabels[i] = progT;

                var bar = MapleUiTheme.Bar(row, "Bar", new Color(0.4f, 0.9f, 0.4f, 1f), withLabel: false);
                var brt = bar.Go.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0.25f, 0f); brt.anchorMax = new Vector2(0.6f, 0f);
                brt.offsetMin = new Vector2(0f, 4f); brt.offsetMax = new Vector2(0f, 14f);
                _achvBars[i] = bar.Fill;

                var claimBtn = MapleUiTheme.YellowButton(row, "Claim", "받기", () =>
                {
                    if (AchievementService.TryClaim(achvs[idx].Id))
                    {
                        _toast($"업적 달성! {achvs[idx].Title}");
                        AudioService.Gold();
                        RefreshAchievements();
                        _refresh?.Invoke();
                    }
                }, UiKit.FontCaption);
                var crt = claimBtn.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(1f, 0.5f); crt.anchorMax = new Vector2(1f, 0.5f);
                crt.pivot = new Vector2(1f, 0.5f);
                crt.sizeDelta = new Vector2(90f, 36f);
                crt.anchoredPosition = new Vector2(-10f, 0f);
                _achvClaimBtns[i] = claimBtn;
            }

            MapleUiTheme.SectionHeader(parent, "칭호");
            var titleChip = MapleUiTheme.InfoChip(parent, "TitleDisp",
                "업적 달성 시 칭호를 획득합니다", 44f);
            _achvTitleLabel = titleChip.GetComponent<TMPro.TMP_Text>() ?? titleChip.GetComponentInChildren<TMPro.TMP_Text>();
        }

        void RefreshAchievements()
        {
            AchievementService.SyncFromGameState();
            var achvs = AchievementService.List;
            for (int i = 0; i < achvs.Length; i++)
            {
                var a = achvs[i];
                int prog = AchievementService.Progress(a.Id);
                bool done = prog >= a.Goal;
                bool claimed = AchievementService.IsClaimed(a.Id);
                if (_achvProgLabels != null && _achvProgLabels[i] != null)
                    _achvProgLabels[i].text = $"{prog}/{a.Goal}";
                if (_achvBars != null && _achvBars[i] != null)
                    _achvBars[i].fillAmount = Mathf.Clamp01((float)prog / a.Goal);
                if (_achvClaimBtns != null && _achvClaimBtns[i] != null)
                {
                    _achvClaimBtns[i].interactable = done && !claimed;
                    var t = _achvClaimBtns[i].GetComponentInChildren<TMPro.TMP_Text>();
                    if (t != null) t.text = claimed ? "완료" : "받기";
                }
            }
            if (_achvTitleLabel != null)
            {
                string active = AchievementService.ActiveTitle;
                var earned = AchievementService.EarnedTitles;
                if (earned.Length == 0)
                    _achvTitleLabel.text = "아직 획득한 칭호가 없습니다";
                else
                    _achvTitleLabel.text = $"보유 칭호: {string.Join(", ", earned)}" +
                        (string.IsNullOrEmpty(active) ? "" : $"\n사용 중: {active}");
            }
        }

        TMPro.TMP_Text _collBonusLabel;
        TMPro.TMP_Text[] _collMonsterLabels;
        TMPro.TMP_Text[] _collWeaponLabels;
        TMPro.TMP_Text[] _collCompLabels;

        void BuildCollectionRows(Transform parent)
        {
            MapleUiTheme.SectionHeader(parent, "도감");

            var bonusChip = MapleUiTheme.InfoChip(parent, "Bonus", "수집 보너스: 계산 중...", 44f);
            _collBonusLabel = bonusChip.GetComponent<TMPro.TMP_Text>() ?? bonusChip.GetComponentInChildren<TMPro.TMP_Text>();

            MapleUiTheme.SectionHeader(parent, "몬스터");
            var monsters = CollectionService.GetMonsterEntries();
            _collMonsterLabels = new TMPro.TMP_Text[monsters.Length];
            for (int i = 0; i < monsters.Length; i++)
            {
                var chipT = MapleUiTheme.InfoChip(parent, "Mon" + i, monsters[i].Name, 38f);
                _collMonsterLabels[i] = chipT.GetComponent<TMPro.TMP_Text>() ?? chipT.GetComponentInChildren<TMPro.TMP_Text>();
            }

            MapleUiTheme.SectionHeader(parent, "무기");
            var weapons = CollectionService.GetWeaponEntries();
            _collWeaponLabels = new TMPro.TMP_Text[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
            {
                var chipT = MapleUiTheme.InfoChip(parent, "Wpn" + i, weapons[i].Name, 38f);
                _collWeaponLabels[i] = chipT.GetComponent<TMPro.TMP_Text>() ?? chipT.GetComponentInChildren<TMPro.TMP_Text>();
            }

            MapleUiTheme.SectionHeader(parent, "동료");
            var comps = CollectionService.GetCompanionEntries();
            _collCompLabels = new TMPro.TMP_Text[comps.Length];
            for (int i = 0; i < comps.Length; i++)
            {
                var chipT = MapleUiTheme.InfoChip(parent, "Comp" + i, comps[i].Name, 38f);
                _collCompLabels[i] = chipT.GetComponent<TMPro.TMP_Text>() ?? chipT.GetComponentInChildren<TMPro.TMP_Text>();
            }
        }

        void RefreshCollection()
        {
            CollectionService.SyncFromOwnedData();

            if (_collBonusLabel != null)
            {
                float atk = CollectionService.BonusAtkPct;
                float hp = CollectionService.BonusHpPct;
                float gold = CollectionService.BonusGoldPct;
                _collBonusLabel.text = $"수집 보너스 — ATK +{atk:0.#}%  HP +{hp:0.#}%  골드 +{gold:0.#}%";
            }

            var monsters = CollectionService.GetMonsterEntries();
            for (int i = 0; i < monsters.Length && i < _collMonsterLabels.Length; i++)
            {
                bool got = CollectionService.IsCollected(monsters[i].Id);
                if (_collMonsterLabels[i] != null)
                {
                    _collMonsterLabels[i].text = got ? $"✓ {monsters[i].Name}" : $"? {monsters[i].Name}";
                    _collMonsterLabels[i].color = got ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }

            var weapons = CollectionService.GetWeaponEntries();
            for (int i = 0; i < weapons.Length && i < _collWeaponLabels.Length; i++)
            {
                bool got = CollectionService.IsCollected(weapons[i].Id);
                if (_collWeaponLabels[i] != null)
                {
                    _collWeaponLabels[i].text = got ? $"✓ {weapons[i].Name}" : $"? {weapons[i].Name}";
                    _collWeaponLabels[i].color = got ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }

            var comps = CollectionService.GetCompanionEntries();
            for (int i = 0; i < comps.Length && i < _collCompLabels.Length; i++)
            {
                bool got = CollectionService.IsCollected(comps[i].Id);
                if (_collCompLabels[i] != null)
                {
                    _collCompLabels[i].text = got ? $"✓ {comps[i].Name}" : $"? {comps[i].Name}";
                    _collCompLabels[i].color = got ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }
        }

        static string CurrencyName(CurrencyId id) => id switch
        {
            CurrencyId.Gold => "골드",
            CurrencyId.RedDiamond => "RD",
            CurrencyId.WeaponTicket => "무기권",
            CurrencyId.WeaponEnhanceStone => "강화석",
            CurrencyId.BlueDiamond => "BD",
            _ => id.ToString()
        };

        void BuildDungeon()
        {
            DungeonModal = _modals.CreateDual("Dungeon", "성장 던전", footer: true, leftWidth: 24f);
            MapleLightTheme.SkinDemoPage(DungeonModal, "Dungeon", wideContent: true);
            var leftPanel = DungeonModal.Go.transform.Find("DungeonLeft");
            if (leftPanel != null) leftPanel.gameObject.SetActive(false);
            var c = DungeonModal.Content;

            string[] names =
            {
                DungeonService.Instance != null ? DungeonService.Instance.NameOf(DungeonId.GoldTemple) : "황금 신전",
                DungeonService.Instance != null ? DungeonService.Instance.NameOf(DungeonId.EnhanceTower) : "강화의 탑",
                DungeonService.Instance != null ? DungeonService.Instance.NameOf(DungeonId.EquipmentRoom) : "장비의 방",
                DungeonService.Instance != null ? DungeonService.Instance.NameOf(DungeonId.TrainingGround) : "수련장"
            };
            string[] doorArts = { "Image_StageTheme_Forest", "Image_StageTheme_Beach", "Image_StageTheme_City", "Image_StageTheme_Forest" };
            var doorRow = UiKit.HStack(c, "Doors", 34f, 0, 0, 8, 8, TextAnchor.MiddleCenter);
            UiKit.Fix(doorRow, -1f, 396f);
            _dungeonDoorSel = new Image[names.Length];
            _dungeonDoorTickets = new TMPro.TMP_Text[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                int idx = i;
                // demo Stage door: navy round card + theme art + name under, sky border when selected
                var col = UiKit.VStack(doorRow, "Door" + i, 6f, 0, 0, 0, 0, TextAnchor.UpperCenter);
                var colLe = col.gameObject.AddComponent<LayoutElement>();
                colLe.preferredWidth = 320f; colLe.minWidth = 320f;
                colLe.flexibleWidth = 0f;
                var cardBg = UiKit.Img(col, "Card", new Color(0.075f, 0.14f, 0.32f, 1f));
                cardBg.sprite = CasualArt.CardRound;
                cardBg.type = UnityEngine.UI.Image.Type.Sliced;
                UiKit.Fix(cardBg, 320f, 300f);
                var doorBtn = cardBg.gameObject.AddComponent<Button>();
                doorBtn.targetGraphic = cardBg;
                UiKit.Press(doorBtn);
                doorBtn.onClick.AddListener(IdleMvp.Core.AudioService.Click);
                doorBtn.onClick.AddListener(() => { _dungeonSel = idx; RefreshDungeon(); });
                var doorArt = UiKit.Img(cardBg.transform, "Art", Color.white);
                doorArt.sprite = CasualArt.C(doorArts[i]);
                doorArt.preserveAspect = true;
                doorArt.raycastTarget = false;
                UiKit.Fill(doorArt.rectTransform, 14f);
                // selection = sky halo backdrop behind the card (border sprites are filled)
                var sel = UiKit.Img(cardBg.transform, "Sel", new Color(0.35f, 0.85f, 1f, 0.85f));
                sel.sprite = MapleLightTheme.RoundedSprite(18);
                sel.type = UnityEngine.UI.Image.Type.Sliced;
                sel.raycastTarget = false;
                UiKit.Fill(sel.rectTransform, -8f);
                sel.transform.SetAsFirstSibling();
                sel.enabled = false;
                _dungeonDoorSel[i] = sel;
                var nameT = UiKit.TmpLabel(col, "Name", names[i], UiKit.TmpBody + 2, Color.white,
                    bold: true, TMPro.TextAlignmentOptions.Center);
                nameT.enableWordWrapping = false;
                UiKit.Fix(nameT, -1f, 40f);
                var tickT = UiKit.TmpLabel(col, "Tickets", "", UiKit.TmpCaption, new Color(0.55f, 0.85f, 1f, 1f),
                    bold: true, TMPro.TextAlignmentOptions.Center);
                tickT.enableWordWrapping = false;
                UiKit.Fix(tickT, -1f, 30f);
                _dungeonDoorTickets[i] = tickT;
            }

            _dungeonDetail = MapleUiTheme.InfoChip(c, "Detail", "", 64f);
            MapleLightTheme.Section(c, "예상 보상");
            var rewardRow = UiKit.HStack(c, "Rewards", 10f, 8, 8, 8, 8, TextAnchor.MiddleLeft);
            UiKit.Fix(rewardRow, -1f, 184f);
            _dungeonRewardHost = rewardRow;

            var adKey = MapleUiTheme.SecondaryButton(DungeonModal.Footer, "AdKey", "광고 열쇠 +1", () =>
            {
                var ds2 = DungeonService.Instance;
                if (ds2 == null) return;
                var id2 = SelectedDungeon();
                if (!ds2.AdKeyAvailable(id2)) { _toast("오늘은 이미 광고 열쇠를 받았습니다"); return; }
                IdleMvp.Economy.AdBridge.Instance?.ShowRewarded("dungeon_key",
                    () => { _toast(ds2.GrantAdKey(id2)); _refresh?.Invoke(); RefreshDungeon(); },
                    err => _toast(err ?? "광고 실패"));
            });
            UiKit.Fix(adKey, 190f, 60f);
            var sweep = MapleUiTheme.PrimaryButton(DungeonModal.Footer, "Sweep", "소탕", () => RunDungeon(true));
            UiKit.Fix(sweep, 180f, 60f);
            var challenge = MapleUiTheme.AccentButton(DungeonModal.Footer, "Go", "도전", () => RunDungeon(false));
            UiKit.Fix(challenge, 200f, 60f);

            DungeonModal.Refresh = RefreshDungeon;
            RefreshDungeon();
        }

        Image[] _dungeonDoorSel;
        TMPro.TMP_Text[] _dungeonDoorTickets;

        static readonly DungeonId[] DungeonIds =
            { DungeonId.GoldTemple, DungeonId.EnhanceTower, DungeonId.EquipmentRoom, DungeonId.TrainingGround };

        DungeonId SelectedDungeon() => DungeonIds[Mathf.Clamp(_dungeonSel, 0, DungeonIds.Length - 1)];

        void RefreshDungeon()
        {
            var id = SelectedDungeon();
            int i = (int)id;
            var ds = DungeonService.Instance;
            string name = ds != null ? ds.NameOf(id) : id.ToString();
            int tickets = ds != null ? ds.TicketsLeft[(int)id] : 0;
            int stage = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
            float scale = 1f + stage * 0.08f;
            if (_dungeonDetail != null)
                _dungeonDetail.text = $"{name} · 열쇠 {tickets}/{DungeonService.KeyCap} (매일 +{DungeonService.KeysPerDay}) · 배율 ×{scale:0.00}";
            if (_dungeonDoorSel != null)
                for (int d = 0; d < _dungeonDoorSel.Length; d++)
                    if (_dungeonDoorSel[d] != null) _dungeonDoorSel[d].enabled = d == i;
            if (_dungeonDoorTickets != null && ds != null)
                for (int d = 0; d < _dungeonDoorTickets.Length; d++)
                    if (_dungeonDoorTickets[d] != null) _dungeonDoorTickets[d].text = $"티켓 {ds.TicketsLeft[d]}";

            if (_dungeonRewardHost != null)
            {
                for (int c = _dungeonRewardHost.childCount - 1; c >= 0; c--)
                    Object.Destroy(_dungeonRewardHost.GetChild(c).gameObject);
                var cell = new Vector2(150f, 168f);
                switch (id)
                {
                    case DungeonId.GoldTemple:
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "G", "골드", UiKit.Num(80 * scale), GrowArt.IconGold, GrowArt.Rarity(1), cell);
                        break;
                    case DungeonId.EnhanceTower:
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "S", "강화석", (3 * scale).ToString("0.#"), GrowArt.IconStone, GrowArt.Rarity(0), cell);
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "Sc", "주문흔적", (5 * scale).ToString("0"), GrowArt.IconPlus, GrowArt.Rarity(1), cell);
                        break;
                    case DungeonId.TrainingGround:
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "Tk", "수련 증표", (6 * scale).ToString("0"), GrowArt.IconPlus, GrowArt.Rarity(2), cell);
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "Md", "명성 훈장", System.Math.Max(1, System.Math.Floor(scale)).ToString("0"), GrowArt.IconCheck, GrowArt.Rarity(3), cell);
                        break;
                    default:
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "X", "경험치", UiKit.Num(15 * scale), GrowArt.IconXp, GrowArt.Rarity(1), cell);
                        FantasyKitSlots.RewardTile(_dungeonRewardHost, "T", "무기티켓", "1", GrowArt.IconSummonWeapon, GrowArt.Rarity(2), cell);
                        break;
                }
            }
        }

        void RunDungeon(bool sweep)
        {
            var ds = DungeonService.Instance;
            if (ds == null) { _toast("던전 서비스 없음"); return; }
            var id = SelectedDungeon();
            if (ds.TryRun(id, PlayerGrowth.Instance, PlayerWallet.Instance, EquipmentService.Instance, out string msg))
                _toast((sweep ? "소탕 성공 · " : "도전 완료 · ") + msg);
            else
                _toast(msg);
            _refresh?.Invoke();
            RefreshDungeon();
        }

        void BuildChat(Transform hudRoot)
        {
            ChatPanel = new GameObject("ChatPanel", typeof(RectTransform));
            ChatPanel.transform.SetParent(hudRoot, false);
            var rt = ChatPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.08f);
            rt.anchorMax = new Vector2(0.38f, 0.92f);
            rt.offsetMin = new Vector2(8f, 8f);
            rt.offsetMax = new Vector2(-8f, -8f);
            var bg = ChatPanel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.13f, 0.15f, 0.92f);
            FantasyKitSlots.Slice(bg, GrowArt.ModalFrame);

            var root = UiKit.HStack(ChatPanel.transform, "Root", 6f, 8, 8, 8, 8, TextAnchor.UpperLeft);
            UiKit.Fill(root);

            var tabs = UiKit.VStack(root, "Tabs", 4f, 0, 0, 0, 0);
            UiKit.Fix(tabs, 72f, -1f);
            string[] ch = { "전체", "지역", "월드", "길드", "파티", "귓속말" };
            for (int i = 0; i < ch.Length; i++)
            {
                int idx = i;
                var b = MapleUiTheme.SecondaryButton(tabs, "C" + i, ch[i], () => _toast($"{ch[idx]} 채널"), UiKit.FontCaption);
                UiKit.Fix(b, -1f, 40f);
            }

            var main = UiKit.VStack(root, "Main", 6f, 0, 0, 0, 0);
            UiKit.Flex(main);
            var header = UiKit.HStack(main, "H", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(header, -1f, 40f);
            var title = UiKit.Label(header, "T", "채팅", UiKit.FontH2, UiKit.TextInverse, FontStyle.Bold);
            UiKit.Flex(title);
            var close = MapleUiTheme.SecondaryButton(header, "X", "닫기", () => ChatPanel.SetActive(false), UiKit.FontCaption);
            UiKit.Fix(close, 72f, 36f);

            var scrollGo = new GameObject("LogScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(main, false);
            var srt = scrollGo.GetComponent<RectTransform>();
            var sle = scrollGo.AddComponent<LayoutElement>();
            sle.flexibleHeight = 1f;
            sle.minHeight = 200f;
            UiKit.Fill(srt);
            var viewport = UiKit.Img(scrollGo.transform, "VP", new Color(1, 1, 1, 0.02f));
            UiKit.Fill(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();
            _chatLog = UiKit.VStack(viewport.transform, "Log", 4f, 4, 4, 4, 4);
            var crt = _chatLog.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            var fitter = _chatLog.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = crt;
            scroll.horizontal = false;

            AddChatLine("시스템", "채팅에 오신 것을 환영합니다.");
            AddChatLine("모험가A", "강호에서 사냥 중!");

            var inputRow = UiKit.HStack(main, "Input", 6f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(inputRow, -1f, 48f);
            var inputGo = new GameObject("Field", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(inputRow, false);
            var ig = inputGo.GetComponent<Image>();
            ig.color = new Color(0f, 0f, 0f, 0.35f);
            UiKit.Flex(inputGo.GetComponent<RectTransform>());
            var le = inputGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = 40f;
            var placeholder = UiKit.Label(inputGo.transform, "Ph", "메시지를 입력해주세요", UiKit.FontCaption, UiKit.TextInverseDim);
            UiKit.Fill(placeholder.rectTransform, 8f);
            var text = UiKit.Label(inputGo.transform, "Text", "", UiKit.FontBody, UiKit.TextInverse);
            UiKit.Fill(text.rectTransform, 8f);
            _chatInput = inputGo.GetComponent<InputField>();
            _chatInput.textComponent = text;
            _chatInput.placeholder = placeholder;
            var send = MapleUiTheme.AccentButton(inputRow, "Send", "전송", () =>
            {
                if (_chatInput == null || string.IsNullOrEmpty(_chatInput.text)) return;
                AddChatLine("나", _chatInput.text);
                _chatInput.text = "";
            });
            UiKit.Fix(send, 80f, 40f);

            ChatPanel.SetActive(false);
        }

        void AddChatLine(string who, string msg)
        {
            if (_chatLog == null) return;
            var line = MapleUiTheme.InfoChip(_chatLog, "L", $"[{who}] {msg}", 40f);
            line.alignment = TextAnchor.MiddleLeft;
        }

        public void ToggleChat()
        {
            if (ChatPanel == null) return;
            ChatPanel.SetActive(!ChatPanel.activeSelf);
        }

        void BuildFaction()
        {
            FactionModal = _modals.Create("Faction", "세력 선택", ModalSize.Medium, footer: false);
            // 입문/전향 후에는 화면 구성이 통째로 달라지므로 열 때마다 다시 그린다.
            // (재빌드하지 않으면 이미 입문했는데도 '입문' 버튼이 남아 재선택돼 성향이 초기화된다)
            FactionModal.Refresh = RebuildFactionContent;
            RebuildFactionContent();
        }

        string _factionBuiltFor = " "; // 아직 안 지음 (빈 문자열 = 미선택 상태와 구분)

        void RebuildFactionContent()
        {
            // Refresh는 모달이 열려있는 동안 전역 갱신마다 불린다.
            // 세력 상태가 그대로면 다시 그리지 않는다 (깜빡임 방지)
            string key = Core.FactionService.Selected + "|" + Core.FactionService.Previous
                + "|" + Core.FactionService.CanChangeFaction;
            if (key == _factionBuiltFor) return;
            _factionBuiltFor = key;

            var c = FactionModal.Content;
            for (int i = c.childCount - 1; i >= 0; i--)
            {
                var old = c.GetChild(i);
                old.SetParent(null, false);   // Destroy는 프레임 끝에 처리되므로 먼저 떼어낸다
                UnityEngine.Object.Destroy(old.gameObject);
            }

            // 어두운 남색 모달 위이므로 밝은 글씨를 쓴다 (TextPrimary/Secondary는 안 보임)
            var headCol = new Color(0.95f, 0.96f, 0.98f);
            var bodyCol = new Color(0.80f, 0.84f, 0.90f);
            if (!Core.FactionService.HasSelected)
            {
                UiKit.Label(c, "Title", "당신의 길을 선택하십시오", UiKit.FontH2, headCol, FontStyle.Bold);
                UiKit.Label(c, "Desc", "레벨 6 달성! 세 세력 중 하나에 입문하여\n전용 무공과 스킬 트리를 해금합니다.",
                    UiKit.FontBody, bodyCol);
            }
            else
            {
                string cur = Core.FactionService.DisplayName;
                string synergy = Core.FactionService.SynergyName;
                string info = $"현재 세력: {cur}";
                if (synergy != null) info += $"  ·  이형무공: {synergy} (+15% 데미지)";
                UiKit.Label(c, "Title", "파천 — 세력 전향", UiKit.FontH2, headCol, FontStyle.Bold);
                UiKit.Label(c, "Desc", info + "\n레벨 30 이상 시 다른 세력으로 전향할 수 있습니다.\n이전 세력의 무공은 보존됩니다.",
                    UiKit.FontBody, bodyCol);
            }

            var row = UiKit.HStack(c, "Factions", 12f, 8, 8, 8, 8, TextAnchor.UpperCenter, true);
            UiKit.Fix(row, -1f, 300f);

            BuildFactionCard(row, "정파", "hero",
                "정의와 협의를 추구하는 무림의 정통파.\n태극기공 · 매화검기 · 금강불괴",
                new Color(0.15f, 0.35f, 0.65f));
            BuildFactionCard(row, "사파", "bowmaster",
                "실리와 자유를 좇는 녹림의 암살자.\n야행심법 · 비영출혈도 · 만독비술",
                new Color(0.45f, 0.15f, 0.55f));
            BuildFactionCard(row, "마도", "archmage",
                "금기의 마공을 수련하는 마도의 구도자.\n마화공 · 혈무폭쇄 · 마신강림",
                new Color(0.6f, 0.12f, 0.15f));
        }

        void BuildFactionCard(Transform parent, string name, string treeId, string desc, Color accent)
        {
            var card = UiKit.VStack(parent, name, 8f, 12, 12, 12, 12);
            UiKit.Flex(card.GetComponent<RectTransform>());
            var bg = card.gameObject.AddComponent<Image>();
            bool isCurrent = Core.FactionService.Selected == treeId;
            // 키트 카드 프레임 + 짙은 세력색 (스프라이트 없이 색만 주면 맹물 사각형이 된다)
            bg.sprite = CasualArt.CardRound != null
                ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(12);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(accent.r * 0.35f, accent.g * 0.35f, accent.b * 0.35f,
                isCurrent ? 0.95f : 0.8f);

            var nameCol = Color.Lerp(accent, Color.white, 0.6f);
            var descCol = new Color(0.88f, 0.90f, 0.94f);

            string label = name + (isCurrent ? " (현재)" : "");
            UiKit.Label(card.transform, "Name", label, UiKit.FontH1, nameCol, FontStyle.Bold);
            var descLbl = UiKit.Label(card.transform, "Desc", desc, UiKit.FontCaption, descCol);
            UiKit.Fix(descLbl, -1f, 52f);
            UiKit.Spacer(card.transform); // 버튼을 카드 하단으로 밀어낸다

            if (!Core.FactionService.HasSelected)
            {
                var btn = MapleUiTheme.PrimaryButton(card.transform, "Sel" + name, "입문", () =>
                {
                    Core.FactionService.SelectFaction(treeId);
                    _toast($"{name}에 입문하였습니다!");
                    _modals.Close();
                    _refresh();
                });
                UiKit.Fix(btn, -1f, 56f);
                var btnImg = btn.GetComponent<Image>();
                if (btnImg != null) btnImg.color = accent;
            }
            else if (!isCurrent)
            {
                string btnLabel = Core.FactionService.CanChangeFaction ? "파천" : "Lv30 해금";
                var btn = MapleUiTheme.PrimaryButton(card.transform, "Chg" + name, btnLabel, () =>
                {
                    string err = Core.FactionService.ChangeFaction(treeId);
                    if (err != null) { _toast(err); return; }
                    _toast($"{name}(으)로 전향하였습니다!");
                    _modals.Close();
                    _refresh();
                });
                UiKit.Fix(btn, -1f, 56f);
                var btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                    btnImg.color = Core.FactionService.CanChangeFaction ? accent : Color.gray;
            }
        }

        void BuildFated()
        {
            FatedModal = _modals.Create("Fated", "기연", ModalSize.Small, footer: false);
            var c = FatedModal.Content;

            var gold = new Color(0.85f, 0.72f, 0.2f);
            UiKit.Label(c, "Title", "기연 발동!", UiKit.FontH1, gold, FontStyle.Bold);
            UiKit.Label(c, "Desc", "강호를 떠돌던 중 신비로운 기연이 찾아왔습니다.\n숨겨진 무공의 비급을 발견했습니다!",
                UiKit.FontBody, UiKit.TextPrimary);

            string hidden = Core.FatedEventService.HiddenJobName;
            UiKit.Label(c, "JobName", $"히든 무공: {hidden}", UiKit.FontH2, gold);
            UiKit.Label(c, "JobDesc", "전용 스킬 트리 4종 해금\n기존 무공과 병행 수련 가능",
                UiKit.FontCaption, UiKit.TextSecondary);

            MapleUiTheme.PrimaryButton(c, "Accept", "수락 — 비급 습득", () =>
            {
                Core.FatedEventService.Instance?.Accept();
                _toast($"기연 · {hidden} 해금!");
                _modals.Close();
                _refresh();
            });
            MapleUiTheme.SecondaryButton(c, "Dismiss", "거절 — 블루다이아 50", () =>
            {
                Core.FatedEventService.Instance?.Dismiss();
                _toast("기연을 거절하고 보상을 획득했습니다.");
                _modals.Close();
                _refresh();
            });
        }
        Text _rebirthCountLabel, _rebirthBonusLabel, _rebirthPreviewLabel;
        UnityEngine.UI.Button _rebirthBtn;
        UnityEngine.UI.Button[] _shopBtns;

        void BuildRebirth()
        {
            // 상점 3칸까지 들어가므로 Small은 넘친다
            RebirthModal = _modals.Create("Rebirth", "환생", ModalSize.Medium, footer: false);
            var c = RebirthModal.Content;

            var gold = new Color(0.85f, 0.72f, 0.2f);
            // 어두운 남색 모달 위이므로 밝은 글씨를 쓴다 (TextSecondary는 안 보임)
            var bodyCol = new Color(0.86f, 0.89f, 0.94f);
            UiKit.Label(c, "Title", "환생", UiKit.FontH1, gold, FontStyle.Bold);
            var d = UiKit.Label(c, "Desc",
                "현재의 성장을 내려놓고 더 강한 모습으로 돌아옵니다.\n레벨·스테이지·골드가 초기화되지만\n장비·무기·동료·세력은 유지됩니다.",
                UiKit.FontCaption, bodyCol);
            UiKit.Fix(d, -1f, 64f);

            // InfoChip은 레거시 UI.Text를 반환한다 (TMP 아님)
            _rebirthCountLabel = MapleUiTheme.InfoChip(c, "Count", "", 44f);
            _rebirthBonusLabel = MapleUiTheme.InfoChip(c, "Bonus", "", 60f);
            _rebirthPreviewLabel = MapleUiTheme.InfoChip(c, "Preview", "", 44f);

            _rebirthBtn = MapleUiTheme.PrimaryButton(c, "DoRebirth", "환생하기", () =>
            {
                var svc = Core.RebirthService.Instance;
                if (svc == null || !svc.CanRebirth) { _toast("스테이지 50 이상 도달 시 해금"); return; }
                svc.TryRebirth();
                _toast($"환생 {svc.Count}회 완료! 더 강해진 모습으로...");
                Core.AudioService.Gold();
                _modals.Close();
                _refresh();
            });

            UiKit.Label(c, "ShopTitle", "환생석 상점", UiKit.FontH2, gold, FontStyle.Bold);
            var shop = Core.RebirthService.Shop;
            _shopBtns = new UnityEngine.UI.Button[shop.Length];
            for (int i = 0; i < shop.Length; i++)
            {
                var item = shop[i];
                _shopBtns[i] = MapleUiTheme.PrimaryButton(c, $"Shop_{item.Id}", $"{item.Name} (석 {item.Cost})", () =>
                {
                    var svc = Core.RebirthService.Instance;
                    if (svc == null) return;
                    string err = svc.TryBuyShop(item.Id);
                    if (err != null) { _toast(err); return; }
                    _toast($"{item.Name} 구매 완료!");
                    Core.AudioService.Gem();
                    RefreshRebirth();
                }, UiKit.FontCaption);
            }

            RebirthModal.Refresh = RefreshRebirth;
        }

        void RefreshRebirth()
        {
            var svc = Core.RebirthService.Instance;
            if (svc == null) return;

            if (_rebirthCountLabel != null)
                _rebirthCountLabel.text = $"환생 횟수: {svc.Count}회  |  환생석: {svc.Stones}개";

            if (_rebirthBonusLabel != null)
            {
                if (svc.Count == 0 && svc.Stones == 0)
                    _rebirthBonusLabel.text = "현재 보너스 없음 (첫 환생 시 적용)";
                else
                {
                    string crit = svc.CritBonus > 0f ? $"  치명 +{svc.CritBonus:0.#}%" : "";
                    _rebirthBonusLabel.text =
                        $"영구 보너스 — ATK +{(svc.AtkMul - 1f) * 100f:0}%  HP +{(svc.HpMul - 1f) * 100f:0}%\n" +
                        $"골드 +{(svc.GoldMul - 1f) * 100f:0}%  경험치 +{(svc.XpMul - 1f) * 100f:0}%{crit}";
                }
            }

            if (_rebirthPreviewLabel != null)
            {
                if (svc.CanRebirth)
                    _rebirthPreviewLabel.text = $"환생 시 환생석 +{svc.StonesOnRebirth}개 획득";
                else
                    _rebirthPreviewLabel.text = "스테이지 50 이상 도달 시 환생 가능";
            }

            if (_rebirthBtn != null)
                _rebirthBtn.interactable = svc.CanRebirth;

            if (_shopBtns != null)
            {
                var shop = Core.RebirthService.Shop;
                for (int i = 0; i < _shopBtns.Length && i < shop.Length; i++)
                {
                    if (_shopBtns[i] != null)
                        _shopBtns[i].interactable = svc.Stones >= shop[i].Cost;
                }
            }
        }
    }
}

