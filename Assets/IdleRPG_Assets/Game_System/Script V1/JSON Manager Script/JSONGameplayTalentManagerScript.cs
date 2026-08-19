using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Talent.Manager;
using SAMPLETEXT.Data.Class.Talent;
using SAMPLETEXT.Account.Manager.Profile;

namespace SAMPLETEXT.Data.Manager.Talent
{
    public class JSONGameplayTalentManagerScript : MonoBehaviour
    {
        [SerializeField]
        TalentsManagerScript MainTalent;
        [SerializeField]
        ProfileManagerScript MainProfile;
        
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
                FileName = "Talent_Stats.text";
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

        private JSONTalentDatabaseClass createSaveGameObject()
        {
            JSONTalentDatabaseClass save = new JSONTalentDatabaseClass();

            foreach (float TSCV in MainTalent.TalentSkillsCollectionValue)
            {
                save._JSONTalentSkillsCollectionValue.Add(TSCV);
            }

            foreach (float TCV in MainTalent.TalentCollectionValue)
            {
                save._JSONTalentCollectionValue.Add(TCV);
            }

            foreach (float TCRV in MainTalent.TalentCollectionRequireValue)
            {
                save._JSONTalentCollectionRequireValue.Add(TCRV);
            }

            foreach (bool TCUC in MainTalent.TalentCollectionUnlockCondition)
            {
                save._JSONTalentCollectionUnlockCondition.Add(TCUC);
            }

            save._JSONMeteorShowerTimerValue = MainTalent.MeteorShowerTimerValue;
            save._JSONIceSpikeTimerValue = MainTalent.IceSpikeTimerValue;
            save._JSONScaryScreamTimerValue = MainTalent.ScaryScreamTimerValue;
            save._JSONTalentPointValue = MainTalent.TalentPointValue;
            save._JSONTotalAttackValue = MainTalent.TotalAttackValue;
            save._JSONTotalAdditionalGoldValue = MainTalent.TotalAdditionalGoldValue;
            save._JSONTotalAttackSpeed = MainTalent.TotalAttackSpeed;
            save._JSONTotalReduceUpgradeCost = MainTalent.TotalReduceUpgradeCost;
            save._JSONTotalAdditionalDamageFromEnemyHealth = MainTalent.TotalAdditionalDamageFromEnemyHealth;
            save._JSONTotalAdditionalDamageMeteorShowerFromEnemyHealth = MainTalent.TotalAdditionalDamageMeteorShowerFromEnemyHealth;
            save._JSONTotalAdditionalGoldValueIceSpike = MainTalent.TotalAdditionalGoldValueIceSpike;
            save._JSONTotalAdditionalItemDropChance = MainTalent.TotalAdditionalItemDropChance;
            save._JSONTotalAdditionalDamageSubHero = MainTalent.TotalAttackValueSubHero;
            save._JSONTotalAttackSpeedSubHero = MainTalent.TotalAttackSpeedSubHero;
            save._JSONTotalCriticalDamage = MainTalent.TotalCriticalDamage;
            save._JSONTotalCriticalDamageSubHero = MainTalent.TotalCritialDamageSubHeroes;
            save._JSONTotalAdditionalItemDropChance = MainTalent.TotalAdditionalItemDropChance;

            return save;
        }

        void JSONSave()
        {
            JSONTalentDatabaseClass save = createSaveGameObject();

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

                    JSONTalentDatabaseClass load = JsonUtility.FromJson<JSONTalentDatabaseClass>(JsonString);

                   for (int i = 0; i < load._JSONTalentSkillsCollectionValue.Count; i++ )
                    {
                        MainTalent.TalentSkillsCollectionValue = load._JSONTalentSkillsCollectionValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionValue.Count; i++)
                    {
                        MainTalent.TalentCollectionValue = load._JSONTalentCollectionValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionRequireValue.Count; i++)
                    {
                        MainTalent.TalentCollectionRequireValue = load._JSONTalentCollectionRequireValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionUnlockCondition.Count; i++)
                    {
                        MainTalent.TalentCollectionUnlockCondition = load._JSONTalentCollectionUnlockCondition;
                    }

                    MainTalent.MeteorShowerTimerValue = load._JSONMeteorShowerTimerValue;
                    MainTalent.IceSpikeTimerValue = load._JSONIceSpikeTimerValue;
                    MainTalent.ScaryScreamTimerValue = load._JSONScaryScreamTimerValue;
                    MainTalent.TalentPointValue = load._JSONTalentPointValue;
                    MainTalent.TotalAttackValue = load._JSONTotalAttackValue;
                    MainTalent.TotalAdditionalGoldValue = load._JSONTotalAdditionalGoldValue;
                    MainTalent.TotalAttackSpeed = load._JSONTotalAttackSpeed;
                    MainTalent.TotalReduceUpgradeCost = load._JSONTotalReduceUpgradeCost;
                    MainTalent.TotalAdditionalDamageMeteorShowerFromEnemyHealth = load._JSONTotalAdditionalDamageMeteorShowerFromEnemyHealth;
                    MainTalent.TotalAdditionalDamageMeteorShowerFromEnemyHealth = load._JSONTotalAdditionalDamageMeteorShowerFromEnemyHealth;
                    MainTalent.TotalAdditionalGoldValueIceSpike = load._JSONTotalAdditionalGoldValueIceSpike;
                    MainTalent.TotalAdditionalItemDropChance = load._JSONTotalAdditionalItemDropChance;
                    MainTalent.TotalAttackValueSubHero = load._JSONTotalAdditionalDamageSubHero;
                    MainTalent.TotalAttackSpeedSubHero = load._JSONTotalAttackSpeedSubHero;
                    MainTalent.TotalCriticalDamage = load._JSONTotalCriticalDamage;
                    MainTalent.TotalCritialDamageSubHeroes = load._JSONTotalCriticalDamageSubHero;

                    StartCoroutine(DelayLoad());
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainTalent.TalentPointValue = MainProfile.PointsValue;
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

                    JSONTalentDatabaseClass load = JsonUtility.FromJson<JSONTalentDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONTalentSkillsCollectionValue.Count; i++)
                    {
                        MainTalent.TalentSkillsCollectionValue = load._JSONTalentSkillsCollectionValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionValue.Count; i++)
                    {
                        MainTalent.TalentCollectionValue = load._JSONTalentCollectionValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionRequireValue.Count; i++)
                    {
                        MainTalent.TalentCollectionRequireValue = load._JSONTalentCollectionRequireValue;
                    }

                    for (int i = 0; i < load._JSONTalentCollectionUnlockCondition.Count; i++)
                    {
                        MainTalent.TalentCollectionUnlockCondition = load._JSONTalentCollectionUnlockCondition;
                    }

                    MainTalent.MeteorShowerTimerValue = load._JSONMeteorShowerTimerValue;
                    MainTalent.IceSpikeTimerValue = load._JSONIceSpikeTimerValue;
                    MainTalent.ScaryScreamTimerValue = load._JSONScaryScreamTimerValue;
                    MainTalent.TalentPointValue = load._JSONTalentPointValue;
                    MainTalent.TotalAttackValue = load._JSONTotalAttackValue;
                    MainTalent.TotalAdditionalGoldValue = load._JSONTotalAdditionalGoldValue;
                    MainTalent.TotalAttackSpeed = load._JSONTotalAttackSpeed;
                    MainTalent.TotalReduceUpgradeCost = load._JSONTotalReduceUpgradeCost;
                    MainTalent.TotalAdditionalDamageMeteorShowerFromEnemyHealth = load._JSONTotalAdditionalDamageMeteorShowerFromEnemyHealth;
                    MainTalent.TotalAdditionalDamageMeteorShowerFromEnemyHealth = load._JSONTotalAdditionalDamageMeteorShowerFromEnemyHealth;
                    MainTalent.TotalAdditionalGoldValueIceSpike = load._JSONTotalAdditionalGoldValueIceSpike;
                    MainTalent.TotalAdditionalItemDropChance = load._JSONTotalAdditionalItemDropChance;
                    MainTalent.TotalAttackValueSubHero = load._JSONTotalAdditionalDamageSubHero;
                    MainTalent.TotalAttackSpeedSubHero = load._JSONTotalAttackSpeedSubHero;
                    MainTalent.TotalCriticalDamage = load._JSONTotalCriticalDamage;
                    MainTalent.TotalCritialDamageSubHeroes = load._JSONTotalCriticalDamageSubHero;

                    StartCoroutine(DelayLoad());
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainTalent.TalentPointValue = MainProfile.PointsValue;
                    SaveFile();
                }
            }
        }

        IEnumerator DelayLoad()
        {
            yield return new WaitForSeconds(.1f);
            MainTalent.FirstCheck();
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

