using System.Collections;
using SAMPLETEXT;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Soft tutorial skip only. Does NOT touch cameras (that caused No cameras rendering).
    /// </summary>
    public class TemplateGameplayBootstrap : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(0.5f);

            ForceTutorialCompletePrefs();
            SoftSkipTutorial();

            yield return new WaitForSecondsRealtime(0.2f);
            TryOpenGameplayOnce();

            RestoreBattleCameraViewport();
            EnsureMainHeroVisible();
            yield return new WaitForSecondsRealtime(0.5f);
            RestoreBattleCameraViewport();
            EnsureMainHeroVisible();
            yield return new WaitForSecondsRealtime(1.5f);
            RestoreBattleCameraViewport();
            EnsureMainHeroVisible();

            Debug.Log("[IdleMvp] Soft tutorial skip done (WorldCamera RT viewport restored).");
        }

        /// <summary>
        /// Letterbox must not shrink WorldCamera when it renders to WorldRender RT.
        /// </summary>
        static void RestoreBattleCameraViewport()
        {
            var cams = Object.FindObjectsOfType<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam == null || cam.targetTexture == null)
                    continue;
                cam.rect = new Rect(0f, 0f, 1f, 1f);
            }

            var world = GameObject.Find("WorldCamera");
            if (world != null)
            {
                var cam = world.GetComponent<Camera>();
                if (cam != null)
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
            }
        }

        /// <summary>
        /// Keep a readable hero sprite. Large Multiple sheets are unreliable on Standalone,
        /// so we pin HeroIdleFrame and leave Animator off — combat is driven by CombatAutoAttackBridge.
        /// </summary>
        static void EnsureMainHeroVisible()
        {
            var heroRoot = GameObject.Find("BattleSceneMainHeroView");
            if (heroRoot == null)
            {
                Debug.LogWarning("[IdleMvp] BattleSceneMainHeroView not found.");
                return;
            }

            heroRoot.SetActive(true);

            var sr = heroRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (sr == null)
            {
                Debug.LogWarning("[IdleMvp] Main hero SpriteRenderer missing.");
                return;
            }

            var anim = sr.GetComponent<Animator>();
            if (anim != null)
                anim.enabled = false;

            if (!ApplyFallbackHeroSprite(sr))
            {
                Debug.LogError("[IdleMvp] Could not apply hero fallback sprite.");
                return;
            }

            var t = heroRoot.transform;
            t.localScale = new Vector3(0.9f, 0.9f, 1f);
            sr.enabled = true;
            sr.color = Color.white;
            sr.sortingOrder = 10;

            var b = sr.bounds;
            Debug.Log(
                $"[IdleMvp] Hero forced visible: sprite={sr.sprite?.name} " +
                $"bounds={b.size.x:0.##}x{b.size.y:0.##} pos={t.position}");
        }

        static bool ApplyFallbackHeroSprite(SpriteRenderer sr)
        {
            var fallback = Resources.Load<Sprite>("MvpUi/HeroIdleFrame");
            if (fallback == null)
            {
                var tex = Resources.Load<Texture2D>("MvpUi/HeroIdleFrame");
                if (tex != null)
                {
                    fallback = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.1f),
                        70f,
                        0,
                        SpriteMeshType.FullRect);
                    fallback.name = "HeroIdleFrame_runtime";
                }
            }

            if (fallback == null)
            {
                Debug.LogWarning(
                    "[IdleMvp] Resources/MvpUi/HeroIdleFrame missing. " +
                    "Project search: HeroIdleFrame → Texture Type = Sprite (2D and UI) → Apply.");
                return false;
            }

            sr.sprite = fallback;
            sr.flipX = false;
            sr.flipY = false;
            Debug.Log($"[IdleMvp] Hero sprite set to {fallback.name} ({fallback.rect.width}x{fallback.rect.height})");
            return true;
        }

        static void ForceTutorialCompletePrefs()
        {
            PlayerPrefs.SetInt("MissionStarted", 1);
            for (int i = 0; i <= 18; i++)
                PlayerPrefs.SetInt("ActiveTutPanel" + i, 1);
            PlayerPrefs.SetInt("GaveGemReward1", 1);
            PlayerPrefs.SetInt("GaveGemReward2", 1);
            PlayerPrefs.Save();
        }

        static void SoftSkipTutorial()
        {
            var tut = Object.FindObjectOfType<Tutorial_Manager>(true);
            if (tut == null) return;

            if (tut.activeTutPanelSave != null)
            {
                for (int i = 0; i < tut.activeTutPanelSave.Length; i++)
                    tut.activeTutPanelSave[i] = true;
            }

            try
            {
                tut.SkipTutorial();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[IdleMvp] SkipTutorial: " + e.Message);
            }

            if (tut.tutorialPanel != null)
                tut.tutorialPanel.SetActive(false);

            if (tut.activeTutPanel != null)
            {
                foreach (var p in tut.activeTutPanel)
                {
                    if (p != null) p.SetActive(false);
                }
            }

            if (tut.VillageButton != null) tut.VillageButton.SetActive(true);
            if (tut.TeamButton != null) tut.TeamButton.SetActive(true);
            if (tut.GameModesButton != null) tut.GameModesButton.SetActive(true);
            if (tut.shopButton != null) tut.shopButton.SetActive(true);
            if (tut.GameplayButton != null) tut.GameplayButton.SetActive(true);

            if (tut.disableGroup1 != null)
            {
                foreach (var go in tut.disableGroup1)
                {
                    if (go != null) go.SetActive(true);
                }
            }
        }

        static void TryOpenGameplayOnce()
        {
            var tut = Object.FindObjectOfType<Tutorial_Manager>(true);
            if (tut == null || tut.GameplayButton == null) return;

            tut.GameplayButton.SetActive(true);
            var btn = tut.GameplayButton.GetComponent<Button>()
                      ?? tut.GameplayButton.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.Invoke();
            }
        }
    }
}
