using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This controls how the screen fades into different colors. 
/// It also triggers the win transition (which probably doesn't belong here...) 
/// </summary>
public class Fader : MonoBehaviour
{
    private GameManager gameManager;
    private Image image;

    private float winTimer = 0.0f;
    private bool winTransition = false;
    public bool WinTransition { get => winTransition; }

    void Start()
    {
        image = GetComponent<Image>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (!winTransition)
        {
            var monsterObj = GameObject.FindGameObjectWithTag("Monster");
            if (monsterObj)
            {
                // Fade to red when the monster is attacking
                var monster = monsterObj.GetComponent<Monster>();
                if (monster.attackTimer > 0.0f)
                {
                    image.color = new Color(1.0f, 0.0f, 0.0f, monster.attackTimer);
                }
            }
        }
    }

    ///<summary> Begins a coroutine that fades the screen to white before calling the game manager's win method. </summary>
    public void StartWinTransition()
    {
        if (!winTransition)
        {
            winTransition = true;
            StartCoroutine(WinTransitionCoroutine());
        }
    }

    private IEnumerator WinTransitionCoroutine()
    {
        // Fade to white after the last key is collected, then go to the winning state.
        while (winTimer < 1.0f)
        {
            winTimer += Time.deltaTime;
            image.color = new Color(1.0f, 1.0f, 1.0f, winTimer);
            yield return null;
        }
        gameManager.WinGame();
    }
}
