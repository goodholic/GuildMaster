using UnityEngine;
using GuildMaster.Core;

namespace GuildMaster
{
    /// <summary>
    /// Main entry point for the Guild Master game
    /// This script should be attached to a GameObject in the main scene
    /// </summary>
    public class Main : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private bool skipMainMenu = false;
        [SerializeField] private bool enableDebugMode = false;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject uiManagerPrefab;
        
        void Awake()
        {
            // Ensure only one instance exists
            Main[] instances = FindObjectsOfType<Main>();
            if (instances.Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            // Set target frame rate for mobile
            Application.targetFrameRate = 60;
            
            // Initialize game systems
            InitializeGame();
        }
        
        void InitializeGame()
        {
            // Create GameManager if it doesn't exist
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    GameObject gmObject = new GameObject("GameManager");
                    gmObject.AddComponent<GameManager>();
                }
            }
            
            // Create UIManager if it doesn't exist
            if (UI.UIManager.Instance == null)
            {
                if (uiManagerPrefab != null)
                {
                    Instantiate(uiManagerPrefab);
                }
                else
                {
                    GameObject uiObject = new GameObject("UIManager");
                    uiObject.AddComponent<UI.UIManager>();
                }
            }
            
            // Set initial game state
            if (skipMainMenu)
            {
                GameManager.Instance.CurrentState = GameManager.GameState.Guild;
            }
            else
            {
                GameManager.Instance.CurrentState = GameManager.GameState.MainMenu;
            }
            
            // Enable debug mode if set
            if (enableDebugMode)
            {
                EnableDebugMode();
            }
        }
        
        void EnableDebugMode()
        {
            Debug.Log("Debug Mode Enabled!");
            
            // Add debug resources
            if (GameManager.Instance?.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.AddDebugResources();
            }
            
            // Create debug UI
            GameObject debugUI = new GameObject("DebugUI");
            debugUI.AddComponent<DebugUI>();
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Game paused - save state
                GameManager.Instance?.SaveManager?.SaveGame();
            }
            else
            {
                // Game resumed - calculate idle rewards if needed
                // TODO: Implement idle system when needed
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Lost focus - could implement idle mode here
                // TODO: Implement idle system when needed
            }
            else
            {
                // Gained focus - resume normal operation
                // TODO: Implement idle system when needed
            }
        }
    }
    
    /// <summary>
    /// Debug UI for development
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        private bool showDebugPanel = false;
        private Rect debugRect = new Rect(10, 10, 300, 500);
        
        void Update()
        {
            // Toggle debug panel with F1
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showDebugPanel = !showDebugPanel;
            }
        }
        
        void OnGUI()
        {
            if (!showDebugPanel) return;
            
            debugRect = GUI.Window(0, debugRect, DrawDebugWindow, "Debug Panel");
        }
        
        void DrawDebugWindow(int windowID)
        {
            GUILayout.Label("=== Debug Commands ===");
            
            // Resource cheats
            GUILayout.Label("Resources:");
            if (GUILayout.Button("Add 1000 Gold"))
            {
                GameManager.Instance?.ResourceManager?.AddGold(1000);
            }
            if (GUILayout.Button("Add 500 Wood"))
            {
                GameManager.Instance?.ResourceManager?.AddWood(500);
            }
            if (GUILayout.Button("Add 500 Stone"))
            {
                GameManager.Instance?.ResourceManager?.AddStone(500);
            }
            if (GUILayout.Button("Add 200 Mana Stones"))
            {
                GameManager.Instance?.ResourceManager?.AddManaStone(200);
            }
            
            GUILayout.Space(10);
            
            // Guild cheats
            GUILayout.Label("Guild:");
            if (GUILayout.Button("Add 100 Reputation"))
            {
                GameManager.Instance?.GuildManager?.AddReputation(100);
            }
            if (GUILayout.Button("Recruit Random Adventurer"))
            {
                var randomJob = (JobClass)Random.Range(0, System.Enum.GetValues(typeof(JobClass)).Length);
                var unit = new Unit($"Debug Hero", Random.Range(1, 10), randomJob);
                GameManager.Instance?.GuildManager?.RecruitAdventurer(unit);
            }
            
            GUILayout.Space(10);
            
            // Battle cheats
            GUILayout.Label("Battle:");
            if (GUILayout.Button("Start Test Battle"))
            {
                StartTestBattle();
            }
            if (GUILayout.Button("Win Current Battle"))
            {
                // TODO: Implement instant win
            }
            
            GUILayout.Space(10);
            
            // System
            GUILayout.Label("System:");
            if (GUILayout.Button("Save Game"))
            {
                GameManager.Instance?.SaveManager?.SaveGame();
            }
            if (GUILayout.Button("Load Game"))
            {
                StartCoroutine(GameManager.Instance?.SaveManager?.LoadGame());
            }
            if (GUILayout.Button("Delete Save"))
            {
                GameManager.Instance?.SaveManager?.DeleteSave();
            }
            
            GUILayout.Space(10);
            
            // Game speed
            GUILayout.Label($"Game Speed: {Time.timeScale}x");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("1x")) GameManager.Instance?.SetGameSpeed(1f);
            if (GUILayout.Button("2x")) GameManager.Instance?.SetGameSpeed(2f);
            if (GUILayout.Button("4x")) GameManager.Instance?.SetGameSpeed(4f);
            GUILayout.EndHorizontal();
            
            // Make window draggable
            GUI.DragWindow();
        }
        
        void StartTestBattle()
        {
            var battleManager = GameManager.Instance?.BattleManager;
            var guildManager = GameManager.Instance?.GuildManager;
            
            if (battleManager == null || guildManager == null) return;
            
            // Get player units
            var playerUnits = guildManager.GetAvailableAdventurers();
            
            // Generate AI units
            var aiSquads = Battle.AIGuildGenerator.GenerateAIGuild(
                Battle.AIGuildGenerator.Difficulty.Novice, 
                1
            );
            
            // Convert squads to unit list
            var aiUnits = new System.Collections.Generic.List<Unit>();
            foreach (var squad in aiSquads)
            {
                aiUnits.AddRange(squad.GetAllUnits());
            }
            
            // Start battle
            GameManager.Instance.CurrentState = GameManager.GameState.Battle;
            battleManager.StartBattle(playerUnits, aiUnits);
        }
    }
}