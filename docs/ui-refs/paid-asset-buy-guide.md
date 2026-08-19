# 유료 에셋 구매·적용 가이드 (키우기형 방향)

목표 톤: **메이플키우기식** — 귀여운 SD 캐릭, 가로 필드, 반투명 HUD + 밝은 모달.  
이미 산 `IdleRPG_Assets`(IDLE RPG CLICKER)는 **시스템 재배선하지 말고** VFX/아이콘 창고로만 사용.

---

## 구매 우선순위

| 순위 | 용도 | 에셋 | 가격 | 링크 |
|---|---|---|---|---|
| **1** | Idle GUI (최우선) | Mobile Fantasy Idle UI Kit | **$19** | https://assetstore.unity.com/packages/2d/gui/mobile-fantasy-idle-ui-kit-324788 |
| **2** | 영웅 SD | 2D Character Maker : RPG Bundle | 스토어 확인 | https://assetstore.unity.com/packages/2d/characters/2d-character-maker-rpg-bundle-psb-324068 |
| **3** | 귀여운 SD 몹 (추천) | **2D Monster - Cute & Chibi: Forest** | **~€13.80** | https://assetstore.unity.com/packages/2d/characters/2d-monster-cute-chibi-forest-unique-skill-animated-prefab-with-s-296965 |
| 3-대체 | 슬라임만 저가 | 2D Slimes & Animations | **$6.99** | https://assetstore.unity.com/packages/2d/characters/2d-slimes-animations-341450 |
| 3-대체 | 버섯 몹 | 2D Mushrooms & Animations | 스토어 확인 | https://assetstore.unity.com/packages/2d/characters/2d-mushrooms-animations-353652 |
| 보류 | 풀 UI | Casual RPG&MMO UI Pack | $49.99 | https://assetstore.unity.com/packages/2d/gui/casual-rpg-mmo-ui-pack-217464 |
| 비추 | 오브젝트/음식 마스코트 몹 | Cute Object Monsters… | ~€15 | 톤이 장난감·푸드류라 메이플 필드 몹과 안 맞음 |
| 비추 | 다크 손그림 몹 | 160 Hand Drawn Monsters | ~$10 | CLICKER와 동일 톤 — 메이플 SD와 거리 있음 |

### 3번 몹을 Forest로 고른 이유

- 슬라임 / 킹슬라임 / 고블린 / 페어리 — **메이플 필드 몹에 가까운 치비 SD**
- Idle / Walk / Attack / Die + 사이드·탑다운 겸용
- 같은 시리즈 Sea / Desert로 스테이지별 몹 확장 가능 (각 ~€13.80)

무료로 먼저: [Free Casual GUI](https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804)  
→ Import 후 `Assets/_Project/Tools/Map-FreeCasualGui.ps1` 실행.

---

## 공통 구매·Import 절차

1. 브라우저 Asset Store에서 **Unity Hub와 같은 계정**으로 로그인  
2. 패키지 페이지 → **Add to My Assets** (유료면 결제)  
3. Unity 에디터 (이 프로젝트: **2022.3.x**)  
   - **Window → Package Manager → My Assets**  
   - Download → **Import**  
4. Import된 PNG만 아래 슬롯으로 **이름 맞춰 복사**  
5. Play로 HUD/필드 확인

---

## 슬롯 매핑 (`Resources/GrowArt/`)

코드 수정 없이 [`GrowArt.cs`](../../Assets/_Project/Scripts/UI/GrowArt.cs)가 로드합니다.

| GrowArt 파일 | 넣는 내용 |
|---|---|
| `PanelFrame.png` / `ModalFrame.png` | 패널·모달 9-slice |
| `CtaButton.png` / `UpgradeButton.png` | 주황 CTA |
| `BarEmpty.png` / `BarFill.png` | 게이지 |
| `InvSlot.png` / `ShopCard.png` | 슬롯·카드 |
| `Chars/Hero.png` | 영웅 idle |
| `Chars/Enemy1.png` … `Enemy6.png` | 일반 몹 |
| `Chars/EnemyMiniBoss.png` / `EnemyBoss.png` | 미니보스·보스 |
| `SkillIcon1.png` … `5.png` | (이미 IdleRPG_Assets에서 추출됨) |
| `Fx/Hit1.png` 등 | (이미 Spells에서 추출됨) |

---

## 체크리스트

- [x] Mobile Fantasy Idle UI Kit Import → `Map-PaidIdleAssets.ps1`로 GrowArt UI 슬롯 덮어쓰기  
- [x] Character Maker: Unity 메뉴 **IdleMvp > Apply Paid Character Maker Hero** (Warrior → `Chars/Hero.png`, Orc → Boss)  
- [ ] Cute & Chibi Forest (또는 Slimes) idle 프레임 → `Chars/Enemy1..6.png` / MiniBoss·Boss  

- [ ] Unity에서 PNG **Sprite (2D and UI)** / 패널은 **9-slice Border**  
- [ ] Play: 상단 칩·모달·필드 몹·타격 FX 확인  

### 적용 명령

```powershell
powershell -File Assets/_Project/Tools/Map-PaidIdleAssets.ps1
```

Unity: **IdleMvp → Apply Paid Character Maker Hero** (패키지 `2d.animation` / `2d.psdimporter` 설치 후)  
