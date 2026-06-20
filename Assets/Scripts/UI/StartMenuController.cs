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
        _document
            .rootVisualElement.Q<Button>("btn-start")
            ?.RegisterCallback<ClickEvent>(_ => GameManager.Instance.TransitionToTownScene());
    }

    #endregion
}
