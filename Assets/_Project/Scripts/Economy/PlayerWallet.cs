using System;
using IdleMvp.Adapters;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>
    /// Grow gold facade — delegates to WalletAdapter / CurrencyWallet (template Gold when bound).
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        public static PlayerWallet Instance { get; private set; }

        public double Gold => WalletAdapter.Instance != null
            ? WalletAdapter.Instance.Gold
            : (CurrencyWallet.Instance != null ? CurrencyWallet.Instance.Get(CurrencyId.Gold) : _legacyGold);

        public event Action OnChanged;

        double _legacyGold = 100;
        const string PrefKey = "IdleGrow.Wallet";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (!PlayerPrefs.HasKey("IdleGrow.Maple.Currency") && PlayerPrefs.HasKey(PrefKey))
            {
                if (double.TryParse(PlayerPrefs.GetString(PrefKey), out var g))
                    _legacyGold = g;
            }
        }

        void Start()
        {
            if (WalletAdapter.Instance != null)
                WalletAdapter.Instance.OnChanged += () => OnChanged?.Invoke();
            if (CurrencyWallet.Instance != null && CurrencyWallet.Instance.Get(CurrencyId.Gold) <= 0 && _legacyGold > 0)
                CurrencyWallet.Instance.Set(CurrencyId.Gold, _legacyGold);
        }

        public void SyncFromAdapter() => OnChanged?.Invoke();

        public void AddGold(double amount)
        {
            if (WalletAdapter.Instance != null)
                WalletAdapter.Instance.AddGold(amount);
            else if (CurrencyWallet.Instance != null)
                CurrencyWallet.Instance.Add(CurrencyId.Gold, amount);
            else
            {
                _legacyGold += amount;
                OnChanged?.Invoke();
            }
        }

        public bool TrySpendGold(double amount)
        {
            if (WalletAdapter.Instance != null)
                return WalletAdapter.Instance.TrySpendGold(amount);
            if (CurrencyWallet.Instance != null)
                return CurrencyWallet.Instance.TrySpend(CurrencyId.Gold, amount);
            if (_legacyGold < amount) return false;
            _legacyGold -= amount;
            OnChanged?.Invoke();
            return true;
        }
    }
}
