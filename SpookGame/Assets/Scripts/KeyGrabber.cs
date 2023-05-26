using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyGrabber : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Key"))
        {
            other.gameObject.GetComponent<Key>().OnCollect();
        }
    }
}
