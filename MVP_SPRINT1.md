# NewRPG — Idle MVP Layer

템플릿 기반 키우기형 MVP 1차 적용 요약.

## 변경 사항

- `Assets/_Project/` — Stage / LootBox / PlayerGrowth / HUD / Bootstrap
- Build Settings: Login 비활성 → **LoadingScene V1 → GameScene V1**
- Product Name: **방치형RPG**
- 세이브 경로(기존): `persistentDataPath/JSON/`

## Play 확인

1. Unity에서 프로젝트 포커스 → 스크립트 컴파일 대기
2. ▶ Play (또는 LoadingScene V1에서 Play)
3. GameScene 상단에 MVP HUD (스테이지 / 상자 / 레벨)
4. **전리품 수령** 버튼으로 Claim
5. 마을·팀 탭은 비활성(“준비중”)

Console에 `[IdleMvp] Bootstrap ready` 로그가 보이면 성공입니다.
