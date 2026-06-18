using UnityEngine;
using UnityEngine.InputSystem;

public class LootDragCollector : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private LayerMask _lootLayer;

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        if (!Mouse.current.leftButton.isPressed) return;
        if (Camera.main == null) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(worldPos, _lootLayer);
        if (hit != null && hit.TryGetComponent(out DroppedItem droppedItem))
            droppedItem.Collect();
    }

    #endregion
}
