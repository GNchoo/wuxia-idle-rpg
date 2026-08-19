using System;
using System.Text;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>
    /// Versioned snapshot of key PlayerPrefs prefixes for backup / migrate.
    /// Does not replace per-service saves — exports a portable JSON blob.
    /// </summary>
    public class SaveSnapshotService : MonoBehaviour
    {
        public static SaveSnapshotService Instance { get; private set; }

        public const int SchemaVersion = 1;
        const string SnapshotPref = "IdleGrow.SaveSnapshot.LastExport";

        static readonly string[] PrefPrefixes =
        {
            "IdleMvp.",
            "IdleGrow."
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public string ExportToPrefs()
        {
            var json = BuildSnapshotJson();
            PlayerPrefs.SetString(SnapshotPref, json);
            PlayerPrefs.Save();
            return $"세이브 스냅샷 저장됨 (v{SchemaVersion}, {json.Length}자)";
        }

        public string ExportToClipboard()
        {
            var json = BuildSnapshotJson();
            GUIUtility.systemCopyBuffer = json;
            PlayerPrefs.SetString(SnapshotPref, json);
            PlayerPrefs.Save();
            return "세이브 스냅샷을 클립보드에 복사했습니다";
        }

        public string ImportFromClipboard()
        {
            string json = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(json) || json.IndexOf("\"schema\"", StringComparison.Ordinal) < 0)
                return "클립보드에 유효한 스냅샷이 없습니다";
            return ImportJson(json);
        }

        public string ImportJson(string json)
        {
            try
            {
                var snap = JsonUtility.FromJson<Snapshot>(json);
                if (snap == null || snap.entries == null) return "스냅샷 파싱 실패";
                if (snap.schema > SchemaVersion)
                    return $"스냅샷 버전 {snap.schema}은 이 빌드({SchemaVersion})보다 높습니다";
                for (int i = 0; i < snap.entries.Length; i++)
                {
                    var e = snap.entries[i];
                    if (e == null || string.IsNullOrEmpty(e.key)) continue;
                    if (e.kind == 0) PlayerPrefs.SetInt(e.key, e.intVal);
                    else if (e.kind == 1) PlayerPrefs.SetFloat(e.key, e.floatVal);
                    else PlayerPrefs.SetString(e.key, e.strVal ?? "");
                }
                PlayerPrefs.Save();
                return $"스냅샷 적용 {snap.entries.Length}키 — 재시작 권장";
            }
            catch (Exception ex)
            {
                return "가져오기 실패: " + ex.Message;
            }
        }

        string BuildSnapshotJson()
        {
            // PlayerPrefs has no enumerate API — export known last snapshot merge + critical keys we touch.
            // Practical V1: store a meta blob listing keys written via tracked list in PlayerPrefs string registry.
            string registry = PlayerPrefs.GetString("IdleGrow.SaveSnapshot.KeyRegistry", "");
            var keys = registry.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new System.Collections.Generic.List<SnapEntry>();
            for (int i = 0; i < keys.Length; i++)
            {
                string k = keys[i].Trim();
                if (string.IsNullOrEmpty(k) || !PlayerPrefs.HasKey(k)) continue;
                // Prefer string; fall back int/float guesses via Try
                list.Add(new SnapEntry { key = k, kind = 2, strVal = PlayerPrefs.GetString(k, "") });
            }
            // Always include wallet/growth core blobs if present as strings
            TryAddString(list, "IdleMvp.LootBox");
            TryAddString(list, "IdleMvp.PlayerGrowth");
            TryAddString(list, "IdleGrow.Dungeon");
            TryAddString(list, "IdleGrow.Maple.Mail");
            var snap = new Snapshot
            {
                schema = SchemaVersion,
                exportedUtcTicks = DateTime.UtcNow.Ticks,
                entries = list.ToArray()
            };
            return JsonUtility.ToJson(snap);
        }

        static void TryAddString(System.Collections.Generic.List<SnapEntry> list, string key)
        {
            if (!PlayerPrefs.HasKey(key)) return;
            for (int i = 0; i < list.Count; i++)
                if (list[i].key == key) return;
            list.Add(new SnapEntry { key = key, kind = 2, strVal = PlayerPrefs.GetString(key, "") });
        }

        /// <summary>Call when writing important prefs so export can find them.</summary>
        public static void RegisterKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            string reg = PlayerPrefs.GetString("IdleGrow.SaveSnapshot.KeyRegistry", "");
            if (reg.IndexOf(key, StringComparison.Ordinal) >= 0) return;
            PlayerPrefs.SetString("IdleGrow.SaveSnapshot.KeyRegistry", reg + key + "\n");
        }

        [Serializable]
        class Snapshot
        {
            public int schema;
            public long exportedUtcTicks;
            public SnapEntry[] entries;
        }

        [Serializable]
        class SnapEntry
        {
            public string key;
            public int kind; // 0 int 1 float 2 string
            public int intVal;
            public float floatVal;
            public string strVal;
        }
    }
}
