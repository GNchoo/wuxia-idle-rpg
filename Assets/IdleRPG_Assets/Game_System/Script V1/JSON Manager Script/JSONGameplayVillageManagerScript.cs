using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Market.Manager;
using SAMPLETEXT.Data.Class.Village;

namespace SAMPLETEXT.Data.Manager.Village
{
    public class JSONGameplayVillageManagerScript : MonoBehaviour
    {
        [SerializeField]
        MarketManagerScript MainVillageTab;

        [Header("Load Village Settings")]
        [SerializeField]
        bool PCSave;
        [SerializeField]
        bool AndroidSave;
        [SerializeField]
        string FolderPath;
        [SerializeField]
        string FileName;

        private void Awake()
        {
            FolderPath = Application.persistentDataPath + "/JSON/";

            if (FileName == string.Empty)
            {
                FileName = "Village_Stats.text";
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                PCSave = true;
                AndroidSave = false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                PCSave = false;
                AndroidSave = true;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                PCSave = false;
                AndroidSave = true;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //MainVillageTab = GameObject.FindObjectOfType<MarketManagerScript>();
            LoadFile();
        }

        public void LoadFile()
        {
            JSONLoad();
        }

        public void SaveFile()
        {
            JSONSave();
        }

        private JSONVillageDatabaseClass createSaveGameObject()
        {
            JSONVillageDatabaseClass save = new JSONVillageDatabaseClass();

            foreach (float MIVC in MainVillageTab.MarketIncomeValueCollection)
            {
                save._JSONMarketIncomeValueCollection.Add(MIVC);
            }

            foreach (float MTCDV in MainVillageTab.MarketTimerCDValueCollection)
            {
                save._JSONMarketTimerCDValueCollection.Add(MTCDV);
            }

            foreach (float MPCVC in MainVillageTab.MarketPurchaseCountValueCollection)
            {
                save._JSONMarketPurchaseCountValueCollection.Add(MPCVC);
            }

            foreach (float MPCCVC in MainVillageTab.MarketPurchaseCostCountValueCollection)
            {
                save._JSONMarketPurchaseCostCountValueCollection.Add(MPCCVC);
            }

            foreach(int VUL in MainVillageTab.VillageUpgradeLevel)
            {
                save._JSONVillageUpgradeLevel.Add(VUL);
            }

            save._JSONTotalOfflineEarningHoursValue = MainVillageTab.TotalOfflineEarningHoursValue;
            save._JSONOfflineTotalCoinEarningsValue = MainVillageTab.OfflineTotalCoinEarningsValue;

            save._JSONVillageMarketUpgradeCurrentLevel = MainVillageTab.VillageMarketUpgradeCurrentLevel;
            save._JSONVillageTownHallUpgradeCurrentLevel = MainVillageTab.VillageTownHallUpgradeCurrentLevel;
            save._JSONVillageMagesGuildUpgradeCurrentLevel = MainVillageTab.VillageMagesGuildUpgradeCurrentLevel;
            save._JSONVillageButcherUpgradeCurrentLevel = MainVillageTab.VillageButcherUpgradeCurrentLevel;
            save._JSONVillageAlchemistUpgradeCurrentLevel = MainVillageTab.VillageAlchemistUpgradeCurrentLevel;
            save._JSONVillageFisherMansHutUpgradeCurrentLevel = MainVillageTab.VillageFisherMansHutUpgradeCurrentLevel;
            save._JSONVillageHarborUpgradeCurrentLevel = MainVillageTab.VillageHarborUpgradeCurrentLevel;
            save._JSONVillageCowFarmUpgradeCurrentLevel = MainVillageTab.VillageCowFarmUpgradeCurrentLevel;
            save._JSONVillageVolcanoUpgradeCurrentLevel = MainVillageTab.VillageVolcanoUpgradeCurrentLevel;
            save._JSONVillageOurStatueUpgradeCurrentLevel = MainVillageTab.VillageOurStatueUpgradeCurrentLevel;
            save._JSONVillageHoleInGroundUpgradeCurrentLevel = MainVillageTab.VillageHoleInGroundUpgradeCurrentLevel;
            save._JSONVillageCourtUpgradeCurrentLevel = MainVillageTab.VillageCourtUpgradeCurrentLevel;
            save._JSONVillageCenterSquareUpgradeCurrentLevel = MainVillageTab.VillageCenterSquareUpgradeCurrentLevel;
            save._JSONVillageTrainingGroundUpgradeCurrentLevel = MainVillageTab.VillageTrainingGroundUpgradeCurrentLevel;
            save._JSONVillagePlainCottageUpgradeCurrentLevel = MainVillageTab.VillagePlainCottageUpgradeCurrentLevel;
            save._JSONVillageChurchUpgradeCurrentLevel = MainVillageTab.VillageChurchUpgradeCurrentLevel;
            save._JSONVillageAltarUpgradeCurrentLevel = MainVillageTab.VillageAltarUpgradeCurrentLevel;
            save._JSONVillageChariotParkingUpgradeCurrentLevel = MainVillageTab.VillageChariotParkingUpgradeCurrentLevel;
            save._JSONVillageShipUpgradeCurrentLevel = MainVillageTab.VillageShipUpgradeCurrentLevel;
            save._JSONVillageArmoryUpgradeCurrentLevel = MainVillageTab.VillageArmoryUpgradeCurrentLevel;
            save._JSONVillageClinicUpgradeCurrentLevel = MainVillageTab.VillageClinicUpgradeCurrentLevel;
            save._JSONVillageIronWorksUpgradeCurrentLevel = MainVillageTab.VillageIronWorksUpgradeCurrentLevel;
            save._JSONVillageArtifactShopUpgradeCurrentLevel = MainVillageTab.VillageArtifactShopUpgradeCurrentLevel;
            save._JSONVillageCemeteryUpgradeCurrentLevel = MainVillageTab.VillageCemeteryUpgradeCurrentLevel;
            save._JSONVillagePowerStationUpgradeCurrentLevel = MainVillageTab.VillagePowerStationUpgradeCurrentLevel;
            save._JSONVillageThievesGuildUpgradeCurrentLevel = MainVillageTab.VillageThievesGuildUpgradeCurrentLevel;
            save._JSONVillageGreenHouseUpgradeCurrentLevel = MainVillageTab.VillageGreenHouseUpgradeCurrentLevel;
            save._JSONVillageRobotFactoryUpgradeCurrentLevel = MainVillageTab.VillageRobotFactoryUpgradeCurrentLevel;
            save._JSONVillageMonsterZooUpgradeCurrentLevel = MainVillageTab.VillageMonsterZooUpgradeCurrentLevel;


            return save;
        }

        void JSONSave()
        {
            JSONVillageDatabaseClass save = createSaveGameObject();

            if (PCSave == true)
            {
                string directoryPath = Path.Combine(FolderPath);

                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string JsonString = JsonUtility.ToJson(save);
                StreamWriter sw = new StreamWriter(directoryPath + FileName);
                sw.Write(JsonString);
                sw.Close();
                Debug.Log("File Save");
            }

            if (AndroidSave == true)
            {
                string directoryPath = Path.Combine(Application.persistentDataPath);

                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string JsonString = JsonUtility.ToJson(save);
                StreamWriter sw = new StreamWriter(directoryPath + FileName);
                sw.Write(JsonString);
                sw.Close();
                Debug.Log("File Save");
            }
        }

		public void DeleteFile()
		{
			if (PCSave == true)
			{
				string directoryPath = Path.Combine(FolderPath);

				if (Directory.Exists(directoryPath) == true)
				{
					if (File.Exists(directoryPath + FileName))
					{
						File.Delete(directoryPath + FileName);
						Debug.Log("File Deleted");
					}
				}
			}

			if (AndroidSave == true)
			{
				string directoryPath = Path.Combine(Application.persistentDataPath);

				if (Directory.Exists(directoryPath) == true)
				{
					if (File.Exists(directoryPath + FileName))
					{
						File.Delete(directoryPath + FileName);
						Debug.Log("File Deleted");
					}
				}
			}
		}

		void JSONLoad()
        {
            if (PCSave == true)
            {
                string directoryPath = Path.Combine(FolderPath);

                if (File.Exists(directoryPath + FileName))
                {
                    StreamReader sr = new StreamReader(directoryPath + FileName);
                    string JsonString = sr.ReadToEnd();
                    sr.Close();

                    JSONVillageDatabaseClass load = JsonUtility.FromJson<JSONVillageDatabaseClass>(JsonString);

                   for (int i = 0; i < load._JSONMarketIncomeValueCollection.Count; i++ )
                    {
                        MainVillageTab.MarketIncomeValueCollection = load._JSONMarketIncomeValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketTimerCDValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketTimerCDValueCollection = load._JSONMarketTimerCDValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketPurchaseCountValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketPurchaseCountValueCollection = load._JSONMarketPurchaseCountValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketPurchaseCostCountValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketPurchaseCostCountValueCollection = load._JSONMarketPurchaseCostCountValueCollection;
                    }

                    for (int i = 0; i < load._JSONVillageUpgradeLevel.Count; i++)
                    {
                        MainVillageTab.VillageUpgradeLevel = load._JSONVillageUpgradeLevel;
                    }

                    MainVillageTab.TotalOfflineEarningHoursValue = load._JSONTotalOfflineEarningHoursValue;

                    MainVillageTab.OfflineTotalCoinEarningsValue = load._JSONOfflineTotalCoinEarningsValue;

                    MainVillageTab.VillageMarketUpgradeCurrentLevel = load._JSONVillageMarketUpgradeCurrentLevel;
                    MainVillageTab.VillageTownHallUpgradeCurrentLevel = load._JSONVillageTownHallUpgradeCurrentLevel;
                    MainVillageTab.VillageMagesGuildUpgradeCurrentLevel = load._JSONVillageMagesGuildUpgradeCurrentLevel;
                    MainVillageTab.VillageButcherUpgradeCurrentLevel = load._JSONVillageButcherUpgradeCurrentLevel;
                    MainVillageTab.VillageAlchemistUpgradeCurrentLevel = load._JSONVillageAlchemistUpgradeCurrentLevel;
                    MainVillageTab.VillageFisherMansHutUpgradeCurrentLevel = load._JSONVillageFisherMansHutUpgradeCurrentLevel;
                    MainVillageTab.VillageHarborUpgradeCurrentLevel = load._JSONVillageHarborUpgradeCurrentLevel;
                    MainVillageTab.VillageCowFarmUpgradeCurrentLevel = load._JSONVillageCowFarmUpgradeCurrentLevel;
                    MainVillageTab.VillageVolcanoUpgradeCurrentLevel = load._JSONVillageVolcanoUpgradeCurrentLevel;
                    MainVillageTab.VillageOurStatueUpgradeCurrentLevel = load._JSONVillageOurStatueUpgradeCurrentLevel;
                    MainVillageTab.VillageHoleInGroundUpgradeCurrentLevel = load._JSONVillageHoleInGroundUpgradeCurrentLevel;
                    MainVillageTab.VillageCourtUpgradeCurrentLevel = load._JSONVillageCourtUpgradeCurrentLevel;
                    MainVillageTab.VillageCenterSquareUpgradeCurrentLevel = load._JSONVillageCenterSquareUpgradeCurrentLevel;
                    MainVillageTab.VillageTrainingGroundUpgradeCurrentLevel = load._JSONVillageTrainingGroundUpgradeCurrentLevel;
                    MainVillageTab.VillagePlainCottageUpgradeCurrentLevel = load._JSONVillagePlainCottageUpgradeCurrentLevel;
                    MainVillageTab.VillageChurchUpgradeCurrentLevel = load._JSONVillageChurchUpgradeCurrentLevel;
                    MainVillageTab.VillageAltarUpgradeCurrentLevel = load._JSONVillageAltarUpgradeCurrentLevel;
                    MainVillageTab.VillageChariotParkingUpgradeCurrentLevel = load._JSONVillageChariotParkingUpgradeCurrentLevel;
                    MainVillageTab.VillageShipUpgradeCurrentLevel = load._JSONVillageShipUpgradeCurrentLevel;
                    MainVillageTab.VillageArmoryUpgradeCurrentLevel = load._JSONVillageArmoryUpgradeCurrentLevel;
                    MainVillageTab.VillageClinicUpgradeCurrentLevel = load._JSONVillageClinicUpgradeCurrentLevel;
                    MainVillageTab.VillageIronWorksUpgradeCurrentLevel = load._JSONVillageIronWorksUpgradeCurrentLevel;
                    MainVillageTab.VillageArtifactShopUpgradeCurrentLevel = load._JSONVillageArtifactShopUpgradeCurrentLevel;
                    MainVillageTab.VillageCemeteryUpgradeCurrentLevel = load._JSONVillageCemeteryUpgradeCurrentLevel;
                    MainVillageTab.VillagePowerStationUpgradeCurrentLevel = load._JSONVillagePowerStationUpgradeCurrentLevel;
                    MainVillageTab.VillageThievesGuildUpgradeCurrentLevel = load._JSONVillageThievesGuildUpgradeCurrentLevel;
                    MainVillageTab.VillageGreenHouseUpgradeCurrentLevel = load._JSONVillageGreenHouseUpgradeCurrentLevel;
                    MainVillageTab.VillageRobotFactoryUpgradeCurrentLevel = load._JSONVillageRobotFactoryUpgradeCurrentLevel;
                    MainVillageTab.VillageMonsterZooUpgradeCurrentLevel = load._JSONVillageMonsterZooUpgradeCurrentLevel;

                    MainVillageTab.ManualMarketValueCheckUpdate();
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    SaveFile();
                }
            }

            if (AndroidSave == true)
            {
                string directoryPath = Path.Combine(Application.persistentDataPath);
                if (File.Exists(directoryPath + FileName))
                {
                    StreamReader sr = new StreamReader(directoryPath + FileName);
                    string JsonString = sr.ReadToEnd();
                    sr.Close();

                    JSONVillageDatabaseClass load = JsonUtility.FromJson<JSONVillageDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONMarketIncomeValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketIncomeValueCollection = load._JSONMarketIncomeValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketTimerCDValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketTimerCDValueCollection = load._JSONMarketTimerCDValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketPurchaseCountValueCollection.Count; i++)
                    {
                        MainVillageTab.MarketPurchaseCountValueCollection = load._JSONMarketPurchaseCountValueCollection;
                    }

                    for (int i = 0; i < load._JSONMarketPurchaseCostCountValueCollection.Count; i++)
                    {
                        MainVillageTab.TotalOfflineEarningHoursValue = load._JSONTotalOfflineEarningHoursValue;
                    }

                    for (int i = 0; i < load._JSONVillageUpgradeLevel.Count; i++)
                    {
                        MainVillageTab.VillageUpgradeLevel = load._JSONVillageUpgradeLevel;
                    }

                    MainVillageTab.TotalOfflineEarningHoursValue = load._JSONTotalOfflineEarningHoursValue;

                    MainVillageTab.OfflineTotalCoinEarningsValue = load._JSONOfflineTotalCoinEarningsValue;

                    MainVillageTab.VillageMarketUpgradeCurrentLevel = load._JSONVillageMarketUpgradeCurrentLevel;
                    MainVillageTab.VillageTownHallUpgradeCurrentLevel = load._JSONVillageTownHallUpgradeCurrentLevel;
                    MainVillageTab.VillageMagesGuildUpgradeCurrentLevel = load._JSONVillageMagesGuildUpgradeCurrentLevel;
                    MainVillageTab.VillageButcherUpgradeCurrentLevel = load._JSONVillageButcherUpgradeCurrentLevel;
                    MainVillageTab.VillageAlchemistUpgradeCurrentLevel = load._JSONVillageAlchemistUpgradeCurrentLevel;
                    MainVillageTab.VillageFisherMansHutUpgradeCurrentLevel = load._JSONVillageFisherMansHutUpgradeCurrentLevel;
                    MainVillageTab.VillageHarborUpgradeCurrentLevel = load._JSONVillageHarborUpgradeCurrentLevel;
                    MainVillageTab.VillageCowFarmUpgradeCurrentLevel = load._JSONVillageCowFarmUpgradeCurrentLevel;
                    MainVillageTab.VillageVolcanoUpgradeCurrentLevel = load._JSONVillageVolcanoUpgradeCurrentLevel;
                    MainVillageTab.VillageOurStatueUpgradeCurrentLevel = load._JSONVillageOurStatueUpgradeCurrentLevel;
                    MainVillageTab.VillageHoleInGroundUpgradeCurrentLevel = load._JSONVillageHoleInGroundUpgradeCurrentLevel;
                    MainVillageTab.VillageCourtUpgradeCurrentLevel = load._JSONVillageCourtUpgradeCurrentLevel;
                    MainVillageTab.VillageCenterSquareUpgradeCurrentLevel = load._JSONVillageCenterSquareUpgradeCurrentLevel;
                    MainVillageTab.VillageTrainingGroundUpgradeCurrentLevel = load._JSONVillageTrainingGroundUpgradeCurrentLevel;
                    MainVillageTab.VillagePlainCottageUpgradeCurrentLevel = load._JSONVillagePlainCottageUpgradeCurrentLevel;
                    MainVillageTab.VillageChurchUpgradeCurrentLevel = load._JSONVillageChurchUpgradeCurrentLevel;
                    MainVillageTab.VillageAltarUpgradeCurrentLevel = load._JSONVillageAltarUpgradeCurrentLevel;
                    MainVillageTab.VillageChariotParkingUpgradeCurrentLevel = load._JSONVillageChariotParkingUpgradeCurrentLevel;
                    MainVillageTab.VillageShipUpgradeCurrentLevel = load._JSONVillageShipUpgradeCurrentLevel;
                    MainVillageTab.VillageArmoryUpgradeCurrentLevel = load._JSONVillageArmoryUpgradeCurrentLevel;
                    MainVillageTab.VillageClinicUpgradeCurrentLevel = load._JSONVillageClinicUpgradeCurrentLevel;
                    MainVillageTab.VillageIronWorksUpgradeCurrentLevel = load._JSONVillageIronWorksUpgradeCurrentLevel;
                    MainVillageTab.VillageArtifactShopUpgradeCurrentLevel = load._JSONVillageArtifactShopUpgradeCurrentLevel;
                    MainVillageTab.VillageCemeteryUpgradeCurrentLevel = load._JSONVillageCemeteryUpgradeCurrentLevel;
                    MainVillageTab.VillagePowerStationUpgradeCurrentLevel = load._JSONVillagePowerStationUpgradeCurrentLevel;
                    MainVillageTab.VillageThievesGuildUpgradeCurrentLevel = load._JSONVillageThievesGuildUpgradeCurrentLevel;
                    MainVillageTab.VillageGreenHouseUpgradeCurrentLevel = load._JSONVillageGreenHouseUpgradeCurrentLevel;
                    MainVillageTab.VillageRobotFactoryUpgradeCurrentLevel = load._JSONVillageRobotFactoryUpgradeCurrentLevel;
                    MainVillageTab.VillageMonsterZooUpgradeCurrentLevel = load._JSONVillageMonsterZooUpgradeCurrentLevel;

                    MainVillageTab.ManualMarketValueCheckUpdate();
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    SaveFile();
                }
            }
        }

        private void OnApplicationQuit()
        {
            SaveFile();
        }

        private void OnApplicationFocus(bool focus)
        {
            //if (focus == true)
            //{
            //    LoadFile();
            //}
            if (focus == false)
            {
                SaveFile();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true)
            {
                SaveFile();
            }
            if (pause == false)
            {
                LoadFile();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

