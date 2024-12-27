
/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeExtractorEditor : EditorWindow
{
    // References to assign in the Editor window
    public Terrain terrain;
    public GameObject treesParent;

    [MenuItem("Tools/Tree Extractor")]
    public static void ShowWindow()
    {
        GetWindow<TreeExtractorEditor>("Tree Extractor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Extract Trees from Terrain", EditorStyles.boldLabel);

        // Terrain selection
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        // Trees Parent selection
        treesParent = (GameObject)EditorGUILayout.ObjectField("Trees Parent", treesParent, typeof(GameObject), true);

        // Extract button
        if (GUILayout.Button("Extract Trees"))
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Terrain.", "OK");
                return;
            }

            if (treesParent == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Trees Parent GameObject.", "OK");
                return;
            }

            ExtractAllTrees();
        }
    }

    private void ExtractAllTrees()
    {
        TerrainData terrainData = terrain.terrainData;
        TreeInstance[] treeInstances = terrainData.treeInstances;
        List<TreeInstance> treesToRemove = new List<TreeInstance>();

        // Iterate through all tree instances
        foreach (TreeInstance tree in treeInstances)
        {
            // Get the tree position in world space
            Vector3 worldPos = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;

            // Get the corresponding TreePrototype
            TreePrototype prototype = terrainData.treePrototypes[tree.prototypeIndex];

            if (prototype.prefab == null)
            {
                Debug.LogWarning($"Tree Prototype at index {tree.prototypeIndex} does not have a prefab assigned.");
                continue;
            }

            // Instantiate the tree prefab at the world position with the terrain's rotation
            GameObject treeObject = (GameObject)PrefabUtility.InstantiatePrefab(prototype.prefab, terrain.transform.parent);

            if (treeObject == null)
            {
                Debug.LogWarning($"Failed to instantiate prefab for prototype index {tree.prototypeIndex}.");
                continue;
            }

            // Set the position
            treeObject.transform.position = worldPos;

            // Optionally, set the tree's rotation and scale based on the TreeInstance data
            treeObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            treeObject.transform.localScale = Vector3.one * tree.widthScale;

            // Parent the tree to the specified parent GameObject
            treeObject.transform.parent = treesParent.transform;

            // Optionally, preserve the tree's height by adjusting Y position based on terrain height
            float terrainHeight = terrain.SampleHeight(worldPos) + terrain.transform.position.y;
            Vector3 adjustedPos = treeObject.transform.position;
            adjustedPos.y = terrainHeight;
            treeObject.transform.position = adjustedPos;
        }

        // Clear all trees from the terrain
        terrainData.treeInstances = new TreeInstance[0];
        EditorUtility.SetDirty(terrainData); // Mark terrainData as dirty to ensure changes are saved
        EditorUtility.DisplayDialog("Success", "All trees have been extracted and terrain trees have been cleared.", "OK");
    }
}
*/