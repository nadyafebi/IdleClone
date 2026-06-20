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

    [SerializeField]
    [Tooltip("Color for the level-requirement popup shown on resource nodes.")]
    private Color _levelRequiredColor = Color.red;

    [SerializeField]
    [Tooltip("Color for heal popups.")]
    private Color _healColor = Color.green;

    #endregion

    #region Public Methods

    public void SetVisible(bool visible) => enabled = visible;

    public void Spawn(Vector2 worldPos, int amount, Color color)
    {
        SpawnText(worldPos, amount.ToString(), color);
    }

    public void SpawnText(Vector2 worldPos, string text, Color color)
    {
        if (_prefab == null)
        {
            Debug.LogError("[DamagePopupSpawner] No DamagePopup prefab assigned.");
            return;
        }

        DamagePopup popup = Instantiate(_prefab, worldPos, Quaternion.identity);
        popup.Initialize(text, color);
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
        if (amount == 0 || GameManager.Instance == null)
            return;

        DamagePopupSpawner spawner = GameManager.Instance.DamagePopupSpawner;
        if (spawner != null)
            spawner.Spawn(worldPos, amount, spawner._enemyDamageColor);
    }

    public static void TrySpawnHeal(Vector2 worldPos, int amount)
    {
        if (GameManager.Instance == null)
            return;

        DamagePopupSpawner spawner = GameManager.Instance.DamagePopupSpawner;
        if (spawner != null)
            spawner.Spawn(worldPos, amount, spawner._healColor);
    }

    public static void TrySpawnLevelRequired(Vector2 worldPos, int level)
    {
        if (GameManager.Instance == null)
            return;

        DamagePopupSpawner spawner = GameManager.Instance.DamagePopupSpawner;
        if (spawner != null)
            spawner.SpawnText(worldPos, $"Lv. {level} required", spawner._levelRequiredColor);
    }

    #endregion
}
