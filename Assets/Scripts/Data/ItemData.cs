using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "IdleClone/Item")]
public class ItemData : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private Sprite _icon;

    #endregion

    #region Public Properties

    public string DisplayName => _displayName;
    public Sprite Icon => _icon;

    #endregion
}
