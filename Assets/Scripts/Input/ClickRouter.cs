using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickRouter : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private LayerMask _enemyLayer;

    [SerializeField]
    private LayerMask _npcLayer;

    #endregion

    #region Public Properties

    public event Action<Vector2> OnGroundClicked;
    public event Action<GameObject> OnEnemyClicked;
    public event Action<GameObject> OnNpcClicked;

    #endregion

    #region Private Fields

    private Camera _mainCamera;

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

    #region Private Helpers

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());

        Collider2D enemyHit = Physics2D.OverlapPoint(worldPos, _enemyLayer);
        if (enemyHit != null)
        {
            OnEnemyClicked?.Invoke(enemyHit.gameObject);
            return;
        }

        Collider2D npcHit = Physics2D.OverlapPoint(worldPos, _npcLayer);
        if (npcHit != null)
        {
            OnNpcClicked?.Invoke(npcHit.gameObject);
            return;
        }

        OnGroundClicked?.Invoke(worldPos);
    }

    #endregion
}
