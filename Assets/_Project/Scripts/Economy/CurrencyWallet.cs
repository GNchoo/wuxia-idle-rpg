using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.Economy
{
    public enum CurrencyId
    {
        Gold = 0,
        RedDiamond = 1,
        WeaponTicket = 2,
        CompanionTicket = 3,
        MonsterPoint = 4,
        ArmorStone = 5,
        WeaponEnhanceStone = 6,
        ScrollTrace = 7,
        StarForceScroll = 8,
        MiracleCube = 9,
        AdditionalCube = 10,
        BlueDiamond = 11,
        TrainingToken = 12,   // 수련 증표 — 수련 트랙 강화
        HonorMedal = 13       // 명성 훈장 — 어빌리티 리롤
    }

    /// <summary>
    /// Extended currencies. Gold/RD sync with template Wallet when bound.
    /// </summary>
    public class CurrencyWallet : MonoBehaviour
    {
        public static CurrencyWallet Instance { get; private set; }

        readonly Dictionary<CurrencyId, double> _bal = new Dictionary<CurrencyId, double>();
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Currency";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            foreach (CurrencyId id in Enum.GetValues(typeof(CurrencyId)))
                _bal[id] = 0;
            Load();
            if (_bal[CurrencyId.Gold] <= 0) _bal[CurrencyId.Gold] = 100;
            if (_bal[CurrencyId.RedDiamond] <= 0) _bal[CurrencyId.RedDiamond] = 50;
            if (_bal[CurrencyId.WeaponTicket] <= 0) _bal[CurrencyId.WeaponTicket] = 5;
            if (_bal[CurrencyId.CompanionTicket] <= 0) _bal[CurrencyId.CompanionTicket] = 3;
            if (_bal[CurrencyId.ScrollTrace] <= 0) _bal[CurrencyId.ScrollTrace] = 20;
            if (_bal[CurrencyId.StarForceScroll] <= 0) _bal[CurrencyId.StarForceScroll] = 5;
            if (_bal[CurrencyId.MiracleCube] <= 0) _bal[CurrencyId.MiracleCube] = 3;
        }

        public double Get(CurrencyId id) => _bal.TryGetValue(id, out var v) ? v : 0;

        public void Set(CurrencyId id, double value, bool notify = true)
        {
            _bal[id] = Math.Max(0, value);
            Save();
            if (notify) OnChanged?.Invoke();
        }

        public void Add(CurrencyId id, double amount)
        {
            if (amount <= 0) return;
            _bal[id] = Get(id) + amount;
            Save();
            OnChanged?.Invoke();
        }

        public bool TrySpend(CurrencyId id, double amount)
        {
            if (amount <= 0) return true;
            if (Get(id) < amount) return false;
            _bal[id] = Get(id) - amount;
            Save();
            OnChanged?.Invoke();
            return true;
        }

        void Save()
        {
            var parts = new List<string>();
            foreach (var kv in _bal)
                parts.Add(((int)kv.Key) + "=" + kv.Value.ToString("R"));
            PlayerPrefs.SetString(PrefKey, string.Join(";", parts));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var raw = PlayerPrefs.GetString(PrefKey, "");
            foreach (var part in raw.Split(';'))
            {
                var kv = part.Split('=');
                if (kv.Length != 2) continue;
                if (!int.TryParse(kv[0], out var id)) continue;
                if (!double.TryParse(kv[1], out var v)) continue;
                if (Enum.IsDefined(typeof(CurrencyId), id))
                    _bal[(CurrencyId)id] = v;
            }
        }
    }
}
