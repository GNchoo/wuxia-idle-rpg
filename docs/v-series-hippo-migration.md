# V 시리즈 — 캐릭터 에셋 전면 교체: SP1 → Hippo Character Editor Megapack (2026-08-17)

유저 결정: 페이퍼돌 자작 리그의 퀄리티 한계 인정 → Hippo Megapack($49.90) 구매.
기존 캐릭터 에셋(SP1)을 전부 걷어내고 전면 교체한다.

## 원칙
- **매 커밋마다 게임이 구동되는 상태 유지** — SP1 제거는 마지막 단계.
- 무협 파츠는 로컬 Z-Image 라인으로 생성해 Hippo 규격(SpriteCollection)에 등록.
- 벤치마크 정책 유지: 시스템·구조 참고는 자유, 타 게임 아트·명칭 복제 금지.

## 단계

### V1. 구조 파악 + 어댑터
- Hippo API 전수 조사 (탐색 에이전트) → 통합 표면 확정.
- `HippoActorController` 어댑터: SP1 `PlayAnimation(name, fade)` 규약 ↔ Hippo
  Animator 상태/트리거 매핑. CharacterActorView가 SP1·페이퍼돌·Hippo 3종을
  같은 방식으로 몰 수 있게.

### V2. 히어로 교체
- 히어로 프리팹을 Hippo 캐릭터로 (기본 서양 파츠로 임시 조립 — 무협화는 V4).
- 무기 앵커: 티어 무기 30종(WearParts)을 Hippo 손 본에 장착.
- 티어 방어구 → Hippo 파츠 매핑 (장비 = 외형 원칙 유지).

### V3. 몹·동료 교체
- 몹 프리셋(산적·녹림도·흑풍단·마인 등)은 전부 인간형 → Hippo 캐릭터로 재조립.
  챕터별 캐스팅·틴트 체계 이식. 동료 2종도 동일.
- 스타일 통일: 주인공만 다른 그림 문제 원천 해소.

### V4. 무협화
- 무협 의상·헤어를 로컬 생성 라인으로 뽑아 SpriteCollection에 등록.
- 외형 꾸미기 화면을 Hippo 파츠 기준으로 재배선 (신체·헤어만 원칙 유지).
- 표정: Hippo 자체 Expression 시스템 사용.

### V5. SP1 완전 제거
- Assets/SP1 삭제, CharPresets·Wuxia 파이프라인(SP1 전용) 정리.
- WuxiaPartInstaller/BoneBaker → Hippo 등록기로 대체.
- 페이퍼돌(HeroTall)·A-포즈 파이프라인 정리 (기록은 커밋 히스토리에 남음).

### V6. 검증
- 전 화면 스크린샷 + 전투 플로우(사냥·돌파·보스·던전) 실동작.
- AAB 재빌드 (SP1 제거로 용량 절감 기대).

## 리스크
- Hippo는 Built-in RP 전용 — 우리 프로젝트 Built-in ✓.
- 몹 수(SP1 프리셋 9종+틴트) 재현에 파츠 다양성 필요 — Megapack 규모로 커버 예상.
- 애니메이션 이벤트(타격 타이밍) 재보정 필요.
