using UnityEngine;

public class RocketTrigger : MonoBehaviour
{
    public GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Rocket touched by: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched rocket. Triggering win.");
            gameManager.WinGame();
        }
    }
}