using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Generates a maze, sets the GridMap to match the maze, and spawns game objects. </summary>
[RequireComponent(typeof(GridMap))]
[RequireComponent(typeof(Grid))]
public class MazeGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject monsterPrefab;
    public GameObject keyPrefab;

    private GridMap gridMap;
    private Grid grid;

    ///<summary> Tracks which cels have been visited by the algorithm and from where. </summary>
    private GridMap.Openings[,] visitMap;

    void Start()
    {
        gridMap = GetComponent<GridMap>();
        grid = GetComponent<Grid>();
        
        visitMap = new GridMap.Openings[gridMap.Columns, gridMap.Rows];

        //Keeps track of exploration history. Most recently visited point is on top.
        var navStack = new Stack<Vector2Int>();
        //Start at (0, 0)
        navStack.Push(Vector2Int.zero);

        var dirChoices = new List<Vector2Int>();
        while (navStack.Count > 0)
        {
            Vector2Int from = navStack.Peek();

            dirChoices.Clear();
            // Explore a neighbor if it hasn't been visited yet.
            if (from.x > 0 && visitMap[from.x - 1, from.y] == GridMap.Openings.Closed) dirChoices.Add(Vector2Int.left);
            if (from.x < gridMap.Columns - 1 && visitMap[from.x + 1, from.y] == GridMap.Openings.Closed) dirChoices.Add(Vector2Int.right);
            if (from.y > 0 && visitMap[from.x, from.y - 1] == GridMap.Openings.Closed) dirChoices.Add(Vector2Int.down);
            if (from.y < gridMap.Rows - 1 && visitMap[from.x, from.y + 1] == GridMap.Openings.Closed) dirChoices.Add(Vector2Int.up);

            if (dirChoices.Count > 0)
            {
                // Pick a random unexplored neighbor and open it in the direction of the current cel.
                int i = Random.Range(0, dirChoices.Count);
                Vector2Int to = from + dirChoices[i];

                GridMap.Openings fromState = visitMap[from.x, from.y];
                GridMap.Openings toState = visitMap[to.x, to.y];

                if (to.y - from.y > 0)
                {
                    fromState |= GridMap.Openings.North;
                    toState |= GridMap.Openings.South;
                }
                else if (from.y - to.y > 0)
                {
                    fromState |= GridMap.Openings.South;
                    toState |= GridMap.Openings.North;
                }
                if (to.x - from.x > 0)
                {
                    fromState |= GridMap.Openings.East;
                    toState |= GridMap.Openings.West;
                }
                else if (from.x - to.x > 0)
                {
                    fromState |= GridMap.Openings.West;
                    toState |= GridMap.Openings.East;
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
        for (int y = 0; y < gridMap.Rows; ++y)
        {
            for (int x = 0; x < gridMap.Columns; ++x)
            {
                gridMap.SetCelOpenings(x, y, visitMap[x, y]);
                
                //Also keep track of dead ends so we can place objects
                if (gridMap.IsCelDeadEnd(x, y))
                {
                    spawnPoints.Add(new Vector3Int(x, 0, y));
                }
            }
        }

        gridMap.GenerateGeometry();

        // Spawn the player at a random dead end.
        int playerIdx = Random.Range(0, spawnPoints.Count);

        // Choose an angle that faces one of the cel's entrances
        float playerAngle = 0.0f;
        int celX = spawnPoints[playerIdx].x;
        int celY = spawnPoints[playerIdx].z;
        GridMap.Openings op = visitMap[celX, celY];
        if (gridMap.IsCelOpen(celX, celY, GridMap.Openings.East))
        {
            playerAngle = 90.0f;
        }
        else if (gridMap.IsCelOpen(celX, celY, GridMap.Openings.South))
        {
            playerAngle = 180.0f;
        }
        else if (gridMap.IsCelOpen(celX, celY, GridMap.Openings.West))
        {
            playerAngle = 270.0f;
        }

        GameObject playerObject = Instantiate(playerPrefab, grid.CellToWorld(spawnPoints[playerIdx]), Quaternion.identity);
        // Turn the player's camera to face the desired direction.
        playerObject.GetComponent<PlayerMovement>().Yaw = playerAngle;

        // Spawn the monster at another dead end.
        int monsterIdx = Random.Range(0, spawnPoints.Count - 1);
        if (monsterIdx >= playerIdx) ++monsterIdx; //Make sure the monster isn't in the same one as the player
        Instantiate(monsterPrefab, grid.CellToWorld(spawnPoints[monsterIdx]), Quaternion.identity);

        // Spawn keys at remaining dead ends.
        for (int i = 0; i < spawnPoints.Count; ++i)
        {
            if (i == monsterIdx || i == playerIdx) continue;
            Instantiate(keyPrefab, grid.CellToWorld(spawnPoints[i]), Quaternion.identity);
        }
    }
}
