using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FoveatedRenderingManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera PeripheralCamera;
    public Camera FovealCamera;

    [Header("Compositing")]
    public Material FoveatedCompositingMaterial;

    [Header("Rendering Settings")]
    public int RenderWidth = 1920;
    public int RenderHeight = 1080;
    [Range(1f, 120f)] public float PeripheralFOV = 60f;
    [Range(1f, 120f)] public float FovealFOV = 30f;

    [Header("Foveal Window Settings")]
    [Range(0.1f, 1f)] public float FovealWindowWidth = 0.5f;
    [Range(0.1f, 1f)] public float FovealWindowHeight = 0.5f;
    [Range(0f, 0.05f)] public float BorderThickness = 0.01f;

    [Header("Input Settings")]
    public float RotateSpeed = 30f; // degrees per second

    private RenderTexture peripheralRT;
    private RenderTexture fovealRT;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        SetupRenderTextures();
        SetupCameras();
        ApplyMaterialParameters();
    }

    void OnValidate()
    {
        SetupRenderTextures();
        SetupCameras();
        ApplyMaterialParameters();
    }

    void SetupRenderTextures()
    {
        if (peripheralRT == null || !peripheralRT.IsCreated())
        {
            if (peripheralRT != null)
                peripheralRT.Release();

            peripheralRT = new RenderTexture(240, 144, 16, RenderTextureFormat.ARGB32);
            peripheralRT.Create();
        }

        if (fovealRT == null || !fovealRT.IsCreated())
        {
            if (fovealRT != null)
                fovealRT.Release();

            fovealRT = new RenderTexture(RenderWidth, RenderHeight, 16, RenderTextureFormat.ARGB32);
            fovealRT.Create();
        }

        if (PeripheralCamera)
            PeripheralCamera.targetTexture = peripheralRT;

        if (FovealCamera)
            FovealCamera.targetTexture = fovealRT;
    }

    void SetupCameras()
    {
        if (PeripheralCamera)
        {
            PeripheralCamera.fieldOfView = PeripheralFOV;
            PeripheralCamera.enabled = true;
            PeripheralCamera.transform.position = Vector3.zero;
            PeripheralCamera.transform.rotation = Quaternion.identity;
        }

        if (FovealCamera)
        {
            FovealCamera.fieldOfView = FovealFOV;
            FovealCamera.enabled = true;
            FovealCamera.transform.position = Vector3.zero;
            FovealCamera.transform.rotation = Quaternion.identity;
            FovealCamera.backgroundColor = new Color(0,0,0,0);
        }

        Matrix4x4 projectionMatrix = FovealCamera.projectionMatrix;
        // projectionMatrix[2, 2] = -1.002f;
        // projectionMatrix[2, 3] = -2.002f;
        // FovealCamera.projectionMatrix = projectionMatrix;
        //
        // // FovealCamera.projectionMatrix = PeripheralCamera.projectionMatrix;
        // Debug.Log("Foveal");
        // Debug.Log(FovealCamera.projectionMatrix);
        // Debug.Log("Peripheral");
        // Debug.Log(PeripheralCamera.projectionMatrix);
    }

    void ApplyMaterialParameters()
    {
        if (FoveatedCompositingMaterial)
        {
            FoveatedCompositingMaterial.SetTexture("_PeripheralTex", peripheralRT);
            FoveatedCompositingMaterial.SetTexture("_FovealTex", fovealRT);
            FoveatedCompositingMaterial.SetVector("_FovealWindowSize", new Vector4(FovealWindowWidth, FovealWindowHeight, 0, 0));
            FoveatedCompositingMaterial.SetFloat("_BorderThickness", BorderThickness);
        }
    }

    void Update()
    {
        // Handle user input for gaze movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down

        yaw += horizontal * RotateSpeed * Time.deltaTime;
        pitch += vertical * RotateSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (FovealCamera)
        {
            FovealCamera.transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        }

        Vector3 fovealForward = FovealCamera.transform.forward;
        
        // Transform foveal forward direction into peripheral camera space
        Vector3 localDir = PeripheralCamera.transform.InverseTransformDirection(fovealForward);
        
        // We now have a direction in peripheral camera space. We need a position in front of the camera.
        // Let's pick a point one unit in front along this direction:
        // Ensure z > 0 for projection. If localDir.z <= 0, camera looks behind. For demonstration, we assume camera faces forward.
        // if (localDir.z <= 0.001f) localDir.z = 0.001f;  // Slight push forward if needed
        Vector3 localPos = localDir.normalized * 1.0f;   // 1 unit forward

        // Project localPos using the peripheral camera’s projection matrix
        Vector4 clipPos = PeripheralCamera.projectionMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1f);
        
        // Convert to NDC
        Vector3 ndc = new Vector3(clipPos.x / clipPos.w, clipPos.y / clipPos.w, clipPos.z / clipPos.w);
        
        // Convert NDC (-1 to 1) to UV (0 to 1)
        Vector2 uv = new Vector2(ndc.x * 0.5f + 0.5f, ndc.y * 0.5f + 0.5f);

        // uv is the center of where the foveal window should appear
        // Offset needed: how far from center (0.5, 0.5)
        Vector2 offset = uv - new Vector2(0.5f, 0.5f);

        if (FoveatedCompositingMaterial)
        {
            FoveatedCompositingMaterial.SetVector("_Offset", new Vector4(offset.x, offset.y, 0, 0));
            FoveatedCompositingMaterial.SetVector("_FovealWindowSize", new Vector4(FovealWindowWidth, FovealWindowHeight, 0, 0));
            FoveatedCompositingMaterial.SetFloat("_BorderThickness", BorderThickness);
        }
        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (FoveatedCompositingMaterial)
        {
            Graphics.Blit(null, dest, FoveatedCompositingMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
