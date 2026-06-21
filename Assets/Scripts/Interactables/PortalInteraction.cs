using TMPro;
using UnityEngine;

public enum PortalLockType
{
    None,
    KillCount,
    Key,
}

public class PortalInteraction : Interactable
{
    #region Serialized Fields

#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _targetScene;
#endif

    [HideInInspector]
    [SerializeField]
    private string _targetSceneName;

    [Tooltip("Max distance from portal for arrival to count (world units).")]
    [SerializeField]
    private float _arrivalThreshold = 0.75f;

    [Header("Lock")]
    [SerializeField]
    private PortalLockType _lockType;

    [Tooltip("Enemy type to track. Required for KillCount lock.")]
    [SerializeField]
    private EnemyData _targetEnemy;

    [SerializeField]
    private int _requiredKills;

    [Tooltip("Icon shown above the portal for a KillCount lock.")]
    [SerializeField]
    private Sprite _lockIcon;

    [Tooltip("Key item consumed on entry. Required for Key lock.")]
    [SerializeField]
    private ItemData _keyItem;

    [Header("Indicator")]
    [SerializeField]
    private Vector2 _indicatorOffset = new(0f, 1.5f);

    [SerializeField]
    private float _indicatorFontSize = 6f;

    [SerializeField]
    private TMP_FontAsset _indicatorFont;

    [SerializeField]
    private float _indicatorIconSize = 0.5f;

    #endregion

    #region Private Fields

    private ClickIndicator _clickIndicator;
    private BoxCollider2D _collider;
    private bool _isTransitioning;

    private GameObject _indicatorRoot;
    private TextMeshPro _indicatorText;

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
        _collider = GetComponent<BoxCollider2D>();

        if (_clickIndicator == null || GameManager.Instance == null)
        {
            Debug.LogError("[PortalInteraction] Missing required scene dependency.");
            enabled = false;
            return;
        }

        if (_lockType == PortalLockType.KillCount)
        {
            if (GameManager.Instance.EnemyProgressTracker == null)
            {
                Debug.LogError("[PortalInteraction] EnemyProgressTracker missing on GameManager.");
                enabled = false;
                return;
            }

            CreateIndicator();
            UpdateKillIndicator();
            GameManager.Instance.EnemyProgressTracker.OnKillCountUpdated += HandleKillCountUpdated;
        }
        else if (_lockType == PortalLockType.Key)
        {
            CreateIndicator();
            _indicatorText.text = "1";
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_lockType == PortalLockType.KillCount && GameManager.Instance != null)
            GameManager.Instance.EnemyProgressTracker.OnKillCountUpdated -= HandleKillCountUpdated;
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (_isTransitioning)
            return;

        if (IsLocked())
        {
            if (_lockType == PortalLockType.Key)
                DamagePopupSpawner.TrySpawnKeyRequired(transform.position);
            Debug.Log($"[PortalInteraction] {gameObject.name} is locked.");
            return;
        }

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

    private bool IsLocked()
    {
        return _lockType switch
        {
            PortalLockType.KillCount => GetRemainingKills() > 0,
            PortalLockType.Key => _keyItem == null
                || !GameManager.Instance.PlayerInventory.Items.TryGetValue(_keyItem, out int count)
                || count < 1,
            _ => false,
        };
    }

    private int GetRemainingKills()
    {
        int killed = GameManager.Instance.EnemyProgressTracker.GetKillCount(_targetEnemy);
        return Mathf.Max(0, _requiredKills - killed);
    }

    private void CreateIndicator()
    {
        _indicatorRoot = new GameObject("LockIndicator");
        _indicatorRoot.transform.SetParent(transform, false);
        _indicatorRoot.transform.localPosition = (Vector3)_indicatorOffset;

        Sprite icon =
            _lockType == PortalLockType.Key
                ? _keyItem != null
                    ? _keyItem.Icon
                    : null
                : _lockIcon;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(_indicatorRoot.transform, false);
        iconObj.transform.localPosition = new(0.25f, 0f, 0f);
        float iconScale = _lockType == PortalLockType.Key ? 0.5f : _indicatorIconSize;
        iconObj.transform.localScale = Vector3.one * iconScale;
        SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = icon;
        iconRenderer.sortingOrder = 20;

        GameObject textObj = new GameObject("Count");
        textObj.transform.SetParent(_indicatorRoot.transform, false);
        textObj.transform.localPosition = new(-0.25f, 0f, 0f);
        _indicatorText = textObj.AddComponent<TextMeshPro>();
        _indicatorText.fontSize = _indicatorFontSize;
        _indicatorText.alignment = TextAlignmentOptions.Center;
        _indicatorText.sortingOrder = 20;
        _indicatorText.rectTransform.sizeDelta = new Vector2(1f, 0.6f);
        if (_indicatorFont != null)
            _indicatorText.font = _indicatorFont;
    }

    private void UpdateKillIndicator()
    {
        if (_indicatorRoot == null)
            return;

        int remaining = GetRemainingKills();
        if (remaining <= 0)
        {
            _indicatorRoot.SetActive(false);
            return;
        }

        _indicatorText.text = remaining.ToString();
    }

    private void HandleKillCountUpdated(EnemyData enemyData)
    {
        if (enemyData != _targetEnemy)
            return;
        UpdateKillIndicator();
    }

    private void BeginTransition()
    {
        if (_lockType == PortalLockType.Key)
            GameManager.Instance.PlayerInventory.RemoveItem(_keyItem, 1);

        _isTransitioning = true;
        GameManager.Instance.TransitionToScene(_targetSceneName);
    }

    #endregion
}
