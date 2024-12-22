using UnityEngine;
using System.Linq;

public class FoveatedLODController : MonoBehaviour
{
    private LODGroup[] lodGroups;

    // Ellipse dimensions (in screen pixels)
    public float fovealEllipseWidth = 100f;
    public float fovealEllipseHeight = 100f;
    public float midFovealEllipseWidth = 300f;
    public float midFovealEllipseHeight = 300f;

    private Texture2D fovealEllipseBorderTexture;
    private Texture2D midFovealEllipseBorderTexture;

    void Start()
    {
#if UNITY_2023_1_OR_NEWER
        lodGroups = FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        // If Unity version < 2023.1, fallback:
        lodGroups = FindObjectsOfType<LODGroup>();
#endif

        // Create ellipse border textures
        // We'll create separate textures for foveal and mid-foveal ellipses
        fovealEllipseBorderTexture = CreateRedEllipseBorderTexture(128, 128, fovealEllipseWidth, fovealEllipseHeight, Color.red);
        midFovealEllipseBorderTexture = CreateRedEllipseBorderTexture(256, 256, midFovealEllipseWidth, midFovealEllipseHeight, Color.red);

        // Note: The texture dimensions (128x128, 256x256) are arbitrary. 
        // They are just source textures; we’ll still scale them via GUI.DrawTexture. 
        // The ellipse equation inside CreateRedEllipseBorderTexture ensures a correct ellipse shape.
    }

    void Update()
    {
        if (Camera.main == null)
            return;

        Vector3 mousePos = Input.mousePosition;
        foreach (LODGroup group in lodGroups)
        {
            // Convert object to screen space
            Vector3 screenPos = Camera.main.WorldToScreenPoint(group.transform.position);

            // Check elliptical regions
            bool inFoveal = IsInsideEllipse(screenPos, mousePos, fovealEllipseWidth, fovealEllipseHeight);
            bool inMidFoveal = !inFoveal && IsInsideEllipse(screenPos, mousePos, midFovealEllipseWidth, midFovealEllipseHeight);

            int targetLOD;
            if (inFoveal)
            {
                targetLOD = 1;
            }
            else if (inMidFoveal)
            {
                targetLOD = 2;
            }
            else
            {

                targetLOD = 3;
            }

            LOD[] lods = group.GetLODs();
            targetLOD = Mathf.Clamp(targetLOD, 0, lods.Length - 1);
            group.ForceLOD(targetLOD);
        }
    }

    void OnGUI()
    {
        // Mouse position in Input is bottom-left origin, GUI is top-left origin
        Vector3 mousePos = Input.mousePosition;
        float guiMouseY = Screen.height - mousePos.y;

        // Draw foveal ellipse border
        DrawEllipse(guiMouseY, mousePos.x, fovealEllipseWidth, fovealEllipseHeight, fovealEllipseBorderTexture);

        // Draw mid-foveal ellipse border
        DrawEllipse(guiMouseY, mousePos.x, midFovealEllipseWidth, midFovealEllipseHeight, midFovealEllipseBorderTexture);
    }

    private void DrawEllipse(float guiMouseY, float mouseX, float width, float height, Texture2D tex)
    {
        float x = mouseX - width * 0.5f;
        float y = guiMouseY - height * 0.5f;
        GUI.DrawTexture(new Rect(x, y, width, height), tex);
    }

    private bool IsInsideEllipse(Vector3 pos, Vector3 center, float width, float height)
    {
        float dx = pos.x - center.x;
        float dy = pos.y - center.y;
        float a = width * 0.5f;
        float b = height * 0.5f;
        float ellipseVal = (dx * dx) / (a * a) + (dy * dy) / (b * b);
        return ellipseVal <= 1f;
    }

    private Texture2D CreateRedEllipseBorderTexture(int texWidth, int texHeight, float ellipseWidth, float ellipseHeight, Color borderColor)
    {
        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        float a = ellipseWidth * 0.5f;
        float b = ellipseHeight * 0.5f;

        // We'll consider the texture coordinates in a normalized [-0.5, 0.5] range
        // and map them to ellipse coordinates. This ensures a proper ellipse shape 
        // independent of texture size.
        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                // Normalize coords to center-based
                float nx = (x - texWidth * 0.5f);
                float ny = (y - texHeight * 0.5f);

                // Scale them to represent actual ellipse ratio
                // Here we just use nx, ny directly to decide the border.
                // We'll scale ellipse to a circle in texture space and rely on draw scaling.
                // Actually, to get a perfect ellipse outline in the texture itself,
                // we use the ellipse equation directly:
                float ellipseVal = (nx * nx) / (a * a) + (ny * ny) / (b * b);

                // We'll define a narrow band as the border
                // ellipseVal = 1 is exact boundary.
                // We'll consider pixels where ellipseVal ~ 1 within a small margin as border.
                float delta = Mathf.Abs(ellipseVal - 1f);

                // Let's say a border thickness of about 2 pixels in texture space
                // The thickness in screen space depends on how we scale the texture
                if (delta < 0.03f) 
                {
                    tex.SetPixel(x, y, borderColor);
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        return tex;
    }
}
