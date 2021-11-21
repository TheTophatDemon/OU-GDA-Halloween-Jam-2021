using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyCounter : MonoBehaviour
{
    private Text text;
    public int numKeys = 0;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        var keys = GameObject.FindGameObjectsWithTag("Key");
        numKeys = keys.Length;
        text.text = "Keys Left: " + keys.Length;
    }
}
