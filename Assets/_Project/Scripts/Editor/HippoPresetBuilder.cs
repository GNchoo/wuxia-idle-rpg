using System.Linq;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using HeroEditor.Common;
using HeroEditor.Common.Data;
using HeroEditor.Common.Enums;
using UnityEditor;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// Hippo(Megapack) 기반 CharPresets 프리팹 일괄 생성 — SP1 프리셋을 같은 이름으로
    /// 교체해 호출부(FieldAutoHuntController·AppearanceService)를 무수정 유지한다.
    /// 캐스팅은 임시 동양풍(Samurai 에디션) — V7에서 AI 무협 파츠로 교체.
    /// </summary>
    public static class HippoPresetBuilder
    {
        const string HumanPrefab = "Assets/HeroEditor/FantasyHeroes/Prefabs/Human.prefab";
        const string Collection = "Assets/HeroEditor/Megapack/Resources/SpriteCollection.asset";
        const string OutDir = "Assets/_Project/Resources/CharPresets";

        class Spec
        {
            public string Name;
            public string Armor, Helmet, Weapon1H, Weapon2H, Hair;
            public Color32 HairColor = new Color32(30, 25, 25, 255);
            public Color32 SkinColor = new Color32(255, 200, 150, 255);
            public float BodyScale = 1f;
        }

        static readonly Spec[] Specs =
        {
            // 주인공(전사 기본) — 경장 무사 + 대검
            new Spec { Name = "Warrior", Armor = "FantasyHeroes.Samurai.Armor.SamuraiLight1",
                       Weapon2H = "FantasyHeroes.Samurai.MeleeWeapon2H.SamuraiSword1",
                       Hair = "Common.Bonus.Hair.Army" },
            // 팔라딘 계열 베이스 + 보스몹 — 중갑 무사
            new Spec { Name = "Berserker", Armor = "FantasyHeroes.Samurai.Armor.SamuraiHeavy1",
                       Helmet = "FantasyHeroes.Samurai.Helmet.SamuraiHelm1",
                       Weapon2H = "FantasyHeroes.Samurai.MeleeWeapon2H.DarkSword" },
            // 녹림도(도적) — 산적 갑주 + 도
            new Spec { Name = "Raider", Armor = "FantasyHeroes.Basic.Armor.BanditArmor",
                       Helmet = "FantasyHeroes.Basic.Helmet.LeatherCap",
                       Weapon1H = "FantasyHeroes.Samurai.MeleeWeapon1H.Katana2 [Paint]" },
            // 자객 — 야행복
            new Spec { Name = "Bandit Cutthroat", Armor = "FantasyHeroes.Basic.Armor.NinjaOutfit",
                       Helmet = "FantasyHeroes.Samurai.Helmet.NinjaHelm1 [Paint]",
                       Weapon1H = "FantasyHeroes.Samurai.MeleeWeapon1H.Ninjato [Paint]" },
            // 궁수 계열 베이스(전투는 근접 캐스팅 — Hippo 활 모션은 2초 차지라 방치형 템포에 안 맞음)
            new Spec { Name = "Bandit Bowman", Armor = "FantasyHeroes.Basic.Armor.LeatherLightArmor",
                       Weapon1H = "FantasyHeroes.Samurai.MeleeWeapon1H.Kunai",
                       Hair = "Common.Basic.Hair.BuzzCut" },
            // 법사 계열 베이스 — 도포
            new Spec { Name = "Peasant", Armor = "FantasyHeroes.Basic.Armor.Robe",
                       Weapon1H = "FantasyHeroes.Samurai.MeleeWeapon1H.Katana1 [Paint]",
                       Hair = "Common.Bonus.Hair.BowlCut" },
            // 하급몹 — 왜소한 부랑자, 병색 피부
            new Spec { Name = "Goblin", Armor = "FantasyHeroes.Basic.Armor.PeasantClothing",
                       Weapon1H = "FantasyHeroes.Basic.MeleeWeapon1H.WoodenClub",
                       SkinColor = new Color32(170, 190, 120, 255), BodyScale = 0.85f },
            // 중급몹 — 모피 갑주 거한
            new Spec { Name = "Orc Warrior", Armor = "FantasyHeroes.Vikings.Armor.VikingFurArmor",
                       Helmet = "FantasyHeroes.Vikings.Helmet.VikingLeatherHelm",
                       Weapon1H = "Extensions.AbandonedWorkshop.MeleeWeapon1H.BarbedClub",
                       SkinColor = new Color32(200, 170, 140, 255), BodyScale = 1.1f },
            // 상급몹 — 해골 가면 광인
            new Spec { Name = "Orc Brute", Armor = "FantasyHeroes.Basic.Armor.SkullBanditOutfit",
                       Helmet = "FantasyHeroes.Basic.Helmet.SkullBanditMask",
                       Weapon2H = "FantasyHeroes.Samurai.MeleeWeapon2H.ShadowSword",
                       SkinColor = new Color32(190, 150, 130, 255), BodyScale = 1.2f },
        };

        [MenuItem("IdleMvp/아트/Hippo 프리셋 일괄 생성", priority = 121)]
        public static void Build()
        {
            var human = AssetDatabase.LoadAssetAtPath<GameObject>(HumanPrefab);
            var col = AssetDatabase.LoadAssetAtPath<SpriteCollection>(Collection);
            if (human == null || col == null) { Debug.LogError("[Hippo] Human.prefab 또는 SpriteCollection 로드 실패"); return; }

            foreach (var spec in Specs)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(human);
                go.name = spec.Name;
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                try
                {
                    var ch = go.GetComponent<Character>();
                    ch.SpriteCollection = col;

                    var look = new CharacterAppearance();
                    if (!string.IsNullOrEmpty(spec.Hair)) look.Hair = spec.Hair;
                    look.HairColor = spec.HairColor;
                    look.BeardColor = spec.HairColor;
                    look.BodyColor = spec.SkinColor;
                    look.EyesColor = new Color32(60, 40, 30, 255);   // 갈색 눈 — 동양풍
                    look.Setup(ch, initialize: false);

                    ch.Equip(Find(col.Armor, spec.Armor), EquipmentPart.Armor);
                    if (spec.Helmet != null) ch.Equip(Find(col.Helmet, spec.Helmet), EquipmentPart.Helmet);
                    if (spec.Weapon1H != null) ch.Equip(Find(col.MeleeWeapon1H, spec.Weapon1H), EquipmentPart.MeleeWeapon1H);
                    if (spec.Weapon2H != null) ch.Equip(Find(col.MeleeWeapon2H, spec.Weapon2H), EquipmentPart.MeleeWeapon2H);

                    ch.Initialize();
                    if (!Mathf.Approximately(spec.BodyScale, 1f))
                        ch.BodyScale = new Vector2(spec.BodyScale, spec.BodyScale);

                    go.AddComponent<IdleMvp.Combat.HippoActorController>().Char = ch;

                    System.IO.Directory.CreateDirectory(OutDir);
                    PrefabUtility.SaveAsPrefabAsset(go, OutDir + "/" + spec.Name + ".prefab");
                    Debug.Log("[Hippo] 프리셋 저장: " + spec.Name);
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Hippo] 프리셋 " + Specs.Length + "종 생성 완료");
        }

        static ItemSprite Find(System.Collections.Generic.List<ItemSprite> list, string id)
        {
            var it = list.FirstOrDefault(i => i.Id == id);
            if (it == null) throw new System.Exception("SpriteCollection에 없음: " + id);
            return it;
        }
    }
}
