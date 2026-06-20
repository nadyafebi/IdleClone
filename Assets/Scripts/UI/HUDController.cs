using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour, IPointerBlocker
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [Header("Left Menus")]
    [SerializeField]
    private PlayerStatsMenu _playerStatsMenu;

    [SerializeField]
    private CraftingMenu _craftingMenu;

    [SerializeField]
    private ShopMenu _shopMenu;

    [Header("Right Menus")]
    [SerializeField]
    private InventoryMenu _inventoryMenu;

    [SerializeField]
    private UpgradeMenu _upgradeMenu;

    #endregion

    #region Private Fields

    private VisualElement _hudPanel;
    private ClickRouter _clickRouter;

    private Label _levelLabel;
    private VisualElement _xpBarFill;
    private Label _xpAmountLabel;
    private PlayerLevel _playerLevel;

    private VisualElement _hpBarFill;
    private Label _hpAmountLabel;
    private PlayerHealth _playerHealth;

    private VisualElement _potionSlot;
    private PlayerEquipment _playerEquipment;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _hudPanel = _document.rootVisualElement.Q("hud-panel");

        _document.rootVisualElement.Q("btn-items")
            ?.RegisterCallback<ClickEvent>(_ => OnItemsButtonClicked());

        _document.rootVisualElement.Q("btn-upgrades")
            ?.RegisterCallback<ClickEvent>(_ => OnUpgradesButtonClicked());

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

        _potionSlot = _document.rootVisualElement.Q("potion-slot");
        if (_potionSlot != null)
            _potionSlot.RegisterCallback<ClickEvent>(_ => UsePotion());

        _playerEquipment = GameManager.Instance?.PlayerEquipment;
        if (_playerEquipment != null)
        {
            _playerEquipment.OnEquipmentChanged += RefreshPotionSlot;
            RefreshPotionSlot();
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

        if (_playerEquipment != null)
            _playerEquipment.OnEquipmentChanged -= RefreshPotionSlot;
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

    public void OpenCraftingUI(IReadOnlyList<RecipeData> recipes)
    {
        CloseAll();
        _craftingMenu?.Open(recipes);
        _inventoryMenu?.Show();
    }

    public void OpenShopUI(string shopName, ItemData currency, IReadOnlyList<ShopItemEntry> entries)
    {
        CloseAll();
        _shopMenu?.Open(shopName, currency, entries);
        _inventoryMenu?.Show();
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

    private void OnItemsButtonClicked()
    {
        if (_inventoryMenu != null && _inventoryMenu.IsVisible)
            CloseAll();
        else
            OpenRightMenu(_inventoryMenu);
    }

    private void OnUpgradesButtonClicked()
    {
        if (_upgradeMenu != null && _upgradeMenu.IsVisible)
            CloseAll();
        else
            OpenRightMenu(_upgradeMenu);
    }

    // Opens player stats on the left alongside the given right menu.
    private void OpenRightMenu(GameMenu rightMenu)
    {
        CloseAll();
        _playerStatsMenu?.Show();
        rightMenu?.Show();
    }

    private void CloseAll()
    {
        HideIfVisible(_playerStatsMenu);
        HideIfVisible(_craftingMenu);
        HideIfVisible(_shopMenu);
        HideIfVisible(_inventoryMenu);
        HideIfVisible(_upgradeMenu);
    }

    private static void HideIfVisible(GameMenu menu)
    {
        if (menu != null && menu.IsVisible)
            menu.Hide();
    }

    private void RefreshPotionSlot()
    {
        if (_potionSlot == null)
            return;

        _potionSlot.Clear();
        ItemData potion = _playerEquipment?.PotionSlot;
        _potionSlot.EnableInClassList("skill-slot--usable", potion != null);
        if (potion == null)
            return;

        var icon = new VisualElement();
        icon.style.backgroundImage = new StyleBackground(potion.Icon);
        icon.style.width = new StyleLength(32f);
        icon.style.height = new StyleLength(32f);
        icon.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
        icon.pickingMode = PickingMode.Ignore;
        _potionSlot.Add(icon);
    }

    private void UsePotion()
    {
        if (_playerEquipment == null || _playerHealth == null)
            return;

        ItemData potion = _playerEquipment.PotionSlot;
        if (potion == null || potion.HealAmount <= 0)
            return;

        int actualHeal = _playerHealth.Heal(potion.HealAmount);
        _playerEquipment.ConsumePotion();

        if (actualHeal > 0)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
                DamagePopupSpawner.TrySpawnHeal(player.transform.position, actualHeal);
        }
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
