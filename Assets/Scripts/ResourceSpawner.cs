using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Tooltip("Drag any number of resource prefabs (stone, stick, etc.) here")]
    public GameObject[] resourcePrefabs;

    [Tooltip("How many total resources to spawn at Start")]
    public int spawnCount = 300;

    [Tooltip("Size (X,Z) of the box around this spawner")]
    public Vector3 spawnAreaSize = new Vector3(500, 0, 500);

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            var prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];

            // pick random X,Z
            var offset = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );

            var spawnPos = transform.position + offset;

            // Cast a ray downward to find ground height
            Ray ray = new Ray(spawnPos + Vector3.up * 100f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                spawnPos = hit.point; // snap to terrain/ground
            }

            var rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Instantiate(prefab, spawnPos, rot);
        }
    }
}
