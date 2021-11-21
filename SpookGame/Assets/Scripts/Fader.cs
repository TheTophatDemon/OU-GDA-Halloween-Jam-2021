using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public KeyCounter keyCounter;

    private Image image;

    private float winTimer = 0.0f;

    void Start()
    {
        image = GetComponent<Image>();
        var keyCounterObj = GameObject.Find("KeyCounter");
        keyCounter = keyCounterObj.GetComponent<KeyCounter>();
    }

    void Update()
    {
        var monsterObj = GameObject.FindGameObjectWithTag("Monster");
        if (monsterObj)
        {
            var monster = monsterObj.GetComponent<Monster>();
            if (monster.attackTimer > 0.0f)
            {
                image.color = new Color(1.0f, 0.0f, 0.0f, monster.attackTimer);
            }
        }

        if (keyCounter.numKeys <= 0)
        {
            winTimer += Time.deltaTime;
            image.color = new Color(1.0f, 1.0f, 1.0f, winTimer);
            if (winTimer > 1.0f)
            {
                SceneManager.LoadScene("Win");
            }
        }
    }
}
