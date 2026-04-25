using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class BlockPileSpawner : MonoBehaviour
{
    [Tooltip("Prefab to spawn as a target block")]
    public GameObject blockPrefab;

    [Tooltip("Width of the block pile (in blocks)")]
    public int pileWidth = 3;

    [Tooltip("Height of the block pile (in blocks)")]
    public int pileHeight = 4;

    [Tooltip("Depth of the block pile (in blocks)")]
    public int pileDepth = 2;

    [Tooltip("Space between block centers")]
    public float blockSpacing = 0.11f;

    [Tooltip("Scale of each block")]
    public float blockScale = 0.1f;

    [Tooltip("Minimum distance from surface edges")]
    public float minEdgeDistance = 0.2f;

    [Tooltip("MRUK surface type to spawn on")]
    public MRUK.SurfaceType surfaceType = MRUK.SurfaceType.FACING_UP;

    void Start()
    {
        StartCoroutine(SpawnPileWhenReady());
    }

    IEnumerator SpawnPileWhenReady()
    {
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
            yield return null;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        int tableCount = 0;

        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE)
            {
                Vector3 basePos = anchor.transform.position;
                Vector3 normal = anchor.transform.up;

                // Add a small random offset so the pile isn't always dead-center on the table
                Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
                Vector3 spawnPos = basePos + new Vector3(randomOffset.x, 0f, randomOffset.y);
                spawnPos += normal * (blockScale * 0.5f);

                SpawnPile(spawnPos);
                tableCount++;
            }
        }

        if (tableCount == 0)
        {
            Debug.LogWarning("BlockPileSpawner: No table found in the room.");
        }
        else
        {
            Debug.Log($"BlockPileSpawner: Spawned block piles on {tableCount} table(s).");
        }
    }

    void SpawnPile(Vector3 basePosition)
    {
        if (blockPrefab == null)
        {
            Debug.LogError("BlockPileSpawner: No block prefab assigned!");
            return;
        }

        for (int y = 0; y < pileHeight; y++)
        {
            for (int x = 0; x < pileWidth; x++)
            {
                for (int z = 0; z < pileDepth; z++)
                {
                    Vector3 offset = new Vector3(
                        x * blockSpacing,
                        y * blockSpacing,
                        z * blockSpacing);

                    Vector3 spawnPosition = basePosition + offset;
                    Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }

        Debug.Log($"BlockPileSpawner: Spawned {pileWidth * pileHeight * pileDepth} blocks.");
    }
}
