using System.Collections;
using UnityEngine;

public class EnemyRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private EnemyMovement _movement;

    [SerializeField]
    private EnemyHealth _health;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Animator _animator;

    [Header("Hit Feedback")]
    [SerializeField]
    private Color _hitColor = new Color(1f, 0.25f, 0.25f);

    [SerializeField]
    [Tooltip("Seconds the sprite stays red after a hit.")]
    private float _hitBlinkDuration = 0.12f;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private Coroutine _blinkCoroutine;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_movement != null)
        {
            _movement.OnFacingChanged += HandleFacingChanged;
            _movement.OnMovementStarted += HandleMovementStarted;
            _movement.OnMovementStopped += HandleMovementStopped;
        }
        if (_health != null)
            _health.OnTookDamage += HandleTookDamage;
    }

    private void OnDisable()
    {
        if (_movement != null)
        {
            _movement.OnFacingChanged -= HandleFacingChanged;
            _movement.OnMovementStarted -= HandleMovementStarted;
            _movement.OnMovementStopped -= HandleMovementStopped;
        }
        if (_health != null)
            _health.OnTookDamage -= HandleTookDamage;

        // Coroutines are stopped by Unity on SetActive(false), but the color
        // change persists — reset it so the respawned enemy doesn't reappear red.
        _blinkCoroutine = null;
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.white;
    }

    private void Start()
    {
        if (_movement == null)
        {
            Debug.LogError("[EnemyRenderer] No EnemyMovement reference assigned!");
            enabled = false;
            return;
        }
        if (_health == null)
        {
            Debug.LogError("[EnemyRenderer] No EnemyHealth reference assigned!");
            enabled = false;
            return;
        }
        if (_spriteRenderer == null)
        {
            Debug.LogError("[EnemyRenderer] No SpriteRenderer reference assigned!");
            enabled = false;
            return;
        }
        if (_animator == null)
        {
            Debug.LogError("[EnemyRenderer] No Animator reference assigned!");
            enabled = false;
            return;
        }
    }

    #endregion

    #region Event Handlers

    private void HandleFacingChanged(bool faceRight)
    {
        _spriteRenderer.flipX = !faceRight;
    }

    private void HandleMovementStarted()
    {
        _animator.SetBool(IsWalkingHash, true);
    }

    private void HandleMovementStopped()
    {
        _animator.SetBool(IsWalkingHash, false);
    }

    private void HandleTookDamage()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(HitBlink());
    }

    #endregion

    #region Hit Feedback

    private IEnumerator HitBlink()
    {
        _spriteRenderer.color = _hitColor;
        yield return new WaitForSeconds(_hitBlinkDuration);
        _spriteRenderer.color = Color.white;
        _blinkCoroutine = null;
    }

    #endregion
}
