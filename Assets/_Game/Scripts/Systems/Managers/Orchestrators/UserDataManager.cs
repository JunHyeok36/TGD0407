using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
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

        private const int USER_DATA_SCHEMA_VERSION = 1;
        private const string FILENAME_USERDATA = "userdata.json";
        private const string FILENAME_SETTINGDATA_LEGACY = "settings.json";
        private const string FILENAME_GAMEDATA_LEGACY = "gamedata.bin";
        private static GameData gameData = new GameData();
        private static SettingData settingData = new SettingData();

        [Serializable]
        private sealed class UserDataEnvelope
        {
            public int schemaVersion = USER_DATA_SCHEMA_VERSION;
            public string appVersion = Application.version;
            public GameData gameData = new GameData();
            public SettingData settingData = new SettingData();
        }

        #endregion
        #region Properties

        public static GameData GameData => gameData;
        public static SettingData SettingData => settingData;
        public static string PATH_USERDATA => Path.Combine(Application.persistentDataPath, FILENAME_USERDATA);
        public static string PATH_SETTINGDATA_LEGACY => Path.Combine(Application.persistentDataPath, FILENAME_SETTINGDATA_LEGACY);
        public static string PATH_GAMEDATA_LEGACY => Path.Combine(Application.persistentDataPath, FILENAME_GAMEDATA_LEGACY);

        #endregion
        #region Methods

        public static void Initialize()
        {
            bool loadedFromLegacy;
            UserDataEnvelope envelope = LoadEnvelope(out loadedFromLegacy);

            gameData = envelope.gameData ?? new GameData();
            settingData = envelope.settingData ?? new SettingData();

            ValidateUserData();

            if (loadedFromLegacy)
            {
                Save();
                CleanupLegacyFiles();
            }
        }

        public static void ValidateUserData()
        {
            try
            {
                gameData.ValidateData();
            }
            catch
            {
                gameData = new GameData();
            }

            settingData.ValidateData();
        }

        public static void Save()
        {
            ValidateUserData();

            UserDataEnvelope envelope = new()
            {
                schemaVersion = USER_DATA_SCHEMA_VERSION,
                appVersion = Application.version,
                gameData = gameData,
                settingData = settingData,
            };

            string json = JsonConvert.SerializeObject(envelope, Formatting.Indented);
            File.WriteAllText(PATH_USERDATA, json);
        }

        public static T Load<T>() where T : IUserData
        {
            bool loadedFromLegacy;
            UserDataEnvelope envelope = LoadEnvelope(out loadedFromLegacy);
            if (loadedFromLegacy)
            {
                gameData = envelope.gameData ?? new GameData();
                settingData = envelope.settingData ?? new SettingData();
                Save();
                CleanupLegacyFiles();
            }

            if (typeof(T) == typeof(GameData)) return (T)(object)(envelope.gameData ?? new GameData());
            else if (typeof(T) == typeof(SettingData)) return (T)(object)(envelope.settingData ?? new SettingData());
            throw new InvalidOperationException($"Unsupported data type: {typeof(T)}");
        }

        private static UserDataEnvelope LoadEnvelope(out bool loadedFromLegacy)
        {
            loadedFromLegacy = false;

            if (TryLoadUnifiedEnvelope(out UserDataEnvelope envelope))
                return envelope;

            loadedFromLegacy = File.Exists(PATH_GAMEDATA_LEGACY) || File.Exists(PATH_SETTINGDATA_LEGACY);
            return new UserDataEnvelope
            {
                schemaVersion = USER_DATA_SCHEMA_VERSION,
                appVersion = Application.version,
                gameData = LoadLegacyGameData(),
                settingData = LoadLegacySettingData(),
            };
        }

        private static bool TryLoadUnifiedEnvelope(out UserDataEnvelope envelope)
        {
            envelope = null;

            if (File.Exists(PATH_USERDATA) == false)
                return false;

            try
            {
                string json = File.ReadAllText(PATH_USERDATA);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                envelope = JsonConvert.DeserializeObject<UserDataEnvelope>(json);
                return envelope != null;
            }
            catch
            {
                return false;
            }
        }

        private static GameData LoadLegacyGameData()
        {
            if (File.Exists(PATH_GAMEDATA_LEGACY) == false)
                return new GameData();

            try
            {
#pragma warning disable SYSLIB0011
                BinaryFormatter formatter = new();
                using FileStream stream = File.OpenRead(PATH_GAMEDATA_LEGACY);
                if (formatter.Deserialize(stream) is not GameData loadedGameData)
                    return new GameData();
#pragma warning restore SYSLIB0011

                loadedGameData.ValidateData();
                return loadedGameData;
            }
            catch
            {
                return new GameData();
            }
        }

        private static SettingData LoadLegacySettingData()
        {
            if (File.Exists(PATH_SETTINGDATA_LEGACY) == false)
                return new SettingData();

            try
            {
                string json = File.ReadAllText(PATH_SETTINGDATA_LEGACY);
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

        private static void CleanupLegacyFiles()
        {
            try
            {
                if (File.Exists(PATH_GAMEDATA_LEGACY))
                    File.Delete(PATH_GAMEDATA_LEGACY);
            }
            catch
            {
            }

            try
            {
                if (File.Exists(PATH_SETTINGDATA_LEGACY))
                    File.Delete(PATH_SETTINGDATA_LEGACY);
            }
            catch
            {
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