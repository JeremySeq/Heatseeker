using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] [Range(0f, 1f)] private float powerUpSpawnChancePerChunk = .7f;

    public static float ChunkSize = 20f;
    [SerializeField] private int asteroidsPerChunk = 10;

    [SerializeField] private float _minVelocity = 1f;
    [SerializeField] private float _maxVelocity = 3f;
    [SerializeField] private float _maxTorque = 0.5f;

    private HashSet<Vector2Int> spawnedChunks = new HashSet<Vector2Int>();

    void Update()
    {
        Vector2 playerPos = this.transform.position;
        Vector2Int currentChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / ChunkSize),
            Mathf.FloorToInt(playerPos.y / ChunkSize)
        );

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int neighborChunk = new Vector2Int(currentChunk.x + dx, currentChunk.y + dy);

                if (!spawnedChunks.Contains(neighborChunk))
                {
                    SpawnAsteroidsInChunk(neighborChunk);
                    spawnedChunks.Add(neighborChunk);
                }
            }
        }
    }

    void SpawnAsteroidsInChunk(Vector2Int chunk)
    {
        Vector2 chunkOrigin = new Vector2(chunk.x * ChunkSize, chunk.y * ChunkSize);

        for (int i = 0; i < asteroidsPerChunk; i++)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(0f, ChunkSize),
                Random.Range(0f, ChunkSize)
            );
            Vector2 spawnPos = chunkOrigin + spawnOffset;

            GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            GameObject asteroid = Instantiate(prefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = asteroid.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float velocity = Random.Range(_minVelocity, _maxVelocity);
                rb.linearVelocity = randomDir * velocity;

                float scale = asteroid.transform.localScale.magnitude;
                float adjustedTorque = Random.Range(-_maxTorque, _maxTorque) / scale;
                rb.AddTorque(adjustedTorque, ForceMode2D.Impulse);
            }
        }

        // spawn power-up with a lower chance
        if (powerUpPrefabs.Length > 0 && Random.value < powerUpSpawnChancePerChunk)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(0f, ChunkSize),
                Random.Range(0f, ChunkSize)
            );
            Vector2 spawnPos = chunkOrigin + spawnOffset;

            GameObject powerUp = Instantiate(
                powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)],
                spawnPos,
                Quaternion.identity
            );
        }
    }
}
