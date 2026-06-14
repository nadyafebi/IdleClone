using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged += HandleFacingChanged;
    }

    private void OnDisable()
    {
        if (_movement == null)
            return;
        _movement.OnFacingChanged -= HandleFacingChanged;
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
    }

    #endregion

    #region Event Handlers

    private void HandleFacingChanged(bool faceRight)
    {
        _spriteRenderer.flipX = faceRight;
    }

    #endregion
}
