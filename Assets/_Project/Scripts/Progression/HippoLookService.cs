using System.Linq;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using HeroEditor.Common;
using HeroEditor.Common.Data;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace IdleMvp.Progression
{
    /// <summary>
    /// Hippo 리그의 외형 규칙 — 장비=외형 원칙(티어 방어구·무기)과 플레이어
    /// 커스터마이징(신체·헤어·얼굴)을 Megapack SpriteCollection 파츠로 적용한다.
    /// SP1 AppearanceService의 Hippo 대응부. 캐스팅은 임시 동양풍(V7에서 AI 무협 교체).
    /// </summary>
    public static class HippoLookService
    {
        // ---- 커스터마이징 저장 (신체·헤어·얼굴만 — 의상·무기는 장비가 결정) ----

        [System.Serializable]
        public class HippoAppearance
        {
            public string hair = "";        // SpriteCollection id ("" = 프리셋 기본 유지)
            public string beard = "";
            public string eyebrows = "";
            public string eyes = "";
            public string mouth = "";
            public string head = "";        // 얼굴형 (Human/Scar 등 — 문양 역할)
            public Color32 hairColor = new Color32(30, 25, 25, 255);
            public Color32 skinColor = new Color32(255, 200, 150, 255);
            public Color32 eyesColor = new Color32(60, 40, 30, 255);
        }

        public static event System.Action OnChanged;

        static HippoAppearance _current;
        static bool _loaded;

        static string SavePath
        {
            get
            {
                string dir = System.IO.Path.Combine(Application.persistentDataPath, "JSON");
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                return System.IO.Path.Combine(dir, "appearance-hippo.json");
            }
        }

        public static HippoAppearance Current
        {
            get
            {
                if (_loaded) return _current;
                _loaded = true;
                try
                {
                    if (System.IO.File.Exists(SavePath))
                        _current = JsonUtility.FromJson<HippoAppearance>(System.IO.File.ReadAllText(SavePath));
                }
                catch (System.Exception e) { Debug.LogWarning("[HippoLook] load failed: " + e.Message); }
                return _current;
            }
        }

        public static void Save(HippoAppearance a)
        {
            _current = a;
            _loaded = true;
            try { System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(a)); }
            catch (System.Exception e) { Debug.LogWarning("[HippoLook] save failed: " + e.Message); }
            OnChanged?.Invoke();
            AppearanceService.NotifyWeaponChanged();   // 프리뷰 리그 재생성 트리거 공유
        }

        public static void ResetToDefault()
        {
            _current = null;
            _loaded = true;
            try { if (System.IO.File.Exists(SavePath)) System.IO.File.Delete(SavePath); } catch { }
            OnChanged?.Invoke();
            AppearanceService.NotifyWeaponChanged();
        }

        // ---- 티어 테이블 (약→강 10단계, 임시 동양풍) ---------------------------

        static readonly string[] ArmorTiers =
        {
            "FantasyHeroes.Basic.Armor.PeasantClothing",
            "FantasyHeroes.Basic.Armor.LeatherJacket",
            "FantasyHeroes.Basic.Armor.BanditArmor",
            "FantasyHeroes.Basic.Armor.LeatherRivetedArmor",
            "FantasyHeroes.Samurai.Armor.SamuraiLight1",
            "FantasyHeroes.Samurai.Armor.SamuraiLight2",
            "FantasyHeroes.Samurai.Armor.SamuraiLight3",
            "FantasyHeroes.Samurai.Armor.SamuraiHeavy1",
            "FantasyHeroes.Samurai.Armor.SamuraiHeavy2",
            "FantasyHeroes.Samurai.Armor.SamuraiHeavy3",
        };

        static readonly string[] HelmetTiers =
        {
            null, null,   // 저티어는 맨머리 — 헤어가 보인다
            "FantasyHeroes.Basic.Helmet.PeasantCap",
            "FantasyHeroes.Basic.Helmet.LeatherCap",
            "FantasyHeroes.Basic.Helmet.LeatherHelm",
            "FantasyHeroes.Samurai.Helmet.NinjaHelm2 [Paint]",
            "FantasyHeroes.Samurai.Helmet.SamuraiHelm1",
            "FantasyHeroes.Samurai.Helmet.SamuraiHelm2",
            "FantasyHeroes.Samurai.Helmet.SamuraiHelm3",
            "FantasyHeroes.Samurai.Helmet.SamuraiElderHelm",
        };

        // 무기 종류(0 검 / 1 지팡이 / 2 활→근접 대검 / 3 단검) × 티어.
        // 활 모션은 Hippo 차지 2초라 방치형 템포에 안 맞아 근접으로 캐스팅.
        static readonly string[][] WeaponTiers =
        {
            new[] {
                "1H:FantasyHeroes.Basic.MeleeWeapon1H.ShortIronKatana [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana1 [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana2 [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana3 [Paint]",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.SamuraiSword1",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.SamuraiSword2",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.SamuraiSword3",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.SunriseSword",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.ShadowSword",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.DarkSword",
            },
            new[] {
                "1H:FantasyHeroes.Basic.MeleeWeapon1H.HardwoodWand",
                "1H:FantasyHeroes.Basic.MeleeWeapon1H.HermitWand",
                "1H:FantasyHeroes.Basic.MeleeWeapon1H.DruidWand",
                "1H:FantasyHeroes.SwampLords.MeleeWeapon1H.ElderStaff",
                "1H:Extensions.AbandonedWorkshop.MeleeWeapon1H.SagewoodWand",
                "1H:Extensions.AbandonedWorkshop.MeleeWeapon1H.EnlightenmentStaff",
                "1H:Extensions.AbandonedWorkshop.MeleeWeapon1H.NightStaff",
                "1H:Extensions.AbandonedWorkshop.MeleeWeapon1H.SolarStaff",
                "1H:Extensions.Epic.MeleeWeapon1H.ElementalStaff",
                "1H:Extensions.Epic.MeleeWeapon1H.StaffOfInfinity",
            },
            new[] {
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.Spear",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.Spear",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.SiegeSpear",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.SiegeSpear",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.Halberd",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.Halberd",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.CataphractSpear",
                "2H:FantasyHeroes.Basic.MeleeWeapon2H.CataphractSpear",
                "2H:FantasyHeroes.Thrones.MeleeWeapon2H.SandWarriorHalberd",
                "2H:FantasyHeroes.Thrones.MeleeWeapon2H.SandWarriorHalberd",
            },
            new[] {
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Kunai",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Kunai",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Sai [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Sai [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Ninjato [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Ninjato [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana1 [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana2 [Paint]",
                "1H:FantasyHeroes.Samurai.MeleeWeapon1H.Katana3 [Paint]",
                "2H:FantasyHeroes.Samurai.MeleeWeapon2H.ShadowSword",
            },
        };

        static SpriteCollection Col(CharacterBase ch) => ch != null ? ch.SpriteCollection : null;

        static ItemSprite Find(System.Collections.Generic.List<ItemSprite> list, string id)
        {
            return string.IsNullOrEmpty(id) ? null : list.FirstOrDefault(i => i.Id == id);
        }

        /// <summary>장비창 티어 → 갑옷·투구, 장착 무기 → 손 무기. 프리셋 캐스팅 위에 덮는다.</summary>
        public static void ApplyEquipmentLook(Character ch)
        {
            var col = Col(ch);
            if (col == null) return;
            var inv = IdleMvp.Adapters.InventoryAdapter.Instance;
            if (inv != null && inv.Slots != null)
            {
                var helm = IdleMvp.Core.ContentCatalog.GetEquip(1, inv.Slots.Length > 1 ? inv.Slots[1].level : 0);
                var armor = IdleMvp.Core.ContentCatalog.GetEquip(2, inv.Slots.Length > 2 ? inv.Slots[2].level : 0);
                if (armor != null)
                {
                    var it = Find(col.Armor, ArmorTiers[Mathf.Clamp(armor.tier, 1, 10) - 1]);
                    if (it != null) ch.Equip(it, EquipmentPart.Armor);
                }
                if (helm != null)
                {
                    string id = HelmetTiers[Mathf.Clamp(helm.tier, 1, 10) - 1];
                    if (id == null) ch.UnEquip(EquipmentPart.Helmet);
                    else
                    {
                        var it = Find(col.Helmet, id);
                        if (it != null) ch.Equip(it, EquipmentPart.Helmet);
                    }
                }
            }

            var w = IdleMvp.Adapters.WeaponSummonAdapter.Instance?.Equipped;
            if (w != null)
            {
                // 티어 근사: 등급(0~5) → 1~10
                int tier = Mathf.Clamp(w.rarity * 2 + 1, 1, 10);
                // AI 무협 무기(V7)가 있으면 그쪽이 우선 — 컬렉션 등록 없이 직접 배정
                var custom = Resources.Load<Sprite>(
                    "WuxiaWeapons/k" + Mathf.Clamp(w.kind, 0, 3) + "_t" + tier.ToString("00"));
                if (custom != null)
                {
                    bool twoHc = tier >= 5;
                    ch.PrimaryMeleeWeapon = custom;
                    ch.SecondaryMeleeWeapon = null;
                    ch.WeaponType = twoHc ? WeaponType.Melee2H : WeaponType.Melee1H;
                }
                else
                {
                    var row = WeaponTiers[Mathf.Clamp(w.kind, 0, 3)][tier - 1];
                    bool twoH = row.StartsWith("2H:");
                    string id = row.Substring(3);
                    var it = Find(twoH ? col.MeleeWeapon2H : col.MeleeWeapon1H, id);
                    if (it != null)
                        ch.Equip(it, twoH ? EquipmentPart.MeleeWeapon2H : EquipmentPart.MeleeWeapon1H);
                }
            }
        }

        /// <summary>신체·헤어·얼굴 커스터마이징을 리그에 적용 (a=null이면 저장본).</summary>
        public static void ApplyCustomization(Character ch, HippoAppearance a = null)
        {
            var col = Col(ch);
            if (a == null) a = Current;
            if (col == null || a == null) return;

            var hair = Find(col.Hair, a.hair);
            if (hair != null) ch.SetBody(hair, BodyPart.Hair, a.hairColor);
            else ch.HairRenderer.color = a.hairColor;

            var beard = Find(col.Beard, a.beard);
            if (beard != null) ch.SetBody(beard, BodyPart.Beard, a.hairColor);

            var head = Find(col.Head, a.head);
            if (head != null) ch.SetBody(head, BodyPart.Head);

            ch.HeadRenderer.color = a.skinColor;
            ch.BodyRenderers.ForEach(r => r.color = a.skinColor);
            ch.EarsRenderer.color = a.skinColor;

            var brows = Find(col.Eyebrows, a.eyebrows);
            if (brows != null) ch.SetBody(brows, BodyPart.Eyebrows);
            var eyes = Find(col.Eyes, a.eyes);
            if (eyes != null) ch.SetBody(eyes, BodyPart.Eyes, a.eyesColor);
            var mouth = Find(col.Mouth, a.mouth);
            if (mouth != null) ch.SetBody(mouth, BodyPart.Mouth);
        }

        /// <summary>히어로 리그 최종 룩: 커스터마이징 → 장비 외형 순서로 덮는다.</summary>
        public static void ApplyHero(Character ch)
        {
            if (ch == null) return;
            try
            {
                ApplyCustomization(ch);
                ApplyEquipmentLook(ch);
                ch.Initialize();
            }
            catch (System.Exception e) { Debug.LogWarning("[HippoLook] apply failed: " + e.Message); }
        }
    }
}
