using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public float spinSpeed = 360.0f;
    private bool collect = false;
    public float shrinkSpeed = 2.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * spinSpeed);
        if (collect)
        {
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
            if (transform.localScale.x < 0.0f || transform.localScale.y < 0.0f || transform.localScale.z < 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void OnCollect()
    {
        if (collect == false)
        {
            collect = true;
            GetComponent<AudioSource>().Play();
        }
    }
}
