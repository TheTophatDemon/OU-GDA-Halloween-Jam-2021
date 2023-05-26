using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary> Controls the element on the HUD that counts the keys. </summary>
public class KeyCounter : MonoBehaviour
{
    private GameManager gameManager;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        text.text = "Keys Left: " + gameManager.KeyCount;
    }
}
