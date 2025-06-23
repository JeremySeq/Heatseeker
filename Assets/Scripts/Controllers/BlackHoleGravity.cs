using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public float gravityStrength = 50f;
    public float pullRadius = 20f;
    public float shrinkRadius = 5f;
    public float minScaleFactor = 0.4f;
    public float destructionRadius = 1f;

    void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (var col in colliders)
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 direction = ((Vector2)transform.position - rb.position).normalized;
                float distance = Vector2.Distance(transform.position, rb.position);

                // Remove object if too close to center
                if (distance < destructionRadius)
                {
                    Destroy(rb.gameObject);
                    continue;
                }

                float forceMagnitude = gravityStrength * (1f - Mathf.Clamp01(distance / pullRadius));
                rb.AddForce(direction * forceMagnitude);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the pull radius
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        // Visualize the shrink radius
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, shrinkRadius);

        // Visualize the destruction radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, destructionRadius);
    }
}