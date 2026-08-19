# 전투·콘텐츠 확장 로드맵 (Phase N) — 2026-08-12 조사 기준

## 현재 상태 진단 (조사 결과)

### 무기
- 데이터는 24종(검/지팡이/활/단검 × 각 6, `Resources/Content/weapons.json`)이지만,
  **캐릭터 리그가 든 무기 그래픽이 장착 무기와 미연결** — 커스터마이징 시점 무기가 고정 표시.
  kind는 공격 애니메이션만 바꿈 (`FieldAutoHuntController.ResolveHeroAppearance`).
- SP1 WeaponR 슬롯에 아이템 28종 존재 (스냅샷 분류 완료):
  - 검: 0002(단검형)·0007(레이피어)·0014(대검)·1003(화염검)
  - 지팡이: 0011(케인)·0019(삼지창)·0006(횃불)
  - 활: 1004(나무활)·1001(흑궁)·1002(화염활)
  - 단검: 0001(나이프)·0020(낫)·0017(갈고리)

### 스킬
- 액티브 4종 = 전부 "최근접 1체 × 배율" 단일타. FX는 데미지와 무관한 장식 4종(skillId%4).
- 트리 3종(hero/bowmaster/archmage)은 이름만 다르고 수치·구조 동일. **메커니즘 필드 없음.**
- 투사체 개념 자체가 코드에 없음 (활·마법도 히트스캔).

### 적/맵
- 몹 6종 고정 풀에서 **챕터 무관 완전 랜덤**. 보스 = Berserker 1종 스탯 뻥튀기, 패턴 0.
- 몹은 공격 애니·판정 없음 (추상 DPS가 히어로 HP를 깎음). 아크메이지 외형(Peasant)이 잡몹과 동일.
- 맵: 3층 상수 구조, 챕터는 배경 1장만 교체. stage-table.json에 티어/배율 필드 미기입 → Hard/Hell 死로직.
- TplArt 보스 158장·CharPresets 9종 중 실전 노출 극소.

### 미구현
- 채팅(백엔드 필요·영구 스텁), FeatureGate.HasClaimable 하드코딩 false, 출석 특별미션,
  튜토리얼(P1), 일일미션/업적/도감(P2), 데미지 숫자, FX 풀링.

## 구현 순서 (Phase N)

### N-1. 무기 체감 ✦ 최우선
- `AppearanceService.WeaponOverrideItem` + `WeaponItemFor(kind, rarity)` 매핑 테이블(위 분류).
- `ApplyTo()` 마지막에 WeaponR 오버라이드 → 필드 히어로/캐릭터 페이지/장비 프리뷰 전부 자동 반영.
- EnsureHero 리빌드 키에 무기 아이템 토큰 추가.

### N-2. 직업 스탠다드 외형
- 8직업 각각 구분되는 SP1 슬롯 프리셋(색·모자·갑옷) 정의 — `JobDefault(jobId)` 확장.
- 아크메이지=Peasant(잡몹 중복) 해소: 로브 톤 + 지팡이. 몹 캐스팅에서 Peasant 제거 검토.

### N-3. 스킬 메커니즘 시스템 ✦ 핵심
- `SkillNodeDef.Mechanic` 추가: `Single / MultiHit(n) / Projectile / Pierce / Homing / AoE`.
- 트리별 배정: hero=다단히트·회전베기(AoE), bowmaster=투사체·관통·멀티샷(유도),
  archmage=유도 매직볼트·광역 폭발(메테오).
- `FieldCombatFx`에 투사체 엔진: `PlayProjectile(sprite, from, to, arc, speed, onHit)` —
  화살/볼트가 실제로 날아가 명중 시 데미지. Homing은 타겟 추적 보간. AoE는 반경 스캔 + 폭발 FX.
- 데미지 숫자 팝업(크리티컬 강조) 추가.

### N-4. 적·맵 다양화
- 몹 캐스팅 챕터별 풀(2차원 배열) + 색 변형(틴트)으로 체감 종류 확대.
- 챕터별 보스: TplArt/Bosses 스프라이트 활용(스프라이트 폴백 경로 재사용).
- 몹 공격 애니메이션 재생 + 접근·공격 판정(추상 DPS 대체).
- stage-table.json에 mapTier/mobHpMul 기입 → Hard/Hell 티어 활성화.
- 층별 강적: 상층(2층) 몹 HP/ATK 보정 + 전용 틴트.

### N-5. 미구현 기능
- FeatureGate.HasClaimable 실동작, 출석 특별미션, 튜토리얼 시퀀스(P1), 일일미션/업적/도감(P2).
- 채팅은 백엔드 필요로 보류(문서화).

## 검증
각 단계: 에디터 플레이 → 스크린샷/전투 로그 → 콘솔 에러 0 → 커밋.
N-3은 치트로 스킬 강제 시전하며 투사체/광역 시각 확인.
