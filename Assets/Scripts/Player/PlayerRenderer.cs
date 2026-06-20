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

    [SerializeField]
    private RuntimeAnimatorController _awakenedAnimatorController;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private float _baseWalkSpeed;
    private RuntimeAnimatorController _beginnerAnimatorController;
    private PlayerProgression _playerProgression;

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

        if (_playerProgression != null)
            _playerProgression.OnClassChanged -= ApplyClassAnimator;
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
        _beginnerAnimatorController = _animator.runtimeAnimatorController;

        _playerProgression = GameManager.Instance != null ? GameManager.Instance.PlayerProgression : null;
        if (_playerProgression != null)
        {
            _playerProgression.OnClassChanged += ApplyClassAnimator;
            ApplyClassAnimator(_playerProgression.CurrentClass);
        }
    }

    #endregion

    #region Event Handlers

    private void ApplyClassAnimator(PlayerClass playerClass)
    {
        RuntimeAnimatorController controller = playerClass == PlayerClass.Awakened && _awakenedAnimatorController != null
            ? _awakenedAnimatorController
            : _beginnerAnimatorController;

        if (_animator.runtimeAnimatorController != controller)
            _animator.runtimeAnimatorController = controller;
    }

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
