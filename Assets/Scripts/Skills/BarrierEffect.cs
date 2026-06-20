using System.Collections;
using UnityEngine;

public class BarrierEffect : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _renderer;

    [Tooltip("Seconds of damage immunity.")]
    [SerializeField]
    private float _duration = 5f;

    #endregion

    #region Private Fields

    private Transform _playerTransform;
    private PlayerHealth _playerHealth;

    private static readonly Vector3 Offset = new(0f, 0.5f, 0f);

    #endregion

    #region Unity Lifecycle

    private void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.IsInvincible = false;
    }

    #endregion

    #region Public Methods

    public void Init(Transform playerTransform, PlayerHealth playerHealth)
    {
        _playerTransform = playerTransform;
        _playerHealth = playerHealth;
        _playerHealth.IsInvincible = true;

        transform.position = playerTransform.position + Offset;

        StartCoroutine(BarrierRoutine());
    }

    #endregion

    #region Private Methods

    private IEnumerator BarrierRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            if (_playerTransform == null)
                break;

            transform.position = _playerTransform.position + Offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    #endregion
}
