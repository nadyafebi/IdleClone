using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemPickupNotifier : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    [Tooltip("Total lifetime in seconds.")]
    private float _duration = 1.5f;

    [SerializeField]
    [Tooltip("Pixels to float upward over the lifetime.")]
    private float _floatPixels = 60f;

    #endregion

    #region Private Fields

    private VisualElement _container;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_document == null)
        {
            Debug.LogError("[ItemPickupNotifier] No UIDocument assigned.");
            enabled = false;
            return;
        }

        _container = _document.rootVisualElement.Q("item-pickup-container");
        if (_container == null)
        {
            Debug.LogError("[ItemPickupNotifier] item-pickup-container not found in HUD.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public void Spawn(string itemName, int quantity)
    {
        if (_container == null) return;

        var label = new Label($"+{quantity} {itemName}");
        label.AddToClassList("item-pickup-label");
        _container.Add(label);
        StartCoroutine(Animate(label));
    }

    public static void TrySpawn(string itemName, int quantity)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.ItemPickupNotifier?.Spawn(itemName, quantity);
    }

    #endregion

    #region Private Methods

    private IEnumerator Animate(Label label)
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            float t = elapsed / _duration;
            label.style.opacity = 1f - t;
            label.style.translate = new StyleTranslate(new Translate(0f, -_floatPixels * t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        label.RemoveFromHierarchy();
    }

    #endregion
}
