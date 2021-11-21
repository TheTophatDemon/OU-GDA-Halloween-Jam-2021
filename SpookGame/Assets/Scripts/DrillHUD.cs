using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillHUD : MonoBehaviour
{
    private Animator animator;
    private GameObject playerObj;
    private PlayerMovement playerMover;

    void Start()
    {
        animator = GetComponent<Animator>();
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
            animator.SetBool("drilling", playerMover.drilling);
        }
    }
}
