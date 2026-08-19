#!/bin/bash
# Waits for the Unity editor to close, then runs a headless Windows build.
UNITY_CLI="C:/Users/ChooPC/AppData/Local/Unity/bin/unity.exe"
PROJ="H:/Game/IdleRPG/NewRPG"
echo "[watcher] waiting for Unity.exe to exit..."
while tasklist 2>/dev/null | grep -qi "Unity.exe"; do sleep 15; done
echo "[watcher] Unity closed. starting headless build..."
"$UNITY_CLI" build "$PROJ" --editor-version 2022.3.62f3 --target StandaloneWindows64 \
  --execute-method IdleMvp.EditorTools.Builder.PerformWinBuild --non-interactive 2>&1 | tail -20
echo "[watcher] exit=$?"
cat "$PROJ/Builds/build-result.txt" 2>/dev/null || echo "no result file"
