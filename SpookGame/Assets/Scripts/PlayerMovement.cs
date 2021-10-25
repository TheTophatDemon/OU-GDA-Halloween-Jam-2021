using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    public float walkSpeed = 10.0f;

    private Vector3 velocity = Vector3.zero;
    private GameObject cameraObject;
    private float cameraYaw;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraObject = transform.Find("Camera").gameObject;
        cameraYaw = cameraObject.transform.localRotation.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        cameraYaw += Input.GetAxis("Mouse X");
        cameraObject.transform.localRotation = Quaternion.AngleAxis(cameraYaw, Vector3.up);

        velocity = cameraObject.transform.localRotation * 
            new Vector3(Input.GetAxis("Horizontal"), -1.0f, Input.GetAxis("Vertical")).normalized * walkSpeed;

        var collision = controller.Move(velocity * Time.deltaTime);
    }
}
