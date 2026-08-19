# 무료 에셋 적용 + 유료 교체 가이드

전략: **무료로 UX/루프를 완성**한 뒤, 유료 Idle UI·SD 캐릭을 `GrowArt` 동명 슬롯으로 덮는다.

로드 우선순위 ([`GrowArt.cs`](../../Assets/_Project/Scripts/UI/GrowArt.cs)):  
`GrowArt` → `CasualGui` → `FreePack` → `MvpUi`

유료 구매 상세: [paid-asset-buy-guide.md](./paid-asset-buy-guide.md)

---

## 1) 지금 적용된 조합

| 경로 | 소스 | 용도 |
|---|---|---|
| `Resources/CasualGui/` | 소프트 파스텔 라운드 프레임 (Free Casual GUI 대기용 슬롯) | 패널·버튼·바 |
| `Resources/FreePack/UI/` | 위와 동일 슬롯명으로 동기화 | 폴백 |
| `Resources/FreePack/Enemy/` + `GrowArt/Chars/Enemy*` | OpenGameArt CC0 enemies (deadbunny, Gas-o, hencher 등) | 필드 몹 |
| `GrowArt/SkillIcon*` / `Nav*` | IdleRPG_Assets (보유 유료) | 스킬·네비 |
| `GrowArt/Fx/Hit*` | IdleRPG_Assets Spells Lightning | 타격 FX |
| HUD 골격 | 코드 토큰 (다크 칩 + 틸 선택) | 메이플키우기 레이아웃 |

### Free Casual GUI (Asset Store 무료) — 사용자 1회 Import

1. https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804  
2. **Add to My Assets** → Package Manager → **Import**  
3. 프로젝트에서 실행:

```powershell
powershell -File Assets/_Project/Tools/Map-FreeCasualGui.ps1
```

스크립트가 PNG를 `FreePack/UI`, `CasualGui`, `GrowArt` 슬롯명으로 복사한다.

---

## 2) 무료 몹 출처

- OpenGameArt `enemies.zip` (CC0) — `deadbunny`, `Gas-o`, `hencher`, `monky`, `pissyfish`, `unibug`, `wasp`  
- 캐시: `_FreeAssetCache/cc0_enemies_ex/` (git 제외 권장)  
- itch [Free Simple Monster Sprite Sheet](https://skyflakes-lab.itch.io/free-monster-sprite-sheet) — 추가 교체 후보 (수동 다운로드)

---

## 3) 보유 유료 팩 부품 회수

`IdleRPG_Assets` → `Resources/GrowArt/Fx/`:

| 파일 | 출처 |
|---|---|
| `Hit1~3.png` | Lightning2 프레임 |
| `SkillPulse.png` | tesla_ball |
| `SkillIconFx.png` | Lightning2 Icon |

전투: [`FieldCombatFx`](../../Assets/_Project/Scripts/Combat/FieldCombatFx.cs)가 타격 시 재생 (+ 짧은 비프).  
템플릿 GameScene/SAMPLETEXT 시스템은 **연결하지 않음**.

---

## 4) 유료 최종 교체 (요약)

| 영역 | 추천 | 가격 |
|---|---|---|
| GUI | Mobile Fantasy Idle UI Kit | $19 |
| 영웅 | 2D Character Maker RPG Bundle | 스토어 확인 |
| 몹 | **2D Monster Cute & Chibi: Forest** (슬라임·고블린) | ~€13.80 |
| 몹 대체 | 2D Slimes & Animations | $6.99 |

절차·슬롯표: [paid-asset-buy-guide.md](./paid-asset-buy-guide.md)
