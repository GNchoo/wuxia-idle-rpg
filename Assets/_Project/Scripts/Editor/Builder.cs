#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>Headless build entry points (unity CLI --execute-method).</summary>
    public static class Builder
    {
        static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/Meta.unity",
            "Assets/_Project/Scenes/Battle.unity"
        };

        public static void PerformWinBuild()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Win/IdleRPG.exe");
        }

        public static void PerformAndroidBuild()
        {
            EditorUserBuildSettings.buildAppBundle = true;
            // 텍스처 압축. Generic(ETC)으로 뒀더니 알파 있는 스프라이트가 전부
            // RGBA32(무압축)로 떨어져 빌드가 1GB가 됐다 (텍스처 430MB = 총픽셀×4byte).
            // minSdk 24(GLES3) 이므로 ASTC 를 쓴다 — 임의 크기 텍스처도 압축된다.
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            Build(BuildTarget.Android, "Builds/Android/IdleRPG.aab");
        }

        static void Build(BuildTarget target, string path)
        {
            // 플레이 모드 중 빌드는 Addressables 가 거부한다. 그런데 BuildPipeline 은
            // 그대로 진행해 'Succeeded'라고 보고한다 — Addressables 콘텐츠가 빠진
            // 반쪽짜리 빌드가 성공으로 남는다(실제로 겪음). 여기서 막는다.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Builder] 플레이 모드 중에는 빌드할 수 없습니다. Play를 끄고 다시 실행하세요.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            var opts = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = path,
                target = target,
                options = BuildOptions.None
            };
            // ⚠️ subtarget 을 안 넣으면 기본값 0(=Generic)이 EditorUserBuildSettings 를
            // **덮어쓴다**. 에디터 캐시가 ASTC여도 빌드가 Generic으로 전량 재임포트해
            // 텍스처 424MB 무압축 빌드가 나왔고, 빌드가 끝나면 에디터 설정까지
            // Generic으로 되돌아가 있었다(두 번 겪고 AAB를 열어보고서야 잡았다).
            if (target == BuildTarget.Android)
                opts.subtarget = (int)MobileTextureSubtarget.ASTC;
            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;
            Debug.Log($"[Builder] {target} result={s.result} errors={s.totalErrors} size={s.totalSize / 1048576}MB");
            System.IO.Directory.CreateDirectory("Builds");
            System.IO.File.WriteAllText("Builds/build-result.txt",
                $"target={target} result={s.result} errors={s.totalErrors} sizeMB={s.totalSize / 1048576}");
            // 배치(CI)에서만 종료 코드를 남긴다. 열려 있는 에디터를 끄면 작업이 날아간다.
            if (s.result != BuildResult.Succeeded && Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
#endif
