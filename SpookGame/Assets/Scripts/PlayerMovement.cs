using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    public float walkSpeed = 10.0f;

    private Vector3 velocity = Vector3.zero;
    private GameObject cameraObject;
    private new Camera camera;
    private float cameraYaw;
    private AudioSource drillSound;
    private AudioSource wallBreakSound;

    private GridMap gridMap;
    private Grid grid;

    public bool drilling = false;
    private Vector3Int drillingCel = Vector3Int.zero;
    public float drillingProgress = 0.0f;
    public float drillingSpeed = 0.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraObject = transform.Find("Camera").gameObject;
        camera = cameraObject.GetComponent<Camera>();
        cameraYaw = cameraObject.transform.localRotation.eulerAngles.y;

        drillSound = GetComponent<AudioSource>();

        var mapObject = GameObject.FindWithTag("Map");
        grid = mapObject.GetComponent<Grid>();
        gridMap = mapObject.GetComponent<GridMap>();

        var wallBreakObj = transform.Find("WallBreakSound");
        wallBreakSound = wallBreakObj.GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        cameraYaw += Input.GetAxis("Mouse X") + Input.GetAxis("KeyLook");
        cameraObject.transform.localRotation = Quaternion.AngleAxis(cameraYaw, Vector3.up);

        drilling = false;
        if (Input.GetAxis("Fire1") > 0.0f)
        {
            RaycastHit hit;
            var ray = new Ray(cameraObject.transform.position, cameraObject.transform.forward);
            
            if (Physics.Raycast(ray, out hit, 1.5f, LayerMask.GetMask("Cave")))
            {
                drilling = true;

                Vector3Int newDrillingCel = grid.WorldToCell(transform.position + (grid.cellSize / 2.0f));
                if (newDrillingCel != drillingCel)
                {
                    drillingProgress = 0.0f;
                    drillingCel = newDrillingCel;
                }
                else
                {
                    //Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta, 5.0f);
                    drillingProgress += Time.deltaTime * drillingSpeed;

                    //Break the wall if the progress exceeds 1. Stop the drill if it can't go through.
                    var wallState = gridMap.GetCelState(drillingCel);
                    if (wallState != MazeGenerator.CelState.Unvisited)
                    {
                        var pWallState = wallState;
                        var celCenter = grid.CellToWorld(drillingCel);
                        var diff = hit.point - celCenter;
                        var removeFromOtherWall = MazeGenerator.CelState.Unvisited;
                        var otherWallOffset = Vector3Int.zero;
                        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
                        {
                            if (diff.x > 0 && drillingCel.x < gridMap.columns - 1) 
                            {
                                wallState |= MazeGenerator.CelState.OpenEast;
                                removeFromOtherWall |= MazeGenerator.CelState.OpenWest;
                                otherWallOffset = Vector3Int.right;
                            }
                            else if (diff.x < 0 && drillingCel.x > 0) 
                            {
                                wallState |= MazeGenerator.CelState.OpenWest;
                                removeFromOtherWall |= MazeGenerator.CelState.OpenEast;
                                otherWallOffset = Vector3Int.left;
                            }
                            else
                            {
                                drilling = false;
                                drillingProgress = 0.0f;
                            }
                        }
                        else
                        {
                            if (diff.z > 0 && drillingCel.z < gridMap.rows - 1) 
                            {
                                wallState |= MazeGenerator.CelState.OpenNorth;
                                removeFromOtherWall |= MazeGenerator.CelState.OpenSouth;
                                otherWallOffset = Vector3Int.forward;
                            }
                            else if (diff.z < 0 && drillingCel.z > 0) 
                            {
                                wallState |= MazeGenerator.CelState.OpenSouth;
                                removeFromOtherWall |= MazeGenerator.CelState.OpenNorth;
                                otherWallOffset = Vector3Int.back;
                            }
                            else
                            {
                                drilling = false;
                                drillingProgress = 0.0f;
                            }
                        }
                        if (wallState != pWallState)
                        {
                            if (drillingProgress > 1.0f)
                            {
                                wallBreakSound.Play();
                                Debug.Log("Wall state at " + drillingCel + " from " + pWallState + " to " + wallState);
                                drillingProgress = 0.0f;
                                gridMap.SetCel(drillingCel.x, drillingCel.z, MazeGenerator.PREFAB_MAP[wallState]);
                                //Make the entrance on the other side
                                var otherWallState = gridMap.GetCelState(drillingCel + otherWallOffset);
                                Debug.Log("Opposite wall state at " + (drillingCel + otherWallOffset) + " from " + otherWallState + " to " + (otherWallState | removeFromOtherWall));
                                gridMap.SetCel(
                                    drillingCel.x + otherWallOffset.x,
                                    drillingCel.z + otherWallOffset.z, 
                                    MazeGenerator.PREFAB_MAP[
                                        otherWallState | removeFromOtherWall]);
                                gridMap.GenerateGeometry();
                            }
                        }
                        else
                        {
                            drilling = false;
                            drillingProgress = 0.0f;
                        }
                    }
                    else
                    {
                        drilling = false;
                        drillingProgress = 0.0f;
                    }
                }
            }
        }

        if (drilling)
        {
            if (!drillSound.isPlaying)
            {
                drillSound.Play();
            }
        }
        else
        {
            drillSound.Stop();
        }

        velocity = cameraObject.transform.localRotation * 
            new Vector3(Input.GetAxis("Horizontal"), -1.0f, Input.GetAxis("Vertical")).normalized * walkSpeed;

        var collision = controller.Move(velocity * Time.deltaTime);
    }
}
