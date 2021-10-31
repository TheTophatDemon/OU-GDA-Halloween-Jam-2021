using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridMap))]
[RequireComponent(typeof(Grid))]
public class MazeGenerator : MonoBehaviour
{
    private static readonly Dictionary<CelState, string> PREFAB_MAP = new Dictionary<CelState, string>() 
    {
        {CelState.Unvisited, "Block"},
        {CelState.OpenNorth, "U Piece N"},
        {CelState.OpenSouth, "U Piece S"},
        {CelState.OpenEast, "U Piece E"},
        {CelState.OpenWest, "U Piece W"},
        {CelState.OpenNorth | CelState.OpenSouth, "Hall Piece NS"},
        {CelState.OpenEast | CelState.OpenWest, "Hall Piece EW"},
        {CelState.OpenNorth | CelState.OpenEast, "Turn Piece NE"},
        {CelState.OpenNorth | CelState.OpenWest, "Turn Piece NW"},
        {CelState.OpenSouth | CelState.OpenEast, "Turn Piece ES"},
        {CelState.OpenSouth | CelState.OpenWest, "Turn Piece SW"},
        {CelState.OpenNorth | CelState.OpenEast | CelState.OpenWest, "T Piece NEW"},
        {CelState.OpenSouth | CelState.OpenEast | CelState.OpenWest, "T Piece ESW"},
        {CelState.OpenEast | CelState.OpenNorth | CelState.OpenSouth, "T Piece NES"},
        {CelState.OpenWest | CelState.OpenNorth | CelState.OpenSouth, "T Piece NSW"},
        {CelState.OpenNorth | CelState.OpenSouth | CelState.OpenEast | CelState.OpenWest, "Cross Piece"}
    };

    [System.Flags]
    private enum CelState
    {
        Unvisited = 0,
        OpenNorth = 0b0001,
        OpenEast = 0b0010,
        OpenSouth = 0b0100,
        OpenWest = 0b1000
    }

    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    private GridMap gridMap;
    private Grid grid;

    private CelState[,] visitMap;

    void Start()
    {
        gridMap = GetComponent<GridMap>();
        grid = GetComponent<Grid>();

        visitMap = new CelState[gridMap.columns, gridMap.rows];

        //Keeps track of exploration history. Most recently visited point is on top.
        var navStack = new Stack<Vector2Int>();
        //Start at (0, 0)
        navStack.Push(Vector2Int.zero);

        var dirChoices = new List<Vector2Int>();
        while (navStack.Count > 0)
        {
            Vector2Int from = navStack.Peek();

            dirChoices.Clear();
            if (from.x > 0 && visitMap[from.x - 1, from.y] == CelState.Unvisited) dirChoices.Add(Vector2Int.left);
            if (from.x < gridMap.columns - 1 && visitMap[from.x + 1, from.y] == CelState.Unvisited) dirChoices.Add(Vector2Int.right);
            if (from.y > 0 && visitMap[from.x, from.y - 1] == CelState.Unvisited) dirChoices.Add(Vector2Int.down);
            if (from.y < gridMap.rows - 1 && visitMap[from.x, from.y + 1] == CelState.Unvisited) dirChoices.Add(Vector2Int.up);

            if (dirChoices.Count > 0)
            {
                int i = Random.Range(0, dirChoices.Count);
                Vector2Int to = from + dirChoices[i];

                CelState fromState = visitMap[from.x, from.y];
                CelState toState = visitMap[to.x, to.y];

                if (to.y - from.y > 0)
                {
                    fromState |= CelState.OpenNorth;
                    toState |= CelState.OpenSouth;
                }
                else if (from.y - to.y > 0)
                {
                    fromState |= CelState.OpenSouth;
                    toState |= CelState.OpenNorth;
                }
                if (to.x - from.x > 0)
                {
                    fromState |= CelState.OpenEast;
                    toState |= CelState.OpenWest;
                }
                else if (from.x - to.x > 0)
                {
                    fromState |= CelState.OpenWest;
                    toState |= CelState.OpenEast;
                }

                visitMap[from.x, from.y] = fromState;
                visitMap[to.x, to.y] = toState;

                navStack.Push(to);
            }
            else
            {
                navStack.Pop();
            }

        }

        var spawnPoints = new List<Vector3Int>();

        //Now take the generated cel states and turn them into prefabs for map components with matching entrances
        for (int y = 0; y < gridMap.rows; ++y)
        {
            for (int x = 0; x < gridMap.columns; ++x)
            {
                gridMap.SetCel(x, y, PREFAB_MAP[visitMap[x, y]]);
                
                //Also keep track of dead ends so we can place objects
                if (PREFAB_MAP[visitMap[x, y]].Contains("U Piece"))
                {
                    spawnPoints.Add(new Vector3Int(x, 0, y));
                }
            }
        }

        gridMap.GenerateGeometry();

        //Spawn the player at a random dead end.
        int playerIdx = Random.Range(0, spawnPoints.Count);
        Instantiate(playerPrefab, grid.CellToWorld(spawnPoints[playerIdx]), Quaternion.identity);

        //TODO: Make rotation face the right way

        //Spawn the monster at another dead end.
        int monsterIdx = Random.Range(0, spawnPoints.Count - 1);
        if (monsterIdx >= playerIdx) ++monsterIdx; //Make sure the monster isn't in the same one as the player
        Instantiate(monsterPrefab, grid.CellToWorld(spawnPoints[monsterIdx]), Quaternion.identity);
    }
}
