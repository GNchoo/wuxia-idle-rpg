using IdleMvp.Progression;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using UnityEngine;

namespace IdleMvp.Adapters
{
    /// <summary>
    /// Read-only bridge from template wave counters into StageProgress.
    /// </summary>
    public class TemplateWaveAdapter : MonoBehaviour
    {
        GameplayEnemyManagerScript _enemy;
        float _retry;

        void Update()
        {
            if (_enemy == null)
            {
                _retry -= Time.unscaledDeltaTime;
                if (_retry > 0) return;
                _retry = 1f;
                _enemy = FindObjectOfType<GameplayEnemyManagerScript>();
                if (_enemy == null) return;
            }

            if (StageProgress.Instance == null) return;
            StageProgress.Instance.SetFromTemplateWaves(_enemy.WaveMinCountValue, _enemy.WaveMaxCountValue);
        }
    }
}
