using System;
using IdleMvp.Economy;
using IdleMvp.Progression;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Binds SAMPLETEXT managers when present in loaded scenes; otherwise adapters use mirrors.
    /// Does not load GameScene combat / Letterbox.
    /// </summary>
    public class TemplateFeatureHost : MonoBehaviour
    {
        public static TemplateFeatureHost Instance { get; private set; }

        public UnityEngine.Object WalletManager { get; private set; }
        public UnityEngine.Object InventoryManager { get; private set; }
        public UnityEngine.Object SubHeroManager { get; private set; }
        public UnityEngine.Object SkillsManager { get; private set; }
        public UnityEngine.Object TalentsManager { get; private set; }
        public UnityEngine.Object HeroPurchaseManager { get; private set; }
        public bool HasTemplateWallet => WalletManager != null;
        public bool HasTemplateInventory => InventoryManager != null;
        public bool HasTemplateSubHero => SubHeroManager != null;
        public bool HasTemplateSkills => SkillsManager != null;

        public event Action OnBound;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            Rebind();
        }

        public void Rebind()
        {
            WalletManager = FindByTypeName("SAMPLETEXT.Wallet.Manager.WalletManagerScript");
            InventoryManager = FindByTypeName("SAMPLETEXT.Inventory.Manager.InventoryManagerScript");
            SubHeroManager = FindByTypeName("SAMPLETEXT.SubHeroUI.Manager.SubHeroesManagerScript");
            SkillsManager = FindByTypeName("SAMPLETEXT.Gameplay.Skills.Manager.GameplaySkillsManagerScript");
            TalentsManager = FindByTypeName("SAMPLETEXT.Talent.Manager.TalentsManagerScript");
            HeroPurchaseManager = FindByTypeName("SAMPLETEXT.ItemPurchase.Manager.Item.ItemsPurchaseHeroManagerScript");

            if (HasTemplateWallet)
                WalletAdapter.Instance?.PullFromTemplate();

            Debug.Log($"[IdleGrow] TemplateFeatureHost bound wallet={HasTemplateWallet} inv={HasTemplateInventory} sub={HasTemplateSubHero} skills={HasTemplateSkills}");
            OnBound?.Invoke();
        }

        static UnityEngine.Object FindByTypeName(string fullName)
        {
            foreach (var mb in FindObjectsOfType<MonoBehaviour>())
            {
                if (mb == null) continue;
                if (mb.GetType().FullName == fullName) return mb;
            }
            return null;
        }

        public static float ReadFloatField(UnityEngine.Object obj, string field)
        {
            if (obj == null) return 0f;
            var f = obj.GetType().GetField(field);
            if (f == null) return 0f;
            var v = f.GetValue(obj);
            if (v is float fl) return fl;
            if (v is int i) return i;
            if (v is double d) return (float)d;
            return 0f;
        }

        public static void WriteFloatField(UnityEngine.Object obj, string field, float value)
        {
            if (obj == null) return;
            var f = obj.GetType().GetField(field);
            if (f == null) return;
            if (f.FieldType == typeof(float)) f.SetValue(obj, value);
            else if (f.FieldType == typeof(double)) f.SetValue(obj, (double)value);
        }

        public static void CallMethod(UnityEngine.Object obj, string method)
        {
            if (obj == null) return;
            var m = obj.GetType().GetMethod(method, Type.EmptyTypes);
            m?.Invoke(obj, null);
        }
    }
}
