# Grow + 메이플 UI 플레이 가이드

## 실행

1. `Assets/_Project/Scenes/Boot` Play
2. Canvas는 **가로 1920×1080** (Game 뷰 Aspect를 16:9로)

## 흐름

1. **서버선택** → 선택 완료  
2. **전직(히어로)**  
3. **메인 사냥 HUD** — 도전=스테이지 돌파  
4. 하단 탭: 캐릭터 / 장비 / 스킬 / 무기 / 동료  
5. 상단: 길드·우편 / 상점 / 메뉴

## V1 Ready (로컬 완결)

- 길드(연습 멤버), 우편, 맵, 상점, 패스, 이벤트(출석), 던전(소탕), 코스튬
- 아레나(점수/티어/일일 5회, 우편 보상), 월드보스(타격+필드 진입), 핫딜
- 오프라인 보상(실 누적), 무기/동료/스킬/장비 성장 루프
- 콘텐츠: 챕터 10 · 스테이지 100 · 레벨캡 60
- 오디오: BGM + UI/전투/보상 SFX (설정에서 토글)

## Coming Soon (메뉴에서 준비중)

- 채팅 (온라인 백엔드 필요), 뷰티(헤어)

## UI

- 전체 TMP(UIHangulSDF, 아웃라인 내장) + FantasyIdleGameGUI 킷 스프라이트
- 듀얼 모달은 좌(프리뷰/스탯)+우(관리) 도킹 패널, 중앙 필드 노출
- 에디터 시각 검증: `MapleMainHud.DebugOpen(id)` + `tools/unity_bridge.py`

## 레퍼런스

`docs/ui-refs/README.md` · `Assets/_Project/Docs/UiRefs/`  
소프트런치 QA: `docs/qa-soft-launch.md`

## 정지선

GameScene Letterbox 전투 경로 사용 안 함.
