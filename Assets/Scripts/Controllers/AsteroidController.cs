using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    
    private Transform _player;
    private static int _despawnDistance = 3; // in chunks
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _player = playerObj.transform;
    }

    void Update()
    {
        if (_player == null)
        {
            return;
        }
        
        // despawn if too far from player
        if (Vector2.Distance(_player.position, transform.position) > _despawnDistance * AsteroidSpawner.ChunkSize)
        {
            Destroy(gameObject);
        }
    }
}
