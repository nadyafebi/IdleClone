using System.Collections;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _renderer;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
    }

    #endregion

    #region Public Methods

    public void Init(Vector2 spawnPos, bool facingRight)
    {
        transform.position = spawnPos;

        if (_renderer == null)
        {
            Debug.LogError("[SlashEffect] No SpriteRenderer assigned.");
            Destroy(gameObject);
            return;
        }

        // Sprite tip is top-left by default, so flip when facing right.
        _renderer.flipX = facingRight;
        _renderer.sortingOrder = 10;

        StartCoroutine(PlayAnimation(facingRight));
    }

    #endregion

    #region Private Methods

    private IEnumerator PlayAnimation(bool facingRight)
    {
        Vector3 forward = facingRight ? Vector3.right : Vector3.left;
        Vector3 startPos = transform.position;
        // Travel to the full 5-unit AoE range from player centre (spawn is already 0.4f ahead).
        Vector3 endPos = startPos + forward * 4.6f;

        float moveDuration = 0.28f;
        float fadeDuration = 0.1f;
        float elapsed = 0f;
        Color baseColor = _renderer.color;

        // Move forward across the full attack range
        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // Fade out at the far end
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    #endregion
}
