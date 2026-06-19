using System.Collections.Generic;
using UnityEngine;

public class ShopNpcInteraction : Interactable
{
    #region Serialized Fields

    [SerializeField]
    private string _shopName = "Shop";

    [SerializeField]
    private ItemData _currency;

    [SerializeField]
    private List<ShopItemEntry> _entries = new();

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
            Debug.LogError("[ShopNpcInteraction] No ClickIndicator found in scene.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public override void OnInteract()
    {
        if (IsPlayerNear() && !_playerMovement.IsMoving)
        {
            OpenShopUI();
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

        StartApproach(approachPos, _approachDistance + 0.5f, OpenShopUI);
    }

    private void OpenShopUI()
    {
        GameManager.Instance.HudController.OpenShopUI(_shopName, _currency, _entries);
    }

    private bool IsPlayerNear() =>
        Vector2.Distance(_playerMovement.transform.position, transform.position)
        <= _approachDistance + 0.5f;

    #endregion
}
