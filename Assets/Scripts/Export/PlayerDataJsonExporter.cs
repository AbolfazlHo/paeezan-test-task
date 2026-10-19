using System;
using System.IO;
using DataStructure;
using ScriptableObjects.Services;
using UnityEngine;

namespace Export
{
    /// <summary>Writes selected player preference values to the persistent info.json file.</summary>
    public static class PlayerDataJsonExporter
    {
        private static string filePath => Path.Combine(Application.persistentDataPath, "info.json");
        

        /// <summary>Creates or replaces the exported player data JSON document.</summary>
        public static void SaveJsonFromPlayerPrefs()
        {
            try
            {
                PlayerInfo info = new PlayerInfo
                {
                    rank = PlayerPrefsSaveService.Main.LoadInt("PlayerRank", 4510),
                    name = PlayerPrefsSaveService.Main.LoadString("PlayerName", "Honey Drops"),
                    score = PlayerPrefsSaveService.Main.LoadInt("Score", 6000),
                    isUser = true,
                    currency = PlayerPrefsSaveService.Main.LoadInt("Currency")
                };

                string json = JsonUtility.ToJson(info, true);
                
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    if (directory != null)
                        Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save JSON: {ex}");
            }
        }
    }
}

    
