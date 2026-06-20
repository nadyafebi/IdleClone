using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerGathering : MonoBehaviour
{
    #region Public Properties

    public bool IsGathering => _gatherCoroutine != null;
    public float GatherRange => _stats != null ? _stats.GatherRange : 0f;

    public event Action OnTargetOutOfRange;

    #endregion

    #region Private Fields

    private ResourceNodeHealth _target;
    private Coroutine _gatherCoroutine;
    private PlayerMovement _playerMovement;
    private PlayerStats _stats;

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

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerGathering] No GameManager found!");
            enabled = false;
            return;
        }
        _stats = GameManager.Instance.PlayerStats;
        if (_stats == null)
        {
            Debug.LogError("[PlayerGathering] No PlayerStats found on GameManager!");
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
            if (Vector2.Distance(transform.position, _target.transform.position) > GatherRange)
            {
                OnTargetOutOfRange?.Invoke();
                yield break;
            }

            _target.TakeHit(_stats.GatheringDamage);
            yield return new WaitForSeconds(_stats.GatherCooldown);
        }
    }

    #endregion
}
