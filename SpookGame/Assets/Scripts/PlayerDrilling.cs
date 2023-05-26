using UnityEngine;

/// <summary> Controls how the player is able to drill through walls. </summary>
public class PlayerDrilling : MonoBehaviour
{
    public GameObject dustParticlePrefab;

    private GameObject cameraObject;

    ///<summary> The debug sphere is used to visually confirm that the drilling will happen in the right cel. Should be invisible in final game.</summary>
    private GameObject debugSphere;
    private AudioSource drillSound;
    private AudioSource wallBreakSound;

    private GridMap gridMap;
    private Grid grid;

    public bool drilling = false;
    public float drillingProgress = 0.0f;
    public float drillingSpeed = 0.25f;

    void Start()
    {
        cameraObject = transform.Find("Camera").gameObject;
        debugSphere = transform.Find("DebugSphere").gameObject;
        debugSphere.transform.parent = null;

        drillSound = GetComponent<AudioSource>();

        var mapObject = GameObject.FindWithTag("Map");
        grid = mapObject.GetComponent<Grid>();
        gridMap = mapObject.GetComponent<GridMap>();

        var wallBreakObj = transform.Find("WallBreakSound");
        wallBreakSound = wallBreakObj.GetComponent<AudioSource>();
    }

    void Update()
    {
        // Determine grid coordinates for where the player is and where the player is facing.
        Vector3Int from = grid.WorldToCell(transform.position + grid.cellSize / 2.0f);
        int fromX = from.x;
        int fromY = from.z;
        int toX = from.x;
        int toY = from.z;

        // Calculate the cel the drill is pointing towards by comparing the player's direction to the global forward vector.
        Vector3 direction = cameraObject.transform.TransformDirection(Vector3.forward);
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0.7f)
            {
                toX += 1;
            }
            else if (direction.x < -0.7f)
            {
                toX -= 1;
            }
        }
        else
        {
            if (direction.z > 0.7f)
            {
                toY += 1;
            }
            else if (direction.z < -0.7f)
            {
                toY -= 1;
            }
        }

        debugSphere.transform.position = grid.CellToWorld(new Vector3Int(toX, 0, toY));

        drilling = false;
        if (Input.GetAxis("Fire1") > 0.0f)
        {
            // Allow the player to drill when there is a wall directly in front of them.
            RaycastHit hit;
            var ray = new Ray(cameraObject.transform.position, cameraObject.transform.forward);
            
            if (Physics.Raycast(ray, out hit, 1.5f, LayerMask.GetMask("Cave")))
            {
                if (gridMap.CanDrill(fromX, fromY, toX, toY)) drilling = true;
            }
        }

        if (drilling)
        {   
            //if (!drillSound.isPlaying) Debug.Log($"Drilling wall from ({fromX}, {fromY}) to ({toX}, {toY}).");
            
            float damage = gridMap.Drill(fromX, fromY, toX, toY, Time.deltaTime * drillingSpeed);
            drillingProgress = damage;
            if (damage >= GridMap.MAX_DAMAGE)
            {
                Debug.Log($"Destroyed wall from ({fromX}, {fromY}) to ({toX}, {toY}).");
                wallBreakSound.Play();
                drillingProgress = 0.0f;
                // Spawn particle effect
                Instantiate(dustParticlePrefab, grid.CellToWorld(new Vector3Int(toX, 0, toY)), Quaternion.identity);
                Instantiate(dustParticlePrefab, grid.CellToWorld(new Vector3Int(fromX, 0, fromY)), Quaternion.identity);
            }

            // Play the drilling sound whilst drilling
            if (!drillSound.isPlaying)
            {
                drillSound.Play();
            }
        }
        else
        {
            drillSound.Stop();
            drillingProgress = 0.0f;
        }
    }
}