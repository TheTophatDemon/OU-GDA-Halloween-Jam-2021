using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Controls the drill image shown on the HUD. </summary>
public class DrillHUD : MonoBehaviour
{
    private Animator animator;
    private GameObject playerObj;
    private PlayerDrilling playerDrilling;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            playerDrilling = playerObj.GetComponent<PlayerDrilling>();
        }
        else
        {
            animator.SetBool("drilling", playerDrilling.drilling);
        }
    }
}
