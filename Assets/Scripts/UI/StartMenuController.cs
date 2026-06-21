using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StartMenuController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    #endregion

    #region Private Fields

    private Label _afkLabel;
    private long _saveTimestamp;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        var root = _document.rootVisualElement;
        bool hasSave = SaveSystem.HasSave();

        root.Q("btn-new-game").style.display = hasSave ? DisplayStyle.None : DisplayStyle.Flex;
        root.Q("save-panel").style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;

        root.Q<Button>("btn-new-game")
            ?.RegisterCallback<ClickEvent>(_ => GameManager.Instance.TransitionToTownScene());

        // Cheat menu toggle
        var cheatPanel = root.Q("cheat-panel");
        root.Q<Button>("btn-cheat-toggle")
            ?.RegisterCallback<ClickEvent>(_ =>
            {
                bool visible = cheatPanel.style.display == DisplayStyle.Flex;
                cheatPanel.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
            });

        root.Q<Button>("btn-add-5m") ?.RegisterCallback<ClickEvent>(_ => SubtractTimestamp(5  * 60));
        root.Q<Button>("btn-add-30m")?.RegisterCallback<ClickEvent>(_ => SubtractTimestamp(30 * 60));
        root.Q<Button>("btn-add-1h") ?.RegisterCallback<ClickEvent>(_ => SubtractTimestamp(60 * 60));

        if (!hasSave)
            return;

        SaveData data = SaveSystem.Load();
        if (data != null)
            PopulateSaveInfo(root, data);

        root.Q<Button>("btn-load")
            ?.RegisterCallback<ClickEvent>(_ => GameManager.Instance.TransitionToLastSavedScene());

        root.Q<Button>("btn-delete")
            ?.RegisterCallback<ClickEvent>(_ => DeleteSave(root));
    }

    #endregion

    #region Private Methods

    private void PopulateSaveInfo(VisualElement root, SaveData data)
    {
        root.Q<Label>("lbl-level").text = $"Level {data.level}";

        var activityLabel = root.Q<Label>("lbl-activity");
        if (data.lastTargetType == "enemy")
        {
            activityLabel.text = $"Fighting - {data.lastTargetDisplayName}";
            activityLabel.style.display = DisplayStyle.Flex;
        }
        else if (data.lastTargetType == "resource")
        {
            activityLabel.text = $"Gathering - {data.lastTargetDisplayName}";
            activityLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            activityLabel.style.display = DisplayStyle.None;
        }

        if (data.saveTimestamp > 0)
        {
            _saveTimestamp = data.saveTimestamp;
            _afkLabel = root.Q<Label>("lbl-afk");
            RefreshAfkLabel();
            StartCoroutine(TickAfkLabel());
        }
    }

    private static string FormatTime(long seconds)
    {
        long hours   = seconds / 3600;
        long minutes = (seconds % 3600) / 60;

        if (hours > 0)
            return $"{hours}h {minutes}m";

        return $"{minutes}m";
    }

    private void RefreshAfkLabel()
    {
        if (_afkLabel == null) return;
        long secondsAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _saveTimestamp;
        _afkLabel.text = $"{FormatTime(secondsAgo)} AFK";
    }

    private IEnumerator TickAfkLabel()
    {
        var wait = new WaitForSeconds(60f);
        while (true)
        {
            yield return wait;
            RefreshAfkLabel();
        }
    }

    private void SubtractTimestamp(long seconds)
    {
        SaveData data = SaveSystem.Load();
        if (data == null || data.saveTimestamp <= 0) return;

        data.saveTimestamp -= seconds;
        SaveSystem.Save(data);

        GameManager.Instance.RecomputeOfflineResult();

        _saveTimestamp = data.saveTimestamp;
        RefreshAfkLabel();
    }

    private void DeleteSave(VisualElement root)
    {
        GameManager.Instance.DeleteSave();
        root.Q("btn-new-game").style.display = DisplayStyle.Flex;
        root.Q("save-panel").style.display = DisplayStyle.None;
    }

    #endregion
}
