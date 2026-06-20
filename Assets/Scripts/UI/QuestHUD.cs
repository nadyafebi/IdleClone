using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestHUD : MonoBehaviour, IPointerBlocker
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Private Fields

    private VisualElement _panel;
    private ScrollView _questList;
    private Button _toggleButton;
    private bool _expanded = true;
    private bool _panelVisible;

    private QuestManager _questManager;
    private PlayerInventory _playerInventory;
    private ClickRouter _clickRouter;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _questManager = GameManager.Instance?.QuestManager;
        _playerInventory = GameManager.Instance?.PlayerInventory;

        if (_questManager == null)
        {
            Debug.LogError("[QuestHUD] QuestManager not found on GameManager.");
            enabled = false;
            return;
        }

        var root = _document.rootVisualElement;
        _panel = root.Q("quest-panel");
        _questList = root.Q<ScrollView>("quest-list");
        _toggleButton = root.Q<Button>("quest-toggle");

        _toggleButton?.RegisterCallback<ClickEvent>(_ => ToggleExpand());

        _clickRouter = GameManager.Instance.ClickRouter;
        _clickRouter?.AddSpatialBlocker(this);

        _questManager.OnQuestUpdated += HandleQuestUpdated;
        if (_playerInventory != null)
            _playerInventory.OnChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        _clickRouter?.RemoveSpatialBlocker(this);

        if (_questManager != null)
            _questManager.OnQuestUpdated -= HandleQuestUpdated;

        if (_playerInventory != null)
            _playerInventory.OnChanged -= Refresh;
    }

    #endregion

    #region Public Methods

    public bool ContainsScreenPoint(Vector2 screenPos)
    {
        if (!_panelVisible || _panel == null)
            return false;

        Rect wb = _panel.worldBound;
        float dpi = _panel.panel.scaledPixelsPerPoint;

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

    private void ToggleExpand()
    {
        _expanded = !_expanded;

        if (_questList != null)
            _questList.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;

        if (_toggleButton != null)
            _toggleButton.text = _expanded ? "▲" : "▼";
    }

    private void HandleQuestUpdated(QuestData _) => Refresh();

    private void Refresh()
    {
        if (_questList == null || _panel == null)
            return;

        _questList.Clear();

        List<QuestData> activeQuests = _questManager.GetActiveQuests();

        _panelVisible = activeQuests.Count > 0;
        _panel.style.display = _panelVisible ? DisplayStyle.Flex : DisplayStyle.None;

        foreach (QuestData quest in activeQuests)
        {
            int current = Mathf.Min(_questManager.GetCurrentCount(quest), quest.RequiredCount);
            bool met = _questManager.IsObjectiveMet(quest);

            string progressText = quest.Type switch
            {
                QuestType.Kill => $"Kill {quest.TargetEnemy?.EnemyName ?? "?"}: {current}/{quest.RequiredCount}",
                QuestType.Collect => $"Collect {quest.TargetItem?.DisplayName ?? "?"}: {current}/{quest.RequiredCount}",
                QuestType.Upgrade => $"{quest.TargetUpgradeType} upgrade tier: {current}/{quest.RequiredCount}",
                _ => $"{current}/{quest.RequiredCount}",
            };

            var entry = new VisualElement();
            entry.AddToClassList("quest-entry");

            var nameLabel = new Label(quest.QuestName);
            nameLabel.AddToClassList("quest-name");
            if (met)
                nameLabel.AddToClassList("quest-ready");

            var progressLabel = new Label(progressText);
            progressLabel.AddToClassList("quest-progress");

            entry.Add(nameLabel);
            entry.Add(progressLabel);
            _questList.Add(entry);
        }
    }

    #endregion
}
