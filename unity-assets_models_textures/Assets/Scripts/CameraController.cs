using UnityEngine;

/// <summary>
/// Third person follow camera: smooth position tracking plus right-mouse-drag look.
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>Player the camera follows.</summary>
    public Transform player;

    /// <summary>Approximate seconds the camera takes to catch up with the player.</summary>
    public float smoothTime = 0.15f;

    /// <summary>Degrees of rotation applied per unit of mouse movement.</summary>
    public float mouseSensitivity = 3f;

    /// <summary>Lowest allowed downward pitch of the camera, in degrees.</summary>
    public float minPitch = -40f;

    /// <summary>Highest allowed upward pitch of the camera, in degrees.</summary>
    public float maxPitch = 40f;

    // Offset from the player to the camera, expressed in the yaw/pitch pivot's space
    // and rotated by it before being added to the player's position.
    private readonly Vector3 cameraOffset = new Vector3(0f, 2.5f, -6.25f);

    // Persistent pivot orientation. Kept as state rather than read back off the
    // transform so the look direction survives the follow moving to a new position.
    private float pivotYaw;

    // Persistent pivot pitch, clamped to the pitch limits on every mouse update.
    private float pivotPitch;

    // SmoothDamp velocity carried between frames; resetting it would snap the camera.
    private Vector3 smoothVelocity;

    // Adopts the camera's authored rotation as the starting pivot orientation, so the
    // scene's specified tilt survives the first frame instead of snapping to level.
    private void Awake()
    {
        Vector3 authoredRotation = transform.eulerAngles;
        pivotYaw = authoredRotation.y;
        pivotPitch = Mathf.Clamp(authoredRotation.x, minPitch, maxPitch);
    }

    // Rotates the pivot yaw and clamped pitch while the right mouse button is held.
    private void Update()
    {
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        pivotYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        pivotPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        pivotPitch = Mathf.Clamp(pivotPitch, minPitch, maxPitch);
    }

    // Smoothly follows the player without disturbing the pivot orientation.
    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Quaternion pivotRotation = Quaternion.Euler(pivotPitch, pivotYaw, 0f);
        Vector3 targetPosition = player.position + pivotRotation * cameraOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothVelocity,
            smoothTime);
        transform.rotation = pivotRotation;
    }
}
