using UnityEngine;
using UnityEngine.UIElements;

public class StartMenuController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

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

        if (!hasSave)
            return;

        SaveData data = SaveSystem.Load();
        if (data != null)
            root.Q<Label>("lbl-level").text = $"Level {data.level}";

        root.Q<Button>("btn-load")
            ?.RegisterCallback<ClickEvent>(_ => GameManager.Instance.TransitionToTownScene());

        root.Q<Button>("btn-delete")
            ?.RegisterCallback<ClickEvent>(_ => DeleteSave(root));
    }

    #endregion

    #region Private Methods

    private void DeleteSave(VisualElement root)
    {
        GameManager.Instance.DeleteSave();
        root.Q("btn-new-game").style.display = DisplayStyle.Flex;
        root.Q("save-panel").style.display = DisplayStyle.None;
    }

    #endregion
}
