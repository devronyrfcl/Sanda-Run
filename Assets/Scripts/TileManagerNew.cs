using UnityEngine;

public class TileManagerNew : MonoBehaviour
{
    public GameObject[] tilePrefabs;  // Assign tile prefabs in inspector
    public float spawnZ = 0f;         // Starting spawn position on Z axis
    public float tileLength = 86f;    // Distance between tiles
    public int tilesOnScreen = 5;     // How many tiles are active at once

    private int lastPrefabIndex = -1; // To avoid duplicates
    private Transform playerTransform;
    private float safeZone = 100f;    // Distance before spawning new tile

    private GameObject[] activeTiles;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        activeTiles = new GameObject[tilesOnScreen];

        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i == 0 ? 0 : -1);
        }
    }

    void Update()
    {
        if (playerTransform.position.z - safeZone > (spawnZ - tilesOnScreen * tileLength))
        {
            SpawnTile();
            DeleteTile();
        }
    }

    void SpawnTile(int prefabIndex = -1)
    {
        if (prefabIndex == -1)
            prefabIndex = GetRandomPrefabIndex();

        Vector3 spawnPosition = new Vector3(0, 1.286f, spawnZ);
        GameObject tile = Instantiate(tilePrefabs[prefabIndex], spawnPosition, Quaternion.identity);
        activeTiles[0] = tile;

        // Shift active tiles array
        for (int i = activeTiles.Length - 1; i > 0; i--)
        {
            activeTiles[i] = activeTiles[i - 1];
        }

        activeTiles[0] = tile;
        spawnZ += tileLength;
        lastPrefabIndex = prefabIndex;
    }

    void DeleteTile()
    {
        if (activeTiles[activeTiles.Length - 1] != null)
        {
            Destroy(activeTiles[activeTiles.Length - 1]);
        }
    }

    int GetRandomPrefabIndex()
    {
        if (tilePrefabs.Length <= 1)
            return 0;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, tilePrefabs.Length);
        } while (randomIndex == lastPrefabIndex);

        return randomIndex;
    }
}
