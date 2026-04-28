using UnityEngine;

public class RocketLaunch : MonoBehaviour
{
    public float launchSpeed = 5f;
    private bool isLaunching = false;

    private void Update()
    {
        if (isLaunching)
        {
            transform.position += Vector3.up * launchSpeed * Time.deltaTime;
        }
    }

    public void Launch()
    {
        isLaunching = true;
    }
}