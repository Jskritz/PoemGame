using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}