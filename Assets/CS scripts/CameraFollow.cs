using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;            // the player the camera orbits around
    [SerializeField] private float distance = 4f;         // how far behind the target the camera sits
    [SerializeField] private float height = 1.5f;         // how high above the target the camera sits
    [SerializeField] private float mouseSensitivity = 3f; // how fast the mouse rotates the camera

    private float rotationX = 0f;   // up/down rotation (pitch)
    private float rotationY = 0f;   // left/right rotation (yaw)

    private void Update()
    {
        // Read mouse movement and accumulate rotation
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Limit vertical look so the camera can't flip over
        rotationX = Mathf.Clamp(rotationX, -30f, 60f);

        // Base offset: behind and above the target
        Vector3 direction = new Vector3(0, height, -distance);

        // Build the rotation from the accumulated mouse input
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Position the camera around the target using that rotation
        transform.position = target.position + rotation * direction;

        // Always look at the target (slightly above its base)
        transform.LookAt(target.position + Vector3.up * height);
    }
}