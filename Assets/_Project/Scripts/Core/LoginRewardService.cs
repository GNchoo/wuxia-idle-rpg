using System;
using UnityEngine;

namespace IdleMvp.Core
{
    public static class LoginRewardService
    {
        const string KeyLastLogin = "IdleGrow.Login.LastDate";
        const string KeyStreak    = "IdleGrow.Login.Streak";
        const string KeyReturnSent = "IdleGrow.Login.ReturnSent";
        const int ReturnThresholdDays = 3;

        public static int Streak { get; private set; } = 1;
        public static bool IsReturnLogin { get; private set; }

        public static float StreakMultiplier
        {
            get
            {
                if (Streak >= 7) return 2.0f;
                if (Streak >= 5) return 1.5f;
                if (Streak >= 3) return 1.2f;
                return 1.0f;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnBoot()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            string last = PlayerPrefs.GetString(KeyLastLogin, "");

            if (last == today)
            {
                Streak = Mathf.Max(1, PlayerPrefs.GetInt(KeyStreak, 1));
                IsReturnLogin = false;
                return;
            }

            int gap = 0;
            if (!string.IsNullOrEmpty(last) &&
                DateTime.TryParseExact(last, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var prev))
            {
                gap = (int)(DateTime.UtcNow.Date - prev.Date).TotalDays;
            }

            if (gap == 1)
            {
                Streak = PlayerPrefs.GetInt(KeyStreak, 1) + 1;
                IsReturnLogin = false;
            }
            else if (gap >= ReturnThresholdDays)
            {
                Streak = 1;
                IsReturnLogin = true;
                SendReturnMail(gap);
            }
            else
            {
                Streak = 1;
                IsReturnLogin = false;
            }

            PlayerPrefs.SetString(KeyLastLogin, today);
            PlayerPrefs.SetInt(KeyStreak, Streak);
            PlayerPrefs.Save();
        }

        static void SendReturnMail(int absentDays)
        {
            string sentKey = KeyReturnSent + DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetInt(sentKey, 0) == 1) return;

            var mail = Economy.MailService.Instance;
            if (mail == null) return;

            double goldReward = 5000 + absentDays * 1000;
            double rdReward = 10 + absentDays * 2;
            if (goldReward > 30000) goldReward = 30000;
            if (rdReward > 50) rdReward = 50;

            mail.Send(
                "복귀 보상",
                $"{absentDays}일 만에 돌아오셨네요!\n다시 만나서 반갑습니다.",
                gold: goldReward,
                rd: rdReward,
                extra: Economy.CurrencyId.WeaponTicket,
                extraAmt: Mathf.Min(absentDays, 10)
            );

            PlayerPrefs.SetInt(sentKey, 1);
            PlayerPrefs.Save();
        }
    }
}
