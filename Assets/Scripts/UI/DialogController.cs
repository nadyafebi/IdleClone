using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogController : MonoBehaviour, IPointerBlocker
{
    #region Public Properties

    // Fires with the 0-based index of the newly visible line each time the player advances.
    public event Action<int> OnLineAdvanced;

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [Tooltip("World-space Y offset above the NPC transform used as the panel anchor.")]
    [SerializeField]
    private float _anchorOffsetY = 1.5f;

    [Tooltip("Seconds of inactivity before the dialog fades out.")]
    [SerializeField]
    private float _dismissDelay = 10f;

    [Tooltip("Duration of the fade-out animation in seconds.")]
    [SerializeField]
    private float _fadeDuration = 0.5f;

    #endregion

    #region Private Fields

    private VisualElement _panel;
    private Label _speakerLabel;
    private Label _lineLabel;
    private Camera _camera;

    private ClickRouter _clickRouter;
    private DialogData _currentData;
    private int _currentLineIndex;
    private Transform _npcTransform;
    private Action _onClosed;
    private Coroutine _dismissCoroutine;
    private bool _isOpen;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        var root = _document.rootVisualElement;
        _panel = root.Q("dialog-panel");
        _speakerLabel = _panel.Q<Label>("speaker-label");
        _lineLabel = _panel.Q<Label>("line-label");

        _panel.style.display = DisplayStyle.None;
        _panel.RegisterCallback<ClickEvent>((_) => Advance());
    }

    private void OnDestroy()
    {
        if (_clickRouter != null)
            _clickRouter.RemoveSpatialBlocker(this);
    }

    private void Update()
    {
        if (!_isOpen || _npcTransform == null)
            return;

        PositionPanel();
    }

    #endregion

    #region Public Methods

    public void SetVisible(bool visible)
    {
        if (!visible) Close();
    }

    // Called by GameManager after each scene load to rewire per-scene dependencies.
    public void OnSceneLoaded()
    {
        Close();

        if (_clickRouter != null)
            _clickRouter.RemoveSpatialBlocker(this);

        _clickRouter = GameManager.Instance.ClickRouter;
        if (_clickRouter == null)
        {
            Debug.LogError("[DialogController] No ClickRouter found in scene.");
            return;
        }
        _clickRouter.AddSpatialBlocker(this);

        _camera = Camera.main;
    }

    public void Open(DialogData data, Transform npcTransform, Action onClosed)
    {
        if (_isOpen)
            Close();

        _currentData = data;
        _currentLineIndex = 0;
        _npcTransform = npcTransform;
        _onClosed = onClosed;
        _isOpen = true;

        PositionPanel();

        _panel.style.display = DisplayStyle.Flex;
        _panel.style.opacity = 1f;

        ShowCurrentLine();
        ResetDismissTimer();
    }

    public void Advance()
    {
        if (!_isOpen)
            return;

        _currentLineIndex++;

        if (_currentLineIndex >= _currentData.Lines.Count)
        {
            Close();
            return;
        }

        _panel.style.opacity = 1f;
        ShowCurrentLine();
        OnLineAdvanced?.Invoke(_currentLineIndex);
        ResetDismissTimer();
    }

    public bool ContainsScreenPoint(Vector2 screenPos)
    {
        if (!_isOpen)
            return false;

        // worldBound is in panel logical pixels (top-left origin).
        Rect wb = _panel.worldBound;
        float dpi = _panel.panel.scaledPixelsPerPoint;

        // Convert to screen space (bottom-left origin, physical pixels).
        float xMin = wb.xMin * dpi;
        float xMax = wb.xMax * dpi;
        float yMin = Screen.height - wb.yMax * dpi;
        float yMax = Screen.height - wb.yMin * dpi;

        return screenPos.x >= xMin
            && screenPos.x <= xMax
            && screenPos.y >= yMin
            && screenPos.y <= yMax;
    }

    public void Close()
    {
        if (!_isOpen)
            return;

        StopDismissCoroutine();

        _isOpen = false;
        _panel.style.display = DisplayStyle.None;

        // Null before invoking so a re-entrant Open() call (e.g. auto-chaining quests)
        // can set its own _onClosed without it being cleared when we return here.
        Action onClosed = _onClosed;
        _onClosed = null;
        onClosed?.Invoke();
    }

    #endregion

    #region Dialog Display

    private void ShowCurrentLine()
    {
        _speakerLabel.text = _npcTransform != null ? _npcTransform.gameObject.name : string.Empty;
        _lineLabel.text = _currentData.Lines[_currentLineIndex];
    }

    private void PositionPanel()
    {
        Vector3 worldAnchor = _npcTransform.position + new Vector3(0f, _anchorOffsetY, 0f);

        // Convert world position to UI Toolkit panel coordinates (handles DPI/scale).
        Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            _panel.panel,
            worldAnchor,
            _camera
        );

        float panelWidth = _panel.resolvedStyle.width;
        float panelHeight = _panel.resolvedStyle.height;

        // Centre horizontally, anchor bottom of panel to the computed point.
        _panel.style.left = panelPos.x - panelWidth * 0.5f;
        _panel.style.top = panelPos.y - panelHeight;
    }

    #endregion

    #region Dismiss Timer

    private void ResetDismissTimer()
    {
        StopDismissCoroutine();
        _dismissCoroutine = StartCoroutine(DismissAfterDelay());
    }

    private void StopDismissCoroutine()
    {
        if (_dismissCoroutine != null)
        {
            StopCoroutine(_dismissCoroutine);
            _dismissCoroutine = null;
        }
    }

    private IEnumerator DismissAfterDelay()
    {
        yield return new WaitForSeconds(_dismissDelay);
        yield return FadeOut();
        Close();
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _panel.style.opacity = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            yield return null;
        }
        _panel.style.opacity = 0f;
    }

    #endregion
}
