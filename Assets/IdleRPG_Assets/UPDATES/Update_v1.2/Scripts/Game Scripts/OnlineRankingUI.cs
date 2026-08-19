using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Account.Manager.Profile;

namespace SAMPLETEXT
{
    public class OnlineRankingUI : MonoBehaviour
    {
        [Header("Server")]
        [SerializeField] private string leaderboardUrl = "https://your-domain.com/idle_api/leaderboard.php";

        [Header("UI")]
        [SerializeField] private GameObject panel;     // RankingPanel
        [SerializeField] private Transform rowsParent; // Content from ScrollView
        [SerializeField] private GameObject rowPrefab; // LeaderboardRow prefab
        [SerializeField] private TextMeshProUGUI selfRowText; // text of our position

        [Header("Game Refs")]
        [SerializeField] private WalletManagerScript wallet;
        [SerializeField] private ProfileManagerScript profile;

        private enum Metric { Dps, Gold, Gems }
        private Metric currentMetric = Metric.Dps;

        [Serializable]
        public class LeaderboardEntry
        {
            public string display_name;
            public double max_gold;
            public double max_gems;
            public double max_dps;
        }

        [Serializable]
        public class LeaderboardResponse
        {
            public LeaderboardEntry[] entries;
        }

        // ===== PUBLIC API – add to your buttons =====

        public void OpenRanking()
        {
            panel.SetActive(true);
            currentMetric = Metric.Dps;
            StartCoroutine(LoadLeaderboard(currentMetric));
            UpdateSelfRow();
        }

        public void CloseRanking()
        {
            panel.SetActive(false);
            ClearRows();
        }

        public void OnClickDps()
        {
            currentMetric = Metric.Dps;
            StartCoroutine(LoadLeaderboard(currentMetric));
            UpdateSelfRow();
        }

        public void OnClickGold()
        {
            currentMetric = Metric.Gold;
            StartCoroutine(LoadLeaderboard(currentMetric));
            UpdateSelfRow();
        }

        public void OnClickGems()
        {
            currentMetric = Metric.Gems;
            StartCoroutine(LoadLeaderboard(currentMetric));
            UpdateSelfRow();
        }

        // ===== Load from web =====

        private IEnumerator LoadLeaderboard(Metric metric)
        {
            ClearRows();

            string metricParam = "dps";
            switch (metric)
            {
                case Metric.Gold: metricParam = "gold"; break;
                case Metric.Gems: metricParam = "gems"; break;
                case Metric.Dps: metricParam = "dps"; break;
            }

            string url = leaderboardUrl + "?metric=" + metricParam;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("[RankingUI] Error: " + www.error);
                    yield break;
                }

                string json = www.downloadHandler.text;
                Debug.Log("[RankingUI] Raw JSON: " + json);

                if (string.IsNullOrEmpty(json))
                    yield break;

                // Jeśli serwer zwrócił czystą tablicę:  [ {...}, {...} ]
                // to opakujemy ją w obiekt, którego oczekuje JsonUtility
                string trimmed = json.TrimStart();
                if (trimmed.StartsWith("["))
                {
                    json = "{\"entries\":" + json + "}";
                }

                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);
                if (response == null || response.entries == null)
                {
                    Debug.LogWarning("[RankingUI] Parsed response is null or has no entries.");
                    yield break;
                }

                string selfName = profile != null ? profile.PlayerNameStringValue : "";
                int selfRank = -1;

                int rank = 1;
                foreach (var e in response.entries)
                {
                    double val = 0;
                    switch (metric)
                    {
                        case Metric.Gold: val = e.max_gold; break;
                        case Metric.Gems: val = e.max_gems; break;
                        case Metric.Dps: val = e.max_dps; break;
                    }

                    CreateRow(rank, e.display_name, val);

                    if (!string.IsNullOrEmpty(selfName) && e.display_name == selfName && selfRank == -1)
                    {
                        selfRank = rank;
                    }

                    rank++;
                }

                UpdateSelfRow(selfRank);
            }
        }

        private void CreateRow(int rank, string name, double value)
        {
            GameObject row = Instantiate(rowPrefab, rowsParent);
            TextMeshProUGUI text = row.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{rank}. {name} | {value:N0}";
            }
        }

        private void ClearRows()
        {
            for (int i = rowsParent.childCount - 1; i >= 0; i--)
            {
                Destroy(rowsParent.GetChild(i).gameObject);
            }
        }

        // ===== Our text at the bottom =====

        private void UpdateSelfRow(int rankFromList = -1)
        {
            if (selfRowText == null || wallet == null || profile == null)
                return;

            string name = profile.PlayerNameStringValue;
            double val = 0;

            switch (currentMetric)
            {
                case Metric.Gold: val = wallet.GoldWalletValue; break;
                case Metric.Gems: val = wallet.GemWalletValue; break;
                case Metric.Dps: val = wallet.DPSWalletValue; break;
            }

            string rankText = (rankFromList > 0) ? rankFromList.ToString() : "-";

            selfRowText.text = $"{rankText}. {name} | {val:N0}";
        }
    }
}