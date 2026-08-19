using UnityEngine;

namespace IdleMvp.Core
{
    public enum ContentId
    {
        Guild,
        Mail,
        Arena,
        Raid,
        CostumeBeauty,
        MapSelect,
        Shop,
        Pass,
        Event,
        Dungeon,
        Chat,
        HotDeal
    }

    /// <summary>Feature readiness for HUD / menu gating (V1 soft-launch).</summary>
    public static class FeatureGate
    {
        static readonly ContentId[] ReadyDefaults =
        {
            ContentId.Guild,
            ContentId.Mail,
            ContentId.MapSelect,
            ContentId.Shop,
            ContentId.Pass,
            ContentId.Event,
            ContentId.Dungeon,
            ContentId.CostumeBeauty,
            ContentId.Arena,
            ContentId.Raid,
            ContentId.HotDeal,
            ContentId.Chat
        };

        public static bool IsReady(ContentId id)
        {
            for (int i = 0; i < ReadyDefaults.Length; i++)
                if (ReadyDefaults[i] == id) return true;
            return false;
        }

        public static string DisplayName(ContentId id)
        {
            switch (id)
            {
                case ContentId.Guild: return "길드";
                case ContentId.Mail: return "우편";
                case ContentId.Arena: return "아레나";
                case ContentId.Raid: return "레이드";
                case ContentId.CostumeBeauty: return "코스튬";
                case ContentId.MapSelect: return "맵 선택";
                case ContentId.Shop: return "상점";
                case ContentId.Pass: return "패스";
                case ContentId.Event: return "이벤트";
                case ContentId.Dungeon: return "성장 던전";
                case ContentId.Chat: return "채팅";
                case ContentId.HotDeal: return "핫딜";
                default: return id.ToString();
            }
        }

        public static string ComingSoonBody(ContentId id) =>
            $"{DisplayName(id)} 콘텐츠 준비 중입니다.";

        /// <summary>Delegate to FeatureClaims when available (UI layer); Core callers get this stub.</summary>
        public static System.Func<ContentId, bool> ClaimableCheck;
        public static bool HasClaimable(ContentId id) => ClaimableCheck?.Invoke(id) ?? false;
    }

    /// <summary>Runtime BM flags (mock IAP/Ads vs template bridge).</summary>
    public static class BmRuntimeFlags
    {
        const string PrefUseReal = "IdleGrow.BM.UseRealIapAds";
        const string PrefDebugCheats = "IdleGrow.BM.AllowDebugCheats";

        public static bool UseRealIapAds
        {
            get => PlayerPrefs.GetInt(PrefUseReal, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(PrefUseReal, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool AllowDebugCheats
        {
            get
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                return PlayerPrefs.GetInt(PrefDebugCheats, 1) == 1;
#else
                return PlayerPrefs.GetInt(PrefDebugCheats, 0) == 1;
#endif
            }
            set
            {
                PlayerPrefs.SetInt(PrefDebugCheats, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
