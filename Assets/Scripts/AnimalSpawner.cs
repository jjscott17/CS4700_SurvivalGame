using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [Tooltip("Drag any number of animal prefabs (goat, sheep, etc.) here")]
    public GameObject[] animalPrefabs;

    [Tooltip("How many total animals to spawn at Start")]
    public int spawnCount = 10;

    [Tooltip("Size (X,Z) of the box around this spawner")]
    public Vector3 spawnAreaSize = new Vector3(50, 0, 50);

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // pick a random prefab
            var prefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];

            // random position within our box
            var offset = new Vector3(
                Random.Range(-spawnAreaSize.x/2, spawnAreaSize.x/2),
                0,
                Random.Range(-spawnAreaSize.z/2, spawnAreaSize.z/2)
            );
            var pos = transform.position + offset;

            var rot = Quaternion.Euler(0, Random.Range(0f,360f), 0);

            Instantiate(prefab, pos, rot);
        }
    }
}


