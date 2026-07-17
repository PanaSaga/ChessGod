using System.Collections;
using UnityEngine;

public class BuffSparkleEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPiece playerPiece;
    [SerializeField] private Sprite sparkleSprite;
    [SerializeField] private Transform sparkleParent;

    [Header("Spawn timing")]
    [SerializeField] private int minConcurrent = 2;
    [SerializeField] private int maxConcurrent = 3;
    [SerializeField] private float spawnIntervalMin = 0.12f;
    [SerializeField] private float spawnIntervalMax = 0.25f;

    [Header("Sparkle motion")]
    [SerializeField] private float growDuration = 0.15f;
    [SerializeField] private float holdDuration = 0.3f;
    [SerializeField] private float shrinkDuration = 0.15f;
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 0.5f;
    [SerializeField] private float minSpawnRadius = 0.3f;
    [SerializeField] private float spawnRadius = 0.5f;

    private int activeCount;
    private Coroutine spawnLoop;

    private void Awake()
    {
        if (playerPiece == null)
            Debug.LogError("BuffSparkleEffect needs the Player Piece field assigned in the Inspector.");

        if (sparkleSprite == null)
            Debug.LogError("BuffSparkleEffect needs the Sparkle Sprite field assigned in the Inspector.");

        if (sparkleParent == null)
            sparkleParent = playerPiece != null ? playerPiece.BodyRenderer.transform : transform;
    }

    private void Update()
    {
        if (playerPiece == null) return;

        if (playerPiece.isBuffActive && spawnLoop == null)
        {
            spawnLoop = StartCoroutine(SpawnLoop());
        }
        else if (!playerPiece.isBuffActive && spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }
    }

    // Keeps between minConcurrent and maxConcurrent sparkles alive at all times, each on its own timer.
    private IEnumerator SpawnLoop()
    {
        while (activeCount < minConcurrent)
        {
            StartCoroutine(RunSparkle());
            yield return null;
        }

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            if (activeCount < maxConcurrent)
                StartCoroutine(RunSparkle());
        }
    }

    private IEnumerator RunSparkle()
    {
        activeCount++;

        GameObject sparkle = new GameObject("BuffSparkle");
        sparkle.transform.SetParent(sparkleParent, false);

        // Uniform-area sample within the ring between minSpawnRadius and spawnRadius (sqrt avoids inner-edge bunching).
        float effectiveMinRadius = Mathf.Min(minSpawnRadius, spawnRadius);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.Range(effectiveMinRadius * effectiveMinRadius, spawnRadius * spawnRadius));
        Vector2 randomOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        sparkle.transform.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);

        SpriteRenderer sparkleRenderer = sparkle.AddComponent<SpriteRenderer>();
        sparkleRenderer.sprite = sparkleSprite;
        sparkleRenderer.sortingOrder = 9999;

        float targetScale = Random.Range(minScale, maxScale);

        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            sparkle.transform.localScale = Vector3.one * (targetScale * Mathf.Clamp01(elapsed / growDuration));
            yield return null;
        }
        sparkle.transform.localScale = Vector3.one * targetScale;

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            sparkle.transform.localScale = Vector3.one * (targetScale * (1f - Mathf.Clamp01(elapsed / shrinkDuration)));
            yield return null;
        }

        Destroy(sparkle);
        activeCount--;
    }
}
