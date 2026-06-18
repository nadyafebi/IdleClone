using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class AutoAttackController : MonoBehaviour
{
    #region Private Fields

    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    private ClickRouter _clickRouter;
    private Coroutine _waitCoroutine;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
        _clickRouter = GameManager.Instance?.ClickRouter;

        if (_playerCombat == null)
        {
            Debug.LogError("[AutoAttackController] No PlayerCombat found on this GameObject!");
            enabled = false;
            return;
        }

        _playerCombat.OnTargetKilled += HandleTargetKilled;
    }

    private void OnDestroy()
    {
        if (_playerCombat != null)
            _playerCombat.OnTargetKilled -= HandleTargetKilled;
        ClearWaitState();
    }

    #endregion

    #region Private Methods

    private void HandleTargetKilled()
    {
        ClearWaitState();
        EnemyInteraction nearest = FindNearestAliveEnemy();
        if (nearest != null)
            nearest.OnInteract();
        else
            BeginWaiting();
    }

    private void BeginWaiting()
    {
        _playerMovement.OnMovementStarted += CancelWait;
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked += HandleGroundClickedWhileWaiting;
        _waitCoroutine = StartCoroutine(WaitForEnemy());
    }

    private void ClearWaitState()
    {
        if (_waitCoroutine == null)
            return;
        StopCoroutine(_waitCoroutine);
        _waitCoroutine = null;
        UnsubscribeWaitListeners();
    }

    private void UnsubscribeWaitListeners()
    {
        _playerMovement.OnMovementStarted -= CancelWait;
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked -= HandleGroundClickedWhileWaiting;
    }

    private void CancelWait() => ClearWaitState();

    private void HandleGroundClickedWhileWaiting(Vector2 _) => ClearWaitState();

    private IEnumerator WaitForEnemy()
    {
        var interval = new WaitForSeconds(0.5f);
        while (true)
        {
            yield return interval;
            EnemyInteraction nearest = FindNearestAliveEnemy();
            if (nearest == null)
                continue;

            // Clear wait state manually — can't call ClearWaitState() from inside the coroutine.
            _waitCoroutine = null;
            UnsubscribeWaitListeners();
            nearest.OnInteract();
            yield break;
        }
    }

    private EnemyInteraction FindNearestAliveEnemy()
    {
        EnemyInteraction[] all = FindObjectsByType<EnemyInteraction>(FindObjectsSortMode.None);
        EnemyInteraction nearest = null;
        float nearestDist = float.MaxValue;

        foreach (EnemyInteraction ei in all)
        {
            EnemyHealth health = ei.GetComponent<EnemyHealth>();
            if (health == null || health.CurrentHealth <= 0)
                continue;

            float dist = Vector2.Distance(_playerMovement.transform.position, ei.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = ei;
            }
        }

        return nearest;
    }

    #endregion
}
