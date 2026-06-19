using UnityEngine;

public enum ResourceNodeType
{
    Mining,
    Woodcutting
}

[CreateAssetMenu(fileName = "NewResourceData", menuName = "IdleClone/Resource Data")]
public class ResourceData : ScriptableObject
{
    #region Serialized Fields

    [Header("Identity")]
    [SerializeField]
    private string _resourceName = "Resource";

    [SerializeField]
    private ResourceNodeType _nodeType = ResourceNodeType.Mining;

    [Header("Visual")]
    [SerializeField]
    private Sprite _sprite;

    [Header("Gathering")]
    [SerializeField]
    [Min(1)]
    [Tooltip("Hits required before a drop is produced. Resets after each drop.")]
    private int _maxHealth = 5;

    [SerializeField]
    [Min(0)]
    [Tooltip("Minimum player level required. 0 or 1 means no restriction.")]
    private int _requiredLevel = 1;

    [Header("Rewards")]
    [SerializeField]
    private ItemData _dropItem;

    [SerializeField]
    [Min(1)]
    private int _dropQuantity = 1;

    [SerializeField]
    [Min(0)]
    private int _xpReward = 5;

    [Header("Collider")]
    [SerializeField]
    private Vector2 _colliderSize = Vector2.one;

    [SerializeField]
    private Vector2 _colliderOffset = Vector2.zero;

    #endregion

    #region Public Properties

    public string ResourceName => _resourceName;
    public ResourceNodeType NodeType => _nodeType;
    public Sprite Sprite => _sprite;
    public int MaxHealth => _maxHealth;
    public int RequiredLevel => _requiredLevel;
    public ItemData DropItem => _dropItem;
    public int DropQuantity => _dropQuantity;
    public int XpReward => _xpReward;
    public Vector2 ColliderSize => _colliderSize;
    public Vector2 ColliderOffset => _colliderOffset;

    #endregion
}
