using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DieScreen : MonoBehaviour
{
    private float timer = 0.0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 3.0f)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
