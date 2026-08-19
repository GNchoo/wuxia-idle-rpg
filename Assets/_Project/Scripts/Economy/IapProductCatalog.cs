using System;
using UnityEngine;

namespace IdleMvp.Economy
{
    /// <summary>Stable product IDs for IapBridge → store catalog mapping.</summary>
    public static class IapProductCatalog
    {
        public const string BlueDiamondPack0 = "idle.blue_diamond.pack0";
        public const string BlueDiamondPack1 = "idle.blue_diamond.pack1";
        public const string MembershipMonth = "idle.membership.month";
        public const string WeaponPass = "idle.pass.weapon";
        public const string CompanionPass = "idle.pass.companion";
        public const string RemoveAds = "idle.membership.noads";

        public static string[] AllIds => new[]
        {
            BlueDiamondPack0,
            BlueDiamondPack1,
            MembershipMonth,
            WeaponPass,
            CompanionPass,
            RemoveAds
        };

        public static string DisplayName(string productId)
        {
            switch (productId)
            {
                case BlueDiamondPack0: return "블루다이아 소팩";
                case BlueDiamondPack1: return "블루다이아 대팩";
                case MembershipMonth: return "월간 멤버십";
                case WeaponPass: return "무기 패스";
                case CompanionPass: return "동료 패스";
                case RemoveAds: return "광고 제거";
                default: return productId ?? "";
            }
        }
    }
}
