using UnityEngine;


public class SlowRotator : MonoBehaviour
{
    public float rotationSpeed = 5f; // degrees per minute

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime / 60f);
    }
}