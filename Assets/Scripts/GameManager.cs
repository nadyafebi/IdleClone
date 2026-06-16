using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private ScreenFader _screenFader;

    [SerializeField]
    private DialogController _dialogController;

    [SerializeField]
    private ClickRouter _clickRouter;

    [Tooltip("Fade duration used for all scene transitions.")]
    [SerializeField]
    private float _transitionFadeDuration = 1f;

    #endregion

    #region Public Properties

    public static GameManager Instance { get; private set; }
    public DialogController DialogController => _dialogController;
    public ClickRouter ClickRouter => _clickRouter;

    #endregion

    #region Private Fields

    private string _previousSceneName;
    private bool _isTransitioning;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    #endregion

    #region Public Methods

    public void TransitionToScene(string sceneName)
    {
        _previousSceneName = SceneManager.GetActiveScene().name;
        _isTransitioning = true;
        _screenFader.FadeOut(_transitionFadeDuration, () => SceneManager.LoadScene(sceneName));
    }

    #endregion

    #region Scene Loading

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _clickRouter.ClearAllBlockers();
        _clickRouter.RewireCamera();
        _screenFader.RewireClickRouter();
        _dialogController.OnSceneLoaded();

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
