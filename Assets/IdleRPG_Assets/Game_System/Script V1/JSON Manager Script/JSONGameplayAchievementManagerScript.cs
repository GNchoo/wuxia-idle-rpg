using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Data.Class.Talent;
using SAMPLETEXT.Achievement.Manager;
using SAMPLETEXT.Data.Class.Wallet;

namespace SAMPLETEXT.Data.Manager.Achievement
{
    public class JSONGameplayAchievementManagerScript : MonoBehaviour
    {
        [SerializeField]
		AchievementManagerScript MainAchievement;
        
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
                FileName = "Achievement_Stats.text";
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

        private JSONAchievementDatabaseClass createSaveGameObject()
        {
			JSONAchievementDatabaseClass save = new JSONAchievementDatabaseClass();

            foreach (bool ACC in MainAchievement.AchievementCompleteCollection)
            {
                save._JSONAchievementCompleteCollection.Add(ACC);
            }

			foreach (float AGPC in MainAchievement.AchievementGemPointsCollection)
			{
				save._JSONAchievementGemPointsCollection.Add(AGPC);
			}

			foreach (float AGPIC in MainAchievement.AchievementGemPointsIncreaseCollection)
			{
				save._JSONAchievementGemPointsIncreaseCollection.Add(AGPIC);
			}

			foreach (float CAPVC in MainAchievement.CurrentAchievementPointsValueCollection)
			{
				save._JSONCurrentAchievementPointsValueCollection.Add(CAPVC);
			}

			foreach (float MAPVC in MainAchievement.MaxAchievementPointsValueCollection)
			{
				save._JSONMaxAchievementPointsValueCollection.Add(MAPVC);
			}

			foreach (float APVIC in MainAchievement.AchievementPointsValueIncreaseCollection)
			{
				save._JSONAchievementPointsValueIncreaseCollection.Add(APVIC);
			}


			return save;
        }

        void JSONSave()
        {
			JSONAchievementDatabaseClass save = createSaveGameObject();

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

					JSONAchievementDatabaseClass load = JsonUtility.FromJson<JSONAchievementDatabaseClass>(JsonString);

					for (int i = 0; i < load._JSONAchievementCompleteCollection.Count; i++)
					{
						MainAchievement.AchievementCompleteCollection = load._JSONAchievementCompleteCollection;
					}

					for (int i = 0; i < load._JSONAchievementGemPointsCollection.Count; i++)
					{
						MainAchievement.AchievementGemPointsCollection = load._JSONAchievementGemPointsCollection;
					}

					for (int i = 0; i < load._JSONAchievementGemPointsIncreaseCollection.Count; i++)
					{
						MainAchievement.AchievementGemPointsIncreaseCollection = load._JSONAchievementGemPointsIncreaseCollection;
					}

					for (int i = 0; i < load._JSONCurrentAchievementPointsValueCollection.Count; i++)
					{
						MainAchievement.CurrentAchievementPointsValueCollection = load._JSONCurrentAchievementPointsValueCollection;
					}

					for (int i = 0; i < load._JSONMaxAchievementPointsValueCollection.Count; i++)
					{
						MainAchievement.MaxAchievementPointsValueCollection = load._JSONMaxAchievementPointsValueCollection;
					}

					for (int i = 0; i < load._JSONAchievementPointsValueIncreaseCollection.Count; i++)
					{
						MainAchievement.AchievementPointsValueIncreaseCollection = load._JSONAchievementPointsValueIncreaseCollection;
					}

					StartCoroutine(DelayLoad());
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    //MainTalent.TalentPointValue = MainProfile.PointsValue;
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

					JSONAchievementDatabaseClass load = JsonUtility.FromJson<JSONAchievementDatabaseClass>(JsonString);

					for (int i = 0; i < load._JSONAchievementCompleteCollection.Count; i++)
					{
						MainAchievement.AchievementCompleteCollection = load._JSONAchievementCompleteCollection;
					}

					for (int i = 0; i < load._JSONAchievementGemPointsCollection.Count; i++)
					{
						MainAchievement.AchievementGemPointsCollection = load._JSONAchievementGemPointsCollection;
					}

					for (int i = 0; i < load._JSONAchievementGemPointsIncreaseCollection.Count; i++)
					{
						MainAchievement.AchievementGemPointsIncreaseCollection = load._JSONAchievementGemPointsIncreaseCollection;
					}

					for (int i = 0; i < load._JSONCurrentAchievementPointsValueCollection.Count; i++)
					{
						MainAchievement.CurrentAchievementPointsValueCollection = load._JSONCurrentAchievementPointsValueCollection;
					}

					for (int i = 0; i < load._JSONMaxAchievementPointsValueCollection.Count; i++)
					{
						MainAchievement.MaxAchievementPointsValueCollection = load._JSONMaxAchievementPointsValueCollection;
					}

					for (int i = 0; i < load._JSONAchievementPointsValueIncreaseCollection.Count; i++)
					{
						MainAchievement.AchievementPointsValueIncreaseCollection = load._JSONAchievementPointsValueIncreaseCollection;
					}

					StartCoroutine(DelayLoad());
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    //MainTalent.TalentPointValue = MainProfile.PointsValue;
                    SaveFile();
                }
            }
        }

        IEnumerator DelayLoad()
        {
            yield return new WaitForSeconds(.1f);
            MainAchievement.FirstCheck();
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

