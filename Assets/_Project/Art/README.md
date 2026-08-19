# Art (유료 팩 활용)

`IdleRPG_Assets`는 **시스템 템플릿이 아니라 아트/UI 창고**로 사용합니다.

런타임 로드: `Resources/GrowArt/` (+ `CasualGui/`, `FreePack/`)
- UI: soft CasualGui 프레임 (또는 Free Casual GUI Import 후 덮어쓰기)
- Chars: CC0 귀여운 몹 `Enemy1..6` / Hero
- SkillIcon / Nav / IconGold: IdleRPG_Assets 추출
- Fx/Hit*: IdleRPG_Assets Spells 추출 → `FieldCombatFx`
- Fx/Lightning|Meteor|Ice|Scream|HitBurst: Spells 시퀀스 (Lightning2/comet/Tornado/Hypno+Skull/Explosion) → 스킬 프레임 애니

키우기형 루프는 `_Project` Boot/Meta에 두고, 비주얼만 슬롯 교체합니다.  
유료 Idle UI·SD 영웅 구매: `docs/ui-refs/paid-asset-buy-guide.md`
