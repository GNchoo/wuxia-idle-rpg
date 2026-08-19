using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAMPLETEXT.Gameplay.Manager.MainHero;
using TMPro;

namespace SAMPLETEXT.MainHero.DisplayDamage
{
    public class MainHeroDisplayDamageScript : MonoBehaviour
    {
        GameplayMainHeroManagerScript MainHeroDamage;
        TextMeshProUGUI ThisTextMesh;
        [SerializeField]
        GameObject DestroyCollection;

        // Start is called before the first frame update
        void Start()
        {
            ThisTextMesh = GetComponent<TextMeshProUGUI>();
            MainHeroDamage = FindObjectOfType<GameplayMainHeroManagerScript>();

            if(MainHeroDamage.missedAttack == false && MainHeroDamage.criticalAttacked == false)
            {
                ThisTextMesh.color = Color.white;
                if(MainHeroDamage.MainHeroDPSDamageValue <= 999)
                {
                    ThisTextMesh.text = MainHeroDamage.MainHeroDPSDamageValue.ToString("F0");
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000f).ToString("F2") + "K";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000f).ToString("F2") + "M";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 9999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000000f).ToString("F2") + "B";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000000000f).ToString("F2") + "T";
                }
            }
            else if (MainHeroDamage.missedAttack == false && MainHeroDamage.criticalAttacked == true)
            {
                ThisTextMesh.color = Color.red;
                if (MainHeroDamage.MainHeroDPSDamageValue <= 999)
                {
                    ThisTextMesh.text = MainHeroDamage.MainHeroDPSDamageValue.ToString("F0");
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000f).ToString("F2") + "K";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000f).ToString("F2") + "M";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 9999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000000f).ToString("F2") + "B";
                }
                else if (MainHeroDamage.MainHeroDPSDamageValue <= 999999999999999f)
                {
                    ThisTextMesh.text = (MainHeroDamage.MainHeroDPSDamageValue / 1000000000000f).ToString("F2") + "T";
                }
            }
            else if (MainHeroDamage.missedAttack == true)
            {
                ThisTextMesh.color = Color.black;
                ThisTextMesh.text = "Miss";
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

