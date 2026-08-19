using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.SubHeroUI.Manager;
using SAMPLETEXT.Data.Class.SubHeroes;

namespace SAMPLETEXT.Data.Manager.SubHeroes
{
    public class JSONGameplaySubHeroManagerScript : MonoBehaviour
    {
        [SerializeField]
        SubHeroesManagerScript MainSubHeroes;

        [Header("Load SubHero Settings")]
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
                FileName = "SubHeroes_Stats.text";
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

        private JSONSubHeroesDatabaseClass createSaveGameObject()
        {
            JSONSubHeroesDatabaseClass save = new JSONSubHeroesDatabaseClass();

            foreach (bool SHA in MainSubHeroes.SubHeroActive)
            {
                save._JSONSubHeroActive.Add(SHA);
            }

            foreach (int SHIC in MainSubHeroes.SubHeroItemCount)
            {
                save._JSONSubHeroItemCount.Add(SHIC);
            }

            foreach(int SHAIIDC in MainSubHeroes.SubHeroActiveImageIDCollection)
            {
                save._JSONSubHeroActiveImageIDCollection.Add(SHAIIDC);
            }

            foreach (bool SHAHCC in MainSubHeroes.SubHeroActiveHeroConditionCollection)
            {
                save._JSONSubHeroActiveHeroConditionCollection.Add(SHAHCC);
            }

            foreach(int SHELID in MainSubHeroes.SubHeroEvolveLevelID)
            {
                save._JSONSubHeroEvolveLevelID.Add(SHELID);
            }

            foreach (int SHERD in MainSubHeroes.SubHeroEvolveRequirementsID)
            {
                save._JSONSubHeroEvolveRequirementsID.Add(SHERD);
            }

            foreach (float SHAT in MainSubHeroes.SubHeroActiveAttack)
            {
                save._JSONSubHeroActiveAttack.Add(SHAT);
            }

            foreach (float SHAS in MainSubHeroes.SubHeroAnimatorAttackSpeed)
            {
                save._JSONSubHeroAnimatorAttackSpeed.Add(SHAS);
            }

            foreach (int UEAHID in MainSubHeroes.UnEquipActiveHeroID)
            {
                save._JSONUnEquipActiveHeroID.Add(UEAHID);
            }

            foreach(float SHDBL in MainSubHeroes.SubHeroDPSBaseLevel)
            {
                save._JSONSubHeroDPSBaseLevel.Add(SHDBL);
            }

            foreach(int SHL in MainSubHeroes.SubHeroLevel)
            {
                save._JSONSubHeroLevel.Add(SHL);
            }

            foreach(float SHLPC in MainSubHeroes.SubHeroLevelPurchaseCost)
            {
                save._JSONSubHeroLevelPurchaseCost.Add(SHLPC);
            }

            foreach (float SHTLC in MainSubHeroes.SubHeroTempLevelCount)
            {
                save._JSONSubHeroTempLevelCount.Add(SHTLC);
            }

            foreach (float SHDPSBD in MainSubHeroes.SubHeroDPSBonusDamage)
            {
                save._JSONSubHeroDPSBonusDamage.Add(SHDPSBD);
            }

            foreach (float SHLMT in MainSubHeroes.SubHeroLevelMilestoneTarget)
            {
                save._JSONSubHeroLevelMilestoneTarget.Add(SHLMT);
            }

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
            JSONSubHeroesDatabaseClass save = createSaveGameObject();

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

                    JSONSubHeroesDatabaseClass load = JsonUtility.FromJson<JSONSubHeroesDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONSubHeroActive.Count; i++)
                    {
                        MainSubHeroes.SubHeroActive = load._JSONSubHeroActive;
                    }

                   for (int i = 0; i < load._JSONSubHeroItemCount.Count; i++ )
                    {
                        MainSubHeroes.SubHeroItemCount= load._JSONSubHeroItemCount;
                    }

                   for (int i = 0; i < load._JSONSubHeroActiveImageIDCollection.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveImageIDCollection = load._JSONSubHeroActiveImageIDCollection;
                    }

                   for (int i = 0; i < load._JSONSubHeroActiveHeroConditionCollection.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveHeroConditionCollection = load._JSONSubHeroActiveHeroConditionCollection;
                    }

                   for (int i = 0; i < load._JSONSubHeroEvolveLevelID.Count; i++)
                    {
                        MainSubHeroes.SubHeroEvolveLevelID = load._JSONSubHeroEvolveLevelID;
                    }

                    for (int i = 0; i < load._JSONSubHeroEvolveRequirementsID.Count; i++)
                    {
                        MainSubHeroes.SubHeroEvolveRequirementsID = load._JSONSubHeroEvolveRequirementsID;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveAttack.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveAttack = load._JSONSubHeroActiveAttack;
                    }

                    for (int i = 0; i < load._JSONSubHeroAnimatorAttackSpeed.Count; i++)
                    {
                        MainSubHeroes.SubHeroAnimatorAttackSpeed = load._JSONSubHeroAnimatorAttackSpeed;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveAttack.Count; i++)
                    {
                        MainSubHeroes.UnEquipActiveHeroID = load._JSONUnEquipActiveHeroID;
                    }

                    for (int i = 0; i < load._JSONSubHeroDPSBaseLevel.Count; i++)
                    {
                        MainSubHeroes.SubHeroDPSBaseLevel = load._JSONSubHeroDPSBaseLevel;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevel.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevel = load._JSONSubHeroLevel;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevelPurchaseCost.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevelPurchaseCost = load._JSONSubHeroLevelPurchaseCost;
                    }

                    for (int i = 0; i < load._JSONSubHeroTempLevelCount.Count; i++)
                    {
                        MainSubHeroes.SubHeroTempLevelCount = load._JSONSubHeroTempLevelCount;
                    }

                    for (int i = 0; i < load._JSONSubHeroDPSBonusDamage.Count; i++)
                    {
                        MainSubHeroes.SubHeroDPSBonusDamage = load._JSONSubHeroDPSBonusDamage;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevelMilestoneTarget.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevelMilestoneTarget = load._JSONSubHeroLevelMilestoneTarget;
                    }

                    MainSubHeroes.FirstCheck();
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

                    JSONSubHeroesDatabaseClass load = JsonUtility.FromJson<JSONSubHeroesDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONSubHeroActive.Count; i++)
                    {
                        MainSubHeroes.SubHeroActive = load._JSONSubHeroActive;
                    }

                    for (int i = 0; i < load._JSONSubHeroItemCount.Count; i++)
                    {
                        MainSubHeroes.SubHeroItemCount = load._JSONSubHeroItemCount;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveImageIDCollection.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveImageIDCollection = load._JSONSubHeroActiveImageIDCollection;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveHeroConditionCollection.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveHeroConditionCollection = load._JSONSubHeroActiveHeroConditionCollection;
                    }

                    for (int i = 0; i < load._JSONSubHeroEvolveLevelID.Count; i++)
                    {
                        MainSubHeroes.SubHeroEvolveLevelID = load._JSONSubHeroEvolveLevelID;
                    }

                    for (int i = 0; i < load._JSONSubHeroEvolveRequirementsID.Count; i++)
                    {
                        MainSubHeroes.SubHeroEvolveRequirementsID = load._JSONSubHeroEvolveRequirementsID;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveAttack.Count; i++)
                    {
                        MainSubHeroes.SubHeroActiveAttack = load._JSONSubHeroActiveAttack;
                    }

                    for (int i = 0; i < load._JSONSubHeroActiveAttack.Count; i++)
                    {
                        MainSubHeroes.UnEquipActiveHeroID = load._JSONUnEquipActiveHeroID;
                    }

                    for (int i = 0; i < load._JSONSubHeroAnimatorAttackSpeed.Count; i++)
                    {
                        MainSubHeroes.SubHeroAnimatorAttackSpeed = load._JSONSubHeroAnimatorAttackSpeed;
                    }

                    for (int i = 0; i < load._JSONSubHeroDPSBaseLevel.Count; i++)
                    {
                        MainSubHeroes.SubHeroDPSBaseLevel = load._JSONSubHeroDPSBaseLevel;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevel.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevel = load._JSONSubHeroLevel;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevelPurchaseCost.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevelPurchaseCost = load._JSONSubHeroLevelPurchaseCost;
                    }

                    for (int i = 0; i < load._JSONSubHeroTempLevelCount.Count; i++)
                    {
                        MainSubHeroes.SubHeroTempLevelCount = load._JSONSubHeroTempLevelCount;
                    }

                    for (int i = 0; i < load._JSONSubHeroDPSBonusDamage.Count; i++)
                    {
                        MainSubHeroes.SubHeroDPSBonusDamage = load._JSONSubHeroDPSBonusDamage;
                    }

                    for (int i = 0; i < load._JSONSubHeroLevelMilestoneTarget.Count; i++)
                    {
                        MainSubHeroes.SubHeroLevelMilestoneTarget = load._JSONSubHeroLevelMilestoneTarget;
                    }

                    MainSubHeroes.FirstCheck();
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

