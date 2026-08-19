# 왜 자꾸 "적용했는데 되돌아가는가" — 원인과 규칙

## 한 줄 요약

**구매 에셋(SP1)의 임포트 파이프라인 바깥에서 산출물만 건드렸기 때문이다.**
Unity가 다시 임포트하면 산출물은 원본에서 다시 만들어지므로, 우리가 손댄 것은 사라진다.
정식 경로는 **임포터에 쓰는 것**이고, 그러면 `.meta` 에 저장되어 살아남는다.

## 겪은 사고 4건과 각각의 진짜 원인

| # | 증상 | 원인 | 지금 방식 |
|---|---|---|---|
| 1 | 라이브러리에 추가한 라벨이 강제 재임포트 때 사라짐 (29→23) | `.spriteLib` 는 PSB 산출물이라 재생성된다 | 일반 `.asset` 으로 **복제본**을 만들어 거기에 추가 |
| 2 | 빈 라이브러리를 꽂아 캐릭터가 통째로 깨짐 | `Instantiate`+`CreateAsset` 은 내용이 안 따라온다 | 카테고리·라벨을 하나씩 옮겨 담고, **50개 미만이면 중단** |
| 3 | 무기는 손에 들리는데 방어구는 기본 자세 고정 | 방어구는 본 가중치로 움직이는데 우리 PNG엔 없음 | 원본에서 본을 **에셋에 굽는다** (아래) |
| 4 | 본을 심어도 다음 실행에 원래대로 | `Sprite.SetBones` 등 런타임 API는 **에셋에 저장되지 않는다** | 임포터 API로 전환 |
| 5 | 투구가 머리가 아니라 목 높이에 그려짐 | 읽을 때(`Sprite.GetBones`)는 **피벗 기준 유닛**, 쓸 때(`ISpriteBoneDataProvider`)는 **rect 좌하단 기준 픽셀**인데 그대로 넘겼다 | 넘기기 전에 `pivot*크기 + 위치*PPU` 로 변환 |
| 6 | "소매·바지는 원본이 없어 못 만든다"고 잘못 판단 | `_Fixed` 카테고리만 봤다. 원본은 `_Main` 에 있었다 | 부위별 카테고리를 표로 고정 (`WuxiaBoneBaker.Parts`) |
| 7 | 조끼가 몸통보다 길쭉하고, 바지 다리가 다리의 절반 폭 | '긴 변'만 맞췄다. AI는 사람 등신으로 그리는데 SP1은 등신이 낮다 | 부위별 **박스**로 맞추고 1.35배까지 눌러 채운다 (`KIND_BOX`) |
| 8 | 낮은 모자는 머리에 파묻히고 높은 모자는 뜬다 | 본 위치를 중앙값 하나로 고정했다 | 투구·몸통은 **윗변**을 맞춘다 (`TopAligned`) |
| 9 | 검이 손을 벗어나 몸을 가로질러 매달림 | "무기는 본이 없다"고 잘못 알았다. 원본 51종 전부 `bone_weapon_R` 가중치를 갖는다 | 무기도 굽는다. 뼈는 **손이 쥐는 지점** = 캔버스 반지름의 일정 비율 |
| 10 | 오른손 무기 기준으로 **왼손 뼈**가 뽑힘 | 기준값을 잴 때 우리 파츠를 안 걸렀다. 런타임에 심어 둔 본이 에디터 메모리에 남아 있었다 | 기준값 계산에서 인덱스 9000 이상은 전부 제외 |

## 정식 경로 — 임포터에 쓴다

Unity가 지원하는 방법은 Sprite Editor 의 **Skinning 모듈**이고, 그 모듈이 쓰는 API가
`ISpriteBoneDataProvider` / `ISpriteMeshDataProvider` 다. 여기에 쓰면 `.meta` 에 저장된다.

```csharp
var factories = new SpriteDataProviderFactories();
factories.Init();
var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
provider.InitSpriteEditorDataProvider();

provider.GetDataProvider<ISpriteBoneDataProvider>().SetBones(guid, bones);
provider.GetDataProvider<ISpriteMeshDataProvider>().SetVertices(guid, verts);  // verts 에 boneWeight

provider.Apply();
importer.SaveAndReimport();
```

구현: `Assets/_Project/Scripts/Editor/WuxiaBoneBaker.cs`
메뉴: `IdleMvp/아트/무협 방어구 본 굽기 (에셋에 저장)`

**검증**: `ImportAsset(ForceUpdate | ForceSynchronousImport)` 후에도
본 1 · 가중치 있음 · bindPoses 1 이 유지되는 것을 확인했다.

## 그래도 정석과 다른 점 (알고 쓰는 우회)

| 정석 | 우리 | 왜 |
|---|---|---|
| 캐릭터 `.psb` 에 레이어를 추가하고 재임포트 | 별도 PNG + 라이브러리 복제본 | PSB는 구매 에셋이다. 고치면 에셋 업데이트 때 날아가고, 되돌리기도 어렵다 |
| Sprite Editor 의 Skinning 창에서 손으로 웨이팅 | 원본 본을 스크립트로 복사 | 부위당 뼈가 1개뿐이라 변형이 없다. 사각형 네 꼭짓점에 가중치 1이면 원본과 동일하게 동작 |
| 새 파츠를 PSB 안에서 정렬 | 화면에서 재서 크기 보정 | 참고: `tools/sp1-armor-reference.md` 측정 절차 |

이 우회는 **의도한 것**이고 문서로 남긴다.

### 좌우가 갈리는 부위는 파일이 두 장이어야 한다

`SpriteSkin` 은 스프라이트에 적힌 **뼈 이름**으로 리그의 트랜스폼을 찾는다. 그런데
어깨·손·발·소매·바지다리는 뼈가 `bone_arm_R` / `bone_arm_L` 처럼 좌우 이름이 다르다.
→ 같은 스프라이트를 양쪽 카테고리에 넣으면 한쪽은 뼈를 못 찾는다.
설치 도구가 `이름@L.png` 를 복사해 각각 제 뼈로 굽는다. 그림은 한 장만 그리면 된다.

### 한 벌은 같은 인덱스를 써야 한다

`ChangeSlotItem(슬롯, 인덱스)` 는 그 슬롯의 **모든 카테고리**를 같은 인덱스로 맞추고,
없는 카테고리는 `Empty` 로 만든다. 바지처럼 허리·왼다리·오른다리가 따로인 부위는
세 조각이 같은 인덱스를 받아야 하고, 아니면 나머지가 조용히 사라진다.

## 착수 전 체크리스트 (사고 10건의 공통 원인)

사고 기록은 사후 정리라 같은 실수를 막지 못했다. 위 10건 중 **8건이 같은 모양**이다 —
표본 하나를 보고 전체를 단정했다. 구매 에셋에서 값을 베끼는 도구를 쓰기 전에 이걸 먼저 한다.

1. **전수를 출력한다.** 참고할 카테고리의 **모든** 라벨을 찍고 `n / 최소 / 최대 / 중앙값 /
   뼈 이름`을 본다. 하나만 보고 규칙을 정하지 않는다.
   → "Pants 원본 0개"(실제로는 `_Main` 에 있었다), "무기는 본이 없다"(원본 51종 전부 있다)
2. **"없다"고 결론 낼 땐 어디를 봤는지 적는다.** `_Fixed` 만 보고 "없다"고 했다가 두 번
   틀렸다. 부정 결론은 탐색 범위를 명시해야 검증된다.
3. **도구 출력이 다시 도구 입력이 되는지 본다.** 우리 파츠도 같은 카테고리에 들어가므로,
   기준값 계산에서 빼지 않으면 돌릴 때마다 조금씩 밀린다(실제로 밀렸다).
4. **문서에 없는 API는 1개만 처리해 왕복 검증한다.** 굽고 → 다시 읽고 → 원본과 같은지
   비교. 이걸 처음부터 했으면 좌표계 사고는 5분에 끝났다.
   (`SpriteBone.position` 의 좌표계는 Unity 공식 문서에 **없다** — 실측만이 답이다)
5. **눈으로 판정하지 않는다.** 화면에서 재고 원본과 숫자로 비교한다.

참고: SP1 에셋의 README 는 39줄짜리 데모 씬 사용법뿐이고 리그 규격이 없다.
`unity_docs` 로 `ISpriteBoneDataProvider`/`SpriteBone` 을 찾아도 0건이다.
**규격은 문서가 아니라 원본 파츠를 재서 알아내야 한다** — 그래서 위 1번이 제일 중요하다.

## 앞으로 지킬 규칙

1. **에셋에 남아야 하는 것은 임포터에 쓴다.** 런타임 `Set*` 은 그 실행에만 유효하다.
2. **구매 에셋 원본(`.psb`, `.spriteLib`)은 건드리지 않는다.** 복제본에만 쓴다.
3. **재임포트로 검증한다.** `ForceUpdate | ForceSynchronousImport` 후에도 남아 있어야 통과.
4. **눈으로 판정하지 않는다.** 크기·위치는 정지 화면 diff 로 실측한다
   (`tools/sp1-armor-reference.md`).
5. 캐시를 잡는 컴포넌트(`SpriteSkin`)는 데이터를 바꾼 뒤 **다시 묶어 줘야** 한다.

---

## 전수 점검 (2026-08-14)

지금까지 만든 것 전부에 위 규칙이 적용돼 있는지 확인했다. 근거는 실제 측정값이다.

| 항목 | 결과 | 근거 |
|---|---|---|
| SP1 원본 (`.psb`/`.spriteLib`/프리셋) | ✅ | 임포트 커밋 이후 변경 **0건** (`git diff 8b778486 HEAD -- Assets/SP1` 비어 있음) |
| Layer Lab · IdleRPG_Assets · FREE_2_Monsters | ✅ | 임포트 커밋만 존재 |
| `Wuxia_Lib.asset` | ✅ | 강제 재임포트 후 WeaponR 53→53 (우리것 30→30), Hat 우리것 6→6 |
| CharPresets 프리팹의 라이브러리 참조 | ✅ | 9개 중 9개가 무협 라이브러리 사용 |
| WearParts 임포트 설정 | ✅ | 30/30 이 PPU100 · 중앙피벗 · Sprite |
| ArmorParts 본 굽기 | ✅ | 6/6, 강제 재임포트 후 본1·가중치·bindPose 유지 |
| EquipIcons | ✅ | 60/60 Sprite 타입 |
| 런타임 스크립트의 에셋 조작 | ✅ | `AssetDatabase`/`SetDirty` 사용이 `Scripts/Editor/` 안에만 존재 |
| CasualPanel 런타임 수술 | ✅ (의도) | 프리팹 **인스턴스**만 손댄다. 열 때마다 다시 적용되므로 영속성이 필요 없다 |

### 걸린 것 2건

**1) `SyncCharPresets` 가 스크립트 리로드마다 자동 실행된다**
SP1 프리셋을 `Resources/CharPresets` 로 복사한다. `이미 있으면 건너뛰기`라 덮어쓰지는
않지만, 프리팹이 지워지면 **SP1 원본이 되살아나 무협 파츠가 조용히 사라진다.**
→ 동기화 후 프리셋이 무협 라이브러리를 안 쓰면 경고하도록 고쳤다. 조용히 사라지는 게
제일 나쁘다.

**2) `UIHangulSDF` 가 Dynamic 아틀라스다** (글리프 551)
실행 중 글리프가 추가되며 에셋이 더러워져 깃에 계속 잡힌다. 롤백 위험은 아니지만
빌드마다 아틀라스가 달라질 수 있다. 한글 글리프를 다 구운 뒤 Static 으로 굳히는 걸
검토할 것 — 다만 지금은 사용자가 넣는 문자를 다 예측할 수 없어 Dynamic 이 안전하다.

### 아직 규칙 밖에 있는 것

`Pants` · `ArmorArm` 은 원본 파츠가 0개라 본을 베낄 대상이 없다. 이 두 부위만은
Skinning 창에서 손으로 웨이팅하거나 뼈를 직접 정의해야 하며, 그전까지는
긴팔·긴바지를 만들 수 없다.

---

관련: `tools/wear-spec.md`(제작 규격), `tools/sp1-wear-reference.md`,
`tools/sp1-armor-reference.md`, `docs/wuxia-art-pipeline.md`
