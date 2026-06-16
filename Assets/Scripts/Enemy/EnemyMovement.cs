using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Movement")]
    [SerializeField]
    private float _walkSpeed = 2f;

    [Header("Patrol")]
    [SerializeField]
    [Tooltip("Minimum seconds to idle before choosing the next action.")]
    private float _idleTimeMin = 1f;

    [SerializeField]
    [Tooltip("Maximum seconds to idle before choosing the next action.")]
    private float _idleTimeMax = 3f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Probability [0–1] that the enemy moves rather than idles again.")]
    private float _moveProbability = 0.6f;

    [Header("Debug")]
    [SerializeField]
    private bool _drawGizmos = true;

    #endregion

    #region Public Properties

    public bool IsMoving => _moveCoroutine != null;
    public Vector2 MovementDirection => _movementDirection;

    public event Action<bool> OnFacingChanged;
    public event Action OnMovementStarted;
    public event Action OnMovementStopped;

    #endregion

    #region Private Fields

    private PlatformGraphBuilder _graphBuilder;
    private PlatformNode _currentNode;
    private List<PlatformNode> _platformNodes = new();
    private Coroutine _patrolCoroutine;
    private Coroutine _moveCoroutine;
    private Vector2 _movementDirection;
    private bool _facingRight = true;
    private bool _activated;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _graphBuilder = FindFirstObjectByType<PlatformGraphBuilder>();
        if (_graphBuilder == null)
        {
            Debug.LogError("[EnemyMovement] No PlatformGraphBuilder found in scene!");
            enabled = false;
        }
    }

    private void Start()
    {
        // Spawner calls Activate() before this Start() fires; skip auto-start if so.
        if (_activated || !enabled)
            return;

        PlatformNode startNode = _graphBuilder.FindNearestNode(transform.position, groundOnly: true);
        if (startNode == null)
        {
            Debug.LogError("[EnemyMovement] Could not find a ground node near spawn position!");
            enabled = false;
            return;
        }

        Activate(startNode);
    }

    private void OnDisable()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        if (_patrolCoroutine != null)
        {
            StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }
    }

    #endregion

    #region Public Methods

    public void Activate(PlatformNode startNode)
    {
        _activated = true;

        if (_patrolCoroutine != null)
        {
            StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        _movementDirection = Vector2.zero;

        _currentNode = startNode;
        transform.position = startNode.worldPosition;
        CollectPlatformNodes();
        _patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    public void CancelMovement()
    {
        if (_moveCoroutine == null)
            return;

        StopCoroutine(_moveCoroutine);
        _moveCoroutine = null;
        _movementDirection = Vector2.zero;
        OnMovementStopped?.Invoke();
    }

    #endregion

    #region Patrol

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_idleTimeMin, _idleTimeMax));

            if (UnityEngine.Random.value > _moveProbability || _platformNodes.Count <= 1)
                continue;

            PlatformNode target = PickRandomNode();
            if (target == null)
                continue;

            _moveCoroutine = StartCoroutine(WalkToNode(target));
            yield return _moveCoroutine;
            _moveCoroutine = null;
        }
    }

    private PlatformNode PickRandomNode()
    {
        // Always pick a different node so the enemy visibly moves.
        var candidates = _platformNodes.FindAll(n => n != _currentNode);
        if (candidates.Count == 0)
            return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    // Flood-fill via same-Y walk neighbors only — no ladders, no jumps.
    private void CollectPlatformNodes()
    {
        _platformNodes.Clear();
        var visited = new HashSet<PlatformNode>();
        var queue = new Queue<PlatformNode>();

        queue.Enqueue(_currentNode);
        visited.Add(_currentNode);

        while (queue.Count > 0)
        {
            PlatformNode node = queue.Dequeue();
            _platformNodes.Add(node);

            foreach (PlatformNode neighbor in node.neighbors)
            {
                if (visited.Contains(neighbor))
                    continue;
                if (!Mathf.Approximately(neighbor.worldPosition.y, node.worldPosition.y))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
    }

    #endregion

    #region Movement

    private IEnumerator WalkToNode(PlatformNode target)
    {
        Vector2 destination = target.worldPosition;
        SetFacing(destination.x - (float)transform.position.x);
        _movementDirection = (destination - (Vector2)transform.position).normalized;
        OnMovementStarted?.Invoke();

        while (true)
        {
            float dist = Vector2.Distance(transform.position, destination);
            float step = _walkSpeed * Time.deltaTime;

            if (step >= dist)
            {
                transform.position = destination;
                break;
            }

            transform.position = Vector2.MoveTowards(transform.position, destination, step);
            yield return null;
        }

        _currentNode = target;
        _movementDirection = Vector2.zero;
        OnMovementStopped?.Invoke();
    }

    private void SetFacing(float directionX)
    {
        if (directionX == 0f)
            return;
        bool faceRight = directionX > 0f;
        if (faceRight == _facingRight)
            return;
        _facingRight = faceRight;
        OnFacingChanged?.Invoke(_facingRight);
    }

    #endregion

    #region Editor Visualisation

    private void OnDrawGizmos()
    {
        if (!_drawGizmos)
            return;

        if (_currentNode != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentNode.worldPosition, 0.2f);
        }

        if (_platformNodes == null || _platformNodes.Count == 0)
            return;

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
        foreach (var node in _platformNodes)
            Gizmos.DrawSphere(node.worldPosition, 0.15f);
    }

    #endregion
}
