using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Gameplay.Manager.Enemy;
using SAMPLETEXT.Data.Class.Enemy;

namespace SAMPLETEXT.Data.Manager.Enemy
{
    public class JSONGameplayEnemyManagerScript : MonoBehaviour
    {
        [SerializeField]
        GameplayEnemyManagerScript MainEnemy;

        [Header("Load Gameplay Enemy Settings")]
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
                FileName = "Enemy_Stats.text";
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
            //MainEnemy = GameObject.FindObjectOfType<WalletManagerScript>();
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

        private JSONEnemyDatabaseClass createSaveGameObject()
        {
            JSONEnemyDatabaseClass save = new JSONEnemyDatabaseClass();

            save._JSONWaveMinCountValue = MainEnemy.WaveMinCountValue;
            save._JSONWaveMaxCountValue = MainEnemy.WaveMaxCountValue;
            save._JSONWWavePointsCount = MainEnemy.WavePointsCount;
            save._JSONWWaveTempPointsCount = MainEnemy.TempPointsCount;
            save._JSONEnemyMaxHealthValue = MainEnemy.EnemyMaxHealthValue;
            save._JSONEnemyHealthValue = MainEnemy.EnemyHealthValue;
            save._JSONEnemyHealthMaxTimerCDValue = MainEnemy.EnemyHealthMaxTimerCDValue;
            save._JSONMinGoldCoinEnemyDropValue = MainEnemy.MinGoldCoinEnemyDropValue;
            save._JSONMaxGoldCoinEnemyDropValue = MainEnemy.MaxGoldCoinEnemyDropValue;
            save._JSONEnemyDeathCount = MainEnemy.EnemyDeathCount;
            save._JSONEnemyNameID = MainEnemy.EnemyNameID;
            save._JSONEnemyID = MainEnemy.EnemyID;
            save._JSONEnemySecondEvolve = MainEnemy.EnemySecondEvolve;
            save._JSONEnemyThirdEvolve = MainEnemy.EnemyThirdEvolve;
            save._JSONImageBackgroundIDCount = MainEnemy.ImageBackgroundIDCount;
            save._JSONBossDropChance = MainEnemy.ItemChanceDropMaxValue;

            return save;
        }

        void JSONSave()
        {
            JSONEnemyDatabaseClass save = createSaveGameObject();

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

                    JSONEnemyDatabaseClass load = JsonUtility.FromJson<JSONEnemyDatabaseClass>(JsonString);

                    MainEnemy.WaveMinCountValue = load._JSONWaveMinCountValue;
                    MainEnemy.WaveMaxCountValue = load._JSONWaveMaxCountValue;
                    MainEnemy.WavePointsCount = load._JSONWWavePointsCount;
                    MainEnemy.TempPointsCount = load._JSONWWaveTempPointsCount;
                    MainEnemy.EnemyMaxHealthValue = load._JSONEnemyMaxHealthValue;
                    MainEnemy.EnemyHealthMaxTimerCDValue = load._JSONEnemyHealthMaxTimerCDValue;
                    MainEnemy.MinGoldCoinEnemyDropValue = load._JSONMinGoldCoinEnemyDropValue;
                    MainEnemy.MaxGoldCoinEnemyDropValue = load._JSONMaxGoldCoinEnemyDropValue;
                    MainEnemy.EnemyDeathCount = load._JSONEnemyDeathCount;
                    MainEnemy.EnemyNameID = load._JSONEnemyNameID;
                    MainEnemy.EnemyID = load._JSONEnemyID;
                    MainEnemy.EnemySecondEvolve = load._JSONEnemySecondEvolve;
                    MainEnemy.EnemyThirdEvolve = load._JSONEnemyThirdEvolve;
                    MainEnemy.ImageBackgroundIDCount = load._JSONImageBackgroundIDCount;
                    MainEnemy.ItemChanceDropMaxValue = load._JSONBossDropChance;

                    MainEnemy.FirstCheck();
                    ApplyCurrentHealthFromSave(load._JSONEnemyHealthValue);
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

                    JSONEnemyDatabaseClass load = JsonUtility.FromJson<JSONEnemyDatabaseClass>(JsonString);

                    MainEnemy.WaveMinCountValue = load._JSONWaveMinCountValue;
                    MainEnemy.WaveMaxCountValue = load._JSONWaveMaxCountValue;
                    MainEnemy.WavePointsCount = load._JSONWWavePointsCount;
                    MainEnemy.TempPointsCount = load._JSONWWaveTempPointsCount;
                    MainEnemy.EnemyMaxHealthValue = load._JSONEnemyMaxHealthValue;
                    MainEnemy.EnemyHealthMaxTimerCDValue = load._JSONEnemyHealthMaxTimerCDValue;
                    MainEnemy.MinGoldCoinEnemyDropValue = load._JSONMinGoldCoinEnemyDropValue;
                    MainEnemy.MaxGoldCoinEnemyDropValue = load._JSONMaxGoldCoinEnemyDropValue;
                    MainEnemy.EnemyDeathCount = load._JSONEnemyDeathCount;
                    MainEnemy.EnemyNameID = load._JSONEnemyNameID;
                    MainEnemy.EnemyID = load._JSONEnemyID;
                    MainEnemy.EnemySecondEvolve = load._JSONEnemySecondEvolve;
                    MainEnemy.EnemyThirdEvolve = load._JSONEnemyThirdEvolve;
                    MainEnemy.ImageBackgroundIDCount = load._JSONImageBackgroundIDCount;
                    MainEnemy.ItemChanceDropMaxValue = load._JSONBossDropChance;

                    MainEnemy.FirstCheck();
                    ApplyCurrentHealthFromSave(load._JSONEnemyHealthValue);
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    SaveFile();
                }
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

		private void OnApplicationQuit()
        {
            SaveFile();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false)
                SaveFile();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveFile();
                return;
            }

            // Editor alt-tab / focus changes can fire pause resume and reload saves.
            // Reloading mid-fight used to full-heal enemies (FirstCheck). Skip reload in Editor.
            if (Application.isEditor)
                return;

            LoadFile();
        }

        /// <summary>
        /// FirstCheck always sets HP to max; restore the persisted current HP afterward.
        /// </summary>
        void ApplyCurrentHealthFromSave(float savedHealth)
        {
            if (MainEnemy == null) return;

            // Old saves without this field deserialize as 0 — keep FirstCheck full HP then.
            if (savedHealth <= 0f)
                return;

            MainEnemy.EnemyHealthValue = Mathf.Clamp(savedHealth, 1f, MainEnemy.EnemyMaxHealthValue);
            MainEnemy.EnemyHealthCheckReduction();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

