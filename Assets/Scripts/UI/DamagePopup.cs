using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    [Tooltip("World units to float upward over the lifetime.")]
    private float _floatHeight = 1.5f;

    [SerializeField]
    [Tooltip("Maximum random horizontal drift in world units.")]
    private float _horizontalDrift = 0.3f;

    [SerializeField]
    [Tooltip("Total lifetime in seconds.")]
    private float _duration = 0.8f;

    #endregion

    #region Public Methods

    public void Initialize(int amount, Color color)
    {
        if (_text == null)
        {
            Debug.LogError("[DamagePopup] No TMP_Text assigned.");
            Destroy(gameObject);
            return;
        }

        _text.text = amount.ToString();
        _text.color = color;
        StartCoroutine(Animate(color));
    }

    #endregion

    #region Private Methods

    private IEnumerator Animate(Color baseColor)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        float driftX = Random.Range(-_horizontalDrift, _horizontalDrift);

        while (elapsed < _duration)
        {
            float t = elapsed / _duration;
            transform.position = startPos + new Vector3(driftX * t, _floatHeight * t, 0f);
            _text.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    #endregion
}
