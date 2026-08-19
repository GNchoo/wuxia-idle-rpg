# 무협 아트 파이프라인 — 아이콘 + 착용

## 결론: 착용 가능하다

SP1 리그는 Unity **Sprite Library / Sprite Resolver**를 쓴다.
스프라이트를 라이브러리에 등록하면 `ChangeSlotItem(slot, index)` 한 줄로 교체되고,
**애니메이션(휘두르기)은 리그가 알아서 적용**한다. 새 아트를 넣을 수 있다는 뜻이다.

## 실측한 규격 (Character_RPG.spriteLib)

| 항목 | 값 |
|---|---|
| 라벨 형식 | `WeaponR_Weapon_Fixed_0001` = `{카테고리}_{itemIndex}` |
| 카테고리 | 슬롯마다 **Fixed / Main / Sub 3레이어** (색상 커스터마이징용) |
| 무기 파츠 수 | `WeaponR_Weapon_Fixed` 23종, Main 3종, Sub 2종 |
| 피벗 | **스프라이트 정중앙** (예: 85×57 → pivot 42.5, 28.5) |
| PPU | 100 |
| 크기 | 85×57 ~ 201×124 px (작다 — 아이콘용 1024px와 전혀 다르다) |

`ChangeSlotItem` 내부:
```csharp
resolver.SetCategoryAndLabel(category, category + "_" + itemIndex);
```
→ 해당 라벨이 없으면 `Empty`로 떨어진다(그래서 안 보임).

## 아이콘용과 착용용은 별개 아트다

| | 아이콘 | 착용 |
|---|---|---|
| 구도 | 45도 기울임, 멋있게 | **옆모습**, 리그 방향에 맞춤 |
| 피벗 | 무관 | **손잡이가 중앙 근처**에 오도록 구도 |
| 이펙트 | 광휘·불꽃 OK | 애니메이션과 충돌 → **빼야 함** |
| 크기 | 1024px | ~200px, PPU 100 |

같은 그림을 양쪽에 쓰면 각도와 이펙트가 따로 논다. **따로 생성한다.**

## ⚠️ 함정: PSB 자동생성 spriteLib는 직접 못 고친다 (실측)

`Character_RPG.spriteLib`는 `Character_RPG.psb` 임포트 결과물이다.
`SpriteLibraryAsset.AddCategoryLabel()` + `SaveAssets()` 로 라벨을 추가하면
**호출 직후 GetSprite는 성공하지만**, Play를 재시작하면 사라진다:

```
등록됨=True sprite=WeaponR_Weapon_Fixed_0004   ← 추가 직후엔 보인다
(Play 재시작 후)
WeaponR 아이템 수=28  9001 포함=False          ← 날아갔다
ChangeSlotItem("WeaponR","9001") → 전 레이어 Empty
```

게다가 `ChangeSlotItem`은 **Init 때 만든 목록**만 본다:
```csharp
if (_category_ItemIndexList_Map[category].Contains(itemIndex)) ... else "Empty";
```
목록에 없는 인덱스를 넘기면 조용히 Empty가 된다(에러 없음).

**따라서 새 무기를 넣으려면 PSB를 건드리는 대신:**
- 기존 라이브러리를 **복제한 새 SpriteLibraryAsset**을 만들고 거기에 새 파츠를 추가한 뒤,
  런타임에 `SpriteLibrary.spriteLibraryAsset` 을 그 에셋으로 **교체**한다.
- 교체 후 `Init()`을 다시 태워 `_category_ItemIndexList_Map` 을 재구축해야 한다.

### 복제 경로 검증 결과 (여기까지 확인함)

| 단계 | 방법 | 결과 |
|---|---|---|
| 1. 라이브러리 복제 | `Instantiate` + `CreateAsset` | ❌ 카테고리 0개 (데이터 안 따라옴) |
| 1'. 라이브러리 복제 | **`AssetDatabase.CopyAsset`** | ✅ 카테고리 98개 온전 |
| 2. 새 라벨 추가 | `AddCategoryLabel` + `SaveAssets` | ✅ 조회 성공 |
| 3. 재임포트 생존 | `refresh` 후 재확인 | ✅ **라벨 24개 유지** (PSB 영향 없음) |
| 4. 런타임 교체 | `lib.spriteLibraryAsset = wuxia` | ✅ 이름 바뀜 |
| 5. 목록 재구축 | `hero.Init()` | ❌ **9001이 목록에 안 들어옴** |

**5번이 마지막 관문.** `Init_SpriteLibrary()`는 라벨을 이렇게 읽는다:
```csharp
foreach (var label in _spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(category))
    if (label.Split('_').Length == 4) { itemIndex = split[3]; ... }
```
`WeaponR_Weapon_Fixed_9001` → 길이 4라 조건은 통과해야 한다.
그런데 목록에 안 들어온다 → **SpriteLibrary 컴포넌트가 asset 교체를 즉시 반영하지 않는 것**으로 보인다.

### 프리팹 교체까지 해보고 얻은 결론 (2026-08-14)

프리팹의 SpriteLibrary를 복제본으로 갈아끼우는 것까지 성공했다.
캐릭터도 안 깨지고 슬롯도 정상(WeaponR=28)이었다. **그런데 새 인덱스가 안 읽혔다.**

원인: **`.spriteLib` 확장자는 Unity가 특별 취급한다.**
```
AddCategoryLabel 직후          → WeaponR_Weapon_Fixed 라벨 29개 ✅
일반 refresh 후                 → 24개 유지 (한 번은 살아남음)
Refresh(ForceUpdate) 후         → 23개로 되돌아감 ❌  추가분 소실
```
즉 강제 재임포트가 걸리면 우리가 넣은 라벨이 날아간다. 언제 ForceUpdate가
걸릴지 통제할 수 없으므로 **이 방식은 신뢰할 수 없다.**

### ⚠️ 겪은 사고 — 빈 라이브러리를 프리팹에 꽂아 캐릭터가 통째로 깨짐

첫 시도에서 `Instantiate + CreateAsset` 으로 만든 **카테고리 0개짜리 빈 라이브러리**가
디스크에 남아 있었는데, 설치기가 "이미 있네" 하고 그걸 재사용해 리그 9개에 꽂았다.
결과: `GetItemIndexList("WeaponR")` 가 KeyNotFoundException — 모든 슬롯 소실.

→ `WuxiaPartInstaller` 에 **카테고리 50개 미만이면 중단**하는 가드를 넣었고,
   사고 복구용 `무협 파츠 원복` 메뉴를 만들었다. 같은 실수를 반복하지 않는다.

### 해결 — `.asset` 확장자 (검증 완료)

`.spriteLib` 대신 일반 ScriptableObject(`.asset`)로 저장하니 특수 임포터를 타지
않아 우리가 넣은 라벨이 재임포트에도 살아남는다. 이게 현재 방식이다
(`Assets/_Project/Art/Wuxia/Wuxia_Lib.asset`).

복제할 때 `CopyAsset`으로 확장자만 바꾸면 **로드가 안 된다**. 카테고리/라벨을
하나씩 옮겨 담아 진짜 복제본을 만들어야 한다(`WuxiaPartInstaller.Install`).

### ⚠️ 배선 누락 — 파츠를 만들어도 아무도 안 읽고 있었다 (2026-08-14)

`wear-index.txt` 는 생성됐지만 런타임 코드가 그 표를 참조하지 않아,
무기를 장착해도 SP1 기본 파츠가 손에 들렸다. 아트만 만들고 끝난 상태였다.

→ `AppearanceService.WeaponItemForId(무기id, kind, rarity)` 추가.
표에 있으면 우리 파츠(9000번대), 없으면 기존 `WeaponItemFor(kind, rarity)` 로 떨어진다.
파츠를 다시 설치했으면 `ReloadWearMap()` 을 부른다.

## 새 무기 등록 절차

1. AI로 **옆모습·이펙트 없는** 무기 생성 (손잡이가 중앙 근처)
2. 크로마키 마젠타 배경 → `tools/strip_chroma.py`로 알파 생성
3. `tools/split_sheet.py`로 시트 분할 (요청 1회에 여러 개)
4. 스프라이트 임포트: PPU 100, Pivot Center, Filter Point(픽셀아트면)
5. `SpriteLibraryAsset`에 카테고리 `WeaponR_Weapon_Fixed`,
   라벨 `WeaponR_Weapon_Fixed_9001` 로 추가 (Main/Sub는 Empty)
6. `AppearanceService.WeaponItemFor()`가 새 인덱스를 반환하도록 수정
7. `ChangeSlotItem("WeaponR", "9001")` → 즉시 반영

## 생성 파이프라인 (검증 완료)

- **시트 생성이 개별 생성보다 낫다**: 요청 1회에 5종, 화풍이 한 흐름으로 이어진다
  (개별로 뽑으면 티어마다 화풍이 튄다). 60회 → 12회로 비용도 1/5.
- **투명 배경은 프롬프트로 안 된다**: `transparent background`를 요구하면
  모델이 체커보드를 *그림으로* 그린다. 부정 지시(`no checkerboard`)도 안 먹는다.
  → 긍정문으로 **크로마키 마젠타 단색**을 요구하고 후처리로 지운다.
- **분할은 격자 위치를 가정하지 않는다**: 알파 연결 성분으로 잘라내므로
  모델이 간격을 안 맞춰도 어긋나지 않는다.
- 생성기는 `EditorApplication.update` 기반 비동기 — 에디터가 멈추지 않는다.

관련: `Assets/_Project/Scripts/Editor/XaiIconGenerator.cs`,
`tools/strip_chroma.py`, `tools/split_sheet.py`, `docs/equip-icon-prompts.json`

## 착용용 스프라이트 규격 — 캐릭터가 이미 든 파츠에서 실측

규격을 상상하지 않는다. 원본 21종(`WeaponR_Weapon_Fixed_0001~0021`)을 PNG로 뽑아
쟀다. 전체 표: **`tools/sp1-wear-reference.md`**.

| 항목 | SP1 원본 (실측) | 우리 생성물(가공 전) | 조치 |
|---|---|---|---|
| 주축 | **-30°** (날이 좌상단) | 90° (세로로 섬) | **반시계 +60° 회전** |
| 크기 | 85×57 ~ 206×140 | 350~590 정사각 | 종류별 긴 변 90/105/150/205 |
| 캔버스 | 알파 타이트 박스 | — | 타이트 크롭 (정사각 패딩 없음) |
| pivot | 정중앙 = 박스 중심 | 정중앙 | 유지 |
| PPU | 100 | 임포트 기본값 | **100 고정** |
| 외곽선 | **순검정**, 긴 변의 1/48 | 없음 | 후처리로 두름 |

### ⚠️ 회전 부호를 틀려 무기를 등 뒤로 뻗고 있었다

처음엔 `-60°`로 돌렸다. 축은 ±30으로 같아 보이지만 **좌우가 반대**라
(끝이 우상단), 리그가 팔을 돌려도 무기가 적 반대쪽을 향했다.
원본은 전부 끝이 **좌상단**이다 — 주축 각도만 재고 방향을 안 본 탓이다.
`+60°`로 고치니 적을 향해 뻗는다.

외곽선도 빠져 있었다. 원본은 경계 픽셀 100%가 순검정이라, 테두리 없는
우리 것만 붕 떠 보였다.

```bash
python tools/fit_wear.py Assets/_Project/Resources/WearParts
```
인자 없이 돌리면 파일명으로 종류를 알아내 위 표를 적용한다.

검증: 검·창 모두 손에 들려 적 쪽을 향하고, 크기가 몹의 무기와 같은 급으로 보인다.
