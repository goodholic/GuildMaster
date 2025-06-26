using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GuildMaster.Core
{
    public class SaveManager : MonoBehaviour
    {
        // Save Data Structure
        [System.Serializable]
        public class SaveData
        {
            // Meta Data
            public string SaveVersion = "1.0.0";
            public DateTime SaveTime;
            public float PlayTime;
            
            // Guild Data
            public string GuildName;
            public int GuildLevel;
            public int GuildReputation;
            public int MaxAdventurers;
            
            // Resources
            public int Gold;
            public int Wood;
            public int Stone;
            public int ManaStone;
            public int Reputation;
            
            // Buildings
            public List<BuildingSaveData> Buildings = new List<BuildingSaveData>();
            
            // Adventurers
            public List<AdventurerSaveData> Adventurers = new List<AdventurerSaveData>();
            
            // Game Progress
            public int CurrentStoryChapter;
            public List<string> CompletedQuests = new List<string>();
            public Dictionary<string, int> Statistics = new Dictionary<string, int>();
        }

        [System.Serializable]
        public class BuildingSaveData
        {
            public string BuildingType;
            public int Level;
            public int PositionX;
            public int PositionY;
            public bool IsConstructing;
            public float ConstructionTimeRemaining;
        }

        [System.Serializable]
        public class AdventurerSaveData
        {
            public string Name;
            public int Level;
            public string JobClass;
            public float CurrentHealth;
            public float MaxHealth;
            public float Attack;
            public float Defense;
            public float MagicPower;
            public float Speed;
            public float CriticalRate;
            public float Accuracy;
            public int Experience;
        }

        // Save Settings
        private const string SAVE_FILE_NAME = "guildmaster_save.json";
        private const string BACKUP_SAVE_FILE_NAME = "guildmaster_save_backup.json";
        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private string BackupSavePath => Path.Combine(Application.persistentDataPath, BACKUP_SAVE_FILE_NAME);

        // Auto Save
        private float autoSaveInterval = 300f; // 5 minutes
        private float lastAutoSaveTime;
        private bool isAutoSaveEnabled = true;

        // Events
        public event Action OnSaveStarted;
        public event Action OnSaveCompleted;
        public event Action OnLoadStarted;
        public event Action OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;

        void Start()
        {
            lastAutoSaveTime = Time.time;
        }

        void Update()
        {
            // Auto save check
            if (isAutoSaveEnabled && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveGame();
                lastAutoSaveTime = Time.time;
            }
        }

        public void SaveGame()
        {
            StartCoroutine(SaveGameCoroutine());
        }

        private IEnumerator SaveGameCoroutine()
        {
            OnSaveStarted?.Invoke();
            
            try
            {
                SaveData saveData = CreateSaveData();
                
                // Backup existing save
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupSavePath, true);
                }
                
                // Save to JSON
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SavePath, json);
                
                OnSaveCompleted?.Invoke();
                Debug.Log($"Game saved successfully at {DateTime.Now}");
            }
            catch (Exception e)
            {
                OnSaveError?.Invoke($"Failed to save game: {e.Message}");
                Debug.LogError($"Save failed: {e}");
            }
            
            yield return null;
        }

        private SaveData CreateSaveData()
        {
            SaveData saveData = new SaveData();
            saveData.SaveTime = DateTime.Now;
            saveData.PlayTime = Time.time;
            
            // Get managers
            var gameManager = GameManager.Instance;
            if (gameManager == null) return saveData;
            
            // Save Guild Data
            var guildManager = gameManager.GuildManager;
            if (guildManager != null)
            {
                var guildData = guildManager.GetGuildData();
                saveData.GuildName = guildData.GuildName;
                saveData.GuildLevel = guildData.GuildLevel;
                saveData.GuildReputation = guildData.GuildReputation;
                saveData.MaxAdventurers = guildData.MaxAdventurers;
                
                // Save Buildings
                foreach (var building in guildData.Buildings)
                {
                    BuildingSaveData buildingSave = new BuildingSaveData
                    {
                        BuildingType = building.Type.ToString(),
                        Level = building.Level,
                        PositionX = building.Position.x,
                        PositionY = building.Position.y,
                        IsConstructing = building.IsConstructing,
                        ConstructionTimeRemaining = building.ConstructionTimeRemaining
                    };
                    saveData.Buildings.Add(buildingSave);
                }
                
                // Save Adventurers
                foreach (var adventurer in guildData.Adventurers)
                {
                    AdventurerSaveData adventurerSave = new AdventurerSaveData
                    {
                        Name = adventurer.Name,
                        Level = adventurer.Level,
                        JobClass = adventurer.JobClass.ToString(),
                        CurrentHealth = adventurer.CurrentHealth,
                        MaxHealth = adventurer.MaxHealth,
                        Attack = adventurer.Attack,
                        Defense = adventurer.Defense,
                        MagicPower = adventurer.MagicPower,
                        Speed = adventurer.Speed,
                        CriticalRate = adventurer.CriticalRate,
                        Accuracy = adventurer.Accuracy,
                        Experience = 0 // TODO: Add experience system
                    };
                    saveData.Adventurers.Add(adventurerSave);
                }
            }
            
            // Save Resources
            var resourceManager = gameManager.ResourceManager;
            if (resourceManager != null)
            {
                var resources = resourceManager.GetResources();
                saveData.Gold = resources.Gold;
                saveData.Wood = resources.Wood;
                saveData.Stone = resources.Stone;
                saveData.ManaStone = resources.ManaStone;
                saveData.Reputation = resources.Reputation;
            }
            
            return saveData;
        }

        public IEnumerator LoadGame()
        {
            OnLoadStarted?.Invoke();
            
            if (!File.Exists(SavePath))
            {
                Debug.Log("No save file found. Starting new game.");
                OnLoadCompleted?.Invoke();
                yield break;
            }

            yield return StartCoroutine(LoadGameFromFile(SavePath, false));
        }

        private IEnumerator LoadGameFromFile(string filePath, bool isBackup)
        {
            SaveData saveData = null;
            bool loadSuccess = false;
            string errorMessage = "";

            // Try to load the file (no yield in try-catch)
            try
            {
                string json = File.ReadAllText(filePath);
                saveData = JsonUtility.FromJson<SaveData>(json);
                loadSuccess = saveData != null;
            }
            catch (Exception e)
            {
                errorMessage = $"Failed to load game: {e.Message}";
                Debug.LogError($"Load failed: {e}");
            }

            // Handle the result (yield allowed here)
            if (loadSuccess && saveData != null)
            {
                yield return StartCoroutine(ApplySaveData(saveData));
                OnLoadCompleted?.Invoke();
                Debug.Log($"Game loaded successfully. Save from: {saveData.SaveTime}");
            }
            else
            {
                OnLoadError?.Invoke(errorMessage);
                
                // Try to load backup if this wasn't already a backup attempt
                if (!isBackup && File.Exists(BackupSavePath))
                {
                    Debug.Log("Attempting to load backup save...");
                    yield return StartCoroutine(LoadGameFromFile(BackupSavePath, true));
                }
            }
        }

        private IEnumerator LoadBackupSave()
        {
            yield return StartCoroutine(LoadGameFromFile(BackupSavePath, true));
        }

        private IEnumerator ApplySaveData(SaveData saveData)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) yield break;
            
            // Load Resources
            var resourceManager = gameManager.ResourceManager;
            if (resourceManager != null)
            {
                resourceManager.AddGold(saveData.Gold - resourceManager.GetGold());
                resourceManager.AddWood(saveData.Wood - resourceManager.GetWood());
                resourceManager.AddStone(saveData.Stone - resourceManager.GetStone());
                resourceManager.AddManaStone(saveData.ManaStone - resourceManager.GetManaStone());
                resourceManager.AddReputation(saveData.Reputation - resourceManager.GetReputation());
            }
            
            // TODO: Load Guild Data, Buildings, Adventurers
            // This would require additional methods in GuildManager to clear and rebuild from save data
            
            yield return null;
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                
                if (File.Exists(BackupSavePath))
                {
                    File.Delete(BackupSavePath);
                }
                
                Debug.Log("Save files deleted.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save files: {e}");
            }
        }

        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public DateTime GetLastSaveTime()
        {
            if (File.Exists(SavePath))
            {
                return File.GetLastWriteTime(SavePath);
            }
            return DateTime.MinValue;
        }

        public void SetAutoSaveEnabled(bool enabled)
        {
            isAutoSaveEnabled = enabled;
        }

        public void SetAutoSaveInterval(float seconds)
        {
            autoSaveInterval = Mathf.Max(60f, seconds); // Minimum 1 minute
        }
    }
}