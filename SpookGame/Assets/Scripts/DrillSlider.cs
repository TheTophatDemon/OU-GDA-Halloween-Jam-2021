using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrillSlider : MonoBehaviour
{
    private Slider slider;
    private GameObject playerObj;
    private PlayerMovement playerMover;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            playerMover = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            slider.value = playerMover.drillingProgress;
        }
    }
}
