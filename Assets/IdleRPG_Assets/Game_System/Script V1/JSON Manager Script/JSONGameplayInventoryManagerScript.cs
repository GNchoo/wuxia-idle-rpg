using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Data.Class.Village;
using SAMPLETEXT.Inventory.Manager;

namespace SAMPLETEXT.Data.Manager.Inventory
{
    public class JSONGameplayInventoryManagerScript : MonoBehaviour
    {
        [SerializeField]
        InventoryManagerScript MainInventory;

        [Header("Load Inventory Settings")]
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
                FileName = "Inventory_Stats.text";
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

        private JSONInventoryDatabaseClass createSaveGameObject()
        {
            JSONInventoryDatabaseClass save = new JSONInventoryDatabaseClass();

            foreach (bool IACL in MainInventory.InventoryActiveCollectionList)
            {
                save._JSONInventoryActiveCollectionList.Add(IACL);
            }

            foreach (int ILCL in MainInventory.InventoryLevelCollectionList)
            {
                save._JSONInventoryLevelCollectionList.Add(ILCL);
            }

            foreach (int IFCL in MainInventory.InventoryFragmentCollectionList)
            {
                save._JSONInventoryFragmentCollectionList.Add(IFCL);
            }

            foreach (bool IEL in MainInventory.ItemEquipList)
            {
                save._JSONItemEquipList.Add(IEL);
            }

            save._JSONWeaponEquipID = MainInventory.WeaponEquipID;
            save._JSONHeadEquipID = MainInventory.HeadEquipID;

            save._JSONChestEquipID = MainInventory.ChestEquipID;
            save._JSONAccessoryEquipID = MainInventory.AccessoryEquipID;
            save._JSONLegsEquipID = MainInventory.LegsEquipID;
            save._JSONShoesEquipID = MainInventory.ShoesEquipID;


            return save;
        }

        void JSONSave()
        {
            JSONInventoryDatabaseClass save = createSaveGameObject();

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

                    JSONInventoryDatabaseClass load = JsonUtility.FromJson<JSONInventoryDatabaseClass>(JsonString);

                   for (int i = 0; i < load._JSONInventoryActiveCollectionList.Count; i++ )
                    {
                        MainInventory.InventoryActiveCollectionList = load._JSONInventoryActiveCollectionList;
                    }

                    for (int i = 0; i < load._JSONInventoryLevelCollectionList.Count; i++)
                    {
                        MainInventory.InventoryLevelCollectionList = load._JSONInventoryLevelCollectionList;
                    }

                    for (int i = 0; i < load._JSONInventoryFragmentCollectionList.Count; i++)
                    {
                        MainInventory.InventoryFragmentCollectionList = load._JSONInventoryFragmentCollectionList;
                    }

                    for (int i = 0; i < load._JSONItemEquipList.Count; i++)
                    {
                        MainInventory.ItemEquipList = load._JSONItemEquipList;
                    }


                    MainInventory.WeaponEquipID = load._JSONWeaponEquipID;

                    MainInventory.HeadEquipID = load._JSONHeadEquipID;

                    MainInventory.ChestEquipID = load._JSONChestEquipID;
                    MainInventory.AccessoryEquipID = load._JSONAccessoryEquipID;
                    MainInventory.LegsEquipID = load._JSONLegsEquipID;
                    MainInventory.ShoesEquipID = load._JSONShoesEquipID;
                    

                    MainInventory.FirstCheck();
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

                    JSONInventoryDatabaseClass load = JsonUtility.FromJson<JSONInventoryDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONInventoryActiveCollectionList.Count; i++)
                    {
                        MainInventory.InventoryActiveCollectionList = load._JSONInventoryActiveCollectionList;
                    }

                    for (int i = 0; i < load._JSONInventoryLevelCollectionList.Count; i++)
                    {
                        MainInventory.InventoryLevelCollectionList = load._JSONInventoryLevelCollectionList;
                    }

                    for (int i = 0; i < load._JSONInventoryFragmentCollectionList.Count; i++)
                    {
                        MainInventory.InventoryFragmentCollectionList = load._JSONInventoryFragmentCollectionList;
                    }

                    for (int i = 0; i < load._JSONItemEquipList.Count; i++)
                    {
                        MainInventory.ItemEquipList = load._JSONItemEquipList;
                    }


                    MainInventory.WeaponEquipID = load._JSONWeaponEquipID;

                    MainInventory.HeadEquipID = load._JSONHeadEquipID;

                    MainInventory.ChestEquipID = load._JSONChestEquipID;
                    MainInventory.AccessoryEquipID = load._JSONAccessoryEquipID;
                    MainInventory.LegsEquipID = load._JSONLegsEquipID;
                    MainInventory.ShoesEquipID = load._JSONShoesEquipID;

                    MainInventory.FirstCheck();
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

