using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Progression;
using System;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>
    /// Stage-scaled AFK loot box with offline cap (mushroom-style accrual).
    /// </summary>
    public class LootBoxService : MonoBehaviour
    {
        public static LootBoxService Instance { get; private set; }

        public double PendingGold { get; private set; }
        public double PendingXp { get; private set; }
        public double PendingEnhanceStone { get; private set; }
        /// <summary>Seconds of accrual currently represented by pending rewards (capped).</summary>
        public float PendingAccruedSeconds { get; private set; }
        public DateTime LastClaimUtc { get; private set; }

        public float PendingHours => PendingAccruedSeconds / 3600f;

        public event Action OnChanged;

        const string PrefKey = "IdleMvp.LootBox";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
            AccrueOffline();
        }

        void Update()
        {
            // Light online accrual while playing (~1s ticks via unscaled time accumulation)
            _accum += Time.unscaledDeltaTime;
            if (_accum >= 1f)
            {
                float sec = _accum;
                _accum = 0f;
                AccrueSeconds(sec);
            }
        }

        float _accum;

        public float GoldPerMinute
        {
            get
            {
                int stage = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
                return 10f + stage * 2.5f;
            }
        }

        public float XpPerMinute
        {
            get
            {
                int stage = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
                return 5f + stage * 1.2f;
            }
        }

        public float StonePerMinute
        {
            get
            {
                int stage = StageProgress.Instance != null ? StageProgress.Instance.StageIndex : 1;
                return 0.5f + stage * 0.08f;
            }
        }

        public float CapHours
        {
            get
            {
                if (MembershipService.Instance != null)
                    return MembershipService.Instance.CapHours;
                return BalanceConfig.Data.offlineCapHours;
            }
        }

        public void AccrueOffline()
        {
            var now = DateTime.UtcNow;
            double seconds = (now - LastClaimUtc).TotalSeconds;
            if (seconds < 0) seconds = 0;
            double cap = CapHours * 3600.0;
            if (seconds > cap) seconds = cap;
            AccrueSeconds((float)seconds);
            // LastClaimUtc stays until Claim; accrual is additive on pending
            // Reset baseline so we don't double-count on next AccrueOffline
            LastClaimUtc = now;
            Save();
            OnChanged?.Invoke();
        }

        public void AccrueSeconds(float seconds)
        {
            if (seconds <= 0) return;
            float mul = MembershipService.Instance != null ? MembershipService.Instance.AccrueMul : 1f;
            if (SkillAdapter.Instance != null)
                mul += SkillAdapter.Instance.PassiveIdlePct * 0.01f;
            if (PlayerGrowth.Instance != null)
                mul += PlayerGrowth.Instance.SpecIdlePct * 0.01f;
            if (IdleMvp.Progression.ArtifactService.Instance != null)
                mul += IdleMvp.Progression.ArtifactService.Instance.IdlePctBonus * 0.01f;

            float capSec = CapHours * 3600f;
            float room = Mathf.Max(0f, capSec - PendingAccruedSeconds);
            float apply = Mathf.Min(seconds, room);
            if (apply <= 0f) return;

            PendingAccruedSeconds += apply;
            PendingGold += GoldPerMinute * (apply / 60f) * mul;
            PendingXp += XpPerMinute * (apply / 60f) * mul;
            PendingEnhanceStone += StonePerMinute * (apply / 60f) * mul;
            Save();
            OnChanged?.Invoke();
        }

        /// <summary>Chapter hunt kill → small AFK-box drip (online farm feeds the box too).</summary>
        public void NotifyHuntKill()
        {
            AccrueSeconds(2.5f);
        }

        public (double gold, double xp, double stone) Claim(float rewardMul = 1f)
        {
            rewardMul = Mathf.Max(0.1f, rewardMul);
            var g = PendingGold * rewardMul;
            var x = PendingXp * rewardMul;
            var s = PendingEnhanceStone * rewardMul;
            PendingGold = 0;
            PendingXp = 0;
            PendingEnhanceStone = 0;
            PendingAccruedSeconds = 0f;
            LastClaimUtc = DateTime.UtcNow;

            if (PlayerGrowth.Instance != null && x > 0)
                PlayerGrowth.Instance.AddXp(Mathf.FloorToInt((float)x));

            if (PlayerWallet.Instance != null && g > 0)
                PlayerWallet.Instance.AddGold(g);
            else
                TryAddTemplateGold(g);

            if (EquipmentService.Instance != null && s > 0)
                EquipmentService.Instance.AddEnhanceStones(s);

            Save();
            OnChanged?.Invoke();
            return (g, x, s);
        }

        /// <summary>1.5× claim costs Blue Diamond (fallback Red Diamond).</summary>
        public string ClaimBonus(float mul, int blueCost)
        {
            if (PendingGold + PendingXp + PendingEnhanceStone <= 0)
                return "수령할 보상이 없습니다";
            bool paid = false;
            if (CurrencyWallet.Instance != null &&
                CurrencyWallet.Instance.TrySpend(CurrencyId.BlueDiamond, blueCost))
                paid = true;
            else if (WalletAdapter.Instance != null &&
                     WalletAdapter.Instance.TrySpendRedDiamond(blueCost))
                paid = true;
            if (!paid) return $"블루다이아 {blueCost} 필요";
            var got = Claim(mul);
            return $"×{mul:0.#} 수령 · 골드 {got.gold:0} · XP {got.xp:0} · 강화석 {got.stone:0.#}";
        }

        void TryAddTemplateGold(double gold)
        {
            if (gold <= 0) return;
            var wallets = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in wallets)
            {
                if (mb == null) continue;
                var t = mb.GetType();
                if (t.Name != "WalletManagerScript") continue;
                var field = t.GetField("GoldWalletValue");
                if (field != null && field.FieldType == typeof(float))
                {
                    float cur = (float)field.GetValue(mb);
                    field.SetValue(mb, cur + (float)gold);
                    return;
                }
            }
        }

        void Save()
        {
            var json = JsonUtility.ToJson(new SaveData
            {
                gold = PendingGold,
                xp = PendingXp,
                stone = PendingEnhanceStone,
                accruedSec = PendingAccruedSeconds,
                lastClaimTicks = LastClaimUtc.Ticks
            });
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();
            SaveSnapshotService.RegisterKey(PrefKey);
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey))
            {
                LastClaimUtc = DateTime.UtcNow;
                return;
            }
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefKey));
            if (data == null)
            {
                LastClaimUtc = DateTime.UtcNow;
                return;
            }
            PendingGold = data.gold;
            PendingXp = data.xp;
            PendingEnhanceStone = data.stone;
            PendingAccruedSeconds = Mathf.Max(0f, data.accruedSec);
            LastClaimUtc = data.lastClaimTicks > 0
                ? new DateTime(data.lastClaimTicks, DateTimeKind.Utc)
                : DateTime.UtcNow;
        }

        [Serializable]
        class SaveData
        {
            public double gold, xp, stone;
            public float accruedSec;
            public long lastClaimTicks;
        }
    }
}
