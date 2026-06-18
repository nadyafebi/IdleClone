using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private EnemyData _data;

    #endregion

    #region Public Properties

    public EnemyData Data => _data;

    #endregion
}
