using UnityEngine;
using System.Linq;
using GazeTracking;

public class FoveatedLODController : MonoBehaviour
{
    private LODGroup[] lodGroups;

    public float fovealRadius = 0.2f;
    public float midFovealRadius = 0.4f;

    private ZoneVisualizer zoneVisualizer;

    void Start()
    {
#if UNITY_2023_1_OR_NEWER
        lodGroups = FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        lodGroups = FindObjectsOfType<LODGroup>();
#endif

        zoneVisualizer = FindObjectOfType<ZoneVisualizer>();

        if (zoneVisualizer == null)
        {
            Debug.LogError("ZoneVisualizer not found in the scene. Please add a ZoneVisualizer to your Canvas.");
        }
    }

    void Update()
    {
        Vector2 normalizedGazePos = GetNormalizedGazePosition();

        if (zoneVisualizer != null)
        {
            zoneVisualizer.SetCenter(normalizedGazePos);
        }

        UpdateLODGroups(normalizedGazePos);
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
            group.ForceLOD(-1);


            Vector3 screenPos = Camera.main.WorldToScreenPoint(group.transform.position);

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
}
