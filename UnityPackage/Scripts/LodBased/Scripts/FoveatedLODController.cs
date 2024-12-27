using UnityEngine;
using System.Linq;
using GazeTracking;
using FoveatedRenderingVRS;
using UnityEngine.SceneManagement;

public class FoveatedLODController : MonoBehaviour
{
    private LODGroup[] lodGroups;

    [Header("LOD Ellipse Radii (Normalized)")]
    [Tooltip("Foveal region ellipse radii in normalized screen coords")]
    public Vector2 fovealRadii = new Vector2(0.2f, 0.2f);

    [Tooltip("Mid-foveal region ellipse radii in normalized screen coords")]
    public Vector2 midFovealRadii = new Vector2(0.4f, 0.3f);

    [Header("LOD Visualization & Gaze")]
    [Tooltip("Show or hide the ellipse border (ZoneVisualizer)")]
    public bool showBorder = true;

    [Tooltip("Use the VRS plugin gaze or fallback to mouse")]
    public bool usePluginGaze = true;

    [Tooltip("If false, do not override the ellipse zone in ZoneVisualizer (when VRS is also on).")]
    public bool overrideZoneVisualizer = true;

    [Tooltip("If false, do not override the gaze center (when VRS is also on).")]
    public bool overrideGaze = true;

    private ZoneVisualizer zoneVisualizer;
    private VrsGazeUpdater gazeUpdater;
    private bool pluginGazeActive;

    void Start()
    {
#if UNITY_2023_1_OR_NEWER
        lodGroups = FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        lodGroups = FindObjectsOfType<LODGroup>();
#endif

        // Attempt to find a GazeUpdater from VRS
        gazeUpdater = FindObjectOfType<VrsGazeUpdater>();
        pluginGazeActive = (gazeUpdater != null) && usePluginGaze;

        // Find the ZoneVisualizer
        zoneVisualizer = FindObjectOfType<ZoneVisualizer>();
        if (zoneVisualizer == null)
        {
            Debug.LogError("FoveatedLODController: ZoneVisualizer not found in scene!");
        }
        else
        {
            // If we’re allowed to override the ellipse
            if (overrideZoneVisualizer)
            {
                zoneVisualizer.isVisualizationEnabled = showBorder;
                zoneVisualizer.UpdateRadii(fovealRadii, midFovealRadii);
            }
        }
    }

    void Update()
    {
        // 1) Determine current normalized gaze
        Vector2 normalizedGaze = pluginGazeActive
            ? new Vector2(-gazeUpdater.x, gazeUpdater.y)
            : GetNormalizedMousePosition();

        // 2) If allowed to override the gaze center, apply to ZoneVisualizer
        if (zoneVisualizer != null && overrideGaze && overrideZoneVisualizer)
        {
            zoneVisualizer.SetCenter(new Vector2(-normalizedGaze.x, normalizedGaze.y));
        }

        // 3) Update LODs
        UpdateLODGroups(normalizedGaze);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    /// <summary>
    /// Convert mouse pos to normalized [-1..1] screen coords
    /// </summary>
    private Vector2 GetNormalizedMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        float nx = (mousePos.x / Screen.width) * 2f - 1f;
        float ny = (mousePos.y / Screen.height) * 2f - 1f;
        return new Vector2(nx, ny);
    }

    /// <summary>
    /// For each LODGroup, measure distance from gaze and assign LOD accordingly.
    /// </summary>
    private void UpdateLODGroups(Vector2 gazePos)
    {
        if (Camera.main == null) return;

        foreach (LODGroup group in lodGroups)
        {
            // Convert object pos to normalized [-1..1] coords
            Vector3 screenPos = Camera.main.WorldToScreenPoint(group.transform.position);
            Vector2 normPos = new Vector2(
                (screenPos.x / Screen.width) * 2f - 1f,
                (screenPos.y / Screen.height) * 2f - 1f
            );

            float dx = normPos.x - gazePos.x;
            float dy = normPos.y - gazePos.y;

            bool inFoveal = IsWithinEllipse(dx, dy, fovealRadii);
            bool inMidFoveal = !inFoveal && IsWithinEllipse(dx, dy, midFovealRadii);

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

            group.ForceLOD(targetLOD);
        }
    }

    /// <summary>
    /// Returns true if (dx, dy) is within the ellipse defined by 'radii'.
    /// ellipse eq: (dx^2 / rx^2) + (dy^2 / ry^2) <= 1
    /// </summary>
    private bool IsWithinEllipse(float dx, float dy, Vector2 radii)
    {
        if (radii.x <= 0f || radii.y <= 0f) return false;
        float norm = (dx * dx) / (radii.x * radii.x) + (dy * dy) / (radii.y * radii.y);
        return (norm <= 1f);
    }
}
