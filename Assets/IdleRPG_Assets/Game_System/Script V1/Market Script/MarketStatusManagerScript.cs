using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SAMPLETEXT.Market.Manager;

namespace SAMPLETEXT.Market.Manager.Status
{
    public class MarketStatusManagerScript : MonoBehaviour
    {
        [SerializeField]
        MarketManagerScript MainMarket;
        [Header("Market Status Settings")]
        [SerializeField]
        TextMeshProUGUI MarketNameText;
        [SerializeField]
        TextMeshProUGUI MarketCostText;
        [SerializeField]
        TextMeshProUGUI MarketNextMileStoneText;
        [SerializeField]
        TextMeshProUGUI MarketCurrentTimeText;
        [SerializeField]
        TextMeshProUGUI MarketNextTimeText;
        [SerializeField]
        TextMeshProUGUI MarketCurrentIncomeText;
        [SerializeField]
        TextMeshProUGUI MarketNextIncomeText;
        [SerializeField]
        Image MarketStatusImage;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void MarketStatusDisplayText(int MarketStatusID)
        {
            MarketStatusID = MainMarket.MarketStatusID;
            MarketNameText.text = MainMarket.MarketNameStringCollection[MarketStatusID] + " " + "LV" + MainMarket.MarketPurchaseCountValueCollection[MarketStatusID].ToString();
            MarketNextMileStoneText.text = "NEXT MILESTONE " + MainMarket.MarketNextMileStoneStatus[MarketStatusID];
            MarketCurrentTimeText.text = MainMarket.MarketTimerCDValueCollection[MarketStatusID].ToString();
            MarketNextTimeText.text = MainMarket.MarketTimerCDValueCollection[MarketStatusID].ToString();

            MarketStatusImage.sprite = MainMarket.MarketSpriteStatus[MarketStatusID];

			if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] > 999999999999999)
			{
				MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] = 999999999999999;
			}

            // Current Cost
            if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] <= 999)
            {
                MarketCostText.text = "COST: " + MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID].ToString("F0");
            }
            else if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] <= 999999)
            {
                MarketCostText.text = "COST: " + (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] / 1000f).ToString("F2") + "K";
            }
            else if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] <= 999999999)
            {
                MarketCostText.text = "COST: " + (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] / 1000000f).ToString("F2") + "M";
            }
            else if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] <= 999999999999)
            {
                MarketCostText.text = "COST: " + (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] / 1000000000f).ToString("F2") + "B";
            }
            else if (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] <= 999999999999999)
            {
                MarketCostText.text = "COST: " + (MainMarket.MarketPurchaseCostCountValueCollection[MarketStatusID] / 1000000000000f).ToString("F2") + "T";
            }


            if (MainMarket.MarketIncomeValueCollection[MarketStatusID] > 999999999999999)
			{
				MainMarket.MarketIncomeValueCollection[MarketStatusID] = 999999999999999;
			}

            // Current Income
            if (MainMarket.MarketIncomeValueCollection[MarketStatusID] <= 999)
            {
                MarketCurrentIncomeText.text = MainMarket.MarketIncomeValueCollection[MarketStatusID].ToString("F0");
            }
            else if (MainMarket.MarketIncomeValueCollection[MarketStatusID] <= 999999)
            {
                MarketCurrentIncomeText.text = (MainMarket.MarketIncomeValueCollection[MarketStatusID] / 1000f).ToString("F2") + "K";
            }
            else if (MainMarket.MarketIncomeValueCollection[MarketStatusID] <= 999999999)
            {
                MarketCurrentIncomeText.text = (MainMarket.MarketIncomeValueCollection[MarketStatusID] / 1000000f).ToString("F2") + "M";
            }
            else if (MainMarket.MarketIncomeValueCollection[MarketStatusID] <= 999999999999)
            {
                MarketCurrentIncomeText.text = (MainMarket.MarketIncomeValueCollection[MarketStatusID] / 1000000000f).ToString("F2") + "B";
            }
            else if (MainMarket.MarketIncomeValueCollection[MarketStatusID] <= 999999999999999)
            {
                MarketCurrentIncomeText.text = (MainMarket.MarketIncomeValueCollection[MarketStatusID] / 1000000000000f).ToString("F2") + "T";
            }

            if (MainMarket.MarketNextIncomeStatus[MarketStatusID] > 999999999999999)
			{
				MainMarket.MarketNextIncomeStatus[MarketStatusID] = 999999999999999;
			}

            // Next Income
            if (MainMarket.MarketNextIncomeStatus[MarketStatusID] <= 999)
            {
                MarketNextIncomeText.text = MainMarket.MarketNextIncomeStatus[MarketStatusID].ToString("F0");
            }
            else if (MainMarket.MarketNextIncomeStatus[MarketStatusID] <= 999999)
            {
                MarketNextIncomeText.text = (MainMarket.MarketNextIncomeStatus[MarketStatusID] / 1000f).ToString("F2") + "K";
            }
            else if (MainMarket.MarketNextIncomeStatus[MarketStatusID] <= 999999999)
            {
                MarketNextIncomeText.text = (MainMarket.MarketNextIncomeStatus[MarketStatusID] / 1000000f).ToString("F2") + "M";
            }
            else if (MainMarket.MarketNextIncomeStatus[MarketStatusID] <= 999999999999)
            {
                MarketNextIncomeText.text = (MainMarket.MarketNextIncomeStatus[MarketStatusID] / 1000000000f).ToString("F2") + "B";
            }
            else if (MainMarket.MarketNextIncomeStatus[MarketStatusID] <= 999999999999999)
            {
                MarketNextIncomeText.text = (MainMarket.MarketNextIncomeStatus[MarketStatusID] / 1000000000000f).ToString("F2") + "T";
            }

        }
    }
}

