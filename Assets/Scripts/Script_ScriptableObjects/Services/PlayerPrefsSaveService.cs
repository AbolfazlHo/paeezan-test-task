using Export;
using DataStructure;
using UnityEngine;

namespace ScriptableObjects.Services
{
    /// <summary>Provides typed PlayerPrefs access and serialization for shop progress.</summary>
    [CreateAssetMenu(menuName = "Services/Player Prefs Save Service", fileName = "PlayerPrefsSaveService")]
    public class PlayerPrefsSaveService : ScriptableObject
    {
        private const string ShopProgressKey = "ShopProgress";

        /// <summary>Loads the shared save-service asset from Resources.</summary>
        public static PlayerPrefsSaveService Main
        {
            get
            {
                if (!_main)
                {
                    _main = Resources.Load<PlayerPrefsSaveService>("PlayerPrefsSaveService");
                }
                return _main;
            }
        }
        private static PlayerPrefsSaveService _main;

        
        /// <summary>Persists an integer preference and refreshes the JSON export.</summary>
        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        /// <summary>Reads an integer preference or returns its default value.</summary>
        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>Persists a floating-point preference and refreshes the JSON export.</summary>
        public void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        /// <summary>Reads a floating-point preference or returns its default value.</summary>
        public float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        /// <summary>Persists a string preference and refreshes the JSON export.</summary>
        public void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        /// <summary>Reads a string preference or returns its default value.</summary>
        public string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        /// <summary>Normalizes and saves shop progress as JSON in PlayerPrefs.</summary>
        public void SaveShopProgress(ShopProgressData progress)
        {
            if (progress == null)
            {
                Debug.LogError("Cannot save shop progress because the data is null.");
                return;
            }

            progress.Normalize();
            SaveString(ShopProgressKey, JsonUtility.ToJson(progress));
        }

        /// <summary>Saves shop progress and currency together after a successful purchase.</summary>
        public void SaveShopPurchase(ShopProgressData progress, int currency)
        {
            if (progress == null)
            {
                Debug.LogError("Cannot save a shop purchase because the progress data is null.");
                return;
            }

            progress.Normalize();
            PlayerPrefs.SetString(ShopProgressKey, JsonUtility.ToJson(progress));
            PlayerPrefs.SetInt("Currency", currency);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        /// <summary>Loads and normalizes saved shop progress, returning defaults if missing or invalid.</summary>
        public ShopProgressData LoadShopProgress()
        {
            var json = LoadString(ShopProgressKey);
            if (string.IsNullOrEmpty(json))
                return new ShopProgressData();

            try
            {
                var progress = JsonUtility.FromJson<ShopProgressData>(json);
                if (progress == null)
                    return new ShopProgressData();

                progress.Normalize();
                return progress;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Could not load shop progress. Using default values. {exception.Message}");
                return new ShopProgressData();
            }
        }
    }
}
