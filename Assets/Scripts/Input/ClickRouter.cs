using System;
using System.Collections.Generic;
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
    private readonly HashSet<object> _fullBlockers = new();
    private readonly List<IPointerBlocker> _spatialBlockers = new();
    private float _mouseDownTime = -1f;
    private Vector2 _mouseDownScreenPos;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleClick();
    }

    #endregion

    #region Public Methods

    public void RewireCamera() => _mainCamera = Camera.main;

    public void ClearAllBlockers()
    {
        _fullBlockers.Clear();
        _spatialBlockers.Clear();
    }

    public void AddFullBlocker(object source) => _fullBlockers.Add(source);

    public void RemoveFullBlocker(object source) => _fullBlockers.Remove(source);

    public void AddSpatialBlocker(IPointerBlocker blocker) => _spatialBlockers.Add(blocker);

    public void RemoveSpatialBlocker(IPointerBlocker blocker) => _spatialBlockers.Remove(blocker);

    #endregion

    #region Private Helpers

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _mouseDownTime = Time.unscaledTime;
            _mouseDownScreenPos = mouse.position.ReadValue();
            return;
        }

        if (!mouse.leftButton.wasReleasedThisFrame)
            return;

        if (Time.unscaledTime - _mouseDownTime >= 1f)
            return;

        if (_fullBlockers.Count > 0)
            return;

        Vector2 screenPos = _mouseDownScreenPos;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        foreach (IPointerBlocker blocker in _spatialBlockers)
        {
            if (blocker.ContainsScreenPoint(screenPos))
                return;
        }

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
