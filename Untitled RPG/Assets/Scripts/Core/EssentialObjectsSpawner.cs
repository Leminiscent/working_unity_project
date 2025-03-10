using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _essentialObjectsPrefab;

    private void Awake()
    {
        EssentialObjects[] existingObjects = FindObjectsOfType<EssentialObjects>();

        if (existingObjects.Length == 0)
        {
            Vector3 spawnPos = new(0, 0, 0);
            Grid grid = FindObjectOfType<Grid>();

            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }
            Instantiate(_essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
