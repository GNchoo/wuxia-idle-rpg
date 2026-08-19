using IdleMvp.Adapters;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// Timed main+sub companion damage pulses on the hunt field.
    /// Visual actors are owned by FieldAutoHuntController.
    /// </summary>
    public class CompanionCombatBridge : MonoBehaviour
    {
        public static CompanionCombatBridge Instance { get; private set; }

        public float Remaining { get; private set; }
        public float Cooldown { get; private set; }
        public bool IsActive => Remaining > 0f;

        const float Duration = 30f;
        const float Cd = 45f;
        const float Tick = 0.6f;
        float _tick;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            if (Cooldown > 0f) Cooldown -= Time.deltaTime;

            var comp = CompanionAdapter.Instance;
            if (comp == null || comp.Main == null) return;

            if (Remaining > 0f)
            {
                Remaining -= Time.deltaTime;
                _tick -= Time.deltaTime;
                return;
            }

            if (comp.AutoSummon && Cooldown <= 0f)
                TrySummon(out _);
        }

        /// <summary>Start field sortie window. Returns false with Korean reason on failure.</summary>
        public bool TrySummon(out string message)
        {
            if (CompanionAdapter.Instance?.Main == null)
            {
                message = "메인을 먼저 지정하세요";
                return false;
            }
            if (Cooldown > 0f && Remaining <= 0f)
            {
                message = $"쿨다운 {Cooldown:0}초";
                return false;
            }
            Remaining = Duration;
            Cooldown = Cd;
            _tick = 0f;
            FieldAutoHuntController.Instance?.SyncCompanionActors(force: true);
            message = $"{CompanionAdapter.Instance.Main.name} 필드 출전 ({Duration:0}초)";
            return true;
        }

        /// <summary>Legacy no-op-safe entry (prefer TrySummon).</summary>
        public void Summon() => TrySummon(out _);

        public bool TryConsumePulseDamage(out float damage)
        {
            damage = 0f;
            if (Remaining <= 0f) return false;
            if (_tick > 0f) return false;
            _tick = Tick;
            float atk = CombatPowerService.GetAtk();
            var ca = CompanionAdapter.Instance;
            float ratio = ca != null ? ca.MainInheritRatio : 0.3f;
            float sub = ca != null ? ca.SubInheritRatio : 0f;
            damage = Mathf.Max(1f, atk * (ratio + sub) * 1.2f);
            return true;
        }
    }
}
