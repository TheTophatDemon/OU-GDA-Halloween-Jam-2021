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

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        grid = GetComponent<Grid>();

        prefabs = Resources.LoadAll<GameObject>("Map Components");
        
        cels = new int[columns, rows];

        for (int y = 0; y < rows; ++y)
        {
            for (int x = 0; x < columns; ++x)
            {
                // cels[x, y] = Random.Range(-1, prefabs.Length);
                cels[x, y] = -1;
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
    
    //Creates a visible mesh based off of the tiles
    public void GenerateGeometry() 
    {
        Mesh mesh = new Mesh();

        //Merge prefabs into the mesh for each non-empty tile
        var combines = new List<CombineInstance>();
        for (var y = 0; y < rows; ++y) 
        {
            for (var x = 0; x < columns; ++x)
            {
                if (cels[x, y] >= 0)
                {
                    var prefab = prefabs[cels[x, y]];
                    var combine = new CombineInstance();
                    combine.mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                    prefab.transform.position = grid.CellToWorld(new Vector3Int(x, 0, y));
                    combine.transform = prefab.transform.localToWorldMatrix;
                    combines.Add(combine);
                }
            }
        }
        mesh.CombineMeshes(combines.ToArray(), true, true, false);
        
        //TODO: Support for multiple submeshes?

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
