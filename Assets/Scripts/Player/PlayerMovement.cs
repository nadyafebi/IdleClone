using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Movement")]
    [SerializeField]
    private float _walkSpeed = 4f;

    [SerializeField]
    private float _climbSpeed = 3f;

    [Header("References")]
    [SerializeField]
    private ClickRouter _clickRouter;

    [Header("Debug")]
    [SerializeField]
    private bool _drawPathGizmos = true;

    [SerializeField]
    private Color _pathColor = Color.cyan;

    #endregion

    #region Public Properties

    public bool IsMoving => _moveCoroutine != null;
    public bool IsClimbing => _isClimbingSegment;

    public Vector2 MovementDirection
    {
        get
        {
            if (!IsMoving)
                return Vector2.zero;

            int idx = _activePath.Count > 0 ? _activePath.IndexOf(_currentNode) : -1;
            if (idx < 0)
                return Vector2.zero;

            if (idx < _activePath.Count - 1)
                return (
                    _activePath[idx + 1].worldPosition - _activePath[idx].worldPosition
                ).normalized;

            Vector2 toExact = _exactDestination - (Vector2)transform.position;
            return toExact.sqrMagnitude > 0.0001f ? toExact.normalized : Vector2.zero;
        }
    }

    public event Action<bool> OnFacingChanged;
    public event Action<Vector2> OnDestinationSet;
    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    public event Action OnClimbStarted;
    public event Action OnClimbStopped;

    #endregion

    #region Private Fields

    private PlatformGraphBuilder _graphBuilder;
    private PlatformNode _currentNode;
    private List<PlatformNode> _activePath = new();
    private Coroutine _moveCoroutine;
    private Vector2 _exactDestination;
    private bool _isClimbingSegment; // True for every frame of a ladder segment; cleared only once the player lands.
    private bool _facingRight = false;
    private PlatformNode _pendingNode;
    private Vector2 _pendingExactDest;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked += HandleGroundClicked;
    }

    private void OnDisable()
    {
        if (_clickRouter != null)
            _clickRouter.OnGroundClicked -= HandleGroundClicked;
    }

    private void Start()
    {
        if (_clickRouter == null)
        {
            Debug.LogError("[PlayerMovement] No ClickRouter reference assigned!");
            enabled = false;
            return;
        }

        _graphBuilder = FindFirstObjectByType<PlatformGraphBuilder>();
        if (_graphBuilder == null)
        {
            Debug.LogError("[PlayerMovement] No PlatformGraphBuilder found in scene!");
            enabled = false;
            return;
        }

        _currentNode = _graphBuilder.FindNearestNode(transform.position);
        if (_currentNode != null)
            transform.position = _currentNode.worldPosition;
    }

    #endregion

    #region Public Methods

    public void CancelMovement()
    {
        bool wasMoving = _moveCoroutine != null;

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        if (_isClimbingSegment)
        {
            _isClimbingSegment = false;
            if (wasMoving)
                OnClimbStopped?.Invoke();
        }

        _pendingNode = null;
        _activePath.Clear();

        if (wasMoving)
            OnMovementStopped?.Invoke();
    }

    #endregion

    #region Input

    private void HandleGroundClicked(Vector2 worldPos)
    {
        PlatformNode targetNode = _graphBuilder.FindNearestNode(worldPos, groundOnly: true);
        if (targetNode == null)
            return;

        Vector2 exactDest = new(worldPos.x, targetNode.worldPosition.y);

        // Fire immediately so the cursor updates even when the destination is queued.
        OnDestinationSet?.Invoke(exactDest);

        if (_isClimbingSegment)
        {
            // Queue the destination — the coroutine picks it up once the player lands.
            _pendingNode = targetNode;
            _pendingExactDest = exactDest;
            return;
        }

        StartPathTo(targetNode, exactDest);
    }

    #endregion

    #region Pathing

    private void StartPathTo(PlatformNode targetNode, Vector2 exactDest)
    {
        // No early-return when targetNode == currentNode: the player may still
        // need to walk to a different X position on the same platform.
        List<PlatformNode> path = PlatformPathfinder.FindPath(_currentNode, targetNode);
        if (path.Count == 0)
            return;

        bool wasMoving = _moveCoroutine != null;
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _exactDestination = exactDest;
        _pendingNode = null;
        _activePath = path;
        _moveCoroutine = StartCoroutine(FollowPath(path));

        if (!wasMoving)
            OnMovementStarted?.Invoke();
    }

    #endregion

    #region Movement

    private IEnumerator FollowPath(List<PlatformNode> path)
    {
        int waypointIndex = 1;

        while (waypointIndex < path.Count)
        {
            PlatformNode waypoint = path[waypointIndex];
            PlatformNode previous = path[waypointIndex - 1];

            // Set the climb flag BEFORE any yield so HandleGroundClicked sees the
            // correct value on every frame within this segment.
            bool segmentIsClimb = !Mathf.Approximately(
                waypoint.worldPosition.y,
                previous.worldPosition.y
            );
            _isClimbingSegment = segmentIsClimb;

            if (segmentIsClimb)
            {
                // Snap to the ladder entry node before climbing to avoid diagonal movement.
                while (true)
                {
                    float dist = Vector2.Distance(transform.position, previous.worldPosition);
                    float step = _walkSpeed * Time.deltaTime;
                    if (step >= dist)
                    {
                        transform.position = previous.worldPosition;
                        break;
                    }
                    transform.position = Vector2.MoveTowards(
                        transform.position,
                        previous.worldPosition,
                        step
                    );
                    yield return null;
                }

                OnClimbStarted?.Invoke();
            }

            // On the final walk segment, target exactDestination to avoid overshooting the click.
            bool isFinalWalkSegment = !segmentIsClimb && waypointIndex == path.Count - 1;
            Vector2 segmentTarget = isFinalWalkSegment ? _exactDestination : waypoint.worldPosition;
            float speed = segmentIsClimb ? _climbSpeed : _walkSpeed;

            if (!segmentIsClimb)
                SetFacing(segmentTarget.x - previous.worldPosition.x);

            while (true)
            {
                float distToWaypoint = Vector2.Distance(transform.position, segmentTarget);
                float stepSize = speed * Time.deltaTime;

                if (stepSize >= distToWaypoint)
                {
                    transform.position = segmentTarget;
                    _currentNode = waypoint;
                    break;
                }

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    segmentTarget,
                    stepSize
                );
                yield return null;

                // Exit the inner loop if CancelMovement cleared the climb flag externally.
                if (!_isClimbingSegment && segmentIsClimb)
                    yield break;
            }

            // Clear the flag only after landing so no mid-segment frame sees it false.
            if (segmentIsClimb)
            {
                _isClimbingSegment = false;
                OnClimbStopped?.Invoke();

                if (_pendingNode != null)
                {
                    PlatformNode queued = _pendingNode;
                    Vector2 queuedDest = _pendingExactDest;
                    _pendingNode = null;

                    List<PlatformNode> newPath = PlatformPathfinder.FindPath(_currentNode, queued);
                    if (newPath.Count > 0)
                    {
                        _exactDestination = queuedDest;
                        _activePath = newPath;
                        path = newPath;
                        waypointIndex = 1;
                        continue;
                    }
                }
            }

            waypointIndex++;
        }

        SetFacing(_exactDestination.x - (float)transform.position.x);

        while (true)
        {
            float dist = Vector2.Distance(transform.position, _exactDestination);
            float step = _walkSpeed * Time.deltaTime;
            if (step >= dist)
            {
                transform.position = _exactDestination;
                break;
            }
            transform.position = Vector2.MoveTowards(transform.position, _exactDestination, step);
            yield return null;
        }

        _isClimbingSegment = false;
        _activePath.Clear();
        _moveCoroutine = null;

        if (_pendingNode != null)
        {
            PlatformNode queued = _pendingNode;
            Vector2 queuedDest = _pendingExactDest;
            _pendingNode = null;
            StartPathTo(queued, queuedDest);
        }
        else
        {
            OnMovementStopped?.Invoke();
        }
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
        if (!_drawPathGizmos || _activePath == null || _activePath.Count == 0)
            return;

        Gizmos.color = _pathColor;
        for (int i = 0; i < _activePath.Count - 1; i++)
            Gizmos.DrawLine(_activePath[i].worldPosition, _activePath[i + 1].worldPosition);

        if (_activePath.Count >= 1)
            Gizmos.DrawLine(_activePath[^1].worldPosition, _exactDestination);

        Gizmos.DrawWireSphere(_exactDestination, 0.2f);
    }

    #endregion
}
