using System;
using System.Collections.Generic;
using IdleMvp.Core;
using IdleMvp.Economy;
using UnityEngine;
using PassService = IdleMvp.Economy.PassService;

namespace IdleMvp.Adapters
{
    [Serializable]
    public class WeaponItem
    {
        public string id;
        public string name;
        public int rarity;
        public int level = 1;
        public int awaken;
        public int count = 1;
        public bool equipped;
        public bool locked;
        /// <summary>0=sword 1=staff 2=bow 3=claw.</summary>
        public int kind;
        public string catalogId;
        public float baseAtk;

        /// <summary>
        /// 이름으로 무기 종류 추정: 0=검(양날) 1=도(외날) 2=창·곤 3=기병(奇兵).
        /// catalogId가 없는 구버전 세이브 전용 폴백이다.
        /// 무협 고증 개편으로 예전 서양식 이름(지팡이/활/아대)은 더 이상 나오지 않지만,
        /// 옛 세이브가 남아 있을 수 있어 그 키워드도 계속 받아준다.
        /// </summary>
        static readonly string[][] KindKeywords =
        {
            null,                                                  // 0 검 = 기본값
            new[] { "도", "刀", "유엽", "안령", "우미", "박도", "묘도", "환도", "귀두" },
            new[] { "곤", "창", "봉", "槍", "棍", "월아산", "편곤", "언월" },
            new[] { "판관필", "아미자", "철선", "호두구", "원앙월", "구절편", "유성추", "암기" },
            new[] { "권갑", "수갑", "호수", "권투", "장갑" },   // 4 = 권갑(주먹)
        };

        public static int KindFromName(string n)
        {
            if (string.IsNullOrEmpty(n)) return 0;

            // 구버전(서양식) 이름 폴백
            if (n.IndexOf("지팡이", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("스태프", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("마장", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("Staff", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return 2;
            if (n.IndexOf("활", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("궁", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("Bow", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return 2;
            // 옛 '아대/너클'은 이제 권갑(4)으로 본다. 주먹 무기니 그쪽이 맞다
            if (n.IndexOf("아대", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("너클", System.StringComparison.Ordinal) >= 0
                || n.IndexOf("Claw", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return 4;

            // 무협 이름 — 뒤쪽 종류부터 봐야 '박도'가 '도'로, '월아산'이 창곤으로 잡힌다
            for (int k = KindKeywords.Length - 1; k >= 1; k--)
            {
                var keys = KindKeywords[k];
                if (keys == null) continue;
                for (int i = 0; i < keys.Length; i++)
                    if (n.IndexOf(keys[i], System.StringComparison.Ordinal) >= 0) return k;
            }
            return 0;
        }
    }

    /// <summary>
    /// Weapon gacha using HeroChest rarity algorithm. Tickets or RD20.
    /// </summary>
    public class WeaponSummonAdapter : MonoBehaviour
    {
        public static WeaponSummonAdapter Instance { get; private set; }

        public List<WeaponItem> Owned { get; private set; } = new List<WeaponItem>();
        public int SummonLevel { get; private set; } = 1;
        public int TotalSummons { get; private set; }
        public int PityEpic { get; private set; }
        public int PityLegendary { get; private set; }
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Weapons";
        const float MismatchAtkMul = 0.75f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
            if (Owned.Count == 0)
            {
                Owned.Add(new WeaponItem
                {
                    id = "starter",
                    name = "초보자의 검",
                    rarity = 0,
                    level = 1,
                    equipped = true,
                    count = 1,
                    kind = 0
                });
            }
        }

        public float WeaponCp
        {
            get
            {
                float cp = 0f;
                foreach (var w in Owned)
                {
                    float hold = WeaponHoldScore(w);
                    cp += hold * 0.35f;
                    if (w.equipped) cp += hold;
                }
                return cp;
            }
        }

        public float EquippedWeaponCp
        {
            get
            {
                var w = Equipped;
                return w != null ? WeaponHoldScore(w) : 0f;
            }
        }

        public float EquippedWeaponAtk
        {
            get
            {
                var w = Equipped;
                if (w == null) return 0f;
                float baseAtk = w.baseAtk > 0f ? w.baseAtk : 4f;
                float atk = (baseAtk + w.level * (1.2f + w.rarity * 0.5f)) * (1f + w.awaken * 0.1f);
                if (!JobProgress.WeaponMatchesJob(w.kind))
                    atk *= MismatchAtkMul;
                return atk;
            }
        }

        public bool EquippedKindMismatch
        {
            get
            {
                var w = Equipped;
                return w != null && !JobProgress.WeaponMatchesJob(w.kind);
            }
        }

        static float WeaponHoldScore(WeaponItem w) =>
            (5f + w.level * 2f) * (1f + w.rarity * 0.4f) * (1f + w.awaken * 0.15f);

        /// <summary>
        /// 보유 효과 — 벤치마크 카피: 무기는 장착하지 않아도 '획득만으로' 공격력에
        /// 기여한다 (CP 점수만 주던 HoldScore의 실스탯화). 등급·각성 비례.
        /// </summary>
        public float HoldAtkPct
        {
            get
            {
                float pct = 0f;
                foreach (var w in Owned)
                    pct += 0.2f + w.rarity * 0.2f + w.awaken * 0.1f;
                return pct;
            }
        }

        /// <summary>
        /// 세트 효과 — 같은 계열 무기를 여러 종 모으면 붙는다 (2/4/6종).
        /// 보유 기준이라 장착하지 않아도 수집 자체가 성장이 된다.
        /// </summary>
        public float SetAtkPct
        {
            get
            {
                var defs = ContentCatalog.Weapons;
                if (defs == null || defs.Length == 0) return 0f;
                var bySet = new Dictionary<string, HashSet<string>>();
                foreach (var w in Owned)
                {
                    string cid = !string.IsNullOrEmpty(w.catalogId) ? w.catalogId : w.id;
                    string set = null;
                    foreach (var d in defs)
                        if (d.id == cid) { set = d.setId; break; }
                    if (string.IsNullOrEmpty(set)) continue;
                    if (!bySet.TryGetValue(set, out var hs))
                        bySet[set] = hs = new HashSet<string>();
                    hs.Add(cid);
                }
                float pct = 0f;
                foreach (var kv in bySet)
                {
                    int n = kv.Value.Count;
                    if (n >= 2) pct += 3f;
                    if (n >= 4) pct += 5f;
                    if (n >= 6) pct += 8f;
                }
                return pct;
            }
        }

        /// <summary>세트 단계 요약 (UI 표시용).</summary>
        public string SetSummary()
        {
            float p = SetAtkPct;
            return p > 0f ? $"세트 효과 공격 +{p:0.#}%" : "세트 효과 없음";
        }

        public static float Score(WeaponItem w)
        {
            if (w == null) return -1f;
            float s = w.rarity * 10000f + w.awaken * 1000f + w.level * 10f + w.count;
            if (w.locked) s += 500000f;
            if (w.equipped) s += 1000000f;
            return s;
        }

        /// <summary>Display order: equipped/locked first, then rarity → awaken → level.</summary>
        public List<WeaponItem> GetSortedOwned(int minRarity = 0, bool equippedOnly = false)
        {
            var list = new List<WeaponItem>(Owned.Count);
            foreach (var w in Owned)
            {
                if (w == null) continue;
                if (equippedOnly && !w.equipped) continue;
                if (w.rarity < minRarity) continue;
                list.Add(w);
            }
            list.Sort((a, b) => Score(b).CompareTo(Score(a)));
            return list;
        }

        public void ToggleLock(string id)
        {
            var w = Owned.Find(x => x.id == id);
            if (w == null) return;
            w.locked = !w.locked;
            Save();
            OnChanged?.Invoke();
        }

        public WeaponItem Equipped
        {
            get
            {
                foreach (var w in Owned)
                    if (w.equipped) return w;
                return Owned.Count > 0 ? Owned[0] : null;
            }
        }

        public string SummonOne()
        {
            bool paid = false;
            if (CurrencyWallet.Instance != null && CurrencyWallet.Instance.TrySpend(CurrencyId.WeaponTicket, 1))
                paid = true;
            else if (WalletAdapter.Instance != null && WalletAdapter.Instance.TrySpendRedDiamond(20))
                paid = true;
            if (!paid) return "무기 소환권 또는 레드다이아 20 필요";

            int pe = PityEpic;
            int pl = PityLegendary;
            var rarity = GachaRoll.RollWithPity(ref pe, ref pl);
            PityEpic = pe;
            PityLegendary = pl;
            if (SummonLevel >= 5 && rarity < GachaRarity.Rare && UnityEngine.Random.value < 0.1f)
                rarity = GachaRarity.Rare;

            int prefer = JobProgress.PreferredWeaponKind();
            var def = ContentCatalog.PickWeapon(prefer, 2.8f);
            string name = def != null ? def.name : "나무 검";
            int kind = def != null ? def.kind : WeaponItem.KindFromName(name);
            string catalogId = def != null ? def.id : "";
            float baseAtk = def != null ? def.baseAtk : 4f;

            AddOrStack(name, kind, catalogId, baseAtk, rarity);
            TotalSummons++;
            if (TotalSummons % 20 == 0) SummonLevel++;
            IdleMvp.Core.DailyMissionService.Increment("summon");
            IdleMvp.Core.QuestService.Notify(IdleMvp.Core.QuestService.Kind.Summon);
            IdleMvp.Core.AchievementService.IncrementProgress(IdleMvp.Core.AchievementService.Category.Summon);
            PassService.Instance?.NotifyWeaponSummon();
            // 10연차에서는 마지막에 한 번만 저장·통지한다.
            // 매 뽑기마다 하면 PlayerPrefs.Save(동기 디스크 쓰기)와 전체 UI 재빌드가 10배로 터진다.
            if (!_batching)
            {
                Save();
                OnChanged?.Invoke();
            }
            string pityHint = rarity >= GachaRarity.Epic ? "" : $" · 천장 {PityLegendary}/{GachaRoll.HardPityLegendary}";
            string warn = "";
            if (def != null && !JobProgress.WeaponMatchesJob(def.kind))
                warn = " (직업 비추천 kind)";
            return $"무기 소환: {rarity} {name}{warn}{pityHint}";
        }

        bool _batching;

        public string SummonTen()
        {
            var sb = "";
            _batching = true;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var msg = SummonOne();
                    if (msg.Contains("필요"))
                        return i == 0 ? msg : sb + "\n(재화 부족으로 중단)";
                    sb += msg + "\n";
                }
            }
            finally
            {
                _batching = false;
                Save();
                OnChanged?.Invoke();
            }
            return sb.Trim();
        }

        /// <summary>
        /// 문파 제한으로 못 드는 무기면 사유를, 쓸 수 있으면 null을 준다.
        /// 평민(세력 미선택)과 사파·마도는 제한이 없다 — 정파 문파만 병기가 정해져 있다.
        /// </summary>
        public string EquipBlockReason(string id)
        {
            foreach (var w in Owned)
                if (w.id == id) return IdleMvp.Core.SectService.WhyCannotUse(w.kind);
            return null;
        }

        public bool Equip(string id)
        {
            // 문파가 취급하지 않는 병기는 아예 들지 못한다(고증).
            // UI가 먼저 막아 주지만, 다른 경로로 들어와도 여기서 걸린다.
            var block = EquipBlockReason(id);
            if (block != null) { Debug.Log("[Weapon] 장착 거부 — " + block); return false; }

            bool ok = false;
            foreach (var w in Owned)
            {
                w.equipped = w.id == id;
                if (w.equipped) ok = true;
            }
            if (ok)
            {
                Save();
                OnChanged?.Invoke();
                SyncInventoryWeaponSlot();
                // 손에 든 무기 그래픽 갱신 (필드 리그 + 모든 프리뷰)
                IdleMvp.Combat.FieldAutoHuntController.Instance?.RefreshHeroAppearance();
                if (EquippedKindMismatch)
                    Debug.Log("[Weapon] Equipped kind mismatches job — ATK ×0.75");
            }
            return ok;
        }

        /// <summary>Equip highest score owned weapon (rarity → awaken → level → count).</summary>
        public string EquipBest()
        {
            if (Owned == null || Owned.Count == 0) return "보유 무기 없음";
            WeaponItem best = null;
            float bestScore = -1f;
            foreach (var w in Owned)
            {
                // EquipBest ignores lock pin bonus — pure combat score
                float score = w.rarity * 10000f + w.awaken * 1000f + w.level * 10f + w.count;
                // Job-matching weapons dodge the mismatch ATK penalty — prefer them strongly.
                if (!JobProgress.WeaponMatchesJob(w.kind))
                    score *= 0.6f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = w;
                }
            }
            if (best == null) return "보유 무기 없음";
            Equip(best.id);
            return $"{best.name} 자동장착 (Lv.{best.level} · 등급 {best.rarity} · 각성 {best.awaken})";
        }

        /// <summary>
        /// Dismantle low-rarity unequipped unlocked weapons, and trim surplus stacks
        /// past awaken reserve. Returns stones gained message.
        /// </summary>
        public string DisassembleJunk(int maxRarityExclusive = 1)
        {
            float stones = 0f;
            int removed = 0;
            int trimmed = 0;
            for (int i = Owned.Count - 1; i >= 0; i--)
            {
                var w = Owned[i];
                if (w == null || w.equipped || w.locked) continue;

                // Keep 2 extras for awaken when below max awaken
                int reserve = w.awaken < 5 ? 2 : 1;
                if (w.count > reserve)
                {
                    int extra = w.count - reserve;
                    w.count = reserve;
                    stones += extra * (0.4f + w.rarity * 0.3f);
                    trimmed += extra;
                }

                if (w.rarity < maxRarityExclusive)
                {
                    stones += 0.8f + w.rarity * 0.5f + w.level * 0.05f + w.awaken * 0.4f + w.count * 0.3f;
                    Owned.RemoveAt(i);
                    removed++;
                }
            }

            if (removed == 0 && trimmed == 0)
                return "분해할 저등급 무기 없음";

            if (stones > 0f)
                CurrencyWallet.Instance?.Add(CurrencyId.WeaponEnhanceStone, stones);
            Save();
            OnChanged?.Invoke();
            return $"무기 분해 {removed}종 · 잉여 {trimmed} · 강화석 +{stones:0.#}";
        }

        void SyncInventoryWeaponSlot()
        {
            var inv = InventoryAdapter.Instance;
            var w = Equipped;
            if (inv == null || inv.Slots == null || inv.Slots.Length == 0 || w == null) return;
            inv.Slots[0].owned = true;
            inv.Slots[0].level = Mathf.Max(1, w.level);
            inv.Slots[0].rarity = Mathf.Clamp(w.rarity, 0, 5);
            inv.NotifyChanged();
        }

        public string LevelUpEquipped()
        {
            var w = Equipped;
            if (w == null) return "장착 무기 없음";
            int max = 20 + w.awaken * 20;
            if (w.level >= max) return "최대 레벨 (각성 필요)";
            if (CurrencyWallet.Instance == null ||
                !CurrencyWallet.Instance.TrySpend(CurrencyId.WeaponEnhanceStone, 1 + w.level * 0.2f))
                return "무기 강화석 부족";
            w.level++;
            Save();
            OnChanged?.Invoke();
            return $"{w.name} Lv.{w.level}";
        }

        /// <summary>소환·드랍 공용 — 무기를 겹치거나(각성) 새로 추가한다. 저장·통지는 호출자 책임.</summary>
        void AddOrStack(string name, int kind, string catalogId, float baseAtk, GachaRarity rarity)
        {
            var existing = Owned.Find(w =>
                (!string.IsNullOrEmpty(catalogId) && w.catalogId == catalogId && w.rarity == (int)rarity)
                || (w.name == name && w.rarity == (int)rarity && string.IsNullOrEmpty(w.catalogId)));
            if (existing != null)
            {
                existing.count++;
                if (existing.baseAtk <= 0f) existing.baseAtk = baseAtk;
                if (string.IsNullOrEmpty(existing.catalogId)) existing.catalogId = catalogId;
                TryAutoAwaken(existing);
            }
            else
            {
                Owned.Add(new WeaponItem
                {
                    id = Guid.NewGuid().ToString("N"),
                    name = name,
                    rarity = (int)rarity,
                    level = 1,
                    count = 1,
                    kind = kind,
                    catalogId = catalogId,
                    baseAtk = baseAtk
                });
            }
            if (!string.IsNullOrEmpty(catalogId))
                IdleMvp.Core.CollectionService.TryCollect("w_" + catalogId);
        }

        /// <summary>필드 드랍 — 소환권 없이 무기를 지급한다 (DropService 전용).</summary>
        public void GrantDrop(Core.WeaponDef def, GachaRarity rarity)
        {
            if (def == null) return;
            AddOrStack(def.name, def.kind, def.id, def.baseAtk, rarity);
            Save();
            OnChanged?.Invoke();
        }

        void TryAutoAwaken(WeaponItem w)
        {
            while (w.count >= 2 && w.awaken < 5)
            {
                w.count -= 1;
                w.awaken++;
            }
            // Promote: 5-star + 5 extras → next rarity
            if (w.awaken >= 5 && w.count >= 5 && w.rarity < 3)
            {
                w.count -= 5;
                w.awaken = 0;
                w.rarity++;
                w.level = 1;
            }
        }

        void Save()
        {
            var json = JsonUtility.ToJson(new Wrap
            {
                items = Owned.ToArray(),
                summonLevel = SummonLevel,
                total = TotalSummons,
                pityEpic = PityEpic,
                pityLegendary = PityLegendary
            });
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.items != null) Owned = new List<WeaponItem>(w.items);
            SummonLevel = Mathf.Max(1, w?.summonLevel ?? 1);
            TotalSummons = w?.total ?? 0;
            PityEpic = w?.pityEpic ?? 0;
            PityLegendary = w?.pityLegendary ?? 0;
            foreach (var item in Owned)
            {
                if (item == null) continue;

                // 옛 세이브는 catalogId 없이 이름만 갖고 있다 → 이름으로 카탈로그를 되찾는다.
                // 이게 없으면 무협 리네이밍 전 이름('빙결 지팡이')이 그대로 화면에 남고,
                // 무기별 아이콘도 catalogId가 비어 종류 아이콘으로 폴백된다.
                if (string.IsNullOrEmpty(item.catalogId) && !string.IsNullOrEmpty(item.name))
                    item.catalogId = CatalogIdFromName(item.name);

                if (!string.IsNullOrEmpty(item.catalogId))
                {
                    var def = ContentCatalog.GetWeapon(item.catalogId);
                    if (def != null)
                    {
                        item.kind = def.kind;
                        if (item.baseAtk <= 0f) item.baseAtk = def.baseAtk;
                        item.name = def.name;   // 리네이밍을 기존 세이브에도 반영
                        continue;
                    }
                }
                if (item.kind == 0 && !string.IsNullOrEmpty(item.name))
                    item.kind = WeaponItem.KindFromName(item.name);
                if (item.baseAtk <= 0f) item.baseAtk = 4f + item.kind;
            }
        }

        /// <summary>무협 리네이밍 이전 이름 → weapons.json id.</summary>
        static readonly Dictionary<string, string> LegacyWeaponNames = new Dictionary<string, string>
        {
            { "나무 검", "w_wood_sword" }, { "강철 검", "w_steel_sword" },
            { "미스릴 검", "w_mythril_blade" }, { "드래곤 블레이드", "w_dragon_blade" },
            { "홍련 사브르", "w_crimson_saber" }, { "기사의 클레이모어", "w_knight_claymore" },
            { "고목 지팡이", "w_oak_staff" }, { "화염 지팡이", "w_flame_staff" },
            { "빙결 지팡이", "w_frost_staff" }, { "아케인 스태프", "w_arcane_staff" },
            { "뇌전 로드", "w_thunder_rod" }, { "공허의 완드", "w_void_wand" },
            { "사냥꾼의 활", "w_hunter_bow" }, { "메이플 보우", "w_maple_bow" },
            { "폭풍 활", "w_storm_bow" }, { "불사조 활", "w_phoenix_bow" },
            { "합성 장궁", "w_composite_bow" }, { "저격 활", "w_sniper_bow" },
            { "청동 아대", "w_bronze_claw" }, { "그림자 아대", "w_shadow_claw" },
            { "맹독 아대", "w_venom_claw" }, { "암살자의 아대", "w_assassin_claw" },
            { "철 너클", "w_iron_knuckle" }, { "혈귀의 주먹", "w_blood_fist" },
            { "초보자의 검", "w_wood_sword" },
        };

        static string CatalogIdFromName(string name)
        {
            string hit;
            if (LegacyWeaponNames.TryGetValue(name, out hit)) return hit;
            // 이미 무협 이름이면 카탈로그에서 그대로 찾는다
            var defs = ContentCatalog.Weapons;
            if (defs != null)
                for (int i = 0; i < defs.Length; i++)
                    if (defs[i] != null && defs[i].name == name) return defs[i].id;
            return "";
        }

        [Serializable]
        class Wrap
        {
            public WeaponItem[] items;
            public int summonLevel;
            public int total;
            public int pityEpic;
            public int pityLegendary;
        }
    }
}
