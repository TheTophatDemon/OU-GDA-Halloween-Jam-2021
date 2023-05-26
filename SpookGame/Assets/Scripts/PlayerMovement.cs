using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls how the player moves across the map
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    // Maxmimum movement speed in units per second.
    public float walkSpeed = 10.0f;

    private Vector3 velocity = Vector3.zero;
    private GameObject cameraObject;

    // This code keeps track of the camera's yaw manually because localRotation isn't always consistent.
    private float cameraYaw;

    public float Yaw { get => cameraYaw; set => cameraYaw = value; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraObject = transform.Find("Camera").gameObject;
        //cameraYaw = cameraObject.transform.localRotation.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Turn camera on the Y axis along with mouse movement (or keyboard)
        float keyTurning = Input.GetAxis("KeyLook") * 5.0f;
        if (keyTurning == 0.0f) 
        {
            cameraYaw += Input.GetAxis("Mouse X");
        } 
        else 
        {
            cameraYaw += keyTurning;
        }
        cameraObject.transform.localRotation = Quaternion.AngleAxis(cameraYaw, Vector3.up);

        // Set velocity based on key input
        velocity = cameraObject.transform.localRotation * 
            new Vector3(Input.GetAxis("Horizontal"), -1.0f, Input.GetAxis("Vertical")).normalized * walkSpeed;

        // Execute movement
        var collision = controller.Move(velocity * Time.deltaTime);
    }
}
