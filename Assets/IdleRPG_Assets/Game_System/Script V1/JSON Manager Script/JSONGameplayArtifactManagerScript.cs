using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Artifact.Manager;
using SAMPLETEXT.Data.Class.Artifact;

namespace SAMPLETEXT.Data.Manager.Artifact
{
    public class JSONGameplayArtifactManagerScript : MonoBehaviour
    {
        [SerializeField]
        ArtifactManagerScript MainArtifact;

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
                FileName = "Artifact_Stats.text";
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

        private JSONArtifactDatabaseClass createSaveGameObject()
        {
            JSONArtifactDatabaseClass save = new JSONArtifactDatabaseClass();

            foreach (bool AAC in MainArtifact.ArtifactActiveCollection)
            {
                save._JSONArtifactActiveCollection.Add(AAC);
            }

            foreach (float ASVC in MainArtifact.ArtifactSkillValueCollection)
            {
                save._JSONArtifactSkillValueCollection.Add(ASVC);
            }

            foreach (float AECC in MainArtifact.ArtifactEvolveCountCollection)
            {
                save._JSONArtifactEvolveCountCollection.Add(AECC);
            }

            foreach (float AERCC in MainArtifact.ArtifactEvolveRequireCountCollection)
            {
                save._JSONArtifactEvolveRequireCountCollection.Add(AERCC);
            }

            foreach (float AECIC in MainArtifact.ArtifactEvolveCountInputCollection)
            {
                save._JSONArtifactEvolveCountInputCollection.Add(AECIC);
            }
            
            save._JSONBloodySkullTotalValue = MainArtifact.BloodySkullTotalValue;
            save._JSONDeflationTotalValue = MainArtifact.DeflationTotalValue;
            save._JSONGodsHourGlassTotalValue = MainArtifact.GodsHourGlassTotalValue;
            save._JSONGoldenSeedsTotalValue = MainArtifact.GoldenSeedsTotalValue;
            save._JSONHugeOpportunityTotalValue = MainArtifact.HugeOpportunityTotalValue;
            save._JSONInevitableVictoryTotalValue = MainArtifact.InevitableVictoryTotalValue;
            save._JSONHornOfPlentyTotalValue = MainArtifact.HornOfPlentyTotalValue;
            save._JSONTalentedTotalValue = MainArtifact.TalentedTotalValue;
            save._JSONTrippleSwordTotalValue = MainArtifact.TrippleSwordTotalValue;

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
            JSONArtifactDatabaseClass save = createSaveGameObject();

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

                    JSONArtifactDatabaseClass load = JsonUtility.FromJson<JSONArtifactDatabaseClass>(JsonString);

                   for (int i = 0; i < load._JSONArtifactActiveCollection.Count; i++ )
                    {
                        MainArtifact.ArtifactActiveCollection= load._JSONArtifactActiveCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactSkillValueCollection.Count; i++)
                    {
                        MainArtifact.ArtifactSkillValueCollection = load._JSONArtifactSkillValueCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveCountCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveCountCollection = load._JSONArtifactEvolveCountCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveRequireCountCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveRequireCountCollection = load._JSONArtifactEvolveRequireCountCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveCountInputCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveCountInputCollection = load._JSONArtifactEvolveCountInputCollection;
                    }

                    MainArtifact.BloodySkullTotalValue = load._JSONBloodySkullTotalValue;
                    MainArtifact.DeflationTotalValue = load._JSONDeflationTotalValue;
                    MainArtifact.GodsHourGlassTotalValue = load._JSONGodsHourGlassTotalValue;
                    MainArtifact.GoldenSeedsTotalValue = load._JSONGoldenSeedsTotalValue;
                    MainArtifact.HugeOpportunityTotalValue = load._JSONHugeOpportunityTotalValue;
                    MainArtifact.InevitableVictoryTotalValue = load._JSONInevitableVictoryTotalValue;
                    MainArtifact.HornOfPlentyTotalValue = load._JSONHornOfPlentyTotalValue;
                    MainArtifact.TalentedTotalValue = load._JSONTalentedTotalValue;
                    MainArtifact.TrippleSwordTotalValue = load._JSONTrippleSwordTotalValue;

                    MainArtifact.FirstCheck();
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

                    JSONArtifactDatabaseClass load = JsonUtility.FromJson<JSONArtifactDatabaseClass>(JsonString);

                    for (int i = 0; i < load._JSONArtifactActiveCollection.Count; i++)
                    {
                        MainArtifact.ArtifactActiveCollection = load._JSONArtifactActiveCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactSkillValueCollection.Count; i++)
                    {
                        MainArtifact.ArtifactSkillValueCollection = load._JSONArtifactSkillValueCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveCountCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveCountCollection = load._JSONArtifactEvolveCountCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveRequireCountCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveRequireCountCollection = load._JSONArtifactEvolveRequireCountCollection;
                    }

                    for (int i = 0; i < load._JSONArtifactEvolveCountInputCollection.Count; i++)
                    {
                        MainArtifact.ArtifactEvolveCountInputCollection = load._JSONArtifactEvolveCountInputCollection;
                    }

                    MainArtifact.BloodySkullTotalValue = load._JSONBloodySkullTotalValue;
                    MainArtifact.DeflationTotalValue = load._JSONDeflationTotalValue;
                    MainArtifact.GodsHourGlassTotalValue = load._JSONGodsHourGlassTotalValue;
                    MainArtifact.GoldenSeedsTotalValue = load._JSONGoldenSeedsTotalValue;
                    MainArtifact.HugeOpportunityTotalValue = load._JSONHugeOpportunityTotalValue;
                    MainArtifact.InevitableVictoryTotalValue = load._JSONInevitableVictoryTotalValue;
                    MainArtifact.HornOfPlentyTotalValue = load._JSONHornOfPlentyTotalValue;
                    MainArtifact.TalentedTotalValue = load._JSONTalentedTotalValue;
                    MainArtifact.TrippleSwordTotalValue = load._JSONTrippleSwordTotalValue;

                    MainArtifact.FirstCheck();
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

