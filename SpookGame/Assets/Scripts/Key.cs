using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary> Controls the movement of the key and its behavior when it's collected. </summary>
public class Key : MonoBehaviour
{
    public float spinSpeed = 360.0f;
    private bool collect = false;
    public float shrinkSpeed = 2.0f;

    void Update()
    {
        // Spin
        transform.Rotate(Vector3.up * Time.deltaTime * spinSpeed);
        
        if (collect)
        {
            // Shrink when collected, then disappear
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
            if (transform.localScale.x < 0.0f || transform.localScale.y < 0.0f || transform.localScale.z < 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    ///<summary> Called from outside of this script when the key is retrieved. </summary>
    public void OnCollect()
    {
        if (collect == false)
        {
            collect = true;
            GetComponent<AudioSource>().Play();
        }
    }
}
