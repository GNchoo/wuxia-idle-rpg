using System;
using IdleMvp.Core;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>IAP purchase gate — mock success or template bridge placeholder.</summary>
    public class IapBridge : MonoBehaviour
    {
        public static IapBridge Instance { get; private set; }

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Purchase(string productId, Action onSuccess, Action<string> onFail = null)
        {
            if (string.IsNullOrEmpty(productId))
            {
                onFail?.Invoke("productId 없음");
                return;
            }

            if (!BmRuntimeFlags.UseRealIapAds)
            {
                Debug.Log($"[IapBridge] Mock purchase: {productId} ({IapProductCatalog.DisplayName(productId)})");
                OnPurchaseSucceeded?.Invoke(productId);
                onSuccess?.Invoke();
                return;
            }

            // Real path: wire Template DiamondPurchaseManager when available.
            try
            {
                var host = IdleMvp.Adapters.TemplateFeatureHost.Instance;
                if (host != null && host.WalletManager != null)
                {
                    // Fallback mock until template IAP is bound per build.
                    Debug.LogWarning("[IapBridge] UseRealIapAds ON but template IAP not bound — mock grant.");
                    OnPurchaseSucceeded?.Invoke(productId);
                    onSuccess?.Invoke();
                    return;
                }
            }
            catch (Exception e)
            {
                OnPurchaseFailed?.Invoke(productId, e.Message);
                onFail?.Invoke(e.Message);
                return;
            }

            OnPurchaseFailed?.Invoke(productId, "IAP 미연결");
            onFail?.Invoke("IAP 미연결");
        }
    }

    /// <summary>Rewarded ad gate — mock success or template ads placeholder.</summary>
    public class AdBridge : MonoBehaviour
    {
        public static AdBridge Instance { get; private set; }

        public event Action<string> OnRewarded;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowRewarded(string placement, Action onReward, Action<string> onFail = null)
        {
            if (MembershipService.Instance != null && MembershipService.Instance.RemoveAds)
            {
                // Membership skips ads → still grant reward
                OnRewarded?.Invoke(placement);
                onReward?.Invoke();
                return;
            }

            if (!BmRuntimeFlags.UseRealIapAds)
            {
                Debug.Log("[AdBridge] Mock rewarded: " + placement);
                OnRewarded?.Invoke(placement);
                onReward?.Invoke();
                return;
            }

            Debug.LogWarning("[AdBridge] UseRealIapAds ON but Ads SDK not bound — mock reward.");
            OnRewarded?.Invoke(placement);
            onReward?.Invoke();
        }
    }
}
