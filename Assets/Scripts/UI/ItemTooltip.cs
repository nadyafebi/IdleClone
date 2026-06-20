using UnityEngine;
using UnityEngine.UIElements;

public class ItemTooltip : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Private Fields

    private VisualElement _panel;
    private Label _nameLabel;
    private Label _categoryLabel;
    private Label _effectLabel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_document == null)
        {
            Debug.LogError("[ItemTooltip] Missing UIDocument.");
            enabled = false;
            return;
        }

        // Tooltip must not block pointer events on menus beneath it.
        _document.rootVisualElement.pickingMode = PickingMode.Ignore;
        _document.sortingOrder = 100;

        _panel = _document.rootVisualElement.Q("tooltip-panel");
        _nameLabel = _panel.Q<Label>("tooltip-name");
        _categoryLabel = _panel.Q<Label>("tooltip-category");
        _effectLabel = _panel.Q<Label>("tooltip-effect");

        _panel.style.display = DisplayStyle.None;
    }

    #endregion

    #region Public Methods

    public void RegisterHover(VisualElement element, ItemData item)
    {
        element.RegisterCallback<PointerEnterEvent>(evt => Show(item, evt.position));
        element.RegisterCallback<PointerMoveEvent>(evt => UpdatePosition(evt.position));
        element.RegisterCallback<PointerLeaveEvent>(_ => Hide());
    }

    public void SetVisible(bool visible)
    {
        if (!visible) Hide();
    }

    public void Hide()
    {
        if (_panel == null) return;
        _panel.style.display = DisplayStyle.None;
    }

    #endregion

    #region Private Helpers

    private void Show(ItemData item, Vector2 panelPos)
    {
        _nameLabel.text = item.DisplayName;
        _categoryLabel.text = item.Category.ToString();

        string effect = GetEffectText(item);
        _effectLabel.text = effect;
        _effectLabel.style.display = string.IsNullOrEmpty(effect)
            ? DisplayStyle.None
            : DisplayStyle.Flex;

        UpdatePosition(panelPos);
        _panel.style.display = DisplayStyle.Flex;
    }

    private void UpdatePosition(Vector2 panelPos)
    {
        _panel.style.left = panelPos.x + 14f;
        _panel.style.top = panelPos.y + 14f;
    }

    private static string GetEffectText(ItemData item) => item.Category switch
    {
        ItemCategory.Weapon => $"+{item.AttackBonus} Attack",
        ItemCategory.Shield => $"+{item.DefenseBonus} Defense",
        ItemCategory.Potion => $"Heals {item.HealAmount} HP",
        _ => string.Empty,
    };

    #endregion
}
