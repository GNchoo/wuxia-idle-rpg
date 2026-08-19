# 메이플 키우기 UI 레퍼런스

캡처 원본: `Assets/_Project/Docs/UiRefs/`

## 확보된 캡처

| 파일 | 화면 |
|---|---|
| `01-main-hud-kerning.png` | 메인 사냥 HUD (커닝시티) |
| `02-main-hud-perion.png` | 메인 사냥 HUD (페리온) |
| `03-equipment-modal.png` | 장비 관리 모달 |
| `04-character-modal.png` | 캐릭터 강화 모달 |
| `05-costume.png` | 코스튬 |
| `06-shop-event.png` | 상점 이벤트/일반 |
| `07-shop-membership.png` | 멤버십 상세 |
| `08-fast-hunt.png` | 빠른 사냥 팝업 |
| `09-skill-modal.png` | 스킬 모달 |
| `10-offline-reward.png` | 오프라인 보상 |
| `11-beauty.png` | 뷰티 |
| `12-job-advance.png` | 전직 |
| `13-server-select.png` | 서버선택 |

## 미확보 (스텁 / 추가 요청)

우선순위 높은 순:

1. 무기 소환 / 무기 인벤 (10연 결과, 장착·각성)
2. 동료 (메인/서브 배치, 소환)
3. 유물
4. 슬롯 강화 (주문서 / 스타포스 / 잠재)
5. 엘리트 몬스터 소환
6. 성장 던전 목록·전투 진입
7. 길드 / 아레나 / 월드보스·레이드
8. 우편 / 햄버거 메뉴(설정)
9. 스테이지 돌파·보스전 전용 UI
10. 일일/배틀패스

구현: `Assets/_Project/Scripts/UI/Maple/` (`MapleMainHud`, `MapleUiTheme`, `MapleModalHost`)

## 에셋 (개발용)

- 무료 적용 현황 + Free Casual GUI Import: [free-casual-gui.md](./free-casual-gui.md)  
- **유료 구매 안내** (Idle UI $19 / 영웅 / 몹): [paid-asset-buy-guide.md](./paid-asset-buy-guide.md)  

지금: `CasualGui` 소프트 UI + CC0 귀여운 몹 + 보유팩 스킬아이콘/타격FX.  
HUD 골격은 Maple 코드 토큰(다크 칩) 유지.
