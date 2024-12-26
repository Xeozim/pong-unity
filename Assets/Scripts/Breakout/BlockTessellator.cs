using UnityEngine;

public class BlockTessellator2D : MonoBehaviour
{
    public Bounds bounds;

    public GameObject blockPrefab;

    void Start()
    {
        TessellateBounds();
    }

    public void TessellateBounds()
    {
        // Clear children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Calculate the number of cubes in each dimension
        var xCount = Mathf.CeilToInt(bounds.size.x / blockPrefab.transform.localScale.x);
        var yCount = Mathf.CeilToInt(bounds.size.y / blockPrefab.transform.localScale.y);

        // Loop through each cube position
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                // Calculate the cube's position
                var cubePos = bounds.min + new Vector3(x * blockPrefab.transform.localScale.x, y * blockPrefab.transform.localScale.y, 0) + (blockPrefab.transform.localScale / 2); // Offset by half the cube size to center

                // Instantiate the cube
                var block = Instantiate(blockPrefab, cubePos, Quaternion.identity);
                block.transform.parent = transform;
            }
        }
    }

    public void OnGameOverStateUpdated()
    {
        TessellateBounds();
    }
}