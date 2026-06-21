using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class OfflineProgressionUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Private Fields

    private VisualElement _overlay;
    private Label _titleLabel;
    private Label _timeLabel;
    private Label _activityLabel;
    private VisualElement _rewardsList;
    private Button _claimButton;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        var root = _document.rootVisualElement;
        _overlay = root.Q("offline-overlay");
        _titleLabel = root.Q<Label>("lbl-offline-title");
        _timeLabel = root.Q<Label>("lbl-offline-time");
        _activityLabel = root.Q<Label>("lbl-offline-activity");
        _rewardsList = root.Q("offline-rewards-list");
        _claimButton = root.Q<Button>("btn-offline-claim");

        HidePanel();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    #endregion

    #region Scene Loading

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HidePanel();

        if (GameManager.Instance == null)
            return;
        if (scene.name == GameManager.Instance.StartSceneName)
            return;

        OfflineProgressionResult result = GameManager.Instance.PendingOfflineResult;
        if (result == null || !result.IsValid)
            return;

        ShowPanel(result);
    }

    #endregion

    #region Private Methods

    private void ShowPanel(OfflineProgressionResult result)
    {
        if (_titleLabel == null)
            return;

        // Title
        _titleLabel.RemoveFromClassList("offline-title--died");
        if (result.PlayerDied)
        {
            _titleLabel.text = "You Died";
            _titleLabel.AddToClassList("offline-title--died");
        }
        else
        {
            _titleLabel.text = "AFK Gains";
        }

        // Time away
        _timeLabel.text = $"You were away for {FormatTime(result.TimeAwaySeconds)}";

        // Activity
        string verb = result.TargetType == "enemy" ? "Fighting" : "Gathering";
        _activityLabel.text = $"{verb} {result.TargetDisplayName}";

        // Rewards list
        _rewardsList.Clear();

        AddRewardRow($"XP:  +{result.XpGained}");

        foreach (ItemSaveEntry entry in result.ItemsGained)
        {
            ItemData item = GameManager.Instance.SaveRegistry.FindItem(entry.itemName);
            string displayName = item != null ? item.DisplayName : entry.itemName;
            AddRewardRow($"{displayName}:  +{entry.quantity}");
        }

        // Claim button
        _claimButton.text = result.PlayerDied ? "Claim & Respawn" : "Claim";

        _claimButton.clicked -= OnClaim;
        _claimButton.clicked += OnClaim;

        _overlay.style.display = DisplayStyle.Flex;
    }

    private void HidePanel()
    {
        if (_overlay != null)
            _overlay.style.display = DisplayStyle.None;
    }

    private void OnClaim()
    {
        HidePanel();

        OfflineProgressionResult result = GameManager.Instance.PendingOfflineResult;
        if (result == null)
            return;

        GameManager gm = GameManager.Instance;

        gm.PlayerLevel.AddXp(result.XpGained);

        foreach (ItemSaveEntry entry in result.ItemsGained)
        {
            ItemData item = gm.SaveRegistry.FindItem(entry.itemName);
            if (item != null)
                gm.PlayerInventory.AddItem(item, entry.quantity);
        }

        for (int i = 0; i < result.PotionsConsumed; i++)
            gm.PlayerEquipment.ConsumePotion();

        if (result.TargetType == "enemy" && result.KillsEarned > 0)
        {
            EnemyData enemy = gm.SaveRegistry.FindEnemy(result.TargetName);
            if (enemy != null)
            {
                gm.EnemyProgressTracker.AddKills(enemy, result.KillsEarned);
                gm.QuestManager.AddOfflineKills(enemy, result.KillsEarned);
            }
        }

        gm.ClearPendingOfflineResult();

        if (result.PlayerDied)
            gm.TransitionToTownScene();
    }

    private void AddRewardRow(string text)
    {
        var label = new Label(text);
        label.AddToClassList("offline-reward-row");
        _rewardsList.Add(label);
    }

    private static string FormatTime(long seconds)
    {
        long hours = seconds / 3600;
        long minutes = (seconds % 3600) / 60;

        if (hours > 0)
            return $"{hours} hour{(hours != 1 ? "s" : "")} {minutes} minute{(minutes != 1 ? "s" : "")}";

        return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
    }

    #endregion
}
