#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// Reimports Character Maker PSB and bakes preset idle frames into GrowArt sprites.
    /// </summary>
    public static class ApplyPaidAssets
    {
        const string PsbPath = "Assets/SP1/2D Character Maker/CharacterAssets/RPG Bundle/Character/Character_RPG.psb";
        const string WarriorPrefab = "Assets/SP1/2D Character Maker/CharacterAssets/RPG Bundle/Preset/Warrior.prefab";
        const string OrcPrefab = "Assets/SP1/2D Character Maker/CharacterAssets/RPG Bundle/Preset/Orc Brute.prefab";
        const string HeroOut = "Assets/_Project/Resources/GrowArt/Chars/Hero.png";
        const string BossOut = "Assets/_Project/Resources/GrowArt/Chars/EnemyBoss.png";
        const string MiniBossOut = "Assets/_Project/Resources/GrowArt/Chars/EnemyMiniBoss.png";

        [MenuItem("IdleMvp/Apply Paid Character Maker Hero")]
        public static void ApplyFromMenu()
        {
            if (!ApplyAll())
                EditorUtility.DisplayDialog("IdleMvp", "Paid asset bake failed. Check Console.", "OK");
            else
                EditorUtility.DisplayDialog("IdleMvp", "Hero / Orc frames baked into GrowArt/Chars.", "OK");
        }

        public static void ApplyFromCli()
        {
            ApplyAll();
            EditorApplication.Exit(0);
        }

        static bool ApplyAll()
        {
            if (!File.Exists(ToAbsolute(PsbPath)))
            {
                Debug.LogWarning("[IdleMvp] Character_RPG.psb missing — skip Character Maker bake.");
                return false;
            }

            AssetDatabase.ImportAsset(PsbPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            bool okHero = BakePrefabToPng(WarriorPrefab, HeroOut, 512);
            bool okBoss = BakePrefabToPng(OrcPrefab, BossOut, 512);
            if (okBoss)
                File.Copy(ToAbsolute(BossOut), ToAbsolute(MiniBossOut), true);

            AssetDatabase.ImportAsset(HeroOut, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(BossOut, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(MiniBossOut, ImportAssetOptions.ForceUpdate);
            SetSpriteImporter(HeroOut);
            SetSpriteImporter(BossOut);
            SetSpriteImporter(MiniBossOut);
            AssetDatabase.Refresh();

            Debug.Log($"[IdleMvp] Paid bake done. Hero={okHero} Boss={okBoss}");
            return okHero;
        }

        static bool BakePrefabToPng(string prefabPath, string outPath, int size)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("[IdleMvp] Prefab not found: " + prefabPath);
                return false;
            }

            var prevSetup = EditorSceneManager.GetSceneManagerSetup();
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            GameObject instance = null;
            GameObject camGo = null;
            try
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = Vector3.zero;

                // Force SpriteSkin / Animator update so mesh is posed.
                foreach (var anim in instance.GetComponentsInChildren<Animator>(true))
                {
                    anim.Update(0f);
                    var clips = anim.runtimeAnimatorController != null
                        ? anim.runtimeAnimatorController.animationClips
                        : null;
                    if (clips != null)
                    {
                        for (int i = 0; i < clips.Length; i++)
                        {
                            if (clips[i] != null && clips[i].name.ToLowerInvariant().Contains("idle"))
                            {
                                anim.Play(clips[i].name, 0, 0f);
                                anim.Update(0f);
                                break;
                            }
                        }
                    }
                }

                Bounds b = CalculateBounds(instance);
                camGo = new GameObject("BakeCam");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                cam.allowHDR = false;
                cam.allowMSAA = false;
                float half = Mathf.Max(b.extents.x, b.extents.y) * 1.15f;
                if (half < 0.5f) half = 1.2f;
                cam.orthographicSize = half;
                Vector3 center = b.center;
                cam.transform.position = new Vector3(center.x, center.y, center.z - 10f);
                cam.transform.rotation = Quaternion.identity;
                cam.cullingMask = ~0;

                var rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                cam.targetTexture = null;
                Object.DestroyImmediate(rt);

                string abs = ToAbsolute(outPath);
                Directory.CreateDirectory(Path.GetDirectoryName(abs));
                File.WriteAllBytes(abs, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                Debug.Log("[IdleMvp] Wrote " + outPath);
                return true;
            }
            finally
            {
                if (instance != null) Object.DestroyImmediate(instance);
                if (camGo != null) Object.DestroyImmediate(camGo);
                if (prevSetup != null && prevSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(prevSetup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }
        }

        static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return new Bounds(root.transform.position + Vector3.up, Vector3.one * 2f);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        static void SetSpriteImporter(string assetPath)
        {
            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }

        static string ToAbsolute(string assetPath)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(project, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
#endif
