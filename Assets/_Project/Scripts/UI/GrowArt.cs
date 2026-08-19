using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.UI
{
    /// <summary>
    /// Loads UI/actor sprites.
    /// Priority: GrowArt (paid / extracted) → CasualGui (soft free) → FreePack → MvpUi.
    /// Combat FX: GrowArt/Fx/* (from IdleRPG_Assets Spells).
    /// </summary>
    public static class GrowArt
    {
        // Kit sprites load directly from Assets/FantasyIdleGameGUI/Resources ("Sprites/..."),
        // which preserves the importer 9-slice borders set by FantasyKitImporterFix.
        public static Sprite Hero => First("GrowArt/Chars/Hero", "GrowArt/Hero", "FreePack/Hero/Hero", "MvpUi/HeroIdleFrame");
        public static Sprite Enemy => First("GrowArt/Chars/Enemy1", "GrowArt/EnemyWildDog", "FreePack/Enemy/Enemy");
        public static Sprite BattleBg => First("Sprites/Images/Bg_Ingame", "GrowArt/BattleBg", "FreePack/UI/BattleBg");
        public static Sprite PanelFrame => First("Sprites/Popups/Popup_Bg", "GrowArt/PanelFrame", "CasualGui/Panel", "FreePack/UI/PanelFrame", "MvpUi/PanelFrame");
        public static Sprite ModalFrame => First("Sprites/Popups/Popup_Bg", "GrowArt/ModalFrame", "CasualGui/Modal", "FreePack/UI/ModalFrame", "PanelFrame");
        public static Sprite ModalInner => First("CasualKit/Popoup01-03_White_Bg", "Sprites/Frame/Frame_Cyan", "GrowArt/ModalInner", "CasualGui/Modal", "FreePack/UI/ModalInner");
        public static Sprite BottomBar => First("CasualKit/Popup_FullWidth03_Single_Navy", "Sprites/UI_etcs/Bg_Menu_Bar", "GrowArt/BottomBar", "CasualGui/BottomBar", "FreePack/UI/BottomBar");
        public static Sprite GroundBar => First("CasualKit/Popup_FullWidth03_Single_Navy", "Sprites/UI_etcs/Bg_Menu", "GrowArt/BottomBarAlt", "CasualGui/BottomBar", "FreePack/UI/BottomBar");
        public static Sprite UpgradeButton => First("WuxiaUi/btn_primary", "CasualKit/Button01_195_Green", "Sprites/Buttons/Btn_Green", "GrowArt/UpgradeButton", "CasualGui/BtnOrange", "FreePack/UI/UpgradeButton", "MvpUi/UpgradeButton");
        public static Sprite CtaButton => First("WuxiaUi/btn_primary", "CasualKit/Button01_195_Sky", "Sprites/Buttons/Btn_Mint", "GrowArt/CtaButton", "CasualGui/BtnOrange", "FreePack/UI/CtaButton", "UpgradeButton");
        // 아이콘은 구매 에셋(CasualKit)을 최우선으로 쓴다. 예전 에셋은 폴백일 뿐.
        public static Sprite IconGold => First("CasualKit/IcoC_Gold", "Sprites/Icons/Icon_Gold", "GrowArt/IconGold", "FreePack/Icons/IconGold");
        public static Sprite IconGem => First("CasualKit/IcoC_Gems", "Sprites/Icons/Icon_Gem", "GrowArt/IconGem", "FreePack/Icons/IconGem");
        public static Sprite IconCp => First("CasualKit/IcoC_Battle", "Sprites/Icons/Icon_Boss", "GrowArt/IconCp", "FreePack/IconCp");
        public static Sprite IconClose => First("CasualKit/Ico_Close", "Sprites/Popups/Btn_X", "GrowArt/IconClose", "FreePack/Icons/IconClose", "FreePack/UI/IconClose");
        public static Sprite TopFog => First("Sprites/Popups/Popup_Title", "GrowArt/TopFog", "CasualGui/TopFog", "FreePack/UI/TopFog", "MvpUi/TopBarFog");
        public static Sprite SquareFrame => First("CasualKit/BasicFrame_Round12", "Sprites/Frame/Frame_List_Gray", "GrowArt/SquareFrame", "CasualGui/Slot", "FreePack/UI/SquareFrame", "MvpUi/SquareFrame");
        public static Sprite CircleFrame => First("CasualKit/BasicFrame_Circle77", "Sprites/Frame/Frame_Profile", "GrowArt/CircleFrame", "GrowArt/SkillCircle", "FreePack/UI/CircleFrame");
        public static Sprite SkillCircle => First("CasualKit/BasicFrame_Circle77", "Sprites/Frame/Frame_Profile", "GrowArt/SkillCircle", "FreePack/UI/SkillCircle", "CircleFrame");
        public static Sprite BarEmpty => First("WuxiaUi/bar_bg", "CasualKit/Slider_Basic03_Bg", "Sprites/Components/Bar_Back", "GrowArt/BarEmpty", "CasualGui/BarBg", "FreePack/UI/BarEmpty");
        public static Sprite BarFill => First("WuxiaUi/bar_fill", "CasualKit/Slider_Basic03_Fill_White", "Sprites/Components/Bar_Front", "GrowArt/BarFill", "CasualGui/BarFill", "FreePack/UI/BarFill");
        public static Sprite ShopCard => First("CasualKit/CardFrame03_White", "Sprites/Frame/Frame_List_Yellow", "GrowArt/ShopCard", "CasualGui/Slot", "FreePack/UI/ShopCard", "SquareFrame");
        public static Sprite InvSlot => First("CasualKit/BasicFrame_Round12", "Sprites/Frame/Frame_List_Gray", "GrowArt/InvSlot", "CasualGui/Slot", "FreePack/UI/InvSlot", "SquareFrame");

        public static Sprite FxHit => First("GrowArt/Fx/Hit1", "GrowArt/Fx/Hit2", "GrowArt/Fx/Hit3");
        public static Sprite FxSkill => First("GrowArt/Fx/SkillPulse", "GrowArt/Fx/Hit1");

        static readonly Dictionary<string, Sprite[]> FxSeqCache = new Dictionary<string, Sprite[]>();

        /// <summary>IdleRPG_Assets Spells frame sequence under GrowArt/Fx/{folder}/01..N.</summary>
        public static Sprite[] FxSequence(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return null;
            if (FxSeqCache.TryGetValue(folder, out var cached)) return cached;

            var loaded = Resources.LoadAll<Sprite>("GrowArt/Fx/" + folder);
            if (loaded == null || loaded.Length == 0)
            {
                // Fallback: build from Texture2D if importer hasn't created sprites yet
                var tex = Resources.LoadAll<Texture2D>("GrowArt/Fx/" + folder);
                if (tex != null && tex.Length > 0)
                {
                    System.Array.Sort(tex, (a, b) => string.CompareOrdinal(a.name, b.name));
                    var built = new List<Sprite>(tex.Length);
                    foreach (var t in tex)
                    {
                        if (t == null || t.name.IndexOf("skull", System.StringComparison.OrdinalIgnoreCase) >= 0)
                            continue;
                        built.Add(Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                            new Vector2(0.5f, 0.5f), 100f));
                    }
                    cached = built.ToArray();
                }
                else
                    cached = System.Array.Empty<Sprite>();
            }
            else
            {
                System.Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
                var list = new List<Sprite>(loaded.Length);
                foreach (var s in loaded)
                {
                    if (s == null) continue;
                    if (s.name.IndexOf("skull", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    list.Add(s);
                }
                cached = list.ToArray();
            }

            FxSeqCache[folder] = cached;
            return cached;
        }

        // ---- Fantasy Idle UI Kit deep slots ----
        public static Sprite TabOn => First("WuxiaUi/tab_on", "CasualKit/Menu_TopBtn_Focus", "Sprites/Components/Tab_On", "GrowArt/TabOn");
        public static Sprite TabOff => First("WuxiaUi/tab_off", "CasualKit/Menu_TopBtn", "Sprites/Components/Tab_Off", "GrowArt/TabOff");
        public static Sprite StageFrame => First("CasualKit/BasicFrame_Round20", "Sprites/Frame/Frame_Stage", "GrowArt/StageFrame");
        public static Sprite PopupTitle => First("Sprites/Popups/Popup_Title", "GrowArt/PopupTitle");
        public static Sprite BadgeNew => First("CasualKit/Alert_Text_s_Red", "Sprites/UI_etcs/Badge_New", "GrowArt/BadgeNew");
        public static Sprite SkillDockBg => First("CasualKit/BasicFrame_Round12", "Sprites/UI_etcs/Bg_Skill_Set", "GrowArt/SkillDockBg");

        public static Sprite BtnNeutral => First("WuxiaUi/btn_secondary", "CasualKit/Button01_195_Gray", "Sprites/Buttons/Btn_Round_Gray", "GrowArt/BtnNeutral");
        public static Sprite BtnDanger => First("CasualKit/Button01_195_Red", "Sprites/Buttons/Btn_Red", "GrowArt/BtnDanger");
        public static Sprite BtnAlt => First("CasualKit/Button01_195_Purple", "Sprites/Buttons/Btn_Violet_S", "GrowArt/BtnAlt");
        public static Sprite CardDark => First("CasualKit/Popup01_Single_Navy", "Sprites/Frame/Frame_Round_Black", "GrowArt/CardDark");
        public static Sprite ChipFrame => First("CasualKit/BasicFrame_Round12", "Sprites/Frame/Frame_Text_01", "GrowArt/ChipFrame");
        public static Sprite IconFrame => First("CasualKit/BasicFrame_Square", "Sprites/Frame/Frame_Img", "GrowArt/IconFrame");
        public static Sprite TabStrip => First("WuxiaUi/tab_off", "CasualKit/Menu_TopBtn", "Sprites/UI_etcs/Bg_Tab", "GrowArt/TabStrip");

        static readonly string[] RarityNames = { "Gray", "Green", "Blue", "Violet", "Yellow", "Red" };

        /// <summary>등급 테두리 0(회색)..5(빨강). 키트 등급 카드 프레임 우선.</summary>
        static readonly string[] RarityKit =
        {
            "CardFrame03_Single_Dim", "CardFrame03_Single_Blue", "CardFrame03_Single_Green",
            "CardFrame03_Single_Purple", "CardFrame03_Single_Orange", "CardFrame03_Single_Orange",
        };

        public static Sprite Rarity(int grade)
        {
            int g = Mathf.Clamp(grade, 0, 5);
            return First("CasualKit/" + RarityKit[g],
                "Sprites/Frame/Frame_Edge_" + RarityNames[g], "GrowArt/Rarity" + g);
        }

        public static Sprite IconXp => First("CasualKit/Ico_Xp", "Sprites/Icons/Icon_Rank_01", "Sprites/Icons/Icon_Essnce", "GrowArt/IconCp");
        public static Sprite IconStone => First("CasualKit/Ico_Stone", "Sprites/Icons/Icon_Essnce", "Sprites/Icons/Ascend_01_Iron", "GrowArt/Icon/EnhanceHp");
        public static Sprite IconAds => First("CasualKit/Ico_Ads", "Sprites/Icons/Icon_Ads", "GrowArt/Icon/Plus");
        public static Sprite IconAdsFree => First("CasualKit/Ico_AdsFree", "Sprites/Icons/Icon_AdsFree", "Sprites/Icons/Icon_Ads");
        /// <summary>승급/등급 티어 아이콘. 키트 별 등급 아이콘을 우선 사용.</summary>
        public static Sprite IconAscend(int tier)
        {
            switch (Mathf.Clamp(tier, 1, 5))
            {
                case 2: return First("CasualKit/Ico_Def", "Sprites/Icons/Ascend_02_Bronze", "Sprites/Icons/Ascend_01_Iron");
                case 3: return First("CasualKit/Ico_Hp", "Sprites/Icons/Ascend_03_Silver", "Sprites/Icons/Ascend_01_Iron");
                case 4: return First("CasualKit/Ico_Trophy", "Sprites/Icons/Ascend_04_Gold", "Sprites/Icons/Ascend_01_Iron");
                case 5: return First("CasualKit/Ico_Crit", "Sprites/Icons/Ascend_04_Gold", "Sprites/Icons/Ascend_01_Iron");
                default: return First("CasualKit/Ico_Stone", "Sprites/Icons/Ascend_01_Iron");
            }
        }

        // Slot → template item art (Forest/Desert starter sets).
        static readonly string[] SlotItem =
        {
            "Forest_uzupenione__Wooden_sword_FOREST",       // 0 weapon
            "Desert_set_uzupenione__Afrykanska_maska_Desert", // 1 helm
            "Forest_uzupenione__Lniany_T_shirt_FOREST",     // 2 armor
            "Forest_uzupenione__Broken_Amulet_FOREST",      // 3 acc
            "Forest_uzupenione__Podarte_Portki_FOREST",     // 4 bottoms
            "Forest_uzupenione__Sredniowieczne_buty_FOREST" // 5 boots
        };

        /// <summary>Equip slots: 0 weapon, 1 helm, 2 armor, 3 acc, 4 bottoms, 5 boots.</summary>
        /// <summary>슬롯 순서: 무기 / 투구 / 갑옷 / 장신구 / 하의 / 신발</summary>
        static readonly string[] KitGear =
            { "Gear_Sword", "Gear_Helmet", "Gear_Armor", "Gear_Ring", "Gear_Cloth", "Gear_Shoes" };

        public static Sprite IconGear(int slot)
        {
            int s = Mathf.Clamp(slot, 0, 5);
            // 구매 에셋(GUI Pro) 아이콘을 최우선으로 쓴다. 예전 TplArt 아이콘은 폴백일 뿐.
            var kit = IdleMvp.UI.Maple.CasualArt.C(KitGear[s]);
            if (kit != null) return kit;
            var tpl = TplItem(SlotItem[s]);
            if (tpl != null) return tpl;
            switch (s)
            {
                case 0: return IconSummonWeapon;
                case 1: return First("Sprites/Icons/Ascend_02_Bronze", "Sprites/Icons/Icon_Rank_01", "Sprites/Icons/Enhance_Accuracy", "GrowArt/Icon/EnhanceAccuracy");
                case 2: return First("Sprites/Icons/Ascend_03_Silver", "Sprites/Icons/Enhance_HP", "GrowArt/Icon/EnhanceHp");
                case 3: return IconSummonAcc;
                case 4: return First("Sprites/Icons/Ascend_04_Gold", "Sprites/Icons/Enhance_AttackSpeed", "GrowArt/Icon/EnhanceAttackSpeed");
                default: return First("Sprites/Icons/Growth_VIT", "Sprites/Icons/Enhance_HPRegen", "GrowArt/Icon/EnhanceHpRegen");
            }
        }

        // ---- TplArt: curated template watercolor art (Phase G) ----------------

        static Sprite[] _tplHeroes;
        static Sprite[] _tplBosses;
        static Sprite[] TplHeroes
        {
            get
            {
                if (_tplHeroes == null) _tplHeroes = Resources.LoadAll<Sprite>("TplArt/Heroes");
                return _tplHeroes;
            }
        }
        static Sprite[] TplBosses
        {
            get
            {
                if (_tplBosses == null) _tplBosses = Resources.LoadAll<Sprite>("TplArt/Bosses");
                return _tplBosses;
            }
        }

        public static bool HasTplArt => TplHeroes.Length > 0;

        static int StableHash(string s)
        {
            int h = 23;
            if (s != null) foreach (char c in s) h = h * 31 + c;
            return h & 0x7fffffff;
        }

        /// <summary>Chapter background from template biome art (1..20), fallback BattleBg.</summary>
        public static Sprite BiomeBg(int chapter)
        {
            var s = Resources.Load<Sprite>($"TplArt/Biomes/Biome{Mathf.Clamp(chapter, 1, 20):00}");
            return s != null ? s : BattleBg;
        }

        /// <summary>Biome boss sprite for chapter, fallback EnemyBoss.</summary>
        public static Sprite BiomeBoss(int chapter)
        {
            string prefix = $"Boss{Mathf.Clamp(chapter, 1, 20):00}_";
            var pool = TplBosses;
            for (int i = 0; i < pool.Length; i++)
                if (pool[i] != null && pool[i].name.StartsWith(prefix, System.StringComparison.Ordinal))
                    return pool[i];
            return EnemyBoss;
        }

        public static Sprite TplItem(string key)
        {
            return string.IsNullOrEmpty(key) ? null : Resources.Load<Sprite>("TplArt/Items/" + key);
        }

        // Rarity tier → themed item set (starter forest → desert → cyber → divine).
        static readonly string[] TierWeapon =
        {
            "Forest_uzupenione__Wooden_sword_FOREST",
            "Desert_set_uzupenione__Dzida_Desert",
            "Cyber_future_uzupenione__bron_cyber",
            "God_s_armour__boska_bron_piorun"
        };

        /// <summary>
        /// 무기 종류: 0=검(양날) 1=도(외날) 2=창·곤 3=기병(奇兵).
        /// 무협 고증에 맞춰 재편했다 — 착용형 '권갑/조(爪)'는 중국 무술의 표준 병기로
        /// 확인되지 않아(인도 바그나크와 혼동 소지) 판관필·아미자 같은 기병으로 바꿨다.
        /// docs/wuxia-reference.md 참조.
        /// </summary>
        public static readonly string[] WeaponKindNames = { "검", "도", "창곤", "기병", "권갑" };

        public static Sprite IconWeaponKind(int kind)
        {
            switch (kind)
            {
                case 1: return First("CasualKit/IcoC_Blades", "CasualKit/IcoC_Sword");        // 도
                case 2: return First("CasualKit/IcoC_Arrow", "CasualKit/IcoC_Staff");         // 창·곤
                case 3: return First("CasualKit/IcoC_Dagger", "CasualKit/IcoC_Fist");         // 기병
                case 4: return First("CasualKit/IcoC_Fist", "CasualKit/IcoC_Dagger");          // 권갑
                default: return First("CasualKit/IcoC_Sword", "Sprites/Icons/Summon_Weapon"); // 검
            }
        }

        /// <summary>
        /// 무기 id별 실루엣. 키트에 24종 전용 아트는 없으므로 같은 종류 안에서도
        /// 상위 무기는 다른 실루엣을 쓰고, 나머지 구분은 RarityFrame()이 맡는다.
        /// </summary>
        static readonly Dictionary<string, string> WeaponIconById = new Dictionary<string, string>
        {
            // 검(劍) — 목검/철검/청강검/송문고정검/한철검/현철중검
            { "w_wood_sword",     "IcoC_Sword"  }, { "w_steel_sword",     "IcoC_Sword"  },
            { "w_mythril_blade",  "IcoC_Sword"  }, { "w_dragon_blade",    "IcoC_Blades" },
            { "w_crimson_saber",  "IcoC_Blades" }, { "w_knight_claymore", "IcoC_Hammer" },
            // 도(刀) — 유엽도/안령도/우미도/묘도/박도/현철귀두도
            { "w_oak_staff",      "IcoC_Blades" }, { "w_flame_staff",     "IcoC_Blades" },
            { "w_frost_staff",    "IcoC_Blades" }, { "w_arcane_staff",    "IcoC_Hammer" },
            { "w_thunder_rod",    "IcoC_Hammer" }, { "w_void_wand",       "IcoC_Hammer" },
            // 창·곤 — 백랍곤/죽창/화창/대창/편곤/월아산
            { "w_hunter_bow",     "IcoC_Staff"  }, { "w_maple_bow",       "IcoC_Arrow"  },
            { "w_storm_bow",      "IcoC_Arrow"  }, { "w_phoenix_bow",     "IcoC_Arrow"  },
            { "w_composite_bow",  "IcoC_Staff"  }, { "w_sniper_bow",      "IcoC_Staff"  },
            // 기병(奇兵) — 판관필/아미자/철선/호두구/자오원앙월/구절편
            { "w_bronze_claw",    "IcoC_Dagger" }, { "w_shadow_claw",     "IcoC_Dagger" },
            { "w_venom_claw",     "IcoC_Fist"   }, { "w_assassin_claw",   "IcoC_Dagger" },
            { "w_iron_knuckle",   "IcoC_Fist"   }, { "w_blood_fist",      "IcoC_Fist"   },
            // 권갑(拳甲) — 주먹 문파
            { "w_gauntlet_cloth", "IcoC_Fist"   }, { "w_gauntlet_leather","IcoC_Fist"   },
            { "w_gauntlet_iron",  "IcoC_Fist"   }, { "w_gauntlet_steel",  "IcoC_Fist"   },
            { "w_gauntlet_cold",  "IcoC_Fist"   }, { "w_gauntlet_dark",   "IcoC_Fist"   },
        };

        /// <summary>
        /// 무기 id 우선. 직접 만든 무협 착용 스프라이트(Resources/WearParts, 무기 id와 1:1)가
        /// 있으면 그것이 진짜 그 무기의 그림이다 — 키트 실루엣 7종 순환은 폴백으로만 남긴다.
        /// </summary>
        /// <summary>이 무기에 전용 무협 아트가 있는가 — 있으면 틴트를 덧입히지 않는다.</summary>
        public static bool WeaponIconIsDedicated(string id)
            => !string.IsNullOrEmpty(id) && Resources.Load<Sprite>("WearParts/" + id) != null;

        public static Sprite IconWeaponId(string id, int kind)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var mine = Resources.Load<Sprite>("WearParts/" + id);
                if (mine != null) return mine;
                if (WeaponIconById.TryGetValue(id, out var key))
                {
                    var s = Resources.Load<Sprite>("CasualKit/" + key);
                    if (s != null) return s;
                }
            }
            return IconWeaponKind(kind);
        }

        /// <summary>
        /// 장비 티어 아이콘. 전용 아트(Resources/EquipIcons/*)가 있으면 그걸 쓰고,
        /// 아직 없으면 부위 아이콘 + 티어 색으로 대체한다.
        /// (무협 아트로 갈아엎는 중이라 파일이 채워지는 대로 자동으로 바뀐다)
        /// </summary>
        public static Sprite IconEquipDef(Core.EquipDef def, out bool isDedicated)
        {
            isDedicated = false;
            if (def == null) return null;
            if (!string.IsNullOrEmpty(def.icon))
            {
                var s = Resources.Load<Sprite>(def.icon);
                if (s != null) { isDedicated = true; return s; }
            }
            return IconGear(def.slot);
        }

        /// <summary>희귀도 배경 프레임 (0 일반 / 1 희귀 / 2 영웅 / 3 전설).</summary>
        public static Sprite RarityFrame(int rarity)
        {
            return Resources.Load<Sprite>("CasualKit/FrameR" + Mathf.Clamp(rarity, 0, 3));
        }

        /// <summary>
        /// 직업 id별 아이콘 (8직업 전부 다른 실루엣).
        /// 예전엔 IconStat(i % 4)라 8직업이 스탯 아이콘 4개를 돌려썼다.
        /// 전부 컬러 아이콘만 쓴다 — 직업 카드가 밝은 배경이라 흰 픽토그램은 안 보인다.
        /// </summary>
        static readonly Dictionary<string, string> JobIconById = new Dictionary<string, string>
        {
            { "hero",       "IcoC_Sword"  }, // 무사
            { "bowmaster",  "IcoC_Blades" }, // 살수
            { "archmage",   "IcoC_Staff"  }, // 마두
            { "paladin",    "IcoC_Shield" }, // 호법
            { "darkknight", "IcoC_Hammer" }, // 혈귀
            { "marksman",   "IcoC_Bow"    }, // 독사
            { "bishop",     "IcoC_Book"   }, // 도사
            { "nightlord",  "IcoC_Fist"   }, // 야살
            // 계열별 4차 구조를 채우며 추가된 직업
            { "swordlord",    "IcoC_Battle" }, // 검존 (정파 4차)
            { "shadowstep",   "IcoC_Dagger" }, // 밀행 (사파 2차)
            { "bloodlord",    "IcoC_Bolt"   }, // 혈존 (마도 3차)
            { "demonemperor", "IcoC_Crown"  }, // 마제 (마도 4차)
        };

        public static Sprite IconJob(string jobId)
        {
            if (!string.IsNullOrEmpty(jobId) && JobIconById.TryGetValue(jobId, out var key))
            {
                var s = Resources.Load<Sprite>("CasualKit/" + key);
                if (s != null) return s;
            }
            return First("CasualKit/IcoC_Role", "CasualKit/IcoC_Crown");
        }

        /// <summary>
        /// Weapon icon: kind stays visually distinct (sword/staff/bow/claw) via kit icons+tint.
        /// TplArt only where the art truly matches (starter wooden sword) —
        /// per-weapon icon mapping lands with the Phase L weapons.json icon field.
        /// </summary>
        public static Sprite IconWeapon(int kind, int rarity)
        {
            if (kind == 0 && rarity <= 0)
            {
                var s = TplItem(TierWeapon[0]);
                if (s != null) return s;
            }
            return IconWeaponKind(kind);
        }

        /// <summary>
        /// 동료 초상화. 필드에 실제로 소환되는 리그를 렌더링한 그림을 쓴다 —
        /// 예전 TplArt 수채화는 소환된 동료와 전혀 달라서 폴백으로만 남긴다.
        /// </summary>
        public static Sprite IconCompanion(string name, int rarity = 0)
        {
            var live = CompanionArt.PortraitFor(name, rarity);
            if (live != null) return live;

            char grade = rarity >= 3 ? 'L' : rarity == 2 ? 'E' : 'C';
            var pool = TplHeroes;
            if (pool.Length > 0)
            {
                int count = 0;
                for (int i = 0; i < pool.Length; i++)
                    if (pool[i] != null && pool[i].name.Length > 0 && pool[i].name[pool[i].name.Length - 1] == grade)
                        count++;
                if (count == 0) count = pool.Length; // grade missing → whole roster
                int pick = StableHash(name) % count;
                int seen = 0;
                for (int i = 0; i < pool.Length; i++)
                {
                    bool match = count == pool.Length ||
                        (pool[i] != null && pool[i].name[pool[i].name.Length - 1] == grade);
                    if (!match) continue;
                    if (seen == pick) return pool[i];
                    seen++;
                }
            }
            return IconAscend(Mathf.Clamp(rarity % 4, 0, 3) + 1);
        }

        public static Color CompanionTint(int rarity)
        {
            if (HasTplArt) return Color.white; // real portraits — never tint
            switch (Mathf.Clamp(rarity, 0, 5))
            {
                case 1: return new Color(0.55f, 1f, 0.65f, 1f);
                case 2: return new Color(0.55f, 0.75f, 1f, 1f);
                case 3: return new Color(0.85f, 0.55f, 1f, 1f);
                case 4: return new Color(1f, 0.88f, 0.35f, 1f);
                case 5: return new Color(1f, 0.45f, 0.45f, 1f);
                default: return Color.white;
            }
        }

        public static Color WeaponTint(int kind, int rarity)
        {
            // Real item art only ships for kind 0 — keep kit icons tinted for other kinds.
            if (kind == 0 && TplItem(TierWeapon[Mathf.Clamp(rarity, 0, TierWeapon.Length - 1)]) != null)
                return Color.white;
            Color baseTint = kind == 1
                ? new Color(0.65f, 0.85f, 1f, 1f)
                : kind == 2
                    ? new Color(1f, 0.75f, 0.95f, 1f)
                    : new Color(1f, 0.92f, 0.75f, 1f);
            return Color.Lerp(baseTint, CompanionTint(rarity), 0.35f);
        }

        // 무기 소환은 '검', 동료 소환은 '동료' 아이콘. (보석 아이콘을 쓰면 무엇을 뽑는지 안 보인다)
        public static Sprite IconSummonWeapon => First("CasualKit/IcoC_Sword", "Sprites/Icons/Summon_Weapon", "GrowArt/Icon/Plus");
        public static Sprite IconSummonAcc => First("CasualKit/IcoC_Friends", "Sprites/Icons/Summon_Acc", "GrowArt/Icon/Plus");
        public static Sprite IconSummonSkill => First("CasualKit/IcoC_Book", "Sprites/Icons/Summon_Skill", "GrowArt/Icon/Plus");
        public static Sprite IconMenuStore => First("CasualKit/IcoC_Shop", "Sprites/Icons/Menu_Store", "GrowArt/Nav5On");
        public static Sprite IconAllow => First("Sprites/Icons/Icon_Allow");
        public static Sprite IconChat => First("CasualKit/Ico_Chat", "Sprites/Icons/Icon_Chat");
        public static Sprite IconBgm => First("CasualKit/Ico_Sfx", "Sprites/Icons/Icon_BGM");
        public static Sprite IconSfx => First("CasualKit/Ico_Sfx", "Sprites/Icons/Icon_SFX");

        public static Sprite IconSetting => First("CasualKit/IcoC_Setting", "Sprites/Icons/Icon_Setting", "GrowArt/Icon/Setting");
        public static Sprite IconQuest => First("CasualKit/IcoC_Quest", "Sprites/Icons/Icon_Quest", "GrowArt/Icon/Quest");
        public static Sprite IconMail => First("CasualKit/IcoC_Mail", "Sprites/Icons/Icon_Mail", "GrowArt/Icon/Mail");
        public static Sprite IconAuto => First("CasualKit/Ico_Auto", "Sprites/Icons/Icon_Auto", "GrowArt/Icon/Auto");
        public static Sprite IconBoss => First("CasualKit/IcoC_Battle", "Sprites/Icons/Icon_Boss", "GrowArt/Icon/Boss");
        public static Sprite IconCheck => First("CasualKit/Ico_Check", "Sprites/Icons/Icon_Check", "GrowArt/Icon/Check");
        public static Sprite IconLock => First("CasualKit/Ico_Lock", "Sprites/Icons/Icon_Lock", "GrowArt/Icon/Lock");
        public static Sprite IconPlus => First("CasualKit/Ico_Plus", "Sprites/Icons/Icon_Plus", "GrowArt/Icon/Plus");
        // 기능별 추가 아이콘 (전부 컬러본)
        public static Sprite IconBag => First("CasualKit/IcoC_Bag", "Sprites/Icons/Icon_Bag");
        public static Sprite IconMap => First("CasualKit/IcoC_Map", "Sprites/Icons/Icon_Map");
        public static Sprite IconTrophy => First("CasualKit/IcoC_Trophy", "Sprites/Icons/Icon_Trophy");
        public static Sprite IconGuild => First("CasualKit/IcoC_Guild", "Sprites/Icons/Icon_Guild");
        public static Sprite IconChest => First("CasualKit/IcoC_Chest", "Sprites/Icons/Icon_Chest");

        /// <summary>0=STR 1=DEX 2=INT 3=VIT — 키트 스탯 아이콘(공격/속도/치명/체력)에 대응.</summary>
        public static Sprite IconStat(int i)
        {
            switch (i)
            {
                case 0: return First("CasualKit/Ico_Atk", "Sprites/Icons/Growth_STR", "GrowArt/Icon/StatStr");
                case 1: return First("CasualKit/Ico_Spd", "Sprites/Icons/Growth_DEX", "GrowArt/Icon/StatDex");
                case 2: return First("CasualKit/Ico_Crit", "Sprites/Icons/Growth_INT", "GrowArt/Icon/StatInt");
                default: return First("CasualKit/Ico_Hp", "Sprites/Icons/Growth_VIT", "GrowArt/Icon/StatVit");
            }
        }

        /// <summary>Keys: Attack, AttackSpeed, Hp, HpRegen, Accuracy.</summary>
        public static Sprite IconEnhance(string key)
        {
            string kit, ico;
            switch (key)
            {
                case "Attack": kit = "Enhance_Attack"; ico = "Ico_Atk"; break;
                case "AttackSpeed": kit = "Enhance_AttackSpeed"; ico = "Ico_Spd"; break;
                case "Hp": kit = "Enhance_HP"; ico = "Ico_Hp"; break;
                case "HpRegen": kit = "Enhance_HPRegen"; ico = "Ico_Hp"; break;
                default: kit = "Enhance_Accuracy"; ico = "Ico_Def"; break;
            }
            return First("CasualKit/" + ico, "Sprites/Icons/" + kit, "GrowArt/Icon/Enhance" + key);
        }

        // Nav order matches HUD labels: 캐릭터 / 장비 / 스킬 / 무기 / 동료
        static readonly string[] NavIconNames = { "Menu_Chr", "Menu_Gear", "Menu_Skill", "Summon_Weapon", "Menu_Ally" };
        // 네비는 컬러 아이콘을 쓴다 (Pictoicon은 흰 실루엣이라 단조롭다):
        // 캐릭터=역할 / 장비=방패 / 스킬=무공서 / 무기=검 / 동료=동료
        // 캐릭터 탭은 '왕관'(내 캐릭터) — Role 아이콘은 톱니처럼 보여 캐릭터로 안 읽혔다
        static readonly string[] NavKitNames =
            { "IcoC_Crown", "IcoC_Shield", "IcoC_Book", "IcoC_Sword", "IcoC_Friends" };

        public static Sprite NavOn(int i)
        {
            int n = Mathf.Clamp(i, 0, 4);
            return First("CasualKit/" + NavKitNames[n], "Sprites/Icons/" + NavIconNames[n],
                "GrowArt/Nav" + (n + 1) + "On", "FreePack/UI/Nav" + (n + 1) + "On");
        }

        public static Sprite NavOff(int i) => NavOn(i);

        static readonly string[] SkillIconNames =
        {
            "Skill_LightningFang", "Skill_HeavenfallBlade", "Skill_DragonsWrath", "Skill_WindsofHaste",
            "Skill_JudgementBreaker", "Skill_CurseMark", "Skill_DoomSpiral", "Skill_FuryInfusion"
        };

        // 구매 에셋의 스킬 아이콘 3종 + 패시브 1종을 순환 사용
        static readonly string[] SkillKitIcons =
            { "Ico_Skill1", "Ico_Skill2", "Ico_Skill3", "Ico_Book" };

        // 트리 id → 아이콘 파일 접두어 (Resources/SkillIcons/sk_{접두어}_{노드id}.png)
        static string SkillTreePrefix()
        {
            string t = IdleMvp.Core.JobProgress.TreeId;
            if (t == "bowmaster") return "bow";
            if (t == "archmage") return "mage";
            if (t == "hidden") return "hidden";
            return "hero";
        }

        /// <summary>
        /// 현재 트리의 노드 i 아이콘. 직접 만든 무협 아이콘(Resources/SkillIcons)이
        /// 있으면 그것을, 없으면 키트 추상 아이콘을 순환 사용(기존 동작).
        /// </summary>
        public static Sprite SkillIcon(int i)
        {
            var mine = Resources.Load<Sprite>("SkillIcons/sk_" + SkillTreePrefix() + "_" + i);
            if (mine != null) return mine;
            int n = ((i % SkillIconNames.Length) + SkillIconNames.Length) % SkillIconNames.Length;
            int k = ((i % SkillKitIcons.Length) + SkillKitIcons.Length) % SkillKitIcons.Length;
            return First("CasualKit/" + SkillKitIcons[k],
                "Sprites/Icons/" + SkillIconNames[n], "Sprites/Icons/Skill_LightningFang");
        }

        /// <summary>직접 만든 아이콘은 이미 채색돼 있어 색조를 덧입히면 안 된다.</summary>
        public static bool SkillIconIsDedicated(int i)
            => Resources.Load<Sprite>("SkillIcons/sk_" + SkillTreePrefix() + "_" + i) != null;

        // Kit skill icons are white silhouettes — per-skill hue makes them readable & distinct.
        static readonly Color[] SkillHues =
        {
            new Color(1.00f, 0.85f, 0.30f), // 0 lightning — gold
            new Color(1.00f, 0.45f, 0.25f), // 1 meteor — ember
            new Color(0.45f, 0.80f, 1.00f), // 2 ice — cyan
            new Color(0.85f, 0.55f, 1.00f), // 3 scream — violet
            new Color(0.55f, 1.00f, 0.55f), // 4 passive — green
            new Color(1.00f, 0.65f, 0.80f), // 5 passive — pink
            new Color(0.65f, 0.75f, 1.00f), // 6 passive — steel blue
            new Color(1.00f, 0.95f, 0.70f), // 7 passive — pale gold
        };

        public static Color SkillTint(int i)
        {
            // 전용 무협 아이콘은 이미 채색돼 있다 — 색조를 덧입히면 뭉개진다
            if (SkillIconIsDedicated(i)) return Color.white;
            int n = ((i % SkillHues.Length) + SkillHues.Length) % SkillHues.Length;
            return SkillHues[n];
        }

        /// <summary>Hunt mob variety from GrowArt/Chars/Enemy1..6 (falls back to Enemy).</summary>
        public static Sprite EnemyVariant(int seed = -1)
        {
            int idx = seed < 0 ? Random.Range(1, 7) : (seed % 6) + 1;
            return First("GrowArt/Chars/Enemy" + idx, "GrowArt/Chars/Enemy1", "FreePack/Enemy/Enemy" + idx, "Enemy");
        }

        public static Sprite EnemyMiniBoss => First("GrowArt/Chars/EnemyMiniBoss", "FreePack/Enemy/EnemyMiniBoss", "Enemy");
        public static Sprite EnemyBoss => First("GrowArt/Chars/EnemyBoss", "FreePack/Enemy/EnemyBoss", "Enemy");

        public static Sprite First(params string[] paths)
        {
            if (paths == null) return null;
            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (string.IsNullOrEmpty(path)) continue;
                // Allow chaining logical names like "PanelFrame" / "Enemy" as last-resort aliases.
                if (path.IndexOf('/') < 0)
                {
                    switch (path)
                    {
                        case "PanelFrame": return PanelFrame;
                        case "UpgradeButton": return UpgradeButton;
                        case "CircleFrame": return CircleFrame;
                        case "SquareFrame": return SquareFrame;
                        case "Enemy": return Enemy;
                        default: continue;
                    }
                }
                var s = Load(path);
                if (s != null) return s;
            }
            return null;
        }

        // Resources.LoadAll은 캐시가 없어서 부를 때마다 디스크/번들을 뒤진다.
        // 카드 36장을 그리는 루프에서 아이콘마다 불려 스크롤이 끊겼다. 결과를 기억해 둔다.
        // (못 찾은 경로도 null로 기억해야 매번 재조회하지 않는다)
        static readonly Dictionary<string, Sprite> _loadCache = new Dictionary<string, Sprite>(256);

        public static Sprite Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            Sprite cached;
            if (_loadCache.TryGetValue(path, out cached)) return cached;
            var result = LoadUncached(path);
            _loadCache[path] = result;
            return result;
        }

        static Sprite LoadUncached(string path)
        {
            // Multiple-mode kit sheets: LoadAll returns sub-sprites with correct borders.
            var all = Resources.LoadAll<Sprite>(path);
            if (all != null && all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].name != null &&
                        (all[i].name.EndsWith("_0") || all[i].name == System.IO.Path.GetFileName(path)))
                        return all[i];
                }
                return all[0];
            }

            var s = Resources.Load<Sprite>(path);
            if (s != null) return s;

            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) return null;

            // Avoid 40% synthetic borders (they crush corners on titles/tabs).
            Vector4 border = KnownSliceBorder(path, tex.width, tex.height);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        /// <summary>Conservative kit-matched borders for Texture2D fallback only.</summary>
        static Vector4 KnownSliceBorder(string path, int w, int h)
        {
            if (string.IsNullOrEmpty(path) || !NeedsSliceBorder(path)) return Vector4.zero;
            string p = path.Replace('\\', '/');
            if (p.IndexOf("Popup_Bg", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(48, 48, 48, 48);
            if (p.IndexOf("Popup_Title", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(54, 44, 54, 44);
            if (p.IndexOf("Frame_Img", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(35, 35, 35, 35);
            if (p.IndexOf("Frame_Round_Black", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("CardDark", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(40, 30, 40, 30);
            if (p.IndexOf("Frame_List", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("InvSlot", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(30, 30, 30, 30);
            if (p.IndexOf("Frame_Edge", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("Rarity", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(40, 40, 40, 40);
            if (p.IndexOf("Frame_Profile", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("CircleFrame", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(28, 28, 28, 28);
            if (p.IndexOf("Bar_Back", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(15, 12, 15, 12);
            if (p.IndexOf("Bar_Front", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(12, 10, 12, 10);
            if (p.IndexOf("Btn_", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.IndexOf("Button", System.StringComparison.OrdinalIgnoreCase) >= 0) return new Vector4(35, 25, 35, 25);
            // Cap at ~18% of min side — never 40%.
            float b = Mathf.Floor(Mathf.Min(w, h) * 0.18f);
            b = Mathf.Clamp(b, 8f, 40f);
            return new Vector4(b, b, b, b);
        }

        static bool NeedsSliceBorder(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.IndexOf("Frame", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Modal", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Button", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Btn", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Panel", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Bar", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Card", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Slot", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Fog", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Shop", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Tab", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || path.IndexOf("Rarity", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void Apply(UnityEngine.UI.Image img, Sprite sprite, bool preserveAspect = true, bool sliced = false)
        {
            if (img == null || sprite == null) return;
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = preserveAspect;
            if (sliced)
            {
                img.type = UnityEngine.UI.Image.Type.Sliced;
                img.preserveAspect = false;
            }
            else
                img.type = UnityEngine.UI.Image.Type.Simple;
        }

        public static void Tint(UnityEngine.UI.Image img, Color tint)
        {
            if (img == null) return;
            img.color = tint;
        }
    }
}
