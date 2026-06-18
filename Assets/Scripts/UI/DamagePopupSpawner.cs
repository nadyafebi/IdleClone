using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private DamagePopup _prefab;

    [Header("Colors")]
    [SerializeField]
    [Tooltip("Color for damage dealt by the player to enemies.")]
    private Color _playerDamageColor = Color.yellow;

    [SerializeField]
    [Tooltip("Color for damage dealt by enemies to the player.")]
    private Color _enemyDamageColor = Color.red;

    #endregion

    #region Public Methods

    public void Spawn(Vector2 worldPos, int amount, Color color)
    {
        if (_prefab == null)
        {
            Debug.LogError("[DamagePopupSpawner] No DamagePopup prefab assigned.");
            return;
        }

        DamagePopup popup = Instantiate(_prefab, worldPos, Quaternion.identity);
        popup.Initialize(amount, color);
    }

    public static void TrySpawnPlayerDamage(Vector2 worldPos, int amount)
    {
        if (GameManager.Instance == null)
            return;

        DamagePopupSpawner spawner = GameManager.Instance.DamagePopupSpawner;
        if (spawner != null)
            spawner.Spawn(worldPos, amount, spawner._playerDamageColor);
    }

    public static void TrySpawnEnemyDamage(Vector2 worldPos, int amount)
    {
        if (GameManager.Instance == null)
            return;

        DamagePopupSpawner spawner = GameManager.Instance.DamagePopupSpawner;
        if (spawner != null)
            spawner.Spawn(worldPos, amount, spawner._enemyDamageColor);
    }

    #endregion
}
