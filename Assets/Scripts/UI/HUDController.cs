using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour, IPointerBlocker
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private InventoryMenu _inventoryMenu;

    [SerializeField]
    private CraftingMenu _craftingMenu;

    #endregion

    #region Private Fields

    private VisualElement _hudPanel;
    private ClickRouter _clickRouter;
    private GameMenu _openMenu;

    private Label _levelLabel;
    private VisualElement _xpBarFill;
    private Label _xpAmountLabel;
    private PlayerLevel _playerLevel;

    private VisualElement _hpBarFill;
    private Label _hpAmountLabel;
    private PlayerHealth _playerHealth;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _hudPanel = _document.rootVisualElement.Q("hud-panel");

        _document.rootVisualElement.Q("btn-items")
            ?.RegisterCallback<ClickEvent>(_ => ToggleMenu(_inventoryMenu));

        _levelLabel = _document.rootVisualElement.Q<Label>("player-level-label");
        _xpBarFill = _document.rootVisualElement.Q("xp-bar-fill");
        _xpAmountLabel = _document.rootVisualElement.Q<Label>("xp-amount");

        _playerLevel = GameManager.Instance?.PlayerLevel;
        if (_playerLevel != null)
        {
            _playerLevel.OnLevelChanged += HandleLevelChanged;
            _playerLevel.OnXpChanged += HandleXpChanged;
            RefreshLevelLabel(_playerLevel.Level);
            RefreshXpBar(_playerLevel.CurrentXp, _playerLevel.XpToNextLevel);
        }
        else
        {
            Debug.LogWarning("[HUDController] No PlayerLevel found on GameManager.");
        }

        _hpBarFill = _document.rootVisualElement.Q("hp-bar-fill");
        _hpAmountLabel = _document.rootVisualElement.Q<Label>("hp-amount");

        _playerHealth = GameManager.Instance?.PlayerHealth;
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += HandleHealthChanged;
            RefreshHpBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogWarning("[HUDController] No PlayerHealth found on GameManager.");
        }
    }

    private void OnDestroy()
    {
        if (_clickRouter != null)
            _clickRouter.RemoveSpatialBlocker(this);

        if (_playerLevel != null)
        {
            _playerLevel.OnLevelChanged -= HandleLevelChanged;
            _playerLevel.OnXpChanged -= HandleXpChanged;
        }

        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= HandleHealthChanged;
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

    public void OpenCraftingUI(System.Collections.Generic.IReadOnlyList<RecipeData> recipes)
    {
        if (_craftingMenu != null)
            _craftingMenu.Open(recipes);
        if (_inventoryMenu != null && !_inventoryMenu.IsVisible)
            _inventoryMenu.Show();
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

    private void HandleHealthChanged(int current, int max) => RefreshHpBar(current, max);

    private void HandleLevelChanged(int newLevel) => RefreshLevelLabel(newLevel);

    private void HandleXpChanged(int currentXp, int xpToNextLevel) =>
        RefreshXpBar(currentXp, xpToNextLevel);

    private void RefreshHpBar(int current, int max)
    {
        if (_hpBarFill != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            _hpBarFill.style.width = Length.Percent(ratio * 100f);
        }

        if (_hpAmountLabel != null)
            _hpAmountLabel.text = $"{current} / {max}";
    }

    private void RefreshLevelLabel(int level)
    {
        if (_levelLabel != null)
            _levelLabel.text = $"Lv. {level}";
    }

    private void RefreshXpBar(int currentXp, int xpToNextLevel)
    {
        if (_xpBarFill != null)
        {
            float ratio = xpToNextLevel > 0 ? (float)currentXp / xpToNextLevel : 0f;
            _xpBarFill.style.width = Length.Percent(ratio * 100f);
        }

        if (_xpAmountLabel != null)
            _xpAmountLabel.text = $"{currentXp} / {xpToNextLevel}";
    }

    #endregion
}
