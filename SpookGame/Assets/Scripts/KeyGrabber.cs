using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary> Detects when the player is stepping over a key and collects it. </summary>
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
