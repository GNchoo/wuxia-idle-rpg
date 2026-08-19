using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT.Market.Purchase
{
    public class MarketPurchaseScript : MonoBehaviour
    {
        [SerializeField]
        Animator[] Anim;
        // Start is called before the first frame update
        void Start()
        {
            //Anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ActivatePurchaseEffect()
        {
            
        }

        public void DeactivatePurchaseEffect()
        {
            //Anim.SetBool("Market Purchase", false);
        }
        public void ButtonEffect(int ButtonID)
        {
            Anim[ButtonID].SetBool("Market Purchase", true);
            StartCoroutine(DisablePurchaseEffect(ButtonID));
        }

        IEnumerator DisablePurchaseEffect(int ButtonID)
        {
            yield return new WaitForSeconds(.02f);
            Anim[ButtonID].SetBool("Market Purchase", false);
        }
    }
}

