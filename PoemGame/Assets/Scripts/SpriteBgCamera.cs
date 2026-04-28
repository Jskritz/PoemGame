using UnityEngine;

public class FitBackgroundToCamera : MonoBehaviour
{
    public SpriteRenderer background;

    void Start()
    {
        Camera cam = Camera.main;

        float spriteWidth = background.bounds.size.x;
        float spriteHeight = background.bounds.size.y;

        float aspect = (float)Screen.width / Screen.height;

        float sizeBasedOnHeight = spriteHeight / 2f;
        float sizeBasedOnWidth = spriteWidth / (2f * aspect);

        cam.orthographicSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);
    }
}
