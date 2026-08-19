using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SAMPLETEXT.Account.Manager.Profile;
using SAMPLETEXT.Data.Class.Account.Profile;

namespace SAMPLETEXT.Data.Manager.Account.Profile
{
    public class JSONGameplayAccountProfileManagerScript : MonoBehaviour
    {
        [SerializeField]
        ProfileManagerScript MainAccountProfile;

        [Header("Load Account Profile Settings")]
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
                FileName = "AccountProfile_Stats.text";
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

        private JSONAccountProfileDatabaseClass createSaveGameObject()
        {
            JSONAccountProfileDatabaseClass save = new JSONAccountProfileDatabaseClass();

            save._JSONPlayerAvatarCount = MainAccountProfile.PlayerAvatarCount;
            save._JSONPlayerNameStringValue = MainAccountProfile.PlayerNameStringValue;
            save._JSONGUIDValue = MainAccountProfile.GUIDValue;
            save._JSONPointsValue = MainAccountProfile.PointsValue;


            return save;
        }

        void JSONSave()
        {
            JSONAccountProfileDatabaseClass save = createSaveGameObject();

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

                    JSONAccountProfileDatabaseClass load = JsonUtility.FromJson<JSONAccountProfileDatabaseClass>(JsonString);

                    MainAccountProfile.PlayerAvatarCount = load._JSONPlayerAvatarCount;
                    MainAccountProfile.PlayerNameStringValue = load._JSONPlayerNameStringValue;
                    MainAccountProfile.GUIDValue = load._JSONGUIDValue;
                    MainAccountProfile.PointsValue = load._JSONPointsValue;


                    StartCoroutine("DelayLoadCheck");
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainAccountProfile.PointsValue = 4;
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

                    JSONAccountProfileDatabaseClass load = JsonUtility.FromJson<JSONAccountProfileDatabaseClass>(JsonString);

                    MainAccountProfile.PlayerAvatarCount = load._JSONPlayerAvatarCount;
                    MainAccountProfile.PlayerNameStringValue = load._JSONPlayerNameStringValue;
                    MainAccountProfile.GUIDValue = load._JSONGUIDValue;
                    MainAccountProfile.PointsValue = load._JSONPointsValue;

                    StartCoroutine("DelayLoadCheck");
                   
                }

                else
                {
                    Debug.Log("No Data, Creating....");
                    MainAccountProfile.PointsValue = 4;
                    SaveFile();
                }
            }
        }

        IEnumerator DelayLoadCheck()
        {
            yield return new WaitForSeconds(.1f);
            MainAccountProfile.FirstCheck();
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

