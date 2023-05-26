using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary> 
/// This represents the game's map as a grid of cels with entrances in 1 of 4 directions.
/// It handles mesh generation and deformation by drilling.
/// </summary>
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class GridMap : MonoBehaviour
{

    /// <summary> Represents whether the tunnel going through this cel opens in any of the cardinal directions. </summary>
    [System.Flags]
    public enum Openings
    {
        Closed = 0,
        North = 0b0001,
        East = 0b0010,
        South = 0b0100,
        West = 0b1000
    }

    private struct Cel 
    {
        public Openings openings;

        /// <summary>
        /// For each grid cel, there is a float value that increases when a wall is drilled.
        /// When it exceeds 1.0f, the wall is destroyed, and the cel's model is changed to reflect that.
        /// </summary>
        public float damage;
    }

    public const float MAX_DAMAGE = 1.0f;

    /// <summary> Gives one of the prefab map pieces that open into the given directions. </summary>
    private Dictionary<Openings, GameObject> prefabMap;

    // Grid dimensions
    [SerializeField]
    private int rows;
    [SerializeField]
    private int cols;
    public int Rows { get => rows; }
    public int Columns { get => cols; }

    private Cel[,] cels;

    // Unity components.
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Grid grid;

    void Awake()
    {
        // Load all the map pieces, categorized by their openings. 
        prefabMap = new Dictionary<Openings, GameObject>() 
        {
            {Openings.Closed,                                                 Resources.Load<GameObject>("Map Components/Block")},
            {Openings.North,                                                  Resources.Load<GameObject>("Map Components/U Piece N")},
            {Openings.South,                                                  Resources.Load<GameObject>("Map Components/U Piece S")},
            {Openings.East,                                                   Resources.Load<GameObject>("Map Components/U Piece E")},
            {Openings.West,                                                   Resources.Load<GameObject>("Map Components/U Piece W")},
            {Openings.North | Openings.South,                                 Resources.Load<GameObject>("Map Components/Hall Piece NS")},
            {Openings.East  | Openings.West,                                  Resources.Load<GameObject>("Map Components/Hall Piece EW")},
            {Openings.North | Openings.East,                                  Resources.Load<GameObject>("Map Components/Turn Piece NE")},
            {Openings.North | Openings.West,                                  Resources.Load<GameObject>("Map Components/Turn Piece NW")},
            {Openings.South | Openings.East,                                  Resources.Load<GameObject>("Map Components/Turn Piece ES")},
            {Openings.South | Openings.West,                                  Resources.Load<GameObject>("Map Components/Turn Piece SW")},
            {Openings.North | Openings.East  | Openings.West,                 Resources.Load<GameObject>("Map Components/T Piece NEW")},
            {Openings.South | Openings.East  | Openings.West,                 Resources.Load<GameObject>("Map Components/T Piece ESW")},
            {Openings.East  | Openings.North | Openings.South,                Resources.Load<GameObject>("Map Components/T Piece NES")},
            {Openings.West  | Openings.North | Openings.South,                Resources.Load<GameObject>("Map Components/T Piece NSW")},
            {Openings.North | Openings.South | Openings.East | Openings.West, Resources.Load<GameObject>("Map Components/Cross Piece")}
        };

        cels = new Cel[Columns, Rows];
    }

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        grid = GetComponent<Grid>();
    }

    /// <summary>
    /// Returns true if the cel at (toX, toY) can be drilled from (fromX, fromY).
    /// </summary>
    public bool CanDrill(int fromX, int fromY, int toX, int toY) 
    {
        // Don't allow drilling diagonally or from within the same cel.
        if (toX == fromX && toY == fromY) return false;
        if (toX != fromX && toY != fromY) return false;
        // Don't allow drilling out of bounds.
        if (toX < 0 || toY < 0 || toX >= cols || toY >= rows) return false;
        if (fromY == toY)
        {
            // Drilling horizontally
            if (toX > fromX)
            {
                return !IsCelOpen(fromX, fromY, Openings.East) || !IsCelOpen(toX, toY, Openings.West);
            }
            else
            {
                return !IsCelOpen(fromX, fromY, Openings.West) || !IsCelOpen(toX, toY, Openings.East);
            }
        }
        else
        {
            // Drilling vertically
            if (toY > fromY)
            {
                return !IsCelOpen(fromX, fromY, Openings.North) || !IsCelOpen(toX, toY, Openings.South);
            }
            else
            {
                return !IsCelOpen(fromX, fromY, Openings.South) || !IsCelOpen(toX, toY, Openings.North);
            }
        }
    }

    /// <summary>
    /// Increases the cel's damage value at (toX, toY).
    /// Returns the drilled wall's damage value, which will be equal to MAX_DAMAGE when the wall has just been destroyed.
    /// </summary>
    /// <param name="fromX"> X position of the cel the drill starts in. Used to determine which wall to destroy.</param>
    /// <param name="fromY"> Y position of the cel the drill starts in. Used to determine which wall to destroy.</param>
    /// <param name="damage"> The amount to add to the wall's damage value. </param>
    public float Drill(int fromX, int fromY, int toX, int toY, float damage) 
    {
        // Don't allow drilling diagonally or from within the same cel.
        if (toX == fromX && toY == fromY) return 0.0f;
        if (toX != fromX && toY != fromY) return 0.0f;

        cels[toX, toY].damage += damage;
        if (cels[toX, toY].damage >= MAX_DAMAGE)
        {
            cels[toX, toY].damage = 0.0f;
            
            if (fromY == toY)
            {
                // Drilling horizontally
                if (toX > fromX)
                {
                    cels[fromX, fromY].openings |= Openings.East;
                    cels[toX, toY].openings |= Openings.West;
                }
                else
                {
                    cels[fromX, fromY].openings |= Openings.West;
                    cels[toX, toY].openings |= Openings.East;
                }
            }
            else
            {
                // Drilling vertically
                if (toY > fromY)
                {
                    cels[fromX, fromY].openings |= Openings.North;
                    cels[toX, toY].openings |= Openings.South;
                }
                else
                {
                    cels[fromX, fromY].openings |= Openings.South;
                    cels[toX, toY].openings |= Openings.North;
                }
            }

            GenerateGeometry();

            return MAX_DAMAGE;
        }
        return cels[toX, toY].damage;
    }

    /// <summary> Returns true if the cel at (x, y) has the openings described by op (bitmask matching). </summary>
    public bool IsCelOpen(int x, int y, Openings op)
    {
        return (cels[x, y].openings & op) == op;
    }

    /// <summary> Returns true if the cel at (x, y) is a dead end (a "U" piece). </summary>
    public bool IsCelDeadEnd(int x, int y) 
    {
        return prefabMap[cels[x, y].openings].name.Contains("U Piece");
    }

    /// <summary> Sets the cell at (x, y) to have only the specified openings. </summary>
    public void SetCelOpenings(int x, int y, Openings op) 
    {
        cels[x, y].openings = op;
    }

    /// <summary> Creates a visible mesh based off of the tiles. </summary>
    public void GenerateGeometry() 
    {
        // The final combined mesh.
        var bigMesh = new Mesh();

        // All of the groups of the same submeshe index are combined together first
        // before the submeshes themselves are combined. This is done in order
        // to prevent the mysterious CombineMeshes function from throwing away
        // the material index information.  

        // This dictionary collects submeshes to be combined, grouped by their material.
        var subMeshData = new Dictionary<Material, List<CombineInstance>>();

        for (var y = 0; y < Rows; ++y) 
        {
            for (var x = 0; x < Columns; ++x)
            {
                GameObject prefab = prefabMap[cels[x, y].openings];
                
                // Calculate the map piece's world position
                prefab.transform.position = grid.CellToWorld(new Vector3Int(x, 0, y));
                
                // Add the prefab's submeshes to their respective CombineInstance lists.
                var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                var meshRender = prefab.GetComponent<MeshRenderer>();
                for (int i = 0; i < mesh.subMeshCount; ++i)
                {
                    var combine = new CombineInstance();
                    combine.mesh = mesh;
                    combine.transform = prefab.transform.localToWorldMatrix;
                    combine.subMeshIndex = i;
                    if (!subMeshData.ContainsKey(meshRender.sharedMaterials[i])) 
                    {
                        subMeshData[meshRender.sharedMaterials[i]] = new List<CombineInstance>();
                    }
                    subMeshData[meshRender.sharedMaterials[i]].Add(combine);
                }
            }
        }
        
        // Now combine the aggregate meshes for each submesh index.
        var subCombines = new List<CombineInstance>();
        foreach ((Material mat, List<CombineInstance> ci) in subMeshData)
        {
            // Combine each list of CombineInstances into a single mesh.
            var m = new Mesh();
            m.CombineMeshes(ci.ToArray(), true, true, false);

            var combine = new CombineInstance();
            combine.mesh = m;
            subCombines.Add(combine);
        }     

        // Combine into the final mesh
        bigMesh.CombineMeshes(subCombines.ToArray(), false, false, false);

        meshFilter.mesh = bigMesh;
        meshCollider.sharedMesh = bigMesh;
    }
}
