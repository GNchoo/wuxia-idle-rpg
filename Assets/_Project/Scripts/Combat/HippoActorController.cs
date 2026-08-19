using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// Hippo(Character Editor Megapack) 리그를 SP1 규약(PlayAnimation(name, fade))으로
    /// 모는 어댑터. CharacterActorView가 SP1·Hippo를 같은 방식으로 몰 수 있게 한다.
    /// Hippo는 Animator 기반: 하체 State(int) + 상체 트리거(Slash/Jab/Hit).
    /// 네이티브 방향은 오른쪽(SP1은 왼쪽) — ActorView가 RigFlip으로 반영.
    /// </summary>
    public class HippoActorController : MonoBehaviour
    {
        public Character Char;

        void Awake()
        {
            if (Char == null) Char = GetComponent<Character>() ?? GetComponentInChildren<Character>();
        }

        void Start()
        {
            if (Char != null) Char.GetReady();   // 무기 든 전투 자세 고정
        }

        public void PlayAnimation(string animName, float fade = 0f, float normalizedStartTime = 0f)
        {
            if (Char == null || Char.Animator == null || !Char.Animator.isInitialized) return;

            if (animName.StartsWith("attack"))
            {
                // 스윙류는 Slash, 그 외(주문·활 등 빠른 동작)는 Jab — 전 몹 근접 캐스팅 전제
                if (animName.Contains("spell") || animName.Contains("bow") || animName.Contains("stab"))
                    Char.Jab();
                else
                    Char.Slash();
                return;
            }
            if (animName.StartsWith("hit")) { Char.Hit(); return; }
            if (animName.StartsWith("die")) { Char.SetState(CharacterState.DeathF); return; }

            switch (animName)
            {
                case "walk": Char.SetState(CharacterState.Walk); break;
                case "run": Char.SetState(CharacterState.Run); break;
                // 리그에 점프 클립이 없다 — 웅크림+달리기로 도약을 만든다 (ActorView가 두 단계로 호출)
                case "crouch": Char.SetState(CharacterState.Crouch); break;
                case "jump": Char.SetState(CharacterState.Run); break;
                default: Char.SetState(CharacterState.Idle); break;
            }
        }
    }
}
