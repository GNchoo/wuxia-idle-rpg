using System;
using IdleMvp.Economy;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Gold/RD facade. Template WalletManager is canonical when bound.
    /// </summary>
    public class WalletAdapter : MonoBehaviour
    {
        public static WalletAdapter Instance { get; private set; }

        public event Action OnChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (CurrencyWallet.Instance != null)
                CurrencyWallet.Instance.OnChanged += () => OnChanged?.Invoke();
        }

        public double Gold
        {
            get
            {
                var host = TemplateFeatureHost.Instance;
                if (host != null && host.HasTemplateWallet)
                    return TemplateFeatureHost.ReadFloatField(host.WalletManager, "GoldWalletValue");
                return CurrencyWallet.Instance != null ? CurrencyWallet.Instance.Get(CurrencyId.Gold) : 0;
            }
        }

        public double RedDiamond
        {
            get
            {
                var host = TemplateFeatureHost.Instance;
                if (host != null && host.HasTemplateWallet)
                    return TemplateFeatureHost.ReadFloatField(host.WalletManager, "GemWalletValue");
                return CurrencyWallet.Instance != null ? CurrencyWallet.Instance.Get(CurrencyId.RedDiamond) : 0;
            }
        }

        public void AddGold(double amount)
        {
            if (amount <= 0) return;
            var host = TemplateFeatureHost.Instance;
            if (host != null && host.HasTemplateWallet)
            {
                float cur = TemplateFeatureHost.ReadFloatField(host.WalletManager, "GoldWalletValue");
                TemplateFeatureHost.WriteFloatField(host.WalletManager, "GoldWalletValue", cur + (float)amount);
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletValueManualUpdate");
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletDataSave");
            }
            CurrencyWallet.Instance?.Add(CurrencyId.Gold, amount);
            PlayerWallet.Instance?.SyncFromAdapter();
            OnChanged?.Invoke();
        }

        public bool TrySpendGold(double amount)
        {
            if (amount <= 0) return true;
            if (Gold < amount) return false;
            var host = TemplateFeatureHost.Instance;
            if (host != null && host.HasTemplateWallet)
            {
                float cur = TemplateFeatureHost.ReadFloatField(host.WalletManager, "GoldWalletValue");
                TemplateFeatureHost.WriteFloatField(host.WalletManager, "GoldWalletValue", cur - (float)amount);
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletValueManualUpdate");
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletDataSave");
            }
            CurrencyWallet.Instance?.TrySpend(CurrencyId.Gold, amount);
            PlayerWallet.Instance?.SyncFromAdapter();
            OnChanged?.Invoke();
            return true;
        }

        public void AddRedDiamond(double amount)
        {
            if (amount <= 0) return;
            var host = TemplateFeatureHost.Instance;
            if (host != null && host.HasTemplateWallet)
            {
                float cur = TemplateFeatureHost.ReadFloatField(host.WalletManager, "GemWalletValue");
                TemplateFeatureHost.WriteFloatField(host.WalletManager, "GemWalletValue", cur + (float)amount);
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletValueManualUpdate");
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletDataSave");
            }
            CurrencyWallet.Instance?.Add(CurrencyId.RedDiamond, amount);
            OnChanged?.Invoke();
        }

        public bool TrySpendRedDiamond(double amount)
        {
            if (amount <= 0) return true;
            if (RedDiamond < amount) return false;
            var host = TemplateFeatureHost.Instance;
            if (host != null && host.HasTemplateWallet)
            {
                float cur = TemplateFeatureHost.ReadFloatField(host.WalletManager, "GemWalletValue");
                TemplateFeatureHost.WriteFloatField(host.WalletManager, "GemWalletValue", cur - (float)amount);
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletValueManualUpdate");
                TemplateFeatureHost.CallMethod(host.WalletManager, "WalletDataSave");
            }
            CurrencyWallet.Instance?.TrySpend(CurrencyId.RedDiamond, amount);
            OnChanged?.Invoke();
            return true;
        }

        public void PullFromTemplate()
        {
            var host = TemplateFeatureHost.Instance;
            if (host == null || !host.HasTemplateWallet || CurrencyWallet.Instance == null) return;
            CurrencyWallet.Instance.Set(CurrencyId.Gold, Gold, false);
            CurrencyWallet.Instance.Set(CurrencyId.RedDiamond, RedDiamond, false);
            PlayerWallet.Instance?.SyncFromAdapter();
            OnChanged?.Invoke();
        }
    }
}
