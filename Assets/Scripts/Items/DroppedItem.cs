using System.Collections;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _icon;

    [SerializeField]
    private float _bobAmplitude = 0.08f;

    [SerializeField]
    private float _bobFrequency = 2f;

    [SerializeField]
    private float _autoLootDelay = 10f;

    #endregion

    #region Private Fields

    private Vector3 _basePosition;
    private ItemData _item;
    private int _quantity;
    private PlayerInventory _inventory;
    private bool _collected;

    #endregion

    #region Public Methods

    public void Initialize(ItemData item, int quantity)
    {
        if (_icon == null)
        {
            Debug.LogError("[DroppedItem] Missing SpriteRenderer reference.");
            return;
        }

        _item = item;
        _quantity = quantity;
        _icon.sprite = item.Icon;
        _inventory = GameManager.Instance.PlayerInventory;

        StartCoroutine(SpawnArc());
        StartCoroutine(AutoLootCoroutine());
    }

    public void Collect()
    {
        if (_collected) return;
        _collected = true;
        _inventory?.AddItem(_item, _quantity);
        Destroy(gameObject);
    }

    #endregion

    #region Private Helpers

    private IEnumerator AutoLootCoroutine()
    {
        yield return new WaitForSeconds(_autoLootDelay);
        Collect();
    }

    private IEnumerator SpawnArc()
    {
        Vector3 start = transform.position;
        // Each drop scatters to a slightly different landing spot
        Vector3 end = start + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0f);
        const float arcHeight = 0.7f;
        const float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float x = Mathf.Lerp(start.x, end.x, t);
            // sin(π·t) gives a natural arc that starts and ends at 0
            float y = Mathf.Lerp(start.y, end.y, t) + arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = new Vector3(x, y, start.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        _basePosition = end;
        StartCoroutine(BobCoroutine());
    }

    private IEnumerator BobCoroutine()
    {
        while (true)
        {
            float y = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
            transform.position = _basePosition + Vector3.up * y;
            yield return null;
        }
    }

    #endregion
}
