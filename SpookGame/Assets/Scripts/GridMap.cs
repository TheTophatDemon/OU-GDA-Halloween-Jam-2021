using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class GridMap : MonoBehaviour
{
    public int rows = 32;
    public int columns = 32;

    public GameObject[] prefabs;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Grid grid;
    
    //Grid of indicies into the prefabs array. -1 for empty tile.
    private int[,] cels;
    private PathNode[,] pathNodes;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        grid = GetComponent<Grid>();

        prefabs = Resources.LoadAll<GameObject>("Map Components");
        
        cels = new int[columns, rows];
        pathNodes = new PathNode[columns, rows];

        for (int y = 0; y < rows; ++y)
        {
            for (int x = 0; x < columns; ++x)
            {
                // cels[x, y] = Random.Range(-1, prefabs.Length);
                cels[x, y] = -1;
                pathNodes[x, y] = new PathNode(new Vector3Int(x, 0, y));
            }
        }

        // GenerateGeometry();
    }

    public void SetCel(int x, int y, int cel) 
    {
        cels[x, y] = cel;
    }

    public void SetCel(int x, int y, string prefabName)
    {
        for(int i = 0; i < prefabs.Length; ++i)
        {
            if (prefabs[i].name.Equals(prefabName))
            {
                cels[x, y] = i;
                return;
            }
        }
        cels[x, y] = -1;
    }

    public int GetCel(Vector3Int vec)
    {
        return GetCel(vec.x, vec.z);
    }

    public int GetCel(int x, int y)
    {
        return cels[x, y];
    }

    public string GetCelName(int x, int y)
    {
        return prefabs[cels[x, y]].name;
    }
    
    public MazeGenerator.CelState GetCelState(Vector3Int vec)
    {
        return GetCelState(vec.x, vec.z);
    }

    public MazeGenerator.CelState GetCelState(int x, int y)
    {
        if (x < 0 || y < 0 || x >= columns || y >= rows) return MazeGenerator.CelState.Unvisited;
        var state = MazeGenerator.CelStateFromMapComponent(prefabs[cels[x, y]].GetComponent<MapComponent>());
        if (x == 0) state &= (~MazeGenerator.CelState.OpenWest);
        if (x == columns - 1) state &= (~MazeGenerator.CelState.OpenEast);
        if (y == 0) state &= (~MazeGenerator.CelState.OpenNorth);
        if (y == rows - 1) state &= (~MazeGenerator.CelState.OpenSouth);
        return state;
    }

    //Creates a visible mesh based off of the tiles
    public void GenerateGeometry() 
    {
        var bigMesh = new Mesh();
        var subMeshData = new List<(Mesh, List<CombineInstance>)>();

        for (var y = 0; y < rows; ++y) 
        {
            for (var x = 0; x < columns; ++x)
            {
                if (cels[x, y] >= 0)
                {
                    var prefab = prefabs[cels[x, y]];
                    prefab.transform.position = grid.CellToWorld(new Vector3Int(x, 0, y));
                    var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                    for (int i = 0; i < mesh.subMeshCount; ++i)
                    {
                        if (i >= subMeshData.Count)
                        {
                            subMeshData.Add((new Mesh(), new List<CombineInstance>()));
                        }

                        var combine = new CombineInstance();
                        combine.mesh = mesh;
                        combine.transform = prefab.transform.localToWorldMatrix;
                        combine.subMeshIndex = i;
                        subMeshData[i].Item2.Add(combine);
                    }
                }
            }
        }
        
        var subCombines = new List<CombineInstance>();
        foreach ((Mesh m, List<CombineInstance> ci) in subMeshData)
        {
            m.CombineMeshes(ci.ToArray(), true, true, false);

            var combine = new CombineInstance();
            combine.mesh = m;
            subCombines.Add(combine);
        }
        bigMesh.CombineMeshes(subCombines.ToArray(), false, false, false);

        meshFilter.mesh = bigMesh;
        meshCollider.sharedMesh = bigMesh;
    }

    private class PathNode : System.IComparable<PathNode>
    {
        public Vector3Int pos;
        public float distFromStart;
        public float distFromGoal;
        public float combinedDist;
        public PathNode parent;
        public bool inOpen;
        public bool inClosed;

        public PathNode(Vector3Int pos)
        {
            this.pos = pos;
        }

        public int CompareTo(PathNode other)
        {
            return (int) Mathf.Sign(combinedDist - other.combinedDist);
        }
    }

    public List<Vector3> GetPath(Vector3 start, Vector3 end) 
    {
        Vector3Int celStart = grid.WorldToCell(start + (grid.cellSize / 2.0f));
        celStart.y = 0;
        Vector3Int celEnd = grid.WorldToCell(end + (grid.cellSize / 2.0f));
        celEnd.y = 0;

        //Clear the pathfinding data from the last time
        for (int y = 0; y < rows; ++y)
        {
            for (int x = 0; x < columns; ++x)
            {
                pathNodes[x, y].distFromStart = 0.0f;
                pathNodes[x, y].distFromGoal = 0.0f;
                pathNodes[x, y].combinedDist = 0.0f;
                pathNodes[x, y].parent = null;
                pathNodes[x, y].inOpen = false;
                pathNodes[x, y].inClosed = false;
            }
        }
        var path = new List<Vector3>();

        var open = new SortedList<PathNode, PathNode>(); //Apparently C# doesn't have a priority queue yet...
        var closed = new List<PathNode>();

        if (celStart.x < 0 || celStart.x >= columns || celEnd.x < 0 || celEnd.x >= columns
            || celStart.z < 0 || celStart.z >= rows || celEnd.z < 0 || celEnd.z >= rows)
        {
            return path;
        }

        var startNode = pathNodes[celStart.x, celStart.z];
        startNode.distFromGoal = (celEnd - celStart).magnitude;
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
                var nodeComponent = prefabs[cels[node.pos.x, node.pos.z]].GetComponent<MapComponent>();
                searchDirs.Clear();
                if (nodeComponent.openEast) searchDirs.Add(Vector3Int.right);
                if (nodeComponent.openWest) searchDirs.Add(Vector3Int.left);
                if (nodeComponent.openNorth) searchDirs.Add(Vector3Int.forward);
                if (nodeComponent.openSouth) searchDirs.Add(Vector3Int.back);
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
                        newNode.distFromGoal = distFromGoal;
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
