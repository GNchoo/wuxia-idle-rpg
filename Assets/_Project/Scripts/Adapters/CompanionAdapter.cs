using System;
using System.Collections.Generic;
using IdleMvp.Core;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Adapters
{
    [Serializable]
    public class CompanionItem
    {
        public string id;
        public string name;
        public int rarity;
        public int level = 1;
        public int count = 1;
        public bool main;
        public bool sub;
        public int awaken;
        public int levelCap = 12;
        public bool locked;
        /// <summary>dps / tank / support</summary>
        public string role = "dps";
        public string catalogId;
    }

    public class CompanionSummonResult
    {
        public bool Ok;
        public string Error;
        public string Id;
        public string Name;
        public int Rarity;
        public int Level;
        public bool IsNew;
        public bool LeveledUp;
        public string Message;
    }

    /// <summary>
    /// Mirrors SubHeroesManager + HeroChest gacha. Syncs counts when template SubHero is bound.
    /// </summary>
    public class CompanionAdapter : MonoBehaviour
    {
        public static CompanionAdapter Instance { get; private set; }

        public List<CompanionItem> Owned { get; private set; } = new List<CompanionItem>();
        public int SummonLevel { get; private set; } = 1;
        public int TotalSummons { get; private set; }
        public int PityEpic { get; private set; }
        public int PityLegendary { get; private set; }
        public bool AutoSummon = true;
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Companions";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        public int OwnedCount => Owned.Count;

        public float CompanionCp
        {
            get
            {
                float cp = 0f;
                foreach (var c in Owned)
                {
                    float baseCp = (12f + c.level * 8f) * (1f + c.rarity * 0.45f) * (1f + c.awaken * 0.12f);
                    if (c.main) cp += baseCp;
                    if (c.sub) cp += baseCp * 0.55f;
                    cp += baseCp * 0.15f; // hold
                }
                var host = TemplateFeatureHost.Instance;
                if (host != null && host.HasTemplateSubHero)
                    cp += TemplateFeatureHost.ReadFloatField(host.WalletManager, "SubHeroTotalDPS") * 0.01f;
                return cp;
            }
        }

        public CompanionItem Main
        {
            get
            {
                foreach (var c in Owned)
                    if (c.main) return c;
                return null;
            }
        }

        public int MaxSubSlots => Mathf.Clamp(1 + SummonLevel / 2, 1, 5);

        public int SubCount
        {
            get
            {
                int n = 0;
                foreach (var c in Owned) if (c.sub) n++;
                return n;
            }
        }

        public List<CompanionItem> GetSubs()
        {
            var list = new List<CompanionItem>();
            foreach (var c in Owned)
                if (c.sub) list.Add(c);
            list.Sort((a, b) => Score(b).CompareTo(Score(a)));
            return list;
        }

        public static float Score(CompanionItem c)
        {
            if (c == null) return -1f;
            float s = c.rarity * 10000f + c.awaken * 1000f + c.level * 10f + c.count;
            if (c.locked) s += 500000f;
            if (c.sub) s += 800000f;
            if (c.main) s += 1000000f;
            return s;
        }

        /// <summary>Display order: main → sub → locked → rarity/awaken/level.</summary>
        public List<CompanionItem> GetSortedOwned(int minRarity = 0, bool deployedOnly = false)
        {
            var list = new List<CompanionItem>(Owned.Count);
            foreach (var c in Owned)
            {
                if (c == null) continue;
                if (deployedOnly && !c.main && !c.sub) continue;
                if (c.rarity < minRarity) continue;
                list.Add(c);
            }
            list.Sort((a, b) => Score(b).CompareTo(Score(a)));
            return list;
        }

        public void ToggleLock(string id)
        {
            var c = Owned.Find(x => x.id == id);
            if (c == null) return;
            c.locked = !c.locked;
            Save();
            OnChanged?.Invoke();
        }

        /// <summary>Best as main, next best fill sub slots up to MaxSubSlots.</summary>
        public string DeployBest()
        {
            if (Owned == null || Owned.Count == 0) return "보유 동료 없음";
            var ranked = new List<CompanionItem>(Owned);
            ranked.Sort((a, b) =>
            {
                float sa = a.rarity * 10000f + a.awaken * 1000f + a.level * 10f + a.count;
                float sb = b.rarity * 10000f + b.awaken * 1000f + b.level * 10f + b.count;
                return sb.CompareTo(sa);
            });

            foreach (var c in Owned)
            {
                c.main = false;
                c.sub = false;
            }

            ranked[0].main = true;
            int maxSub = MaxSubSlots;
            int subs = 0;
            for (int i = 1; i < ranked.Count && subs < maxSub; i++)
            {
                ranked[i].sub = true;
                subs++;
            }

            Save();
            OnChanged?.Invoke();
            IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
            return $"자동배치 · 메인 {ranked[0].name} · 서브 {subs}/{maxSub}";
        }

        public CompanionSummonResult TrySummonOne()
        {
            bool paid = false;
            if (CurrencyWallet.Instance != null && CurrencyWallet.Instance.TrySpend(CurrencyId.CompanionTicket, 1))
                paid = true;
            else if (WalletAdapter.Instance != null && WalletAdapter.Instance.TrySpendRedDiamond(50))
                paid = true;
            if (!paid)
            {
                return new CompanionSummonResult
                {
                    Ok = false,
                    Error = "동료 소환권 또는 레드다이아 50 필요",
                    Message = "동료 소환권 또는 레드다이아 50 필요"
                };
            }

            int pe = PityEpic;
            int pl = PityLegendary;
            var rarity = GachaRoll.RollWithPity(ref pe, ref pl);
            PityEpic = pe;
            PityLegendary = pl;
            var def = ContentCatalog.PickCompanion();
            string name = def != null ? def.name : "전사 동료";
            string role = def != null ? def.role : "dps";
            string catalogId = def != null ? def.id : "";
            bool isNew = false;
            bool leveledUp = false;
            int level = 1;
            var existing = Owned.Find(c =>
                (!string.IsNullOrEmpty(catalogId) && c.catalogId == catalogId && c.rarity == (int)rarity)
                || (c.name == name && c.rarity == (int)rarity));
            string resultId = null;
            if (existing != null)
            {
                existing.count++;
                if (string.IsNullOrEmpty(existing.role)) existing.role = role;
                if (string.IsNullOrEmpty(existing.catalogId)) existing.catalogId = catalogId;
                if (existing.count >= 2 && existing.level < Mathf.Max(12, existing.levelCap))
                {
                    existing.count--;
                    existing.level++;
                    leveledUp = true;
                }
                level = existing.level;
                resultId = existing.id;
            }
            else
            {
                isNew = true;
                var item = new CompanionItem
                {
                    id = Guid.NewGuid().ToString("N"),
                    name = name,
                    rarity = (int)rarity,
                    level = 1,
                    count = 1,
                    main = Owned.Count == 0,
                    role = role,
                    catalogId = catalogId
                };
                Owned.Add(item);
                level = 1;
                resultId = item.id;
            }

            TotalSummons++;
            if (TotalSummons % 15 == 0) SummonLevel++;
            TryGrantTemplateSubHero();
            IdleMvp.Core.DailyMissionService.Increment("summon");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Summon);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Summon);
            if (!string.IsNullOrEmpty(catalogId))
                IdleMvp.Core.CollectionService.TryCollect("c_" + catalogId);
            PassService.Instance?.NotifyCompanionSummon();
            // 10연차에서는 마지막에 한 번만. 매번 하면 디스크 저장·UI 전체 재빌드·동료 액터
            // 재소환이 10배로 겹쳐 몇 초씩 멈춘다.
            if (!_batching)
            {
                Save();
                OnChanged?.Invoke();
                if (Owned.Exists(x => x.main))
                    IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
            }

            string tag = isNew ? "신규" : leveledUp ? "중복→레벨업" : "중복";
            string roleTag = string.IsNullOrEmpty(role) ? "" : $"[{role}] ";
            return new CompanionSummonResult
            {
                Ok = true,
                Id = resultId,
                Name = name,
                Rarity = (int)rarity,
                Level = level,
                IsNew = isNew,
                LeveledUp = leveledUp,
                Message = $"동료 소환: {rarity} {roleTag}{name} ({tag})"
            };
        }

        public string SummonOne()
        {
            var r = TrySummonOne();
            return r.Message;
        }

        bool _batching;

        public List<CompanionSummonResult> TrySummonTen()
        {
            var list = new List<CompanionSummonResult>(10);
            _batching = true;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var r = TrySummonOne();
                    list.Add(r);
                    if (!r.Ok) break;
                }
            }
            finally
            {
                _batching = false;
                Save();
                OnChanged?.Invoke();
                if (Owned.Exists(x => x.main))
                    IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
            }
            return list;
        }

        public string SummonTen()
        {
            var results = TrySummonTen();
            if (results.Count == 0) return "소환 실패";
            if (!results[0].Ok) return results[0].Message;
            var sb = "";
            foreach (var r in results)
            {
                if (!r.Ok)
                {
                    sb += "(재화 부족으로 중단)";
                    break;
                }
                sb += r.Message + "\n";
            }
            return sb.Trim();
        }

        public void SetMain(string id)
        {
            foreach (var c in Owned) c.main = c.id == id;
            Save();
            OnChanged?.Invoke();
            IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
        }

        /// <summary>Assign as sub companion. Returns true if now sub, false if cleared.</summary>
        public bool SetSub(string id)
        {
            int maxSub = MaxSubSlots;
            var c = Owned.Find(x => x.id == id);
            if (c == null) return false;
            if (c.sub)
            {
                c.sub = false;
                Save();
                OnChanged?.Invoke();
                IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
                return false;
            }
            int n = 0;
            foreach (var o in Owned) if (o.sub) n++;
            if (n >= maxSub)
            {
                foreach (var o in Owned)
                {
                    if (!o.sub) continue;
                    o.sub = false;
                    break;
                }
            }
            c.sub = true;
            Save();
            OnChanged?.Invoke();
            IdleMvp.Combat.FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
            return true;
        }

        public void ToggleSub(string id) => SetSub(id);

        public float PassiveAtkPct
        {
            get
            {
                float p = 0f;
                float tankSynergy = 0f;
                float supportSynergy = 0f;
                foreach (var c in Owned)
                {
                    if (!c.main && !c.sub) continue;
                    p += c.rarity * 0.8f + c.level * 0.15f + c.awaken * 1.5f;
                    var def = !string.IsNullOrEmpty(c.catalogId)
                        ? ContentCatalog.GetCompanion(c.catalogId)
                        : ContentCatalog.GetCompanionByName(c.name);
                    if (def != null)
                    {
                        float scale = c.main ? 1f : 0.55f;
                        p += def.passiveAtkPct * scale;
                        if (def.role == "tank") tankSynergy += 1f;
                        if (def.role == "support") supportSynergy += 1f;
                    }
                    else if (c.role == "tank") tankSynergy += 1f;
                    else if (c.role == "support") supportSynergy += 1f;
                }
                // Role synergy: tank+support together slightly boosts ATK inherit
                if (tankSynergy > 0f && supportSynergy > 0f) p += 1.5f;
                return p;
            }
        }

        public float PassiveHpPct
        {
            get
            {
                float p = 0f;
                foreach (var c in Owned)
                {
                    if (!c.main && !c.sub) continue;
                    var def = !string.IsNullOrEmpty(c.catalogId)
                        ? ContentCatalog.GetCompanion(c.catalogId)
                        : ContentCatalog.GetCompanionByName(c.name);
                    if (def != null) p += def.passiveHpPct * (c.main ? 1f : 0.55f);
                }
                return p;
            }
        }

        public float PassiveGoldPct
        {
            get
            {
                float p = 0f;
                foreach (var c in Owned)
                {
                    if (!c.main && !c.sub) continue;
                    var def = !string.IsNullOrEmpty(c.catalogId)
                        ? ContentCatalog.GetCompanion(c.catalogId)
                        : ContentCatalog.GetCompanionByName(c.name);
                    if (def != null) p += def.passiveGoldPct * (c.main ? 1f : 0.55f);
                }
                return p;
            }
        }

        public string TryAwaken(string id)
        {
            var c = Owned.Find(x => x.id == id);
            if (c == null) return "동료 없음";
            if (c.count < 3) return "동일 동료 3중첩 필요";
            if (c.awaken >= 3) return "각성 MAX";
            c.count -= 3;
            if (c.count < 1) c.count = 1;
            c.awaken++;
            c.levelCap = 12 + c.awaken * 4;
            if (c.level > c.levelCap) c.level = c.levelCap;
            Save();
            OnChanged?.Invoke();
            return $"{c.name} 각성 {c.awaken} · 레벨캡 {c.levelCap}";
        }

        public float MainInheritRatio
        {
            get
            {
                var m = Main;
                if (m == null) return 0f;
                return 0.25f + m.level * 0.04f + m.rarity * 0.05f + m.awaken * 0.03f;
            }
        }

        public float SubInheritRatio
        {
            get
            {
                float r = 0f;
                foreach (var c in Owned)
                {
                    if (!c.sub) continue;
                    r += (0.15f + c.level * 0.02f + c.rarity * 0.03f) * 0.55f;
                }
                return r;
            }
        }

        void TryGrantTemplateSubHero()
        {
            var host = TemplateFeatureHost.Instance;
            if (host == null || !host.HasTemplateSubHero) return;
            try
            {
                var active = host.SubHeroManager.GetType().GetField("SubHeroActive");
                var counts = host.SubHeroManager.GetType().GetField("SubHeroItemCount");
                if (active?.GetValue(host.SubHeroManager) is List<bool> act &&
                    counts?.GetValue(host.SubHeroManager) is List<float> cnt && act.Count > 0)
                {
                    int i = UnityEngine.Random.Range(0, act.Count);
                    act[i] = true;
                    if (i < cnt.Count) cnt[i] += 1;
                }
            }
            catch { /* template layout variance */ }
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new Wrap
            {
                items = Owned.ToArray(),
                summonLevel = SummonLevel,
                total = TotalSummons,
                auto = AutoSummon,
                pityEpic = PityEpic,
                pityLegendary = PityLegendary
            }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.items != null) Owned = new List<CompanionItem>(w.items);
            SummonLevel = Mathf.Max(1, w?.summonLevel ?? 1);
            TotalSummons = w?.total ?? 0;
            AutoSummon = w?.auto ?? true;
            PityEpic = w?.pityEpic ?? 0;
            PityLegendary = w?.pityLegendary ?? 0;
            foreach (var c in Owned)
            {
                if (c == null) continue;

                // 옛 세이브는 catalogId가 비어 있다 → 옛 이름('전사 동료')으로 되찾는다.
                // 예전엔 role이 비어 있을 때만 이 블록을 타서, 이미 role이 있는 동료는
                // 무협 리네이밍 전 이름이 그대로 화면에 남았다.
                if (string.IsNullOrEmpty(c.catalogId) && !string.IsNullOrEmpty(c.name))
                {
                    string legacy;
                    if (LegacyCompanionNames.TryGetValue(c.name, out legacy)) c.catalogId = legacy;
                    else
                    {
                        var byName = ContentCatalog.GetCompanionByName(c.name);
                        if (byName != null) c.catalogId = byName.id;
                    }
                }

                var def = !string.IsNullOrEmpty(c.catalogId)
                    ? ContentCatalog.GetCompanion(c.catalogId)
                    : null;
                if (def != null)
                {
                    c.role = def.role;
                    c.name = def.name;   // 리네이밍을 기존 세이브에도 반영
                }
                else if (string.IsNullOrEmpty(c.role)) c.role = "dps";
            }
        }

        /// <summary>무협 리네이밍 이전 이름 → companions.json id.</summary>
        static readonly Dictionary<string, string> LegacyCompanionNames = new Dictionary<string, string>
        {
            { "전사 동료", "c_warrior" }, { "마법사 동료", "c_mage" },
            { "궁수 동료", "c_archer" }, { "도적 동료", "c_thief" },
            { "성기사 동료", "c_paladin" }, { "암흑기사 동료", "c_dk" },
            { "불독 동료", "c_fp" }, { "썬콜 동료", "c_il" },
            { "프리스트 동료", "c_priest" }, { "레인저 동료", "c_ranger" },
            { "버서커 동료", "c_berserker" }, { "샤먼 동료", "c_shaman" },
            { "닌자 동료", "c_ninja" }, { "가디언 동료", "c_guardian" },
            { "연금술사 동료", "c_alchemist" }, { "용족 동료", "c_dragonkin" },
        };

        [Serializable]
        class Wrap
        {
            public CompanionItem[] items;
            public int summonLevel;
            public int total;
            public bool auto;
            public int pityEpic;
            public int pityLegendary;
        }
    }
}
