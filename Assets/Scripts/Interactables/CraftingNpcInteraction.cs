using System.Collections.Generic;
using UnityEngine;

public class CraftingNpcInteraction : Interactable
{
    #region Serialized Fields

    [SerializeField]
    private List<RecipeData> _recipes = new();

    [Tooltip("How far from the NPC the player stops (world units).")]
    [SerializeField]
    private float _approachDistance = 1.5f;

    #endregion

    #region Private Fields

    private ClickIndicator _clickIndicator;

    #endregion

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        if (!enabled)
            return;

        _clickIndicator = FindFirstObjectByType<ClickIndicator>();
        if (_clickIndicator == null)
        {
            Debug.LogError("[CraftingNpcInteraction] No ClickIndicator found in scene.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (IsPlayerNear() && !_playerMovement.IsMoving)
        {
            OpenCraftingUI();
            return;
        }

        MoveToNpc();
    }

    #endregion

    #region Private Methods

    private void MoveToNpc()
    {
        _clickIndicator.ShowNpcCursor(transform);

        float approachSign = Mathf.Sign(
            _playerMovement.transform.position.x - transform.position.x
        );
        if (approachSign == 0f)
            approachSign = -1f;

        Vector2 approachPos = new(
            transform.position.x + approachSign * _approachDistance,
            transform.position.y
        );

        StartApproach(approachPos, _approachDistance + 0.5f, OpenCraftingUI);
    }

    private void OpenCraftingUI()
    {
        GameManager.Instance.HudController.OpenCraftingUI(_recipes);
    }

    private bool IsPlayerNear() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position)
        <= _approachDistance + 0.5f;

    #endregion
}
