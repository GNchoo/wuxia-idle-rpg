using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Achievement.Manager;
using SAMPLETEXT.Data.Manager.Village;
using SAMPLETEXT.Data.Manager.Wallet;
using SAMPLETEXT.Data.Manager.Enemy;
using SAMPLETEXT.Data.Manager.PurchaseGem;
using SAMPLETEXT.Data.Manager.SubHeroes;
using SAMPLETEXT.Data.Manager.Artifact;
using SAMPLETEXT.Data.Manager.Boost;
using SAMPLETEXT.Data.Manager.Account.Profile;
using SAMPLETEXT.Data.Manager.Talent;
using SAMPLETEXT.Data.Manager.MainHero;
using SAMPLETEXT.Data.Manager.Inventory;
using SAMPLETEXT.Data.Manager.Achievement;
using UnityEngine.SceneManagement;
using SAMPLETEXT.Artifact.Manager;
using TMPro;

namespace SAMPLETEXT.Account.Manager.Profile
{
    public class ProfileManagerScript : MonoBehaviour
    {
        [SerializeField]
        TalentsManagerScript MainTalents;
        [SerializeField]
        GameplayEnemyManagerScript MainGameplayEnemy;
        [Header("Player Avatar Settings")]
        [SerializeField]
        Sprite talentPointSprite;
        [SerializeField]
        Sprite[] AvatarCollection;
        [SerializeField]
        Image PlayerAvatar;
        [SerializeField]
        Image PlayerAvatarFrontView;
        public int PlayerAvatarCount;
        [SerializeField]
        GameObject AvatarListObj;
        [SerializeField]
        TMP_InputField PlayerNameText;
        public string PlayerNameStringValue;
        public string GUIDValue;
        [SerializeField]
        TextMeshProUGUI GUIDValueText;

        [Header("Points Settings")]
        public float PointsValue;
        //[SerializeField]
        //TextMeshProUGUI PointsValueText;
        [SerializeField]
        Button PrestigeYesButton;

        [Header("Pop Up Window Settings")]
        [SerializeField]
        GameObject PopUpWindowObj;
        [SerializeField]
        Image PopUpWindowPrestigeImage;
        [SerializeField]
        TextMeshProUGUI PurchasePrestigeValueCountText;

		[Header("Achievements Settings")]
		[SerializeField]
		AchievementManagerScript MainAchievement;

		[Header("Reset Settings")]
		[SerializeField]
		JSONGameplayVillageManagerScript VillageSaveSystem;
		[SerializeField]
		JSONGameplayWalletManagerScript WalletSaveSystem;
		[SerializeField]
		JSONGameplayEnemyManagerScript EnemySaveSystem;
		[SerializeField]
		JSONGameplayPurchaseGemManagerScript DiamondSaveSystem;
		[SerializeField]
		JSONGameplaySubHeroManagerScript SubHeroSaveSystem;
		[SerializeField]
		JSONGameplayArtifactManagerScript ArtifactSaveSystem;
		[SerializeField]
		JSONGameplayBoostManagerScript BoostSaveSystem;
		[SerializeField]
		JSONGameplayAccountProfileManagerScript AccountSaveSystem;
		[SerializeField]
		JSONGameplayTalentManagerScript TalentSaveSystem;
		[SerializeField]
		JSONGameplayMainHeroManagerScript MainHeroSaveSystem;
		[SerializeField]
		JSONGameplayInventoryManagerScript InventorySystem;
		[SerializeField]
		JSONGameplayAchievementManagerScript AchievementSaveSystem;
        [SerializeField] ArtifactManagerScript artifactScript;

		// Start is called before the first frame update
		void Start()
        {
            //GenerateGUID();
            FirstCheck();
        }

        public void GenerateGUID()
        {
            Guid myGuid = Guid.NewGuid();
            GUIDValueText.text = "UID: " + myGuid.ToString();
            GUIDValue = "UID: " + myGuid.ToString();
        }

        public void FirstCheck()
        {
            if (GUIDValue == string.Empty)
            {
                GenerateGUID();
            }

            if (PlayerNameStringValue == string.Empty)
            {
                PlayerNameStringValue = "NO NAME";
                PlayerNameText.text = PlayerNameStringValue.ToString();

            }

            if (PlayerNameStringValue != string.Empty)
            {
                PlayerNameText.text = PlayerNameStringValue.ToString();
            }

            //PointsValueText.text = "POINTS: " + PointsValue.ToString();
            PlayerAvatar.sprite = null;
            PlayerAvatarFrontView.sprite = null;
            PlayerAvatar.sprite = AvatarCollection[PlayerAvatarCount];
            PlayerAvatarFrontView.sprite = AvatarCollection[PlayerAvatarCount];

            PrestigeCheckAcceptButton();
        }

        //public void PointsTextManualUpdate()
        //{
        //    //PointsValueText.text = "POINTS: " + PointsValue.ToString();
        //}

        public void PrestigeAcceptButton()
        {
            PopUpWindowActivate();
            MainGameplayEnemy.ResetEnemyWave();
            PrestigeCheckAcceptButton();
        }

        void PopUpWindowActivate()
        {
            PopUpWindowObj.gameObject.SetActive(true);
            PopUpWindowPrestigeImage.sprite = null;
            PopUpWindowPrestigeImage.sprite = talentPointSprite;
            PurchasePrestigeValueCountText.text = string.Empty;

            if (MainGameplayEnemy.ItemChanceCurrentRangeValue > MainGameplayEnemy.ItemChanceDropMaxValue)
            {
                PurchasePrestigeValueCountText.text = "POINTS: " + MainGameplayEnemy.TempPointsCount.ToString();
            }
            else if (MainGameplayEnemy.ItemChanceCurrentRangeValue <= MainGameplayEnemy.ItemChanceDropMaxValue)
            {
                PurchasePrestigeValueCountText.text = "POINTS: " + (MainGameplayEnemy.TempPointsCount * 2).ToString();
            }
            
        }

        public void CollectPrestigePointButton()
        {
            PopUpWindowPrestigeImage.sprite = null;
            PurchasePrestigeValueCountText.text = string.Empty;
            PopUpWindowObj.gameObject.SetActive(false);
        }

        public void PrestigeCheckAcceptButton()
        {
            if (MainGameplayEnemy.TempPointsCount >= 1)
            {
                PrestigeYesButton.interactable = true;
            }
            if (MainGameplayEnemy.TempPointsCount <= 0)
            {
                MainGameplayEnemy.TempPointsCount = 0;
                PrestigeYesButton.interactable = false;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangePlayerAvatar(int AvatarID)
        {
            PlayerAvatarCount = AvatarID;
            ChangePlayerAvatarActivate();

        }
        void ChangePlayerAvatarActivate()
        {
            PlayerAvatar.sprite = null;
            PlayerAvatar.sprite = AvatarCollection[PlayerAvatarCount];
            PlayerAvatarFrontView.sprite = null;
            PlayerAvatarFrontView.sprite = AvatarCollection[PlayerAvatarCount];
            AvatarListObj.gameObject.SetActive(false);

        }

        public void ChangePlayerNameActivate()
        {
            PlayerNameStringValue = "POINTS: " + PlayerNameText.text;
        }

		public void ResetData()
		{
			// Execute Reset System
			VillageSaveSystem.DeleteFile();
			WalletSaveSystem.DeleteFile();
			EnemySaveSystem.DeleteFile();
			DiamondSaveSystem.DeleteFile();
			SubHeroSaveSystem.DeleteFile();
			ArtifactSaveSystem.DeleteFile();
			BoostSaveSystem.DeleteFile();
			AccountSaveSystem.DeleteFile();
			TalentSaveSystem.DeleteFile(); 
			MainHeroSaveSystem.DeleteFile();
			InventorySystem.DeleteFile();
			AchievementSaveSystem.DeleteFile();
            PlayerPrefs.DeleteAll();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
    }
}

