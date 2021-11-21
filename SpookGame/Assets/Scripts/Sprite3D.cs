using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprite3D : MonoBehaviour
{
    // Turns to face the camera at all times
    void Update()
    {
        Vector3 planePos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 cameraPlanePos = Camera.main.transform.position;
        cameraPlanePos.y = 0.0f;
        transform.rotation = Quaternion.LookRotation(planePos - cameraPlanePos, Vector3.up);
    }
}
