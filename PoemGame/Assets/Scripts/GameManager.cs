using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject failPanel;
    public GameObject player;
    public RocketLaunch rocketLaunch;

    private bool gameEnded = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
    }

    public void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("Player reached the rocket. Win!");

        if (rocketLaunch != null) rocketLaunch.Launch();
        if (winPanel != null) winPanel.SetActive(true);

        if (player != null)
        {
            player.SetActive(false);
        }
    }

    public void FailGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("Player failed to reach the rocket.");

        if (failPanel != null) failPanel.SetActive(true);

        if (player != null)
        {
            player.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool HasGameEnded()
    {
        return gameEnded;
    }
}