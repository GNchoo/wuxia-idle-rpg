using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// Lightweight audio: one looping BGM source + one one-shot SFX source.
    /// Clips load lazily from Resources/Audio. Honors GameSettings toggles.
    /// ponytail: single SFX source (PlayOneShot mixes internally) — pool only if voices clip.
    /// </summary>
    public static class AudioService
    {
        static AudioSource _bgm;
        static AudioSource _sfx;
        static readonly System.Collections.Generic.Dictionary<string, AudioClip> _clips = new();

        public static void EnsureRoot()
        {
            if (_bgm != null) return;
            var go = new GameObject("AudioService");
            Object.DontDestroyOnLoad(go);
            _bgm = go.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            _bgm.volume = 0.45f;
            _sfx = go.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.volume = 0.9f;
            Apply();
        }

        static AudioClip Clip(string name)
        {
            if (_clips.TryGetValue(name, out var c)) return c;
            c = Resources.Load<AudioClip>("Audio/" + name);
            _clips[name] = c; // cache nulls too — no repeated misses
            return c;
        }

        /// <summary>Sync sources with GameSettings (called by GameSettings.ApplyAudio).</summary>
        public static void Apply()
        {
            if (_bgm == null) return;
            _bgm.mute = !IdleMvp.UI.GameSettings.BgmEnabled;
            if (IdleMvp.UI.GameSettings.BgmEnabled && !_bgm.isPlaying) PlayBgm();
        }

        public static void PlayBgm()
        {
            EnsureRoot();
            var clip = Clip("Bgm");
            if (clip == null) return;
            if (_bgm.clip != clip) _bgm.clip = clip;
            if (!_bgm.isPlaying) _bgm.Play();
        }

        public static void Sfx(string name, float volume = 1f)
        {
            if (!IdleMvp.UI.GameSettings.SfxEnabled) return;
            EnsureRoot();
            var clip = Clip(name);
            if (clip != null) _sfx.PlayOneShot(clip, volume);
        }

        public static void Click() => Sfx("Click", 0.7f);
        public static void Open() => Sfx("UiOpen", 0.8f);
        public static void Close() => Sfx("UiClose", 0.8f);
        public static void Gold() => Sfx("Gold");
        public static void Gem() => Sfx("Gem");
        public static void Hit() => Sfx("Hit", 0.55f);
        public static void Death() => Sfx("Death", 0.65f);
        public static void Skill(int idx) => Sfx("Skill" + Mathf.Clamp(idx, 0, 3));

        // ---- 보스 패턴음 -----------------------------------------------------
        // 전용 클립 없이 기존 클립을 피치로 변조한다. _sfx 소스의 피치를 건드리면
        // 겹쳐 나가는 다른 소리까지 변하므로 변조 전용 소스를 따로 둔다.
        static AudioSource _pitched;

        static void SfxPitched(string name, float pitch, float volume)
        {
            if (!IdleMvp.UI.GameSettings.SfxEnabled) return;
            EnsureRoot();
            if (_pitched == null)
            {
                _pitched = _sfx.gameObject.AddComponent<AudioSource>();
                _pitched.playOnAwake = false;
                _pitched.volume = 0.9f;
            }
            var clip = Clip(name);
            if (clip == null) return;
            _pitched.pitch = pitch;
            _pitched.PlayOneShot(clip, volume);
        }

        /// <summary>보스 강타 — 낮고 무겁게.</summary>
        public static void BossHeavy() => SfxPitched("Hit", 0.55f, 1f);
        /// <summary>보스 광역 슬램 — 스킬음을 깔아 울린다.</summary>
        public static void BossSlam() => SfxPitched("Skill3", 0.6f, 0.9f);
        /// <summary>보스 분노 진입 — 날카롭게 알린다.</summary>
        public static void BossEnrage() => SfxPitched("Skill0", 1.4f, 0.9f);
    }
}
