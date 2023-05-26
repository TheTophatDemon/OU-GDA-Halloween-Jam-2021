using UnityEngine;
using UnityEngine.SceneManagement;

///<summary> Handles scene transitions, win/lose conditions, key count. </summary>
public class GameManager : MonoBehaviour
{
    public int KeyCount { get => keyCount; }
    private int keyCount;
    ///<summary> The number of keys spawned at the start of the game. </summary>
    public int StartKeyCount {get => startKeyCount; }
    private int startKeyCount = -1; // This count cannot be set until the first update due to the initialization order of components.

    ///<summary> Returns a float between 0 and 1 representing the player's progress towards completing the game. </summary>
    public float Progress 
    {
        get => (startKeyCount <= 0) ? 0.0f : (KeyCount / (float)StartKeyCount);
    }

    private Fader fader;

    void Start()
    {
        fader = FindFirstObjectByType<Fader>();
    }

    void Update()
    {
        keyCount = GameObject.FindGameObjectsWithTag("Key").Length;
        if (startKeyCount <= 0) startKeyCount = keyCount;
        if (keyCount <= 0)
        {
            fader.StartWinTransition();
        }

        // Exit game when pressing escape
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
    }

    public void WinGame()
    {
        SceneManager.LoadScene("Win");
    }

    public void LoseGame()
    {
        if (!fader.WinTransition) SceneManager.LoadScene("Die");
    }
}
