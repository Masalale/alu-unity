using UnityEngine;

/// <summary>
/// Capsule player movement: WASD walking on the ground plane, jumping, and the
/// "falling up" respawn that returns a player who drops out of the level.
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>Horizontal walking speed in units per second.</summary>
    public float speed = 5f;

    /// <summary>Downward acceleration applied every second while airborne.</summary>
    public float gravity = -20f;

    /// <summary>Peak height of a jump, converted to an initial vertical velocity.</summary>
    public float jumpHeight = 2f;

    /// <summary>Minimum number of seconds between two jumps.</summary>
    public float jumpCooldown = 0.5f;

    /// <summary>Horizontal world coordinates the player returns to after falling out of the level.</summary>
    public Vector3 startPosition = new Vector3(0f, 1.25f, 0f);

    /// <summary>World Y below which the player counts as having fallen out of the level.</summary>
    public float fallThreshold = -10f;

    // Capsule's CharacterController, cached once on Awake.
    private CharacterController characterController;

    // Movement velocity; the X and Z terms are overwritten every frame by the
    // input direction, the Y term carries jump and gravity.
    private Vector3 velocity;

    // Seconds left before another jump is accepted; counts down to zero.
    private float jumpCooldownTimer;

    // True from the moment a fall is handled until the player lands again, so a
    // single fall can never trigger more than one respawn.
    private bool isRespawning;

    // World Y the player is dropped at so they fall back onto the start platform.
    private const float respawnHeight = 10f;

    // Caches the CharacterController before the first frame is updated.
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Walks, jumps, applies gravity, and respawns the player once they fall out of the level.
    private void Update()
    {
        // Normalised so that walking diagonally is not faster than walking straight.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float forward = Input.GetAxisRaw("Vertical");
        Vector3 direction = (transform.right * horizontal + transform.forward * forward).normalized;

        jumpCooldownTimer -= Time.deltaTime;

        if (characterController.isGrounded && jumpCooldownTimer <= 0f && Input.GetButton("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCooldownTimer = jumpCooldown;
        }

        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);

        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
        else if (isRespawning && characterController.isGrounded)
        {
            // Back on solid ground: the next fall is allowed to respawn again.
            isRespawning = false;
        }
    }

    /// <summary>
    /// Teleports the player above the start platform with zero horizontal velocity so
    /// they fall straight back onto it. Does nothing while a respawn is already pending.
    /// </summary>
    public void Respawn()
    {
        if (isRespawning)
        {
            return;
        }

        isRespawning = true;
        velocity = Vector3.zero;
        characterController.enabled = false;
        transform.position = new Vector3(startPosition.x, respawnHeight, startPosition.z);
        characterController.enabled = true;
    }
}
