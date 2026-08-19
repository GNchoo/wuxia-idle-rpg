using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.UI
{
    /// <summary>
    /// Local audio / settings toggles persisted in PlayerPrefs.
    /// </summary>
    public static class GameSettings
    {
        const string BgmKey = "IdleGrow.Settings.Bgm";
        const string SfxKey = "IdleGrow.Settings.Sfx";
        const string SpeedKey = "IdleGrow.Settings.Speed";

        public static int SpeedLevel
        {
            get => PlayerPrefs.GetInt(SpeedKey, 1);
            set
            {
                PlayerPrefs.SetInt(SpeedKey, value);
                PlayerPrefs.Save();
                ApplySpeed();
            }
        }

        public static string ToggleSpeed()
        {
            SpeedLevel = SpeedLevel >= 2 ? 1 : 2;
            return $"전투 속도 x{SpeedLevel}";
        }

        public static void ApplySpeed()
        {
            Time.timeScale = SpeedLevel;
        }

        public static bool BgmEnabled
        {
            get => PlayerPrefs.GetInt(BgmKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(BgmKey, value ? 1 : 0);
                PlayerPrefs.Save();
                ApplyAudio();
            }
        }

        public static bool SfxEnabled
        {
            get => PlayerPrefs.GetInt(SfxKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SfxKey, value ? 1 : 0);
                PlayerPrefs.Save();
                ApplyAudio();
            }
        }

        public static string ToggleBgm()
        {
            BgmEnabled = !BgmEnabled;
            return BgmEnabled ? "BGM ON" : "BGM OFF";
        }

        public static string ToggleSfx()
        {
            SfxEnabled = !SfxEnabled;
            return SfxEnabled ? "SFX ON" : "SFX OFF";
        }

        public static void ApplyAudio()
        {
            AudioListener.pause = false;
            AudioListener.volume = 1f;
            AudioService.Apply();
        }

        public static string OpenSettingsSummary()
        {
            ApplyAudio();
            return
                $"설정 · BGM {(BgmEnabled ? "ON" : "OFF")} · SFX {(SfxEnabled ? "ON" : "OFF")} · 속도 x{SpeedLevel}\n" +
                $"실 IAP/Ads {(BmRuntimeFlags.UseRealIapAds ? "ON" : "OFF(mock)")}\n" +
                "데이터 초기화는 에디터 디버그 메뉴에서만 가능합니다.";
        }

        /// <summary>Wipe local IdleGrow / IdleMvp prefs (dev / support).</summary>
        public static string WipeLocalSave()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            return "로컬 세이브 삭제됨 — 재시작 필요";
        }
    }
}
