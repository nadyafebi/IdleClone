using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickRouter : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private LayerMask _interactableLayer;

    #endregion

    #region Public Properties

    public event Action<Vector2> OnGroundClicked;

    #endregion

    #region Private Fields

    private Camera _mainCamera;
    private DialogController _dialogController;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _mainCamera = Camera.main;
        _dialogController = FindFirstObjectByType<DialogController>();
    }

    private void Update()
    {
        HandleClick();
    }

    #endregion

    #region Private Helpers

    private bool IsPointerOverUIPanel(Vector2 screenPos)
    {
        return _dialogController != null && _dialogController.ContainsScreenPoint(screenPos);
    }

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        Vector2 screenPos = mouse.position.ReadValue();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (IsPointerOverUIPanel(screenPos))
            return;

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);

        Collider2D interactableHit = Physics2D.OverlapPoint(worldPos, _interactableLayer);
        if (interactableHit != null)
        {
            interactableHit.GetComponent<Interactable>()?.OnInteract();
            return;
        }

        OnGroundClicked?.Invoke(worldPos);
    }

    #endregion
}
