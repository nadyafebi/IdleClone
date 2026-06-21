using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveRegistry", menuName = "IdleClone/Save Registry")]
public class SaveRegistry : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    private ItemData[] _items;

    [SerializeField]
    private QuestData[] _quests;

    [SerializeField]
    private EnemyData[] _enemies;

    #endregion

    #region Private Fields

    private Dictionary<string, ItemData>  _itemLookup;
    private Dictionary<string, QuestData> _questLookup;
    private Dictionary<string, EnemyData> _enemyLookup;

    #endregion

    #region Unity Lifecycle

    // Clear cached dictionaries when the SO is disabled (e.g. between Play-mode sessions in the Editor).
    private void OnDisable()
    {
        _itemLookup  = null;
        _questLookup = null;
        _enemyLookup = null;
    }

    #endregion

    #region Public Methods

    public ItemData  FindItem(string itemName)   => Find(ref _itemLookup,  _items,   itemName);
    public QuestData FindQuest(string questName) => Find(ref _questLookup, _quests,  questName);
    public EnemyData FindEnemy(string enemyName) => Find(ref _enemyLookup, _enemies, enemyName);

    #endregion

    #region Private Helpers

    private T Find<T>(ref Dictionary<string, T> lookup, T[] source, string key)
        where T : Object
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, T>(source.Length);
            foreach (T asset in source)
                if (asset != null)
                    lookup[asset.name] = asset;
        }
        lookup.TryGetValue(key, out T result);
        return result;
    }

    #endregion
}
