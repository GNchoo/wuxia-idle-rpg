using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Data.Manager.Achievement;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using TMPro;

namespace SAMPLETEXT.Achievement.Manager
{
	public class AchievementManagerScript : MonoBehaviour
	{
		[Header ("Save System")]
		[SerializeField]
		JSONGameplayAchievementManagerScript AchievementSave;

		[Header("Wallet Settings")]
		[SerializeField]
		WalletManagerScript MainWallet;

		[Header("Enemy Settings")]
		[SerializeField]
		GameplayEnemyManagerScript MainEnemy;

		[Header("Achievement Settings")]
		[SerializeField]
		Button[] AchievementButtonCollection;
		[SerializeField]
		public List<bool> AchievementCompleteCollection = new List<bool>(); // New JSON Achievement
		int AchievementID;
		[SerializeField]
		TextMeshProUGUI[] AchievementTalentPointsCollectionText;
		public List<float> AchievementGemPointsCollection = new List<float>(); // New JSON Achievement
		public List<float> AchievementGemPointsIncreaseCollection = new List<float>(); // New JSON Achievement
		[SerializeField]
		float[] AchievementGemPointsAdditionalInIncreaseCollection;
		public List<float> CurrentAchievementPointsValueCollection = new List<float>(); // New JSON Achievement
		public List<float> MaxAchievementPointsValueCollection = new List<float>(); // New JSON Achievement
		public List<float> AchievementPointsValueIncreaseCollection = new List<float>(); // New JSON Achievement
		[SerializeField]
		float[] AchievementPointsValueAdditionalInIncreaseCollection;
		[SerializeField]
		bool[] AchievementSingleTask;
		[SerializeField]
		TextMeshProUGUI[] AchievementPointsValueCollectionText;
		[SerializeField]
		string CompleteAchievementString;

		[Header("Building Upgrade Settings")]
		public float AchievementBuildingUpgradeAdditional;
		// Start is called before the first frame update
		void Start()
		{
			//FirstCheck();
		}

		public void FirstCheck()
		{
			//Achievement Points
			for (int i = 0; i < AchievementPointsValueCollectionText.Length; i++)
			{
				if (AchievementSingleTask[i] == false)
				{
					AchievementPointsValueCollectionText[i].gameObject.SetActive(true);
				}

				if (AchievementSingleTask[i] == true)
				{
					AchievementPointsValueCollectionText[i].gameObject.SetActive(false);
				}

				AchievementPointsValueCollectionText[i].text = CurrentAchievementPointsValueCollection[i].ToString() + " / " + MaxAchievementPointsValueCollection[i].ToString();
			}

			//Achievement Gem Points and Achievmement Complete
			for (int i = 0; i < AchievementTalentPointsCollectionText.Length; i++)
			{
				if (AchievementCompleteCollection[i] == true)
				{
					AchievementTalentPointsCollectionText[i].text = CompleteAchievementString;
					AchievementButtonCollection[i].interactable = false;
				}

				if (AchievementCompleteCollection[i] == false)
				{
					AchievementTalentPointsCollectionText[i].text = AchievementGemPointsCollection[i].ToString();

					if (CurrentAchievementPointsValueCollection[i] < MaxAchievementPointsValueCollection[i])
					{
						AchievementButtonCollection[i].interactable = false;
					}

					if (CurrentAchievementPointsValueCollection[i] >= MaxAchievementPointsValueCollection[i])
					{
						AchievementButtonCollection[i].interactable = true;
					}
				}
					
			}

			MainWallet.AchievementGoldWalletValue = CurrentAchievementPointsValueCollection[2];
			MainEnemy.AchievementMaxWaveCountValue = CurrentAchievementPointsValueCollection[21];
		}

		// Update is called once per frame
		//void Update()
		//{

		//}

		public void AchievementCollectButton(int ButtonID)
		{
			AchievementID = ButtonID;
			MainWallet.GemWalletValue += AchievementGemPointsCollection[AchievementID];
			MainWallet.WalletDataSave();
			MainWallet.WalletValueManualUpdate();

			if (AchievementSingleTask[AchievementID] == false)
			{
				MaxAchievementPointsValueCollection[AchievementID] += AchievementPointsValueIncreaseCollection[AchievementID];
				AchievementPointsValueIncreaseCollection[AchievementID] += AchievementPointsValueAdditionalInIncreaseCollection[AchievementID];

				AchievementGemPointsCollection[AchievementID] += AchievementGemPointsIncreaseCollection[AchievementID];
				AchievementGemPointsIncreaseCollection[AchievementID] += AchievementGemPointsAdditionalInIncreaseCollection[AchievementID];

				if (CurrentAchievementPointsValueCollection.Count != 17)
				{
					CurrentAchievementPointsValueCollection[17] += 1;
				}
			}
			AchievementManualUpdate(AchievementID);
		}

		void AchievementManualUpdate(int AchievementTempID)
		{
			if (AchievementSingleTask[AchievementTempID] == false)
			{
				AchievementPointsValueCollectionText[AchievementTempID].gameObject.SetActive(true);
			}

			else if (AchievementSingleTask[AchievementTempID] == true)
			{
				AchievementPointsValueCollectionText[AchievementTempID].gameObject.SetActive(false);

				if (AchievementCompleteCollection[AchievementTempID] == false)
				{
					MainWallet.GemWalletValue += AchievementGemPointsCollection[AchievementTempID];
					AchievementCompleteCollection[AchievementTempID] = true;
				}
				
			}

			// Check Achievement Complete
			if (AchievementCompleteCollection[AchievementTempID] == true)
			{
				AchievementTalentPointsCollectionText[AchievementTempID].text = CompleteAchievementString;
				AchievementButtonCollection[AchievementTempID].interactable = false;
			}

			else if (AchievementCompleteCollection[AchievementTempID] == false)
			{
				AchievementTalentPointsCollectionText[AchievementTempID].text = AchievementGemPointsCollection[AchievementTempID].ToString();

				if (CurrentAchievementPointsValueCollection[AchievementTempID] < MaxAchievementPointsValueCollection[AchievementTempID])
				{
					AchievementButtonCollection[AchievementTempID].interactable = false;
				}

				if (CurrentAchievementPointsValueCollection[AchievementTempID] >= MaxAchievementPointsValueCollection[AchievementTempID])
				{
					AchievementButtonCollection[AchievementTempID].interactable = true;
				}
			}

			// Save System
			AchievementSave.SaveFile();
		}

		public void CollectedGemsAchievementCheckManualUpdate()
		{
			if (AchievementCompleteCollection[1] == false)
			{
				CurrentAchievementPointsValueCollection[1] = 1;
				AchievementButtonManualUpdate(1);
			}
		}

		public void EarnedGoldQuantityAchievementCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[2] = MainWallet.AchievementGoldWalletValue;
			AchievementButtonManualUpdate(2);
		}

		public void MaximumHeroEvolveCheckManualUpdate()
		{
			if (AchievementCompleteCollection[5] == false)
			{
				CurrentAchievementPointsValueCollection[5] = 1;
				AchievementButtonManualUpdate(5);
			}
		}

		public void ArtifactLevelCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[8] += 1;
			AchievementButtonManualUpdate(8);
		}

		public void CollectedArtifactCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[9] += 1;
			AchievementButtonManualUpdate(9);
		}

		public void FirstPrestigeAchievementCheckManualUpdate()
		{
			if (AchievementCompleteCollection[10] == false)
			{
				CurrentAchievementPointsValueCollection[10] = 1;
				AchievementButtonManualUpdate(10);
			}
		}
		public void FindFirstHeroCheckManualUpdate()
		{
			if (AchievementCompleteCollection[12] == false)
			{
				CurrentAchievementPointsValueCollection[12] = 1;
				AchievementButtonManualUpdate(12);
			}
		}

		public void CollectedHeroesCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[13] += 1;
			AchievementButtonManualUpdate(13);
		}

		public void LegendaryHeroCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[15] += 1;
			AchievementButtonManualUpdate(15);
		}

		public void BuildingUpgradesCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[16] += AchievementBuildingUpgradeAdditional;
			AchievementButtonManualUpdate(16);
		}



		public void WavesConquerorQuantityAchievementCheckManualUpdate()
		{
			CurrentAchievementPointsValueCollection[21] = MainEnemy.AchievementMaxWaveCountValue;
			AchievementButtonManualUpdate(21);
		}



		void AchievementButtonManualUpdate(int AchievementTempID)
		{
			// Check Achievement Complete
			if (AchievementCompleteCollection[AchievementTempID] == true)
			{
				AchievementTalentPointsCollectionText[AchievementTempID].text = CompleteAchievementString;
				AchievementButtonCollection[AchievementTempID].interactable = false;
			}

			else if (AchievementCompleteCollection[AchievementTempID] == false)
			{
				AchievementTalentPointsCollectionText[AchievementTempID].text = AchievementGemPointsCollection[AchievementTempID].ToString();

				if (CurrentAchievementPointsValueCollection[AchievementTempID] < MaxAchievementPointsValueCollection[AchievementTempID])
				{
					AchievementButtonCollection[AchievementTempID].interactable = false;
				}

				if (CurrentAchievementPointsValueCollection[AchievementTempID] >= MaxAchievementPointsValueCollection[AchievementTempID])
				{
					AchievementButtonCollection[AchievementTempID].interactable = true;
				}
			}

			// Save System
			AchievementSave.SaveFile();
		}
	}
}

