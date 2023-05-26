using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monster : MonoBehaviour
{
    public float walkSpeed = 2.0f;

    private Vector3 velocity = Vector3.zero;

    private CharacterController controller;
    private Pathfinder mapPathfinder;

    private Vector3 walkTo;

    private AudioSource growlSound;
    private bool growled = false;
    private AudioSource screamSound;

    //When it's 1, the player dies
    public float attackTimer = 0.0f;
    public float attackSpeed = 1.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        var growler = transform.Find("Growler");
        growlSound = growler.GetComponent<AudioSource>();

        var screamer = transform.Find("Screamer");
        screamSound = screamer.GetComponent<AudioSource>();
        
        walkTo = transform.position;
    }

    void Update()
    {
        //Workaround for weird-ass deployment bug
        if (mapPathfinder == null)
        {
            var mapObject = GameObject.FindWithTag("Map");
            mapPathfinder = mapObject.GetComponent<Pathfinder>();
        }

        var playerObject = GameObject.FindWithTag("Player");

        var walkDiff = (walkTo - transform.position);
        walkDiff.y = 0.0f;
        var walkDist = walkDiff.magnitude;
        Vector3 walkDir = Vector3.zero;
        if (walkDist != 0.0f)
        {
            walkDir = walkDiff / walkDist;
        }
        
        if (playerObject != null)
        {
            if (walkDist < walkSpeed * Time.deltaTime) 
            {
                var path = mapPathfinder.GetPath(transform.position, playerObject.transform.position);
                if (path.Count > 0) walkTo = path[0];
            }

            var playerDiff = playerObject.transform.position - transform.position;
            var playerDist = playerDiff.magnitude;

            //Growl if you see the player
            if (!Physics.Raycast(transform.position, playerDiff / playerDist, playerDist, LayerMask.GetMask("Cave")))
            {
                if (!growled)
                {
                    growlSound.Play();
                    growled = true;
                }
                if (playerDist < 4.0f)
                {
                    attackTimer += attackSpeed * Time.deltaTime;
                    if (!screamSound.isPlaying)
                    {
                        screamSound.Play();
                    }
                    if (attackTimer > 1.0f)
                    {
                        //Die
                        SceneManager.LoadScene("Die");
                    }
                }
            }
            else
            {
                growled = false;
            }            
            if (playerDist >= 4.0f)
            {
                attackTimer -= attackSpeed * Time.deltaTime;
                if (attackTimer < 0.0f) attackTimer = 0.0f;
            }
        }
        velocity = walkDir * walkSpeed;
        velocity.y = -1.0f;

        var collision = controller.Move(velocity * Time.deltaTime);
    }
}
