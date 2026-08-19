using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Data.Class.Boost;
using SAMPLETEXT.ItemPurchase.Manager.Boost;

namespace SAMPLETEXT.Data.Manager.Boost
{
    public class JSONGameplayBoostManagerScript : MonoBehaviour
    {
        [SerializeField]
        ItemsPurchaseBoostManagerScript MainBoost;

        [Header("Load Artifact Settings")]
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
                FileName = "Boost_Stats.text";
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

		private JSONBoostDatabaseClass createSaveGameObject()
        {
            JSONBoostDatabaseClass save = new JSONBoostDatabaseClass();

            save._JSONActivateDoubleGoldIncomeCondition = MainBoost.ActivateDoubleGoldIncomeCondition;

            save._JSONDoubleGoldIncomeCountDownTimerValue = MainBoost.DoubleGoldIncomeCountDownTimerValue;
            save._JSONDoubleVillageIncomeCountdownTimerValue = MainBoost.DoubleVillageIncomeCountdownTimerValue;

            save._JSONActivateDoubleVillageIncomeCondition = MainBoost.ActivateDoubleVillageIncomeCondition;

            save._JSONActivateOfflineEarningsCondition = MainBoost.ActivateOfflineEarningsCondition;

            return save;
        }

        void JSONSave()
        {
            JSONBoostDatabaseClass save = createSaveGameObject();

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

                    JSONBoostDatabaseClass load = JsonUtility.FromJson<JSONBoostDatabaseClass>(JsonString);

                    MainBoost.ActivateDoubleGoldIncomeCondition = load._JSONActivateDoubleGoldIncomeCondition;

                    MainBoost.ActivateDoubleVillageIncomeCondition = load._JSONActivateDoubleVillageIncomeCondition;

                    MainBoost.DoubleGoldIncomeCountDownTimerValue = load._JSONDoubleGoldIncomeCountDownTimerValue;

                    MainBoost.ActivateOfflineEarningsCondition = load._JSONActivateOfflineEarningsCondition;

                    MainBoost.DoubleVillageIncomeCountdownTimerValue = load._JSONDoubleVillageIncomeCountdownTimerValue;

                    StartCoroutine(DelayLoading());
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

                    JSONBoostDatabaseClass load = JsonUtility.FromJson<JSONBoostDatabaseClass>(JsonString);

                    MainBoost.ActivateDoubleGoldIncomeCondition = load._JSONActivateDoubleGoldIncomeCondition;

                    MainBoost.ActivateDoubleVillageIncomeCondition = load._JSONActivateDoubleVillageIncomeCondition;

                    MainBoost.DoubleGoldIncomeCountDownTimerValue = load._JSONDoubleGoldIncomeCountDownTimerValue;

                    MainBoost.ActivateOfflineEarningsCondition = load._JSONActivateOfflineEarningsCondition;

                    MainBoost.DoubleVillageIncomeCountdownTimerValue = load._JSONDoubleVillageIncomeCountdownTimerValue;


                    StartCoroutine(DelayLoading());
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    SaveFile();
                }
            }
        }

        IEnumerator DelayLoading()
        {
            yield return new WaitForSeconds(.1f);
            MainBoost.LoadFirstCheck();
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

