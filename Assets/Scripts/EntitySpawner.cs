using UnityEngine;
using Meta.XR.MRUtilityKit;

public class EntitySpawner : MonoBehaviour
{
    public float spawnTimer = 1;
    public GameObject prefabToSpawn;
    public float spawnRadius = 3;

    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabel;
    public float normalOffset = 0.1f;
    public int spawnLimit = 20;

    private float timer;
    private int spawnCount = 0;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!MRUK.Instance && !MRUK.Instance.IsInitialized)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnTimer)
        {
            SpawnEntity();
            timer -= spawnTimer;
        }
    }

    public void SpawnEntity()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        bool hasFound = room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, minEdgeDistance, new LabelFilter(spawnLabel), out Vector3 pos, out Vector3 norm);

        if (hasFound && spawnCount < spawnLimit)
        {
            Vector3 randomPositionNormalOffset = pos + norm * normalOffset;
            randomPositionNormalOffset.y = 2;

            Instantiate(prefabToSpawn, randomPositionNormalOffset, Quaternion.identity);
            spawnCount++;
        }
    }
}
