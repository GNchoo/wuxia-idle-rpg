using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// Legacy 1:1 battle — thin redirect to field hunt stats API.
    /// </summary>
    public class AutoBattleController : MonoBehaviour
    {
        public static float GetPlayerAtk() => FieldAutoHuntController.GetPlayerAtk();
        public static float GetPlayerCp() => FieldAutoHuntController.GetPlayerCp();
    }
}
