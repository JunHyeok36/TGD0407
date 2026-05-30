using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TDG0407.Systems.Managers
{

    using Domain.Interfaces;
    using Systems.Data;

    /// <summary>
    /// 유저 저장 데이터를 관리하는 클래스입니다.
    /// </summary>
    public static class UserDataManager
    {
        #region Fields

        private const string FILENAME_SETTINGDATA = "settings.json";
        private const string FILENAME_GAMEDATA = "gamedata.bin";
        private static GameData gameData = new GameData();
        private static SettingData settingData = new SettingData();

        #endregion
        #region Properties

        public static GameData GameData => gameData;
        public static SettingData SettingData => settingData;
        public static string PATH_SETTINGDATA => Path.Combine(Application.persistentDataPath, FILENAME_SETTINGDATA);
        public static string PATH_GAMEDATA => Path.Combine(Application.persistentDataPath, FILENAME_GAMEDATA);

        #endregion
        #region Methods

        public static void Initialize()
        {
            gameData = Load<GameData>();
            settingData = Load<SettingData>();
        }

        public static void ValidateUserData()
        {
            gameData.ValidateData();
        }

        public static void Save()
        {
            gameData.ValidateData();

            BinaryFormatter formatter = new BinaryFormatter();
            using FileStream stream = File.Create(PATH_GAMEDATA);
            formatter.Serialize(stream, gameData);
        }

        public static T Load<T>() where T : IUserData
        {
            if (typeof(T) == typeof(GameData)) return (T)(object)LoadGameData();
            else if (typeof(T) == typeof(SettingData)) return (T)(object)LoadSettingData();
            throw new InvalidOperationException($"Unsupported data type: {typeof(T)}");

            static GameData LoadGameData()
            {
                if (File.Exists(PATH_GAMEDATA) == false)
                    return new GameData();

                try
                {
                    BinaryFormatter formatter = new();
                    using FileStream stream = File.OpenRead(PATH_GAMEDATA);
                    if (formatter.Deserialize(stream) is not GameData loadedGameData)
                        return new GameData();

                    loadedGameData.Initialize();
                    return loadedGameData;
                }
                catch
                {
                    return new GameData();
                }
            }

            static SettingData LoadSettingData()
            {
                if (File.Exists(PATH_SETTINGDATA) == false)
                    return new SettingData();

                try
                {
                    string json = File.ReadAllText(PATH_SETTINGDATA);
                    if (string.IsNullOrWhiteSpace(json))
                        return new SettingData();

                    SettingData loaded = JsonUtility.FromJson<SettingData>(json);
                    return loaded ?? new SettingData();
                }
                catch
                {
                    return new SettingData();
                }
            }
        }

        

        public static int GenerateInstanceId()
        {
            int instanceId = gameData.nextInstanceId;
            gameData.nextInstanceId++;
            return instanceId;
        }

        public static void SyncInstanceId(int instanceId)
        {
            if(gameData.nextInstanceId <= instanceId)
                gameData.nextInstanceId = instanceId + 1;
        }

        public static T LoadInitialData<T>(TextAsset jsonAsset) where T : class
        {
            if(jsonAsset == null || string.IsNullOrWhiteSpace(jsonAsset.text)) return null;
            return JsonUtility.FromJson<T>(jsonAsset.text);
        }

        #endregion
    }
}