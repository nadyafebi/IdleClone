using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour, IPointerBlocker
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private InventoryMenu _inventoryMenu;

    #endregion

    #region Private Fields

    private VisualElement _hudPanel;
    private ClickRouter _clickRouter;
    private GameMenu _openMenu;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _hudPanel = _document.rootVisualElement.Q("hud-panel");

        _document.rootVisualElement.Q("btn-items")
            ?.RegisterCallback<ClickEvent>(_ => ToggleMenu(_inventoryMenu));
    }

    private void OnDestroy()
    {
        if (_clickRouter != null)
            _clickRouter.RemoveSpatialBlocker(this);
    }

    #endregion

    #region Public Methods

    public void OnSceneLoaded()
    {
        if (_clickRouter != null)
            _clickRouter.RemoveSpatialBlocker(this);

        _clickRouter = GameManager.Instance.ClickRouter;
        if (_clickRouter == null)
        {
            Debug.LogError("[HUDController] No ClickRouter found in scene.");
            return;
        }

        _clickRouter.AddSpatialBlocker(this);
    }

    public bool ContainsScreenPoint(Vector2 screenPos)
    {
        Rect wb = _hudPanel.worldBound;
        float dpi = _hudPanel.panel.scaledPixelsPerPoint;

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

    #region Private Methods

    private void ToggleMenu(GameMenu menu)
    {
        if (menu == null) return;

        // Close whatever is open if it's a different menu
        if (_openMenu != null && _openMenu != menu)
        {
            _openMenu.Hide();
            _openMenu = null;
        }

        menu.Toggle();
        _openMenu = menu.IsVisible ? menu : null;
    }

    #endregion
}
