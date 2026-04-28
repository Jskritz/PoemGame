using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
    public GameObject rocket;
    public Transform spawnPoint;

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasSpawned) return;

        if (other.CompareTag("Player"))
        {
            hasSpawned = true;

            rocket.transform.position = spawnPoint.position;
            rocket.SetActive(true);

            Debug.Log("Rocket spawned.");
        }
    }
}