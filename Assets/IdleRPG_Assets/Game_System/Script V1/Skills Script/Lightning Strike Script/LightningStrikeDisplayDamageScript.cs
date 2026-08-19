using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.SkillsAnimation.Manager;
using SAMPLETEXT.Gameplay.Manager.Enemy;

namespace SAMPLETEXT.Skills.DisplayDamage
{
    public class LightningStrikeDisplayDamageScript : MonoBehaviour
    {
        LightningStrikeAnimationScript LightningStrikeSkillFieldAttack;
        MeteorShowerAnimationScript MeteorShowerSkillFieldAttack;
        //FieldAttackScript[] MainFieldAttackCollection;
        //[SerializeField]
        //int SubHeroDisplayAttackID;
        TextMeshProUGUI ThisTextMesh;
        [SerializeField]
        GameObject DestroyCollection;

        [Header("Skills Settings Control")]
        [SerializeField]
        bool LightningStrikeSkill;
        [SerializeField]
        bool MeteorShowerSkill;
        [SerializeField]
        bool IceSpikeSkill;
        [SerializeField]
        bool ScaryScreamSkill;
        // Start is called before the first frame update
        void Start()
        {
            if (LightningStrikeSkill == true)
            {
                LightningStrikeSkillFieldAttack = GameObject.FindObjectOfType<LightningStrikeAnimationScript>();
                ThisTextMesh = GetComponent<TextMeshProUGUI>();
                ThisTextMesh.text = LightningStrikeSkillFieldAttack.LightningStrikeDamageValue.ToString("F0");
            }

            if (MeteorShowerSkill == true)
            {
                MeteorShowerSkillFieldAttack = GameObject.FindObjectOfType<MeteorShowerAnimationScript>();
                ThisTextMesh = GetComponent<TextMeshProUGUI>();
                ThisTextMesh.text = MeteorShowerSkillFieldAttack.MeteorDamageValue.ToString("F0");
            }




        }

        // Update is called once per frame
        void Update()
        {

        }

        public void EndAnimation()
        {
            Destroy(DestroyCollection.gameObject);
        }
    }
}

