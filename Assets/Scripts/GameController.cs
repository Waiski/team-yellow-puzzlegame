using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text gameOverText;

    private bool gameIsRunning = true;

    public void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOverText.enabled = true;
        gameIsRunning = false;
    }

    public void GameWon()
    {
        gameOverText.text = "You Win!";
        gameOverText.enabled = true;
        gameIsRunning = false;
    }

    public bool IsGameRunning()
    {
        return gameIsRunning;
    }
}
