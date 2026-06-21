using UnityEngine;

public class StartMenuPlayerDisplay : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private RuntimeAnimatorController _awakenedAnimatorController;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private RuntimeAnimatorController _beginnerAnimatorController;
    private PlayerProgression _playerProgression;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_animator == null)
        {
            Debug.LogError("[StartMenuPlayerDisplay] No Animator reference assigned!");
            enabled = false;
            return;
        }

        _beginnerAnimatorController = _animator.runtimeAnimatorController;

        _playerProgression = GameManager.Instance != null ? GameManager.Instance.PlayerProgression : null;
        if (_playerProgression != null)
        {
            _playerProgression.OnClassChanged += ApplyClass;
            ApplyClass(_playerProgression.CurrentClass);
        }

        _animator.SetBool(IsWalkingHash, true);
    }

    private void OnDisable()
    {
        if (_playerProgression != null)
            _playerProgression.OnClassChanged -= ApplyClass;
    }

    #endregion

    #region Private Methods

    private void ApplyClass(PlayerClass playerClass)
    {
        RuntimeAnimatorController controller = playerClass == PlayerClass.Awakened && _awakenedAnimatorController != null
            ? _awakenedAnimatorController
            : _beginnerAnimatorController;

        if (_animator.runtimeAnimatorController != controller)
            _animator.runtimeAnimatorController = controller;

        _animator.SetBool(IsWalkingHash, true);
    }

    #endregion
}
