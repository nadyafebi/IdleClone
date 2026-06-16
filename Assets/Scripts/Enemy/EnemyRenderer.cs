using UnityEngine;

public class EnemyRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private EnemyMovement _movement;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Animator _animator;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged += HandleFacingChanged;
        _movement.OnMovementStarted += HandleMovementStarted;
        _movement.OnMovementStopped += HandleMovementStopped;
    }

    private void OnDisable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged -= HandleFacingChanged;
        _movement.OnMovementStarted -= HandleMovementStarted;
        _movement.OnMovementStopped -= HandleMovementStopped;
    }

    private void Start()
    {
        if (_movement == null)
        {
            Debug.LogError("[EnemyRenderer] No EnemyMovement reference assigned!");
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

    #endregion
}
