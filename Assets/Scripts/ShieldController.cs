using UnityEngine;

public class ShieldController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Missile"))
        {
            other.GetComponent<MissileController>().Explode();
        }
    }
}
