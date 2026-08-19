using System.Collections;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using UnityEngine;

namespace IdleMvp.Boot
{
    /// <summary>
    /// Template combat fires from Animation Events on the main-hero Animator.
    /// We keep a static fallback sprite (Animator off) for visibility, so this
    /// bridge calls AttackActivate on a timer to keep idle combat running.
    /// </summary>
    public class CombatAutoAttackBridge : MonoBehaviour
    {
        [SerializeField] float minInterval = 0.45f;
        [SerializeField] float maxInterval = 2.2f;
        [SerializeField] float baseCycleSeconds = 2.4f;

        IEnumerator Start()
        {
            // Wait until gameplay managers exist.
            for (int i = 0; i < 120; i++)
            {
                if (Object.FindObjectOfType<GameplayMainHeroManagerScript>(true) != null)
                    break;
                yield return null;
            }

            while (enabled)
            {
                var hero = Object.FindObjectOfType<GameplayMainHeroManagerScript>(true);
                if (hero == null)
                {
                    yield return new WaitForSecondsRealtime(0.75f);
                    continue;
                }

                float speed = hero.AttackSpeedValue > 0.05f ? hero.AttackSpeedValue : 1f;
                float interval = Mathf.Clamp(baseCycleSeconds / speed, minInterval, maxInterval);

                try
                {
                    hero.AttackActivate();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[IdleMvp] AttackActivate failed: " + e.Message);
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }
}
