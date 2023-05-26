using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(GridMap))]
public class Pathfinder : MonoBehaviour
{
    /// <summary> This class represents the pathfinding information for each cel. </summary>
    private class PathNode : System.IComparable<PathNode>
    {
        public Vector3Int pos = Vector3Int.zero;

        /// <summary> The cumulative distance traveled during the algorithm to get to this node. </summary>
        public float distFromStart = 0.0f;

        /// <summary> 
        /// This represents the A* pathfinding function h(x) + f(x):
        /// the sum of the cost of the path up to this node and the estimated cost left until the end.
        /// </summary>
        public float combinedDist = 0.0f;

        /// <summary> This is the node visited before this node was visited. The parent chain is used to determine the final path. </summary>
        public PathNode parent = null;

        /// <summary> This variable is an optimization so that the open list does not have to be searched repeatedly. </summary>
        public bool inOpen = false;
        /// <summary> This variable is an optimization so that the closed list does not have to be searched repeatedly. </summary>
        public bool inClosed = false;

        /// <summary> Sorts nodes such that the lowest combined distance comes first. </summary>
        public int CompareTo(PathNode other)
        {
            return (int) Mathf.Sign(combinedDist - other.combinedDist);
        }
    }

    private Grid grid;
    private GridMap gridMap = null;

    private PathNode[,] pathNodes;

    void Start()
    {
        grid = GetComponent<Grid>();
        gridMap = GetComponent<GridMap>();

        pathNodes = new PathNode[gridMap.Columns, gridMap.Rows];
        for (int y = 0; y < gridMap.Rows; ++y)
        {
            for (int x = 0; x < gridMap.Columns; ++x)
            {
                pathNodes[x, y] = new PathNode();
            }
        }
    }

    /// <summary> Returns a list of target positions for the optimal path between the vectors <c>start</c> and <c>end</c>. </summary>
    public List<Vector3> GetPath(Vector3 start, Vector3 end) 
    {
        if (gridMap == null) return new List<Vector3>();

        Vector3Int celStart = grid.WorldToCell(start + (grid.cellSize / 2.0f));
        celStart.y = 0;
        Vector3Int celEnd = grid.WorldToCell(end + (grid.cellSize / 2.0f));
        celEnd.y = 0;

        // Initialize a new grid of pathfinding nodes for this search.
        for (int y = 0; y < gridMap.Rows; ++y)
        {
            for (int x = 0; x < gridMap.Columns; ++x)
            {
                pathNodes[x, y].pos = new Vector3Int(x, 0, y);
                pathNodes[x, y].distFromStart = 0.0f;
                pathNodes[x, y].combinedDist = 0.0f;
                pathNodes[x, y].parent = null;
                pathNodes[x, y].inOpen = false;
                pathNodes[x, y].inClosed = false;
            }
        }
        var path = new List<Vector3>();

        var open = new SortedList<PathNode, PathNode>(); //Apparently C# doesn't have a priority queue yet...
        var closed = new List<PathNode>();

        if (celStart.x < 0 || celStart.x >= gridMap.Columns || celEnd.x < 0 || celEnd.x >= gridMap.Columns
            || celStart.z < 0 || celStart.z >= gridMap.Rows || celEnd.z < 0 || celEnd.z >= gridMap.Rows)
        {
            return path;
        }

        var startNode = pathNodes[celStart.x, celStart.z];
        startNode.inOpen = true;
        open.Add(startNode, startNode);

        var searchDirs = new List<Vector3Int>();

        PathNode node = null;
        while (open.Count > 0)
        {
            node = open.Keys[0];
            open.RemoveAt(0);
            node.inOpen = false;
            node.inClosed = true;
            closed.Add(node);

            if (node.pos.x == celEnd.x && node.pos.z == celEnd.z)
            {
                break;
            }
            else
            {
                //Analyze neighbors
                searchDirs.Clear();
                if (gridMap.IsCelOpen(node.pos.x, node.pos.z, GridMap.Openings.East))  searchDirs.Add(Vector3Int.right);
                if (gridMap.IsCelOpen(node.pos.x, node.pos.z, GridMap.Openings.West))  searchDirs.Add(Vector3Int.left);
                if (gridMap.IsCelOpen(node.pos.x, node.pos.z, GridMap.Openings.North)) searchDirs.Add(Vector3Int.forward);
                if (gridMap.IsCelOpen(node.pos.x, node.pos.z, GridMap.Openings.South)) searchDirs.Add(Vector3Int.back);
                foreach(Vector3Int dir in searchDirs)
                {
                    var newPos = node.pos + dir;

                    //Debug.DrawLine(grid.CellToWorld(node.pos), grid.CellToWorld(newPos), Color.magenta, 0.1f);

                    var newNode = pathNodes[newPos.x, newPos.z];
                    var distFromStart = node.distFromStart + 1.0f;
                    var distFromGoal = (celEnd - newPos).magnitude;
                    var combinedDist = distFromStart + distFromGoal;
                    
                    if (!newNode.inClosed || distFromStart < newNode.distFromStart)
                    {
                        newNode.distFromStart = distFromStart;
                        newNode.combinedDist = combinedDist;
                        newNode.parent = node;
                        
                        if (newNode.inOpen)
                        {
                            open.Remove(newNode);
                        }
                        if (newNode.inClosed)
                        {
                            closed.Remove(newNode);
                            newNode.inClosed = false;
                        }
                        newNode.inOpen = true;
                        open.Add(newNode, newNode);
                    }
                }
            }
        }

        //Construct path from parent chain
        PathNode it = node;
        while (it != startNode && it != null)
        {
            path.Insert(0, grid.CellToWorld(it.pos));
            
            if (it.parent != null) 
            {
                Debug.DrawLine(grid.CellToWorld(it.pos), grid.CellToWorld(it.parent.pos), Color.magenta, 0.1f);
                //Debug.Log("Drawing line from " + it.pos.ToString() + " to " + it.parent.pos.ToString());
            }
            
            it = it.parent;
        }

        //Debug.DrawLine(start, start + Vector3.up * 10.0f, Color.magenta, 10.0f);
       // Debug.DrawLine(end, end + Vector3.up * 10.0f, Color.magenta, 10.0f);

        return path;
    }
}
