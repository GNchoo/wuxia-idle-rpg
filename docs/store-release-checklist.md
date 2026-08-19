# 스토어 출시 체크리스트

## 완료 (이 리포지토리에서 끝난 것)

- [x] 플레이어 빌드 브레이커 제거 (레거시 `using UnityEditor` 12파일)
- [x] UI 전면개편: TMP(UIHangulSDF) + FantasyIdleGameGUI 킷, 전 화면 검증
- [x] 오디오: BGM + UI/전투/보상 SFX, 설정 토글
- [x] 콘텐츠: 챕터 10 · 스테이지 100 · 레벨캡 60
- [x] 기능: 아레나/월드보스/핫딜 개방·검증, 채팅만 게이트
- [x] 앱 아이콘 512px (`Assets/_Project/Art/AppIcon.png`) + PlayerSettings 적용
- [x] 앱 ID `com.idlestudio.idlerpg` · 버전 1.0.0 (versionCode 1)
- [x] 헤드리스 빌드 진입점 `IdleMvp.EditorTools.Builder` (Win/Android)

## 남은 작업 — 기계 (이 PC에서)

- [x] Windows 스모크 빌드 통과 (`Builds/Win/IdleRPG.exe`, 에러 0)
- [x] Android Build Support 모듈 설치 (Hub, SDK 34/35/36 + NDK + OpenJDK)
- [x] Android .aab 빌드 성공 — `Builds/Android/IdleRPG.aab` 65MB (IL2CPP, ARM64+ARMv7, minSdk 24, 디버그 서명)
- [ ] **업로드 키스토어 생성** (Play 업로드 전 유일한 잔여 기계 작업 — 비밀번호 직접 입력 필요):
  ```bash
  "C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool.exe" -genkeypair -v -keystore H:/Game/IdleRPG/upload.keystore -alias idlerpg -keyalg RSA -keysize 2048 -validity 10000
  ```
  이후 Unity PlayerSettings > Publishing Settings에 키스토어/비밀번호 입력 후 재빌드

## 남은 작업 — 스토어 콘솔 (계정 필요, 사용자 직접)

- [ ] Google Play Console 개발자 계정 ($25) / App Store Connect ($99/년)
- [ ] 앱 등록: 이름 방치형RPG, 패키지 `com.idlestudio.idlerpg`
- [ ] 스토어 등록정보: 스크린샷(스크린샷 루프 재활용 가능), 그래픽 이미지, 설명
- [ ] 개인정보처리방침 URL (Ads/Analytics 사용 시 필수)
- [ ] 콘텐츠 등급 설문
- [ ] IAP 상품 등록 후 `BmRuntimeFlags.UseRealIapAds` 활성 + `IapProductCatalog` 상품 ID 매칭
- [ ] LevelPlay/Unity Ads 앱 키 발급 (광고 사용 시)

## 유료 에셋 (선택 — 최종 품질 업그레이드 시 요청 예정)

현재 무료/보유 에셋으로 출시 가능 품질. 더 올리려면:
1. **캐릭터/몬스터 스프라이트 팩** — 현 무료팩 대체 (통일된 아트 스타일)
2. **BGM 팩** — 현재 킷 데모 트랙 1곡 → 챕터별 BGM
3. iOS 출시 시: **Mac 빌드 머신** 필요 (Xcode)

## 알려진 한계

- 채팅: 온라인 백엔드 없음 (게이트 유지)
- 세이브: 로컬 JSON (클라우드 세이브 미지원 — UGS Cloud Save 후보)
- BigDouble 미적용: 수치 상한 ~1e15 (스테이지 100 보스 HP 65만 — 여유 충분)
