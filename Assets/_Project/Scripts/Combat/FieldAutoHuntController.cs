using System.Collections;
using System.Collections.Generic;
using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using IdleMvp.UI;
using UnityEngine;

namespace IdleMvp.Combat
{
    public enum CombatMode
    {
        Hunt,
        Breakthrough,
        WorldBoss
    }

    /// <summary>
    /// Maple Idle-style dual mode: chapter hunt (respawn farm) + stage breakthrough push.
    /// </summary>
    public class FieldAutoHuntController : MonoBehaviour
    {
        public static FieldAutoHuntController Instance { get; private set; }

        public const int WavesPerNormalStage = 3;
        public const int MobsPerWave = 5;
        public const int HuntMaxMobs = 8; // 3층 필드를 채우는 밀도
        public const float HuntRespawnInterval = 1.5f;
        public const float HuntMobHpFactor = 0.28f;
        public const float BreakthroughMobHpFactor = 0.35f;
        public const float MiniBossHpFactor = 0.85f;
        public const float BossTimeLimit = 60f;
        public const float MoveSpeed = 420f;
        public const float AttackRange = 110f;
        public const float AttackInterval = 0.45f;
        public const float ActorSize = 260f;
        public const float GroundY = 48f;
        /// <summary>Platform floors: 0 = ground, 1..3 = platforms above. 실제로 쓰는 층은 테마마다 다르다(MapLayout).</summary>
        public const int FloorCount = 4;
        public static float FloorY(int floor) => GroundY + Mathf.Clamp(floor, 0, FloorCount - 1) * 150f;
        public const float CompanionSize = 150f;

        public CombatMode Mode { get; private set; } = CombatMode.Hunt;
        public bool Blocked { get; private set; }
        public bool IsBossFight { get; private set; }
        public bool IsMiniBossFight { get; private set; }
        public int CurrentWave { get; private set; } = 1;
        public int AliveMobs { get; private set; }
        public string StatusText { get; private set; } = "";
        public float FocusEnemyHp { get; private set; }
        public float FocusEnemyMaxHp { get; private set; }
        public float BossTimeLeft { get; private set; }
        public string LastMessage { get; private set; } = "";
        public bool IsFieldBound => _bound && _field != null;
        public bool AutoPushActive { get; private set; }
        public int PushFailCount => _pushFailCount;
        public const int MaxPushFails = 3;
        public float HeroHp => _heroHp;
        public float HeroMaxHp => _heroMaxHp;

        public event System.Action OnChanged;

        // Character Maker casting: preset name + attack animation per role.
        const string HeroPreset = "Warrior";
        const string HeroAttackAnim = "attack_swing1";
        const string CompanionMainPreset = "Bandit Cutthroat";
        const string CompanionSubPreset = "Bandit Bowman";
        const string CompanionAttackAnim = "attack_swing1";
        const string MiniBossPreset = "Orc Brute";
        const string MiniBossAttackAnim = "attack_bash";
        const string BossPreset = "Berserker";
        const string BossAttackAnim = "attack_twohanded_swing1";
        // 챕터별 몹 캐스팅 풀 — 초반 고블린/도적 → 중반 오크 → 후반 광전사 (Peasant는 몹에서 제외: 법사 외형과 중복)
        static readonly (string preset, string attack)[][] ChapterMobPools =
        {
            new[] { ("Goblin", "attack_swing1"), ("Bandit Cutthroat", "attack_swing1") },                                   // ch1
            new[] { ("Goblin", "attack_swing1"), ("Bandit Bowman", "attack_bow1") },                                        // ch2
            new[] { ("Bandit Cutthroat", "attack_swing1"), ("Bandit Bowman", "attack_bow1"), ("Raider", "attack_swing1") }, // ch3
            new[] { ("Orc Warrior", "attack_swing2"), ("Goblin", "attack_swing1") },                                        // ch4
            new[] { ("Orc Warrior", "attack_swing2"), ("Raider", "attack_swing1") },                                        // ch5
            new[] { ("Orc Warrior", "attack_swing2"), ("Orc Brute", "attack_bash") },                                       // ch6
            new[] { ("Raider", "attack_swing1"), ("Berserker", "attack_twohanded_swing1") },                                // ch7
            new[] { ("Orc Brute", "attack_bash"), ("Berserker", "attack_twohanded_swing1") },                               // ch8
            new[] { ("Berserker", "attack_twohanded_swing1"), ("Orc Warrior", "attack_swing2"), ("Raider", "attack_swing1") }, // ch9
            new[] { ("Orc Brute", "attack_bash"), ("Berserker", "attack_twohanded_swing1"), ("Raider", "attack_swing1") },     // ch10+
        };
        // 챕터별 몹 틴트 — 같은 프리셋도 챕터마다 색감이 달라 보이게
        static readonly Color[] ChapterMobTints =
        {
            Color.white,
            new Color(0.90f, 1f, 0.90f),
            new Color(1f, 0.92f, 0.86f),
            new Color(0.86f, 0.94f, 1f),
            new Color(1f, 0.88f, 0.96f),
            new Color(1f, 0.95f, 0.78f),
            new Color(0.96f, 0.84f, 0.84f),
            new Color(0.86f, 0.82f, 1f),
            new Color(0.78f, 0.94f, 1f),
            new Color(1f, 0.76f, 0.76f),
        };

        RectTransform _field;
        CharacterActorView _hero;
        CharacterActorView _compMain;
        CharacterActorView _compSub;
        string _compMainKey;
        string _compSubKey;
        string _heroAppearanceKey;
        readonly List<CharacterActorView> _mobs = new List<CharacterActorView>();
        float _attackTimer;
        float _fieldHalfW = 420f;

        // ---- 넓은 맵 (H8) ----
        /// <summary>논리 맵은 보이는 화면의 몇 배인가.</summary>
        public const float MapWidthFactor = 3f;
        /// <summary>맵 절반 폭 (필드 픽셀). 히어로·몹은 이 범위를 돌아다닌다.</summary>
        float _mapHalfW = 420f * MapWidthFactor;
        float _huntSpawnTimer;
        bool _bound;
        bool _resolving;
        float _heroHp = 100f;

        // ---- 사망 처리 (H3): 손실이 아니라 '정체'로 설계한다 ----
        /// <summary>부활 대기 남은 초 (0이면 생존).</summary>
        public float ReviveLeft { get; private set; }
        public bool IsDown => ReviveLeft > 0f;
        const float ReviveWait = 5f;
        /// <summary>세력 절기까지 남은 처치 수 카운터 (H8).</summary>
        int _sweepKills;
        const int SweepEveryKills = 25;

        /// <summary>연속 사망 횟수. 처치가 나오면 리셋.</summary>
        int _deathStreak;
        const int DemoteAfterDeaths = 3;
        float _heroMaxHp = 100f;
        float _enemyHitTimer;
        int _pushFailCount;
        float _bossHeavyTimer;
        float _bossSlamTimer;
        bool _bossEnraged;

        // R3 skill mechanic state
        float _buffEndTime;
        float _buffDefMul = 1f;   // Buff: damage reduction multiplier
        float _buffAtkMul = 1f;   // Buff: attack speed multiplier
        float _counterEndTime;
        float _counterDmg;
        readonly List<CharacterActorView> _summons = new List<CharacterActorView>();
        float _summonEndTime;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            if (WeaponSummonAdapter.Instance != null)
                WeaponSummonAdapter.Instance.OnChanged += RefreshHeroAppearance;
            // 방어구도 겉모습을 바꾼다 — 강화하면 그 자리에서 보이게
            if (InventoryAdapter.Instance != null)
                InventoryAdapter.Instance.OnChanged += RefreshHeroAppearance;
            IdleMvp.Core.JobProgress.OnJobChanged += RefreshHeroAppearance;
        }

        void OnDisable()
        {
            if (WeaponSummonAdapter.Instance != null)
                WeaponSummonAdapter.Instance.OnChanged -= RefreshHeroAppearance;
            if (InventoryAdapter.Instance != null)
                InventoryAdapter.Instance.OnChanged -= RefreshHeroAppearance;
            IdleMvp.Core.JobProgress.OnJobChanged -= RefreshHeroAppearance;
        }

        public bool CanStartBreakthrough(out string reason)
        {
            if (Mode == CombatMode.Breakthrough)
            {
                reason = "돌파 진행 중";
                return false;
            }
            int idx = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
            return CanPushStage(idx, out reason);
        }

        bool CanPushStage(int stageIndex, out string reason)
        {
            reason = "";
            var row = StageTable.Get(stageIndex);
            if (row == null)
            {
                reason = "스테이지 데이터 없음";
                return false;
            }

            if (!CombatPowerService.CanEnterStage(row, out reason))
                return false;
            return true;
        }

        public bool TryStartBreakthrough()
        {
            if (!CanStartBreakthrough(out var reason))
            {
                LastMessage = reason;
                StatusText = reason;
                OnChanged?.Invoke();
                return false;
            }

            AutoPushActive = true;
            _pushFailCount = 0;
            EnterBreakthrough();
            return true;
        }

        public void AbortBreakthrough()
        {
            if (Mode != CombatMode.Breakthrough && !AutoPushActive) return;
            AutoPushActive = false;
            _pushFailCount = 0;
            LastMessage = "돌파 포기 — 챕터 사냥으로 복귀";
            StageProgress.Instance?.SettleHuntToCleared();
            EnterHunt(showMessage: true);
        }

        public void PlayHeroJump()
        {
            if (_hero != null && _hero.Alive)
                _hero.PlayJump();
        }

        public void BindField(RectTransform field)
        {
            _field = field;
            _bound = _field != null;
            var fx = GetComponent<FieldCombatFx>() ?? gameObject.AddComponent<FieldCombatFx>();
            fx.Bind(_field);
            if (WeaponSummonAdapter.Instance != null)
            {
                WeaponSummonAdapter.Instance.OnChanged -= RefreshHeroAppearance;
                WeaponSummonAdapter.Instance.OnChanged += RefreshHeroAppearance;
            }
            StopAllCoroutines();
            StartCoroutine(BindNextFrame());
        }

        IEnumerator BindNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (_field != null && _field.rect.width > 10f)
                _fieldHalfW = Mathf.Max(200f, _field.rect.width * 0.5f - 20f);
            EnterHunt(showMessage: false);
        }

        /// <summary>Compatibility: restart current field hunt (or abort breakthrough into hunt).</summary>
        public void RestartStage() => EnterHunt(showMessage: false);

        public void ResetEnemyForCurrentStage() => RestartStage();

        void EnterHunt(bool showMessage)
        {
            ClearMobs();
            Mode = CombatMode.Hunt;
            Blocked = false;
            IsBossFight = false;
            IsMiniBossFight = false;
            CurrentWave = 1;
            BossTimeLeft = 0f;
            _attackTimer = 0f;
            _huntSpawnTimer = 0f;
            _resolving = false;
            ResetHeroHp();

            EnsureHero();
            if (_hero != null)
                _hero.SetX(-_mapHalfW * 0.55f, -_mapHalfW, _mapHalfW);
            SyncCompanionActors(force: true);

            var stage = StageProgress.Instance;
            var label = stage != null ? stage.GetHuntLabel() : "Ch.1-1";
            StatusText = showMessage && !string.IsNullOrEmpty(LastMessage)
                ? LastMessage
                : $"챕터 사냥 · {label}";

            var row = StageTable.Get(ActiveStageIndex());
            int fill = row != null && row.spawnCount > 0 ? row.spawnCount : HuntMaxMobs;
            for (int i = 0; i < fill; i++)
                SpawnHuntMob(row, i);

            RefreshFocusHp();
            OnChanged?.Invoke();
        }

        void EnterBreakthrough()
        {
            ClearMobs();
            Mode = CombatMode.Breakthrough;
            Blocked = false;
            IsBossFight = false;
            IsMiniBossFight = false;
            CurrentWave = 1;
            BossTimeLeft = 0f;
            _attackTimer = 0f;
            _resolving = false;
            LastMessage = "";
            // Must refill HP every attempt — otherwise retries start at 0 HP and fail instantly.
            ResetHeroHp();
            _enemyHitTimer = 2f; // spawn grace so first contact doesn't one-shot

            EnsureHero();
            if (_hero != null)
                _hero.SetX(-_mapHalfW * 0.55f, -_mapHalfW, _mapHalfW);
            SyncCompanionActors(force: true);

            var stage = StageProgress.Instance;
            var row = stage != null ? StageTable.Get(stage.StageIndex) : StageTable.Get(1);
            if (row == null)
            {
                StatusText = "돌파 실패 — 데이터 없음";
                StopAutoPushAndHunt("스테이지 데이터 없음");
                return;
            }

            if (row.boss)
            {
                IsBossFight = true;
                BossTimeLeft = row.bossTimeLimit > 0 ? row.bossTimeLimit : BossTimeLimit;
                StatusText = AutoPushActive
                    ? $"연속 돌파 · 보스 ({BossTimeLeft:0}초) · 실패 {_pushFailCount}/{MaxPushFails}"
                    : $"스테이지 돌파 · 보스 ({BossTimeLeft:0}초)";
                SpawnBoss(row);
                _bossHeavyTimer = 6f;
                _bossSlamTimer = 12f;
                _bossEnraged = false;
            }
            else
            {
                StatusText = AutoPushActive
                    ? $"연속 돌파 · Wave 1/{WavesPerNormalStage} · 실패 {_pushFailCount}/{MaxPushFails}"
                    : $"스테이지 돌파 · Wave 1/{WavesPerNormalStage}";
                SpawnWave(row, 1);
            }

            RefreshFocusHp();
            OnChanged?.Invoke();
        }

        void Update()
        {
            if (!_bound || _field == null || Blocked) return;
            if (StageProgress.Instance == null || PlayerGrowth.Instance == null) return;
            if (_hero == null || !_hero.Alive) return;

            if (IsDown)
            {
                ReviveLeft -= Time.deltaTime;
                if (ReviveLeft > 0f)
                {
                    StatusText = $"쓰러짐 — {ReviveLeft:0.0}초 후 운기조식";
                    return;   // 사냥 정지
                }
                ReviveLeft = 0f;
                ResetHeroHp();
                LastMessage = "운기조식으로 회복 — 전투 재개";
                StatusText = "";
                OnChanged?.Invoke();
            }

            if (_field.rect.width > 10f)
                _fieldHalfW = Mathf.Max(200f, _field.rect.width * 0.5f - 20f);
            _mapHalfW = _fieldHalfW * MapWidthFactor;
            TickHeroFall();
            TickMapScroll();

            SyncCompanionActors(force: false);
            UpdateSummons();
            UpdateBuffAura();
            TickAutoPotion();
            if (!IsBuffActive && (_buffAtkMul != 1f || _buffDefMul != 1f))
            {
                _buffAtkMul = 1f;
                _buffDefMul = 1f;
            }

            if (Mode == CombatMode.Hunt)
                TickHuntSpawn();
            else if ((Mode == CombatMode.Breakthrough || Mode == CombatMode.WorldBoss) && IsBossFight && BossTimeLeft > 0f)
            {
                BossTimeLeft -= Time.deltaTime;
                if (BossTimeLeft <= 0f)
                {
                    BossTimeLeft = 0f;
                    if (Mode == CombatMode.WorldBoss)
                    {
                        LastMessage = "월드보스 시간 초과";
                        EnterHunt(showMessage: true);
                    }
                    else
                        FailBreakthrough("시간 초과 — 챕터 사냥으로 복귀");
                    return;
                }

                if (Time.frameCount % 15 == 0)
                {
                    StatusText = Mode == CombatMode.WorldBoss
                        ? $"월드보스 ({BossTimeLeft:0}초)"
                        : $"스테이지 돌파 · 보스 ({BossTimeLeft:0}초)";
                    OnChanged?.Invoke();
                }
            }

            var target = FindNearestMob();
            if (target == null)
            {
                _hero.SetMoving(false);
                if (Mode == CombatMode.Breakthrough)
                    OnBreakthroughWaveCleared();
                else if (Mode == CombatMode.WorldBoss)
                {
                    LastMessage = RaidService.Instance != null && RaidService.Instance.ClearedToday
                        ? "월드보스 클리어"
                        : "월드보스 종료";
                    EnterHunt(showMessage: true);
                }
                return;
            }

            // Enemy hits hero (lite) — same floor only, skip during spawn grace
            _enemyHitTimer -= Time.deltaTime;
            var dmgRow = StageTable.Get(ActiveStageIndex());
            float eatk = dmgRow != null ? dmgRow.enemyAtk : 8f;
            float def = CombatPowerService.GetDef();

            // Boss pattern: enrage below 30% HP
            if (IsBossFight && target.IsBoss && !_bossEnraged &&
                target.Hp < target.MaxHp * 0.3f)
            {
                _bossEnraged = true;
                IdleMvp.Core.AudioService.BossEnrage();
                target.SetPermanentTint(new Color(1f, 0.4f, 0.3f, 1f));
                if (FieldCombatFx.Instance != null)
                {
                    var bPos = target.FieldAnchor;
                    FieldCombatFx.Instance.PlayAoeBlast(1, bPos, 120f);
                    FieldCombatFx.Instance.PopDamage(bPos + new Vector2(0f, 140f), 0f, true);
                }
                StatusText = "보스 분노!";
                OnChanged?.Invoke();
            }

            float enrageMul = _bossEnraged ? 1.3f : 1f;
            float enrageSpd = _bossEnraged ? 0.7f : 1.1f;

            // Boss pattern: heavy strike (8s cycle, 2x damage)
            if (IsBossFight && target.IsBoss)
            {
                _bossHeavyTimer -= Time.deltaTime;
                if (_bossHeavyTimer <= 0f)
                {
                    _bossHeavyTimer = _bossEnraged ? 5f : 8f;
                    IdleMvp.Core.AudioService.BossHeavy();
                    target.PlayAttack(Mathf.Sign(_hero.X - target.X));
                    float heavyRaw = Mathf.Max(1f, eatk * 2f * enrageMul - def * 0.5f);
                    float heavyTaken = Mathf.Min(heavyRaw, Mathf.Max(5f, _heroMaxHp * 0.35f));
                    if (IsBuffActive) heavyTaken *= _buffDefMul;
                    _heroHp -= heavyTaken;
                    if (FieldCombatFx.Instance != null)
                    {
                        var hPos = _hero.FieldAnchor;
                        FieldCombatFx.Instance.PlayHit(hPos, true);
                        FieldCombatFx.Instance.PopDamage(hPos + new Vector2(0f, 92f), heavyTaken, true);
                    }
                    OnChanged?.Invoke();
                }

                // Boss pattern: AoE slam (15s cycle, hits regardless of floor)
                _bossSlamTimer -= Time.deltaTime;
                if (_bossSlamTimer <= 0f)
                {
                    _bossSlamTimer = _bossEnraged ? 10f : 15f;
                    IdleMvp.Core.AudioService.BossSlam();
                    target.PlayAttack(Mathf.Sign(_hero.X - target.X));
                    float slamRaw = Mathf.Max(1f, eatk * 1.5f * enrageMul - def * 0.5f);
                    float slamTaken = Mathf.Min(slamRaw, Mathf.Max(5f, _heroMaxHp * 0.28f));
                    if (IsBuffActive) slamTaken *= _buffDefMul;
                    _heroHp -= slamTaken;
                    if (FieldCombatFx.Instance != null)
                    {
                        var bPos = target.FieldAnchor;
                        FieldCombatFx.Instance.PlayAoeBlast(2, bPos, 200f);
                        var hPos = _hero.FieldAnchor;
                        FieldCombatFx.Instance.PopDamage(hPos + new Vector2(0f, 92f), slamTaken, true);
                    }
                    OnChanged?.Invoke();
                }
            }

            if (_enemyHitTimer <= 0f && target.Floor == _hero.Floor &&
                Mathf.Abs(target.X - _hero.X) <= AttackRange * 1.15f)
            {
                _enemyHitTimer = IsBossFight ? enrageSpd : 1.1f;
                target.PlayAttack(Mathf.Sign(_hero.X - target.X));
                float raw = Mathf.Max(1f, eatk * enrageMul - def * 0.5f);
                float taken = Mathf.Min(raw, Mathf.Max(5f, _heroMaxHp * 0.22f));
                if (IsBuffActive) taken *= _buffDefMul;
                // 적 평타도 닿는 프레임에 들어간다 — 사망 판정은 매 프레임 도니 미뤄도 안전하다
                StartCoroutine(EnemyImpactCo(target, taken));
            }

            if (_heroHp <= 0f)
            {
                _heroHp = 0f;
                if (Mode == CombatMode.Hunt)
                {
                    _deathStreak++;
                    if (_deathStreak >= DemoteAfterDeaths)
                    {
                        // 3연속 사망 = 이 스테이지는 무리다 → 한 단계 아래로 (손실 페널티는 없다)
                        _deathStreak = 0;
                        var sp2 = StageProgress.Instance;
                        int cur = sp2 != null ? sp2.HuntStage : 1;
                        if (sp2 != null && cur > 1 && sp2.TrySetHuntStage(cur - 1))
                            LastMessage = $"연속 전투 불능 — 한 단계 물러섭니다 ({cur - 1})";
                        else
                            LastMessage = "전투 불능 — 이곳이 한계입니다";
                        ReviveLeft = ReviveWait;
                        EnterHunt(showMessage: false);
                    }
                    else
                    {
                        ReviveLeft = ReviveWait;
                        LastMessage = $"전투 불능 — {ReviveWait:0}초 운기조식 ({_deathStreak}/{DemoteAfterDeaths})";
                    }
                    OnChanged?.Invoke();
                }
                else if (Mode == CombatMode.WorldBoss)
                {
                    LastMessage = "월드보스 전투 실패";
                    EnterHunt(showMessage: true);
                }
                else
                    FailBreakthrough("전투 불능 — 돌파 실패");
                return;
            }

            float dx = target.X - _hero.X;
            _hero.Face(target.X);

            // Different floor: run under the target, then jump to its platform.
            if (target.Floor != _hero.Floor && Mathf.Abs(dx) <= AttackRange * 1.6f)
            {
                _hero.SetFloor(target.Floor, FloorY(target.Floor));
                PositionCompanionsBesideHero();
            }

            if (Mathf.Abs(dx) > AttackRange)
            {
                float step = Mathf.Sign(dx) * MoveSpeed * Time.deltaTime;
                if (Mathf.Abs(step) > Mathf.Abs(dx) - AttackRange)
                    step = dx - Mathf.Sign(dx) * AttackRange * 0.9f;
                _hero.SetX(_hero.X + step, -_mapHalfW, _mapHalfW);
                _hero.SetMoving(true);
                PositionCompanionsBesideHero();
            }
            else if (target.Floor != _hero.Floor)
            {
                // Under the platform waiting for the jump — treat as approaching.
                _hero.SetMoving(false);
            }
            else
            {
                _hero.SetMoving(false);
                PositionCompanionsBesideHero();
                _attackTimer += Time.deltaTime;
                float atkInterval = AttackInterval / (IsBuffActive ? _buffAtkMul : 1f)
                    / CombatPowerService.GetAttackSpeedMul();   // 수련 '신속'
                if (_attackTimer >= atkInterval)
                {
                    _attackTimer = 0f;
                    _hero.PlayAttack(Mathf.Sign(dx));
                    float skillBurst = SkillAdapter.Instance != null
                        ? SkillAdapter.Instance.TryTriggerAutoSkillMul()
                        : 1f;
                    float dmg = Mathf.Max(1f,
                        CombatPowerService.GetAtk() * 1.6f * CombatPowerService.GetOutgoingMul() * skillBurst);
                    // 수련 '패왕격' — 보스 한정 추가 피해
                    if (target.IsBoss)
                        dmg *= 1f + IdleMvp.Core.TrainingService.BossDmgPct * 0.01f;
                    // 치명타 — 표시용이던 크확/크뎀을 실판정으로 (벤치마크 동일 구조)
                    bool crit = Random.value * 100f < CombatPowerService.GetCritRatePct();
                    if (crit) dmg *= CombatPowerService.GetCritDamagePct() * 0.01f;
                    dmg = CombatPowerService.MitigateByDef(dmg, StageTable.Get(ActiveStageIndex()));
                    bool skillPulse = skillBurst > 1.05f || crit;
                    // 피해·이펙트는 칼이 닿는 프레임에 — 수치는 스윙 시작 시점 기준으로 이미 굳혔다
                    StartCoroutine(HeroImpactCo(target, dmg, skillPulse));
                }
            }

            foreach (var m in _mobs)
            {
                if (m == null || !m.Alive || m.IsBoss) continue;
                // Mobs stay on their own platform; only chase the hero on the same floor.
                if (m.Floor != _hero.Floor) { m.SetMoving(false); continue; }
                float mdx = _hero.X - m.X;
                if (Mathf.Abs(mdx) > AttackRange * 1.2f)
                {
                    float step = Mathf.Sign(mdx) * (MoveSpeed * 0.25f) * Time.deltaTime;
                    // 자기가 딛고 선 발판 밖으로는 못 나간다 — 끝까지 쫓다 허공으로 걸어가면 안 된다
                    float pMin, pMax;
                    PlatformBoundsAt(m.Floor, m.X, out pMin, out pMax);
                    m.SetX(m.X + step, pMin, pMax);
                    m.Face(_hero.X);
                    m.SetMoving(true);
                }
                else
                    m.SetMoving(false);
            }
        }

        /// <summary>적 평타의 타격 프레임. 때린 놈이 먼저 죽었으면 안 맞은 걸로 친다.</summary>
        IEnumerator EnemyImpactCo(CharacterActorView attacker, float taken)
        {
            yield return new WaitForSeconds(CharacterActorView.ImpactDelay);
            if (attacker == null || !attacker.Alive || _hero == null || !_hero.Alive || IsDown) yield break;
            _heroHp -= taken;
            TryCounterAttack(attacker);
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 히어로 평타의 타격 프레임. 스윙 시작 한 프레임 뒤에 피해·이펙트가 같이 터진다.
        /// 그 사이 다른 경로(동료·스킬)로 죽었으면 조용히 넘긴다.
        /// </summary>
        IEnumerator HeroImpactCo(CharacterActorView target, float dmg, bool skillPulse)
        {
            yield return new WaitForSeconds(CharacterActorView.ImpactDelay);
            if (target == null || !target.Alive) yield break;

            target.TakeDamage(dmg);
            if (FieldCombatFx.Instance != null)
            {
                var tPos = target.FieldAnchor;
                FieldCombatFx.Instance.PlayHit(tPos, skillPulse || IsGauntletEquipped);
                FieldCombatFx.Instance.PopDamage(tPos + new Vector2(0f, 92f), dmg, skillPulse);
                // 경지에 따른 기운. 무기든 권갑이든 경지가 오르면 기가 뿜어져 나온다
                PlayRealmAura(tPos);
                if (SkillAdapter.Instance != null &&
                    SkillAdapter.Instance.TryConsumeSkillPulse(out int skillId))
                {
                    ExecuteSkillMechanic(skillId, dmg, target);
                }
            }
            if (CompanionCombatBridge.Instance != null &&
                CompanionCombatBridge.Instance.TryConsumePulseDamage(out float pulse))
            {
                if (target.Alive) target.TakeDamage(pulse);
            }
            if (!target.Alive)
                OnMobKilled(target);
            RefreshFocusHp();
            CountAlive();
            OnChanged?.Invoke();
        }

        /// <summary>(floor, x)가 딛고 선 발판의 좌우 끝. 발판이 없으면 맵 전체를 준다.</summary>
        void PlatformBoundsAt(int floor, float x, out float minX, out float maxX)
        {
            int chapter = CurrentChapter();
            MapLayout.BoundsAt(MapLayout.ThemeOf(chapter), chapter, floor, x, _mapHalfW, out minX, out maxX);
        }

        /// <summary>
        /// 발판 끝에서 걸어 나가면 지면으로 떨어진다. 히어로는 x를 자유롭게 움직여
        /// 목표 밑까지 달려간 뒤 뛰어오르는 구조라, 낙하가 있어야 층 이동이 자연스럽다.
        /// </summary>
        void TickHeroFall()
        {
            if (_hero == null || !_hero.Alive || _hero.Floor <= 0) return;
            int chapter = CurrentChapter();
            float a, b;
            if (MapLayout.BoundsAt(MapLayout.ThemeOf(chapter), chapter, _hero.Floor, _hero.X, _mapHalfW, out a, out b))
                return;
            _hero.SetFloor(0, FloorY(0));
            PositionCompanionsBesideHero();
        }

        /// <summary>히어로를 화면 가운데 두되, 맵 끝에서는 더 밀지 않는다.</summary>
        void TickMapScroll()
        {
            var stage = FieldWorldStage.Instance;
            if (stage == null || _hero == null) return;
            float limit = Mathf.Max(0f, _mapHalfW - _fieldHalfW);
            // 사냥에서만 맵을 돌아다닌다. 돌파·월드보스는 화면 중앙에서 싸운다.
            float want = Mode == CombatMode.Hunt
                ? Mathf.Clamp(_hero.X, -limit, limit)
                : 0f;
            // 부드럽게 따라간다 — 즉시 스냅하면 화면이 튄다
            float cur = stage.ScrollX;
            stage.SetScrollX(Mathf.Lerp(cur, want, 1f - Mathf.Exp(-6f * Time.deltaTime)));
        }

        void TickHuntSpawn()
        {
            // 스폰 규칙은 스테이지 표가 정한다 (H2). 0이면 예전 상수로 폴백.
            var spawnRow = StageTable.Get(ActiveStageIndex());
            int maxMobs = spawnRow != null && spawnRow.spawnCount > 0 ? spawnRow.spawnCount : HuntMaxMobs;
            if (AliveMobs >= maxMobs) return;
            _huntSpawnTimer += Time.deltaTime;
            float interval = spawnRow != null && spawnRow.spawnDelay > 0f ? spawnRow.spawnDelay : HuntRespawnInterval;

            if (_huntSpawnTimer < interval) return;
            _huntSpawnTimer = 0f;

            var row = StageTable.Get(ActiveStageIndex());
            SpawnHuntMob(row, AliveMobs);
            RefreshFocusHp();
            OnChanged?.Invoke();
        }

        void ResetDeathStreakOnKill()
        {
            if (_deathStreak != 0) _deathStreak = 0;
        }

        void OnMobKilled(CharacterActorView mob)
        {
            ResetDeathStreakOnKill();
            if (Mode == CombatMode.Hunt && ++_sweepKills >= SweepEveryKills)
            {
                _sweepKills = 0;
                StartCoroutine(FactionSweepCo());
            }
            // 깨달음 퀘스트 진행 (경지 승급 시련)
            IdleMvp.Core.RealmService.ReportKill();
            IdleMvp.Core.DailyMissionService.Increment("kill");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Kill);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Kill);
            if (!string.IsNullOrEmpty(mob.PresetName))
                IdleMvp.Core.CollectionService.TryCollect(mob.PresetName);
            IdleMvp.Core.AudioService.Death();
            var row = StageTable.Get(ActiveStageIndex());

            // 필드 드랍 — 사냥터(챕터)·직업·레벨 연동 (S3)
            var drop = IdleMvp.Core.DropService.RollOnKill(row != null ? row.chapter : 1, mob.IsBoss);
            if (drop != null && FieldCombatFx.Instance != null)
                FieldCombatFx.Instance.PopLabel(drop.Value.Label,
                    mob.FieldAnchor + new Vector2(0f, 130f), drop.Value.Tint);

            if (Mode == CombatMode.Hunt)
            {
                float rewardMul = 1f;
                if (row != null)
                    rewardMul = CombatPowerService.RewardMulForGate(
                        CombatPowerService.EvaluateStageGate(row));
                float gold = row != null ? Mathf.Max(1f, row.clearGold * 0.08f) : 3f;
                float goldMul = rewardMul;
                if (SkillAdapter.Instance != null)
                    goldMul += SkillAdapter.Instance.PassiveGoldPct * 0.01f;
                if (PlayerGrowth.Instance != null)
                    goldMul += PlayerGrowth.Instance.SpecGoldPct * 0.01f;
                if (CompanionAdapter.Instance != null)
                    goldMul += CompanionAdapter.Instance.PassiveGoldPct * 0.01f;
                if (ArtifactService.Instance != null)
                    goldMul += ArtifactService.Instance.GoldPctBonus * 0.01f;
                goldMul += Core.CollectionService.BonusGoldPct * 0.01f;
                goldMul += Core.TrainingService.GoldGainPct * 0.01f;   // 수련 '재물운'
                if (GuildAdapter.Instance != null && GuildAdapter.Instance.Joined)
                    goldMul += GuildAdapter.Instance.GuildGoldPct * 0.01f;   // 길드 '재물 분배'
                var rebirth = RebirthService.Instance;
                if (rebirth != null && rebirth.Count > 0)
                    goldMul *= rebirth.GoldMul;
                gold *= goldMul;
                int xp = row != null
                    ? Mathf.Max(1, Mathf.FloorToInt(row.xpPerKill * rewardMul
                        * (rebirth != null ? rebirth.XpMul : 1f)
                        * (1f + Core.TrainingService.XpGainPct * 0.01f
                           + (PlayerGrowth.Instance != null
                              ? PlayerGrowth.Instance.SpecXpPct * 0.01f : 0f))))   // 수련 '오성' + 특별 '경험치 획득'
                    : 1;
                float stone = row != null ? 0.05f + row.index * 0.002f : 0.05f;
                PlayerWallet.Instance?.AddGold(gold);
                PlayerGrowth.Instance?.AddXp(xp);
                EquipmentService.Instance?.AddEnhanceStones(stone);
                CurrencyWallet.Instance?.Add(CurrencyId.MonsterPoint, 0.4f);
                if (UnityEngine.Random.value < 0.08f)
                    CurrencyWallet.Instance?.Add(CurrencyId.WeaponTicket, 1);
                if (UnityEngine.Random.value < 0.04f)
                    CurrencyWallet.Instance?.Add(CurrencyId.CompanionTicket, 1);
                if (UnityEngine.Random.value < 0.12f)
                    CurrencyWallet.Instance?.Add(CurrencyId.WeaponEnhanceStone, 0.5f);
                if (UnityEngine.Random.value < 0.03f)
                    CurrencyWallet.Instance?.Add(CurrencyId.AdditionalCube, 1);
                LootBoxService.Instance?.NotifyHuntKill();
            }
            else if (Mode == CombatMode.WorldBoss)
            {
                RaidService.Instance?.Strike();
            }
            else
            {
                float bg = row != null ? row.clearGold * (mob.IsBoss ? 0.2f : 0.05f) : 2f;
                int bx = row != null ? Mathf.Max(1, Mathf.FloorToInt(row.clearXp * (mob.IsBoss ? 0.2f : 0.05f))) : 1;
                PlayerWallet.Instance?.AddGold(bg);
                PlayerGrowth.Instance?.AddXp(bx);
            }

            if (mob != null)
            {
                _mobs.Remove(mob);
                mob.ReleaseAfterDeath();
            }
            CountAlive();
        }

        void OnBreakthroughWaveCleared()
        {
            if (_resolving || Mode != CombatMode.Breakthrough) return;
            _resolving = true;

            var stage = StageProgress.Instance;
            var row = StageTable.Get(stage != null ? stage.StageIndex : 1);

            if (IsBossFight || IsMiniBossFight)
            {
                CompleteBreakthrough(row);
                _resolving = false;
                return;
            }

            if (CurrentWave < WavesPerNormalStage)
            {
                CurrentWave++;
                StatusText = $"스테이지 돌파 · Wave {CurrentWave}/{WavesPerNormalStage}";
                SpawnWave(row, CurrentWave);
                RefreshFocusHp();
                OnChanged?.Invoke();
                _resolving = false;
                return;
            }

            // After final wave → miniboss
            IsMiniBossFight = true;
            StatusText = "스테이지 돌파 · 미니보스!";
            SpawnMiniBoss(row);
            RefreshFocusHp();
            OnChanged?.Invoke();
            _resolving = false;
        }

        void CompleteBreakthrough(StageRow row)
        {
            if (row != null)
            {
                PlayerWallet.Instance?.AddGold(row.clearGold);
                PlayerGrowth.Instance?.AddXp(row.clearXp);
                EquipmentService.Instance?.AddEnhanceStones(0.5f + row.index * 0.05f);
                string drop = ArtifactService.Instance?.TryDropRandom(0.12f);
                if (!string.IsNullOrEmpty(drop))
                    LastMessage = drop;
            }

            bool advanced = StageProgress.Instance != null && StageProgress.Instance.TryAdvanceAfterClear();
            _pushFailCount = 0;
            LastMessage = advanced
                ? $"돌파 성공! → {StageProgress.Instance.GetDisplayLabel()}"
                : "최종 스테이지 돌파 완료!";

            if (AutoPushActive && advanced)
            {
                int next = StageProgress.Instance.StageIndex;
                if (CanPushStage(next, out string gateReason))
                {
                    StatusText = LastMessage + " · 연속 도전";
                    EnterBreakthrough();
                    OnChanged?.Invoke();
                    return;
                }
                StopAutoPushAndHunt(gateReason + " — 사냥터로 복귀");
                return;
            }

            if (AutoPushActive && !advanced)
            {
                StopAutoPushAndHunt(LastMessage);
                return;
            }

            StageProgress.Instance?.SettleHuntToCleared();
            EnterHunt(showMessage: true);
        }

        void FailBreakthrough(string message)
        {
            if (AutoPushActive && _pushFailCount < MaxPushFails)
            {
                _pushFailCount++;
                LastMessage = $"{message} (재시도 {_pushFailCount}/{MaxPushFails})";
                StatusText = LastMessage;
                EnterBreakthrough();
                OnChanged?.Invoke();
                return;
            }

            StopAutoPushAndHunt(message + (AutoPushActive ? " — 연속 돌파 중단" : ""));
        }

        void StopAutoPushAndHunt(string reason)
        {
            AutoPushActive = false;
            _pushFailCount = 0;
            StageProgress.Instance?.SettleHuntToCleared();
            LastMessage = reason;
            EnterHunt(showMessage: true);
        }

        CharacterActorView SpawnActor(string goName, string preset, string attackAnim,
            float hp, bool hero, bool boss, float sizePx, bool showHpBar = true,
            int wuxiaTier = 0, int wuxiaKind = 0)
        {
            var go = new GameObject(goName);
            var actor = go.AddComponent<CharacterActorView>();
            actor.Setup(_field, preset, attackAnim, hp, hero, boss, GroundY, sizePx, showHpBar,
                wuxiaTier, wuxiaKind);
            return actor;
        }

        /// <summary>공격 모션으로 무기 종류를 짐작한다 (프리셋에는 무기 정보가 없다).</summary>
        static int KindForAttack(string anim)
        {
            if (string.IsNullOrEmpty(anim)) return 0;
            if (anim.Contains("bow")) return 2;       // 궁
            if (anim.Contains("bash")) return 3;      // 권갑
            return 0;                                  // 검
        }

        int CurrentChapter()
        {
            var row = StageTable.Get(ActiveStageIndex());
            return row != null ? row.chapter : 1;
        }

        static (string preset, string attack) PickMob(int seed, int chapter, int mobPreset = 0)
        {
            var pool = ChapterMobPools[Mathf.Clamp(chapter - 1, 0, ChapterMobPools.Length - 1)];
            // 조합 변형: 같은 챕터라도 2~3스테이지마다 등장 순서가 밀려 다른 조합처럼 보인다
            int shift = Mathf.Max(0, mobPreset);
            int idx = seed < 0 ? Random.Range(0, pool.Length) : seed % pool.Length;
            return pool[(idx + shift) % pool.Length];
        }

        static Color MobTint(int chapter)
        {
            return ChapterMobTints[Mathf.Clamp(chapter - 1, 0, ChapterMobTints.Length - 1)];
        }

        /// <summary>
        /// 적 복장 티어 — 챕터가 오르면 같은 프리셋도 좋은 무협 복장을 입는다.
        /// 챕터를 그대로 쓰면 잡졸이 플레이어보다 잘 입고 나온다(실측: 3챕터에서 몹 t3 vs 히어로 t1)
        /// → 절반 속도로 올린다.
        /// </summary>
        static int MobTier(int chapter, bool elite)
        {
            return Mathf.Clamp(1 + (chapter - 1) / 2 + (elite ? 1 : 0), 1, 10);
        }

        /// <summary>동료 복장 티어 — 등급이 높은 동료가 좋은 복장을 입는다.</summary>
        static int CompanionTier(int rarity)
        {
            return Mathf.Clamp(2 + rarity * 2, 1, 10);
        }

        void SpawnHuntMob(StageRow row, int slot)
        {
            int chapter = row != null ? row.chapter : 1;
            float baseHp = row != null ? row.enemyHp * HuntMobHpFactor * (row.mobHpMul > 0 ? row.mobHpMul : 1f) : 50f;
            // 발판이 있는 층에만 배치한다 — 테마마다 쓰는 층이 다르다(MapLayout).
            int theme = MapLayout.ThemeOf(chapter);
            int floor = MapLayout.PickFloor(theme, slot);
            // 상층 정예: 최상층 몹은 더 강하고 크게 (오르는 보람)
            bool elite = floor == FloorCount - 1;
            if (elite) baseHp *= 1.6f;
            var cast = PickMob(-1, chapter, row != null ? row.mobPreset : 0);
            var actor = SpawnActor($"HuntMob_{slot}_{Time.frameCount}", cast.preset, cast.attack,
                baseHp, false, false, ActorSize * (elite ? 1.0f : 0.85f), true,
                MobTier(chapter, elite), KindForAttack(cast.attack));
            var tint = MobTint(chapter);
            if (elite) tint = new Color(tint.r, tint.g * 0.82f, tint.b * 0.82f, 1f);
            actor.SetPermanentTint(tint);
            // 고른 층의 발판 칸 하나에 올린다. 발판 위에만 서므로 허공에 뜨지 않는다.
            // 히어로 코앞에 튀어나오지 않게 최소 거리를 두되, 좁은 발판이면 포기한다.
            float heroX = _hero != null ? _hero.X : 0f;
            int segs = Mathf.Max(1, MapLayout.Count(theme, floor));
            float minX, maxX, x;
            int guard = 8;
            do
            {
                MapLayout.Bounds(theme, chapter, floor, Random.Range(0, segs), _mapHalfW, out minX, out maxX);
                float pad = Mathf.Min(70f, (maxX - minX) * 0.2f);
                x = Random.Range(minX + pad, maxX - pad);
            } while (Mathf.Abs(x - heroX) < _fieldHalfW * 0.35f && guard-- > 0);
            actor.SetX(x, minX, maxX);
            actor.SetFloorInstant(floor, FloorY(floor));
            actor.Face(-_fieldHalfW);
            _mobs.Add(actor);
            CountAlive();
        }

        void SpawnWave(StageRow row, int wave)
        {
            ClearMobs();
            float baseHp = row != null ? row.enemyHp * BreakthroughMobHpFactor : 70f;
            baseHp *= 1f + (wave - 1) * 0.08f;
            for (int i = 0; i < MobsPerWave; i++)
            {
                var cast = PickMob(wave * 10 + i, CurrentChapter(), row != null ? row.mobPreset : 0);
                var actor = SpawnActor($"Mob_{wave}_{i}", cast.preset, cast.attack,
                    baseHp, false, false, ActorSize * 0.85f, true,
                    MobTier(CurrentChapter(), false), KindForAttack(cast.attack));
                float t = MobsPerWave <= 1 ? 0.5f : i / (float)(MobsPerWave - 1);
                float x = Mathf.Lerp(-_fieldHalfW * 0.1f, _fieldHalfW * 0.85f, t);
                actor.SetX(x, -_fieldHalfW, _fieldHalfW);
                int floor = i % (FloorCount + 1);
                if (floor >= FloorCount) floor = 0;
                actor.SetFloorInstant(floor, FloorY(floor));
                actor.Face(-_fieldHalfW);
                _mobs.Add(actor);
            }
            CountAlive();
        }

        void SpawnMiniBoss(StageRow row)
        {
            ClearMobs();
            float hp = row != null ? row.enemyHp * MiniBossHpFactor : 300f;
            var actor = SpawnActor("MiniBoss", MiniBossPreset, MiniBossAttackAnim, hp, false, true, ActorSize,
                true, MobTier(CurrentChapter(), true), KindForAttack(MiniBossAttackAnim));
            actor.SetX(_fieldHalfW * 0.4f, -_fieldHalfW, _fieldHalfW);
            actor.Face(-_fieldHalfW);
            _mobs.Add(actor);
            CountAlive();
            FocusEnemyHp = actor.Hp;
            FocusEnemyMaxHp = actor.MaxHp;
        }

        void SpawnBoss(StageRow row)
        {
            ClearMobs();
            float hp = row != null ? row.enemyHp : 500f;
            // 챕터별 보스 아트: 존재하지 않는 프리셋명 → BuildModel 폴백이 TplArt/Bosses(챕터)를 사용
            var actor = SpawnActor("Boss", "__ChapterBossSprite__", BossAttackAnim, hp, false, true, ActorSize * 1.35f);
            actor.SetX(_fieldHalfW * 0.45f, -_fieldHalfW, _fieldHalfW);
            actor.Face(-_fieldHalfW);
            _mobs.Add(actor);
            CountAlive();
            FocusEnemyHp = actor.Hp;
            FocusEnemyMaxHp = actor.MaxHp;
        }

        CharacterActorView FindNearestMob()
        {
            CharacterActorView best = null;
            float bestD = float.MaxValue;
            foreach (var m in _mobs)
            {
                if (m == null || !m.Alive) continue;
                float d = Mathf.Abs(m.X - _hero.X);
                // Off-floor targets cost extra so the hero clears its own floor first.
                if (_hero != null && m.Floor != _hero.Floor)
                    d += 220f;
                // 등 뒤 대상은 페널티 — 좌우로 매킬 홱홱 뒤집히면 '회전하며 깜빡이는'
                // 것처럼 보인다(유저 지적). 같은 방향을 먼저 정리한다.
                if (_hero != null && Mathf.Sign(m.X - _hero.X) != _hero.FacingSign)
                    d += 150f;
                if (d < bestD)
                {
                    bestD = d;
                    best = m;
                }
            }
            return best;
        }

        void CountAlive()
        {
            int n = 0;
            foreach (var m in _mobs)
                if (m != null && m.Alive) n++;
            AliveMobs = n;
        }

        void RefreshFocusHp()
        {
            var t = FindNearestMob();
            if (t != null)
            {
                FocusEnemyHp = t.Hp;
                FocusEnemyMaxHp = t.MaxHp;
            }
            else
            {
                FocusEnemyHp = 0f;
                FocusEnemyMaxHp = 1f;
            }
        }

        /// <summary>
        /// Plays out an active skill's field mechanic: extra distributed damage + real
        /// projectiles/blasts. Base skillBurst damage already landed on the main target;
        /// mechanics add spectacle plus bonus damage at reduced coefficients.
        /// </summary>
        void ExecuteSkillMechanic(int skillId, float dmg, CharacterActorView target)
        {
            var fx = FieldCombatFx.Instance;
            if (fx == null || _hero == null || target == null) return;
            var nodes = SkillAdapter.Instance != null ? SkillTreeDef.Nodes : null;
            var node = nodes != null && skillId >= 0 && skillId < nodes.Length
                ? nodes[skillId]
                : default(SkillTreeNode);
            var heroPos = _hero.FieldAnchor + new Vector2(0f, 46f);
            var tPos = target.FieldAnchor + new Vector2(0f, 30f);
            fx.PlayCastFlash(skillId, heroPos);

            switch (node.Mechanic)
            {
                case SkillMechanic.Projectile:
                {
                    int shots = Mathf.Max(1, node.Hits);
                    for (int i = 0; i < shots; i++)
                    {
                        var captured = target;
                        float delay = i * 0.12f;
                        StartCoroutine(DelayedProjectile(delay, skillId, heroPos, captured,
                            arcHeight: 26f, coef: 0.55f));
                    }
                    break;
                }
                case SkillMechanic.Homing:
                {
                    int shots = Mathf.Max(1, node.Hits);
                    var picks = NearestMobs(shots);
                    for (int i = 0; i < picks.Count; i++)
                    {
                        var captured = picks[i];
                        StartCoroutine(DelayedProjectile(i * 0.10f, skillId, heroPos, captured,
                            arcHeight: 90f + i * 26f, coef: 0.65f));
                    }
                    break;
                }
                case SkillMechanic.Pierce:
                {
                    float dir = Mathf.Sign(target.X - _hero.X);
                    if (dir == 0f) dir = 1f;
                    var edge = new Vector2(dir > 0f ? _fieldHalfW : -_fieldHalfW, heroPos.y);
                    fx.PlayProjectile(skillId, heroPos, edge, 0.30f, 0f, null);
                    // 스냅샷 순회 — 타격 처리 중 _mobs가 변형될 수 있음
                    foreach (var m in new System.Collections.Generic.List<CharacterActorView>(_mobs))
                    {
                        if (m == null || !m.Alive || m.Floor != _hero.Floor) continue;
                        if (Mathf.Sign(m.X - _hero.X) != dir) continue;
                        float dist = Mathf.Abs(m.X - _hero.X);
                        StartCoroutine(DelayedStrike(0.30f * (dist / Mathf.Max(1f, _fieldHalfW * 2f)) + 0.02f,
                            m, dmg * 0.7f, skillId, false));
                    }
                    break;
                }
                case SkillMechanic.AoE:
                {
                    float radius = node.Radius > 1f ? node.Radius : 170f;
                    fx.PlaySkill(skillId, tPos);
                    fx.PlayAoeBlast(skillId, tPos, radius);
                    // 스냅샷 순회 — DamageMobExtra의 킬 처리가 _mobs를 변형함
                    foreach (var m in new System.Collections.Generic.List<CharacterActorView>(_mobs))
                    {
                        if (m == null || !m.Alive || m == target) continue;
                        if (m.Floor != target.Floor) continue;
                        if (Mathf.Abs(m.X - target.X) > radius) continue;
                        DamageMobExtra(m, dmg * 0.75f, true);
                    }
                    break;
                }
                case SkillMechanic.MultiHit:
                {
                    int hits = Mathf.Max(2, node.Hits);
                    for (int i = 1; i < hits; i++)
                        StartCoroutine(DelayedStrike(i * 0.09f, target, dmg * 0.45f, skillId, i == hits - 1));
                    fx.PlaySkill(skillId, tPos);
                    break;
                }
                case SkillMechanic.Buff:
                {
                    float dur = node.Duration > 0f ? node.Duration : 10f;
                    _buffEndTime = Time.time + dur;
                    BuffSkillSlot = skillId;
                    bool isMaShin = node.Name == "마신강림";
                    _buffAtkMul = isMaShin ? 2f : 1f;
                    _buffDefMul = isMaShin ? 1.2f : 0.5f;
                    fx.PlaySkill(skillId, heroPos);
                    break;
                }
                case SkillMechanic.Counter:
                {
                    float dur = node.Duration > 0f ? node.Duration : 10f;
                    _counterEndTime = Time.time + dur;
                    BuffSkillSlot = skillId;
                    _counterDmg = dmg * 0.7f;
                    fx.PlaySkill(skillId, heroPos);
                    break;
                }
                case SkillMechanic.Summon:
                {
                    ClearSummons();
                    int count = Mathf.Max(1, node.Hits);
                    float dur = node.Duration > 0f ? node.Duration : 12f;
                    _summonEndTime = Time.time + dur;
                    // 혈영분신술 — 분신은 본인 모습이어야 한다. 산적("Bandit Cutthroat")이
                    // 튀어나오던 하드코딩을 히어로 프리셋 + 그림자 틴트로 교체.
                    ResolveHeroAppearance(out string cloneP, out string cloneA);
                    for (int i = 0; i < count; i++)
                    {
                        float xOff = (i + 1) * 60f * (i % 2 == 0 ? -1f : 1f);
                        var eqw = WeaponSummonAdapter.Instance?.Equipped;
                        var s = SpawnActor($"Summon_{i}", cloneP, cloneA,
                            9999f, false, false, CompanionSize, false,
                            MobTier(CurrentChapter(), false), eqw != null ? eqw.kind : 0);
                        s.SetPermanentTint(new Color(0.55f, 0.45f, 0.75f, 0.8f));
                        s.SetX(_hero.X + xOff, -_fieldHalfW, _fieldHalfW);
                        _summons.Add(s);
                    }
                    fx.PlaySkill(skillId, heroPos);
                    break;
                }
                case SkillMechanic.HPCost:
                {
                    float cost = node.HpCostPct > 0f ? node.HpCostPct : 0.1f;
                    _heroHp -= _heroMaxHp * cost;
                    if (_heroHp < 1f) _heroHp = 1f;
                    if (fx != null)
                        fx.PopDamage(heroPos + new Vector2(0f, 60f), _heroMaxHp * cost, true);
                    float radius = node.Radius > 1f ? node.Radius : 170f;
                    fx.PlaySkill(skillId, tPos);
                    fx.PlayAoeBlast(skillId, tPos, radius);
                    foreach (var m in new System.Collections.Generic.List<CharacterActorView>(_mobs))
                    {
                        if (m == null || !m.Alive || m == target) continue;
                        if (m.Floor != target.Floor) continue;
                        if (Mathf.Abs(m.X - target.X) > radius) continue;
                        DamageMobExtra(m, dmg * 0.85f, true);
                    }
                    break;
                }
                case SkillMechanic.DOT:
                {
                    int ticks = Mathf.Max(2, node.Hits);
                    fx.PlaySkill(skillId, tPos);
                    for (int i = 0; i < ticks; i++)
                        StartCoroutine(DelayedStrike(0.8f * (i + 1), target, dmg * 0.3f, skillId, i == ticks - 1, dotTick: true));
                    break;
                }
                default:
                    fx.PlaySkill(skillId, tPos);
                    break;
            }
        }

        bool IsBuffActive => Time.time < _buffEndTime;
        bool IsCounterActive => Time.time < _counterEndTime;

        /// <summary>남은 버프 시간 (초). HUD 스킬독이 표시한다. 없으면 0.</summary>
        public float BuffTimeLeft => Mathf.Max(0f, _buffEndTime - Time.time);
        public float CounterTimeLeft => Mathf.Max(0f, _counterEndTime - Time.time);
        /// <summary>마지막으로 버프/반격을 시전한 스킬독 슬롯 (없으면 -1).</summary>
        public int BuffSkillSlot { get; private set; } = -1;

        // 버프·반격이 살아 있는 동안 히어로 주위에 기운을 반복해 표시한다.
        // 시전 순간 한 번 번쩍이고 끝나던 것 — 지속 스킬이 지속처럼 보여야 한다.
        float _auraTimer;

        void UpdateBuffAura()
        {
            bool buff = IsBuffActive, counter = IsCounterActive;
            if (!buff && !counter) return;
            _auraTimer -= Time.deltaTime;
            if (_auraTimer > 0f || _hero == null || FieldCombatFx.Instance == null) return;
            _auraTimer = 0.8f;
            var pos = _hero.FieldAnchor;
            if (buff)     // 마신강림 = 진홍, 금강불괴 = 금빛
                FieldCombatFx.Instance.PlayAoeBlast(pos, 110f,
                    _buffAtkMul > 1f ? new Color(0.9f, 0.2f, 0.25f, 0.5f) : new Color(1f, 0.85f, 0.35f, 0.5f));
            if (counter)  // 태극유운검 = 청백
                FieldCombatFx.Instance.PlayAoeBlast(pos, 90f, new Color(0.55f, 0.8f, 1f, 0.45f));
        }

        /// <summary>
        /// 세력 절기 (H8). 25처치마다 화면 전체를 쓸어내는 광역 연출.
        /// 정파=검강 참격(넓고 한 방), 사파=암기 난무(잦고 얕게), 마도=혈마파(느리고 무겁게).
        /// </summary>
        IEnumerator FactionSweepCo()
        {
            var fx = FieldCombatFx.Instance;
            if (fx == null || _hero == null) yield break;

            // 세력마다 쓰는 이펙트 아트와 리듬이 다르다 (0=뇌전 1=낙성 2=한빙)
            string tree = IdleMvp.Core.JobProgress.TreeId;
            int waves, art; float step, radius, coef; string label;
            if (tree == "bowmaster") { waves = 7; step = 0.08f; radius = 190f; coef = 1.1f; art = 2; label = "암기난무"; }
            else if (tree == "archmage") { waves = 3; step = 0.20f; radius = 380f; coef = 3.0f; art = 1; label = "혈마파"; }
            else { waves = 5; step = 0.11f; radius = 300f; coef = 1.9f; art = 0; label = "검강참"; }

            var tint = FieldCombatFx.FactionTint();
            fx.PopLabel(label, _hero.FieldAnchor + new Vector2(0f, 170f), tint);
            IdleMvp.Core.AudioService.Click();

            float dmg = Mathf.Max(1f, CombatPowerService.GetAtk() * coef
                * CombatPowerService.GetOutgoingMul());
            var row = StageTable.Get(ActiveStageIndex());

            for (int w = 0; w < waves; w++)
            {
                // 히어로 좌우를 번갈아 훑는다 — 한자리 폭발보다 '쓸어내는' 그림이 된다
                float side = (w % 2 == 0) ? 1f : -1f;
                var center = _hero.FieldAnchor + new Vector2(side * radius * 0.55f, 40f);
                fx.PlayAoeBlast(center, radius, tint);
                // 실제 스킬 아트를 층마다 얹는다 — 링만으로는 눈에 띄지 않는다
                fx.PlaySkill(art, center);
                fx.PlaySkill(art, center + new Vector2(side * radius * 0.8f, 70f));
                // 스냅샷 순회 — 킬 처리가 _mobs를 변형함
                foreach (var m in new System.Collections.Generic.List<CharacterActorView>(_mobs))
                {
                    if (m == null || !m.Alive) continue;
                    if ((m.FieldAnchor - center).sqrMagnitude > radius * radius) continue;
                    DamageMobExtra(m, CombatPowerService.MitigateByDef(dmg, row), true);
                }
                yield return new WaitForSeconds(step);
            }
        }

        void TryCounterAttack(CharacterActorView attacker)
        {
            if (!IsCounterActive || attacker == null || !attacker.Alive) return;
            if (Random.value > 0.3f) return;
            DamageMobExtra(attacker, _counterDmg, true);
            var fx = FieldCombatFx.Instance;
            if (fx != null && _hero != null)
                fx.PlayHit(attacker.FieldAnchor, true);
        }

        void ClearSummons()
        {
            foreach (var s in _summons)
                if (s != null) Destroy(s.gameObject);
            _summons.Clear();
        }

        float _summonAtkTimer;
        void UpdateSummons()
        {
            if (_summons.Count == 0) return;
            if (Time.time >= _summonEndTime) { ClearSummons(); return; }
            CharacterActorView target = null;
            foreach (var m in _mobs)
                if (m != null && m.Alive) { target = m; break; }
            if (target == null) return;
            _summonAtkTimer += Time.deltaTime;
            bool canAttack = _summonAtkTimer >= AttackInterval * 1.2f;
            foreach (var s in _summons)
            {
                if (s == null) continue;
                float dx = target.X - s.X;
                if (Mathf.Abs(dx) > AttackRange)
                {
                    s.SetX(s.X + Mathf.Sign(dx) * MoveSpeed * 0.6f * Time.deltaTime, -_fieldHalfW, _fieldHalfW);
                }
                else if (canAttack)
                {
                    s.PlayAttack(Mathf.Sign(dx));
                    float dmg = Mathf.Max(1f, CombatPowerService.GetAtk() * 0.6f);
                    DamageMobExtra(target, dmg, false);
                }
            }
            if (canAttack) _summonAtkTimer = 0f;
        }

        System.Collections.Generic.List<CharacterActorView> NearestMobs(int count)
        {
            var list = new System.Collections.Generic.List<CharacterActorView>();
            foreach (var m in _mobs)
                if (m != null && m.Alive) list.Add(m);
            list.Sort((a, b) =>
                Mathf.Abs(a.X - _hero.X).CompareTo(Mathf.Abs(b.X - _hero.X)));
            if (list.Count > count) list.RemoveRange(count, list.Count - count);
            return list;
        }

        System.Collections.IEnumerator DelayedProjectile(float delay, int skillId, Vector2 from,
            CharacterActorView mob, float arcHeight, float coef)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (mob == null || !mob.Alive || FieldCombatFx.Instance == null) yield break;
            var to = mob.FieldAnchor + new Vector2(0f, 34f);
            float dist = Vector2.Distance(from, to);
            float dmg = Mathf.Max(1f, CombatPowerService.GetAtk() * 1.6f
                * CombatPowerService.GetOutgoingMul() * coef);
            var captured = mob;
            FieldCombatFx.Instance.PlayProjectile(skillId, from, to,
                Mathf.Clamp(dist / 1500f, 0.12f, 0.4f), arcHeight,
                () => DamageMobExtra(captured, dmg, true));
        }

        System.Collections.IEnumerator DelayedStrike(float delay, CharacterActorView mob,
            float dmg, int skillId, bool finalFx, bool dotTick = false)
        {
            yield return new WaitForSeconds(delay);
            if (mob == null || !mob.Alive) yield break;
            if (finalFx && FieldCombatFx.Instance != null)
                FieldCombatFx.Instance.PlaySkill(skillId, mob.FieldAnchor + new Vector2(0f, 26f));
            // 중독·출혈 틱은 독색 기운을 함께 띄운다 — 지속 피해가 지속처럼 보이게
            if (dotTick && FieldCombatFx.Instance != null)
                FieldCombatFx.Instance.PlayAoeBlast(mob.FieldAnchor + new Vector2(0f, 20f), 55f,
                    new Color(0.45f, 0.85f, 0.3f, 0.55f));
            DamageMobExtra(mob, dmg, false);
        }

        /// <summary>
        /// 권갑(무기 종류 4)을 들고 있는가.
        /// 권갑은 손에 끼는 것이라 스프라이트가 작아 화면에서 거의 안 보인다.
        /// 그래서 '무기를 든 티'를 이펙트로 대신 낸다.
        /// </summary>
        bool IsGauntletEquipped
        {
            get
            {
                var w = Adapters.WeaponSummonAdapter.Instance;
                return w != null && w.Equipped != null && w.Equipped.kind == 4;
            }
        }

        /// <summary>
        /// 경지 기운(氣) — 타격 지점에 경지에 맞는 기운을 띄운다.
        ///
        /// 권갑만 이펙트를 주면 다른 무기는 밋밋하고, 반대로 다 같은 이펙트를 주면
        /// 강해진 티가 안 난다. 그래서 **경지가 이펙트의 크기와 색을 정하고**,
        /// 무기 종류는 그 위에서 모양만 다르게 한다.
        ///  - 삼류(0)는 아직 기를 못 뿜으니 이펙트가 없다
        ///  - 위로 갈수록 커지고 색이 바뀐다 (은백 → 청 → 녹 → 금 → 주황 → 보라 → 적)
        ///  - 권갑은 무기가 안 보이므로 한 단계 더 크게 준다
        /// </summary>
        void PlayRealmAura(Vector2 pos)
        {
            var fx = FieldCombatFx.Instance;
            if (fx == null) return;

            var realm = Core.RealmService.Current;
            if (realm == null || realm.AuraScale <= 0.01f) return;   // 삼류는 기가 안 보인다

            float scale = realm.AuraScale;
            int kind = EquippedWeaponKind;
            if (kind == 4) scale *= 1.35f;      // 권갑: 무기가 안 보이니 기운을 크게

            // 무기 종류마다 기운이 퍼지는 모양이 다르다
            //   검·도 = 좁고 날카롭게 / 창곤 = 길게 / 기병·권갑 = 둥글게
            float radius = kind == 0 || kind == 1 ? 38f
                         : kind == 2 ? 54f
                         : 46f;
            fx.PlayAoeBlast(pos, radius * scale, realm.Aura);
        }

        /// <summary>지금 든 무기 종류 (없으면 검).</summary>
        int EquippedWeaponKind
        {
            get
            {
                var w = Adapters.WeaponSummonAdapter.Instance;
                return w != null && w.Equipped != null ? w.Equipped.kind : 0;
            }
        }

        /// <summary>Skill-mechanic bonus damage path: pops numbers and settles kills.</summary>
        void DamageMobExtra(CharacterActorView m, float dmg, bool strong)
        {
            if (m == null || !m.Alive) return;
            dmg = CombatPowerService.MitigateByDef(dmg, StageTable.Get(ActiveStageIndex()));
            m.TakeDamage(dmg);
            if (FieldCombatFx.Instance != null)
            {
                var pos = m.FieldAnchor;
                FieldCombatFx.Instance.PlayHit(pos, strong);
                FieldCombatFx.Instance.PopDamage(pos + new Vector2(0f, 92f), dmg, strong);
            }
            if (!m.Alive) OnMobKilled(m);
            RefreshFocusHp();
            CountAlive();
            OnChanged?.Invoke();
        }

        void EnsureHero()
        {
            EnsureHero(force: false);
        }

        void EnsureHero(bool force)
        {
            if (_field == null) return;
            ResolveHeroAppearance(out string preset, out string anim);
            // 무기 외형 서명 — 장착 무기(등급·종류)가 손에 든 파츠를 정한다 (HippoLookService)
            var eqw = WeaponSummonAdapter.Instance?.Equipped;
            string weaponSig = eqw != null ? eqw.catalogId + "." + eqw.rarity + "." + eqw.kind : "";
            // 방어구 외형 서명 — 티어·등급이 바뀔 때만 재생성되도록 키에 넣는다.
            // (레벨을 그대로 쓰면 강화 클릭마다 재생성돼 도로 깜빡인다)
            string armorSig = "";
            var inv = IdleMvp.Adapters.InventoryAdapter.Instance;
            if (inv != null && inv.Slots != null)
                for (int i = 1; i < inv.Slots.Length && i < 6; i++)
                {
                    var d = IdleMvp.Core.ContentCatalog.GetEquip(i, inv.Slots[i].level);
                    if (d != null) armorSig += d.tier + "." + d.rarity + "/";
                }
            // Hippo 커스터마이징(신체·헤어·얼굴)도 서명에 — 저장 즉시 필드 반영
            var hippoLook = IdleMvp.Progression.HippoLookService.Current;
            string hippoSig = hippoLook != null ? JsonUtility.ToJson(hippoLook) : "";
            string key = preset + "|" + anim + "|" + weaponSig + "|" + armorSig + "|" + hippoSig;
            if (_hero != null && !force && key == _heroAppearanceKey) return;
            float keepX = _hero != null ? _hero.X : -_fieldHalfW * 0.55f;
            if (_hero != null)
            {
                Destroy(_hero.gameObject);
                _hero = null;
            }
            _heroAppearanceKey = key;
            _hero = SpawnActor("Hero", preset, anim, 99999f, true, false, ActorSize, showHpBar: false);
            _hero.SetX(keepX, -_fieldHalfW, _fieldHalfW);
        }

        public void RefreshHeroAppearance()
        {
            if (!_bound || _field == null) return;
            // ⚠️ force:true 로 뒀더니 무기 드랍·인벤토리 이벤트마다 외형이 안 바뀌어도
            // 리그를 통째로 재생성했고, 재생성 직후 SpriteSkin이 묶이기 전 1~2프레임 동안
            // 캐릭터 폭이 0.83→0.06으로 무너져 '회전하며 사라지는' 것처럼 보였다
            // (FlickerProbe 실측). 외형 서명이 바뀔 때만 재생성한다.
            EnsureHero(force: false);
            SyncCompanionActors(force: false);
            // 필드 리그만 다시 만들면 HUD 초상화·장비창 프리뷰는 예전 무기를 든 채 남는다.
            // 프리뷰들은 AppearanceService.OnChanged 로 다시 그린다.
            IdleMvp.Progression.AppearanceService.NotifyWeaponChanged();
            OnChanged?.Invoke();
        }

        static void ResolveHeroAppearance(out string preset, out string attackAnim)
        {
            // Look = job (base rig; saved customization overlays in CharacterActorView).
            preset = IdleMvp.Progression.AppearanceService.PresetForJob(IdleMvp.Core.JobProgress.JobId);

            // Motion = equipped weapon kind (0 sword, 1 staff/magic, 2 bow, 3 claw/dagger).
            var w = WeaponSummonAdapter.Instance?.Equipped;
            int kind = w != null ? w.kind : 0;
            switch (kind)
            {
                case 1: attackAnim = "attack_spell1"; break;
                case 2: attackAnim = "attack_bow1"; break;
                case 3: attackAnim = "attack_swing3"; break;
                default:
                    attackAnim = w != null && w.rarity >= 3 ? "attack_twohanded_swing1" : "attack_swing1";
                    break;
            }
        }

        /// <summary>
        /// Spawn/update main+sub companion actors beside the hero.
        /// Visible whenever Main/Sub is set; scales up during field sortie pulse.
        /// </summary>
        public void SyncCompanionActors(bool force = false)
        {
            if (_field == null || _hero == null) return;
            var ca = CompanionAdapter.Instance;
            var mainItem = ca?.Main;
            string mainId = mainItem?.id;
            Adapters.CompanionItem subItem = null;
            if (ca != null)
            {
                foreach (var o in ca.Owned)
                {
                    if (o.sub) { subItem = o; break; }
                }
            }
            string subId = subItem?.id;

            if (force || mainId != _compMainKey)
            {
                if (_compMain != null) { Destroy(_compMain.gameObject); _compMain = null; }
                _compMainKey = mainId;
                if (!string.IsNullOrEmpty(mainId))
                {
                    // 동료마다 다른 리그를 쓴다. 동료창 카드도 같은 프리셋의 초상화를 쓴다
                    // (예전엔 여기만 하드코딩 프리셋이라 뽑은 동료와 딴판이었다).
                    string preset = UI.CompanionArt.PresetFor(mainItem.name, mainItem.rarity);
                    _compMain = SpawnActor("CompMain", preset, CompanionAttackAnim,
                        99999f, false, false, CompanionSize, false,
                        CompanionTier(mainItem.rarity), KindForAttack(CompanionAttackAnim));
                }
            }
            if (force || subId != _compSubKey)
            {
                if (_compSub != null) { Destroy(_compSub.gameObject); _compSub = null; }
                _compSubKey = subId;
                if (!string.IsNullOrEmpty(subId))
                {
                    string preset = UI.CompanionArt.PresetFor(subItem.name, subItem.rarity);
                    _compSub = SpawnActor("CompSub", preset, CompanionAttackAnim,
                        99999f, false, false, CompanionSize * 0.9f, false,
                        CompanionTier(subItem.rarity), KindForAttack(CompanionAttackAnim));
                }
            }

            PositionCompanionsBesideHero();
            ApplyCompanionSortieVisual();
        }

        void PositionCompanionsBesideHero()
        {
            if (_hero == null) return;
            float hx = _hero.X;
            if (_compMain != null)
            {
                _compMain.SetX(hx - 95f, -_fieldHalfW, _fieldHalfW);
                if (_compMain.Floor != _hero.Floor)
                    _compMain.SetFloor(_hero.Floor, FloorY(_hero.Floor));
                _compMain.Face(_hero.X + 200f);
                _compMain.SetMoving(false);
            }
            if (_compSub != null)
            {
                _compSub.SetX(hx - 165f, -_fieldHalfW, _fieldHalfW);
                if (_compSub.Floor != _hero.Floor)
                    _compSub.SetFloor(_hero.Floor, FloorY(_hero.Floor));
                _compSub.Face(_hero.X + 200f);
                _compSub.SetMoving(false);
            }
        }

        void ApplyCompanionSortieVisual()
        {
            bool active = CompanionCombatBridge.Instance != null && CompanionCombatBridge.Instance.IsActive;
            float scale = active ? 1.12f : 1f;
            if (_compMain != null) _compMain.transform.localScale = Vector3.one * scale;
            if (_compSub != null) _compSub.transform.localScale = Vector3.one * (active ? 1.08f : 1f);
        }

        void ClearCompanions()
        {
            if (_compMain != null) Destroy(_compMain.gameObject);
            if (_compSub != null) Destroy(_compSub.gameObject);
            _compMain = null;
            _compSub = null;
            _compMainKey = null;
            _compSubKey = null;
        }

        void ClearMobs()
        {
            foreach (var m in _mobs)
            {
                if (m != null) Destroy(m.gameObject);
            }
            _mobs.Clear();
            AliveMobs = 0;
            ClearSummons();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            ClearMobs();
            ClearCompanions();
            if (_hero != null) Destroy(_hero.gameObject);
        }

        public static float GetPlayerAtk() => CombatPowerService.GetAtk();

        public static float GetPlayerCp() => CombatPowerService.GetTotalCp();

        /// <summary>
        /// 지금 실제로 싸우고 있는 스테이지. 사냥 중이면 사냥 단계, 돌파 중이면 돌파 단계다.
        /// 배경·발판도 이 값을 따라야 몹 배치와 챕터가 어긋나지 않는다.
        /// </summary>
        public int ActiveStageIndex()
        {
            var stage = StageProgress.Instance;
            if (stage == null) return 1;
            return Mode == CombatMode.Hunt ? stage.HuntStage : stage.StageIndex;
        }

        void ResetHeroHp()
        {
            _heroMaxHp = CombatPowerService.GetMaxHp();
            _heroHp = _heroMaxHp;
            _enemyHitTimer = 0.5f;
        }

        /// <summary>물약: 체력을 비율만큼 회복한다. 실제 회복량을 돌려준다.</summary>
        public float HealHero(float pct)
        {
            if (_heroMaxHp <= 0f) _heroMaxHp = CombatPowerService.GetMaxHp();
            float before = _heroHp;
            _heroHp = Mathf.Min(_heroMaxHp, _heroHp + _heroMaxHp * pct);
            return _heroHp - before;
        }

        /// <summary>
        /// HP가 임계 아래로 내려가면 보유 물약을 자동으로 마신다 (쿨타임은 PotionService가 관리).
        /// 죽음 처리(전투불능 리셋)보다 먼저 개입해야 의미가 있으므로 Update 초입에서 돈다.
        /// </summary>
        void TickAutoPotion()
        {
            if (!IdleMvp.Core.PotionService.AutoUse) return;
            if (_heroMaxHp <= 0f || _heroHp <= 0f) return;
            if (_heroHp > _heroMaxHp * IdleMvp.Core.PotionService.AutoThreshold) return;
            if (!IdleMvp.Core.PotionService.TryUse()) return;
            float healed = HealHero(IdleMvp.Core.PotionService.HealPct);
            if (FieldCombatFx.Instance != null && _hero != null)
            {
                var pos = _hero.FieldAnchor;
                FieldCombatFx.Instance.PlayAoeBlast(pos, 90f, new Color(0.35f, 1f, 0.5f, 0.6f));
                FieldCombatFx.Instance.PopDamage(pos + new Vector2(0f, 110f), healed, false);
            }
            IdleMvp.Core.AudioService.Gem();
            OnChanged?.Invoke();
        }

        public bool HeroNeedsHeal => _heroMaxHp > 0f && _heroHp < _heroMaxHp * 0.98f;

        /// <summary>Manual skill button: CD check, FX, burst damage on nearest foe.</summary>
        public string TryCastSkill(int skillId)
        {
            var sk = SkillAdapter.Instance;
            if (sk == null) return "스킬 시스템 없음";
            if (!sk.TryBeginManualCast(skillId, out string err))
                return err;

            var target = FindNearestMob();
            if (target == null || !target.Alive)
            {
                sk.RefundManualCast(skillId);
                return "대상 없음";
            }

            if (_hero != null)
            {
                _hero.Face(target.X);
                _hero.PlayAttack(Mathf.Sign(target.X - _hero.X));
            }

            IdleMvp.Core.AudioService.Skill(skillId);
            float mul = 1.8f + sk.DamageBonus[Mathf.Clamp(skillId, 0, 3)];
            float dmg = Mathf.Max(1f,
                CombatPowerService.GetAtk() * 2.8f * mul * CombatPowerService.GetOutgoingMul());
            dmg = CombatPowerService.MitigateByDef(dmg, StageTable.Get(ActiveStageIndex()));
            target.TakeDamage(dmg);
            if (FieldCombatFx.Instance != null)
            {
                FieldCombatFx.Instance.PopDamage(
                    target.FieldAnchor + new Vector2(0f, 92f), dmg, true);
                ExecuteSkillMechanic(skillId, dmg, target);
            }
            if (!target.Alive)
                OnMobKilled(target);
            RefreshFocusHp();
            CountAlive();
            OnChanged?.Invoke();
            return SkillTreeDef.Nodes[Mathf.Clamp(skillId, 0, 7)].Name + " 시전!";
        }

        public bool TryStartWorldBoss()
        {
            if (!RaidService.Instance.TryEnterWorldBoss(this, out var reason))
            {
                LastMessage = reason;
                StatusText = reason;
                OnChanged?.Invoke();
                return false;
            }

            ClearMobs();
            Mode = CombatMode.WorldBoss;
            Blocked = false;
            IsBossFight = true;
            IsMiniBossFight = false;
            BossTimeLeft = 90f;
            ResetHeroHp();
            EnsureHero();
            if (_hero != null)
                _hero.SetX(-_mapHalfW * 0.55f, -_mapHalfW, _mapHalfW);
            SyncCompanionActors(force: true);

            var row = StageTable.Get(StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1);
            float rec = (row?.recommendedCp ?? 100f) * 1.2f;
            var raidRow = new StageRow
            {
                index = row?.index ?? 1,
                enemyHp = RaidService.Instance != null ? RaidService.Instance.BossHp : 50000f,
                // 월드보스는 스테이지 기준 1.5배 + 레이드 난이도 배수 (전엔 난이도가 HP에만 붙었다)
                enemyDef = (row?.enemyDef ?? 0f) * 1.5f,
                enemyAtk = (row?.enemyAtk ?? 20f) * 1.5f
                    * (RaidService.Instance != null
                        ? RaidService.AtkMulOf(RaidService.Instance.Difficulty) : 1f),
                recommendedCp = rec,
                minCp = rec * BalanceConfig.Data.clearCpRatioMin,
                softCp = rec * BalanceConfig.Data.clearCpRatioMax,
                boss = true,
                bossTimeLimit = 90f,
                mobHpMul = 1f
            };
            StatusText = $"월드보스 ({BossTimeLeft:0}초)";
            _bossHeavyTimer = 6f;
            _bossSlamTimer = 12f;
            _bossEnraged = false;
            SpawnBoss(raidRow);
            RefreshFocusHp();
            OnChanged?.Invoke();
            return true;
        }
    }
}
