using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The tree's transform
    public Vector3 offset; // Offset from the tree
    public float smoothSpeed = 0.125f; // How smoothly the camera follows

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
