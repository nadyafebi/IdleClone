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
    private ClickRouter _clickRouter;

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

    public void RewireClickRouter()
    {
        _clickRouter = FindFirstObjectByType<ClickRouter>();
        if (_clickRouter == null)
            Debug.LogWarning(
                "[ScreenFader] No ClickRouter found — input will not be blocked during fade."
            );
    }

    public void FadeOut(float duration, Action onComplete)
    {
        StartCoroutine(FadeOutCoroutine(duration, onComplete));
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeInCoroutine(duration, onComplete));
    }

    #endregion

    #region Private Methods

    private IEnumerator FadeOutCoroutine(float duration, Action onComplete)
    {
        if (_clickRouter != null)
            _clickRouter.AddFullBlocker(this);
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
        // Blocker and overlay stay active — the scene unloads while black.
        // FadeIn on the new scene will clean up.
    }

    private IEnumerator FadeInCoroutine(float duration, Action onComplete)
    {
        if (_clickRouter != null)
            _clickRouter.AddFullBlocker(this);
        _overlay.style.display = DisplayStyle.Flex;
        _overlay.style.opacity = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _overlay.style.opacity = Mathf.Clamp01(1f - elapsed / duration);
            yield return null;
        }

        _overlay.style.opacity = 0f;
        _overlay.style.display = DisplayStyle.None;
        if (_clickRouter != null)
            _clickRouter.RemoveFullBlocker(this);
        onComplete?.Invoke();
    }

    #endregion
}
