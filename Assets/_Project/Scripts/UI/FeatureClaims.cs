using IdleMvp.Adapters;
using IdleMvp.Core;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.UI
{
    /// <summary>Claimable badges without putting Economy refs into FeatureGate (Core).</summary>
    public static class FeatureClaims
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register() => Core.FeatureGate.ClaimableCheck = HasClaimable;

        public static bool HasClaimable(ContentId id)
        {
            switch (id)
            {
                case ContentId.Mail:
                    return MailService.Instance != null && MailService.Instance.UnreadCount > 0;
                case ContentId.Pass:
                    return PassService.Instance != null && PassService.Instance.HasClaimable;
                case ContentId.Event:
                    return CanClaimAttendToday();
                case ContentId.Guild:
                    return GuildAdapter.Instance != null && GuildAdapter.Instance.HasDailyReward;
                default:
                    return false;
            }
        }

        static bool CanClaimAttendToday()
        {
            string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            string last = PlayerPrefs.GetString("IdleGrow.Maple.AttendLastDay", "");
            return last != today;
        }
    }
}
