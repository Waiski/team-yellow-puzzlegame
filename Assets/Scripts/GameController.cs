using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] LoseScreenController loseScreen;
    [SerializeField] WinScreenController winScreen;
    
    public Text gameOverText;

    private bool gameIsRunning = true;

    public void GameOver()
    {
        loseScreen.Show();
        gameIsRunning = false;
    }

    public void GameWon()
    {
        winScreen.Show();
        gameIsRunning = false;
    }

    public bool IsGameRunning()
    {
        return gameIsRunning;
    }
}
