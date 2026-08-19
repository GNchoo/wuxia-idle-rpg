using System;
using IdleMvp.Adapters;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>Selected job + unlock checks. Prefs: IdleGrow.Maple.JobId</summary>
    public static class JobProgress
    {
        public const string PrefJobId = "IdleGrow.Maple.JobId";
        public const string PrefJobNameLegacy = "IdleGrow.Maple.Job";

        public static event Action OnJobChanged;

        public static string JobId
        {
            get
            {
                string id = PlayerPrefs.GetString(PrefJobId, "");
                if (!string.IsNullOrEmpty(id)) return id;
                // Migrate legacy display name → id
                string legacy = PlayerPrefs.GetString(PrefJobNameLegacy, "무사");
                var jobs = ContentCatalog.Jobs;
                for (int i = 0; i < jobs.Length; i++)
                {
                    if (jobs[i].name == legacy)
                    {
                        PlayerPrefs.SetString(PrefJobId, jobs[i].id);
                        PlayerPrefs.Save();
                        return jobs[i].id;
                    }
                }
                return "hero";
            }
        }

        public static JobDef Current => ContentCatalog.GetJob(JobId);

        public static string TreeId
        {
            get
            {
                var j = Current;
                return j != null && !string.IsNullOrEmpty(j.treeId) ? j.treeId : "hero";
            }
        }

        public static bool IsUnlocked(JobDef job)
        {
            if (job == null) return false;
            if (job.unlocked) return true;
            int lv = PlayerGrowth.Instance != null ? PlayerGrowth.Instance.Level : 1;
            return lv >= job.unlockLevel;
        }

        public static bool IsUnlocked(int index)
        {
            return IsUnlocked(ContentCatalog.GetJobByIndex(index));
        }

        public static string SetJob(string jobId)
        {
            var job = ContentCatalog.GetJob(jobId);
            if (job == null) return "직업 없음";
            if (!IsUnlocked(job))
                return $"잠금 · Lv.{job.unlockLevel} 필요";

            string prevTree = TreeId;
            PlayerPrefs.SetString(PrefJobId, job.id);
            PlayerPrefs.SetString(PrefJobNameLegacy, job.name);
            PlayerPrefs.SetInt("IdleGrow.Maple.JobDone", 1);
            PlayerPrefs.Save();

            if (prevTree != job.treeId)
                SkillAdapter.Instance?.ReloadForTree(job.treeId);
            else
                SkillAdapter.Instance?.RefreshSkillNamesFromTree();

            OnJobChanged?.Invoke();
            return $"전직 완료 · {job.name}";
        }

        public static float AtkMul => Current != null ? Current.atkMul : 1f;
        public static float HpMul => Current != null ? Current.hpMul : 1f;
        public static float DefMul => Current != null ? Current.defMul : 1f;

        public static int PreferredWeaponKind()
        {
            var j = Current;
            if (j == null) return 0;
            var kinds = j.ParseAllowedKinds();
            return kinds.Length > 0 ? kinds[0] : 0;
        }

        public static bool WeaponMatchesJob(int kind)
        {
            var j = Current;
            return j == null || j.AllowsWeaponKind(kind);
        }
    }
}
