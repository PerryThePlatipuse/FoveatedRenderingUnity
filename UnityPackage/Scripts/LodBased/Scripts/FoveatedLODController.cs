using UnityEngine;
using System.Linq;
using GazeTracking;
using System.Collections.Generic;

public class FoveatedLODController : MonoBehaviour
{
    private List<LODGroup> lodGroups = new List<LODGroup>();
    private List<Terrain> terrains = new List<Terrain>();

    public float fovealRadius = 0.2f;
    public float midFovealRadius = 0.4f;

    private ZoneVisualizer zoneVisualizer;

    void Start()
    {
        // Find all LODGroups in the scene
#if UNITY_2023_1_OR_NEWER
        LODGroup[] sceneLODs = FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        LODGroup[] sceneLODs = FindObjectsOfType<LODGroup>();
#endif
        lodGroups.AddRange(sceneLODs);

        // Find all terrains in the scene
        terrains.AddRange(FindObjectsOfType<Terrain>());

        if (terrains.Count == 0)
        {
            Debug.LogWarning("No Terrain found in the scene.");
        }

        zoneVisualizer = FindObjectOfType<ZoneVisualizer>();

        if (zoneVisualizer == null)
        {
            Debug.LogError("ZoneVisualizer not found in the scene. Please add a ZoneVisualizer to your Canvas.");
        }
        else
        {
            zoneVisualizer.UpdateRadii(new Vector2(fovealRadius, fovealRadius), new Vector2(midFovealRadius, midFovealRadius));
        }
    }

    void Update()
    {
        Vector2 normalizedGazePos = GetNormalizedGazePosition();

        if (zoneVisualizer != null)
        {
            zoneVisualizer.SetCenter(Input.mousePosition);
        }

        UpdateLODGroups(normalizedGazePos);
        UpdateTerrainLOD(normalizedGazePos);
    }

    private Vector2 GetNormalizedGazePosition()
    {
        Vector3 mousePos = Input.mousePosition; // testing

        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;
        return new Vector2(normalizedX, normalizedY);
    }

    private void UpdateLODGroups(Vector2 gazePos)
    {
        if (Camera.main == null)
            return;

        foreach (LODGroup group in lodGroups)
        {
            if (group == null)
                continue;

            group.ForceLOD(-1); // Reset to highest LOD

            Vector3 worldPos = group.transform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // If the object is behind the camera, skip
            if (screenPos.z < 0)
                continue;

            Vector2 normalizedScreenPos = new Vector2(
                (screenPos.x / Screen.width) * 2f - 1f,
                (screenPos.y / Screen.height) * 2f - 1f
            );

            float distance = Vector2.Distance(normalizedScreenPos, gazePos);

            bool inFoveal = distance <= fovealRadius;
            bool inMidFoveal = distance > fovealRadius && distance <= midFovealRadius;

            int targetLOD;
            if (inFoveal)
            {
                targetLOD = 0;
            }
            else if (inMidFoveal)
            {
                targetLOD = 1;
            }
            else
            {
                targetLOD = Mathf.Min(2, group.GetLODs().Length - 1);
            }

            LOD[] lods = group.GetLODs();
            targetLOD = Mathf.Clamp(targetLOD, 0, lods.Length - 1);

            group.ForceLOD(targetLOD);
        }
    }

    private void UpdateTerrainLOD(Vector2 gazePos)
    {
        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            if (terrainData.treeInstances.Length == 0)
                continue;

            // Example approach: Adjust tree density based on distance from gaze point
            // Note: Unity doesn't support dynamic tree density out of the box, so this requires a custom implementation
            // This could involve enabling/disabling tree instances or swapping prototypes based on distance

            // For demonstration, here's a simple approach to toggle tree visibility
            // This is not optimized and is for illustrative purposes only

            List<TreeInstance> treesToRemove = new List<TreeInstance>();
            List<TreeInstance> treesToAdd = new List<TreeInstance>();

            // Iterate through all tree instances
            foreach (var tree in terrainData.treeInstances)
            {
                Vector3 worldPos = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                // If the tree is behind the camera, skip
                if (screenPos.z < 0)
                    continue;

                Vector2 normalizedScreenPos = new Vector2(
                    (screenPos.x / Screen.width) * 2f - 1f,
                    (screenPos.y / Screen.height) * 2f - 1f
                );

                float distance = Vector2.Distance(normalizedScreenPos, gazePos);

                bool shouldBeVisible = distance <= midFovealRadius; // Example condition

                // Implement your logic to add/remove trees based on shouldBeVisible
                // Unity's Terrain API doesn't support removing individual trees at runtime efficiently
                // Consider using alternative methods like shader-based fading or custom tree systems
            }

            // Apply changes if necessary
            if (treesToRemove.Count > 0 || treesToAdd.Count > 0)
            {
                // Create a new list of trees
                List<TreeInstance> currentTrees = terrainData.treeInstances.ToList();

                // Remove trees
                foreach (var tree in treesToRemove)
                {
                    currentTrees.Remove(tree);
                }

                // Add trees
                currentTrees.AddRange(treesToAdd);

                // Assign the modified list back to the terrain
                terrainData.treeInstances = currentTrees.ToArray();
            }
        }
    }
}
