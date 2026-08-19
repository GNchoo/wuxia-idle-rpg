using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Core
{
    [Serializable]
    public class JobDef
    {
        public string id;
        public string name;
        public string role;
        public string primaryStat;
        public string treeId;
        public bool unlocked;
        public int unlockLevel = 1;
        public float atkMul = 1f;
        public float hpMul = 1f;
        public float defMul = 1f;
        public string allowedKinds = "0";
        public string desc;

        public int[] ParseAllowedKinds()
        {
            if (string.IsNullOrEmpty(allowedKinds)) return new[] { 0 };
            var parts = allowedKinds.Split(',');
            var list = new List<int>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i].Trim(), out int k))
                    list.Add(k);
            }
            return list.Count > 0 ? list.ToArray() : new[] { 0 };
        }

        public bool AllowsWeaponKind(int kind)
        {
            var kinds = ParseAllowedKinds();
            for (int i = 0; i < kinds.Length; i++)
                if (kinds[i] == kind) return true;
            return false;
        }
    }

    [Serializable]
    public class WeaponDef
    {
        public string id;
        public string name;
        public int kind;
        public float baseAtk = 5f;
        public string setId;
    }

    [Serializable]
    public class CompanionDef
    {
        public string id;
        public string name;
        public string role = "dps";
        public float passiveAtkPct;
        public float passiveHpPct;
        public float passiveGoldPct;
    }

    /// <summary>
    /// 부위별 장비 티어. 레벨 10당 한 단계씩 올라간다(reqLevel 1, 11, 21 ...).
    /// 이름·등급·성능·색이 티어마다 달라서, 강화만 하던 장비창이 실제로 '바뀌는' 느낌을 준다.
    /// </summary>
    [Serializable]
    public class EquipDef
    {
        public string id;
        public string name;
        public int slot;        // 0 무기 1 투구 2 갑옷 3 장신구 4 하의 5 신발
        public int tier;        // 1..10
        public int reqLevel;
        public int rarity;      // 0 일반 1 희귀 2 영웅 3 전설
        public int atk;
        public int hp;
        public int def;
        public string tint;     // #RRGGBB — 전용 아이콘이 없을 때 부위 아이콘을 티어별로 물들인다
        public string icon;     // Resources 경로 (예: EquipIcons/eq_w01). 없으면 부위 아이콘 폴백
        public string desc;
    }

    [Serializable]
    class EquipFile { public EquipDef[] equipment; }

    [Serializable]
    public class ArtifactDef
    {
        public string id;
        public string name;
        public string setId;
        public float slotCp;
        public float goldPct;
        public float idlePct;
        public float atkPct;
    }

    [Serializable]
    class JobsFile { public JobDef[] jobs; }

    [Serializable]
    class WeaponsFile { public WeaponDef[] weapons; }

    [Serializable]
    class CompanionsFile { public CompanionDef[] companions; }

    [Serializable]
    class ArtifactsFile { public ArtifactDef[] artifacts; }

    /// <summary>Loads content tables from Resources/Content/* (mirrors Data/).</summary>
    public static class ContentCatalog
    {
        static JobDef[] _jobs;
        static WeaponDef[] _weapons;
        static CompanionDef[] _companions;
        static ArtifactDef[] _artifacts;
        static EquipDef[] _equipment;
        static bool _loaded;

        public static JobDef[] Jobs
        {
            get { EnsureLoaded(); return _jobs; }
        }

        public static WeaponDef[] Weapons
        {
            get { EnsureLoaded(); return _weapons; }
        }

        public static CompanionDef[] Companions
        {
            get { EnsureLoaded(); return _companions; }
        }

        public static ArtifactDef[] Artifacts
        {
            get { EnsureLoaded(); return _artifacts; }
        }

        public static EquipDef[] Equipment
        {
            get { EnsureLoaded(); return _equipment; }
        }

        /// <summary>부위+레벨 → 해당 티어 장비 (레벨 10당 다음 티어).</summary>
        public static EquipDef GetEquip(int slot, int level)
        {
            var all = Equipment;
            if (all == null || all.Length == 0) return null;
            EquipDef best = null;
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                if (e == null || e.slot != slot) continue;
                if (e.reqLevel > level) continue;
                if (best == null || e.tier > best.tier) best = e;
            }
            // 레벨이 1티어 요구치에도 못 미치면 가장 낮은 티어를 준다
            if (best == null)
                for (int i = 0; i < all.Length; i++)
                    if (all[i] != null && all[i].slot == slot &&
                        (best == null || all[i].tier < best.tier)) best = all[i];
            return best;
        }

        /// <summary>해당 부위의 전체 티어 목록 (티어 오름차순).</summary>
        public static List<EquipDef> EquipsForSlot(int slot)
        {
            var list = new List<EquipDef>();
            var all = Equipment;
            if (all == null) return list;
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].slot == slot) list.Add(all[i]);
            list.Sort((a, b) => a.tier.CompareTo(b.tier));
            return list;
        }

        public static void Load()
        {
            _jobs = LoadArray<JobsFile, JobDef>("Content/jobs", f => f?.jobs, FallbackJobs);
            _weapons = LoadArray<WeaponsFile, WeaponDef>("Content/weapons", f => f?.weapons, FallbackWeapons);
            _companions = LoadArray<CompanionsFile, CompanionDef>("Content/companions", f => f?.companions, FallbackCompanions);
            _artifacts = LoadArray<ArtifactsFile, ArtifactDef>("Content/artifacts", f => f?.artifacts, FallbackArtifacts);
            _equipment = LoadArray<EquipFile, EquipDef>("Content/equipment", f => f?.equipment, () => new EquipDef[0]);
            _loaded = true;
        }

        static void EnsureLoaded()
        {
            if (!_loaded) Load();
        }

        static TItem[] LoadArray<TFile, TItem>(string path, Func<TFile, TItem[]> pick, Func<TItem[]> fallback)
            where TFile : class
        {
            var asset = Resources.Load<TextAsset>(path);
            if (asset != null)
            {
                var file = JsonUtility.FromJson<TFile>(asset.text);
                var arr = pick(file);
                if (arr != null && arr.Length > 0) return arr;
            }
            Debug.LogWarning($"[IdleMvp] {path} missing — using fallback.");
            return fallback();
        }

        public static JobDef GetJob(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(id)) return _jobs.Length > 0 ? _jobs[0] : null;
            for (int i = 0; i < _jobs.Length; i++)
                if (_jobs[i].id == id) return _jobs[i];
            return _jobs.Length > 0 ? _jobs[0] : null;
        }

        public static JobDef GetJobByIndex(int index)
        {
            EnsureLoaded();
            if (_jobs == null || _jobs.Length == 0) return null;
            return _jobs[Mathf.Clamp(index, 0, _jobs.Length - 1)];
        }

        public static WeaponDef GetWeapon(string id)
        {
            EnsureLoaded();
            for (int i = 0; i < _weapons.Length; i++)
                if (_weapons[i].id == id) return _weapons[i];
            return null;
        }

        public static CompanionDef GetCompanion(string id)
        {
            EnsureLoaded();
            for (int i = 0; i < _companions.Length; i++)
                if (_companions[i].id == id) return _companions[i];
            return null;
        }

        public static CompanionDef GetCompanionByName(string name)
        {
            EnsureLoaded();
            for (int i = 0; i < _companions.Length; i++)
                if (_companions[i].name == name) return _companions[i];
            return null;
        }

        public static ArtifactDef GetArtifact(string id)
        {
            EnsureLoaded();
            for (int i = 0; i < _artifacts.Length; i++)
                if (_artifacts[i].id == id) return _artifacts[i];
            return null;
        }

        public static WeaponDef PickWeapon(int preferredKind = -1, float preferWeight = 2.5f)
        {
            EnsureLoaded();
            if (_weapons == null || _weapons.Length == 0) return null;
            float total = 0f;
            for (int i = 0; i < _weapons.Length; i++)
                total += _weapons[i].kind == preferredKind ? preferWeight : 1f;
            float roll = UnityEngine.Random.Range(0f, total);
            for (int i = 0; i < _weapons.Length; i++)
            {
                float w = _weapons[i].kind == preferredKind ? preferWeight : 1f;
                if (roll <= w) return _weapons[i];
                roll -= w;
            }
            return _weapons[_weapons.Length - 1];
        }

        public static CompanionDef PickCompanion()
        {
            EnsureLoaded();
            if (_companions == null || _companions.Length == 0) return null;
            return _companions[UnityEngine.Random.Range(0, _companions.Length)];
        }

        static JobDef[] FallbackJobs() => new[]
        {
            new JobDef { id = "hero", name = "무사", role = "전사", primaryStat = "STR", treeId = "hero", unlocked = true, atkMul = 1.08f, hpMul = 1.12f, defMul = 1.05f, allowedKinds = "0,1", desc = "정파 무사" },
            new JobDef { id = "bowmaster", name = "살수", role = "궁수", primaryStat = "DEX", treeId = "bowmaster", unlocked = true, atkMul = 1.1f, hpMul = 0.95f, defMul = 0.92f, allowedKinds = "2,3", desc = "사파 살수" },
            new JobDef { id = "archmage", name = "마두", role = "마법사", primaryStat = "INT", treeId = "archmage", unlocked = true, atkMul = 1.15f, hpMul = 0.88f, defMul = 0.9f, allowedKinds = "0,1", desc = "마도 마두" }
        };

        static WeaponDef[] FallbackWeapons() => new[]
        {
            new WeaponDef { id = "w_wood_sword", name = "목검", kind = 0, baseAtk = 4 },
            new WeaponDef { id = "w_oak_staff", name = "고목선장", kind = 1, baseAtk = 5 },
            new WeaponDef { id = "w_hunter_bow", name = "사냥꾼의 궁", kind = 2, baseAtk = 6 },
            new WeaponDef { id = "w_bronze_claw", name = "청동 권갑", kind = 3, baseAtk = 5 }
        };

        static CompanionDef[] FallbackCompanions() => new[]
        {
            new CompanionDef { id = "c_warrior", name = "무사 동료", role = "tank", passiveAtkPct = 2, passiveHpPct = 4 },
            new CompanionDef { id = "c_mage", name = "도사 동료", role = "dps", passiveAtkPct = 4 }
        };

        static ArtifactDef[] FallbackArtifacts() => new[]
        {
            new ArtifactDef { id = "a_maple_leaf", name = "영약 잎사귀", setId = "origin", slotCp = 25, goldPct = 2, idlePct = 1 }
        };
    }
}
