using System;
using System.Collections.Generic;
using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Progression
{
    [Serializable]
    public class ArtifactOwned
    {
        public string defId;
        public int fragments = 1;
        public bool equipped;
    }

    /// <summary>Idle meta artifacts: own / equip up to 3 / set bonuses.</summary>
    public class ArtifactService : MonoBehaviour
    {
        public static ArtifactService Instance { get; private set; }

        public List<ArtifactOwned> Owned { get; private set; } = new List<ArtifactOwned>();
        public const int MaxEquip = 3;
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Artifacts";

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

        public int EquippedCount
        {
            get
            {
                int n = 0;
                foreach (var o in Owned) if (o.equipped) n++;
                return n;
            }
        }

        public float ArtifactCp
        {
            get
            {
                float cp = 0f;
                foreach (var o in Owned)
                {
                    var d = ContentCatalog.GetArtifact(o.defId);
                    if (d == null) continue;
                    cp += d.slotCp * 0.25f * Mathf.Max(1, o.fragments);
                    if (o.equipped) cp += d.slotCp;
                }
                cp += SetBonusCp();
                return cp;
            }
        }

        public float AtkPctBonus => SumEquipped(d => d.atkPct) + SetBonusAtkPct();
        public float GoldPctBonus => SumEquipped(d => d.goldPct) + SetBonusGoldPct();
        public float IdlePctBonus => SumEquipped(d => d.idlePct) + SetBonusIdlePct();

        float SumEquipped(Func<ArtifactDef, float> pick)
        {
            float v = 0f;
            foreach (var o in Owned)
            {
                if (!o.equipped) continue;
                var d = ContentCatalog.GetArtifact(o.defId);
                if (d != null) v += pick(d);
            }
            return v;
        }

        Dictionary<string, int> CountSets()
        {
            var map = new Dictionary<string, int>();
            foreach (var o in Owned)
            {
                if (!o.equipped) continue;
                var d = ContentCatalog.GetArtifact(o.defId);
                if (d == null || string.IsNullOrEmpty(d.setId)) continue;
                if (!map.ContainsKey(d.setId)) map[d.setId] = 0;
                map[d.setId]++;
            }
            return map;
        }

        float SetBonusCp()
        {
            float cp = 0f;
            foreach (var kv in CountSets())
                if (kv.Value >= 2) cp += 40f * kv.Value;
            return cp;
        }

        float SetBonusAtkPct()
        {
            float v = 0f;
            foreach (var kv in CountSets())
                if (kv.Value >= 2) v += 2f * (kv.Value - 1);
            return v;
        }

        float SetBonusGoldPct()
        {
            float v = 0f;
            foreach (var kv in CountSets())
                if (kv.Value >= 2 && (kv.Key == "fortune" || kv.Key == "maple")) v += 3f;
            return v;
        }

        float SetBonusIdlePct()
        {
            float v = 0f;
            foreach (var kv in CountSets())
                if (kv.Value >= 2 && kv.Key == "idle") v += 4f;
            return v;
        }

        public string GrantFragment(string defId, int amount = 1)
        {
            if (string.IsNullOrEmpty(defId) || amount <= 0) return "";
            var def = ContentCatalog.GetArtifact(defId);
            if (def == null) return "유물 데이터 없음";
            var o = Owned.Find(x => x.defId == defId);
            if (o == null)
            {
                Owned.Add(new ArtifactOwned { defId = defId, fragments = amount });
                Save();
                OnChanged?.Invoke();
                return $"유물 획득 · {def.name}";
            }
            o.fragments += amount;
            Save();
            OnChanged?.Invoke();
            return $"{def.name} 조각 +{amount}";
        }

        public string TryDropRandom(float chance = 0.08f)
        {
            if (UnityEngine.Random.value > chance) return null;
            var pool = ContentCatalog.Artifacts;
            if (pool == null || pool.Length == 0) return null;
            var pick = pool[UnityEngine.Random.Range(0, pool.Length)];
            return GrantFragment(pick.id, 1);
        }

        public string Equip(string defId)
        {
            var o = Owned.Find(x => x.defId == defId);
            if (o == null) return "미보유 유물";
            if (o.equipped) return "이미 장착";
            if (EquippedCount >= MaxEquip)
                return $"장착칸 {MaxEquip}개 초과 — 다른 유물을 해제하세요";
            o.equipped = true;
            Save();
            OnChanged?.Invoke();
            var d = ContentCatalog.GetArtifact(defId);
            return $"{d?.name ?? defId} 장착";
        }

        public string Unequip(string defId)
        {
            var o = Owned.Find(x => x.defId == defId);
            if (o == null || !o.equipped) return "장착 중 아님";
            o.equipped = false;
            Save();
            OnChanged?.Invoke();
            return "장착 해제";
        }

        public string ToggleEquip(string defId)
        {
            var o = Owned.Find(x => x.defId == defId);
            if (o == null) return "미보유";
            return o.equipped ? Unequip(defId) : Equip(defId);
        }

        public string StatusLine()
        {
            return $"유물 {Owned.Count}종 · 장착 {EquippedCount}/{MaxEquip} · CP+{ArtifactCp:0} · ATK+{AtkPctBonus:0.#}%";
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new Wrap { items = Owned.ToArray() }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.items != null) Owned = new List<ArtifactOwned>(w.items);
        }

        [Serializable]
        class Wrap
        {
            public ArtifactOwned[] items;
        }
    }
}
