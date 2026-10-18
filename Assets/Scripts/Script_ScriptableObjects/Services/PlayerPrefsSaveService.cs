using Export;
using DataStructure;
using UnityEngine;

namespace ScriptableObjects.Services
{
    [CreateAssetMenu(menuName = "Services/Player Prefs Save Service", fileName = "PlayerPrefsSaveService")]
    public class PlayerPrefsSaveService : ScriptableObject
    {
        private const string ShopProgressKey = "ShopProgress";

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

        
        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

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
