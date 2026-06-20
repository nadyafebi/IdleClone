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

    [Header("Respawn")]
#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _townScene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _townSceneName;

    [Header("Transitions")]
    [Tooltip("Fade duration used for all scene transitions.")]
    [SerializeField]
    private float _transitionFadeDuration = 1f;

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
    public ItemTooltip ItemTooltip => _itemTooltip;

    #endregion

    #region Private Fields

    private string _previousSceneName;
    private bool _isTransitioning;
    private bool _isRespawning;

    #endregion

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate() => _townSceneName = _townScene != null ? _townScene.name : "";
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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    #endregion

    #region Public Methods

    public void TransitionToScene(string sceneName)
    {
        _previousSceneName = SceneManager.GetActiveScene().name;
        _isTransitioning = true;
        Time.timeScale = 0f;
        _screenFader.FadeOut(_transitionFadeDuration, () => SceneManager.LoadScene(sceneName));
    }

    public void RespawnPlayer()
    {
        _isRespawning = true;
        _isTransitioning = true;
        Time.timeScale = 0f;
        _screenFader.FadeOut(_transitionFadeDuration, () => SceneManager.LoadScene(_townSceneName));
    }

    #endregion

    #region Scene Loading

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

    #endregion
}
