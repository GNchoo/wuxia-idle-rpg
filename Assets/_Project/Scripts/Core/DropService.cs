using IdleMvp.Adapters;
using UnityEngine;

namespace IdleMvp.Core
{
    /// <summary>
    /// 필드 드랍 — 몹을 잡으면 골드 외에 실물이 떨어진다.
    ///
    /// 규칙 (키우기류 표준):
    ///  - 사냥터(챕터)가 높을수록 좋은 것이 잘 나온다
    ///  - 장비(무기)는 **내 직업이 들 수 있는 종류 + 내 레벨대 티어**만 나온다
    ///    — 못 쓰는 드랍은 보상이 아니라 소음이다
    ///  - 보스는 배율이 크게 붙는다
    /// </summary>
    public static class DropService
    {
        public struct Drop
        {
            public string Label;   // FX 로 띄울 문구 ("+물약", "+강화석 12" ...)
            public Color Tint;
        }

        /// <summary>킬 1건 처리. 드랍이 있으면 Drop, 없으면 null.</summary>
        // 확률 공시 화면(ShowRateDisclosure)이 같은 상수를 읽는다 — 여기만 고치면 공시도 맞다.
        public const float WeaponRate = 0.004f;
        public const float PotionRate = 0.015f;     // 사다리 폭: 0.019 - 0.004
        public const float StoneRate = 0.035f;      // 0.054 - 0.019
        public const float ArtifactRate = 0.015f;   // 0.069 - 0.054
        public const float BossDropMul = 6f;

        public static Drop? RollOnKill(int chapter, bool boss)
        {
            float mul = boss ? BossDropMul : 1f;
            float roll = Random.value;

            // 확률 사다리 — 위에서부터 하나만 걸린다 (합계 일반 ~9%, 보스 ~54%)
            if (roll < WeaponRate * mul)
            {
                var w = RollWeapon(chapter);
                if (w != null)
                    return new Drop { Label = "+" + w, Tint = new Color(1f, 0.72f, 0.2f) };
                roll = 1f; // 무기 실패 시 이번 킬은 통과 (아래 사다리 재진입 방지)
            }
            if (roll < (WeaponRate + PotionRate) * mul)
            {
                PotionService.Grant(1);
                return new Drop { Label = "+물약", Tint = new Color(0.4f, 1f, 0.5f) };
            }
            if (roll < (WeaponRate + PotionRate + StoneRate) * mul)
            {
                int n = 4 + chapter * 2;
                Economy.CurrencyWallet.Instance?.Add(Economy.CurrencyId.WeaponEnhanceStone, n);
                return new Drop { Label = "+강화석 " + n, Tint = new Color(0.6f, 0.85f, 1f) };
            }
            if (roll < (WeaponRate + PotionRate + StoneRate + ArtifactRate) * mul)
            {
                var arts = ContentCatalog.Artifacts;
                if (arts != null && arts.Length > 0)
                {
                    var a = arts[Random.Range(0, arts.Length)];
                    IdleMvp.Progression.ArtifactService.Instance?.GrantFragment(a.id, 1);
                    return new Drop { Label = "+유물조각", Tint = new Color(0.85f, 0.6f, 1f) };
                }
            }
            return null;
        }

        /// <summary>
        /// 직업이 들 수 있는 종류 중 하나를 골라, 내 레벨대 티어의 무기를 떨군다.
        /// 등급은 챕터가 정한다 — 초반은 일반·고급, 후반으로 갈수록 희귀·영웅.
        /// </summary>
        static string RollWeapon(int chapter)
        {
            var all = ContentCatalog.Weapons;
            if (all == null || all.Length == 0) return null;

            // 1) 직업 허용 종류 수집
            var kinds = new System.Collections.Generic.List<int>();
            for (int k = 0; k < 4; k++)
                if (JobProgress.WeaponMatchesJob(k)) kinds.Add(k);
            if (kinds.Count == 0) { kinds.Add(0); }
            int kind = kinds[Random.Range(0, kinds.Count)];

            // 2) 그 종류의 무기들을 카탈로그 순서(=티어 순)로 모은다
            var pool = new System.Collections.Generic.List<WeaponDef>();
            for (int i = 0; i < all.Length; i++)
                if (all[i].kind == kind) pool.Add(all[i]);
            if (pool.Count == 0) return null;

            // 3) 내 레벨 → 티어 밴드 (레벨 캡 60을 풀 길이에 사상, ±1 흔들림)
            int level = IdleMvp.Progression.PlayerGrowth.Instance != null
                ? IdleMvp.Progression.PlayerGrowth.Instance.Level : 1;
            int center = Mathf.Clamp(level * pool.Count / 60, 0, pool.Count - 1);
            int idx = Mathf.Clamp(center + Random.Range(-1, 2), 0, pool.Count - 1);
            var def = pool[idx];

            // 4) 등급은 챕터 테이블
            var rarity = RollRarity(chapter);
            WeaponSummonAdapter.Instance?.GrantDrop(def, rarity);
            return def.name;
        }

        static GachaRarity RollRarity(int chapter)
        {
            float r = Random.value;
            // 챕터가 오를수록 상위 등급 문턱이 낮아진다.
            // 전설은 필드 드랍에서 나오지 않는다 — 가챠의 가치를 지킨다.
            float epic = 0.02f + chapter * 0.008f;      // ch10에서 10%
            float rare = 0.12f + chapter * 0.02f;       // ch10에서 32%
            if (r < epic) return GachaRarity.Epic;
            if (r < epic + rare) return GachaRarity.Rare;
            return GachaRarity.Common;
        }
    }
}
