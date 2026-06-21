using UnityEngine;
using UnityEngine.UIElements;

public class BossHUD : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [Tooltip("Optional — auto-found if left empty.")]
    [SerializeField]
    private BossController _boss;

    #endregion

    #region Private Fields

    private VisualElement _hpFill;
    private Label _hpAmount;
    private Label _nameLabel;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_document == null)
        {
            Debug.LogError("[BossHUD] UIDocument not assigned.");
            enabled = false;
            return;
        }

        if (_boss == null)
            _boss = FindFirstObjectByType<BossController>();

        if (_boss == null || _boss.Health == null)
        {
            Debug.LogError("[BossHUD] BossController or its Health not found.");
            enabled = false;
            return;
        }

        VisualElement root = _document.rootVisualElement;
        _hpFill   = root.Q("boss-hp-fill");
        _hpAmount = root.Q<Label>("boss-hp-amount");
        _nameLabel = root.Q<Label>("boss-name-label");

        if (_nameLabel != null)
            _nameLabel.text = _boss.BossName;

        _boss.Health.OnHealthChanged += RefreshBar;
        _boss.Health.OnDied         += Hide;

        RefreshBar(_boss.Health.CurrentHealth, _boss.Health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (_boss?.Health != null)
        {
            _boss.Health.OnHealthChanged -= RefreshBar;
            _boss.Health.OnDied         -= Hide;
        }
    }

    #endregion

    #region Private Methods

    private void RefreshBar(int current, int max)
    {
        if (_hpFill != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            _hpFill.style.width = Length.Percent(ratio * 100f);
        }

        if (_hpAmount != null)
            _hpAmount.text = $"{current} / {max}";
    }

    private void Hide()
    {
        _document.rootVisualElement.style.display = DisplayStyle.None;
    }

    #endregion
}
