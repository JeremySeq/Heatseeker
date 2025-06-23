using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 2f;
    
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMissile();
            timer = 0f;
        }
    }
    
    void SpawnMissile()
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2) this.transform.position + spawnDirection * spawnRadius;

        Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
    }
}
