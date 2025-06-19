using System.Collections.Generic;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    [Header("Star Sprites")]
    [SerializeField] private Sprite[] starSpritesSheet1;
    [SerializeField] private Sprite[] starSpritesSheet2;

    [Header("Nebula Sprites")]
    [SerializeField] private Sprite[] nebulaSprites;

    [Header("Chunk Settings")]
    public static float ChunkSize = 50f;
    [SerializeField] private int starsPerChunk = 25;
    [SerializeField] private int nebulaePerChunk = 2;

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 1.5f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float nebulaMinScale = 8f;
    [SerializeField] private float nebulaMaxScale = 14f;

    private HashSet<Vector2Int> activeChunks = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, List<GameObject>> starsByChunk = new Dictionary<Vector2Int, List<GameObject>>();
    private Dictionary<Vector2Int, List<GameObject>> nebulaeByChunk = new Dictionary<Vector2Int, List<GameObject>>();

    void Update()
    {
        Vector2 playerPos = transform.position;
        Vector2Int currentChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / ChunkSize),
            Mathf.FloorToInt(playerPos.y / ChunkSize)
        );

        HashSet<Vector2Int> desiredChunks = new HashSet<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int chunk = new Vector2Int(currentChunk.x + dx, currentChunk.y + dy);
                desiredChunks.Add(chunk);

                if (!activeChunks.Contains(chunk))
                {
                    SpawnStarsInChunk(chunk);
                    SpawnNebulaeInChunk(chunk);
                }
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (Vector2Int chunk in activeChunks)
        {
            if (!desiredChunks.Contains(chunk))
            {
                toRemove.Add(chunk);
            }
        }

        foreach (Vector2Int chunk in toRemove)
        {
            if (starsByChunk.TryGetValue(chunk, out List<GameObject> stars))
            {
                foreach (GameObject star in stars)
                    if (star != null) Destroy(star);
                starsByChunk.Remove(chunk);
            }

            if (nebulaeByChunk.TryGetValue(chunk, out List<GameObject> nebulae))
            {
                foreach (GameObject nebula in nebulae)
                    if (nebula != null) Destroy(nebula);
                nebulaeByChunk.Remove(chunk);
            }

            activeChunks.Remove(chunk);
        }
    }

    void SpawnStarsInChunk(Vector2Int chunk)
    {
        if (starSpritesSheet1.Length == 0 && starSpritesSheet2.Length == 0) return;

        Vector2 chunkOrigin = new Vector2(chunk.x * ChunkSize, chunk.y * ChunkSize);

        List<GameObject> stars = new List<GameObject>();

        for (int i = 0; i < starsPerChunk; i++)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(0f, ChunkSize),
                Random.Range(0f, ChunkSize)
            );
            Vector2 spawnPos = chunkOrigin + spawnOffset;

            GameObject star = new GameObject("Star");

            // set Z based on desired depth (closer to 0 = foreground)
            float zDepth = Random.Range(2f, 10f);
            star.transform.position = new Vector3(spawnPos.x, spawnPos.y, zDepth);

            SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Stars";

            Sprite[] chosenSheet = Random.value < 0.5f ? starSpritesSheet1 : starSpritesSheet2;
            if (chosenSheet.Length > 0)
                renderer.sprite = chosenSheet[Random.Range(0, chosenSheet.Length)];

            renderer.color = GetRandomStarColor();

            float scale = Random.Range(minScale, maxScale);
            star.transform.localScale = Vector3.one * scale;

            stars.Add(star);
        }

        starsByChunk[chunk] = stars;
        activeChunks.Add(chunk);
    }


    void SpawnNebulaeInChunk(Vector2Int chunk)
    {
        if (nebulaSprites.Length == 0) return;

        Vector2 chunkOrigin = new Vector2(chunk.x * ChunkSize, chunk.y * ChunkSize);
        List<GameObject> nebulae = new List<GameObject>();

        for (int i = 0; i < nebulaePerChunk; i++)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(0f, ChunkSize),
                Random.Range(0f, ChunkSize)
            );
            Vector2 spawnPos = chunkOrigin + spawnOffset;

            GameObject nebula = new GameObject("Nebula");

            float zDepth = Random.Range(7f, 15f); // deeper than stars
            nebula.transform.position = new Vector3(spawnPos.x, spawnPos.y, zDepth);

            float rotationZ = Random.Range(0f, 360f);
            nebula.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

            SpriteRenderer renderer = nebula.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Background";

            renderer.sprite = nebulaSprites[Random.Range(0, nebulaSprites.Length)];
            renderer.color = new Color(1f, 1f, 1f, Random.Range(0.15f, 0.5f));

            float scale = Random.Range(nebulaMinScale, nebulaMaxScale);
            nebula.transform.localScale = Vector3.one * scale;

            nebula.AddComponent<SlowRotator>().rotationSpeed = Random.Range(-20f, 20f);

            nebulae.Add(nebula);
        }

        nebulaeByChunk[chunk] = nebulae;
    }


    Color GetRandomStarColor()
    {
        Color[] baseColors = new Color[]
        {
            new Color(1f, 1f, 1f),
            new Color(1f, 0.95f, 0.8f),
            new Color(0.8f, 0.9f, 1f),
            new Color(1f, 0.8f, 0.8f),
            new Color(0.9f, 1f, 0.9f)
        };

        Color baseColor = baseColors[Random.Range(0, baseColors.Length)];
        float brightness = Random.Range(0.6f, 1f);
        return baseColor * brightness;
    }
}
