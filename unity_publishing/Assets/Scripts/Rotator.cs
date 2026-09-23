using UnityEngine;

public class Rotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(45f * Time.deltaTime, 0f, 0f);
    }
}
