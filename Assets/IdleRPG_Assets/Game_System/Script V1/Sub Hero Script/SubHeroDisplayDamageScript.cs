using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.FieldAttack;
using TMPro;

namespace SAMPLETEXT.SubHero.DisplayDamage
{
    public class SubHeroDisplayDamageScript : MonoBehaviour
    {

        FieldAttackScript[] MainFieldAttackCollection;
        [SerializeField]
        int SubHeroDisplayAttackID;
        TextMeshProUGUI ThisTextMesh;
        [SerializeField]
        GameObject DestroyCollection;
        // Start is called before the first frame update
        void Start()
        {
            MainFieldAttackCollection = GameObject.FindObjectsOfType<FieldAttackScript>();
            ThisTextMesh = GetComponent<TextMeshProUGUI>();

            for (int i = 0; i < MainFieldAttackCollection.Length; i++)
            {
                if (MainFieldAttackCollection[i].FieldID == SubHeroDisplayAttackID)
                {
                    if (MainFieldAttackCollection[i].missedAttack == false && MainFieldAttackCollection[i].subHerocriticalAttacked == false)
                    {
                        ThisTextMesh.color = Color.white;
                        //ThisTextMesh.text = MainFieldAttackCollection[i].AttackDamage.ToString("F0");

                        if (MainFieldAttackCollection[i].AttackDamage <= 999)
                        {
                            ThisTextMesh.text = MainFieldAttackCollection[i].AttackDamage.ToString("F0");
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].AttackDamage / 1000f).ToString("F2") + "K";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].AttackDamage / 1000000f).ToString("F2") + "M";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 9999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].AttackDamage / 1000000000f).ToString("F2") + "B";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].AttackDamage / 1000000000000f).ToString("F2") + "T";
                        }
                    }
                    else if (MainFieldAttackCollection[i].missedAttack == false && MainFieldAttackCollection[i].subHerocriticalAttacked == true)
                    {
                        ThisTextMesh.color = Color.red;
                        //ThisTextMesh.text = MainFieldAttackCollection[i].AttackDamage.ToString("F0");

                        if (MainFieldAttackCollection[i].AttackDamage <= 999)
                        {
                            ThisTextMesh.text = MainFieldAttackCollection[i].subHerocriticalDamage.ToString("F0");
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].subHerocriticalDamage / 1000f).ToString("F2") + "K";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].subHerocriticalDamage / 1000000f).ToString("F2") + "M";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 9999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].subHerocriticalDamage / 1000000000f).ToString("F2") + "B";
                        }
                        else if (MainFieldAttackCollection[i].AttackDamage <= 999999999999999f)
                        {
                            ThisTextMesh.text = (MainFieldAttackCollection[i].subHerocriticalDamage / 1000000000000f).ToString("F2") + "T";
                        }
                    }
                    else if (MainFieldAttackCollection[i].missedAttack == true)
                    {
                        ThisTextMesh.color = Color.black;
                        ThisTextMesh.text = "Miss";
                    }


                }
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

