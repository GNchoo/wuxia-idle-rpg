using System;
using UnityEngine;

namespace IdleMvp.Core
{
    public class FatedEventService : MonoBehaviour
    {
        public static FatedEventService Instance { get; private set; }
        public static event Action OnFatedEvent;

        const string PrefUnlocked = "IdleGrow.Fated.Unlocked";
        const string PrefDismissed = "IdleGrow.Fated.Dismissed";
        const float CheckInterval = 60f;
        // 0.0001(0.01%)로 뒀더니 기댓값 167시간 — 게임 수명 안에 아무도 못 본다.
        // 0.5%/분이면 기댓값 ~3.3시간 방치: '기연'답게 드물지만 실제로 겪는 수치.
        const float Chance = 0.005f;

        public bool IsUnlocked { get; private set; }
        public bool IsPending { get; private set; }

        float _timer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            IsUnlocked = PlayerPrefs.GetInt(PrefUnlocked, 0) == 1;
        }

        void Update()
        {
            if (IsUnlocked || IsPending) return;
            var hunt = Combat.FieldAutoHuntController.Instance;
            if (hunt == null || !hunt.IsFieldBound) return;
            if (hunt.Mode != Combat.CombatMode.Hunt) return;

            _timer += Time.deltaTime;
            if (_timer < CheckInterval) return;
            _timer = 0f;

            if (UnityEngine.Random.value < Chance)
            {
                IsPending = true;
                OnFatedEvent?.Invoke();
            }
        }

        public void Accept()
        {
            if (!IsPending) return;
            IsPending = false;
            IsUnlocked = true;
            PlayerPrefs.SetInt(PrefUnlocked, 1);
            PlayerPrefs.Save();
        }

        public void Dismiss()
        {
            if (!IsPending) return;
            IsPending = false;
            PlayerPrefs.SetInt(PrefDismissed, PlayerPrefs.GetInt(PrefDismissed, 0) + 1);
            PlayerPrefs.Save();
            Economy.CurrencyWallet.Instance?.Add(Economy.CurrencyId.BlueDiamond, 50);
        }

        public static string HiddenJobName
        {
            get
            {
                switch (FactionService.Selected)
                {
                    case "hero":      return "검선";
                    case "bowmaster": return "만독불침";
                    case "archmage":  return "천마재림";
                    default:          return "무극대도";
                }
            }
        }
    }
}
