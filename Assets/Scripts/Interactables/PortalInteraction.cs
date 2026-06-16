using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalInteraction : Interactable
{
    #region Serialized Fields

#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _targetScene;
#endif

    [SerializeField]
    private string _targetSceneName;

    [SerializeField]
    private float _fadeDuration = 1f;

    [Tooltip("Max distance from portal for arrival to count (world units).")]
    [SerializeField]
    private float _arrivalThreshold = 0.75f;

    #endregion

    #region Private Fields

    private ClickIndicator _clickIndicator;
    private ScreenFader _screenFader;
    private BoxCollider2D _collider;
    private bool _isTransitioning;

    #endregion

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate() => _targetSceneName = _targetScene != null ? _targetScene.name : "";
#endif

    protected override void Start()
    {
        base.Start();
        if (!enabled)
            return;

        _clickIndicator = FindFirstObjectByType<ClickIndicator>();
        _screenFader = FindFirstObjectByType<ScreenFader>();
        _collider = GetComponent<BoxCollider2D>();

        if (_clickIndicator == null || _screenFader == null)
        {
            Debug.LogError("[PortalInteraction] Missing required scene dependency.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (_isTransitioning)
            return;

        _clickIndicator.ShowPortalCursor(transform);
        StartApproach(transform.position, _arrivalThreshold, BeginTransition);
    }

    #endregion

    #region Protected Methods

    protected override bool IsPlayerInRange()
    {
        if (_collider == null)
            return base.IsPlayerInRange();

        Vector2 playerPos = _playerMovement.transform.position;
        return Vector2.Distance(playerPos, _collider.ClosestPoint(playerPos)) <= _arrivalThreshold;
    }

    #endregion

    #region Private Methods

    private void BeginTransition()
    {
        _isTransitioning = true;
        _screenFader.FadeOut(_fadeDuration, () => SceneManager.LoadScene(_targetSceneName));
    }

    #endregion
}
