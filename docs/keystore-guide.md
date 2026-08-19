# 업로드 키스토어 가이드 (사용자 직접 작업)

현재 AAB는 **디버그 키 서명**이라 Play Console에 업로드할 수 없다.
키스토어 생성과 비밀번호 입력은 **직접** 해야 한다 — 비밀번호는 AI 세션이든
스크립트든 어디에도 남기지 않는 것이 원칙이다.

## 1. 키스토어 생성 (Unity 안에서, 5분)

1. Unity 메뉴 → **Edit > Project Settings > Player > Android 탭 > Publishing Settings**
2. **Keystore Manager** 버튼 → **Keystore... > Create New > Anywhere**
3. 저장 위치: **프로젝트 폴더 밖** (예: `C:\Keys\idlerpg-upload.keystore`)
   — 프로젝트 안에 두면 git에 딸려 들어갈 위험이 있다. `.gitignore`에 `*.keystore`가 있어도 밖이 안전.
4. 비밀번호 입력 (키스토어/키 각각). **비밀번호를 잃으면 앱 업데이트가 영구 불가** —
   비밀번호 관리자에 저장할 것.
5. Alias: `idlerpg` 등 간단한 이름.

## 2. 빌드에 적용

Keystore Manager에서 생성하면 Publishing Settings에 자동 기재된다.
비밀번호는 세션마다 Unity가 물어본다(디스크에 저장 안 함) — 헤드리스 빌드가 필요하면
환경변수 방식(`UNITY_KEYSTORE_PASS` 등)을 쓰되, 그 값 관리도 직접.

## 3. 확인

빌드 후 서명 확인:
```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool" -printcert -jarfile Builds/Android/IdleRPG.aab
```
`CN=`이 디버그(`CN=Android Debug`)가 아니면 성공.

## 4. Play Console 등록 (계정 필요)

- 최초 업로드 시 **Play App Signing** 등록 (구글이 서명키 보관, 우리 키는 업로드키)
- 이후 절차는 `store-release-checklist.md` 참조
