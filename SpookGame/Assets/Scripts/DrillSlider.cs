using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Controls the slider shown whilst drilling. </summary>
public class DrillSlider : MonoBehaviour
{
    private Slider slider;
    private GameObject playerObj;
    private PlayerDrilling playerDriller;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            playerDriller = playerObj.GetComponent<PlayerDrilling>();
        }
        else
        {
            slider.value = playerDriller.drillingProgress;
            // Make slider disappear when at 0.
            slider.transform.localScale = slider.value > 0.0f ? Vector3.one : Vector3.zero;
        }
    }
}
