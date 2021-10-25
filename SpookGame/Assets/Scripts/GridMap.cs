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
}
