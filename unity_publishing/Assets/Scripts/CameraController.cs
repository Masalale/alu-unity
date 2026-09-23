using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        transform.position = new Vector3(22f, 26f, 7f);
        transform.rotation = Quaternion.Euler(65f, 0f, 0f);

        if (player != null)
        {
            offset = transform.position - player.transform.position;
        }
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = new Vector3(
                player.transform.position.x + offset.x,
                26f,
                player.transform.position.z + offset.z
            );
        }
    }
}
