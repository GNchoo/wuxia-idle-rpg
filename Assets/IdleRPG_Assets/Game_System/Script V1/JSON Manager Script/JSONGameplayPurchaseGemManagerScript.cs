using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.ItemPurchase.Manager.Gem;
using SAMPLETEXT.Data.Class.Purchase.Gem;

namespace SAMPLETEXT.Data.Manager.PurchaseGem
{
    public class JSONGameplayPurchaseGemManagerScript : MonoBehaviour
    {
        [SerializeField]
        DiamondPurchaseManagerScript MainDiamondPurchase;

        [Header("Load Purchase Gem Settings")]
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
                FileName = "PurchaseGem_Stats.text";
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

        private JSONGemPurchaseDatabaseClass createSaveGameObject()
        {
            JSONGemPurchaseDatabaseClass save = new JSONGemPurchaseDatabaseClass();

            save._JSONAdsDisable = MainDiamondPurchase.AdsDisable;
            save._JSONVIPPurchaseValue = MainDiamondPurchase.VIPPurchaseValue;
			save._JSONVIPCountValue = MainDiamondPurchase.VIPCountValue;
            

            return save;
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

		void JSONSave()
        {
            JSONGemPurchaseDatabaseClass save = createSaveGameObject();

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

                    JSONGemPurchaseDatabaseClass load = JsonUtility.FromJson<JSONGemPurchaseDatabaseClass>(JsonString);

                    MainDiamondPurchase.AdsDisable = load._JSONAdsDisable;
                    MainDiamondPurchase.VIPPurchaseValue = load._JSONVIPPurchaseValue;
					MainDiamondPurchase.VIPCountValue = load._JSONVIPCountValue;

                    MainDiamondPurchase.FirshCheck();
                 
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

                    JSONGemPurchaseDatabaseClass load = JsonUtility.FromJson<JSONGemPurchaseDatabaseClass>(JsonString);


                    MainDiamondPurchase.AdsDisable = load._JSONAdsDisable;
                    MainDiamondPurchase.VIPPurchaseValue = load._JSONVIPPurchaseValue;
					MainDiamondPurchase.VIPCountValue = load._JSONVIPCountValue;

					MainDiamondPurchase.FirshCheck();
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

