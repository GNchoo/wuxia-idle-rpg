using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SAMPLETEXT.ItemPurchase.Manager.Gem;


namespace SAMPLETEXT
{
    public class RefreshVIPText : MonoBehaviour
    {

        public TMP_Text vipText;
        public DiamondPurchaseManagerScript shopObject;
        public void OnEnable()
        {
            vipText.text = "VIP: " + shopObject.VIPCountValue.ToString();
        }
    }
}