using System.Collections.Generic;
using UnityEngine.UIElements;

public class CraftingMenu : GameMenu
{
    #region Private Fields

    private VisualElement _panel;
    private ScrollView _recipeList;
    private PlayerInventory _inventory;
    private readonly List<RecipeData> _recipes = new();
    private readonly List<(RecipeData Recipe, Button Button)> _recipeButtons = new();

    #endregion

    #region Protected Properties

    protected override VisualElement BlockingElement => _panel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _panel = Root.Q("crafting-panel");
        _recipeList = Root.Q<ScrollView>("recipe-list");
    }

    #endregion

    #region Public Methods

    public void Open(IReadOnlyList<RecipeData> recipes)
    {
        _recipes.Clear();
        _recipes.AddRange(recipes);
        BuildRecipes();
        if (!IsVisible)
            Show();
        else
            RefreshButtons();
    }

    #endregion

    #region Protected Methods

    protected override void OnShow()
    {
        _inventory.OnChanged += RefreshButtons;
        RefreshButtons();
    }

    protected override void OnHide()
    {
        _inventory.OnChanged -= RefreshButtons;
    }

    #endregion

    #region Private Helpers

    private void BuildRecipes()
    {
        _recipeList.Clear();
        _recipeButtons.Clear();

        VisualElement lastRow = null;

        foreach (RecipeData recipe in _recipes)
        {
            if (recipe == null || recipe.ResultItem == null)
                continue;

            var row = new VisualElement();
            row.AddToClassList("recipe-row");
            lastRow = row;

            // Result icon with slot background
            var resultWrapper = new VisualElement();
            resultWrapper.AddToClassList("recipe-result-wrapper");

            var resultIcon = new VisualElement();
            resultIcon.AddToClassList("recipe-result-icon");
            resultIcon.style.backgroundImage = new StyleBackground(recipe.ResultItem.Icon);
            resultWrapper.Add(resultIcon);

            if (recipe.ResultQuantity > 1)
            {
                var qtyLabel = new Label(recipe.ResultQuantity.ToString());
                qtyLabel.AddToClassList("recipe-result-qty");
                resultWrapper.Add(qtyLabel);
            }

            row.Add(resultWrapper);

            // Ingredient chips
            var ingredientsContainer = new VisualElement();
            ingredientsContainer.AddToClassList("recipe-ingredients");

            foreach (IngredientEntry ingredient in recipe.Ingredients)
            {
                if (ingredient.Item == null)
                    continue;

                var chip = new VisualElement();
                chip.AddToClassList("ingredient-chip");

                var icon = new VisualElement();
                icon.AddToClassList("ingredient-icon");
                icon.style.backgroundImage = new StyleBackground(ingredient.Item.Icon);

                var countLabel = new Label($"x{ingredient.Quantity}");
                countLabel.AddToClassList("ingredient-label");

                chip.Add(icon);
                chip.Add(countLabel);
                ingredientsContainer.Add(chip);
            }

            row.Add(ingredientsContainer);

            // Craft button
            RecipeData captured = recipe;
            var craftButton = new Button(() => TryCraft(captured));
            craftButton.text = "Craft";
            craftButton.AddToClassList("craft-button");
            row.Add(craftButton);

            _recipeButtons.Add((recipe, craftButton));
            _recipeList.Add(row);
        }

        lastRow?.AddToClassList("recipe-row--last");
    }

    private void RefreshButtons()
    {
        foreach ((RecipeData recipe, Button button) in _recipeButtons)
            button.SetEnabled(CanCraft(recipe));
    }

    private bool CanCraft(RecipeData recipe)
    {
        foreach (IngredientEntry ingredient in recipe.Ingredients)
        {
            if (ingredient.Item == null)
                continue;
            if (!_inventory.Items.TryGetValue(ingredient.Item, out int qty) || qty < ingredient.Quantity)
                return false;
        }
        return true;
    }

    private void TryCraft(RecipeData recipe)
    {
        if (!CanCraft(recipe))
            return;

        foreach (IngredientEntry ingredient in recipe.Ingredients)
            _inventory.RemoveItem(ingredient.Item, ingredient.Quantity);

        _inventory.AddItem(recipe.ResultItem, recipe.ResultQuantity);
    }

    #endregion
}
