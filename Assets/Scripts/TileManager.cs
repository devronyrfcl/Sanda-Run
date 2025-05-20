using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public List<GameObject> activeTiles;

    public float zSpawn = 0f;
    public float tileLength = 200f;
    public int numberOfTiles = 5;

    public int totalNumOfTiles;

    private Transform playerTransform;

    private int previousIndex;

    private void Start()
    {

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        totalNumOfTiles = tilePrefabs.Length;

        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i == 0)
            {
                SpawnTile(0);
            }
            else
            {
                SpawnTile(Random.Range(0, tilePrefabs.Length));
            }
        }
    }

    private void Update()
    {
        if (playerTransform.position.z - 145 > zSpawn - (numberOfTiles * tileLength)) // Here is a safeZone (tilength * 4 + something)
        {
            int index = Random.Range(0, totalNumOfTiles);
            if (index == previousIndex)
            {
                index = Random.Range(0, totalNumOfTiles);
            }

            SpawnTile(index);
            DeleteTile();
        }
    }

    public void SpawnTile(int tileIndex = 0)
    {
        Vector3 spawnPos = new Vector3(0, 1.3f, zSpawn);
        GameObject go = Instantiate(tilePrefabs[tileIndex], spawnPos, transform.rotation);
        activeTiles.Add(go);
        zSpawn += tileLength;

        previousIndex = tileIndex;
    }

    private void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }



}
