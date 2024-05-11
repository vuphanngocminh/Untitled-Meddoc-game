using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{
    public int playerScore;
    public Text scoreText;
    public Text missingOrgan;
    public Text scoreNeededDisplay;
    public Text timeRemainingDisplay;
    public int scoreNeeded;
    public GameObject gameWinScreen;
    public GameObject gameLoseScreen;  // Reference to the lose screen object

    private float timeRemaining;  // Time remaining before losing
    public float loseTime = 120f; // Set the lose time in seconds here (default is 120 seconds)

    [ContextMenu("inwasd")]
    private void Start()
    {
        scoreNeededDisplay.text = scoreNeeded.ToString();
        timeRemaining = loseTime;  // Initialize the timer
    }

    private void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;  
            timeRemainingDisplay.text = ((int)timeRemaining).ToString();
        }
        else if (playerScore < scoreNeeded)
        {
            gameLose();  // Call gameLose when time runs out
        }
    }

    [ContextMenu("in")]
    public void addScore()
    {
        playerScore = playerScore + 1;
        scoreText.text = playerScore.ToString();
    }

    public void missing(string text)
    {
        missingOrgan.text = text;
    }

    public void checkWin()
    {
        if (playerScore >= scoreNeeded)
        {
            gameWin();
            Debug.Log("win");
        }
        else
        {
            Debug.Log("win cc");
        }
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameWin()
    {
        gameWinScreen.SetActive(true);
        timeRemaining = 0; // Stop the timer on win
    }

    public void gameLose()
    {
        gameLoseScreen.SetActive(true);
        timeRemaining = 0; // Stop the timer on lose
        Debug.Log("lose"); // Log the lose event
    }
}
