using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerGathering : MonoBehaviour
{
    #region Serialized Fields

    [Header("Gathering")]
    [SerializeField]
    [Min(1)]
    private int _gatheringDamage = 1;

    [SerializeField]
    [Tooltip("Seconds between gathering hits.")]
    private float _gatherCooldown = 1f;

    [SerializeField]
    [Tooltip("Maximum distance to the resource node to keep gathering.")]
    private float _gatherRange = 2f;

    #endregion

    #region Public Properties

    public bool IsGathering => _gatherCoroutine != null;
    public float GatherRange => _gatherRange;

    public event Action OnTargetOutOfRange;

    #endregion

    #region Private Fields

    private ResourceNodeHealth _target;
    private Coroutine _gatherCoroutine;
    private PlayerMovement _playerMovement;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        if (_playerMovement == null)
        {
            Debug.LogError("[PlayerGathering] No PlayerMovement found on this GameObject.");
            enabled = false;
        }
    }

    #endregion

    #region Public Methods

    public void StartGathering(ResourceNodeHealth target)
    {
        if (_target == target)
            return;

        StopGathering();
        _target = target;
        _gatherCoroutine = StartCoroutine(GatherLoop());
    }

    public void StopGathering()
    {
        if (_gatherCoroutine != null)
        {
            StopCoroutine(_gatherCoroutine);
            _gatherCoroutine = null;
        }
        _target = null;
    }

    #endregion

    #region Gathering

    private IEnumerator GatherLoop()
    {
        while (_target != null)
        {
            if (Vector2.Distance(transform.position, _target.transform.position) > _gatherRange)
            {
                OnTargetOutOfRange?.Invoke();
                yield break;
            }

            _target.TakeHit(_gatheringDamage);
            yield return new WaitForSeconds(_gatherCooldown);
        }
    }

    #endregion
}
