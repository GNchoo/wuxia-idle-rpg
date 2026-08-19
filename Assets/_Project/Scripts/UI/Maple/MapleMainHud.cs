using IdleMvp.Adapters;
using IdleMvp.Boot;
using IdleMvp.Combat;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using IdleMvp.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// Landscape idle-RPG main HUD + modals, laid out with UiKit stacks
    /// (top bar / field / bottom dock skeleton) and widget-based modals.
    /// </summary>
    public class MapleMainHud : MonoBehaviour
    {
        FieldAutoHuntController _battle;
        RectTransform _fieldRt;

        // top bar
        TMP_Text _lvText, _cpText, _stageLabel;
        ChipView _goldChip, _gemChip, _blueChip, _ticketChip;
        BarView _stageBar;
        Button _challengeBtn;
        TMP_Text _challengeLabel, _speedLabel;

        // quest / status
        TMP_Text _questText, _statusText;

        // bottom dock
        TMP_Text _nameText;
        BarView _hpBar, _mpBar, _expBar;
        Image[] _navBgs;
        Button _autoBtn;
        TMP_Text _autoLabel;
        Image _autoImg;

        // skill dock
        readonly TMP_Text[] _skillCd = new TMP_Text[4];
        readonly Image[] _skillCdFill = new Image[4];

        // modals
        MapleModalHost _modals;
        ModalView _charModal, _equipModal, _skillModal, _weaponModal, _compModal;
        ModalView _potionEnhModal;
        ModalView _trainingModal;
        StatRowView[] _trainRows;
        UnityEngine.UI.Text _trainHeaderText, _trainAbilityText;   // InfoText는 레거시 Text
        ModalView _collectionModal;
        UnityEngine.UI.Text _collectionText;
        ModalView _townModal;
        ModalView _shopModal, _fastHuntModal, _offlineModal, _costumeModal, _jobModal, _serverModal, _stubModal, _menuModal;
        ModalView _mailModal, _guildModal, _arenaModal, _raidModal, _mapModal;
        MapleExtraScreens _extra;
        MapleAppearanceScreen _appearance;
        ModalView _hotDealModal, _eventModal, _dungeonModal, _artifactModal, _factionModal, _fatedModal, _rebirthModal;
        Text _stubBody;
        Text _mailInfo, _guildInfo, _arenaInfo, _raidInfo, _mapInfo;
        Image _guildDot, _mailDot;
        Image[] _navDots;
        int _lastPkgToastLevel = -1;

        // char modal rows
        StatRowView _cpRow, _gradeRow, _pointRow, _atkRow, _hpRow, _defRow, _talentRow;
        StatRowView _gradeProgress;
        // equip modal
        Text _enhanceInfo, _eliteInfo;
        ChipView _equipGoldChip, _equipStoneChip, _equipScrollChip, _equipSfChip;
        Text _equipCpBanner;
        Button _equipUpgradeBtn;
        Image _equipPreviewFace;
        // skill modal
        StatRowView[] _skillRows;
        StatRowView[] _skillEnhanceRows;
        StatRowView _skillTalentRow;
        StatRowView _passDmgRow, _passGoldRow, _passIdleRow;
        StatRowView _specDmgRow, _specGoldRow, _specIdleRow;
        StatRowView _potionEnhRow;
        Text _charPointsBanner;
        SkillDetailView _skillDetail;
        int _skillSelected;
        Text _skillTreeHint, _skillCpBanner;
        StatSummaryView _charStats, _equipStats, _weaponStats, _compStats;
        StatRowView _charMpLine, _charCritLine, _charCritDmgLine, _charSpdLine;
        TMP_Text _charJobBadge;
        Text _equipEmptyHint, _equipCapLabel;
        // char growth rows
        Text _ascendInfo;
        BarView _ascendBar;
        // weapon modal
        TMP_Text _weaponEquipped;
        Text _weaponStars;
        StatRowView _weaponEquipEffect, _weaponOwnEffect;
        RectTransform _weaponGrid;
        ItemCardView _weaponEqCard;
        Button _weaponLockBtn;
        int _weaponFilter; // 0 all, 1 equipped, 2 epic+
        // companion modal
        ItemCardView _compMainCard, _compSubCard;
        ItemCardView[] _compSubSlots;
        Text _compInfo;
        string _compSelId;
        TMP_Text _compSelLabel;
        Button _compSelAwaken, _compSelLock, _compSelMain, _compSelSub;
        Text _compSummonEffect;
        Button _compAutoBtn;
        Button[] _compPresetBtns;
        int _compPreset;
        RectTransform _compGrid;
        RectTransform _compDeployRow;
        int _compFilter; // 0 all, 1 deployed, 2 epic+
        ModalView _summonResultModal;
        RectTransform _summonResultGrid;
        // equip preview
        ItemCardView[] _equipPreviewSlots;
        Image _equipPreviewAvatar;
        Text[] _equipPreviewLv;
        // shop modal
        Image[] _shopCatBgs;
        Text _shopInfo;
        ChipView _shopRdChip, _shopBlueChip;
        GameObject[] _shopPanels;
        StatRowView _shopPassRow, _shopSeasonInfo;
        // equip
        int _equipSelected;
        // tab panels
        GameObject[] _skillPanels;
        GameObject[] _charPanels;
        ItemCardView[] _skillTiles;
        // offline modal
        StatRowView _offGoldRow, _offXpRow, _offStoneRow;
        // server modal
        Image[] _serverBgs;

        bool _fullAuto = true;
        bool _serverPicked;
        string _nick = "모험가";

        static readonly string[] NavLabels = { "캐릭터", "장비", "스킬", "무기", "동료" };

        void Start()
        {
            GrowGameBootstrap.EnsureRoot();
            KoreanUiFont.Get();
            AudioService.EnsureRoot();
            AudioService.PlayBgm();
            GameSettings.ApplyAudio();
            Build();
            _battle = gameObject.GetComponent<FieldAutoHuntController>() ?? gameObject.AddComponent<FieldAutoHuntController>();
            if (_fieldRt != null) _battle.BindField(_fieldRt);
            _battle.OnChanged += RefreshHud;

            Subscribe(StageProgress.Instance);
            Subscribe(PlayerGrowth.Instance);
            Subscribe(PlayerWallet.Instance);
            Subscribe(CurrencyWallet.Instance);
            Subscribe(WalletAdapter.Instance);
            Subscribe(LootBoxService.Instance);
            Subscribe(SkillAdapter.Instance);
            Subscribe(WeaponSummonAdapter.Instance);
            Subscribe(CompanionAdapter.Instance);
            Subscribe(InventoryAdapter.Instance);

            _serverPicked = PlayerPrefs.GetInt("IdleGrow.Maple.ServerPicked", 0) == 1;
            _nick = PlayerPrefs.GetString("IdleGrow.Maple.Nick", "모험가");
            if (!_serverPicked) _modals.Open(_serverModal);
            else TryShowOffline();

            RefreshAll();
        }

        void Subscribe(object svc)
        {
            if (svc == null) return;
            var ev = svc.GetType().GetEvent("OnChanged");
            if (ev != null) ev.AddEventHandler(svc, (System.Action)RefreshAll);
        }

        void OnDestroy()
        {
            if (_battle != null) _battle.OnChanged -= RefreshHud;
            IdleMvp.Core.QuestService.OnChanged -= RefreshSubQuestCard;
        }

        void Update()
        {
            var sk = SkillAdapter.Instance;
            if (sk == null) return;
            // 버프·반격이 살아 있으면 쿨다운 자리에 남은 지속시간을 초록으로 보여준다.
            // (쿨다운과 지속이 겹치면 지속이 우선 — 유저가 알고 싶은 건 "지금 세졌는가")
            var battle = Combat.FieldAutoHuntController.Instance;
            float buffLeft = battle != null ? Mathf.Max(battle.BuffTimeLeft, battle.CounterTimeLeft) : 0f;
            for (int i = 0; i < 4; i++)
            {
                if (_skillCd[i] == null) continue;
                float cd = sk.CurrentCd[i];
                bool showBuff = buffLeft > 0f && cd <= 0f && battle != null && battle.BuffSkillSlot == i;
                _skillCd[i].text = showBuff ? buffLeft.ToString("0") : (cd > 0 ? cd.ToString("0") : "");
                _skillCd[i].color = showBuff ? new Color(0.5f, 1f, 0.6f) : UiKit.TextInverse;
                // 라디얼 필 — 최대 쿨타임은 관측값으로 추정 (늘어나는 순간이 최대치)
                if (_skillCdFill[i] != null)
                {
                    if (cd > _skillCdMax[i]) _skillCdMax[i] = cd;
                    if (cd <= 0f && _skillCdMax[i] > 0f) _skillCdMax[i] = _skillCdMax[i]; // 유지
                    _skillCdFill[i].fillAmount = (cd > 0f && _skillCdMax[i] > 0.01f)
                        ? Mathf.Clamp01(cd / _skillCdMax[i]) : 0f;
                }
            }

            // 물약 버튼 상태 (쿨타임 라디얼 + 보유 수)
            if (_potionCdFill != null)
                _potionCdFill.fillAmount = Mathf.Clamp01(Core.PotionService.CooldownLeft / Core.PotionService.CooldownSec);
            if (_potionCountText != null)
            {
                int pc = Core.PotionService.Count;
                _potionCountText.text = pc.ToString();
                _potionCountText.color = pc > 0 ? UiKit.TextInverse : new Color(1f, 0.5f, 0.45f);
            }
        }

        readonly float[] _skillCdMax = new float[4];

        // =====================================================================
        // Build
        // =====================================================================

        void Build()
        {
            var canvasGo = new GameObject("MapleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            // 높이 기준(1)으로 맞춘다. 0.5(가로·세로 혼합)로 두면 창 비율이 바뀔 때마다
            // 캔버스 논리 높이가 같이 변해서(1920x400이면 1080 -> 657) 캔버스 단위로 크기가
            // 고정된 캐릭터·바닥층이 화면 대비 커졌다 작아졌다 한다.
            // 높이를 고정하면 가로가 넓어질수록 필드가 좌우로 더 보일 뿐 크기는 유지된다.
            scaler.matchWidthOrHeight = 1f;

            // 구매 에셋 팝업 프리팹(확인창/획득창)과 연출을 이 캔버스에 붙인다
            Casual.CasualDialogs.SetHost(canvasGo.transform);
            Casual.CasualFx.SetRunner(this);
            Casual.CasualScreens.Init(canvasGo.transform, Toast, RefreshAll);
            Casual.CasualScreens.BindAppearance(() => DebugOpen("appearance"));
            // 프리팹 화면을 열 때 손그림 모달이 뒤에 남아 두 개가 겹치지 않게
            Casual.CasualScreens.BindCloseLegacy(() => _modals?.Close());
            Casual.CasualScreens.BindOpenById(DebugOpen);
            _summonFxHost = canvasGo.transform;

            // Background + actors now live in the world layer behind the overlay canvas.
            // Field rect stays as the pixel-space coordinate frame for combat + FX.
            var field = UiKit.Rect(canvasGo.transform, "Field");
            field.anchorMin = Vector2.zero;
            field.anchorMax = Vector2.one;
            field.offsetMin = new Vector2(0f, 96f);
            field.offsetMax = new Vector2(0f, -8f);
            _fieldRt = field;

            IdleMvp.Combat.FieldWorldStage.Ensure(field);

            BuildTopHud(canvasGo.transform);
            BuildQuestCard(canvasGo.transform);
            BuildSubQuestCard(canvasGo.transform);
            BuildStatusCapsule(canvasGo.transform);
            BuildBottomNav(canvasGo.transform);
            BuildSkillCluster(canvasGo.transform);
            BuildTutorialCard(canvasGo.transform);

            _statusText = UiKit.TmpLabel(canvasGo.transform, "BattleStatus", "", UiKit.TmpBody, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            _statusText.enableWordWrapping = false;
            var str = _statusText.rectTransform;
            str.anchorMin = str.anchorMax = new Vector2(0.5f, 1f);
            str.pivot = new Vector2(0.5f, 1f);
            str.sizeDelta = new Vector2(760f, 30f);
            str.anchoredPosition = new Vector2(0f, -176f);

            _modals = new MapleModalHost(canvasGo.transform);
            _modals.OnClosed += () => SetNavSelected(-1);

            BuildGrowthModals();
            BuildShopAndPopups();
            BuildMetaScreens();

            _extra = new MapleExtraScreens(_modals, Toast, RefreshAll);
            _extra.BuildAll(canvasGo.transform);

            _appearance = new MapleAppearanceScreen(_modals, Toast, RefreshAll);
            _appearance.Build();
            _modals.OnClosed += () => _appearance.ReleaseRig();
            _jobModal = _extra.JobModal;
            _serverModal = _extra.ServerModal;
            _offlineModal = _extra.OfflineModal;
            _hotDealModal = _extra.HotDealModal;
            _eventModal = _extra.EventModal;
            _dungeonModal = _extra.DungeonModal;
            _artifactModal = _extra.ArtifactModal;
            _factionModal = _extra.FactionModal;
            _fatedModal = _extra.FatedModal;
            _rebirthModal = _extra.RebirthModal;
            Core.FactionService.TrySubscribe();
            Core.FactionService.OnChanged += () =>
            {
                if (Core.FactionService.ShouldShowSelection && _factionModal != null)
                    _modals.Open(_factionModal);
            };
            Core.FatedEventService.OnFatedEvent += () =>
            {
                if (_fatedModal != null) _modals.Open(_fatedModal);
            };
            if (LevelPackageService.Instance != null)
                LevelPackageService.Instance.OnChanged += () =>
                {
                    var lp = LevelPackageService.Instance;
                    if (lp == null || !lp.HasPending) return;
                    int lv = lp.PendingPackage?.Level ?? 0;
                    if (lv == _lastPkgToastLevel) return;   // 같은 패키지를 두 번 알리지 않는다
                    _lastPkgToastLevel = lv;
                    Toast($"Lv{lv} 패키지 수령 가능! 상점을 확인하세요.");
                };
            _extra.BindServerComplete(() =>
            {
                _serverPicked = true;
                PlayerPrefs.SetInt("IdleGrow.Maple.ServerPicked", 1);
                PlayerPrefs.Save();
                _modals.Close();
                ShowJobOnce();
            });

            UiToast.Ensure(canvasGo.transform);
        }

        /// <summary>Floating top widgets: player chip | stage+도전 | utility icons (no full-width strip).</summary>
        void BuildTopHud(Transform root)
        {
            // ---- left player chip (avatar circle + name / CP) ----
            var left = MapleUiTheme.Chip(root, "PlayerChip");
            var lrt = left.rectTransform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0f, 1f);
            lrt.pivot = new Vector2(0f, 1f);
            lrt.sizeDelta = new Vector2(330f, 96f);
            lrt.anchoredPosition = new Vector2(12f, -10f);

            var lh = UiKit.HStack(left.transform, "H", 10f, 12, 12, 10, 10);
            UiKit.Fill(lh);

            var avatarFrame = UiKit.Img(lh, "AvatarFrame", Color.white);
            if (!FantasyKitSlots.Slice(avatarFrame, GrowArt.CircleFrame, 72f))
                avatarFrame.color = new Color(1f, 1f, 1f, 0.12f);
            UiKit.Fix(avatarFrame, 72f, 72f);
            // HUD portrait = snapshot of the player's actual customized rig (head/torso focus).
            var avatarPrev = CharacterPreview.Attach(avatarFrame.transform, "HudPortrait", 128, 128, 0.62f, 1.28f, live: false);
            UiKit.Fill(avatarPrev.Rect, 7f);

            var info = UiKit.VStack(lh, "Info", 2f, 0, 0, 4, 4, TextAnchor.UpperLeft);
            UiKit.Flex(info);

            _lvText = UiKit.TmpLabel(info, "Lv", "", UiKit.TmpCaption, UiKit.TextInverseDim, bold: true);
            _lvText.enableWordWrapping = false;
            UiKit.Fix(_lvText, -1f, 24f);

            var cpRow = UiKit.HStack(info, "CpRow", 6f);
            UiKit.Fix(cpRow, -1f, 32f);
            if (GrowArt.IconCp != null)
            {
                var cpIcon = UiKit.Sprite(cpRow, "CpIcon", GrowArt.IconCp);
                UiKit.Fix(cpIcon, 24f, 24f);
            }
            _cpText = UiKit.TmpLabel(cpRow, "Cp", "", UiKit.TmpBody, UiKit.TextInverse, bold: true);
            _cpText.enableWordWrapping = false;
            UiKit.Flex(_cpText);

            // Currency chips under player chip — compact floating row, no full-width strip
            var curRow = UiKit.HStack(root, "CurRow", 8f, 0, 0, 0, 0);
            var csr = curRow.GetComponent<RectTransform>();
            csr.anchorMin = csr.anchorMax = new Vector2(0f, 1f);
            csr.pivot = new Vector2(0f, 1f);
            csr.sizeDelta = new Vector2(640f, 40f);
            csr.anchoredPosition = new Vector2(12f, -112f);
            _goldChip = MapleUiTheme.CurrencyChip(curRow, "Gold", GrowArt.IconGold, UiKit.GoldColor);
            UiKit.Fix(_goldChip.Go.transform, 150f, 40f);
            _gemChip = MapleUiTheme.CurrencyChip(curRow, "Gem", GrowArt.IconGem, UiKit.GemColor);
            UiKit.Fix(_gemChip.Go.transform, 150f, 40f);
            _blueChip = MapleUiTheme.CurrencyChip(curRow, "Blue", GrowArt.IconAscend(1), FantasyKitSlots.KitTeal);
            UiKit.Fix(_blueChip.Go.transform, 150f, 40f);
            _ticketChip = MapleUiTheme.CurrencyChip(curRow, "Ticket", null, UiKit.Accent);
            UiKit.Fix(_ticketChip.Go.transform, 150f, 40f);

            // ---- center stage banner + challenge (Frame_Stage kept near native aspect) ----
            var center = UiKit.Rect(root, "StageWrap");
            center.anchorMin = center.anchorMax = new Vector2(0.5f, 1f);
            center.pivot = new Vector2(0.5f, 1f);
            center.sizeDelta = new Vector2(560f, 156f);
            center.anchoredPosition = new Vector2(0f, -8f);

            var banner = UiKit.Img(center, "Banner", new Color(0.08f, 0.09f, 0.12f, 0.55f));
            var stageSp = GrowArt.StageFrame;
            if (stageSp != null)
            {
                banner.sprite = stageSp;
                banner.type = Image.Type.Sliced;
                banner.pixelsPerUnitMultiplier = 1f;
                // 키트 프레임은 흰색 원본 — white로 두면 흰 띠가 되어 글씨가 안 보인다
                banner.color = new Color(0.06f, 0.10f, 0.22f, 0.92f);
            }
            var brt = banner.rectTransform;
            brt.anchorMin = new Vector2(0f, 1f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.sizeDelta = new Vector2(0f, 92f);
            brt.anchoredPosition = Vector2.zero;

            var cv = UiKit.VStack(banner.transform, "V", 6f, 52, 52, 16, 14, TextAnchor.UpperCenter);
            UiKit.Fill(cv);
            _stageLabel = UiKit.TmpLabel(cv, "StageLabel", "", UiKit.TmpBody, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            _stageLabel.enableWordWrapping = false;
            _stageLabel.overflowMode = TextOverflowModes.Overflow;
            UiKit.Fix(_stageLabel, -1f, 28f);

            _stageBar = MapleUiTheme.Bar(cv, "StageBar", UiKit.Accent);
            UiKit.Fix(_stageBar.Go.transform, -1f, 22f);

            _challengeBtn = MapleUiTheme.PrimaryButton(center, "Challenge", "도전", OnChallenge, UiKit.TmpBody);
            var chr = _challengeBtn.GetComponent<RectTransform>();
            chr.anchorMin = chr.anchorMax = new Vector2(0.5f, 0f);
            chr.pivot = new Vector2(0.5f, 0f);
            chr.sizeDelta = new Vector2(180f, 56f);
            chr.anchoredPosition = Vector2.zero;
            _challengeLabel = _challengeBtn.GetComponentInChildren<TMP_Text>();

            var speedBtn = MapleUiTheme.SecondaryButton(center, "Speed", $"x{GameSettings.SpeedLevel}", () =>
            {
                Toast(GameSettings.ToggleSpeed());
                if (_speedLabel != null) _speedLabel.text = $"x{GameSettings.SpeedLevel}";
            }, UiKit.TmpCaption);
            var srt = speedBtn.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(1f, 0f);
            srt.pivot = new Vector2(1f, 0f);
            srt.sizeDelta = new Vector2(60f, 40f);
            srt.anchoredPosition = new Vector2(-8f, 8f);
            _speedLabel = speedBtn.GetComponentInChildren<TMP_Text>();
            GameSettings.ApplySpeed();

            // ---- right utility icons (currency is on left chip, like Maple Idle) ----
            var right = UiKit.HStack(root, "TopIcons", 8f, 0, 0, 0, 0, TextAnchor.MiddleRight);
            var rrt = right.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(1f, 1f);
            rrt.sizeDelta = new Vector2(300f, 64f);
            rrt.anchoredPosition = new Vector2(-12f, -12f);

            string[] icons = { "길드", "우편", "상점", "메뉴" };
            Sprite[] iconSprites = { GrowArt.IconQuest, GrowArt.IconMail, GrowArt.IconMenuStore, GrowArt.IconSetting };
            for (int i = 0; i < icons.Length; i++)
            {
                int idx = i;
                var b = MapleUiTheme.IconButton(right, "Top" + i, iconSprites[i] != null ? "" : icons[i],
                    iconSprites[i], () => OnTopIcon(idx));
                UiKit.Fix(b, 64f, 64f);
                if (i <= 1)
                {
                    var dot = MapleUiTheme.AlertDot(b.transform, "Dot", 18f);
                    var dr = dot.rectTransform;
                    dr.anchorMin = dr.anchorMax = new Vector2(1f, 1f);
                    dr.pivot = new Vector2(1f, 1f);
                    dr.anchoredPosition = new Vector2(-2f, -2f);
                    dot.gameObject.SetActive(false);
                    if (i == 0) _guildDot = dot;
                    else _mailDot = dot;
                }
            }
        }

        void BuildQuestCard(Transform root)
        {
            var q = MapleUiTheme.Chip(root, "Quest");
            var rt = q.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(300f, 116f);
            rt.anchoredPosition = new Vector2(12f, -164f);

            var v = UiKit.VStack(q.transform, "V", 4f, 14, 14, 12, 12, TextAnchor.UpperLeft);
            UiKit.Fill(v);
            var titleRow = UiKit.HStack(v, "TitleRow", 6f);
            UiKit.Fix(titleRow, -1f, 24f);
            if (GrowArt.IconQuest != null)
            {
                var qIcon = UiKit.Sprite(titleRow, "QIcon", GrowArt.IconQuest);
                UiKit.Fix(qIcon, 20f, 20f);
            }
            var title = UiKit.TmpLabel(titleRow, "Title", "가이드 퀘스트", UiKit.TmpCaption, UiKit.GoldColor, bold: true);
            title.enableWordWrapping = false;
            UiKit.Flex(title);
            _questText = UiKit.TmpLabel(v, "Body", "", UiKit.TmpBody, UiKit.TextInverse, bold: true);
            _questText.overflowMode = TextOverflowModes.Overflow;
            var rewardT = UiKit.TmpLabel(v, "Reward", "보상: RD · XP", UiKit.TmpCaption, UiKit.TextInverseDim);
            rewardT.enableWordWrapping = false;
        }

        // ---- 서브퀘스트 카드 (가이드 퀘스트 바로 아래, 상시) --------------------
        Image _subQuestChip;
        TMP_Text _subQuestTitle, _subQuestBody;
        Color _subQuestIdleColor;   // 칩 원래 색 — 흰색으로 덮으면 밝은 카드가 되어 글씨가 안 보인다
        static readonly Color SubQuestDone = new Color(1f, 0.85f, 0.35f, 1f);

        void BuildSubQuestCard(Transform root)
        {
            _subQuestChip = MapleUiTheme.Chip(root, "SubQuest");
            _subQuestIdleColor = _subQuestChip.color;
            var rt = _subQuestChip.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(300f, 92f);
            rt.anchoredPosition = new Vector2(12f, -288f);   // 가이드 퀘스트(=-164, h116) 바로 아래

            var v = UiKit.VStack(_subQuestChip.transform, "V", 3f, 14, 14, 10, 10, TextAnchor.UpperLeft);
            UiKit.Fill(v);
            _subQuestTitle = UiKit.TmpLabel(v, "Title", "", UiKit.TmpCaption, UiKit.GoldColor, bold: true);
            _subQuestTitle.enableWordWrapping = false;
            UiKit.Fix(_subQuestTitle, -1f, 22f);
            _subQuestBody = UiKit.TmpLabel(v, "Body", "", UiKit.TmpCaption, UiKit.TextInverse, bold: true);
            _subQuestBody.overflowMode = TextOverflowModes.Overflow;
            _subQuestBody.enableWordWrapping = false;

            // 카드 전체가 버튼 — 완료 상태에서 탭하면 즉시 수령하고 다음 퀘스트가 뜬다
            var btn = _subQuestChip.gameObject.AddComponent<Button>();
            btn.targetGraphic = _subQuestChip;
            UiKit.Press(btn);
            btn.onClick.AddListener(() =>
            {
                string msg = IdleMvp.Core.QuestService.TryClaim();
                if (msg != null)
                {
                    Toast("퀘스트 완료! " + msg);
                    Casual.CasualFx.EnhanceFlash(_subQuestChip.transform);
                    IdleMvp.Core.AudioService.Gold();
                }
                RefreshSubQuestCard();
            });

            IdleMvp.Core.QuestService.OnChanged += RefreshSubQuestCard;
            RefreshSubQuestCard();
        }

        void RefreshSubQuestCard()
        {
            if (_subQuestChip == null) return;
            // 튜토리얼이 아직 진행 중이면 서브퀘스트는 숨긴다 (한 번에 하나만 가르친다)
            bool show = IdleMvp.Core.TutorialService.Done;
            _subQuestChip.gameObject.SetActive(show);
            if (!show) return;
            bool done = IdleMvp.Core.QuestService.IsComplete;
            _subQuestTitle.text = (done ? "★ " : "") + "퀘스트 · " + IdleMvp.Core.QuestService.Current.Title
                + "  #" + (IdleMvp.Core.QuestService.Step + 1);
            _subQuestBody.text = done
                ? "완료! 터치하여 보상 수령"
                : IdleMvp.Core.QuestService.DescText
                    + " (" + UiKit.Num(IdleMvp.Core.QuestService.Progress) + "/" + UiKit.Num(IdleMvp.Core.QuestService.Goal) + ")";
            _subQuestChip.color = done ? SubQuestDone : _subQuestIdleColor;
            _subQuestBody.color = done ? new Color(0.25f, 0.18f, 0.02f, 1f) : UiKit.TextInverse;
            _subQuestTitle.color = done ? new Color(0.35f, 0.24f, 0.02f, 1f) : UiKit.GoldColor;
        }

        RectTransform _equipSlotGrid;
        HeroCardView[] _equipCells;

        // ---- 확인창 경유 실행 (뽑기/강화) ------------------------------------

        void ConfirmWeaponSummon(int pulls)
        {
            var cw = CurrencyWallet.Instance;
            var costs = new System.Collections.Generic.List<Casual.CostLine>
            {
                Casual.CostLine.Of("무기 소환권", pulls,
                    cw != null ? cw.Get(CurrencyId.WeaponTicket) : 0),
            };
            Casual.CasualDialogs.Confirm(
                pulls > 1 ? $"무기 {pulls}연차 소환" : "무기 소환",
                pulls > 1 ? "소환권 10장을 사용해 10회 소환합니다." : "소환권 1장을 사용합니다.",
                costs,
                () =>
                {
                    var wa = WeaponSummonAdapter.Instance;
                    if (wa == null) return;
                    string msg = pulls > 1 ? wa.SummonTen() : wa.SummonOne();
                    Toast(msg);
                    Casual.CasualFx.SummonBurst(_summonFxHost, pulls > 1);
                    RefreshAll();
                });
        }

        void ConfirmCompanionPull(int pulls)
        {
            var cw = CurrencyWallet.Instance;
            var costs = new System.Collections.Generic.List<Casual.CostLine>
            {
                Casual.CostLine.Of("동료 소환권", pulls,
                    cw != null ? cw.Get(CurrencyId.CompanionTicket) : 0),
            };
            Casual.CasualDialogs.Confirm(
                pulls > 1 ? $"동료 {pulls}연차 소환" : "동료 소환",
                pulls > 1 ? "소환권 10장을 사용해 10회 소환합니다." : "소환권 1장을 사용합니다.",
                costs,
                () =>
                {
                    var ca = CompanionAdapter.Instance;
                    if (ca == null) return;
                    if (pulls > 1)
                    {
                        var list = ca.TrySummonTen();
                        if (list.Count == 0 || !list[0].Ok)
                        { Toast(list.Count > 0 ? list[0].Message : "소환 실패"); return; }
                        Toast($"보유 {ca.OwnedCount} · 소환 {list.Count}회");
                        Casual.CasualFx.SummonBurst(_summonFxHost, true);
                        ShowSummonResults(list);
                    }
                    else
                    {
                        var r = ca.TrySummonOne();
                        if (!r.Ok) { Toast(r.Message); return; }
                        Toast($"보유 {ca.OwnedCount} · {r.Message}");
                        Casual.CasualFx.SummonBurst(_summonFxHost, false);
                        ShowSummonResults(new System.Collections.Generic.List<CompanionSummonResult> { r });
                    }
                    FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                });
        }

        void ConfirmWeaponUpgrade()
        {
            var wa = WeaponSummonAdapter.Instance;
            var w = wa?.Equipped;
            if (w == null) { Toast("장착 무기 없음"); return; }
            int max = 20 + w.awaken * 20;
            if (w.level >= max) { Toast("최대 레벨 (각성 필요)"); return; }

            var cw = CurrencyWallet.Instance;
            double need = 1 + w.level * 0.2f;
            var costs = new System.Collections.Generic.List<Casual.CostLine>
            {
                Casual.CostLine.Of("무기 강화석", need,
                    cw != null ? cw.Get(CurrencyId.WeaponEnhanceStone) : 0),
            };
            Casual.CasualDialogs.Confirm("무기 강화",
                $"{w.name}  Lv.{w.level} → Lv.{w.level + 1}",
                costs,
                () =>
                {
                    Toast(wa.LevelUpEquipped());
                    Casual.CasualFx.EnhanceFlash(_summonFxHost);
                    RefreshAll();
                });
        }

        void ConfirmSlotEnhance(string title, string what, CurrencyId cur, double need,
            System.Func<string> run)
        {
            var cw = CurrencyWallet.Instance;
            var costs = new System.Collections.Generic.List<Casual.CostLine>
            {
                Casual.CostLine.Of(what, need, cw != null ? cw.Get(cur) : 0),
            };
            Casual.CasualDialogs.Confirm(title,
                $"{SlotEnhanceService.SlotLabel(_equipSelected)} 슬롯에 적용합니다.",
                costs,
                () =>
                {
                    string msg = run();
                    Toast(msg);
                    // 실패에도 축하 링이 번쩍이던 것 — 성공만 빛나고 실패는 흔들린다
                    bool failed = msg.Contains("실패") || msg.Contains("부족") ||
                                  msg.Contains("소진") || msg.Contains("파괴");
                    if (failed) Casual.CasualFx.FailShake(_summonFxHost);
                    else Casual.CasualFx.EnhanceFlash(_summonFxHost);
                    RefreshAll();
                });
        }

        Transform _summonFxHost;

        /// <summary>장착 슬롯 6칸을 무기 카드와 같은 HeroCard로 그린다.</summary>
        void BuildEquipSlotCards(int slotCount)
        {
            if (_equipSlotGrid == null) return;
            ClearChildren(_equipSlotGrid);

            var inv = InventoryAdapter.Instance;
            var glg = _equipSlotGrid.GetComponent<GridLayoutGroup>();
            var cell = glg != null ? glg.cellSize : new Vector2(178f, 250f);
            var w0 = WeaponSummonAdapter.Instance?.Equipped;

            for (int i = 0; i < slotCount; i++)
            {
                int idx = i;
                var st = inv != null && i < inv.Slots.Length ? inv.Slots[i] : null;
                int rar = st != null ? st.rarity : 0;
                int lv = st != null ? st.level : 1;
                string label = inv != null ? inv.SlotLabel(i) : "슬롯 " + (i + 1);

                // 무기 슬롯은 실제 장착 무기의 아이콘/등급을 그대로 보여준다
                Sprite icon = GrowArt.IconGear(i);
                Color tint = Color.white;
                if (i == 0 && w0 != null)
                {
                    icon = GrowArt.IconWeaponId(w0.catalogId, w0.kind);
                    tint = GrowArt.WeaponIconIsDedicated(w0.catalogId)
                        ? Color.white : GrowArt.WeaponTint(w0.kind, w0.rarity);
                    rar = w0.rarity;
                    lv = w0.level;
                    label = w0.name;
                }

                _equipCells[i] = CasualCards.HeroCard(_equipSlotGrid, "Slot" + i, label,
                    icon, tint, rar, rar, 5,
                    lv.ToString(), 0f, _equipSelected == i ? "선택" : $"등급 {rar}", false,
                    () => { _equipSelected = idx; RefreshAll(); }, cell);
            }
        }

        /// <summary>
        /// 엘리트 소환 조작. 장비창 안에 묻혀 있던 걸 HP/MP 캡슐 왼쪽으로 꺼냈다.
        /// </summary>
        void BuildEliteCluster(Transform root, RectTransform capsule)
        {
            float capsuleLeft = capsule.anchoredPosition.x - capsule.sizeDelta.x * 0.5f;

            var col = UiKit.VStack(root, "EliteCluster", 6f, 0, 0, 0, 0, TextAnchor.LowerCenter);
            var crt = col;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(1f, 0f);
            crt.sizeDelta = new Vector2(150f, 78f);
            crt.anchoredPosition = new Vector2(capsuleLeft - 10f, 102f);

            var eb = MapleUiTheme.AccentButton(col, "Elite", "엘리트 소환",
                () => Toast(EliteSummonService.Instance?.TrySummonElite()), UiKit.FontCaption);
            UiKit.Fix(eb, 150f, 36f);

            _eliteLvBtn = MapleUiTheme.SecondaryButton(col, "EliteLv", "소환 레벨 ↑",
                () => Toast(EliteSummonService.Instance?.TryRaiseSummonLevel()), UiKit.FontCaption);
            UiKit.Fix(_eliteLvBtn, 150f, 36f);
            _eliteLvLabel = _eliteLvBtn.GetComponentInChildren<TMPro.TMP_Text>();

            BuildPotionButton(root, capsule);
        }

        Button _eliteLvBtn;
        TMPro.TMP_Text _eliteLvLabel;
        Button _potionBtn;
        Image _potionIcon;
        Image _potionCdFill;
        TMPro.TMP_Text _potionCountText;

        /// <summary>
        /// 체력 물약 버튼 — 소모품 + 쿨타임 30초 + 자동 사용 (PotionService).
        /// 예전의 '레드다이아 즉석 결제 회복'은 물약이 없을 때의 구매 경로로 강등.
        /// </summary>
        void BuildPotionButton(Transform root, RectTransform capsule)
        {
            float capsuleRight = capsule.anchoredPosition.x + capsule.sizeDelta.x * 0.5f;
            var img = UiKit.Img(root, "PotionBtn", Color.white);
            var frame = GrowArt.CircleFrame;
            if (frame != null) { img.sprite = frame; img.type = Image.Type.Simple; img.color = new Color(0.55f, 0.16f, 0.20f, 1f); }
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(66f, 66f);
            rt.anchoredPosition = new Vector2(capsuleRight + 96f, 108f);

            var potSp = CasualArt.C("IcoC_PotionHp") ?? CasualArt.C("Ico_Hp");
            _potionIcon = UiKit.Img(img.transform, "Ic", Color.white);
            if (potSp != null) { _potionIcon.sprite = potSp; _potionIcon.preserveAspect = true; }
            UiKit.Fill(_potionIcon.rectTransform, 12f);

            // 쿨타임 라디얼 (시계방향으로 걷히는 어두운 덮개)
            _potionCdFill = UiKit.Img(img.transform, "Cd", new Color(0f, 0f, 0f, 0.6f));
            if (frame != null)
            {
                _potionCdFill.sprite = frame;
                _potionCdFill.type = Image.Type.Filled;
                _potionCdFill.fillMethod = Image.FillMethod.Radial360;
                _potionCdFill.fillOrigin = (int)Image.Origin360.Top;
                _potionCdFill.fillClockwise = false;
            }
            UiKit.Fill(_potionCdFill.rectTransform);
            _potionCdFill.raycastTarget = false;
            _potionCdFill.fillAmount = 0f;

            // 보유 수 뱃지 (우하단)
            _potionCountText = UiKit.TmpLabel(img.transform, "N", "", UiKit.TmpCaption, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.BottomRight);
            UiKit.Fill(_potionCountText.rectTransform, 4f);

            _potionBtn = img.gameObject.AddComponent<Button>();
            _potionBtn.targetGraphic = img;
            UiKit.Press(_potionBtn);
            _potionBtn.onClick.AddListener(OnPotionTapped);

            // 우상단 '+' 뱃지 → 물약 강화 모달 (자식 버튼이 레이캐스트를 먼저 먹는다)
            var enh = UiKit.Img(img.transform, "Enh", new Color(0.30f, 0.72f, 0.34f, 1f));
            var circ = CasualArt.C("BasicFrame_Circle77");
            if (circ != null) enh.sprite = circ;
            var ert = enh.rectTransform;
            ert.anchorMin = ert.anchorMax = new Vector2(1f, 1f);
            ert.pivot = new Vector2(1f, 1f);
            ert.anchoredPosition = new Vector2(4f, 4f);
            ert.sizeDelta = new Vector2(30f, 30f);
            var plus = UiKit.TmpLabel(enh.transform, "P", "+", UiKit.TmpBody, Color.white,
                bold: true, TextAlignmentOptions.Center);
            UiKit.Fill(plus.rectTransform);
            plus.raycastTarget = false;
            var enhBtn = enh.gameObject.AddComponent<Button>();
            enhBtn.targetGraphic = enh;
            UiKit.Press(enhBtn);
            enhBtn.onClick.AddListener(() => { if (_potionEnhModal != null) _modals.Open(_potionEnhModal); });
        }

        void BuyPotionUpgrade()
        {
            if (Core.PotionService.IsMaxLevel) { Toast("이미 최대 강화입니다"); return; }
            var w = WalletAdapter.Instance;
            double cost = Core.PotionService.UpgradeCostGold;
            if (w == null || !w.TrySpendGold(cost)) { Toast($"골드 {UiKit.Num(cost)} 필요"); return; }
            Core.PotionService.UpgradeOne();
            // 물약 강화도 '강화' 계열 — 서브퀘스트·일일미션 카운트에 합산 (장비 강화와 동일 취급)
            Core.DailyMissionService.Increment("enhance");
            Core.QuestService.Notify(Core.QuestService.Kind.Enhance);
            Toast($"물약 강화 Lv.{Core.PotionService.Level} — 회복 {Core.PotionService.HealPct * 100f:0}%, 쿨 {Core.PotionService.CooldownSec:0.#}초!");
            AudioService.Gold();
            RefreshAll();
        }

        void OnPotionTapped()
        {
            var fah = FieldAutoHuntController.Instance;
            if (fah == null) return;

            if (Core.PotionService.Count <= 0)
            {
                // 물약이 없다 → 레드다이아로 팩 구매 (기존 즉석 결제의 후속 역할)
                var w = WalletAdapter.Instance;
                if (w == null || !w.TrySpendRedDiamond(Core.PotionService.PackCostRd))
                { Toast($"레드다이아 {Core.PotionService.PackCostRd} 필요 (물약 {Core.PotionService.PackSize}개)"); return; }
                Core.PotionService.Grant(Core.PotionService.PackSize);
                Toast($"물약 {Core.PotionService.PackSize}개 구매!");
                AudioService.Gold();
                return;
            }
            if (Core.PotionService.CooldownLeft > 0f)
            { Toast($"물약 쿨타임 {Core.PotionService.CooldownLeft:0}초"); return; }
            if (!fah.HeroNeedsHeal) { Toast("이미 체력이 가득 찼습니다"); return; }
            if (!Core.PotionService.TryUse()) return;
            float healed = fah.HealHero(Core.PotionService.HealPct);
            Toast($"체력 {UiKit.Num(healed)} 회복!");
            Casual.CasualFx.EnhanceFlash(_summonFxHost);
            AudioService.Gem();
        }

        /// <summary>Floating HP/MP capsule above the nav dock (Maple Idle center-bottom).</summary>
        void BuildStatusCapsule(Transform root)
        {
            var chip = MapleUiTheme.Chip(root, "StatusCapsule");
            var rt = chip.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(420f, 78f);
            rt.anchoredPosition = new Vector2(-40f, 102f);

            var v = UiKit.VStack(chip.transform, "V", 4f, 14, 14, 8, 8, TextAnchor.UpperLeft);
            UiKit.Fill(v);
            _nameText = UiKit.TmpLabel(v, "Name", "", UiKit.TmpCaption, UiKit.TextInverse, bold: true,
                TextAlignmentOptions.Center);
            _nameText.enableWordWrapping = false;
            UiKit.Fix(_nameText, -1f, 22f);
            _hpBar = MapleUiTheme.Bar(v, "Hp", UiKit.HpColor);
            UiKit.Fix(_hpBar.Go.transform, -1f, 22f);
            _mpBar = MapleUiTheme.Bar(v, "Mp", UiKit.MpColor);
            UiKit.Fix(_mpBar.Go.transform, -1f, 22f);

            BuildEliteCluster(root, rt);

            // Full Auto circular toggle to the right of status (kit profile-circle frame)
            _autoImg = UiKit.Img(root, "FullAuto", UiKit.Primary);
            bool autoCircle = GrowArt.CircleFrame != null;
            if (autoCircle)
            {
                _autoImg.sprite = GrowArt.CircleFrame;
                _autoImg.type = Image.Type.Simple;
                _autoImg.preserveAspect = true;
                // 키트 원형 프레임은 흰색 원본이라 white로 두면 흰 동그라미만 보인다
                _autoImg.color = new Color(0.16f, 0.30f, 0.60f, 1f);
            }
            var ar = _autoImg.rectTransform;
            ar.anchorMin = ar.anchorMax = new Vector2(0.5f, 0f);
            ar.pivot = new Vector2(0.5f, 0f);
            ar.sizeDelta = new Vector2(78f, 78f);
            ar.anchoredPosition = new Vector2(220f, 102f);
            _autoBtn = _autoImg.gameObject.AddComponent<Button>();
            _autoBtn.targetGraphic = _autoImg;
            UiKit.Press(_autoBtn);
            // 킷 전용 자동 아이콘(흰 실루엣) + 하단 작은 라벨 — 저해상도 삼각형과
            // 글자가 겹쳐 깨져 보이던 것 교체 (유저 지적 2회)
            var autoSp = CasualArt.C("Ico_Auto");
            if (autoSp != null)
            {
                var autoIcon = UiKit.Sprite(_autoImg.transform, "Icon", autoSp);
                var air = autoIcon.rectTransform;
                air.anchorMin = air.anchorMax = new Vector2(0.5f, 0.58f);
                air.sizeDelta = new Vector2(38f, 38f);
                autoIcon.raycastTarget = false;
            }
            _autoLabel = UiKit.TmpLabel(_autoImg.transform, "L", "자동", UiKit.TmpCaption - 2, UiKit.TextInverse,
                bold: true, autoSp != null ? TextAlignmentOptions.Bottom : TextAlignmentOptions.Center);
            UiKit.Fill(_autoLabel.rectTransform, 6f);
            _autoBtn.onClick.AddListener(() =>
            {
                _fullAuto = !_fullAuto;
                _autoLabel.text = _fullAuto ? "자동" : "수동";
                _autoImg.color = autoCircle
                    ? (_fullAuto ? Color.white : new Color(0.55f, 0.55f, 0.6f, 1f))
                    : (_fullAuto ? UiKit.Primary : UiKit.NeutralDark);
                Toast(_fullAuto ? "자동사냥 ON" : "자동사냥 OFF");
            });
        }

        GameObject _tutCard;
        TMP_Text _tutTitle, _tutDesc, _tutStepNo;
        Button _tutGo;

        /// <summary>
        /// 가이드 카드 — 튜토리얼이 끝나면 같은 자리에서 연쇄 서브퀘스트 카드가 된다.
        /// (키우기류 표준: '다음 할 일'이 항상 화면에 떠 있다)
        /// </summary>
        void BuildTutorialCard(Transform root)
        {
            var card = UiKit.Img(root, "TutorialCard", Color.white);
            if (CasualArt.PopupNavy != null) { card.sprite = CasualArt.PopupNavy; card.type = Image.Type.Sliced; }
            else { card.sprite = MapleLightTheme.RoundedSprite(14); card.type = Image.Type.Sliced; card.color = new Color(0.08f, 0.14f, 0.32f, 0.98f); }
            var rt = card.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(660f, 132f);
            rt.anchoredPosition = new Vector2(0f, 178f); // 히어로 HP 캡슐 위
            _tutCard = card.gameObject;

            _tutStepNo = UiKit.TmpLabel(card.transform, "No", "1/6", UiKit.TmpCaption - 2,
                new Color(0.55f, 0.85f, 1f, 1f), bold: true, TextAlignmentOptions.TopRight);
            _tutStepNo.enableWordWrapping = false;
            var nrt = _tutStepNo.rectTransform;
            nrt.anchorMin = new Vector2(1f, 1f); nrt.anchorMax = Vector2.one; nrt.pivot = Vector2.one;
            nrt.sizeDelta = new Vector2(80f, 26f); nrt.anchoredPosition = new Vector2(-14f, -10f);

            _tutTitle = UiKit.TmpLabel(card.transform, "Title", "", UiKit.TmpBody, new Color(1f, 0.85f, 0.35f, 1f),
                bold: true, TextAlignmentOptions.TopLeft);
            _tutTitle.enableWordWrapping = false;
            var trt = _tutTitle.rectTransform;
            trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f); trt.pivot = new Vector2(0.5f, 1f);
            trt.offsetMin = new Vector2(20f, -42f); trt.offsetMax = new Vector2(-96f, -10f);

            _tutDesc = UiKit.TmpLabel(card.transform, "Desc", "", UiKit.TmpCaption - 1, new Color(1f, 1f, 1f, 0.88f),
                bold: false, TextAlignmentOptions.TopLeft);
            _tutDesc.enableWordWrapping = true;
            var drt = _tutDesc.rectTransform;
            drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(1f, 1f);
            drt.offsetMin = new Vector2(20f, 12f); drt.offsetMax = new Vector2(-236f, -44f);

            var btnRow = UiKit.HStack(card.transform, "Btns", 8f, 0, 0, 0, 0, TextAnchor.MiddleRight);
            var brt = btnRow.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1f, 0f); brt.anchorMax = new Vector2(1f, 0f); brt.pivot = new Vector2(1f, 0f);
            brt.sizeDelta = new Vector2(224f, 52f); brt.anchoredPosition = new Vector2(-12f, 10f);

            _tutGo = MapleUiTheme.PrimaryButton(btnRow, "Go", "이동", () =>
            {
                var step = IdleMvp.Core.TutorialService.Current;
                if (!string.IsNullOrEmpty(step.Screen)) DebugOpen(step.Screen);
            }, UiKit.FontCaption);
            UiKit.Fix(_tutGo, 96f, 46f);
            _tutNext = MapleUiTheme.AccentButton(btnRow, "Next", "다음", () =>
            {
                bool finished = IdleMvp.Core.TutorialService.Advance();
                if (finished)
                {
                    CurrencyWallet.Instance?.Add(CurrencyId.RedDiamond, 30);
                    Toast("튜토리얼 완료! RD 30 지급");
                    IdleMvp.Core.AudioService.Gold();
                }
                RefreshTutorialCard();
            }, UiKit.FontCaption);
            UiKit.Fix(_tutNext, 96f, 46f);
            _tutNextLabel = _tutNext.GetComponentInChildren<TMP_Text>();

            var skip = UiKit.TmpLabel(card.transform, "Skip", "건너뛰기", UiKit.TmpCaption - 3,
                new Color(1f, 1f, 1f, 0.5f), bold: false, TextAlignmentOptions.BottomLeft);
            skip.enableWordWrapping = false;
            var srt = skip.rectTransform;
            srt.anchorMin = new Vector2(0f, 0f); srt.anchorMax = new Vector2(0f, 0f); srt.pivot = Vector2.zero;
            srt.sizeDelta = new Vector2(110f, 30f); srt.anchoredPosition = new Vector2(20f, 8f);
            var skipBtn = skip.gameObject.AddComponent<Button>();
            skipBtn.transition = Selectable.Transition.None;
            skipBtn.onClick.AddListener(() =>
            {
                IdleMvp.Core.TutorialService.SkipAll();
                RefreshTutorialCard();
            });
            skip.raycastTarget = true;
            _tutSkip = skip.gameObject;

            RefreshTutorialCard();
        }

        Button _tutNext;
        TMP_Text _tutNextLabel;
        GameObject _tutSkip;

        void RefreshTutorialCard()
        {
            if (_tutCard == null) return;

            if (IdleMvp.Core.TutorialService.Done)
            {
                // 튜토리얼 종료 — 하단 대형 카드는 숨기고, 좌상단 서브퀘스트 카드가 이어받는다
                // (화면 한가운데를 가리지 말라는 유저 지적)
                _tutCard.SetActive(false);
                RefreshSubQuestCard();
                return;
            }
            _tutCard.SetActive(true);

            var step = IdleMvp.Core.TutorialService.Current;
            if (_tutTitle != null) _tutTitle.text = step.Title;
            if (_tutDesc != null) _tutDesc.text = step.Desc;
            if (_tutStepNo != null)
                _tutStepNo.text = (IdleMvp.Core.TutorialService.StepIndex + 1) + "/" + IdleMvp.Core.TutorialService.Steps.Length;
            if (_tutGo != null) _tutGo.gameObject.SetActive(!string.IsNullOrEmpty(step.Screen));
            if (_tutSkip != null) _tutSkip.SetActive(true);
            if (_tutNextLabel != null) _tutNextLabel.text = "다음";
            if (_tutNext != null) _tutNext.interactable = true;
        }

        void BuildBottomNav(Transform root)
        {
            // thin EXP bar at absolute bottom (no label — 12px tall)
            _expBar = MapleUiTheme.Bar(root, "Exp", UiKit.ExpColor, withLabel: false);
            var er = _expBar.Go.GetComponent<RectTransform>();
            er.anchorMin = new Vector2(0f, 0f);
            er.anchorMax = new Vector2(1f, 0f);
            er.offsetMin = Vector2.zero;
            er.offsetMax = new Vector2(0f, 12f);

            var dock = MapleUiTheme.Strip(root, "Dock");
            if (GrowArt.BottomBar != null)
            {
                dock.sprite = GrowArt.BottomBar;
                dock.type = Image.Type.Sliced;
                // Kit bar is light lavender — tint charcoal so inverse labels read.
                dock.color = new Color(0.20f, 0.21f, 0.27f, 0.98f);
            }
            var rt = dock.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = new Vector2(0f, 12f);
            rt.offsetMax = new Vector2(0f, 96f);

            var nav = UiKit.HStack(dock.transform, "Nav", 4f, 8, 8, 6, 6, TextAnchor.MiddleCenter, true);
            UiKit.Fill(nav);
            _navBgs = new Image[NavLabels.Length];
            _navDots = new Image[NavLabels.Length];
            for (int i = 0; i < NavLabels.Length; i++)
            {
                int idx = i;
                var bg = UiKit.Img(nav, "N" + i, new Color(1f, 1f, 1f, 0.04f));
                // Bg_Tab gradient strip as the selected-state highlight (tinted transparent when off).
                var tabSp = GrowArt.TabStrip;
                if (tabSp != null)
                {
                    bg.sprite = tabSp;
                    bg.type = Image.Type.Sliced;
                    bg.color = new Color(1f, 1f, 1f, 0.05f);
                }
                var btn = bg.gameObject.AddComponent<Button>();
                btn.targetGraphic = bg;
                UiKit.Press(btn);
                _navBgs[i] = bg;

                var iconSp = GrowArt.NavOff(idx);
                if (iconSp != null)
                {
                    var ic = UiKit.Sprite(bg.transform, "Icon", iconSp);
                    var ir = ic.rectTransform;
                    ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.68f);
                    ir.sizeDelta = new Vector2(38f, 38f);
                }
                var t = UiKit.TmpLabel(bg.transform, "L", NavLabels[i], UiKit.TmpCaption, UiKit.TextInverseDim,
                    bold: true, TextAlignmentOptions.Center);
                t.enableWordWrapping = false;
                var trt = t.rectTransform;
                trt.anchorMin = new Vector2(0f, 0f);
                trt.anchorMax = new Vector2(1f, 0f);
                trt.pivot = new Vector2(0.5f, 0f);
                trt.offsetMin = new Vector2(2f, 6f);
                trt.offsetMax = new Vector2(-2f, 28f);

                var under = UiKit.Img(bg.transform, "Under", UiKit.Accent);
                var ur = under.rectTransform;
                ur.anchorMin = new Vector2(0.2f, 0f);
                ur.anchorMax = new Vector2(0.8f, 0f);
                ur.offsetMin = new Vector2(0f, 2f);
                ur.offsetMax = new Vector2(0f, 5f);
                under.enabled = false;
                under.raycastTarget = false;
                under.name = "Under";

                var ndot = MapleUiTheme.AlertDot(bg.transform, "Dot", 16f);
                var ndr = ndot.rectTransform;
                ndr.anchorMin = ndr.anchorMax = new Vector2(0.78f, 0.86f);
                ndot.gameObject.SetActive(false);
                _navDots[i] = ndot;

                btn.onClick.AddListener(() => OpenGrowth(idx));
            }
        }

        /// <summary>Bottom-right skill cluster + jump (Maple Idle), not a tall side column.</summary>
        void BuildSkillCluster(Transform root)
        {
            var wrap = UiKit.Rect(root, "SkillCluster");
            wrap.anchorMin = wrap.anchorMax = new Vector2(1f, 0f);
            wrap.pivot = new Vector2(1f, 0f);
            wrap.sizeDelta = new Vector2(220f, 220f);
            wrap.anchoredPosition = new Vector2(-10f, 100f);

            // 배경 판을 깔지 않는다 — 키트 라운드 스프라이트가 흰색이라 필드 위에
            // 허연 구름덩이처럼 떴다(유저 지적). 원형 버튼들이 각자 어두운 판을 가진다.

            var grid = UiKit.Grid(wrap, "Skills", new Vector2(72f, 72f), new Vector2(8f, 8f), 2);
            var gr = grid.GetComponent<RectTransform>();
            gr.anchorMin = new Vector2(0f, 0f);
            gr.anchorMax = new Vector2(1f, 1f);
            gr.offsetMin = new Vector2(0f, 80f);
            gr.offsetMax = new Vector2(-80f, 0f);

            for (int i = 0; i < 4; i++)
            {
                int id = i;
                // 민짜 사각형 → 키트 원형 프레임. 전용 무협 아이콘이 꽉 차게 들어간다.
                var frame = UiKit.Img(grid, "Skill" + i, new Color(0.13f, 0.15f, 0.22f, 0.95f));
                if (GrowArt.CircleFrame != null)
                { frame.sprite = GrowArt.CircleFrame; frame.type = Image.Type.Simple; }
                var btn = frame.gameObject.AddComponent<Button>();
                btn.targetGraphic = frame;
                UiKit.Press(btn);

                var iconSp = GrowArt.SkillIcon(i);
                if (iconSp != null)
                {
                    var ic = UiKit.Sprite(frame.transform, "Icon", iconSp);
                    UiKit.Fill(ic.rectTransform, 8f);
                    ic.color = GrowArt.SkillTint(i);
                }

                // 쿨타임 라디얼 — 어두운 덮개가 시계 반대방향으로 걷힌다
                _skillCdFill[i] = UiKit.Img(frame.transform, "CdFill", new Color(0f, 0f, 0f, 0.62f));
                if (GrowArt.CircleFrame != null)
                {
                    _skillCdFill[i].sprite = GrowArt.CircleFrame;
                    _skillCdFill[i].type = Image.Type.Filled;
                    _skillCdFill[i].fillMethod = Image.FillMethod.Radial360;
                    _skillCdFill[i].fillOrigin = (int)Image.Origin360.Top;
                    _skillCdFill[i].fillClockwise = false;
                }
                UiKit.Fill(_skillCdFill[i].rectTransform);
                _skillCdFill[i].raycastTarget = false;
                _skillCdFill[i].fillAmount = 0f;

                _skillCd[i] = UiKit.TmpLabel(frame.transform, "Cd", "", UiKit.TmpHeader, UiKit.TextInverse,
                    bold: true, TextAlignmentOptions.Center);
                UiKit.Fill(_skillCd[i].rectTransform);

                btn.onClick.AddListener(() =>
                {
                    if (_battle == null) return;
                    string msg = _battle.TryCastSkill(id);
                    if (!string.IsNullOrEmpty(msg)) Toast(msg);
                    RefreshHud();
                });
            }

            // 점프 버튼: 키트 원형 버튼 (색만 칠한 사각형이 아니라)
            var jump = UiKit.Img(wrap, "Jump", new Color(0.12f, 0.14f, 0.18f, 0.85f));
            var jumpSp = CasualArt.C("Button_Circle128_Dark");
            if (jumpSp != null) { jump.sprite = jumpSp; jump.color = Color.white; }
            else { jump.sprite = MapleLightTheme.RoundedSprite(999); jump.type = Image.Type.Sliced; }
            var jr = jump.rectTransform;
            jr.anchorMin = jr.anchorMax = new Vector2(1f, 0f);
            jr.pivot = new Vector2(1f, 0f);
            jr.sizeDelta = new Vector2(88f, 88f);
            jr.anchoredPosition = Vector2.zero;
            var jBtn = jump.gameObject.AddComponent<Button>();
            jBtn.targetGraphic = jump;
            UiKit.Press(jBtn);
            var jLabel = UiKit.TmpLabel(jump.transform, "L", "점프", UiKit.TmpBody, UiKit.TextInverse,
                bold: true, TextAlignmentOptions.Center);
            UiKit.Fill(jLabel.rectTransform);
            jBtn.onClick.AddListener(() => _battle?.PlayHeroJump());
        }

        // =====================================================================
        // Growth modals
        // =====================================================================

        void BuildGrowthModals()
        {
            BuildCharModal();
            BuildEquipModal();
            BuildSkillModal();
            BuildWeaponModal();
            BuildCompanionModal();
        }

        void BuildCharModal()
        {
            _charModal = _modals.CreateDual("Char", "캐릭터", footer: false, leftWidth: 400f);
            MapleLightTheme.SkinDemoPage(_charModal, "Char");
            var left = _charModal.LeftRail;
            var c = _charModal.Content;

            // 킷 데모 페이지: 중앙 대형 캐릭터 + 바닥 타원 그림자 (Heroes 화면 구도)
            var heroShadow = UiKit.Img(_charModal.Go.transform, "HeroShadow", new Color(0.01f, 0.05f, 0.14f, 0.32f));
            heroShadow.sprite = MapleLightTheme.RoundedSprite(48);
            heroShadow.type = Image.Type.Sliced;
            heroShadow.raycastTarget = false;
            var hsRt = heroShadow.rectTransform;
            hsRt.anchorMin = hsRt.anchorMax = new Vector2(0.5f, 0.5f);
            hsRt.sizeDelta = new Vector2(230f, 42f);
            hsRt.anchoredPosition = new Vector2(-30f, -234f);
            var charPrev = CharacterPreview.Attach(_charModal.Go.transform, "CharBody", 620, 700, 2.15f, 0.80f, live: true);
            var cprRt = charPrev.Rect;
            cprRt.anchorMin = cprRt.anchorMax = new Vector2(0.5f, 0.5f);
            cprRt.pivot = new Vector2(0.5f, 0.5f);
            cprRt.sizeDelta = new Vector2(620f, 700f);
            cprRt.anchoredPosition = new Vector2(-30f, 20f);

            // ref 04 좌측: 다크 CP 필 + 밝은 배경 스탯 라인 (공/방/HP/MP/크확/크뎀/공속)
            // job label (demo EPIC badge + name spot)
            var jobRow = UiKit.HStack(left, "JobRow", 10f, 4, 0, 0, 0, TextAnchor.MiddleLeft);
            UiKit.Fix(jobRow, -1f, 44f);
            // 직업명 배지('무사'). Label_Trapezoid는 세로 9슬라이스가 없어(y=0,w=0) 60→38로 눌리면
            // 모양이 찌그러진다. 4면 슬라이스되는 카드 프레임에 세력색을 입힌다.
            var jobBadge = UiKit.Img(jobRow, "JobBadge", new Color(0.46f, 0.30f, 0.85f, 1f));
            jobBadge.sprite = CasualArt.C("CardFrame01_Bg") ?? CasualArt.CardRound
                ?? MapleLightTheme.RoundedSprite(8);
            jobBadge.type = UnityEngine.UI.Image.Type.Sliced;
            UiKit.Fix(jobBadge, 132f, 42f);
            var jobRim = UiKit.Img(jobBadge.transform, "Rim", new Color(0.80f, 0.62f, 1f, 0.75f));
            jobRim.sprite = CasualArt.C("BorderFrame_Round01_Blue");
            jobRim.type = UnityEngine.UI.Image.Type.Sliced;
            jobRim.raycastTarget = false;
            UiKit.Fill(jobRim.rectTransform);
            _charJobBadge = UiKit.TmpLabel(jobBadge.transform, "T", "모험가", UiKit.TmpCaption - 2, Color.white,
                bold: true, TextAlignmentOptions.Center);
            _charJobBadge.enableWordWrapping = false;
            UiKit.Fill(_charJobBadge.rectTransform, 2f);

            MapleLightTheme.CpPill(left, "CpPill", out var cpVal);
            var atkLine = MapleLightTheme.NavyRow(left, "AtkL", "공격력", 54f, GrowArt.IconEnhance("Attack"));
            var defLine = MapleLightTheme.NavyRow(left, "DefL", "방어력", 54f, GrowArt.IconEnhance("Accuracy"));
            var hpLine = MapleLightTheme.NavyRow(left, "HpL", "최대 HP", 54f, GrowArt.IconEnhance("Hp"));
            _charMpLine = MapleLightTheme.NavyRow(left, "Mp", "최대 MP", 54f, GrowArt.IconXp);
            _charCritLine = MapleLightTheme.NavyRow(left, "Crit", "크리티컬 확률", 54f, GrowArt.IconEnhance("Accuracy"));
            _charCritDmgLine = MapleLightTheme.NavyRow(left, "CritD", "크리티컬 데미지", 54f, GrowArt.IconEnhance("Attack"));
            _charSpdLine = MapleLightTheme.NavyRow(left, "Spd", "공격 속도", 54f, GrowArt.IconEnhance("AttackSpeed"));
            _charStats = new StatSummaryView
            {
                Go = left.gameObject, Cp = cpVal,
                Atk = atkLine.Value, Hp = hpLine.Value, Def = defLine.Value
            };

            var charBtnRow = UiKit.HStack(_charModal.Go.transform, "CharActions", 14f, 0, 0, 0, 0, TextAnchor.MiddleCenter, true);
            var cbrRt = charBtnRow.GetComponent<RectTransform>();
            cbrRt.anchorMin = cbrRt.anchorMax = new Vector2(0.5f, 0f);
            cbrRt.pivot = new Vector2(0.5f, 0f);
            cbrRt.sizeDelta = new Vector2(560f, 64f);
            cbrRt.anchoredPosition = new Vector2(-30f, 26f);
            MapleUiTheme.AccentButton(charBtnRow, "Look", "외형 꾸미기", () =>
            {
                if (_appearance != null) _modals.Open(_appearance.Modal);
            }, UiKit.FontCaption);
            MapleUiTheme.YellowButton(charBtnRow, "Job", "전직", () =>
            {
                if (_jobModal != null) _modals.Open(_jobModal);
            }, UiKit.FontCaption);
            _charPointsBanner = MapleUiTheme.InfoChip(left, "PointsChip", "잔여 스탯 —", 56f);

            // ref 04 우측: 등급 바 한 줄 + 강화 카드 3장(그라데이션) + 특별 능력치 2열 행
            _gradeProgress = MapleLightTheme.GradeBar(c, "MapleGrade", "내공 등급");
            _gradeRow = _gradeProgress;
            _pointRow = MapleLightTheme.NavyRow(c, "Pts", "스탯 포인트");
            _cpRow = MapleLightTheme.NavyRow(c, "Cp", "전투력");

            MapleLightTheme.Section(c, "능력치 강화");
            var primary = UiKit.HStack(c, "PrimaryStats", 10f, 0, 0, 0, 0, TextAnchor.MiddleCenter, true);
            UiKit.Fix(primary, -1f, 248f);
            _atkRow = MapleLightTheme.InvestCardWide(primary, "Atk", "주 스탯 · 공격", "능력치 강화", () => BuyStat("ATK"));
            _defRow = MapleLightTheme.InvestCardWide(primary, "Def", "방어력", "능력치 강화", () => BuyStat("DEF"));
            _hpRow = MapleLightTheme.InvestCardWide(primary, "Hp", "최대 HP", "능력치 강화", () => BuyStat("HP"));

            MapleLightTheme.Section(c, "특별 능력치");
            var specGrid = UiKit.FillGrid(c, "SpecGrid", new Vector2(380f, 84f), new Vector2(10f, 10f), 2, 4);
            _specDmgRow = MapleLightTheme.SpecialRow(specGrid, "SpecDmg", "최종 데미지", "+1", () => BuySpecial("DMG"), false, null);
            _specGoldRow = MapleLightTheme.SpecialRow(specGrid, "SpecGold", "골드 획득", "+1", () => BuySpecial("GOLD"), false, null);
            _specIdleRow = MapleLightTheme.SpecialRow(specGrid, "SpecIdle", "방치 효율", "+1", () => BuySpecial("IDLE"), false, null);
            _talentRow = MapleLightTheme.SpecialRow(specGrid, "Talent", "스킬 특성", "RD 10", () =>
            {
                var msg = SkillAdapter.Instance?.InvestTalent();
                if (!string.IsNullOrEmpty(msg)) Toast(msg);
                RefreshAll();
            }, false, null);
            _potionEnhRow = MapleLightTheme.SpecialRow(specGrid, "PotionEnh", "물약 강화", "강화", BuyPotionUpgrade, false, null);
            MapleLightTheme.SpecialRow(specGrid, "LockCrit", "크리티컬 확률", null, null, true, "등급 5 달성 필요");
            MapleLightTheme.SpecialRow(specGrid, "LockAcc", "명중", null, null, true, "레벨 20 달성 필요");

            MapleLightTheme.Section(c, "승급");
            _ascendBar = MapleUiTheme.Bar(c, "AscBar", FantasyKitSlots.KitTeal, true);
            UiKit.Fix(_ascendBar.Go.transform, -1f, 34f);
            var ascTrack = _ascendBar.Go.GetComponent<UnityEngine.UI.Image>();
            if (ascTrack != null) ascTrack.color = new Color(0.09f, 0.13f, 0.26f, 1f); // navy track on blue page
            _ascendInfo = MapleUiTheme.InfoChip(c, "AscInfo", "스탯 투자 시 자동 승급", 72f);

            _charPanels = null; // tabs removed
            _charModal.Refresh = () =>
            {
                var p = PlayerGrowth.Instance;
                if (p == null) return;
                _charStats?.RefreshFromCombat();
                _charStats?.SetAvatar(GrowArt.Hero);
                bool can = p.StatPoints > 0;
                const string costTxt = "스탯 1";
                int perGrade = IdleMvp.Core.BalanceConfig.Data.pointsPerGrade;
                int spentInGrade = p.SpentStatPoints % Mathf.Max(1, perGrade);
                int remain = Mathf.Max(0, perGrade - spentInGrade);
                float gradePct = perGrade > 0 ? (float)spentInGrade / perGrade : 0f;

                if (_charMpLine?.Value != null)
                    _charMpLine.Value.text = UiKit.Num(CombatPowerService.GetMaxMp());
                if (_charCritLine?.Value != null)
                    _charCritLine.Value.text = $"{CombatPowerService.GetCritRatePct():0.#}%";
                if (_charCritDmgLine?.Value != null)
                    _charCritDmgLine.Value.text = $"{CombatPowerService.GetCritDamagePct():0.#}%";
                if (_charSpdLine?.Value != null)
                    _charSpdLine.Value.text = $"{CombatPowerService.GetAttackSpeedPct():0.#}%";

                if (_charPointsBanner != null)
                {
                    double rd = WalletAdapter.Instance != null ? WalletAdapter.Instance.RedDiamond : 0;
                    _charPointsBanner.text =
                        $"잔여 스탯 {p.StatPoints:N0}  ·  특수 {p.SpecialStatPoints:N0}  ·  RD {UiKit.Num(rd)}";
                }

                if (_gradeProgress != null)
                {
                    if (_gradeProgress.Label != null) _gradeProgress.Label.text = "내공 등급";
                    _gradeProgress.Value.text = $"{p.Grade} 단계";
                    _gradeProgress.Progress?.Set(gradePct, $"{spentInGrade}/{perGrade}");
                }
                if (_cpRow?.Value != null) _cpRow.Value.text = UiKit.Num(CombatPowerService.GetTotalCp());
                if (_pointRow?.Value != null) _pointRow.Value.text = $"잔여 {p.StatPoints:N0} · 특수 {p.SpecialStatPoints:N0}";

                var bd = CombatPowerService.GetBreakdown();
                float compMul = CompanionAdapter.Instance != null
                    ? 1f + CompanionAdapter.Instance.PassiveAtkPct * 0.01f
                    : 1f;

                void SyncInvest(StatRowView row, string level, string bonus, string preview, string cost, bool enabled, string cta)
                {
                    if (row == null) return;
                    if (row.Level != null) row.Level.text = level;
                    if (row.Bonus != null) row.Bonus.text = bonus;
                    if (row.Value != null && row.Cost != null && ReferenceEquals(row.Value, row.Cost))
                        row.Value.text = string.IsNullOrEmpty(preview) ? cost : $"{preview} · {cost}";
                    else
                    {
                        if (row.Value != null) row.Value.text = preview;
                        if (row.Cost != null) row.Cost.text = cost;
                    }
                    var bl = row.Action?.GetComponentInChildren<TMP_Text>();
                    if (bl != null) bl.text = cta;
                    UiKit.SetEnabled(row.Action, enabled);
                }

                SyncInvest(_atkRow, $"{p.Atk}/∞", $"+{bd.Atk:0.#}",
                    $"다음 {bd.Atk + compMul:0.#}", costTxt, can, can ? "능력치 강화" : "포인트 부족");
                SyncInvest(_defRow, $"{p.Def}/∞", $"+{bd.Def:0.#}",
                    $"다음 {bd.Def + 1f:0.#}", costTxt, can, can ? "능력치 강화" : "포인트 부족");
                SyncInvest(_hpRow, $"{p.Hp}/∞", $"+{bd.MaxHp:0}",
                    $"다음 {bd.MaxHp + 50f:0}", costTxt, can, can ? "능력치 강화" : "포인트 부족");


                bool canSpec = p.SpecialStatPoints > 0;
                string specCost = $"특수 1";
                SyncInvest(_specDmgRow, $"{(int)(p.SpecFinalDmgPct / 1.5f)}/20", $"+{p.SpecFinalDmgPct:0.#}%",
                    $"→ +{p.SpecFinalDmgPct + 1.5f:0.#}%", specCost, canSpec, canSpec ? "+1" : "포인트 부족");
                SyncInvest(_specGoldRow, $"{(int)(p.SpecGoldPct / 2f)}/20", $"+{p.SpecGoldPct:0.#}%",
                    $"→ +{p.SpecGoldPct + 2f:0.#}%", specCost, canSpec, canSpec ? "+1" : "포인트 부족");
                SyncInvest(_specIdleRow, $"{(int)(p.SpecIdlePct / 2.5f)}/20", $"+{p.SpecIdlePct:0.#}%",
                    $"→ +{p.SpecIdlePct + 2.5f:0.#}%", specCost, canSpec, canSpec ? "+1" : "포인트 부족");

                bool potMax = Core.PotionService.IsMaxLevel;
                double potCost = Core.PotionService.UpgradeCostGold;
                bool canPot = !potMax && WalletAdapter.Instance != null && WalletAdapter.Instance.Gold >= potCost;
                int potLv = Core.PotionService.Level;
                SyncInvest(_potionEnhRow, $"{potLv}/{Core.PotionService.MaxLevel}",
                    $"회복 {Core.PotionService.HealPct * 100f:0}% · 쿨 {Core.PotionService.CooldownSec:0.#}초",
                    potMax ? "최대 강화 완료"
                           : $"→ 회복 {Core.PotionService.HealPctAt(potLv + 1) * 100f:0}% · 쿨 {Core.PotionService.CooldownSecAt(potLv + 1):0.#}초",
                    potMax ? "" : $"골드 {UiKit.Num(potCost)}", canPot,
                    potMax ? "완료" : (canPot ? "강화" : "골드 부족"));

                _ascendBar?.Set(gradePct, $"{spentInGrade}/{perGrade} · 다음 등급까지 {remain}회");
                if (_ascendInfo != null)
                    _ascendInfo.text =
                        $"현재 등급 {p.Grade} · 특수 {p.SpecialStatPoints} · 매 {perGrade}회 투자 시 자동 승급 (+{IdleMvp.Core.BalanceConfig.Data.specialStatPerGrade})";

                if (_charJobBadge != null)
                {
                    var jobDef = IdleMvp.Core.JobProgress.Current;
                    _charJobBadge.text = jobDef != null ? jobDef.name : "모험가";
                }

                var sk = SkillAdapter.Instance;
                if (sk != null)
                {
                    double rdHave = WalletAdapter.Instance != null ? WalletAdapter.Instance.RedDiamond : 0;
                    SyncInvest(_talentRow, $"{sk.TalentPointsSpent}P", $"+{sk.PassiveMasteryPct:0.#}%",
                        "패시브 마스터리", $"RD {UiKit.Num(rdHave)}", sk.CanInvestTalent(), "RD 10");
                }
            };
        }

        void ShowCharPanel(int idx)
        {
            if (_charPanels == null) return;
            for (int i = 0; i < _charPanels.Length; i++)
                if (_charPanels[i] != null) _charPanels[i].SetActive(i == idx);
        }

        void BuildEquipModal()
        {
            _equipModal = _modals.CreateDual("Equip", "장비 관리", footer: true, leftWidth: 420f);
            MapleLightTheme.SkinDemoPage(_equipModal, "Equip", wideContent: true);
            var left = _equipModal.LeftRail;
            var c = _equipModal.Content;
            _equipSelected = 0;

            _equipStats = MapleUiTheme.StatSummary(left, "EquipStats", withAvatar: false);
            MapleUiTheme.SectionHeader(left, "장착 미리보기");
            // demo-page style: transparent preview zone + ground shadow (no legacy colored card box)
            var previewCol = UiKit.VStack(left, "PreviewCol", 8f, 0, 0, 4, 4, TextAnchor.UpperCenter);
            MapleUiTheme.StretchFullWidth(previewCol);

            _equipPreviewAvatar = UiKit.Img(previewCol, "Avatar", new Color(0f, 0f, 0f, 0f));
            _equipPreviewAvatar.raycastTarget = false;
            UiKit.Fix(_equipPreviewAvatar, -1f, 250f);
            MapleUiTheme.StretchFullWidth(_equipPreviewAvatar);
            var eqShadow = UiKit.Img(_equipPreviewAvatar.transform, "Shadow", new Color(0.01f, 0.05f, 0.14f, 0.35f));
            eqShadow.sprite = CasualArt.C("BasicFrame_Circle77") ?? MapleLightTheme.RoundedSprite(28);
            eqShadow.type = Image.Type.Simple;
            eqShadow.raycastTarget = false;
            var eqsRt = eqShadow.rectTransform;
            eqsRt.anchorMin = eqsRt.anchorMax = new Vector2(0.5f, 0f);
            eqsRt.pivot = new Vector2(0.5f, 0f);
            eqsRt.sizeDelta = new Vector2(150f, 30f);
            eqsRt.anchoredPosition = new Vector2(0f, 4f);
            const int eqPrevW = 260, eqPrevH = 300;
            var equipPrev = CharacterPreview.Attach(_equipPreviewAvatar.transform, "EquipBody",
                eqPrevW, eqPrevH, 1.35f, 0.85f, live: true);
            // Fill로 늘리면 260x300 렌더텍스처가 가로로 퍼져 캐릭터가 세로로 찌부러진다.
            // 원본 비율(260:300)을 지킨 고정 크기로 가운데 배치한다.
            {
                float boxH = 250f - 12f;
                var pr = equipPrev.Rect;
                pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
                pr.pivot = new Vector2(0.5f, 0.5f);
                pr.sizeDelta = new Vector2(boxH * eqPrevW / eqPrevH, boxH);
                pr.anchoredPosition = new Vector2(0f, 6f);
            }
            _equipPreviewFace = null; // legacy static face removed

            var slotGrid = UiKit.Grid(previewCol, "Slots", new Vector2(64f, 64f), new Vector2(8f, 8f), 3, TextAnchor.MiddleCenter);
            UiKit.Fix(slotGrid, -1f, 152f);
            MapleUiTheme.StretchFullWidth(slotGrid);
            _equipPreviewSlots = new ItemCardView[6];
            _equipPreviewLv = new Text[6];
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                var slotGo = UiKit.Img(slotGrid, "Prev" + i, Color.white);
                FantasyKitSlots.FrameRarity(slotGo, 0, 64f);
                var sle = slotGo.gameObject.GetComponent<LayoutElement>() ?? slotGo.gameObject.AddComponent<LayoutElement>();
                sle.preferredWidth = 64f;
                sle.preferredHeight = 64f;
                sle.minWidth = 64f;
                sle.minHeight = 64f;
                var ic = UiKit.Img(slotGo.transform, "I", Color.white);
                ic.sprite = GrowArt.IconGear(i);
                ic.preserveAspect = true;
                UiKit.Fill(ic.rectTransform, 8f);
                var lv = UiKit.Label(slotGo.transform, "Lv", "", UiKit.FontCaption, UiKit.TextInverse,
                    FontStyle.Bold, TextAnchor.LowerCenter);
                var lrt = lv.rectTransform;
                lrt.anchorMin = new Vector2(0f, 0f);
                lrt.anchorMax = new Vector2(1f, 0.35f);
                lrt.offsetMin = new Vector2(2f, 2f);
                lrt.offsetMax = new Vector2(-2f, 0f);
                MapleUiTheme.FieldTextOutline(lv);
                _equipPreviewLv[i] = lv;
                var btn = slotGo.gameObject.AddComponent<Button>();
                btn.targetGraphic = slotGo;
                btn.onClick.AddListener(() =>
                {
                    _equipSelected = idx;
                    RefreshAll();
                });
                _equipPreviewSlots[i] = new ItemCardView { Go = slotGo.gameObject, Icon = ic };
            }

            var walletRow = MapleUiTheme.ToolBar(c, "Wallet");
            _equipGoldChip = MapleUiTheme.CurrencyChip(walletRow, "Gold", GrowArt.IconGold, UiKit.GoldColor);
            UiKit.Fix(_equipGoldChip.Go.transform, -1f, 36f);
            _equipStoneChip = MapleUiTheme.CurrencyChip(walletRow, "Stone", GrowArt.IconStone, UiKit.Accent);
            UiKit.Fix(_equipStoneChip.Go.transform, -1f, 36f);
            _equipScrollChip = MapleUiTheme.CurrencyChip(walletRow, "Scroll", null, FantasyKitSlots.KitTeal);
            UiKit.Fix(_equipScrollChip.Go.transform, -1f, 36f);
            _equipSfChip = MapleUiTheme.CurrencyChip(walletRow, "Sf", GrowArt.IconAscend(1), FantasyKitSlots.KitTeal);
            UiKit.Fix(_equipSfChip.Go.transform, -1f, 36f);
            _equipCpBanner = MapleUiTheme.InfoChip(c, "EquipCp", "총 CP —", 48f);
            if (_equipCpBanner != null)
            {
                _equipCpBanner.alignment = TextAnchor.MiddleCenter;
                _equipCpBanner.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            MapleUiTheme.SectionHeader(c, "가방");
            _equipEmptyHint = MapleUiTheme.InfoChip(c, "EmptyBag",
                "획득한 장비가 없습니다. 엘리트 몬스터를 소환하여 장비를 획득해보세요!", 64f);
            MapleUiTheme.SectionHeader(c, "장착 슬롯");
            var inv = InventoryAdapter.Instance;
            int slotCount = inv != null ? inv.Slots.Length : 6;
            // 무기창 카드와 같은 모양으로 보여준다(사용자 요청: 5번 스크린샷처럼).
            var equipCell = new Vector2(178f, 250f);
            var grid = UiKit.FillGrid(c, "Slots", equipCell, new Vector2(10f, 10f), 3, 6);
            _equipSlotGrid = grid;
            _equipCells = new HeroCardView[slotCount];
            BuildEquipSlotCards(slotCount);

            MapleUiTheme.SectionHeader(c, "선택 슬롯 강화");
            _enhanceInfo = MapleUiTheme.InfoChip(c, "EnhanceInfo", "", 56f);
            var row = UiKit.HStack(c, "EnhanceRow", UiKit.Space2, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(row, -1f, 56f);
            var b1 = MapleUiTheme.AccentButton(row, "Scroll", "주문서", () =>
                ConfirmSlotEnhance("주문서 강화", "주문의 흔적", CurrencyId.ScrollTrace, 1,
                    () => SlotEnhanceService.Instance?.TryScroll(_equipSelected)));
            UiKit.Fix(b1, -1f, 52f);
            var b2 = MapleUiTheme.AccentButton(row, "Star", "스타포스", () =>
                ConfirmSlotEnhance("스타포스 강화", "스타포스 주문서", CurrencyId.StarForceScroll, 1,
                    () => SlotEnhanceService.Instance?.TryStarForce(_equipSelected)));
            UiKit.Fix(b2, -1f, 52f);
            var b3 = MapleUiTheme.PrimaryButton(row, "Pot", "잠재능력", () =>
                ConfirmSlotEnhance("잠재능력 재설정", "미라클 큐브", CurrencyId.MiracleCube, 1,
                    () => SlotEnhanceService.Instance?.TryPotential(_equipSelected)));
            UiKit.Fix(b3, -1f, 52f);
            var b4 = MapleUiTheme.PrimaryButton(row, "Add", "에디셔널", () =>
                ConfirmSlotEnhance("에디셔널 잠재 (12성 해금)", "애디셔널 큐브", CurrencyId.AdditionalCube, 1,
                    () => SlotEnhanceService.Instance?.TryAdditional(_equipSelected)));
            UiKit.Fix(b4, -1f, 52f);

            _equipUpgradeBtn = MapleUiTheme.AccentButton(c, "Upgrade", "장비 강화", () =>
            {
                EquipmentService.Instance?.TryUpgrade(_equipSelected, PlayerWallet.Instance);
                RefreshAll();
            });
            UiKit.Fix(_equipUpgradeBtn, -1f, 56f);

            // 엘리트 소환 조작은 메인 HUD(HP/MP 왼쪽)로 옮겼다 — BuildEliteCluster 참조.

            _equipCapLabel = MapleUiTheme.InfoChip(_equipModal.Footer, "Cap", "0 / 300", 40f);
            UiKit.Fix(_equipCapLabel.rectTransform, 160f, 40f);
            var dis = MapleUiTheme.PrimaryButton(_equipModal.Footer, "Dis", "일괄 분해", () =>
            {
                var msg = InventoryAdapter.Instance?.DisassembleJunk(1) ?? "인벤 없음";
                Toast(msg);
                RefreshAll();
            });
            UiKit.Fix(dis, 220f, 64f);

            _equipModal.Refresh = () =>
            {
                _equipStats?.RefreshFromCombat();
                var i2 = InventoryAdapter.Instance;
                if (i2 != null)
                {
                    // 장착 슬롯은 HeroCard라 통째로 다시 그린다 (프레임당 1회로 합쳐져 있어 안전)
                    BuildEquipSlotCards(Mathf.Min(6, i2.Slots.Length));
                    if (_equipPreviewSlots != null)
                    {
                        for (int i = 0; i < _equipPreviewSlots.Length && i < i2.Slots.Length; i++)
                        {
                            var s = i2.Slots[i];
                            var img = _equipPreviewSlots[i].Go.GetComponent<Image>();
                            var edge = GrowArt.Rarity(i == _equipSelected ? Mathf.Max(s.rarity, 2) : s.rarity);
                            if (edge != null) FantasyKitSlots.SharpRarity(img, edge);
                            if (_equipPreviewSlots[i].Icon != null)
                            {
                                // 무기 슬롯(0)은 실장착 무기의 아이콘/틴트를 그대로 반영
                                var eqW = i == 0 ? WeaponSummonAdapter.Instance?.Equipped : null;
                                _equipPreviewSlots[i].Icon.sprite = eqW != null
                                    ? GrowArt.IconWeaponId(eqW.catalogId, eqW.kind)
                                    : GrowArt.IconGear(i);
                                _equipPreviewSlots[i].Icon.color = eqW != null
                                    ? (GrowArt.WeaponIconIsDedicated(eqW.catalogId)
                                        ? Color.white : GrowArt.WeaponTint(eqW.kind, eqW.rarity))
                                    : (i == _equipSelected
                                        ? Color.white
                                        : new Color(0.75f, 0.75f, 0.8f, 1f));
                            }
                            if (_equipPreviewLv != null && i < _equipPreviewLv.Length && _equipPreviewLv[i] != null)
                                _equipPreviewLv[i].text = $"Lv.{s.level}";
                            // Selection via tint only — avoid scale piercing neighbors.
                        }
                    }
                }
                var states = SlotEnhanceService.Instance?.States;
                var slot = states != null && _equipSelected < states.Length ? states[_equipSelected] : null;
                string slotName = i2 != null ? i2.SlotLabel(_equipSelected) : "슬롯";
                int enhLv = i2 != null && _equipSelected < i2.Slots.Length ? i2.Slots[_equipSelected].level : 0;
                _enhanceInfo.text = slot != null
                    ? $"{slotName} · 강화 Lv.{enhLv} · 주문서 {slot.scrollSuccess} · 스타포스 {slot.starForce}"
                      + $" · 잠재 {SlotEnhanceService.RankName(slot.potentialRank)}"
                      + (slot.starForce >= 12 ? $" · 에디셔널 {SlotEnhanceService.RankName(slot.addRank)}" : " · 에디셔널 잠금(12성)")
                    : "슬롯 정보 없음";
                if (_eliteInfo != null)
                    _eliteInfo.text = $"엘리트 소환 레벨 {EliteSummonService.Instance?.SummonLevel ?? 1} · 처치 시 장비 획득";

                double goldHave = WalletAdapter.Instance?.Gold ?? 0;
                double stoneHave = 0;
                if (EquipmentService.Instance != null) stoneHave += EquipmentService.Instance.EnhanceStones;
                if (CurrencyWallet.Instance != null)
                {
                    stoneHave += CurrencyWallet.Instance.Get(CurrencyId.ArmorStone);
                    stoneHave += CurrencyWallet.Instance.Get(CurrencyId.WeaponEnhanceStone);
                }
                double trace = CurrencyWallet.Instance != null ? CurrencyWallet.Instance.Get(CurrencyId.ScrollTrace) : 0;
                double sf = CurrencyWallet.Instance != null ? CurrencyWallet.Instance.Get(CurrencyId.StarForceScroll) : 0;
                if (_equipGoldChip?.Value != null) _equipGoldChip.Value.text = UiKit.Num(goldHave);
                if (_equipStoneChip?.Value != null) _equipStoneChip.Value.text = UiKit.Num(stoneHave);
                if (_equipScrollChip?.Value != null) _equipScrollChip.Value.text = UiKit.Num(trace);
                if (_equipSfChip?.Value != null) _equipSfChip.Value.text = UiKit.Num(sf);
                if (_equipCpBanner != null)
                {
                    var ebd = CombatPowerService.GetBreakdown();
                    float eqPart = ebd.ArmorCp + ebd.SlotCp + ebd.ArmorAtk * 10f;
                    _equipCpBanner.text = $"총 CP {UiKit.Num(ebd.TotalCp)}  ·  장비 기여 {UiKit.Num(eqPart)}";
                }

                if (_equipCapLabel != null)
                {
                    int n = i2 != null ? i2.Slots.Length : 0;
                    _equipCapLabel.text = $"{n} / 300";
                }
                if (_equipEmptyHint != null && _equipEmptyHint.transform.parent != null)
                {
                    bool any = false;
                    if (i2 != null)
                        for (int i = 0; i < i2.Slots.Length; i++)
                            if (i2.Slots[i].level > 0 || i2.Slots[i].rarity > 0) { any = true; break; }
                    _equipEmptyHint.transform.parent.gameObject.SetActive(!any);
                }

                var eqCost = EquipmentService.Instance != null ? EquipmentService.Instance.UpgradeGoldCost(_equipSelected) : 0f;
                var label = _equipUpgradeBtn.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = $"장비 강화 (비용 {UiKit.Num(eqCost)} / 보유 {UiKit.Num(goldHave)})";
                UiKit.SetEnabled(_equipUpgradeBtn, goldHave >= eqCost);

                var weap = WeaponSummonAdapter.Instance?.Equipped;
                if (_equipPreviewFace != null)
                {
                    int kind = weap != null ? weap.kind : 0;
                    int rar = weap != null ? weap.rarity : 0;
                    _equipPreviewFace.sprite = GrowArt.IconGear(0);
                    _equipPreviewFace.color = GrowArt.WeaponTint(kind, rar);
                }
                // preview zone stays transparent (demo-page look) — no legacy rarity ring
            };
        }

        void BuildSkillModal()
        {
            _skillModal = _modals.CreateDual("Skill", "스킬", footer: false, leftWidth: 420f);
            MapleLightTheme.SkinDemoPage(_skillModal, "Skill", wideContent: true);
            var left = _skillModal.LeftRail;
            var c = _skillModal.Content;
            _skillSelected = 0;

            MapleUiTheme.SectionHeader(left, "스킬 상세");
            _skillDetail = FantasyKitSlots.SkillDetailPanel(left, "Detail", () =>
            {
                var msg = SkillAdapter.Instance?.PerformNodeAction(_skillSelected);
                if (!string.IsNullOrEmpty(msg)) Toast(msg);
                RefreshAll();
            });
            _skillCpBanner = MapleUiTheme.InfoChip(left, "SkillCp", "총 CP —", 56f);

            MapleUiTheme.TabBar(c, "SkillTabs", new[] { "스킬 목록", "스킬 강화", "마스터리" }, ShowSkillPanel);

            _skillPanels = new GameObject[3];
            for (int p = 0; p < 3; p++)
            {
                var panel = UiKit.VStack(c, "SkillPanel" + p, UiKit.Space2, 0, 0, 0, 0);
                var fle = panel.gameObject.AddComponent<LayoutElement>();
                fle.flexibleWidth = 1f;
                fle.minHeight = 420f;
                _skillPanels[p] = panel.gameObject;
            }

            var list = _skillPanels[0].transform;
            var activeBox = MapleUiTheme.SectionBox(list, "ActiveBox");
            MapleUiTheme.SectionHeader(activeBox, "액티브 스킬");
            _skillTreeHint = MapleUiTheme.InfoChip(activeBox, "TreeHint", "액티브 4종은 순서대로 해금 · 패시브는 레벨 달성 후 습득", 48f);

            // demo stage-map: hex nodes connected by path lines
            var hexCell = new Vector2(150f, 208f);
            var grid = UiKit.HStack(activeBox, "ActiveGrid", 0f, 12, 12, 12, 8, TextAnchor.UpperLeft);
            UiKit.Fix(grid, -1f, 216f);
            _skillTiles = new ItemCardView[8];
            _skillRows = new StatRowView[8];
            for (int i = 0; i < 4; i++)
            {
                int id = i;
                if (i > 0) CasualCards.HexLink(grid, hexCell.x * 0.5f);
                var card = CasualCards.HexNode(grid, "S" + i, GrowArt.SkillIcon(i), GrowArt.SkillTint(i),
                    () => SelectSkillNode(id), hexCell);
                _skillTiles[i] = card;
                _skillRows[i] = new StatRowView { Go = card.Go, Value = card.Sub, Cost = card.Title, Action = card.Button };
            }
            var passiveBox = MapleUiTheme.SectionBox(list, "PassiveBox");
            MapleUiTheme.SectionHeader(passiveBox, "패시브 스킬");
            var pgrid = UiKit.HStack(passiveBox, "PassiveGrid", 0f, 12, 12, 12, 8, TextAnchor.UpperLeft);
            UiKit.Fix(pgrid, -1f, 216f);
            for (int i = 4; i < 8; i++)
            {
                int id = i;
                if (i > 4) CasualCards.HexLink(pgrid, hexCell.x * 0.5f);
                var card = CasualCards.HexNode(pgrid, "S" + i, GrowArt.SkillIcon(i), GrowArt.SkillTint(i),
                    () => SelectSkillNode(id), hexCell);
                _skillTiles[i] = card;
                _skillRows[i] = new StatRowView { Go = card.Go, Value = card.Sub, Cost = card.Title, Action = card.Button };
            }

            var enh = _skillPanels[1].transform;
            MapleUiTheme.SectionHeader(enh, "노드 습득 · 강화");
            _skillEnhanceRows = new StatRowView[8];
            for (int i = 0; i < 8; i++)
            {
                int id = i;
                var node = SkillTreeDef.Nodes[i];
                _skillEnhanceRows[i] = FantasyKitSlots.EnhanceRow(enh, "Up" + i, node.Name, GrowArt.SkillIcon(i), "습득",
                    () =>
                    {
                        SelectSkillNode(id);
                        var msg = SkillAdapter.Instance?.PerformNodeAction(id);
                        if (!string.IsNullOrEmpty(msg)) Toast(msg);
                        RefreshAll();
                    });
            }

            var mas = _skillPanels[2].transform;
            MapleUiTheme.SectionHeader(mas, "패시브 · 마스터리");
            _skillTalentRow = FantasyKitSlots.EnhanceRow(mas, "Talent", "특성 투자", GrowArt.IconEnhance("AttackSpeed"),
                "RD 10", () =>
                {
                    var msg = SkillAdapter.Instance?.InvestTalent();
                    if (!string.IsNullOrEmpty(msg)) Toast(msg);
                    RefreshAll();
                });
            _passDmgRow = FantasyKitSlots.InfoRow(mas, "PDmg", "최종 데미지%", GrowArt.IconEnhance("Attack"));
            _passGoldRow = FantasyKitSlots.InfoRow(mas, "PGold", "골드 획득%", GrowArt.IconGold);
            _passIdleRow = FantasyKitSlots.InfoRow(mas, "PIdle", "방치 효율%", GrowArt.IconXp);

            ShowSkillPanel(0);
            _skillModal.Refresh = () => RefreshSkillModal();
        }

        void SelectSkillNode(int id)
        {
            _skillSelected = Mathf.Clamp(id, 0, 7);
            RefreshSkillModal();
        }

        void OpenSkillAt(int id)
        {
            _skillSelected = Mathf.Clamp(id, 0, 7);
            ShowSkillPanel(0);
            _modals.Open(_skillModal);
            RefreshSkillModal();
        }

        void RefreshSkillModal()
        {
            var s2 = SkillAdapter.Instance;
            if (s2 == null) return;
            s2.RefreshUnlocks();
            if (_skillCpBanner != null)
            {
                var sbd = CombatPowerService.GetBreakdown();
                _skillCpBanner.text = $"총 CP {UiKit.Num(sbd.TotalCp)} · 스킬 기여 {UiKit.Num(sbd.SkillCp)} · 배율 x{sbd.OutgoingMul:0.##}";
            }

            for (int i = 0; i < 8; i++)
            {
                var node = SkillTreeDef.Nodes[i];
                int nlv = s2.NodeLevel[i];
                var action = s2.GetNodeAction(i);
                bool selected = i == _skillSelected;

                string sub;
                switch (action)
                {
                    case SkillNodeAction.LockedLevel:
                        sub = $"잠금\nLv.{node.ReqLevel}";
                        break;
                    case SkillNodeAction.LockedPrereq:
                        sub = "선행 필요";
                        break;
                    case SkillNodeAction.Learn:
                        sub = "습득 가능";
                        break;
                    case SkillNodeAction.Max:
                        sub = $"Lv.{nlv}/{node.MaxLevel}\nMAX";
                        break;
                    default:
                        sub = $"Lv.{nlv}/{node.MaxLevel}";
                        break;
                }

                if (_skillTiles != null && i < _skillTiles.Length && _skillTiles[i] != null)
                {
                    _skillTiles[i].Sub.text = sub;
                    _skillTiles[i].Title.text = "";

                    bool dim = action == SkillNodeAction.LockedLevel || action == SkillNodeAction.LockedPrereq;
                    if (_skillTiles[i].Icon != null)
                    {
                        _skillTiles[i].Icon.sprite = GrowArt.SkillIcon(i);
                        _skillTiles[i].Icon.color = dim ? new Color(0.45f, 0.45f, 0.5f, 1f) : GrowArt.SkillTint(i);
                    }
                    // hex node state: locked = slate hex, open = blue, selected = glow ring
                    var hexImg = _skillTiles[i].Go.GetComponent<Image>();
                    if (hexImg != null)
                    {
                        var hexSprite = CasualArt.C(dim ? "Button_Hexagon199_White_Bg" : "Button_Hexagon199_Blue");
                        if (hexSprite != null)
                        {
                            hexImg.sprite = hexSprite;
                            hexImg.color = dim ? new Color(0.40f, 0.47f, 0.60f, 1f) : Color.white;
                        }
                        var glowT = _skillTiles[i].Go.transform.Find("Glow");
                        if (glowT != null) glowT.gameObject.SetActive(selected && !dim);
                    }
                }

                if (_skillEnhanceRows != null && i < _skillEnhanceRows.Length && _skillEnhanceRows[i] != null)
                {
                    var row = _skillEnhanceRows[i];
                    row.Value.text = nlv > 0 ? $"Lv.{nlv}/{node.MaxLevel}" : "미습득";
                    if (row.Cost != null) row.Cost.text = s2.NodeStatusLine(i);
                    var lbl = row.Action?.GetComponentInChildren<TMP_Text>();
                    if (lbl != null) lbl.text = s2.ActionButtonLabel(i);
                    UiKit.SetEnabled(row.Action, s2.CanPerformAction(i));
                }
            }

            if (_skillTreeHint != null)
                _skillTreeHint.text = "액티브 4종은 순서대로 해금 · 패시브는 레벨 달성 후 습득";

            RefreshSkillDetail(s2);

            if (_skillTalentRow != null)
            {
                double rdHave = WalletAdapter.Instance != null ? WalletAdapter.Instance.RedDiamond : 0;
                _skillTalentRow.Value.text = $"누적 {s2.TalentPointsSpent} · 마스터리 {s2.PassiveMasteryPct:0.#}%";
                if (_skillTalentRow.Cost != null) _skillTalentRow.Cost.text = $"보유 RD {UiKit.Num(rdHave)}";
                var talentLbl = _skillTalentRow.Action?.GetComponentInChildren<TMP_Text>();
                if (talentLbl != null) talentLbl.text = "RD 10";
                UiKit.SetEnabled(_skillTalentRow.Action, s2.CanInvestTalent());
            }
            if (_passDmgRow?.Value != null) _passDmgRow.Value.text = $"+{s2.PassiveDmgPct:0.#}%";
            if (_passGoldRow?.Value != null) _passGoldRow.Value.text = $"+{s2.PassiveGoldPct:0.#}%";
            if (_passIdleRow?.Value != null) _passIdleRow.Value.text = $"+{s2.PassiveIdlePct:0.#}%";
        }

        void RefreshSkillDetail(SkillAdapter s2)
        {
            if (_skillDetail == null || s2 == null) return;
            int id = _skillSelected;
            var node = SkillTreeDef.Nodes[id];
            int nlv = s2.NodeLevel[id];
            if (_skillDetail.Icon != null)
            {
                _skillDetail.Icon.sprite = GrowArt.SkillIcon(id);
                _skillDetail.Icon.color = Color.white;
            }
            if (_skillDetail.Title != null) _skillDetail.Title.text = node.Name;
            if (_skillDetail.Rank != null)
                _skillDetail.Rank.text = nlv <= 0 ? "미습득" : $"Lv.{nlv} / {node.MaxLevel}";
            if (_skillDetail.LevelBar != null)
                _skillDetail.LevelBar.Set(nlv / (float)Mathf.Max(1, node.MaxLevel), $"{nlv}/{node.MaxLevel}");
            if (_skillDetail.Effect != null) _skillDetail.Effect.text = s2.EffectPreview(id);
            if (_skillDetail.Desc != null) _skillDetail.Desc.text = node.Description ?? "";

            s2.GetActionCosts(id, out double gold, out double stone, out int rd);
            double haveGold = WalletAdapter.Instance?.Gold ?? 0;
            double haveStone = 0;
            if (EquipmentService.Instance != null) haveStone += EquipmentService.Instance.EnhanceStones;
            if (CurrencyWallet.Instance != null) haveStone += CurrencyWallet.Instance.Get(CurrencyId.WeaponEnhanceStone);
            double haveRd = WalletAdapter.Instance?.RedDiamond ?? 0;

            string costLine = s2.GetNodeAction(id) == SkillNodeAction.Learn
                ? $"골드 {gold:0}{(stone > 0 ? $" · 강화석 {stone:0.#}" : "")}"
                : $"골드 {gold:0} · 강화석 {stone:0.#} · RD {rd}";
            if (_skillDetail.Cost != null)
            {
                _skillDetail.Cost.text = costLine;
                bool shortGold = haveGold < gold;
                bool shortStone = haveStone < stone;
                bool shortRd = haveRd < rd;
                _skillDetail.Cost.color = (shortGold || shortStone || shortRd) ? UiKit.Danger : FantasyKitSlots.KitTeal;
            }
            if (_skillDetail.Reason != null)
            {
                string reason = s2.ActionReason(id);
                _skillDetail.Reason.text = reason ?? "";
                _skillDetail.Reason.color = string.IsNullOrEmpty(reason) ? FantasyKitSlots.KitTeal : UiKit.Danger;
            }
            if (_skillDetail.ActionLabel != null)
                _skillDetail.ActionLabel.text = s2.ActionButtonLabel(id);
            UiKit.SetEnabled(_skillDetail.Action, s2.CanPerformAction(id));
        }

        void ShowSkillPanel(int idx)
        {
            if (_skillPanels == null) return;
            for (int i = 0; i < _skillPanels.Length; i++)
                if (_skillPanels[i] != null) _skillPanels[i].SetActive(i == idx);
        }

        void BuildWeaponModal()
        {
            _weaponModal = _modals.CreateDual("Weapon", "무기", footer: true, leftWidth: 380f);
            MapleLightTheme.SkinDemoPage(_weaponModal, "Weapon", wideContent: true);
            var left = _weaponModal.LeftRail;
            var c = _weaponModal.Content;

            _weaponStats = MapleUiTheme.StatSummary(left, "WeaponStats", withAvatar: false);
            MapleUiTheme.SectionHeader(left, "선택 무기");
            _weaponEqCard = FantasyKitSlots.PortraitCard(left, "Equipped", "장착 없음", "", GrowArt.IconSummonWeapon, GrowArt.Rarity(0), null, -1f, 180f);
            _weaponEquipped = _weaponEqCard.Sub;
            _weaponStars = MapleUiTheme.InfoChip(left, "Stars", "★☆☆☆☆", 36f);
            _weaponEquipEffect = FantasyKitSlots.InfoRow(left, "EqFx", "장착 효과", GrowArt.IconEnhance("Attack"), 52f);
            _weaponOwnEffect = FantasyKitSlots.InfoRow(left, "OwnFx", "보유 효과", GrowArt.IconPlus, 52f);
            _weaponLockBtn = MapleUiTheme.SecondaryButton(left, "LockEq", "장착 무기 잠금", () =>
            {
                var weq = WeaponSummonAdapter.Instance?.Equipped;
                if (weq == null) { Toast("장착된 무기가 없습니다"); return; }
                WeaponSummonAdapter.Instance.ToggleLock(weq.id);
                RefreshAll();
            }, UiKit.FontCaption);
            UiKit.Fix(_weaponLockBtn, -1f, 48f);

            var tool = MapleUiTheme.ToolBar(c, "WeaponTool");
            // 누르자마자 실행하지 않고, 필요 재료·보유량을 보여주는 확인창을 먼저 띄운다
            var one = MapleUiTheme.SecondaryButton(tool, "One", "1회 소환",
                () => ConfirmWeaponSummon(1));
            UiKit.Fix(one, 140f, 56f);
            var ten = MapleUiTheme.AccentButton(tool, "Ten", "10회 소환",
                () => ConfirmWeaponSummon(10));
            UiKit.Fix(ten, 160f, 56f);
            var up = MapleUiTheme.PrimaryButton(tool, "Up", "강화", ConfirmWeaponUpgrade);
            UiKit.Fix(up, 120f, 56f);

            MapleUiTheme.SectionHeader(c, "보유 무기");
            MapleUiTheme.TabBar(c, "WeaponFilter", new[] { "전체", "장착중", "에픽+" }, idx =>
            {
                _weaponFilter = idx;
                RefreshAll();
            }, 52f);
            _weaponGrid = UiKit.FillGrid(c, "Owned", new Vector2(190f, 268f), new Vector2(14f, 14f), 4, 8);

            var autoEq = MapleUiTheme.AccentButton(_weaponModal.Footer, "AutoEq", "자동장착", () =>
            {
                Toast(WeaponSummonAdapter.Instance?.EquipBest() ?? "무기 없음");
                FieldAutoHuntController.Instance?.RefreshHeroAppearance();
                RefreshAll();
            });
            UiKit.Fix(autoEq, 160f, 64f);
            var mastery = MapleUiTheme.PrimaryButton(_weaponModal.Footer, "Mastery", "무기 마스터리", () =>
                Toast(WeaponSummonAdapter.Instance?.LevelUpEquipped() ?? "무기 없음"));
            UiKit.Fix(mastery, 180f, 64f);
            var junk = MapleUiTheme.SecondaryButton(_weaponModal.Footer, "Junk", "일괄분해", () =>
            {
                Toast(WeaponSummonAdapter.Instance?.DisassembleJunk(1) ?? "무기 없음");
                RefreshAll();
            });
            UiKit.Fix(junk, 140f, 64f);

            _weaponModal.Refresh = () =>
            {
                _weaponStats?.RefreshFromCombat();
                var w = WeaponSummonAdapter.Instance;
                if (w == null) return;
                var eq = w.Equipped;
                if (eq != null)
                {
                    _weaponEqCard.Title.text = eq.name;
                    _weaponEquipped.text =
                        $"등급 {eq.rarity} · Lv.{eq.level} · ★{eq.awaken}\n" +
                        $"무기 기여 {UiKit.Num(w.EquippedWeaponCp)}";
                    if (_weaponStars != null)
                    {
                        int stars = Mathf.Clamp(eq.awaken + 1, 1, 5);
                        _weaponStars.text = new string('★', stars) + new string('☆', 5 - stars);
                    }
                    if (_weaponEquipEffect?.Value != null)
                        _weaponEquipEffect.Value.text = $"ATK +{w.EquippedWeaponAtk:0.#}";
                    if (_weaponOwnEffect?.Value != null)
                        _weaponOwnEffect.Value.text = $"CP +{UiKit.Num(w.EquippedWeaponCp)}";
                    if (_weaponStats != null)
                        _weaponStats.SetExtra(eq != null ? $"장착 ATK +{w.EquippedWeaponAtk:0.#}  ·  기여 {UiKit.Num(w.EquippedWeaponCp)}" : "");
                    FantasyKitSlots.SharpRarity(_weaponEqCard.Go.GetComponent<Image>(), GrowArt.Rarity(eq.rarity));
                    if (_weaponEqCard.Icon != null)
                    {
                        _weaponEqCard.Icon.sprite = GrowArt.IconWeaponId(eq.catalogId, eq.kind);
                        _weaponEqCard.Icon.color = GrowArt.WeaponTint(eq.kind, eq.rarity);
                    }
                }
                else
                {
                    _weaponEqCard.Title.text = "장착 없음";
                    _weaponEquipped.text = "소환으로 무기를 획득하세요";
                    if (_weaponStars != null) _weaponStars.text = "☆☆☆☆☆";
                    if (_weaponEquipEffect?.Value != null) _weaponEquipEffect.Value.text = "—";
                    if (_weaponOwnEffect?.Value != null) _weaponOwnEffect.Value.text = "—";
                    _weaponStats?.SetExtra("");
                }

                ClearChildren(_weaponGrid);

                var wCell = _weaponGrid.GetComponent<GridLayoutGroup>() != null
                    ? _weaponGrid.GetComponent<GridLayoutGroup>().cellSize
                    : new Vector2(150f, 240f);
                int minRarity = _weaponFilter == 2 ? 2 : 0;
                bool eqOnly = _weaponFilter == 1;
                var sorted = w.GetSortedOwned(minRarity, eqOnly);
                for (int i = 0; i < sorted.Count; i++)
                {
                    var item = sorted[i];
                    int kind = item.kind != 0 ? item.kind : WeaponItem.KindFromName(item.name);
                    string tag = item.equipped ? "장착" : item.locked ? "잠금" : $"등급 {item.rarity}";
                    string id = item.id;
                    string nm = item.name;
                    var wTile = CasualCards.HeroCard(_weaponGrid, "W" + i, item.name,
                        GrowArt.IconWeaponId(item.catalogId, kind), GrowArt.WeaponTint(kind, item.rarity),
                        item.rarity, item.awaken, 5,
                        item.level.ToString(), Mathf.Clamp01(item.count / 5f), tag, false,
                        () =>
                        {
                            WeaponSummonAdapter.Instance?.Equip(id);
                            Toast(nm + " 장착");
                            RefreshAll();
                        }, wCell);
                }

                // Lock control lives on the left rail (acts on equipped weapon).
                if (_weaponLockBtn != null)
                {
                    UiKit.SetEnabled(_weaponLockBtn, eq != null);
                    var wl = _weaponLockBtn.GetComponentInChildren<TMP_Text>();
                    if (wl != null) wl.text = eq != null && eq.locked ? "장착 무기 잠금 해제" : "장착 무기 잠금";
                }
                _weaponModal.Title.text = $"무기  (보유 {w.Owned.Count} · 소환 Lv.{w.SummonLevel})";
            };
        }

        void BuildCompanionModal()
        {
            _compModal = _modals.CreateDual("Comp", "동료", footer: true, leftWidth: 400f);
            MapleLightTheme.SkinDemoPage(_compModal, "Comp", wideContent: true);
            var left = _compModal.LeftRail;
            var c = _compModal.Content;

            _compStats = MapleUiTheme.StatSummary(left, "CompStats", withAvatar: false);
            MapleUiTheme.SectionHeader(left, "메인 동료");
            _compMainCard = FantasyKitSlots.PortraitCard(left, "Main", "메인", "비어 있음", GrowArt.IconAscend(2), GrowArt.Rarity(2),
                null, -1f, 140f);
            _compSummonEffect = MapleUiTheme.InfoChip(left, "SummonFx", "소환 효과 —", 72f);
            _compInfo = MapleUiTheme.InfoChip(left, "CompInfo", "", 56f);

            // Selected companion action panel — tiles stay clean, actions live here.
            MapleUiTheme.SectionHeader(left, "선택 동료");
            _compSelLabel = UiKit.TmpLabel(left, "SelName", "타일을 선택하세요", UiKit.TmpBody, UiKit.TextInverseDim, bold: true);
            _compSelLabel.enableWordWrapping = false;
            UiKit.Fix(_compSelLabel, -1f, 30f);
            var selRow1 = UiKit.HStack(left, "SelRow1", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(selRow1, -1f, 52f);
            _compSelMain = MapleUiTheme.AccentButton(selRow1, "Main", "메인 배치", () =>
            {
                if (string.IsNullOrEmpty(_compSelId)) return;
                CompanionAdapter.Instance?.SetMain(_compSelId);
                Toast("메인 배치");
                RefreshAll();
            }, UiKit.FontCaption);
            _compSelSub = MapleUiTheme.SecondaryButton(selRow1, "Sub", "서브 지정", () =>
            {
                if (string.IsNullOrEmpty(_compSelId)) return;
                bool on = CompanionAdapter.Instance != null && CompanionAdapter.Instance.SetSub(_compSelId);
                Toast(on ? "서브 지정" : "서브 해제");
                RefreshAll();
            }, UiKit.FontCaption);
            var selRow2 = UiKit.HStack(left, "SelRow2", 8f, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(selRow2, -1f, 52f);
            _compSelAwaken = MapleUiTheme.PrimaryButton(selRow2, "Awaken", "각성", () =>
            {
                if (string.IsNullOrEmpty(_compSelId)) return;
                Toast(CompanionAdapter.Instance?.TryAwaken(_compSelId));
                RefreshAll();
            }, UiKit.FontCaption);
            _compSelLock = MapleUiTheme.SecondaryButton(selRow2, "Lock", "잠금", () =>
            {
                if (string.IsNullOrEmpty(_compSelId)) return;
                CompanionAdapter.Instance?.ToggleLock(_compSelId);
                RefreshAll();
            }, UiKit.FontCaption);

            MapleUiTheme.SectionHeader(left, "서브 동료");
            var subHost = UiKit.Img(left, "SubHost", FantasyKitSlots.KitPanel);
            subHost.sprite = CasualArt.CardRound;
            subHost.type = subHost.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            subHost.color = new Color(0.10f, 0.16f, 0.32f, 0.95f);
            UiKit.Fix(subHost, -1f, 120f);
            MapleUiTheme.StretchFullWidth(subHost);
            var subRow = UiKit.HStack(subHost.transform, "SubSlots", 4f, 8, 8, 8, 10, TextAnchor.MiddleLeft, true);
            UiKit.Fill(subRow);
            _compSubSlots = new ItemCardView[6];
            for (int i = 0; i < 6; i++)
            {
                bool locked = i >= 2;
                _compSubSlots[i] = FantasyKitSlots.PortraitCard(subRow, "Sub" + i,
                    locked ? "잠금" : "서브",
                    locked ? $"Lv.{3 + i * 2}" : "빈칸",
                    locked ? GrowArt.IconLock : GrowArt.CircleFrame,
                    GrowArt.Rarity(0), null, 52f, 96f);
            }
            _compSubCard = _compSubSlots[0];

            var presetRow = UiKit.HStack(left, "Presets", 6f, 4, 4, 4, 8, TextAnchor.MiddleLeft, true);
            UiKit.Fix(presetRow, -1f, 44f);
            _compPreset = PlayerPrefs.GetInt("IdleGrow.Maple.CompPreset", 1);
            _compPresetBtns = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                int preset = i + 1;
                _compPresetBtns[i] = MapleUiTheme.SecondaryButton(presetRow, "P" + preset, preset.ToString(), () =>
                {
                    _compPreset = preset;
                    PlayerPrefs.SetInt("IdleGrow.Maple.CompPreset", preset);
                    PlayerPrefs.Save();
                    Toast($"프리셋 {preset} 적용");
                    RefreshAll();
                }, UiKit.FontBody);
                UiKit.Fix(_compPresetBtns[i], 48f, 40f);
            }

            var tool = MapleUiTheme.ToolBar(c, "CompTool");
            var one = MapleUiTheme.SecondaryButton(tool, "One", "1회 소환",
                () => ConfirmCompanionPull(1));
            UiKit.Fix(one, 130f, 56f);
            var ten = MapleUiTheme.AccentButton(tool, "Ten", "10회 소환",
                () => ConfirmCompanionPull(10));
            UiKit.Fix(ten, 140f, 56f);
            var field = MapleUiTheme.PrimaryButton(tool, "Field", "필드 소환", () =>
            {
                if (CompanionCombatBridge.Instance == null)
                {
                    Toast("전투 브릿지 없음");
                    return;
                }
                bool ok = CompanionCombatBridge.Instance.TrySummon(out string msg);
                Toast(msg);
                if (ok) FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                RefreshAll();
            });
            UiKit.Fix(field, 130f, 56f);
            _compAutoBtn = MapleUiTheme.SecondaryButton(tool, "Auto", "자동 OFF", () =>
            {
                var ca = CompanionAdapter.Instance;
                if (ca == null) return;
                ca.AutoSummon = !ca.AutoSummon;
                Toast("자동소환 " + (ca.AutoSummon ? "ON" : "OFF"));
                RefreshAll();
            });
            UiKit.Fix(_compAutoBtn, 120f, 56f);

            MapleUiTheme.SectionHeader(c, "배치");
            _compDeployRow = UiKit.HStack(c, "Deploy", 16f, 4, 4, 4, 4, TextAnchor.MiddleLeft);
            UiKit.Fix(_compDeployRow, -1f, 224f);

            MapleUiTheme.SectionHeader(c, "보유");
            MapleUiTheme.TabBar(c, "CompFilter", new[] { "전체", "배치중", "에픽+" }, idx =>
            {
                _compFilter = idx;
                RefreshAll();
            }, 52f);
            _compGrid = UiKit.FillGrid(c, "Owned", new Vector2(190f, 268f), new Vector2(14f, 14f), 4, 8);

            var deploy = MapleUiTheme.AccentButton(_compModal.Footer, "Deploy", "자동 장착", () =>
            {
                Toast(CompanionAdapter.Instance?.DeployBest() ?? "동료 없음");
                FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                RefreshAll();
            });
            UiKit.Fix(deploy, 180f, 64f);
            var bulkLv = MapleUiTheme.PrimaryButton(_compModal.Footer, "BulkLv", "일괄 레벨업", () =>
            {
                var ca = CompanionAdapter.Instance;
                if (ca == null) { Toast("동료 없음"); return; }
                int n = 0;
                var sorted = ca.GetSortedOwned(0, false);
                for (int i = 0; i < sorted.Count && i < 5; i++)
                {
                    var msg = ca.TryAwaken(sorted[i].id);
                    if (!string.IsNullOrEmpty(msg) && msg.IndexOf("실패", System.StringComparison.Ordinal) < 0) n++;
                }
                Toast(n > 0 ? $"일괄 각성/레벨 {n}건" : "재료 부족 또는 대상 없음");
                RefreshAll();
            });
            UiKit.Fix(bulkLv, 180f, 64f);

            BuildSummonResultModal();

            _compModal.Refresh = () =>
            {
                _compStats?.RefreshFromCombat();
                var ca = CompanionAdapter.Instance;
                if (ca == null) return;
                if (ca.Main != null)
                {
                    _compMainCard.Title.text = "메인";
                    _compMainCard.Sub.text = $"{ca.Main.name} Lv.{ca.Main.level}";
                    FantasyKitSlots.SharpRarity(_compMainCard.Go.GetComponent<Image>(), GrowArt.Rarity(ca.Main.rarity));
                    if (_compMainCard.Icon != null)
                    {
                        _compMainCard.Icon.sprite = GrowArt.IconCompanion(ca.Main.name, ca.Main.rarity);
                        _compMainCard.Icon.color = GrowArt.CompanionTint(ca.Main.rarity);
                    }
                }
                else
                {
                    _compMainCard.Sub.text = "비어 있음";
                }

                var subs = ca.GetSubs();
                int maxSub = ca.MaxSubSlots;
                if (_compSubSlots != null)
                {
                    for (int i = 0; i < _compSubSlots.Length; i++)
                    {
                        var slot = _compSubSlots[i];
                        if (slot == null) continue;
                        bool unlocked = i < maxSub;
                        if (!unlocked)
                        {
                            slot.Title.text = "잠금";
                            slot.Sub.text = $"소환 Lv.{3 + i * 2}";
                            continue;
                        }
                        if (i < subs.Count)
                        {
                            slot.Title.text = $"서브{i + 1}";
                            slot.Sub.text = $"Lv.{subs[i].level}";
                            FantasyKitSlots.SharpRarity(slot.Go.GetComponent<Image>(), GrowArt.Rarity(subs[i].rarity));
                            if (slot.Icon != null)
                            {
                                slot.Icon.sprite = GrowArt.IconCompanion(subs[i].name, subs[i].rarity);
                                slot.Icon.color = GrowArt.CompanionTint(subs[i].rarity);
                            }
                        }
                        else
                        {
                            slot.Title.text = "서브";
                            slot.Sub.text = "비어 있음";
                        }
                    }
                }

                if (_compSummonEffect != null)
                    _compSummonEffect.text = ca.Main != null
                        ? $"소환 효과 · {ca.Main.name} 계승 ATK {ca.PassiveAtkPct:0.#}% · 자동소환 {(ca.AutoSummon ? "ON" : "OFF")}"
                        : "메인 동료를 배치하면 소환 효과가 표시됩니다";

                _compInfo.text =
                    $"보유 {ca.OwnedCount}  ·  서브 {ca.SubCount}/{maxSub}  ·  동료 CP {UiKit.Num(ca.CompanionCp)}\n" +
                    $"소환 Lv.{ca.SummonLevel} · 프리셋 {_compPreset}";
                _compStats?.SetExtra($"총 CP {UiKit.Num(CombatPowerService.GetTotalCp())}");
                if (_compPresetBtns != null)
                {
                    for (int i = 0; i < _compPresetBtns.Length; i++)
                    {
                        var img = _compPresetBtns[i]?.GetComponent<Image>();
                        if (img != null)
                            img.color = (i + 1) == _compPreset ? FantasyKitSlots.KitTeal : FantasyKitSlots.KitPanel;
                    }
                }

                if (_compDeployRow != null)
                {
                    ClearChildren(_compDeployRow);
                    var all = ca.GetSortedOwned(0, false);
                    var mainItem = all.Find(x => x.main);
                    var deployCell = new Vector2(150f, 212f);
                    if (mainItem != null)
                    {
                        var mc = CasualCards.HeroCard(_compDeployRow, "DepMain", mainItem.name,
                            GrowArt.IconCompanion(mainItem.name, mainItem.rarity), GrowArt.CompanionTint(mainItem.rarity),
                            mainItem.rarity, mainItem.awaken, 5, mainItem.level.ToString(),
                            Mathf.Clamp01(mainItem.level / 50f), "메인", false, null, deployCell);
                    }
                    else
                        BuildDeploySlotCard(_compDeployRow, "DepMainEmpty", "메인", false, deployCell);
                    var deploySubs = all.FindAll(x => x.sub);
                    for (int si = 0; si < 5; si++)
                    {
                        if (si < deploySubs.Count)
                        {
                            var it = deploySubs[si];
                            CasualCards.HeroCard(_compDeployRow, "DepSub" + si, it.name,
                                GrowArt.IconCompanion(it.name, it.rarity), GrowArt.CompanionTint(it.rarity),
                                it.rarity, it.awaken, 5, it.level.ToString(),
                                Mathf.Clamp01(it.level / 50f), "서브", false, null, deployCell);
                        }
                        else
                            BuildDeploySlotCard(_compDeployRow, "DepEmpty" + si, "추가", false, deployCell);
                    }
                }

                ClearChildren(_compGrid);

                var cCell = _compGrid.GetComponent<GridLayoutGroup>() != null
                    ? _compGrid.GetComponent<GridLayoutGroup>().cellSize
                    : new Vector2(150f, 230f);
                int minRarity = _compFilter == 2 ? 2 : 0;
                bool deployedOnly = _compFilter == 1;
                var sorted = ca.GetSortedOwned(minRarity, deployedOnly);
                bool selFound = false;
                for (int i = 0; i < sorted.Count; i++)
                {
                    var item = sorted[i];
                    string tag = item.main ? "메인" : item.sub ? "서브" : item.locked ? "잠금" : $"Lv.{item.level}";
                    string id = item.id;
                    string nm = item.name;
                    bool selected = id == _compSelId;
                    if (selected) selFound = true;
                    var tile = CasualCards.HeroCard(_compGrid, "C" + i, nm,
                        GrowArt.IconCompanion(nm, item.rarity), GrowArt.CompanionTint(item.rarity),
                        item.rarity, item.awaken, 5,
                        item.level.ToString(), Mathf.Clamp01(item.level / 50f), tag, false,
                        () =>
                        {
                            _compSelId = id;
                            RefreshAll();
                        }, cCell);
                    if (selected && tile.Name != null) tile.Name.color = new Color(1f, 0.92f, 0.35f, 1f);
                }

                if (!selFound) _compSelId = null;
                var selItem = selFound ? sorted.Find(x => x.id == _compSelId) : null;
                if (_compSelLabel != null)
                    _compSelLabel.text = selItem != null
                        ? $"{selItem.name} · ★{selItem.awaken}{(selItem.locked ? " · 잠금" : "")}"
                        : "타일을 선택하세요";
                UiKit.SetEnabled(_compSelMain, selItem != null);
                UiKit.SetEnabled(_compSelSub, selItem != null);
                UiKit.SetEnabled(_compSelAwaken, selItem != null);
                UiKit.SetEnabled(_compSelLock, selItem != null);
                var lockLbl = _compSelLock != null ? _compSelLock.GetComponentInChildren<TMP_Text>() : null;
                if (lockLbl != null) lockLbl.text = selItem != null && selItem.locked ? "잠금 해제" : "잠금";

                var label = _compAutoBtn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = ca.AutoSummon ? "자동 ON" : "자동 OFF";
            };
        }

        /// <summary>Demo Select Heroes empty slot: transparent navy round card + plus/lock.</summary>
        void BuildDeploySlotCard(Transform parent, string name, string label, bool locked, Vector2 cell)
        {
            var card = UiKit.Img(parent, name, new Color(0.07f, 0.13f, 0.30f, 0.75f));
            card.sprite = CasualArt.CardRound;
            card.type = UnityEngine.UI.Image.Type.Sliced;
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.minWidth = cell.x; le.preferredWidth = cell.x;
            le.minHeight = cell.y; le.preferredHeight = cell.y;
            le.flexibleWidth = 0f;
            var icon = locked ? GrowArt.IconLock : CasualArt.C("ResourceBar_Btn_Icon_Add");
            if (icon != null)
            {
                var ic = UiKit.Sprite(card.transform, "Icon", icon);
                ic.preserveAspect = true;
                ic.color = new Color(0.55f, 0.75f, 1f, 0.9f);
                var irt = ic.rectTransform;
                irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.58f);
                irt.sizeDelta = new Vector2(52f, 52f);
            }
            var t = UiKit.TmpLabel(card.transform, "T", label, UiKit.TmpCaption, new Color(0.62f, 0.80f, 1f, 1f),
                bold: true, TMPro.TextAlignmentOptions.Center);
            t.enableWordWrapping = false;
            var trt = t.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.16f); trt.anchorMax = new Vector2(1f, 0.38f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
        }

        void BuildSummonResultModal()
        {
            _summonResultModal = _modals.Create("SummonResult", "소환 결과", ModalSize.Medium);
            var c = _summonResultModal.Content;
            MapleUiTheme.SectionHeader(c, "획득 동료");
            _summonResultGrid = UiKit.Grid(c, "Results", new Vector2(150f, 240f), new Vector2(8f, 8f), 5);
            var ok = MapleUiTheme.PrimaryButton(_summonResultModal.Footer, "Ok", "확인", () =>
            {
                _modals.Close();
                if (!Casual.CasualScreens.Open("comp") && _compModal != null)
                    _modals.Open(_compModal);
                RefreshAll();
            });
            UiKit.Fix(ok, 200f, 64f);
        }

        void ShowSummonResults(System.Collections.Generic.List<CompanionSummonResult> results)
        {
            if (_summonResultModal == null || _summonResultGrid == null) BuildSummonResultModal();
            ClearChildren(_summonResultGrid);
            int n = 0;
            foreach (var r in results)
            {
                if (!r.Ok) continue;
                string tag = r.IsNew ? "신규" : r.LeveledUp ? "레벨업" : "중복";
                string cid = r.Id;
                string nm = r.Name;
                string amount = $"등급 {r.Rarity} · Lv.{r.Level}";
                var tile = FantasyKitSlots.RewardTile(_summonResultGrid, "R" + n, r.Name, amount,
                    GrowArt.IconCompanion(r.Name, r.Rarity), GrowArt.Rarity(r.Rarity),
                    new Vector2(150f, 240f), tag, withActionStrip: !string.IsNullOrEmpty(cid));
                if (tile.Icon != null) tile.Icon.color = GrowArt.CompanionTint(r.Rarity);
                if (!string.IsNullOrEmpty(cid) && tile.ActionStrip != null)
                {
                    var mainBtn = MapleUiTheme.AccentButton(tile.ActionStrip, "MainBtn", "메인", () =>
                    {
                        CompanionAdapter.Instance?.SetMain(cid);
                        Toast(nm + " 메인 배치");
                        RefreshAll();
                    });
                    UiKit.Fix(mainBtn, -1f, 32f);
                    var mle = mainBtn.GetComponent<LayoutElement>() ?? mainBtn.gameObject.AddComponent<LayoutElement>();
                    mle.flexibleWidth = 1f;

                    var subBtn = MapleUiTheme.SecondaryButton(tile.ActionStrip, "SubBtn", "서브", () =>
                    {
                        bool on = CompanionAdapter.Instance != null && CompanionAdapter.Instance.SetSub(cid);
                        Toast(on ? nm + " 서브 지정" : nm + " 서브 해제");
                        RefreshAll();
                    });
                    UiKit.Fix(subBtn, -1f, 32f);
                    var sle = subBtn.GetComponent<LayoutElement>() ?? subBtn.gameObject.AddComponent<LayoutElement>();
                    sle.flexibleWidth = 1f;
                }
                n++;
            }
            if (n == 0)
            {
                Toast(results.Count > 0 ? results[0].Message : "결과 없음");
                return;
            }
            _summonResultModal.Title.text = n >= 10 ? "10연 소환 결과" : $"소환 결과 ({n})";
            _modals.Open(_summonResultModal);
            RefreshAll();
        }

        // =====================================================================
        // Shop + popups
        // =====================================================================

        void BuildShopAndPopups()
        {
            BuildShopModal();

            // fast hunt
            _fastHuntModal = _modals.Create("FastHunt", "빠른 사냥", ModalSize.Small);
            var fc = _fastHuntModal.Content;
            MapleUiTheme.SectionHeader(fc, "즉시 보상");
            var fInfo = MapleUiTheme.InfoText(fc, "");
            var ad = MapleUiTheme.SecondaryButton(_fastHuntModal.Footer, "Ad", "광고 보기", () =>
            {
                AdBridge.Instance?.ShowRewarded("fast_hunt", () =>
                {
                    Toast(ShopAdapter.Instance?.FastReward() ?? "보상 지급");
                    RefreshAll();
                }, err => Toast(err ?? "광고 실패"));
            });
            UiKit.Fix(ad, 140f, 56f);
            if (GrowArt.IconAds != null)
            {
                var adIc = UiKit.Sprite(ad.transform, "AdIcon", GrowArt.IconAds);
                var art = adIc.rectTransform;
                art.anchorMin = art.anchorMax = new Vector2(0.12f, 0.5f);
                art.sizeDelta = new Vector2(28f, 28f);
            }
            var ticket = MapleUiTheme.SecondaryButton(_fastHuntModal.Footer, "Ticket", "티켓 사용", () => Toast(ShopAdapter.Instance?.FastReward()));
            UiKit.Fix(ticket, 140f, 56f);
            var gemBtn = MapleUiTheme.PrimaryButton(_fastHuntModal.Footer, "Gem", "보석으로 받기", () => Toast(ShopAdapter.Instance?.FastReward()));
            UiKit.Fix(gemBtn, 190f, 56f);
            _fastHuntModal.Refresh = () =>
            {
                fInfo.text = $"챕터 [{StageProgress.Instance?.GetDisplayLabel()}] 사냥 보상을 즉시 수령합니다.\n기대 보상: 골드 · 티켓 · 강화 재료";
            };
            // 물약 강화 — 사냥 화면 물약 버튼의 '+' 뱃지로 진입 (키우기류 표준: 골드 성장 축)
            _potionEnhModal = _modals.Create("PotionEnh", "물약 강화", ModalSize.Small);
            var pc = _potionEnhModal.Content;
            MapleUiTheme.SectionHeader(pc, "회복의 묘약");
            var pInfo = MapleUiTheme.InfoText(pc, "");
            var pUp = MapleUiTheme.PrimaryButton(_potionEnhModal.Footer, "Up", "강화", BuyPotionUpgrade);
            UiKit.Fix(pUp, 220f, 56f);
            _potionEnhModal.Refresh = () =>
            {
                int lv = Core.PotionService.Level;
                bool max = Core.PotionService.IsMaxLevel;
                double cost = Core.PotionService.UpgradeCostGold;
                double gold = WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0;
                pInfo.text =
                    $"Lv.{lv}/{Core.PotionService.MaxLevel} — 회복 {Core.PotionService.HealPct * 100f:0}% · 쿨타임 {Core.PotionService.CooldownSec:0.#}초\n"
                    + (max
                        ? "최대 강화 완료"
                        : $"다음 레벨: 회복 {Core.PotionService.HealPctAt(lv + 1) * 100f:0}% · 쿨타임 {Core.PotionService.CooldownSecAt(lv + 1):0.#}초\n"
                          + $"비용: 골드 {UiKit.Num(cost)} (보유 {UiKit.Num(gold)})");
                var pl = pUp.GetComponentInChildren<TMP_Text>();
                if (pl != null) pl.text = max ? "완료" : $"강화 (골드 {UiKit.Num(cost)})";
                UiKit.SetEnabled(pUp, !max && gold >= cost);
            };
            // 수련 — 벤치마크 '용사의 힘' 카피: 8트랙 단계 강화 + 어빌리티 리롤
            _trainingModal = _modals.Create("Training", "수련", ModalSize.Medium);
            var tc = _trainingModal.Content;
            _trainHeaderText = MapleUiTheme.InfoText(tc, "");
            MapleLightTheme.Section(tc, "수련 트랙");
            var tGrid = UiKit.FillGrid(tc, "TrainGrid", new Vector2(380f, 84f), new Vector2(10f, 10f), 2, 4);
            _trainRows = new StatRowView[Core.TrainingService.TrackCount];
            for (int i = 0; i < _trainRows.Length; i++)
            {
                int ti = i;
                _trainRows[i] = MapleLightTheme.SpecialRow(tGrid, "T" + i,
                    Core.TrainingService.TrackNames[i], "강화", () => OnTrainUpgrade(ti), false, null);
            }
            MapleLightTheme.Section(tc, "어빌리티 (3슬롯)");
            _trainAbilityText = MapleUiTheme.InfoText(tc, "");
            var rr = MapleUiTheme.PrimaryButton(_trainingModal.Footer, "Reroll", "어빌리티 변경", OnTrainReroll);
            UiKit.Fix(rr, 240f, 56f);
            _trainingModal.Refresh = RefreshTrainingModal;

            // 도감 — 이미 작동하던 수집 보너스의 가시화 (보이지 않는 성장축은 없는 것과 같다)
            _collectionModal = _modals.Create("Collection", "도감", ModalSize.Medium, footer: false);
            var cc = _collectionModal.Content;
            _collectionText = MapleUiTheme.InfoText(cc, "");
            _collectionModal.Refresh = () =>
            {
                if (_collectionText == null) return;
                var sb = new System.Text.StringBuilder();
                sb.Append($"몬스터 {Core.CollectionService.MonsterCollectedCount}종 · "
                    + $"무기 {Core.CollectionService.WeaponCollectedCount}종 · "
                    + $"동료 {Core.CollectionService.CompanionCollectedCount}종 수집\n");
                sb.Append($"도감 보너스: 공격 +{Core.CollectionService.BonusAtkPct:0.#}% · "
                    + $"체력 +{Core.CollectionService.BonusHpPct:0.#}% · "
                    + $"골드 +{Core.CollectionService.BonusGoldPct:0.#}%\n");
                var weap = WeaponSummonAdapter.Instance;
                if (weap != null)
                    sb.Append($"무기 보유 효과: 공격 +{weap.HoldAtkPct:0.#}% (보유 무기 전체 — 장착과 무관하게 적용)\n\n");
                void Section(string title, Core.CollectionService.Entry[] entries)
                {
                    sb.Append($"[{title}]\n");
                    foreach (var e in entries)
                        sb.Append(Core.CollectionService.IsCollected(e.Id) ? $"● {e.Name}   " : $"○ ???   ");
                    sb.Append("\n");
                }
                Section("몬스터", Core.CollectionService.GetMonsterEntries());
                Section("무기", Core.CollectionService.GetWeaponEntries());
                Section("동료", Core.CollectionService.GetCompanionEntries());
                _collectionText.text = sb.ToString();
            };

            // 마을(강호 거점) — 콘텐츠 허브 (U2): 전경 아트 + 시설 진입
            _townModal = _modals.Create("Town", "강호 거점", ModalSize.Large, footer: false);
            var townC = _townModal.Content;
            var townBg = UiKit.Img(townC, "TownBg", Color.white);
            var townSp = Resources.Load<Sprite>("TplArt/Biomes/TownBg");
            if (townSp != null) { townBg.sprite = townSp; townBg.preserveAspect = true; }
            else townBg.color = new Color(0.09f, 0.13f, 0.22f, 1f);
            UiKit.Fix(townBg, -1f, 440f);
            MapleLightTheme.Section(townC, "거점 시설");
            (string label, string sub, System.Action act)[] townSpots =
            {
                ("객잔 — 상점·패키지", "물자와 보급을 사들인다", () => _modals.Open(_shopModal)),
                ("대장간 — 장비 강화", "주문서·스타포스·잠재·에디셔널", () => OpenGrowth(1)),
                ("수련장 — 수련·어빌리티", "8대 무공 트랙과 어빌리티", () => { if (_trainingModal != null) _modals.Open(_trainingModal); }),
                ("표국 — 우편", "보상과 소식이 도착한다", () => { if (_mailModal != null) _modals.Open(_mailModal); }),
                ("서고 — 도감", "수집과 보유 효과 열람", () => { if (_collectionModal != null) _modals.Open(_collectionModal); }),
                ("게시판 — 이벤트·패스", "행사와 계약", () => OpenFeature(ContentId.Event)),
            };
            foreach (var spot in townSpots)
            {
                var s = spot;
                FantasyKitSlots.EnhanceRow(townC, "T_" + s.label, s.label + "  ·  " + s.sub,
                    GrowArt.IconGuild, "입장", () => s.act());
            }

            // offline / job / server built in MapleExtraScreens
        }

        void OnTrainUpgrade(int i)
        {
            var t = (Core.TrainingService.Track)i;
            if (Core.TrainingService.IsTrackCapped(t))
            { Toast("단계 상한 — 나머지 트랙을 채우면 다음 단계가 열립니다"); return; }
            if (!Core.TrainingService.TryUpgrade(t))
            { Toast($"수련 증표 {Core.TrainingService.UpgradeCost(t)} 필요 — 수련장 던전에서 획득"); return; }
            AudioService.Gold();
            RefreshAll();
        }

        void OnTrainReroll()
        {
            if (!Core.TrainingService.TryReroll())
            { Toast($"명성 훈장 {Core.TrainingService.RerollCost} 필요 — 수련장 던전에서 획득"); return; }
            AudioService.Gem();
            RefreshAll();
        }

        void RefreshTrainingModal()
        {
            var cw = IdleMvp.Economy.CurrencyWallet.Instance;
            double tok = cw != null ? cw.Get(IdleMvp.Economy.CurrencyId.TrainingToken) : 0;
            double med = cw != null ? cw.Get(IdleMvp.Economy.CurrencyId.HonorMedal) : 0;
            int cap = Core.TrainingService.LevelCap;
            if (_trainHeaderText != null)
                _trainHeaderText.text =
                    $"단계 {Core.TrainingService.Step + 1} — 8개 트랙 전부 {cap}레벨을 채우면 다음 단계가 열립니다.\n"
                    + $"수련 증표 {tok:0} · 명성 훈장 {med:0}  (수련장 던전 · 일 2회)";
            for (int i = 0; i < _trainRows.Length; i++)
            {
                var t = (Core.TrainingService.Track)i;
                var row = _trainRows[i];
                if (row == null) continue;
                int lv = Core.TrainingService.LevelOf(t);
                bool capped = Core.TrainingService.IsTrackCapped(t);
                int cost = Core.TrainingService.UpgradeCost(t);
                if (row.Level != null) row.Level.text = $"{lv}/{cap}";
                if (row.Bonus != null) row.Bonus.text = $"+{Core.TrainingService.TotalPct(t):0.#}%";
                var bl = row.Action != null ? row.Action.GetComponentInChildren<TMP_Text>() : null;
                if (bl != null) bl.text = capped ? "상한" : $"증표 {cost}";
                UiKit.SetEnabled(row.Action, !capped && tok >= cost);
            }
            if (_trainAbilityText != null)
            {
                var sb = new System.Text.StringBuilder();
                for (int s = 0; s < 3; s++)
                {
                    if (!Core.TrainingService.HasAbility(s)) { sb.Append($"슬롯 {s + 1}: (비어 있음)\n"); continue; }
                    var tr = Core.TrainingService.AbilityTrack(s);
                    sb.Append($"슬롯 {s + 1}: [{Core.TrainingService.TierNames[Core.TrainingService.AbilityTier(s)]}] "
                        + $"{Core.TrainingService.TrackNames[(int)tr]} +{Core.TrainingService.AbilityValue(s):0.#}%\n");
                }
                sb.Append($"변경 비용: 명성 훈장 {Core.TrainingService.RerollCost} (누적 {Core.TrainingService.RerollCount}회 — 많이 바꿀수록 상위 등급 확률↑)");
                _trainAbilityText.text = sb.ToString();
            }
        }

        void BuildShopModal()
        {
            _shopModal = _modals.Create("Shop", "상점", ModalSize.Large);
            var c = _shopModal.Content;

            // ref 07: 좌측 세로 카테고리 사이드바 + 우측 콘텐츠
            var body = UiKit.HStack(c, "ShopBody", UiKit.Space2, 0, 0, 0, 0, TextAnchor.UpperLeft);
            UiKit.Fix(body, -1f, 620f);

            var side = UiKit.VStack(body, "ShopSide", 6f, 0, 0, 0, 0, TextAnchor.UpperLeft);
            UiKit.Fix(side, 210f, -1f);
            var sideBg = side.gameObject.AddComponent<Image>();
            sideBg.sprite = CasualArt.CardRound != null
                ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(12);
            sideBg.type = Image.Type.Sliced;
            sideBg.color = new Color(0.075f, 0.13f, 0.30f, 0.95f);
            sideBg.raycastTarget = false;

            var right = UiKit.VStack(body, "ShopRight", UiKit.Space2, 0, 0, 0, 0, TextAnchor.UpperLeft);
            UiKit.Flex(right);

            var curRow = UiKit.HStack(right, "CurRow", 12f, 0, 0, 4, 4);
            UiKit.Fix(curRow, -1f, 48f);
            _shopRdChip = MapleUiTheme.CurrencyChip(curRow, "RD", GrowArt.IconGem, UiKit.GemColor);
            UiKit.Flex(_shopRdChip.Go.transform);
            _shopBlueChip = MapleUiTheme.CurrencyChip(curRow, "Blue", GrowArt.IconGem, FantasyKitSlots.KitTeal);
            UiKit.Flex(_shopBlueChip.Go.transform);

            string[] shopCats = { "소환", "패키지", "재화", "멤버십", "패스", "레벨팩", "시즌패스" };
            _shopCatBgs = new Image[shopCats.Length];
            for (int t = 0; t < shopCats.Length; t++)
            {
                int idx = t;
                var tabBg = UiKit.Img(side, "Cat" + t, new Color(0.10f, 0.17f, 0.36f, 0.95f));
                tabBg.sprite = CasualArt.CardRound != null
                    ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(10);
                tabBg.type = Image.Type.Sliced;
                UiKit.Fix(tabBg, -1f, 60f);
                var tabBtn = tabBg.gameObject.AddComponent<Button>();
                tabBtn.targetGraphic = tabBg;
                UiKit.Press(tabBtn);
                var tl = UiKit.TmpLabel(tabBg.transform, "L", shopCats[t], UiKit.TmpBody, UiKit.TextInverse,
                    bold: true, TextAlignmentOptions.Center);
                tl.enableWordWrapping = false;
                UiKit.Fill(tl.rectTransform, 4f);
                tabBtn.onClick.AddListener(() => ShowShopPanel(idx));
                _shopCatBgs[t] = tabBg;
            }

            _shopPanels = new GameObject[7];
            for (int p = 0; p < 7; p++)
            {
                var panel = UiKit.VStack(right, "ShopPanel" + p, UiKit.Space2, 0, 0, 0, 0);
                var fle = panel.gameObject.AddComponent<LayoutElement>();
                fle.flexibleWidth = 1f;
                fle.minHeight = 280f;
                _shopPanels[p] = panel.gameObject;
            }

            // Summon
            var sum = _shopPanels[0].transform;
            FantasyKitSlots.PackageRow(sum, "Wep", "무기 티켓팩", "무기 소환권 +5 (RD 30)",
                GrowArt.IconSummonWeapon, "구매", () => Toast(ShopAdapter.Instance?.BuyTicketPack(true)));
            FantasyKitSlots.PackageRow(sum, "Comp", "동료 티켓팩", "동료 소환권 +3 (RD 30)",
                GrowArt.IconAscend(3), "구매", () => Toast(ShopAdapter.Instance?.BuyTicketPack(false)));
            FantasyKitSlots.PackageRow(sum, "Skill", "스킬 즉시 습득", "미습득 스킬 1개 즉시 습득 (RD 25)",
                GrowArt.IconSummonSkill, "구매", () => Toast(ShopAdapter.Instance?.BuySkillSummonTicket()));

            // Package
            var pkg = _shopPanels[1].transform;
            FantasyKitSlots.PackageRow(pkg, "Start", "스타터팩", "성장 가속 + 티켓 보너스",
                GrowArt.IconCheck, "구매", () => Toast(ShopAdapter.Instance?.BuyStarterPack()));
            FantasyKitSlots.PackageRow(pkg, "Cube", "큐브 패키지", "잠재 재설정 재료",
                GrowArt.IconEnhance("Accuracy"), "구매", () => Toast(ShopAdapter.Instance?.BuyCubes()));
            FantasyKitSlots.PackageRow(pkg, "Month", "월간 패키지", "매일 재화 · 오프라인 연장",
                GrowArt.IconAdsFree, "구매", () => Toast(ShopAdapter.Instance?.BuyMonthlyPack()));

            // Currency
            var cur = _shopPanels[2].transform;
            FantasyKitSlots.PackageRow(cur, "Blue", "블루다이아 300", "프리미엄 재화",
                GrowArt.IconGem, "구매", () => Toast(ShopAdapter.Instance?.BuyBlueDiamondPack(1)));
            FantasyKitSlots.PackageRow(cur, "Ads", "무료 다이아", "광고 시청 보상",
                GrowArt.IconAds, "광고", () => Toast(ShopAdapter.Instance?.BuyBlueDiamondPack(0)));
            FantasyKitSlots.PackageRow(cur, "Gold", "골드 상자", "대량 골드 즉시 지급",
                GrowArt.IconGold, "구매", () => Toast(ShopAdapter.Instance?.BuyGoldChest()));

            // Membership
            var mem = _shopPanels[3].transform;
            FantasyKitSlots.PackageRow(mem, "Vip", "멤버십 VIP", "30일 · 방치↑ · 광고제거",
                GrowArt.IconCheck, "구매", () =>
                {
                    IapBridge.Instance?.Purchase(IapProductCatalog.MembershipMonth, () =>
                    {
                        Toast(ShopAdapter.Instance?.BuyProduct("membership"));
                        RefreshAll();
                    }, err => Toast(err ?? "구매 실패"));
                });

            // Pass
            var pass = _shopPanels[4].transform;
            FantasyKitSlots.PackageRow(pass, "Pass", "무기 패스", "소환 보너스 트랙 · 누적 보상",
                GrowArt.IconMenuStore, "구매", () =>
                {
                    IapBridge.Instance?.Purchase(IapProductCatalog.WeaponPass, () =>
                    {
                        Toast(PassService.Instance?.BuyWeaponPass());
                        RefreshAll();
                    }, err => Toast(err ?? "구매 실패"));
                });
            FantasyKitSlots.PackageRow(pass, "PassC", "동료 패스", "동료 소환 트랙",
                GrowArt.IconAscend(3), "구매", () =>
                {
                    IapBridge.Instance?.Purchase(IapProductCatalog.CompanionPass, () =>
                    {
                        Toast(PassService.Instance?.BuyCompanionPass());
                        RefreshAll();
                    }, err => Toast(err ?? "구매 실패"));
                });
            _shopPassRow = FantasyKitSlots.InfoRow(pass, "PassStat", "패스 진행", GrowArt.IconAllow);
            FantasyKitSlots.EnhanceRow(pass, "ClaimW", "무기 무료 보상", GrowArt.IconCheck, "수령",
                () => Toast(PassService.Instance?.ClaimWeaponFree()));
            FantasyKitSlots.EnhanceRow(pass, "ClaimC", "동료 무료 보상", GrowArt.IconCheck, "수령",
                () => Toast(PassService.Instance?.ClaimCompanionFree()));
            FantasyKitSlots.EnhanceRow(pass, "MonthDaily", "월간 일일", GrowArt.IconAdsFree, "수령",
                () => Toast(ShopAdapter.Instance?.ClaimMonthlyDaily()));

            // Level Package
            var lvlPkg = _shopPanels[5].transform;
            var lps = LevelPackageService.Instance;
            if (lps != null)
            {
                var allPkg = lps.AllPackages;
                for (int li = 0; li < allPkg.Length; li++)
                {
                    int idx2 = li;
                    var lp = allPkg[li];
                    string cost = lp.CostBlue > 0 ? $"블루 {lp.CostBlue}" : "무료";
                    string sub = $"Lv{lp.Level} 달성 · RD {lp.RedDiamond} + 무기권 {lp.WeaponTicket}";
                    FantasyKitSlots.PackageRow(lvlPkg, "Lv" + lp.Level, $"Lv{lp.Level} 패키지", sub,
                        GrowArt.IconCheck, cost, () =>
                        {
                            string r = LevelPackageService.Instance?.TryClaim(idx2);
                            if (r != null) Toast(r); else { Toast($"Lv{allPkg[idx2].Level} 패키지 수령!"); RefreshAll(); }
                        });
                }
            }

            // Season Pass
            var sp = _shopPanels[6].transform;
            _shopSeasonInfo = FantasyKitSlots.InfoRow(sp, "SPInfo", "시즌 패스", GrowArt.IconMenuStore);
            FantasyKitSlots.PackageRow(sp, "SPBuy", "프리미엄 패스", "전 구간 프리미엄 보상 해금",
                GrowArt.IconGem, "블루 200", () =>
                {
                    string r = SeasonPassService.Instance?.BuyPremium();
                    if (r != null) Toast(r); else { Toast("프리미엄 패스 활성!"); RefreshAll(); }
                });
            FantasyKitSlots.EnhanceRow(sp, "SPClaim", "다음 보상 수령", GrowArt.IconCheck, "수령",
                () =>
                {
                    string r = SeasonPassService.Instance?.ClaimNextTier();
                    if (r != null) Toast(r); else { Toast("시즌 보상 수령!"); RefreshAll(); }
                });

            var exch = MapleUiTheme.SecondaryButton(_shopModal.Footer, "Exch", "블루 50 → RD 30", () => Toast(ShopAdapter.Instance?.ExchangeBlueToRed(50, 30)));
            UiKit.Fix(exch, 260f, 72f);

            ShowShopPanel(0);
            _shopModal.Refresh = () =>
            {
                if (_shopRdChip != null)
                    _shopRdChip.Value.text = UiKit.Num(WalletAdapter.Instance?.RedDiamond ?? 0);
                if (_shopBlueChip != null)
                    _shopBlueChip.Value.text = UiKit.Num(CurrencyWallet.Instance?.Get(CurrencyId.BlueDiamond) ?? 0);
                if (_shopPassRow?.Value != null && PassService.Instance != null)
                    _shopPassRow.Value.text = PassService.Instance.StatusText();
                if (_shopSeasonInfo?.Value != null)
                {
                    var sps = SeasonPassService.Instance;
                    _shopSeasonInfo.Value.text = sps != null
                        ? $"XP {sps.Xp} · Tier {sps.CurrentTierIndex + 1}/{sps.AllTiers.Length} · {sps.DaysRemaining}일 남음{(sps.IsPremium ? " · 프리미엄" : "")}"
                        : "—";
                }
            };
        }

        void ShowShopPanel(int idx)
        {
            if (_shopPanels == null) return;
            for (int i = 0; i < _shopPanels.Length; i++)
                if (_shopPanels[i] != null) _shopPanels[i].SetActive(i == idx);
            if (_shopCatBgs != null)
            {
                for (int i = 0; i < _shopCatBgs.Length; i++)
                {
                    if (_shopCatBgs[i] == null) continue;
                    _shopCatBgs[i].color = i == idx
                        ? new Color(0.26f, 0.62f, 0.98f, 1f)          // 선택: 밝은 블루
                        : new Color(0.10f, 0.17f, 0.36f, 0.95f);      // 비선택: 짙은 남색
                }
            }
        }

        // =====================================================================
        // Meta screens
        // =====================================================================

        void BuildMetaScreens()
        {
            // costume
            _costumeModal = _modals.Create("Costume", "코스튬", ModalSize.Medium);
            var cc = _costumeModal.Content;
            var cosWallet = MapleUiTheme.ToolBar(cc, "CosWallet");
            var blueChip = MapleUiTheme.CurrencyChip(cosWallet, "Blue", GrowArt.IconGem, FantasyKitSlots.KitTeal);
            UiKit.Fix(blueChip.Go.transform, -1f, 36f);
            MapleUiTheme.TabBar(cc, "CosTabs", new[] { "코스튬", "뷰티" },
                idx =>
                {
                    if (idx == 0) Toast("코스튬 카테고리");
                    else ShowStub("뷰티", FeatureGate.ComingSoonBody(ContentId.CostumeBeauty));
                });
            var cosPref = new Vector2(180f, 210f);
            var cosGrid = UiKit.FillGrid(cc, "CosGrid", cosPref, new Vector2(10f, 10f), 3, 4);
            var cosCell = cosGrid.GetComponent<GridLayoutGroup>().cellSize;
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                FantasyKitSlots.SkillTile(cosGrid, "Cos" + i, "외형 " + (i + 1), "블루 60",
                    GrowArt.CircleFrame, GrowArt.Rarity(i % 6), () =>
                    {
                        Toast(CostumeAdapter.Instance?.Buy(idx));
                        if (blueChip.Value != null)
                            blueChip.Value.text = UiKit.Num(CurrencyWallet.Instance?.Get(CurrencyId.BlueDiamond) ?? 0);
                    }, cosCell);
            }
            _costumeModal.Refresh = () =>
            {
                if (blueChip.Value != null)
                    blueChip.Value.text = UiKit.Num(CurrencyWallet.Instance?.Get(CurrencyId.BlueDiamond) ?? 0);
            };
            var apply = MapleUiTheme.PrimaryButton(_costumeModal.Footer, "Apply", "적용", () =>
            {
                var c = CostumeAdapter.Instance;
                Toast(c != null && c.Equipped >= 0 ? $"코스튬 {c.Equipped + 1} 적용 중" : "장착된 코스튬 없음");
            });
            UiKit.Fix(apply, 180f, 56f);

            // job / server built in MapleExtraScreens after this method

            // menu sheet
            _menuModal = _modals.Create("Menu", "메뉴", ModalSize.Medium, footer: false);
            var mc = _menuModal.Content;
            (string label, Sprite icon, System.Action act)[] items =
            {
                ("마을 (강호 거점)", GrowArt.IconGuild, () => { if (_townModal != null) _modals.Open(_townModal); }),
                ("코스튬 · 뷰티", GrowArt.CircleFrame, () => OpenFeature(ContentId.CostumeBeauty)),
                ("전직", GrowArt.IconAscend(3), () => { if (_jobModal != null) _modals.Open(_jobModal); }),
                ("문파", GrowArt.IconGuild, () => { if (!Casual.CasualScreens.Open("sect")) Toast("문파 화면을 열 수 없습니다"); }),
                ("경지", GrowArt.IconAscend(6), () => { if (!Casual.CasualScreens.Open("realm")) Toast("경지 화면을 열 수 없습니다"); }),
                ("수련", GrowArt.IconAscend(2), () => { if (_trainingModal != null) _modals.Open(_trainingModal); }),
                ("도감", GrowArt.IconCheck, () => { if (_collectionModal != null) _modals.Open(_collectionModal); }),
                ("유물", GrowArt.IconGem, () => { if (_artifactModal != null) _modals.Open(_artifactModal); }),
                ("맵 선택", GrowArt.IconBoss, () => OpenFeature(ContentId.MapSelect)),
                ("성장 던전", GrowArt.IconBoss, () => OpenFeature(ContentId.Dungeon)),
                ("이벤트", GrowArt.IconCheck, () => OpenFeature(ContentId.Event)),
                ("핫딜", GrowArt.IconSummonWeapon, () => OpenFeature(ContentId.HotDeal)),
                ("채팅", GrowArt.IconChat, () => OpenFeature(ContentId.Chat)),
                ("환생", GrowArt.IconAscend(5), () => { if (_rebirthModal != null) _modals.Open(_rebirthModal); }),
                ("빠른 사냥", GrowArt.IconBoss, () => _modals.Open(_fastHuntModal)),
                ("오프라인 보상", GrowArt.IconGold, () => {
                    if (Casual.CasualScreens.Open("offline")) return;
                    if (_offlineModal != null) _modals.Open(_offlineModal); }),
                ("아레나", GrowArt.IconAscend(4), () => OpenFeature(ContentId.Arena)),
                ("레이드", GrowArt.IconBoss, () => OpenFeature(ContentId.Raid)),
                ("패스", GrowArt.IconAllow, () => OpenFeature(ContentId.Pass)),
                ("BGM", GrowArt.IconBgm, () => Toast(GameSettings.ToggleBgm())),
                ("SFX", GrowArt.IconSfx, () => Toast(GameSettings.ToggleSfx())),
                ("설정", GrowArt.IconSetting, () =>
                {
                    GameSettings.ApplyAudio();
                    Toast(GameSettings.OpenSettingsSummary());
                }),
            };
            foreach (var it in items)
            {
                var b = FantasyKitSlots.EnhanceRow(mc, "M_" + it.label, it.label, it.icon, "열기", it.act);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var dbg = MapleUiTheme.SecondaryButton(mc, "Debug", "디버그 재화 지급", () => Toast(ShopAdapter.Instance?.GrantDebugStarter()));
            UiKit.Fix(dbg, -1f, 56f);
            var snap = MapleUiTheme.SecondaryButton(mc, "Snap", "세이브 스냅샷 복사", () =>
                Toast(SaveSnapshotService.Instance != null
                    ? SaveSnapshotService.Instance.ExportToClipboard()
                    : "스냅샷 서비스 없음"));
            UiKit.Fix(snap, -1f, 56f);
            var snapIn = MapleUiTheme.SecondaryButton(mc, "SnapIn", "스냅샷 붙여넣기", () =>
                Toast(SaveSnapshotService.Instance != null
                    ? SaveSnapshotService.Instance.ImportFromClipboard()
                    : "스냅샷 서비스 없음"));
            UiKit.Fix(snapIn, -1f, 56f);
            var iapFlag = MapleUiTheme.SecondaryButton(mc, "IapFlag", "IAP/Ads 실연동 토글", () =>
            {
                BmRuntimeFlags.UseRealIapAds = !BmRuntimeFlags.UseRealIapAds;
                Toast("UseRealIapAds = " + BmRuntimeFlags.UseRealIapAds);
            });
            UiKit.Fix(iapFlag, -1f, 56f);
#endif

            BuildFeatureModals();
            // stub
            _stubModal = _modals.Create("Stub", "준비중", ModalSize.Small, footer: false);
            MapleUiTheme.SectionHeader(_stubModal.Content, "안내");
            FantasyKitSlots.PortraitCard(_stubModal.Content, "StubIcon", "준비중", "콘텐츠 오픈 예정",
                GrowArt.IconLock, GrowArt.Rarity(0), null, 280f, 200f);
            _stubBody = MapleUiTheme.InfoText(_stubModal.Content, "해당 기능은 곧 열립니다.");
            UiKit.Fix(_stubBody.rectTransform, -1f, 80f);
        }

        // =====================================================================
        // Actions / navigation
        // =====================================================================

        void BuyStat(string stat)
        {
            var p = PlayerGrowth.Instance;
            if (p == null) return;
            bool ok = p.TrySpendStatPoint(stat);
            if (!ok) Toast("스탯 포인트가 없습니다");
            else
            {
                // enhance juice: punch the invest card's value + button
                var row = stat == "ATK" ? _atkRow : stat == "DEF" ? _defRow : _hpRow;
                if (row?.Bonus != null) UiFx.Punch(row.Bonus.transform, 0.22f);
                if (row?.Action != null) UiFx.Punch(row.Action.transform, 0.14f);
            }
            RefreshAll();
        }

        void BuySpecial(string kind)
        {
            var p = PlayerGrowth.Instance;
            if (p == null) return;
            if (!p.TrySpendSpecialStat(kind))
                Toast(p.SpecialStatPoints <= 0 ? "특수 스탯 포인트가 없습니다" : "배분 실패");
            else
                Toast("특수 스탯 배분 완료");
            RefreshAll();
        }

        void OnChallenge()
        {
            if (_battle == null) return;
            if (_battle.Mode == CombatMode.Breakthrough)
                _battle.AbortBreakthrough();
            else
                _battle.TryStartBreakthrough();
            RefreshHud();
        }

        void OnTopIcon(int idx)
        {
            switch (idx)
            {
                case 0: OpenFeature(ContentId.Guild); break;
                case 1: OpenFeature(ContentId.Mail); break;
                // 상점 아이콘이 OpenFeature를 안 거쳐서 구버전 상점이 열리고 있었다
                case 2: OpenFeature(ContentId.Shop); break;
                default:
                    if (Casual.CasualScreens.Open("menu")) break;
                    _modals.Open(_menuModal);
                    break;
            }
        }

        /// <summary>ContentId → 구매 에셋 프리팹 화면 id (없으면 null).</summary>
        static string CasualIdFor(ContentId id)
        {
            switch (id)
            {
                case ContentId.Guild: return "guild";
                case ContentId.Mail: return "mail";
                case ContentId.Arena: return "arena";
                case ContentId.MapSelect: return "map";
                case ContentId.Shop: return "shop";
                case ContentId.Pass: return "pass";
                case ContentId.HotDeal: return "hotdeal";
                case ContentId.Dungeon: return "dungeon";
                default: return null;
            }
        }

        void OpenFeature(ContentId id)
        {
            if (!FeatureGate.IsReady(id))
            {
                ShowStub(FeatureGate.DisplayName(id), FeatureGate.ComingSoonBody(id));
                return;
            }
            // 프리팹 화면 우선
            var casual = CasualIdFor(id);
            if (casual != null && Casual.CasualScreens.Open(casual)) return;

            switch (id)
            {
                case ContentId.Guild: _modals.Open(_guildModal); break;
                case ContentId.Mail: _modals.Open(_mailModal); break;
                case ContentId.Arena: _modals.Open(_arenaModal); break;
                case ContentId.Raid: _modals.Open(_raidModal); break;
                case ContentId.MapSelect: _modals.Open(_mapModal); break;
                case ContentId.CostumeBeauty: _modals.Open(_costumeModal); break;
                case ContentId.Shop:
                    if (_hotDealModal != null && FeatureGate.IsReady(ContentId.HotDeal) &&
                        PlayerPrefs.GetInt("IdleGrow.Maple.HotDealSeen", 0) == 0)
                    {
                        PlayerPrefs.SetInt("IdleGrow.Maple.HotDealSeen", 1);
                        PlayerPrefs.Save();
                        _modals.Open(_hotDealModal);
                    }
                    else
                        _modals.Open(_shopModal);
                    break;
                case ContentId.Pass:
                    _modals.Open(_shopModal);
                    ShowShopPanel(4);
                    break;
                case ContentId.Event:
                    if (_eventModal != null) _modals.Open(_eventModal);
                    break;
                case ContentId.Dungeon:
                    if (_dungeonModal != null) _modals.Open(_dungeonModal);
                    break;
                case ContentId.Chat:
                    _extra?.ToggleChat();
                    break;
                default:
                    ShowStub(FeatureGate.DisplayName(id), FeatureGate.ComingSoonBody(id));
                    break;
            }
        }

        void BuildFeatureModals()
        {
            // Mail — list tiles instead of plain InfoText.
            _mailModal = _modals.Create("Mail", "우편", ModalSize.Medium);
            var mailList = UiKit.VStack(_mailModal.Content, "MailList", 8f, 0, 0, 0, 0);
            _mailInfo = MapleUiTheme.InfoChip(_mailModal.Content, "MailMeta", "미수령 0건", 40f);
            MapleUiTheme.PrimaryButton(_mailModal.Footer, "ClaimAll", "모두 수령", () =>
            {
                Toast(MailService.Instance?.ClaimAll());
                RefreshAll();
            });
            _mailModal.Refresh = () =>
            {
                ClearChildren(mailList);
                var ms = MailService.Instance;
                if (ms == null)
                {
                    if (_mailInfo != null) _mailInfo.text = "우편 서비스 없음";
                    return;
                }
                if (_mailInfo != null) _mailInfo.text = $"미수령 {ms.UnreadCount}건";
                if (ms.Inbox.Count == 0)
                {
                    FantasyKitSlots.InfoRow(mailList, "Empty", "받은 우편이 없습니다", GrowArt.IconMail);
                    return;
                }
                int shown = Mathf.Min(10, ms.Inbox.Count);
                for (int i = 0; i < shown; i++)
                {
                    var m = ms.Inbox[i];
                    string mid = m.id;
                    string title = m.claimed ? $"[수령] {m.title}" : $"[신규] {m.title}";
                    FantasyKitSlots.PackageRow(mailList, "Mail" + i, title, m.claimed ? "수령 완료" : "보상 대기",
                        GrowArt.IconMail, m.claimed ? null : "수령", m.claimed ? null : () =>
                        {
                            Toast(MailService.Instance?.Claim(mid));
                            RefreshAll();
                        }, 110f);
                }
            };

            // Guild — status card + member list.
            _guildModal = _modals.Create("Guild", "길드", ModalSize.Medium);
            var guildBox = MapleUiTheme.SectionBox(_guildModal.Content, "GuildBox");
            _guildInfo = MapleUiTheme.InfoChip(guildBox, "GuildInfo", "", 96f);
            FantasyKitSlots.InfoRow(guildBox, "Members", "멤버", GrowArt.IconChat, 52f);
            // 길드 스킬 (개인 연구) — 골드 소모, 길드 레벨이 상한
            MapleLightTheme.Section(_guildModal.Content, "길드 스킬 연구");
            var gsGrid = UiKit.FillGrid(_guildModal.Content, "GsGrid", new Vector2(380f, 84f), new Vector2(10f, 10f), 2, 2);
            var gsRows = new StatRowView[GuildAdapter.GuildSkillCount];
            for (int gi = 0; gi < GuildAdapter.GuildSkillCount; gi++)
            {
                int gidx = gi;
                gsRows[gi] = MapleLightTheme.SpecialRow(gsGrid, "Gs" + gi,
                    GuildAdapter.GuildSkillNames[gi], "연구", () =>
                    {
                        Toast(GuildAdapter.Instance?.BuySkill(gidx));
                        RefreshAll();
                    }, false, null);
            }
            MapleUiTheme.AccentButton(_guildModal.Footer, "Join", "가입/생성", () =>
            {
                Toast(GuildAdapter.Instance?.CreateOrJoin("초보 길드"));
                RefreshAll();
            });
            MapleUiTheme.SecondaryButton(_guildModal.Footer, "Donate", "기부", () => Toast(GuildAdapter.Instance?.Donate()));
            MapleUiTheme.PrimaryButton(_guildModal.Footer, "Quest", "일일 퀘스트", () => Toast(GuildAdapter.Instance?.CompleteDailyQuest()));
            _guildModal.Refresh = () =>
            {
                var g = GuildAdapter.Instance;
                if (g == null) return;
                string members = string.Join(", ", g.Members);
                if (_guildInfo != null)
                    _guildInfo.text = g.StatusText() + "\n멤버: " + members
                        + (g.Joined ? $"\n연구 효과: 공격 +{g.GuildAtkPct:0.#}% · 체력 +{g.GuildHpPct:0.#}% · 골드 +{g.GuildGoldPct:0.#}%" : "");
                double gold = WalletAdapter.Instance != null ? WalletAdapter.Instance.Gold : 0;
                for (int gi = 0; gi < gsRows.Length; gi++)
                {
                    var row = gsRows[gi];
                    if (row == null) continue;
                    int lv = g.SkillLv[gi];
                    bool capped = lv >= g.SkillCap;
                    double cost = g.SkillCost(gi);
                    if (row.Level != null) row.Level.text = $"{lv}/{g.SkillCap}";
                    if (row.Bonus != null) row.Bonus.text = gi == 0 ? $"공격 +{g.GuildAtkPct:0.#}%"
                        : gi == 1 ? $"체력 +{g.GuildHpPct:0.#}%" : $"골드 +{g.GuildGoldPct:0.#}%";
                    var bl = row.Action != null ? row.Action.GetComponentInChildren<TMP_Text>() : null;
                    if (bl != null) bl.text = capped ? "상한" : $"골드 {UiKit.Num(cost)}";
                    UiKit.SetEnabled(row.Action, g.Joined && !capped && gold >= cost);
                }
            };

            // Arena — bot CP matches + daily challenges (FeatureGate Ready).
            _arenaModal = _modals.Create("Arena", "아레나", ModalSize.Medium);
            var arenaBox = MapleUiTheme.SectionBox(_arenaModal.Content, "ArenaBox");
            _arenaInfo = MapleUiTheme.InfoChip(arenaBox, "ArenaInfo", "", 128f);
            MapleUiTheme.SectionHeader(arenaBox, "오늘의 상대");
            var arenaRow = UiKit.HStack(arenaBox, "EnhanceRow", UiKit.Space2, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(arenaRow, -1f, 56f);
            for (int i = 0; i < 3; i++)
            {
                int oi = i;
                var btn = MapleUiTheme.AccentButton(arenaRow, "Opp" + i, "도전 " + (i + 1),
                    () =>
                    {
                        Toast(ArenaAdapter.Instance?.Challenge(oi));
                        RefreshAll();
                        _arenaModal?.Refresh?.Invoke();
                    });
                UiKit.Fix(btn, -1f, 52f);
            }
            _arenaModal.Refresh = () =>
            {
                var a = ArenaAdapter.Instance;
                if (a == null) return;
                var ops = a.ListOpponents();
                if (_arenaInfo != null)
                    _arenaInfo.text = a.StatusText() + "\n" + string.Join("\n", ops);
            };

            // Raid — daily local HP pool (FeatureGate Ready).
            _raidModal = _modals.Create("Raid", "월드보스", ModalSize.Medium);
            var raidBox = MapleUiTheme.SectionBox(_raidModal.Content, "RaidBox");
            _raidInfo = MapleUiTheme.InfoChip(raidBox, "RaidInfo", "", 96f);
            var raidRow = UiKit.HStack(raidBox, "EnhanceRow", UiKit.Space2, 0, 0, 0, 0, TextAnchor.MiddleLeft, true);
            UiKit.Fix(raidRow, -1f, 56f);
            var strike = MapleUiTheme.AccentButton(raidRow, "Strike", "타격", () =>
            {
                Toast(RaidService.Instance?.Strike());
                RefreshAll();
                _raidModal?.Refresh?.Invoke();
            });
            UiKit.Fix(strike, -1f, 52f);
            var enter = MapleUiTheme.PrimaryButton(raidRow, "Enter", "필드 진입", () =>
            {
                if (_battle != null && _battle.TryStartWorldBoss())
                {
                    _modals.Close();
                    Toast("월드보스 전투 시작");
                }
                else Toast(_battle != null ? _battle.LastMessage : "전투 없음");
            });
            UiKit.Fix(enter, -1f, 52f);
            _raidModal.Refresh = () =>
            {
                if (_raidInfo != null)
                    _raidInfo.text = RaidService.Instance != null ? RaidService.Instance.StatusText() : "";
            };

            // Map select — full-width rows (never squeeze EnhanceRow into tiny grid cells).
            _mapModal = _modals.Create("Map", "맵 선택", ModalSize.Medium, footer: false);
            _mapInfo = MapleUiTheme.InfoChip(_mapModal.Content, "MapInfo", "", 56f);
            var mapList = UiKit.VStack(_mapModal.Content, "Maps", 8f, 0, 0, 0, 0);
            UiKit.Fix(mapList, -1f, 520f);
            _mapModal.Refresh = () =>
            {
                var sp = StageProgress.Instance;
                if (sp == null) return;
                ClearChildren(mapList);
                int max = Mathf.Min(sp.MaxWaveReached, StageTable.Count);
                int start = Mathf.Max(1, max - 8);
                if (_mapInfo != null)
                    _mapInfo.text = $"돌파 {sp.GetDisplayLabel()} · {sp.GetHuntLabel()} · CP {UiKit.Num(CombatPowerService.GetTotalCp())}";
                for (int idx = start; idx <= max; idx++)
                {
                    int stageIdx = idx;
                    var row = StageTable.Get(stageIdx);
                    string title = row != null
                        ? $"Ch.{row.chapter}-{row.stage}  ·  {StageTable.TierLabel(row.mapTier)}"
                        : $"Stage {stageIdx}";
                    string sub = row != null ? $"권장 CP {row.recommendedCp:0}" : "";
                    var card = FantasyKitSlots.EnhanceRow(mapList, "M" + stageIdx, title, GrowArt.IconBoss, "사냥", () =>
                    {
                        if (StageProgress.Instance != null && StageProgress.Instance.TrySetHuntStage(stageIdx))
                        {
                            _battle?.ResetEnemyForCurrentStage();
                            Toast("사냥 맵: " + title);
                        }
                        else Toast("전투력 부족 또는 미해금 맵");
                    }, 88f);
                    if (card.Value != null) card.Value.text = sub;
                }
            };
        }

        static readonly string[] NavCasualIds = { "char", "equip", "skill", "weapon", "comp" };

        void OpenGrowth(int idx)
        {
            // 구매 에셋 프리팹 화면이 있으면 그쪽을 쓴다.
            // (예전엔 DebugOpen만 후킹해서 실제 네비로 열면 손그림 버전이 나왔다)
            if (idx >= 0 && idx < NavCasualIds.Length &&
                Casual.CasualScreens.Open(NavCasualIds[idx]))
            {
                SetNavSelected(idx);
                return;
            }
            switch (idx)
            {
                case 0: _modals.Open(_charModal); break;
                case 1: _modals.Open(_equipModal); break;
                case 2: _modals.Open(_skillModal); break;
                case 3: _modals.Open(_weaponModal); break;
                case 4: _modals.Open(_compModal); break;
            }
            SetNavSelected(idx);
        }

        void SetNavSelected(int idx)
        {
            if (_navBgs == null) return;
            for (int i = 0; i < _navBgs.Length; i++)
            {
                bool on = i == idx;
                // 키트 탭 스프라이트가 흰색 원본이라 white로 두면 선택 탭이 흰 덩어리가 된다.
                // 선택은 밝은 블루로 표시한다.
                _navBgs[i].color = on
                    ? new Color(0.20f, 0.45f, 0.85f, 0.95f)
                    : new Color(0.08f, 0.13f, 0.26f, 0.55f);
                var under = _navBgs[i].transform.Find("Under")?.GetComponent<Image>();
                if (under != null) under.enabled = on;
                var label = _navBgs[i].transform.Find("L")?.GetComponent<TMP_Text>();
                if (label != null) label.color = on ? UiKit.Accent : UiKit.TextInverseDim;
            }
        }

        void ShowJobOnce()
        {
            if (PlayerPrefs.GetInt("IdleGrow.Maple.JobDone", 0) == 1)
            {
                // One-time appearance customization right after the job pick.
                if (PlayerPrefs.GetInt("IdleGrow.Maple.AppearanceSeen", 0) == 0 && _appearance != null)
                {
                    PlayerPrefs.SetInt("IdleGrow.Maple.AppearanceSeen", 1);
                    PlayerPrefs.Save();
                    _modals.Open(_appearance.Modal);
                    return;
                }
                TryShowOffline();
                return;
            }
            _modals.Open(_jobModal);
            PlayerPrefs.SetInt("IdleGrow.Maple.JobDone", 1);
        }

        void TryShowOffline()
        {
            var lb = LootBoxService.Instance;
            if (lb == null || (lb.PendingGold <= 0 && lb.PendingXp <= 0)) return;
            // 시작 시 자동으로 뜨는 창 — 프리팹 버전을 쓴다
            if (Casual.CasualScreens.Open("offline")) return;
            _modals.Open(_offlineModal);
        }

        /// <summary>Debug hook: open a modal by id (editor visual iteration).</summary>
        public void DebugOpen(string id)
        {
            // 구매 에셋 프리팹으로 만든 화면이 있으면 그쪽을 먼저 쓴다 (손으로 그린 버전 대체)
            if (id == "close") { Casual.CasualScreens.CloseAll(); }
            else if (Casual.CasualScreens.Open(id)) { return; }

            switch (id)
            {
                case "char": _modals.Open(_charModal); SetNavSelected(0); break;
                case "equip": _modals.Open(_equipModal); SetNavSelected(1); break;
                case "skill": _modals.Open(_skillModal); SetNavSelected(2); break;
                case "weapon": _modals.Open(_weaponModal); SetNavSelected(3); break;
                case "comp": _modals.Open(_compModal); SetNavSelected(4); break;
                case "shop": _modals.Open(_shopModal); break;
                case "dungeon": _modals.Open(_dungeonModal); break;
                case "event": _modals.Open(_eventModal); break;
                case "offline": _modals.Open(_offlineModal); break;
                case "job": _modals.Open(_jobModal); break;
                case "server": _modals.Open(_serverModal); break;
                case "mail": _modals.Open(_mailModal); break;
                case "guild": _modals.Open(_guildModal); break;
                case "artifact": _modals.Open(_artifactModal); break;
                case "hotdeal": _modals.Open(_hotDealModal); break;
                case "appearance": _modals.Open(_appearance.Modal); break;
                case "arena": _modals.Open(_arenaModal); break;
                case "raid": _modals.Open(_raidModal); break;
                case "map": _modals.Open(_mapModal); break;
                case "menu": _modals.Open(_menuModal); break;
                case "faction": _modals.Open(_factionModal); break;
                case "fated": _modals.Open(_fatedModal); break;
                case "rebirth": _modals.Open(_rebirthModal); break;
                case "close": _modals.Close(); break;
            }
        }

        void ShowStub(string title, string body)
        {
            _stubModal.Title.text = title;
            _stubBody.text = body;
            _modals.Open(_stubModal);
        }

        void Toast(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            UiToast.Show(msg);
            RefreshAll();
        }

        // =====================================================================
        // Refresh
        // =====================================================================

        void RefreshHud()
        {
            if (_battle == null) return;
            if (_statusText != null) _statusText.text = _battle.StatusText;
            if (_stageLabel != null)
            {
                var sp = StageProgress.Instance;
                if (sp != null)
                    _stageLabel.text = $"{sp.GetHuntLabel()} · 돌파 {sp.GetDisplayLabel()}";
            }
            if (_eliteLvLabel != null)
                _eliteLvLabel.text = $"소환 Lv.{EliteSummonService.Instance?.SummonLevel ?? 1} ↑";
            if (_guildDot != null)
                _guildDot.gameObject.SetActive(GuildAdapter.Instance != null && GuildAdapter.Instance.HasDailyReward);
            if (_mailDot != null)
                _mailDot.gameObject.SetActive(MailService.Instance != null && MailService.Instance.UnreadCount > 0);
            bool pushing = _battle.Mode == CombatMode.Breakthrough || _battle.Mode == CombatMode.WorldBoss;
            if (_challengeLabel != null)
            {
                if (pushing)
                {
                    _challengeLabel.text = _battle.AutoPushActive
                        ? $"포기 · 실패 {_battle.PushFailCount}/{FieldAutoHuntController.MaxPushFails}"
                        : "포기";
                    UiKit.SetEnabled(_challengeBtn, true);
                }
                else
                {
                    bool can = _battle.CanStartBreakthrough(out _);
                    _challengeLabel.text = "도전";
                    UiKit.SetEnabled(_challengeBtn, can);
                }
            }

            float maxHp = Mathf.Max(1f, _battle.HeroMaxHp);
            float curHp = Mathf.Clamp(_battle.HeroHp, 0f, maxHp);
            _hpBar?.Set(curHp / maxHp, $"HP {curHp:0}/{maxHp:0}");
        }

        /// <summary>
        /// 전체 UI 갱신 요청. 실제 작업은 LateUpdate에서 프레임당 1회만 수행한다.
        ///
        /// 10종 서비스의 OnChanged에 물려 있어서 몹 하나 잡을 때마다(골드/경험치/재화 드랍 등)
        /// 십수 번씩 불린다. 예전엔 그때마다 즉시 전체 재빌드를 했는데, 무기창이 열려 있으면
        /// 카드 36장(≈790 GameObject)을 통째로 파괴·재생성했다. Destroy는 프레임 끝에
        /// 처리되므로 같은 프레임의 재빌드가 겹치면 카드가 수백~수천 장까지 쌓여
        /// 10연차가 10초씩 멈추고 스크롤이 끊겼다. 한 프레임에 한 번으로 합친다.
        /// </summary>
        void RefreshAll() => _refreshDirty = true;

        bool _refreshDirty;

        /// <summary>
        /// 자식 전부 제거. Destroy는 프레임 끝에 처리되므로 childCount에 아직 남아 있어,
        /// 같은 프레임에 재빌드가 겹치면 카드가 계속 누적된다. 먼저 부모에서 떼어낸다.
        /// </summary>
        static void ClearChildren(Transform t)
        {
            if (t == null) return;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var ch = t.GetChild(i);
                ch.SetParent(null, false);
                Destroy(ch.gameObject);
            }
        }

        void LateUpdate()
        {
            if (!_refreshDirty) return;
            _refreshDirty = false;
            RefreshNow();
        }

        void RefreshNow()
        {
            RefreshHud();
            var st = StageProgress.Instance;
            var p = PlayerGrowth.Instance;

            if (_cpText != null) _cpText.text = "전투력 " + UiKit.Num(CombatPowerService.GetTotalCp());
            if (_goldChip != null) _goldChip.Value.text = UiKit.Num(WalletAdapter.Instance?.Gold ?? 0);
            if (_gemChip != null) _gemChip.Value.text = UiKit.Num(WalletAdapter.Instance?.RedDiamond ?? 0);
            if (_blueChip != null)
                _blueChip.Value.text = UiKit.Num(CurrencyWallet.Instance != null
                    ? CurrencyWallet.Instance.Get(CurrencyId.BlueDiamond) : 0);
            if (_ticketChip != null)
                _ticketChip.Value.text = UiKit.Num(CurrencyWallet.Instance != null
                    ? CurrencyWallet.Instance.Get(CurrencyId.WeaponTicket) : 0);

            if (st != null)
            {
                if (_stageLabel != null)
                    _stageLabel.text = $"{st.GetHuntLabel()} · 돌파 {st.GetDisplayLabel()}";
                _stageBar?.Set(st.StageInChapter / 10f, $"Stage {st.StageInChapter}/10");
                bool clearReady = _battle != null && _battle.CanStartBreakthrough(out _);
                if (_questText != null)
                    _questText.text = clearReady
                        ? $"{st.GetDisplayLabel()} 돌파 가능 (도전)"
                        : $"{st.GetDisplayLabel()} 클리어하기 · CP {UiKit.Num(CombatPowerService.GetTotalCp())}";
            }

            if (p != null)
            {
                if (_lvText != null) _lvText.text = $"Lv.{p.Level} {_nick}";
                if (_nameText != null) _nameText.text = $"Lv.{p.Level} {_nick}";
                float maxMp = CombatPowerService.GetMaxMp();
                _mpBar?.Set(1f, $"MP {maxMp:0}/{maxMp:0}");
                float expPct = p.XpToNext > 0 ? (float)p.CurrentXp / p.XpToNext : 0f;
                _expBar?.Set(expPct, $"EXP {expPct * 100f:0.0}%");
            }

            _modals?.RefreshOpen();
            RefreshNavDots();
        }

        void RefreshNavDots()
        {
            if (_navDots == null) return;
            var cw = CurrencyWallet.Instance;
            SetDot(0, PlayerGrowth.Instance != null && PlayerGrowth.Instance.StatPoints > 0);
            SetDot(1, SlotEnhanceService.Instance != null && SlotEnhanceService.Instance.HasAffordableEnhance);
            SetDot(2, SkillAdapter.Instance != null && SkillAdapter.Instance.HasLearnableSkill);
            SetDot(3, cw != null && cw.Get(CurrencyId.WeaponTicket) >= 1);
            SetDot(4, cw != null && cw.Get(CurrencyId.CompanionTicket) >= 1);
        }

        /// <summary>배지는 테두리+점 2겹이라 enabled가 아니라 SetActive로 꺼야 안쪽 점이 남지 않는다.</summary>
        void SetDot(int i, bool on)
        {
            if (_navDots == null || i < 0 || i >= _navDots.Length) return;
            var d = _navDots[i];
            if (d != null && d.gameObject.activeSelf != on) d.gameObject.SetActive(on);
        }
    }
}
