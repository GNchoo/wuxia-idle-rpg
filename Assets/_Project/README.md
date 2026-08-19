# Idle MVP (_Project)

키우기형 방치 RPG의 **제품 코드**는 여기만 사용한다.

## 정지선

- `IdleRPG_Assets` = 참고·스프라이트 창고. **GameScene에 MVP 추가 개조 금지**
- 플레이 진입: Build Settings → `_Project/Scenes/Boot` → `Meta`

## 씬

| 씬 | 역할 |
|----|------|
| Boot | 서비스 스폰 후 Meta 로드 |
| Meta | 하단 탭: 전투 / 캐릭터 / 방치·상점·던전 |
| Battle | 단독 자동사냥(탭에서도 동일 로직) |

## 스크립트

- `Boot/` — GrowBootLoader, GrowGameBootstrap
- `Combat/` — AutoBattleController
- `Progression/` — StageProgress, PlayerGrowth, EquipmentService, DungeonService, StageTable
- `Economy/` — LootBoxService, PlayerWallet
- `UI/` — GrowAppShell, GrowTheme, KoreanUiFont

## 구 템플릿 브릿지

`MvpBootstrap` / `TemplateGameplayBootstrap` / `TemplateWaveAdapter` / `MvpHudController`는 레거시.  
Grow 씬에서는 기동하지 않는다.
