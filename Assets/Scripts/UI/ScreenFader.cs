using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ScreenFader : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Private Fields

    private VisualElement _overlay;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_document == null)
        {
            Debug.LogError("[ScreenFader] No UIDocument assigned.");
            enabled = false;
            return;
        }

        _overlay = _document.rootVisualElement.Q("fade-overlay");
        if (_overlay == null)
        {
            Debug.LogError("[ScreenFader] 'fade-overlay' element not found in UIDocument.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public void FadeOut(float duration, Action onComplete)
    {
        StartCoroutine(FadeOutCoroutine(duration, onComplete));
    }

    #endregion

    #region Private Methods

    private IEnumerator FadeOutCoroutine(float duration, Action onComplete)
    {
        _overlay.style.display = DisplayStyle.Flex;
        _overlay.style.opacity = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _overlay.style.opacity = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        _overlay.style.opacity = 1f;
        onComplete?.Invoke();
    }

    #endregion
}
