using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Player")]
    [SerializeField]
    private PlayerStats _playerStats;

    [SerializeField]
    private PlayerLevel _playerLevel;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private PlayerInventory _playerInventory;

    [SerializeField]
    private PlayerEquipment _playerEquipment;

    [SerializeField]
    private PlayerUpgrades _playerUpgrades;

    [SerializeField]
    private PlayerProgression _playerProgression;

    [SerializeField]
    private PlayerSkills _playerSkills;

    [Header("UI")]
    [SerializeField]
    private ScreenFader _screenFader;

    [SerializeField]
    private DialogController _dialogController;

    [SerializeField]
    private HUDController _hudController;

    [SerializeField]
    private DamagePopupSpawner _damagePopupSpawner;

    [SerializeField]
    private ItemPickupNotifier _itemPickupNotifier;

    [SerializeField]
    private ItemTooltip _itemTooltip;

    [Header("Systems")]
    [SerializeField]
    private ClickRouter _clickRouter;

    [SerializeField]
    private QuestManager _questManager;

    [SerializeField]
    private EnemyProgressTracker _enemyProgressTracker;

    [SerializeField]
    private QuestHUD _questHud;

    [Header("Scenes")]
#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _startScene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _startSceneName;

#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _townScene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _townSceneName;

#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _bossArenaScene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _bossArenaSceneName;

#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _field3Scene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _field3SceneName;

    [Header("Transitions")]
    [Tooltip("Fade duration used for all scene transitions.")]
    [SerializeField]
    private float _transitionFadeDuration = 1f;

    [Header("Save System")]
    [SerializeField]
    private SaveRegistry _saveRegistry;

    #endregion

    #region Public Properties

    public static GameManager Instance { get; private set; }
    public HUDController HudController => _hudController;
    public DialogController DialogController => _dialogController;
    public ClickRouter ClickRouter => _clickRouter;
    public PlayerInventory PlayerInventory => _playerInventory;
    public PlayerLevel PlayerLevel => _playerLevel;
    public PlayerHealth PlayerHealth => _playerHealth;
    public DamagePopupSpawner DamagePopupSpawner => _damagePopupSpawner;
    public ItemPickupNotifier ItemPickupNotifier => _itemPickupNotifier;
    public QuestManager QuestManager => _questManager;
    public EnemyProgressTracker EnemyProgressTracker => _enemyProgressTracker;
    public PlayerProgression PlayerProgression => _playerProgression;
    public PlayerEquipment PlayerEquipment => _playerEquipment;
    public PlayerStats PlayerStats => _playerStats;
    public PlayerUpgrades PlayerUpgrades => _playerUpgrades;
    public PlayerSkills PlayerSkills => _playerSkills;
    public ItemTooltip ItemTooltip => _itemTooltip;
    public SaveRegistry SaveRegistry => _saveRegistry;
    public string StartSceneName => _startSceneName;
    public OfflineProgressionResult PendingOfflineResult { get; private set; }

    #endregion

    #region Private Fields

    private string   _previousSceneName;
    private bool     _isTransitioning;
    private bool     _isRespawning;
    private bool     _saveLoaded;
    private SaveData _loadedSaveData;

    #endregion

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate()
    {
        _startSceneName     = _startScene     != null ? _startScene.name     : "";
        _townSceneName      = _townScene      != null ? _townScene.name      : "";
        _bossArenaSceneName = _bossArenaScene != null ? _bossArenaScene.name : "";
        _field3SceneName    = _field3Scene    != null ? _field3Scene.name    : "";
    }
#endif

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_playerHealth != null)
            _playerHealth.OnDied += RespawnPlayer;
    }

    // IEnumerator Start lets us yield one frame so all other Start() calls finish
    // before we restore save data on top of freshly initialized systems.
    private IEnumerator Start()
    {
        SetGameUIVisible(SceneManager.GetActiveScene().name != _startSceneName);
        yield return null;
        LoadGame();
        StartCoroutine(AutoSaveCoroutine());
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    #endregion

    #region Public Methods

    public void TransitionToScene(string sceneName)
    {
        SaveGame();
        _previousSceneName = SceneManager.GetActiveScene().name;
        _isTransitioning = true;
        Time.timeScale = 0f;
        _screenFader.FadeOut(_transitionFadeDuration, () => SceneManager.LoadScene(sceneName));
    }

    public void TransitionToTownScene() => TransitionToScene(_townSceneName);

    public void TransitionToStartScene() => TransitionToScene(_startSceneName);

    public void TransitionToLastSavedScene()
    {
        string sceneName = _loadedSaveData != null
            && !string.IsNullOrEmpty(_loadedSaveData.lastScene)
            && _loadedSaveData.lastScene != _startSceneName
                ? _loadedSaveData.lastScene
                : _townSceneName;
        TransitionToScene(sceneName);
    }

    public void ClearPendingOfflineResult() => PendingOfflineResult = null;

    public void RecomputeOfflineResult()
    {
        SaveData data = SaveSystem.Load();
        if (data == null || _playerStats == null || _playerStats.StatsData == null) return;
        _loadedSaveData = data;
        PendingOfflineResult = OfflineProgressionCalculator.Calculate(
            data,
            _saveRegistry,
            _playerStats.StatsData,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    public void RespawnPlayer()
    {
        _isRespawning = true;
        _isTransitioning = true;
        Time.timeScale = 0f;

        // Dying in the boss arena sends the player back to Field3, not Town
        string target = SceneManager.GetActiveScene().name == _bossArenaSceneName
            ? _field3SceneName
            : _townSceneName;

        _screenFader.FadeOut(_transitionFadeDuration, () => SceneManager.LoadScene(target));
    }

    #endregion

    #region Scene Loading

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isStartScene = scene.name == _startSceneName;
        SetGameUIVisible(!isStartScene);

        if (isStartScene)
        {
            Time.timeScale = 1f;
            _clickRouter.RewireCamera();
            _screenFader.RewireClickRouter(_clickRouter);
            if (_isTransitioning)
            {
                _isTransitioning = false;
                _screenFader.FadeIn(_transitionFadeDuration);
            }
            return;
        }

        if (_isRespawning)
        {
            _isRespawning = false;
            _playerHealth.ResetHealth();
        }

        Time.timeScale = 1f;
        _clickRouter.RewireCamera();
        _screenFader.RewireClickRouter(_clickRouter);
        _dialogController.OnSceneLoaded();
        _hudController.OnSceneLoaded();

        MapManager mapManager = FindFirstObjectByType<MapManager>();
        if (mapManager != null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (
                player != null
                && mapManager.TryGetSpawnPoint(
                    _previousSceneName,
                    out Vector2 spawnPos,
                    out bool facingRight
                )
            )
            {
                player.transform.position = spawnPos;
                player.SetFacing(facingRight);
            }
        }

        if (_isTransitioning)
        {
            _isTransitioning = false;
            _screenFader.FadeIn(_transitionFadeDuration);
        }
    }

    private void SetGameUIVisible(bool visible)
    {
        _hudController.SetVisible(visible);
        _dialogController.SetVisible(visible);
        _damagePopupSpawner.SetVisible(visible);
        _itemPickupNotifier.SetVisible(visible);
        _itemTooltip.SetVisible(visible);
        _questHud?.SetVisible(visible);
    }

    #endregion

    #region Save System

    public void SaveGame()
    {
        if (!_saveLoaded)
            return;
        if (SceneManager.GetActiveScene().name == _startSceneName)
            return;
        _loadedSaveData = BuildSaveData();
        SaveSystem.Save(_loadedSaveData);
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSave();
        ResetPlayerState();
    }

    private void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            _loadedSaveData = data;
            ApplySaveData(data);

            if (_playerStats != null && _playerStats.StatsData != null)
            {
                PendingOfflineResult = OfflineProgressionCalculator.Calculate(
                    data,
                    _saveRegistry,
                    _playerStats.StatsData,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }
        _saveLoaded = true;
    }

    private void OnApplicationQuit() => SaveGame();

    private SaveData BuildSaveData()
    {
        var data = new SaveData
        {
            level             = _playerLevel.Level,
            currentXp         = _playerLevel.CurrentXp,
            currentHealth     = _playerHealth.CurrentHealth,
            playerClass       = (int)_playerProgression.CurrentClass,
            strengthTier      = _playerUpgrades.StrengthTier,
            resilienceTier    = _playerUpgrades.ResilienceTier,
            vitalityTier      = _playerUpgrades.VitalityTier,
            yieldTier         = _playerUpgrades.YieldTier,
            fireballUnlocked  = _playerSkills.FireballUnlocked,
            equippedWeapon    = _playerEquipment.WeaponSlot  != null ? _playerEquipment.WeaponSlot.name  : "",
            equippedShield    = _playerEquipment.ShieldSlot  != null ? _playerEquipment.ShieldSlot.name  : "",
            equippedPotion    = _playerEquipment.PotionSlot  != null ? _playerEquipment.PotionSlot.name  : "",
            equippedPotionQty = _playerEquipment.PotionSlotQuantity,
            lastScene         = SceneManager.GetActiveScene().name,
            saveTimestamp     = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // Capture current combat/gathering target for offline progression
        var combat    = FindFirstObjectByType<PlayerCombat>();
        var gathering = FindFirstObjectByType<PlayerGathering>();
        EnemyData    enemyTarget    = combat?.CurrentTargetData;
        ResourceData resourceTarget = gathering?.CurrentTargetData;

        if (enemyTarget != null)
        {
            data.lastTargetType        = "enemy";
            data.lastTargetName        = enemyTarget.name;
            data.lastTargetDisplayName = enemyTarget.EnemyName;
        }
        else if (resourceTarget != null)
        {
            data.lastTargetType        = "resource";
            data.lastTargetName        = resourceTarget.name;
            data.lastTargetDisplayName = resourceTarget.ResourceName;
        }
        else
        {
            data.lastTargetType        = "none";
            data.lastTargetName        = "";
            data.lastTargetDisplayName = "";
        }

        data.inventory = new List<ItemSaveEntry>();
        foreach (var kvp in _playerInventory.Items)
            data.inventory.Add(new ItemSaveEntry { itemName = kvp.Key.name, quantity = kvp.Value });

        data.quests = new List<QuestSaveEntry>();
        foreach (var kvp in _questManager.States)
            data.quests.Add(new QuestSaveEntry
            {
                questName = kvp.Key.name,
                state     = (int)kvp.Value,
                killCount = _questManager.QuestKillCounts.TryGetValue(kvp.Key, out int kills) ? kills : 0,
            });

        data.enemyKills = new List<EnemyKillEntry>();
        foreach (var kvp in _enemyProgressTracker.KillCounts)
            data.enemyKills.Add(new EnemyKillEntry { enemyName = kvp.Key.name, killCount = kvp.Value });

        // Never save the boss arena as the last scene — always redirect to Field3 so the
        // player resumes there on next load, and offline progression is suppressed.
        if (SceneManager.GetActiveScene().name == _bossArenaSceneName)
        {
            data.lastScene             = _field3SceneName;
            data.lastTargetType        = "none";
            data.lastTargetName        = "";
            data.lastTargetDisplayName = "";
        }

        return data;
    }

    private void ResetPlayerState()
    {
        _playerLevel.LoadLevel(1, 0);
        _playerUpgrades.LoadTiers(0, 0, 0, 0);
        _playerHealth.ResetHealth();
        _playerEquipment.LoadEquipment(null, null, null, 0);
        _playerInventory.LoadItems(new List<ItemSaveEntry>(), _saveRegistry);
        _playerProgression.LoadClass(PlayerClass.Beginner);
        _playerSkills.LoadFireball(false);
        _questManager.LoadQuests(new List<QuestSaveEntry>(), _saveRegistry);
        _enemyProgressTracker.LoadKills(new List<EnemyKillEntry>(), _saveRegistry);
    }

    private void ApplySaveData(SaveData data)
    {
        if (_saveRegistry == null)
        {
            Debug.LogError("[GameManager] SaveRegistry is not assigned — save data cannot be loaded.");
            return;
        }

        // Order matters: level and upgrades must be applied before health so MaxHealth is correct.
        _playerLevel.LoadLevel(data.level, data.currentXp);
        _playerUpgrades.LoadTiers(data.strengthTier, data.resilienceTier, data.vitalityTier, data.yieldTier);
        _playerHealth.LoadHealth(data.currentHealth);
        _playerEquipment.LoadEquipment(
            _saveRegistry.FindItem(data.equippedWeapon),
            _saveRegistry.FindItem(data.equippedShield),
            _saveRegistry.FindItem(data.equippedPotion),
            data.equippedPotionQty
        );
        _playerInventory.LoadItems(data.inventory, _saveRegistry);
        _playerProgression.LoadClass((PlayerClass)data.playerClass);
        _playerSkills.LoadFireball(data.fireballUnlocked);
        _questManager.LoadQuests(data.quests, _saveRegistry);
        _enemyProgressTracker.LoadKills(data.enemyKills, _saveRegistry);
    }

    private IEnumerator AutoSaveCoroutine()
    {
        var wait = new WaitForSeconds(60f);
        while (true)
        {
            yield return wait;
            SaveGame();
        }
    }

    #endregion
}
