using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Debug")]
    [SerializeField]
    private bool _drawPathGizmos = true;

    [SerializeField]
    private Color _pathColor = Color.cyan;

    #endregion

    #region Public Properties

    public bool IsMoving => _moveCoroutine != null;
    public bool IsClimbing => _isClimbingSegment;
    public bool IsJumping => _isJumpingSegment;
    public bool FacingRight => _facingRight;

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

    public float WalkSpeed => _walkSpeed;

    public event Action<bool> OnFacingChanged;
    public event Action<Vector2> OnDestinationSet;
    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    public event Action OnClimbStarted;
    public event Action OnClimbStopped;
    public event Action OnJumpStarted;
    public event Action OnJumpStopped;
    public event Action<float> OnWalkSpeedChanged;

    #endregion

    #region Private Fields

    private float _walkSpeed;
    private float _climbSpeed;
    private float _jumpArcHeight;
    private ClickRouter _clickRouter;
    private PlatformGraphBuilder _graphBuilder;
    private PlatformNode _currentNode;
    private List<PlatformNode> _activePath = new();
    private Coroutine _moveCoroutine;
    private Vector2 _exactDestination;
    private bool _isClimbingSegment; // True for every frame of a ladder segment; cleared only once the player lands.
    private bool _isJumpingSegment; // True for every frame of a jump arc; cleared only once the player lands.
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
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerMovement] No GameManager found!");
            enabled = false;
            return;
        }

        _clickRouter = GameManager.Instance.ClickRouter;
        if (_clickRouter == null)
        {
            Debug.LogError("[PlayerMovement] No ClickRouter found on GameManager!");
            enabled = false;
            return;
        }

        PlayerStats stats = GameManager.Instance.PlayerStats;
        if (stats == null)
        {
            Debug.LogError("[PlayerMovement] No PlayerStats found on GameManager!");
            enabled = false;
            return;
        }
        _walkSpeed = stats.WalkSpeed;
        _climbSpeed = stats.ClimbSpeed;
        _jumpArcHeight = stats.JumpArcHeight;
        // OnEnable fired before Start so _clickRouter was null then; subscribe now.
        _clickRouter.OnGroundClicked += HandleGroundClicked;

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

    public void SetWalkSpeed(float speed)
    {
        _walkSpeed = speed;
        OnWalkSpeedChanged?.Invoke(_walkSpeed);
    }

    public void MoveTo(Vector2 worldPos)
    {
        PlatformNode targetNode = _graphBuilder.FindNearestNode(worldPos, groundOnly: true);
        if (targetNode == null)
            return;

        Vector2 exactDest = new(
            _graphBuilder.ClampXToPlatform(worldPos.x, targetNode),
            targetNode.worldPosition.y
        );

        if (_isClimbingSegment || _isJumpingSegment)
        {
            _pendingNode = targetNode;
            _pendingExactDest = exactDest;
            return;
        }

        StartPathTo(targetNode, exactDest);
    }

    public void SetFacing(bool facingRight)
    {
        _facingRight = facingRight;
        OnFacingChanged?.Invoke(_facingRight);
    }

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

        if (_isJumpingSegment)
        {
            _isJumpingSegment = false;
            if (wasMoving)
                OnJumpStopped?.Invoke();
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

        Vector2 exactDest = new(
            _graphBuilder.ClampXToPlatform(worldPos.x, targetNode),
            targetNode.worldPosition.y
        );

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

            bool yDiffers = !Mathf.Approximately(
                waypoint.worldPosition.y,
                previous.worldPosition.y
            );
            bool xDiffers = !Mathf.Approximately(
                waypoint.worldPosition.x,
                previous.worldPosition.x
            );
            bool segmentIsClimb = yDiffers && !xDiffers;
            bool segmentIsJump = yDiffers && xDiffers;

            // Set transition flags BEFORE any yield so queuing checks see the correct value.
            _isClimbingSegment = segmentIsClimb;
            _isJumpingSegment = segmentIsJump;

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
            else if (segmentIsJump)
            {
                // Snap to the jump source node so the arc always starts from a clean position.
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

                OnJumpStarted?.Invoke();
            }

            // On the final walk segment, target exactDestination to avoid overshooting the click.
            bool isFinalWalkSegment =
                !segmentIsClimb && !segmentIsJump && waypointIndex == path.Count - 1;
            Vector2 segmentTarget = isFinalWalkSegment ? _exactDestination : waypoint.worldPosition;
            float speed = segmentIsClimb ? _climbSpeed : _walkSpeed;

            if (!segmentIsClimb)
                SetFacing(segmentTarget.x - previous.worldPosition.x);

            if (segmentIsJump)
            {
                Vector2 jumpStart = previous.worldPosition;
                Vector2 jumpEnd = waypoint.worldPosition;
                Vector2 peak = new Vector2(
                    (jumpStart.x + jumpEnd.x) * 0.5f,
                    Mathf.Max(jumpStart.y, jumpEnd.y) + _jumpArcHeight
                );

                float duration = Vector2.Distance(jumpStart, jumpEnd) / _walkSpeed;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    transform.position = QuadBezier(jumpStart, peak, jumpEnd, t);
                    yield return null;

                    if (!_isJumpingSegment)
                        yield break;
                }

                transform.position = jumpEnd;
                _currentNode = waypoint;
            }
            else
            {
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
            }

            // Clear transition flags only after landing so no mid-segment frame sees them false.
            if (segmentIsClimb)
            {
                _isClimbingSegment = false;
                OnClimbStopped?.Invoke();
            }
            else if (segmentIsJump)
            {
                _isJumpingSegment = false;
                OnJumpStopped?.Invoke();
            }

            if ((segmentIsClimb || segmentIsJump) && _pendingNode != null)
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
        _isJumpingSegment = false;
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

    private static Vector2 QuadBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
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
