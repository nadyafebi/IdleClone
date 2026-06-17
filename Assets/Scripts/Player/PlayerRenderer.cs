using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Animator _animator;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private float _baseWalkSpeed;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged += HandleFacingChanged;
        _movement.OnMovementStarted += HandleMovementStarted;
        _movement.OnMovementStopped += HandleMovementStopped;
        _movement.OnWalkSpeedChanged += HandleWalkSpeedChanged;
    }

    private void OnDisable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged -= HandleFacingChanged;
        _movement.OnMovementStarted -= HandleMovementStarted;
        _movement.OnMovementStopped -= HandleMovementStopped;
        _movement.OnWalkSpeedChanged -= HandleWalkSpeedChanged;
    }

    private void Start()
    {
        if (_movement == null)
        {
            Debug.LogError("[PlayerRenderer] No PlayerMovement reference assigned!");
            enabled = false;
            return;
        }
        if (_spriteRenderer == null)
        {
            Debug.LogError("[PlayerRenderer] No SpriteRenderer reference assigned!");
            enabled = false;
            return;
        }
        if (_animator == null)
        {
            Debug.LogError("[PlayerRenderer] No Animator reference assigned!");
            enabled = false;
            return;
        }

        _baseWalkSpeed = _movement.WalkSpeed;
    }

    #endregion

    #region Event Handlers

    private void HandleFacingChanged(bool faceRight)
    {
        _spriteRenderer.flipX = faceRight;
    }

    private void HandleMovementStarted()
    {
        _animator.SetBool(IsWalkingHash, true);
    }

    private void HandleMovementStopped()
    {
        _animator.SetBool(IsWalkingHash, false);
    }

    private void HandleWalkSpeedChanged(float newSpeed)
    {
        _animator.speed = newSpeed / _baseWalkSpeed;
    }

    #endregion
}
