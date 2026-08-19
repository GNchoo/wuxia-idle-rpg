namespace IdleMvp.Progression
{
    /// <summary>
    /// 직업 → 기본 프리셋 매핑과 외형 변경 통지 이벤트.
    /// 커스터마이징·장비 외형의 실제 적용은 HippoLookService가 맡는다.
    /// (SP1 시절의 CharacterData·슬롯 리컬러 체계는 V5에서 제거됨)
    /// </summary>
    public static class AppearanceService
    {
        public static event System.Action OnChanged;

        // job id → CharPresets prefab used as the default look.
        static readonly (string job, string preset)[] JobPresets =
        {
            ("hero", "Warrior"),
            ("paladin", "Berserker"),
            ("darkknight", "Raider"),
            ("bowmaster", "Bandit Bowman"),
            ("marksman", "Bandit Bowman"),
            ("archmage", "Peasant"),
            ("bishop", "Peasant"),
            ("nightlord", "Bandit Cutthroat"),
        };

        public static string PresetForJob(string jobId)
        {
            foreach (var (job, preset) in JobPresets)
                if (job == jobId) return preset;
            return "Warrior";
        }

        /// <summary>장착 무기·장비·외형이 바뀌었다 — 프리뷰 리그들(HUD 초상화·장비창)이 다시 그리게 한다.</summary>
        public static void NotifyWeaponChanged() { OnChanged?.Invoke(); }
    }
}
