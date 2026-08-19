#!/bin/bash
# Unity 에디터가 닫히길 기다렸다가 Android AAB 헤드리스 빌드를 돌린다.
# 검증은 빌드 리포트가 아니라 AAB를 zip으로 열어 raw 크기로 한다 (Q 시리즈 함정).
UNITY_CLI="C:/Users/ChooPC/AppData/Local/Unity/bin/unity.exe"
PROJ="H:/Game/IdleRPG/NewRPG"
echo "[watcher] Unity.exe 종료 대기 중..."
while tasklist 2>/dev/null | grep -qi "Unity.exe"; do sleep 15; done
echo "[watcher] 에디터 종료 감지 — Android AAB 헤드리스 빌드 시작"
"$UNITY_CLI" build "$PROJ" --editor-version 2022.3.62f3 --target Android \
  --execute-method IdleMvp.EditorTools.Builder.PerformAndroidBuild --non-interactive 2>&1 | tail -20
echo "[watcher] exit=$?"
cat "$PROJ/Builds/build-result.txt" 2>/dev/null || echo "no result file"
ls -la "$PROJ"/Builds/*.aab 2>/dev/null
