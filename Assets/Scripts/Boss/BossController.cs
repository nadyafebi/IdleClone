using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Boss Data")]
    [SerializeField]
    private EnemyData _bossData;

    [Header("Exit Portal")]
    [Tooltip("Disabled at start; enabled when boss dies.")]
    [SerializeField]
    private GameObject _exitPortal;

    [Header("Minions")]
    [SerializeField]
    private GameObject _minionPrefab;

    [SerializeField]
    private EnemyData _minionData;

    [Tooltip("One transform per platform where a minion spawns during Defense Phase.")]
    [SerializeField]
    private Transform[] _minionSpawnPoints;

    [Header("VFX Prefabs")]
    [Tooltip("Slash prefab reused for melee attack visual.")]
    [SerializeField]
    private GameObject _slashPrefab;

    [Tooltip("Barrier prefab reused as invulnerability visual (no Init called — purely decorative).")]
    [SerializeField]
    private GameObject _barrierPrefab;

    [Tooltip("Orb prefab (Fireball visuals + BossOrbProjectile component).")]
    [SerializeField]
    private GameObject _orbPrefab;

    [Header("Melee Phase")]
    [SerializeField]
    private float _meleePhaseDuration = 10f;

    [SerializeField]
    private float _meleePulseInterval = 2f;

    [SerializeField]
    private int _meleeDamage = 25;

    [SerializeField]
    private float _meleeRadius = 2f;

    [Header("Orb Barrage Phase")]
    [SerializeField]
    private int _wavesPerBarrage = 3;

    [SerializeField]
    private int _orbsPerWave = 5;

    [SerializeField]
    private float _timeBetweenOrbs = 0.3f;

    [SerializeField]
    private float _timeBetweenWaves = 1.7f;

    [Header("Debug")]
    [SerializeField]
    private bool _drawGizmos;

    #endregion

    #region Public Properties

    public EnemyHealth Health => _health;
    public string BossName => _bossData != null ? _bossData.EnemyName : "Boss";

    #endregion

    #region Private Fields

    private EnemyHealth _health;
    private Enemy _enemy;
    private PlatformGraphBuilder _graphBuilder;
    private PlayerMovement _player;

    private int _minionDeathCount;
    private int _minionsToKill;

    private readonly bool[] _thresholdTriggered = new bool[2];

    // 66% and 33% HP thresholds trigger Defense Phase
    private static readonly float[] Thresholds = { 0.66f, 0.33f };

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _enemy  = GetComponent<Enemy>();

        // Initialize health from boss data immediately so HUD shows correct values on scene load
        if (_bossData != null && _health != null && _enemy != null)
        {
            _enemy.SetData(_bossData);
            _health.ResetHealth();
        }
    }

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("[BossController] EnemyHealth component not found.");
            enabled = false;
            return;
        }

        _graphBuilder = FindFirstObjectByType<PlatformGraphBuilder>();
        if (_graphBuilder == null)
        {
            Debug.LogError("[BossController] No PlatformGraphBuilder found in scene.");
            enabled = false;
            return;
        }

        _health.OnDied += HandleBossDied;

        if (_exitPortal != null)
            _exitPortal.SetActive(false);

        StartCoroutine(BossLoop());
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDied -= HandleBossDied;
    }

    #endregion

    #region Boss Loop

    private IEnumerator BossLoop()
    {
        while (true)
        {
            yield return StartCoroutine(MeleePhase());
            if (CheckAndTriggerDefense())
                yield return StartCoroutine(DefensePhase());

            yield return StartCoroutine(OrbBarragePhase());
            if (CheckAndTriggerDefense())
                yield return StartCoroutine(DefensePhase());
        }
    }

    private IEnumerator MeleePhase()
    {
        float elapsed    = 0f;
        // Pulse fires at the start of the phase, then every interval
        float pulseTimer = _meleePulseInterval;

        while (elapsed < _meleePhaseDuration)
        {
            elapsed    += Time.deltaTime;
            pulseTimer += Time.deltaTime;

            if (pulseTimer >= _meleePulseInterval)
            {
                pulseTimer -= _meleePulseInterval;
                MeleePulse();
            }

            yield return null;
        }
    }

    private void MeleePulse()
    {
        PlayerMovement player = GetPlayer();
        if (player == null) return;

        Vector2 toPlayer   = (Vector2)player.transform.position - (Vector2)transform.position;
        bool    facingRight = toPlayer.x >= 0f;

        if (_slashPrefab != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + (facingRight ? Vector2.right : Vector2.left) * 0.4f;
            GameObject go = Instantiate(_slashPrefab);
            go.GetComponent<SlashEffect>()?.Init(spawnPos, facingRight);
        }

        if (toPlayer.magnitude <= _meleeRadius)
            GameManager.Instance.PlayerHealth.TakeDamage(_meleeDamage);
    }

    private IEnumerator OrbBarragePhase()
    {
        for (int wave = 0; wave < _wavesPerBarrage; wave++)
        {
            for (int i = 0; i < _orbsPerWave; i++)
            {
                FireOrb(i);
                yield return new WaitForSeconds(_timeBetweenOrbs);
            }

            if (wave < _wavesPerBarrage - 1)
                yield return new WaitForSeconds(_timeBetweenWaves);
        }
    }

    private void FireOrb(int orbIndex)
    {
        if (_orbPrefab == null) return;

        PlayerMovement player = GetPlayer();
        if (player == null) return;

        Vector2 toPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        // Spread 5 orbs evenly across ±20 degrees around the player direction
        float   spread    = (orbIndex - (_orbsPerWave - 1) / 2f) * 10f;
        Vector2 direction = RotateVector(toPlayer, spread);

        GameObject go = Instantiate(_orbPrefab, transform.position, Quaternion.identity);
        go.GetComponent<BossOrbProjectile>()?.Init(direction);
    }

    private IEnumerator DefensePhase()
    {
        _health.IsInvulnerable = true;

        // Barrier is a purely visual cue — Init() is never called so it won't affect player
        GameObject barrierVfx = null;
        if (_barrierPrefab != null)
        {
            barrierVfx = Instantiate(_barrierPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            barrierVfx.transform.SetParent(transform, worldPositionStays: true);
        }

        _minionDeathCount = 0;
        _minionsToKill    = 0;

        foreach (Transform sp in _minionSpawnPoints)
        {
            if (SpawnMinion(sp.position))
                _minionsToKill++;
        }

        yield return new WaitUntil(() => _minionDeathCount >= _minionsToKill);

        if (barrierVfx != null)
            Destroy(barrierVfx);

        _health.IsInvulnerable = false;
    }

    #endregion

    #region Private Helpers

    private PlayerMovement GetPlayer()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerMovement>();
        return _player;
    }

    private bool SpawnMinion(Vector2 position)
    {
        if (_minionPrefab == null || _minionData == null) return false;

        GameObject    go       = Instantiate(_minionPrefab, position, Quaternion.identity);
        Enemy         enemy    = go.GetComponent<Enemy>();
        EnemyHealth   health   = go.GetComponent<EnemyHealth>();
        EnemyMovement movement = go.GetComponent<EnemyMovement>();

        if (enemy == null || health == null || movement == null)
        {
            Debug.LogError("[BossController] Minion prefab is missing a required component.");
            Destroy(go);
            return false;
        }

        enemy.SetData(_minionData);
        health.ResetHealth();
        health.OnDied += () =>
        {
            _minionDeathCount++;
            Destroy(go);
        };

        PlatformNode node = _graphBuilder.FindNearestNode(position, groundOnly: true);
        if (node != null)
            movement.Activate(node);

        return true;
    }

    private void HandleBossDied()
    {
        StopAllCoroutines();

        if (_exitPortal != null)
            _exitPortal.SetActive(true);

        // Enemy.HandleDied() is also subscribed to OnDied and fires OnAnyEnemyKilled,
        // so EnemyProgressTracker and Quest 8 are notified automatically.
        Destroy(gameObject);
    }

    private bool CheckAndTriggerDefense()
    {
        float ratio = _health.MaxHealth > 0
            ? (float)_health.CurrentHealth / _health.MaxHealth
            : 0f;

        for (int i = 0; i < Thresholds.Length; i++)
        {
            if (!_thresholdTriggered[i] && ratio <= Thresholds[i])
            {
                _thresholdTriggered[i] = true;
                return true;
            }
        }
        return false;
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    #endregion

    #region Editor Visualisation

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, _meleeRadius);
    }

    #endregion
}
