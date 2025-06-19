using UnityEngine;

public class ParallaxStar : MonoBehaviour
{
    public Vector2 spawnOffset;      // original offset within chunk
    public float parallaxFactor;     // 0 = far, 1 = near

    private Transform player;
    private Vector2 chunkOrigin;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chunkOrigin = (Vector2)transform.position - spawnOffset;
    }

    void Update()
    {
        if (player == null) return;
        Vector2 playerPosition = player.position;
        Vector2 parallaxOffset = (Vector2)(playerPosition - chunkOrigin - spawnOffset) * parallaxFactor;
        transform.position = chunkOrigin + spawnOffset + parallaxOffset;
    }
}