using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

///<summary> Controls the AI and movement of the monster.</summary>
public class Monster : MonoBehaviour
{
    private const float MAX_RUN_SPEED = 4.0f;
    private const float MIN_RUN_SPEED = 2.0f;

    public float walkSpeed = 2.0f;
    public float runSpeed = MIN_RUN_SPEED;

    private GameManager gameManager;
    private Vector3 velocity = Vector3.zero;

    private CharacterController controller;
    private Pathfinder mapPathfinder;

    ///<summary> Coordinate in world space that the monster directly walks towards at all times. </summary>
    private Vector3 walkTo;

    private AudioSource growlSound;
    private bool growled = false;
    private AudioSource screamSound;


    ///<summary> Increases when the player is very close. When it reaches 1.0, the player dies. </summary>
    public float attackTimer = 0.0f;
    public float attackSpeed = 1.5f;

    ///<summary> The distance to the player at which the monster begins to attack. </summary>
    private const float ATTACK_THRESHOLD = 3.0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        var growler = transform.Find("Growler");
        growlSound = growler.GetComponent<AudioSource>();

        var screamer = transform.Find("Screamer");
        screamSound = screamer.GetComponent<AudioSource>();
        
        walkTo = transform.position;

        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (mapPathfinder == null)
        {
            var mapObject = GameObject.FindWithTag("Map");
            mapPathfinder = mapObject.GetComponent<Pathfinder>();
        }

        var playerObject = GameObject.FindWithTag("Player");

        // Move directly towards walkTo
        var walkDiff = (walkTo - transform.position);
        walkDiff.y = 0.0f;
        
        // The distance from the walkTo target position. Should not be greater than 1 cel's length.
        var walkDist = walkDiff.magnitude;
        
        Vector3 walkDir = Vector3.zero;
        if (walkDist != 0.0f)
        {
            walkDir = walkDiff / walkDist;
        }

        // When there are few keys left in the level, the monster will be able to track down the player more often by moving quicker.
        runSpeed = MIN_RUN_SPEED + (MAX_RUN_SPEED - MIN_RUN_SPEED) * (1.0f - gameManager.Progress);

        // The monster will run faster when not in the player's line of sight.
        float speed = runSpeed;
        
        if (playerObject != null)
        {
            if (walkDist < speed * Time.deltaTime) 
            {
                // Calculate a new path to the player every time the monster reaches a new cel
                var path = mapPathfinder.GetPath(transform.position, playerObject.transform.position);
                if (path.Count > 0) walkTo = path[0];
            }

            var playerDiff = playerObject.transform.position - transform.position;
            var playerDist = playerDiff.magnitude;

            // Growl if you see the player
            if (!Physics.Raycast(transform.position, playerDiff / playerDist, playerDist, LayerMask.GetMask("Cave")))
            {
                // Will walk slowly once the player is in sight to give the player time to escape!
                speed = walkSpeed;

                if (!growled)
                {
                    growlSound.Play();
                    growled = true;
                }

                // Attack
                if (playerDist < ATTACK_THRESHOLD)
                {
                    attackTimer += attackSpeed * Time.deltaTime;
                    if (!screamSound.isPlaying)
                    {
                        screamSound.Play();
                    }
                    if (attackTimer > 1.0f)
                    {
                        //Die
                        gameManager.LoseGame();
                    }
                }
            }
            else
            {
                growled = false;
            }      

            // Reduce attack timer if player escapes      
            if (playerDist >= ATTACK_THRESHOLD)
            {
                attackTimer -= attackSpeed * Time.deltaTime;
                if (attackTimer < 0.0f) attackTimer = 0.0f;
            }
        }

        // Apply movement and collision
        velocity = walkDir * speed;
        velocity.y = -1.0f;

        var collision = controller.Move(velocity * Time.deltaTime);
    }
}
