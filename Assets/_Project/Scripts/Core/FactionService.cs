using System;
using UnityEngine;

namespace IdleMvp.Core
{
    public static class FactionService
    {
        public static event Action OnChanged;

        const string PrefOrthodox  = "IdleGrow.Faction.Orthodox";
        const string PrefUnorthodox = "IdleGrow.Faction.Unorthodox";
        const string PrefDemonic   = "IdleGrow.Faction.Demonic";
        const string PrefSelected  = "IdleGrow.Faction.Selected";
        const string PrefPrevious  = "IdleGrow.Faction.Previous";

        public static int Orthodox  { get; private set; }
        public static int Unorthodox { get; private set; }
        public static int Demonic   { get; private set; }

        public static string Selected { get; private set; } = "";
        public static string Previous { get; private set; } = "";
        public static bool HasSelected => !string.IsNullOrEmpty(Selected);
        public static bool HasPrevious => !string.IsNullOrEmpty(Previous);

        public static string DisplayName
        {
            get
            {
                if (!HasSelected) return "평민";
                switch (Selected)
                {
                    case "hero":      return "정파";
                    case "bowmaster": return "사파";
                    case "archmage":  return "마도";
                    default:          return "평민";
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            _subscribed = false;
            _triggerPending = false;
            Orthodox   = PlayerPrefs.GetInt(PrefOrthodox, 0);
            Unorthodox = PlayerPrefs.GetInt(PrefUnorthodox, 0);
            Demonic    = PlayerPrefs.GetInt(PrefDemonic, 0);
            Selected   = PlayerPrefs.GetString(PrefSelected, "");
            Previous   = PlayerPrefs.GetString(PrefPrevious, "");
            TrySubscribe();
        }

        static bool _subscribed;
        static bool _triggerPending;

        public static void TrySubscribe()
        {
            if (_subscribed) return;
            var pg = Progression.PlayerGrowth.Instance;
            if (pg == null) return;
            pg.OnChanged += CheckLevelTrigger;
            _subscribed = true;
        }

        static void CheckLevelTrigger()
        {
            if (HasSelected) return;
            var pg = Progression.PlayerGrowth.Instance;
            if (pg == null || pg.Level < 6) return;
            _triggerPending = true;
            OnChanged?.Invoke();
        }

        public static bool ShouldShowSelection => _triggerPending && !HasSelected;

        public static void SelectFaction(string treeId)
        {
            if (HasSelected) return;
            Selected = treeId;
            switch (treeId)
            {
                case "hero":      Orthodox  += 50; break;
                case "bowmaster": Unorthodox += 50; break;
                case "archmage":  Demonic   += 50; break;
            }
            _triggerPending = false;
            Save();

            JobProgress.SetJob(treeId);
            OnChanged?.Invoke();
        }

        public static bool CanChangeFaction
        {
            get
            {
                if (!HasSelected) return false;
                var pg = Progression.PlayerGrowth.Instance;
                return pg != null && pg.Level >= 30;
            }
        }

        public static string ChangeFaction(string newTreeId)
        {
            if (!HasSelected) return "먼저 세력을 선택하세요";
            if (newTreeId == Selected) return "이미 해당 세력입니다";
            var pg = Progression.PlayerGrowth.Instance;
            if (pg == null || pg.Level < 30) return "레벨 30 이상 필요";

            Previous = Selected;
            Selected = newTreeId;
            switch (newTreeId)
            {
                case "hero":      Orthodox  += 30; break;
                case "bowmaster": Unorthodox += 30; break;
                case "archmage":  Demonic   += 30; break;
            }
            Save();
            JobProgress.SetJob(newTreeId);
            OnChanged?.Invoke();
            return null;
        }

        public static string SynergyName
        {
            get
            {
                if (!HasPrevious || Previous == Selected) return null;
                string pair = Previous + "+" + Selected;
                switch (pair)
                {
                    case "hero+archmage":
                    case "archmage+hero":    return "반마반선";
                    case "bowmaster+hero":
                    case "hero+bowmaster":   return "유독검";
                    case "archmage+bowmaster":
                    case "bowmaster+archmage": return "혈독술";
                    default: return null;
                }
            }
        }

        static void Save()
        {
            PlayerPrefs.SetInt(PrefOrthodox, Orthodox);
            PlayerPrefs.SetInt(PrefUnorthodox, Unorthodox);
            PlayerPrefs.SetInt(PrefDemonic, Demonic);
            PlayerPrefs.SetString(PrefSelected, Selected);
            PlayerPrefs.SetString(PrefPrevious, Previous);
            PlayerPrefs.Save();
        }
    }
}
