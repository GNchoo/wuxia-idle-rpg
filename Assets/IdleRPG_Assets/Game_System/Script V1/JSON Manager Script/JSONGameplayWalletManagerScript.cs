using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Wallet.Manager;
using SAMPLETEXT.Data.Class.Wallet;

namespace SAMPLETEXT.Data.Manager.Wallet
{
    public class JSONGameplayWalletManagerScript : MonoBehaviour
    {
        [SerializeField]
        WalletManagerScript MainWalletTab;

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
                FileName = "Wallet_Stats.text";
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

        private JSONWalletDatabaseClass createSaveGameObject()
        {
            JSONWalletDatabaseClass save = new JSONWalletDatabaseClass();

            save._JSONGoldWalletValue = MainWalletTab.GoldWalletValue;
            save._JSONGemWalletValue = MainWalletTab.GemWalletValue;
            save._JSONDPSWalletValue = MainWalletTab.DPSWalletValue;


            return save;
        }

        void JSONSave()
        {
            JSONWalletDatabaseClass save = createSaveGameObject();

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

                    JSONWalletDatabaseClass load = JsonUtility.FromJson<JSONWalletDatabaseClass>(JsonString);

                    MainWalletTab.GoldWalletValue = load._JSONGoldWalletValue;
                    MainWalletTab.GemWalletValue = load._JSONGemWalletValue;
                    MainWalletTab.DPSWalletValue = load._JSONDPSWalletValue;
                  

                    MainWalletTab.WalletValueManualUpdate();
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainWalletTab.GoldWalletValue = 1000;
                    MainWalletTab.WalletValueManualUpdate();
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

                    JSONWalletDatabaseClass load = JsonUtility.FromJson<JSONWalletDatabaseClass>(JsonString);

                    MainWalletTab.GoldWalletValue = load._JSONGoldWalletValue;
                    MainWalletTab.GemWalletValue = load._JSONGemWalletValue;
                    MainWalletTab.DPSWalletValue = load._JSONDPSWalletValue;


                    MainWalletTab.WalletValueManualUpdate();
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainWalletTab.GoldWalletValue = 1000;
                    MainWalletTab.WalletValueManualUpdate();
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

