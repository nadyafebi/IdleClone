using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private ResourceData _data;

    [SerializeField]
    private DroppedItem _droppedItemPrefab;

    #endregion

    #region Public Properties

    public ResourceData Data => _data;

    #endregion

    #region Private Fields

    private ResourceNodeHealth _health;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _health = GetComponent<ResourceNodeHealth>();
    }

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("[ResourceNode] Missing ResourceNodeHealth component.");
            enabled = false;
            return;
        }
        if (_droppedItemPrefab == null)
        {
            Debug.LogError("[ResourceNode] No DroppedItem prefab assigned.");
            enabled = false;
            return;
        }
        if (_data == null)
        {
            Debug.LogError("[ResourceNode] No ResourceData assigned.");
            enabled = false;
            return;
        }

        ApplyData();
        _health.OnDropTriggered += HandleDropTriggered;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDropTriggered -= HandleDropTriggered;
    }

    #endregion

    #region Public Methods

    public void SetData(ResourceData data)
    {
        _data = data;
        ApplyData();
    }

    #endregion

    #region Private Methods

    private void ApplyData()
    {
        _health.SetMaxHealth(_data.MaxHealth);

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.sprite = _data.Sprite;

        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            box.size = _data.ColliderSize;
            box.offset = _data.ColliderOffset;
        }
    }

    private void HandleDropTriggered()
    {
        if (_data.DropItem == null)
            return;

        DroppedItem dropped = Instantiate(
            _droppedItemPrefab,
            transform.position,
            Quaternion.identity
        );
        dropped.Initialize(_data.DropItem, _data.DropQuantity);

        if (_data.XpReward > 0)
            GameManager.Instance?.PlayerLevel?.AddXp(_data.XpReward);
    }

    #endregion
}
