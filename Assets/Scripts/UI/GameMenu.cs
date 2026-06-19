using UnityEngine;
using UnityEngine.UIElements;

public abstract class GameMenu : MonoBehaviour, IPointerBlocker
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Public Properties

    public bool IsVisible { get; private set; }

    #endregion

    #region Protected Properties

    protected VisualElement Root => _document.rootVisualElement;

    // Subclasses override to narrow blocking to their actual panel element
    // rather than the full-screen document root.
    protected virtual VisualElement BlockingElement => Root;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            enabled = false;
            return;
        }

        if (_document == null)
        {
            Debug.LogError($"[{GetType().Name}] Missing UIDocument.");
            enabled = false;
            return;
        }
        Root.style.display = DisplayStyle.None;
    }

    #endregion

    #region Public Methods

    public void Show()
    {
        Root.style.display = DisplayStyle.Flex;
        IsVisible = true;
        GameManager.Instance.ClickRouter.AddSpatialBlocker(this);
        GameManager.Instance.ClickRouter.OnClickedOutside += Hide;
        OnShow();
    }

    public void Hide()
    {
        Root.style.display = DisplayStyle.None;
        IsVisible = false;
        GameManager.Instance.ClickRouter.RemoveSpatialBlocker(this);
        GameManager.Instance.ClickRouter.OnClickedOutside -= Hide;
        OnHide();
    }

    public void Toggle()
    {
        if (IsVisible)
            Hide();
        else
            Show();
    }

    public bool ContainsScreenPoint(Vector2 screenPos)
    {
        if (!IsVisible || BlockingElement.panel == null)
            return false;

        Rect wb = BlockingElement.worldBound;
        float dpi = BlockingElement.panel.scaledPixelsPerPoint;

        float xMin = wb.xMin * dpi;
        float xMax = wb.xMax * dpi;
        float yMin = Screen.height - wb.yMax * dpi;
        float yMax = Screen.height - wb.yMin * dpi;

        return screenPos.x >= xMin
            && screenPos.x <= xMax
            && screenPos.y >= yMin
            && screenPos.y <= yMax;
    }

    #endregion

    #region Protected Methods

    protected virtual void OnShow() { }

    protected virtual void OnHide() { }

    #endregion
}
