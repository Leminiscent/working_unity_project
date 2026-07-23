using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _essentialObjectsPrefab;

    private void Awake()
    {
        // Check if any EssentialObjects exist in the scene.
        EssentialObjects[] existingObjects = FindObjectsByType<EssentialObjects>();

        if (existingObjects.Length == 0)
        {
            // Default spawn position is the origin.
            Vector3 spawnPos = Vector3.zero;

            // Attempt to find a Grid in the scene and use its position.
            Grid grid = FindAnyObjectByType<Grid>();
            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }

            _ = Instantiate(_essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}