using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum NodeType
{
    Ground,
    LadderBottom,
    LadderTop,
}

public class PlatformNode
{
    public Vector2 worldPosition;
    public NodeType type;
    public List<PlatformNode> neighbors = new();
    public List<PlatformNode> jumpNeighbors = new();

    public PlatformNode(Vector2 position, NodeType nodeType)
    {
        worldPosition = position;
        type = nodeType;
    }
}

public class PlatformGraphBuilder : MonoBehaviour
{
    #region Serialized Fields

    [Header("Tilemaps")]
    [SerializeField]
    private Tilemap _groundTilemap;

    [SerializeField]
    private Tilemap _ladderTilemap;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Should match your tile size in world units (usually 1).")]
    private float _nodeSpacing = 1f;

    [SerializeField]
    [Tooltip("How close two nodes must be (in X) to be considered walkable neighbors.")]
    private float _walkNeighborThreshold = 1.5f;

    [SerializeField]
    [Tooltip("How close a ladder top/bottom must be to a ground node to connect to it.")]
    private float _ladderSnapDistance = 1.5f;

    [SerializeField]
    [Tooltip(
        "Max horizontal distance between ground nodes on different platforms to create a jump edge."
    )]
    private float _jumpMaxHorizontalDist = 1.5f;

    [SerializeField]
    [Tooltip(
        "Max vertical distance between ground nodes on different platforms to create a jump edge."
    )]
    private float _jumpMaxVerticalDist = 1.5f;

    [Header("Debug")]
    [SerializeField]
    private bool _drawGizmos = true;

    #endregion

    #region Public Properties

    public List<PlatformNode> AllNodes { get; private set; } = new List<PlatformNode>();

    #endregion

    #region Private Fields

    private Color _groundNodeColor = Color.green;
    private Color _ladderNodeColor = Color.yellow;
    private Color _edgeColor = new(1f, 1f, 1f, 0.3f);
    private Color _jumpEdgeColor = new(0f, 1f, 1f, 0.5f);

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        BuildGraph();
    }

    #endregion

    #region Public API

    public float ClampXToPlatform(float clickX, PlatformNode node)
    {
        float minX = node.worldPosition.x;
        float maxX = node.worldPosition.x;

        foreach (var neighbor in node.neighbors)
        {
            if (!Mathf.Approximately(neighbor.worldPosition.y, node.worldPosition.y))
                continue;

            minX = Mathf.Min(minX, neighbor.worldPosition.x);
            maxX = Mathf.Max(maxX, neighbor.worldPosition.x);
        }

        float halfSpacing = _nodeSpacing * 0.5f;
        return Mathf.Clamp(clickX, minX - halfSpacing, maxX + halfSpacing);
    }

    public PlatformNode FindNearestNode(Vector2 worldPos, bool groundOnly = true)
    {
        PlatformNode closest = null;
        float closestDist = float.MaxValue;

        foreach (var node in AllNodes)
        {
            if (groundOnly && node.type != NodeType.Ground)
                continue;

            float dist = Vector2.Distance(node.worldPosition, worldPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    #endregion

    #region Graph Construction

    private void BuildGraph()
    {
        AllNodes.Clear();

        BuildGroundNodes();
        ConnectWalkingNeighbors();
        BuildLadderConnections();
        ConnectJumpableNeighbors();

        Debug.Log($"[PlatformGraphBuilder] Built graph: {AllNodes.Count} nodes.");
    }

    private void BuildGroundNodes()
    {
        BoundsInt bounds = _groundTilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (!_groundTilemap.HasTile(cellPos))
                continue;

            // Only surface tiles become nodes — skip buried ones.
            Vector3Int above = cellPos + Vector3Int.up;
            if (_groundTilemap.HasTile(above))
                continue;

            Vector3 cellWorld = _groundTilemap.GetCellCenterWorld(cellPos);
            Vector2 nodePos = new Vector2(cellWorld.x, cellWorld.y + _nodeSpacing * 0.5f);

            AllNodes.Add(new PlatformNode(nodePos, NodeType.Ground));
        }
    }

    private void ConnectWalkingNeighbors()
    {
        for (int i = 0; i < AllNodes.Count; i++)
        {
            for (int j = i + 1; j < AllNodes.Count; j++)
            {
                PlatformNode a = AllNodes[i];
                PlatformNode b = AllNodes[j];

                if (a.type != NodeType.Ground || b.type != NodeType.Ground)
                    continue;
                if (!Mathf.Approximately(a.worldPosition.y, b.worldPosition.y))
                    continue;

                float xDist = Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
                if (xDist <= _nodeSpacing * _walkNeighborThreshold)
                {
                    a.neighbors.Add(b);
                    b.neighbors.Add(a);
                }
            }
        }
    }

    private void BuildLadderConnections()
    {
        BoundsInt bounds = _ladderTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            int? ladderYMin = null;
            int? ladderYMax = null;

            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (!_ladderTilemap.HasTile(new Vector3Int(x, y, 0)))
                    continue;

                if (ladderYMin == null)
                    ladderYMin = y;
                ladderYMax = y;
            }

            if (ladderYMin == null)
                continue;

            Vector3 bottomWorld = _ladderTilemap.GetCellCenterWorld(
                new Vector3Int(x, ladderYMin.Value, 0)
            );
            Vector3 topWorld = _ladderTilemap.GetCellCenterWorld(
                new Vector3Int(x, ladderYMax.Value, 0)
            );

            PlatformNode bottomGroundNode = FindNearestGroundNodeBelow(
                bottomWorld,
                _ladderSnapDistance
            );
            PlatformNode topGroundNode = FindNearestGroundNodeAbove(topWorld, _ladderSnapDistance);

            if (bottomGroundNode == null || topGroundNode == null)
                continue;
            if (bottomGroundNode == topGroundNode)
                continue;

            if (!bottomGroundNode.neighbors.Contains(topGroundNode))
                bottomGroundNode.neighbors.Add(topGroundNode);

            if (!topGroundNode.neighbors.Contains(bottomGroundNode))
                topGroundNode.neighbors.Add(bottomGroundNode);
        }
    }

    private void ConnectJumpableNeighbors()
    {
        for (int i = 0; i < AllNodes.Count; i++)
        {
            for (int j = i + 1; j < AllNodes.Count; j++)
            {
                PlatformNode a = AllNodes[i];
                PlatformNode b = AllNodes[j];

                if (a.type != NodeType.Ground || b.type != NodeType.Ground)
                    continue;

                float xDist = Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
                float yDist = Mathf.Abs(a.worldPosition.y - b.worldPosition.y);

                // Must differ in both axes — same-Y nodes are walk edges, same-X are ladder edges.
                if (yDist < 0.1f || xDist < 0.1f)
                    continue;

                if (xDist <= _jumpMaxHorizontalDist && yDist <= _jumpMaxVerticalDist)
                {
                    if (!a.jumpNeighbors.Contains(b))
                        a.jumpNeighbors.Add(b);
                    if (!b.jumpNeighbors.Contains(a))
                        b.jumpNeighbors.Add(a);
                }
            }
        }
    }

    private PlatformNode FindNearestGroundNodeBelow(Vector3 worldPos, float maxDist)
    {
        PlatformNode closest = null;
        float closestDist = maxDist;

        foreach (var node in AllNodes)
        {
            if (node.type != NodeType.Ground)
                continue;
            if (node.worldPosition.y > worldPos.y + 0.01f) // must be below or level
                continue;

            float dist = Vector2.Distance(node.worldPosition, worldPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    private PlatformNode FindNearestGroundNodeAbove(Vector3 worldPos, float maxDist)
    {
        PlatformNode closest = null;
        float closestDist = maxDist;

        foreach (var node in AllNodes)
        {
            if (node.type != NodeType.Ground)
                continue;
            if (node.worldPosition.y < worldPos.y - 0.01f) // must be above or level
                continue;

            float dist = Vector2.Distance(node.worldPosition, worldPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    #endregion

    #region Editor Visualisation

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || AllNodes == null)
            return;

        foreach (var node in AllNodes)
        {
            Gizmos.color = node.type == NodeType.Ground ? _groundNodeColor : _ladderNodeColor;
            Gizmos.DrawSphere(node.worldPosition, 0.12f);

            Gizmos.color = _edgeColor;
            foreach (var neighbor in node.neighbors)
                Gizmos.DrawLine(node.worldPosition, neighbor.worldPosition);

            Gizmos.color = _jumpEdgeColor;
            foreach (var neighbor in node.jumpNeighbors)
                Gizmos.DrawLine(node.worldPosition, neighbor.worldPosition);
        }
    }

    #endregion
}
