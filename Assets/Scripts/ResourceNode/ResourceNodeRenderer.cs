using System.Collections;
using UnityEngine;

public class ResourceNodeRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private ResourceNodeHealth _health;

    [Header("Hit Feedback")]
    [SerializeField]
    private Color _hitColor = new Color(1f, 0.25f, 0.25f);

    [SerializeField]
    [Tooltip("Seconds the sprite stays tinted after a hit.")]
    private float _hitBlinkDuration = 0.12f;

    #endregion

    #region Private Fields

    private Coroutine _blinkCoroutine;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_health != null)
            _health.OnTookDamage += HandleTookDamage;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnTookDamage -= HandleTookDamage;

        // Coroutines stop on disable but the color change persists — reset it.
        _blinkCoroutine = null;
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.white;
    }

    private void Start()
    {
        if (_spriteRenderer == null)
        {
            Debug.LogError("[ResourceNodeRenderer] No SpriteRenderer reference assigned.");
            enabled = false;
            return;
        }
        if (_health == null)
        {
            Debug.LogError("[ResourceNodeRenderer] No ResourceNodeHealth reference assigned.");
            enabled = false;
        }
    }

    #endregion

    #region Event Handlers

    private void HandleTookDamage()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(HitBlink());
    }

    #endregion

    #region Private Methods

    private IEnumerator HitBlink()
    {
        _spriteRenderer.color = _hitColor;
        yield return new WaitForSeconds(_hitBlinkDuration);
        _spriteRenderer.color = Color.white;
        _blinkCoroutine = null;
    }

    #endregion
}
