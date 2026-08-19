using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Core
{
    public static class CollectionService
    {
        public static event Action OnChanged;

        public struct Entry
        {
            public string Id;
            public string Name;
            public string Category; // "monster", "weapon", "companion"
        }

        static readonly Entry[] MonsterEntries =
        {
            new Entry { Id = "Goblin",             Name = "산적",       Category = "monster" },
            new Entry { Id = "Bandit Cutthroat",   Name = "녹림도",     Category = "monster" },
            new Entry { Id = "Bandit Bowman",      Name = "녹림궁수",   Category = "monster" },
            new Entry { Id = "Raider",             Name = "비적",       Category = "monster" },
            new Entry { Id = "Orc Warrior",        Name = "흑풍단원",   Category = "monster" },
            new Entry { Id = "Orc Brute",          Name = "흑풍두목",   Category = "monster" },
            new Entry { Id = "Berserker",          Name = "마인",       Category = "monster" },
        };

        const string PrefKey = "IdleGrow.Collection.";

        public static bool IsCollected(string id) => PlayerPrefs.GetInt(PrefKey + id, 0) == 1;

        public static bool TryCollect(string id)
        {
            if (IsCollected(id)) return false;
            PlayerPrefs.SetInt(PrefKey + id, 1);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
            return true;
        }

        public static int MonsterCollectedCount
        {
            get
            {
                int c = 0;
                foreach (var e in MonsterEntries)
                    if (IsCollected(e.Id)) c++;
                return c;
            }
        }

        public static int WeaponCollectedCount
        {
            get
            {
                int c = 0;
                var weapons = ContentCatalog.Weapons;
                if (weapons != null)
                    foreach (var w in weapons)
                        if (IsCollected("w_" + w.id)) c++;
                return c;
            }
        }

        public static int CompanionCollectedCount
        {
            get
            {
                int c = 0;
                var comps = ContentCatalog.Companions;
                if (comps != null)
                    foreach (var comp in comps)
                        if (IsCollected("c_" + comp.id)) c++;
                return c;
            }
        }

        public static float BonusAtkPct =>
            MonsterCollectedCount * 0.5f + WeaponCollectedCount * 1.0f + CompanionCollectedCount * 0.8f;

        public static float BonusHpPct =>
            MonsterCollectedCount * 0.3f + WeaponCollectedCount * 0.5f + CompanionCollectedCount * 0.5f;

        public static float BonusGoldPct =>
            MonsterCollectedCount * 0.2f + CompanionCollectedCount * 0.3f;

        public static Entry[] GetMonsterEntries() => MonsterEntries;

        public static Entry[] GetWeaponEntries()
        {
            var weapons = ContentCatalog.Weapons;
            if (weapons == null || weapons.Length == 0) return Array.Empty<Entry>();
            var entries = new Entry[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
                entries[i] = new Entry { Id = "w_" + weapons[i].id, Name = weapons[i].name, Category = "weapon" };
            return entries;
        }

        public static Entry[] GetCompanionEntries()
        {
            var comps = ContentCatalog.Companions;
            if (comps == null || comps.Length == 0) return Array.Empty<Entry>();
            var entries = new Entry[comps.Length];
            for (int i = 0; i < comps.Length; i++)
                entries[i] = new Entry { Id = "c_" + comps[i].id, Name = comps[i].name, Category = "companion" };
            return entries;
        }

        public static void SyncFromOwnedData()
        {
            var ws = Adapters.WeaponSummonAdapter.Instance;
            if (ws != null)
                foreach (var w in ws.Owned)
                    if (!string.IsNullOrEmpty(w.catalogId)) TryCollect("w_" + w.catalogId);

            var cs = Adapters.CompanionAdapter.Instance;
            if (cs != null)
                foreach (var c in cs.Owned)
                    if (!string.IsNullOrEmpty(c.catalogId)) TryCollect("c_" + c.catalogId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoSync() => SyncFromOwnedData();
    }
}
