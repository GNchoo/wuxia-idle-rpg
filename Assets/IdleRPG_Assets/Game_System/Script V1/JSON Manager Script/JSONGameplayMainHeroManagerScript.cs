using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Data.Class.MainHero;
using SAMPLETEXT.Gameplay.Manager.MainHero;

namespace SAMPLETEXT.Data.Manager.MainHero
{
    public class JSONGameplayMainHeroManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameplayMainHeroManagerScript MainHero;

        [Header("Load Wallet Settings")]
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
                FileName = "MainHero_Stats.text";
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
            //MainWalletTab = GameObject.FindObjectOfType<WalletManagerScript>();
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

        private JSONMainHeroDatabaseClass createSaveGameObject()
        {
            JSONMainHeroDatabaseClass save = new JSONMainHeroDatabaseClass();

            save._JSONDPSBasePurchaseCostValue = MainHero.DPSBasePurchaseCostValue;
            save._JSONMainHeroDPSMaxDamageValue = MainHero.MainHeroDPSMaxDamageValue;
            save._JSONMainHeroLevel = MainHero.MainHeroLevel;
            save._JSONAttackSpeedValue = MainHero.AttackSpeedValue;


            return save;
        }

        void JSONSave()
        {
            JSONMainHeroDatabaseClass save = createSaveGameObject();

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

                    JSONMainHeroDatabaseClass load = JsonUtility.FromJson<JSONMainHeroDatabaseClass>(JsonString);

                    MainHero.DPSBasePurchaseCostValue = load._JSONDPSBasePurchaseCostValue;
                    MainHero.MainHeroDPSMaxDamageValue = load._JSONMainHeroDPSMaxDamageValue;
                    MainHero.MainHeroLevel = load._JSONMainHeroLevel;
                    MainHero.AttackSpeedValue = load._JSONAttackSpeedValue;
                  

                    MainHero.FirstCheck();
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

                    JSONMainHeroDatabaseClass load = JsonUtility.FromJson<JSONMainHeroDatabaseClass>(JsonString);

                    MainHero.DPSBasePurchaseCostValue = load._JSONDPSBasePurchaseCostValue;
                    MainHero.MainHeroDPSMaxDamageValue = load._JSONMainHeroDPSMaxDamageValue;
                    MainHero.MainHeroLevel = load._JSONMainHeroLevel;
                    MainHero.AttackSpeedValue = load._JSONAttackSpeedValue;


                    MainHero.FirstCheck();
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

