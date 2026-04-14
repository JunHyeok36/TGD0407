using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TDG0407.Systems.Data
{
    using Domain.Entities;

    /// <summary>
    /// 유저 저장 데이터를 관리하는 클래스입니다.
    /// </summary>
    public static class UserDataManager
    {
        #region Fields

        private const string SaveFileName = "userdata.bin";
        private static UserData userData = new();

        #endregion
        #region Properties

        public static UserData UserData => userData;
        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        #endregion
        #region Methods

        public static void Initialize()
        {
            userData = Load();
        }

        public static void ValidateUserData()
        {
            userData.ValidateData();
        }

        public static void Save()
        {
            userData.ValidateData();

            BinaryFormatter formatter = new();
            using FileStream stream = File.Create(SaveFilePath);
            formatter.Serialize(stream, userData);
        }

        public static UserData Load()
        {
            if(File.Exists(SaveFilePath) == false)
                return new UserData();

            try
            {
                BinaryFormatter formatter = new();
                using FileStream stream = File.OpenRead(SaveFilePath);
                if (formatter.Deserialize(stream) is not UserData loadedUserData)
                    return new UserData();

                loadedUserData.Initialize();
                return loadedUserData;
            }
            catch
            {
                return new UserData();
            }
        }

        public static int GenerateInstanceId()
        {
            int instanceId = userData.nextInstanceId;
            userData.nextInstanceId++;
            return instanceId;
        }

        public static void SyncInstanceId(int instanceId)
        {
            if(userData.nextInstanceId <= instanceId)
                userData.nextInstanceId = instanceId + 1;
        }

        public static T LoadInitialData<T>(TextAsset jsonAsset) where T : class
        {
            if(jsonAsset == null || string.IsNullOrWhiteSpace(jsonAsset.text)) return null;
            return JsonUtility.FromJson<T>(jsonAsset.text);
        }

        #endregion
    }
}