using System;
using IdleMvp.Adapters;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Progression
{
    public enum MapleEquipSlot
    {
        Weapon, Head, Chest, Accessory, Legs, Shoes,
        Cape, Shoulder, Belt, Ring, Necklace
    }

    [Serializable]
    public class SlotEnhanceState
    {
        public int scrollTriesUsed;
        public int scrollSuccess;
        public float scrollAtkPct;
        public int starForce;
        public int potentialRank; // 0..5
        public float potentialAtkPct;
        public float potentialFinalPct;
        // 에디셔널 잠재 — 스타포스 12성 이상에서 해금 (벤치마크 구조)
        public int addRank; // 0..5
        public float addAtkPct;
        public float addFinalPct;
    }

    /// <summary>
    /// Maple-only gap: scroll / starforce / potential on slots (kept across gear swaps).
    /// </summary>
    public class SlotEnhanceService : MonoBehaviour
    {
        public static SlotEnhanceService Instance { get; private set; }

        public SlotEnhanceState[] States { get; private set; }
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.SlotEnhance";
        /// <summary>Canonical player slots (matches InventoryAdapter).</summary>
        public const int SlotCount = 6;
        public const int LegacySlotCount = 11;

        public static string SlotLabel(int i)
        {
            string[] names = { "무기", "투구", "갑옷", "장신구", "하의", "신발" };
            if (i < 0 || i >= names.Length) return "잠금";
            return names[i];
        }

        public bool IsSlotActive(int slot) => slot >= 0 && slot < SlotCount;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            States = new SlotEnhanceState[SlotCount];
            for (int i = 0; i < SlotCount; i++) States[i] = new SlotEnhanceState();
            Load();
        }

        public float BonusCp
        {
            get
            {
                float cp = 0f;
                for (int i = 0; i < SlotCount && i < States.Length; i++)
                {
                    var s = States[i];
                    cp += s.scrollSuccess * 8f + s.scrollAtkPct * 40f;
                    cp += s.starForce * 12f;
                    cp += s.potentialRank * 25f + s.potentialAtkPct * 50f + s.potentialFinalPct * 80f;
                    cp += s.addRank * 25f + s.addAtkPct * 50f + s.addFinalPct * 80f;
                }
                return cp;
            }
        }

        public float DamageMultiplier
        {
            get
            {
                float mul = 1f;
                for (int i = 0; i < SlotCount && i < States.Length; i++)
                {
                    var s = States[i];
                    mul += s.scrollAtkPct * 0.01f;
                    mul += s.starForce * 0.008f;
                    mul += s.potentialAtkPct * 0.01f;
                    mul += s.potentialFinalPct * 0.01f;
                    mul += s.addAtkPct * 0.01f;
                    mul += s.addFinalPct * 0.01f;
                }
                return mul;
            }
        }

        public bool HasAffordableEnhance
        {
            get
            {
                var cw = Economy.CurrencyWallet.Instance;
                if (cw == null) return false;
                return cw.Get(Economy.CurrencyId.ScrollTrace) >= 1
                    || cw.Get(Economy.CurrencyId.StarForceScroll) >= 1
                    || cw.Get(Economy.CurrencyId.MiracleCube) >= 1
                    || cw.Get(Economy.CurrencyId.AdditionalCube) >= 1;
            }
        }

        public string TryScroll(int slot)
        {
            if (!IsSlotActive(slot)) return "잠긴 슬롯";
            slot = Mathf.Clamp(slot, 0, SlotCount - 1);
            var s = States[slot];
            if (s.scrollTriesUsed >= 10) return "주문서 시도 횟수 소진";
            if (CurrencyWallet.Instance == null || !CurrencyWallet.Instance.TrySpend(CurrencyId.ScrollTrace, 1))
                return "주문의 흔적 부족";

            s.scrollTriesUsed++;
            IdleMvp.Core.DailyMissionService.Increment("enhance");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Enhance);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Enhance);
            float chance = 0.7f;
            if (s.scrollTriesUsed == 5) chance += 0.1f;
            if (s.scrollTriesUsed == 10) chance += 0.2f;
            if (UnityEngine.Random.value <= chance)
            {
                s.scrollSuccess++;
                s.scrollAtkPct += 1.5f;
                Save();
                OnChanged?.Invoke();
                return $"주문서 성공! ({s.scrollSuccess}/10) 공격+{s.scrollAtkPct:0.#}%";
            }
            Save();
            OnChanged?.Invoke();
            return "주문서 실패";
        }

        public string TryStarForce(int slot)
        {
            if (!IsSlotActive(slot)) return "잠긴 슬롯";
            slot = Mathf.Clamp(slot, 0, SlotCount - 1);
            var s = States[slot];
            if (CurrencyWallet.Instance == null || !CurrencyWallet.Instance.TrySpend(CurrencyId.StarForceScroll, 1))
                return "스타포스 주문서 부족";

            IdleMvp.Core.DailyMissionService.Increment("enhance");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Enhance);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Enhance);
            int sf = s.starForce;
            float chance = sf < 10 ? 0.7f - sf * 0.04f : 0.3f;
            float roll = UnityEngine.Random.value;
            if (roll <= chance)
            {
                s.starForce++;
                Save();
                OnChanged?.Invoke();
                return $"스타포스 성공 → {s.starForce}성";
            }
            // 파괴 문턱은 벤치마크와 동일한 20성↑ (전엔 15성이라 체감이 너무 가혹했다)
            if (sf >= 20 && roll > 0.95f)
            {
                s.starForce = 0;
                Save();
                OnChanged?.Invoke();
                return "스타포스 파괴! 0성으로 초기화 (장비는 유지)";
            }
            if (sf >= 13)
            {
                s.starForce = Mathf.Max(0, s.starForce - 1);
                Save();
                OnChanged?.Invoke();
                return $"스타포스 하락 → {s.starForce}성";
            }
            Save();
            OnChanged?.Invoke();
            return "스타포스 실패 (유지)";
        }

        public string TryPotential(int slot)
        {
            if (!IsSlotActive(slot)) return "잠긴 슬롯";
            slot = Mathf.Clamp(slot, 0, SlotCount - 1);
            var s = States[slot];
            if (s.starForce < 3) return "스타포스 3성 이상 필요";
            if (CurrencyWallet.Instance == null || !CurrencyWallet.Instance.TrySpend(CurrencyId.MiracleCube, 1))
                return "미라클 큐브 부족";

            IdleMvp.Core.DailyMissionService.Increment("enhance");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Enhance);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Enhance);
            if (s.potentialRank < 5 && UnityEngine.Random.value < RankUpChance(s.potentialRank))
                s.potentialRank++;
            s.potentialAtkPct = 2f + s.potentialRank * 1.5f + UnityEngine.Random.Range(0f, 2f);
            s.potentialFinalPct = s.potentialRank >= 2 ? 1f + s.potentialRank : 0f;
            Save();
            OnChanged?.Invoke();
            return $"잠재 리롤 [{RankName(s.potentialRank)}] 공격{s.potentialAtkPct:0.#}% 최종{s.potentialFinalPct:0.#}%";
        }

        // 등급업 확률 — 벤치마크 카피 (노말→레어 6% … 레전드리→미스틱 0.21%)
        static readonly float[] RankUpProb = { 0.06f, 0.033f, 0.0167f, 0.006f, 0.0021f };
        public static float RankUpChance(int rank) =>
            rank >= 0 && rank < RankUpProb.Length ? RankUpProb[rank] : 0f;
        static readonly string[] RankNames = { "범품", "양품", "일품", "절품", "신품", "무극" };
        public static string RankName(int rank) =>
            RankNames[Mathf.Clamp(rank, 0, RankNames.Length - 1)];

        /// <summary>에디셔널 잠재 — 스타포스 12성 이상 해금, 애디셔널 큐브 소모 (벤치마크 구조).</summary>
        public string TryAdditional(int slot)
        {
            if (!IsSlotActive(slot)) return "잠긴 슬롯";
            slot = Mathf.Clamp(slot, 0, SlotCount - 1);
            var s = States[slot];
            if (s.starForce < 12) return "스타포스 12성 이상 필요";
            if (CurrencyWallet.Instance == null || !CurrencyWallet.Instance.TrySpend(CurrencyId.AdditionalCube, 1))
                return "애디셔널 큐브 부족 (사냥 중 드랍)";

            IdleMvp.Core.DailyMissionService.Increment("enhance");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Enhance);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Enhance);
            if (s.addRank < 5 && UnityEngine.Random.value < RankUpChance(s.addRank))
                s.addRank++;
            s.addAtkPct = 1f + s.addRank * 1f + UnityEngine.Random.Range(0f, 1.5f);
            s.addFinalPct = s.addRank >= 2 ? 0.5f + s.addRank * 0.5f : 0f;
            Save();
            OnChanged?.Invoke();
            return $"에디셔널 리롤 [{RankName(s.addRank)}] 공격{s.addAtkPct:0.#}% 최종{s.addFinalPct:0.#}%";
        }

        public string RecoverStarForce(int slot)
        {
            slot = Mathf.Clamp(slot, 0, SlotCount - 1);
            var s = States[slot];
            if (s.starForce >= 12) return "이미 12성 이상";
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendRedDiamond(5000))
                return "레드다이아 5000 필요";
            if (WalletAdapter.Instance == null || !WalletAdapter.Instance.TrySpendGold(1000000))
            {
                WalletAdapter.Instance?.AddRedDiamond(5000);
                return "골드 100만 필요";
            }
            s.starForce = 12;
            Save();
            OnChanged?.Invoke();
            return "12성 복구 완료";
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new Wrap { states = States }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.states == null) return;
            // Migrate legacy 11 → 6 (keep first 6)
            if (w.states.Length == SlotCount)
                States = w.states;
            else if (w.states.Length >= SlotCount)
            {
                for (int i = 0; i < SlotCount; i++)
                    States[i] = w.states[i] ?? new SlotEnhanceState();
            }
            else
            {
                for (int i = 0; i < w.states.Length && i < SlotCount; i++)
                    States[i] = w.states[i] ?? new SlotEnhanceState();
            }
        }

        [Serializable]
        class Wrap
        {
            public SlotEnhanceState[] states;
        }
    }
}
