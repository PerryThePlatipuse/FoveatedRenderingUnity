#include "PluginInterface.h"
#include "Enums.h"
#include "Utils.h"
#include <IUnityGraphicsD3D11.h>

// Singleton instance of PluginInterface
static PluginInterface* s_pluginInstance = nullptr;

// Constructor
PluginInterface::PluginInterface()
    : unityInterfaces(nullptr), unityGraphics(nullptr), device(nullptr), renderEventHandler(nullptr),
    tanHalfHorizontalFov(1.0f), tanHalfVerticalFov(1.0f) { // Initialize to 1.0f
    s_pluginInstance = this;
}

// Destructor
PluginInterface::~PluginInterface() {
    Unload();
    s_pluginInstance = nullptr;
}

// Initialize and load plugin components
void PluginInterface::Load(IUnityInterfaces* unityInterfacesPtr) {
    unityInterfaces = unityInterfacesPtr;
    unityGraphics = unityInterfaces->Get<IUnityGraphics>();
    unityGraphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

    // Initialize the graphics device upon plugin load
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Unload and clean up plugin components
void PluginInterface::Unload() {
    if (unityGraphics) {
        unityGraphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        unityGraphics = nullptr;
    }

    // Release foveated rendering resources
    ReleaseFoveatedRendering();

    if (renderEventHandler) {
        delete renderEventHandler;
        renderEventHandler = nullptr;
    }

    // Unregister NVidia API
    nvApiWrapper.Unload();

    unityInterfaces = nullptr;
}

// Handle Unity render events
void PluginInterface::HandleRenderEvent(int eventID) {
    if (renderEventHandler && device) {
        ID3D11DeviceContext* deviceContext = nullptr;
        device->GetImmediateContext(&deviceContext);

        renderEventHandler->HandleEvent(static_cast<EventID>(eventID), deviceContext, vrsManager.GetVrsHelper());

        // Release the device context after operations
        deviceContext->Release();
    }
}

// Initialize foveated rendering
bool PluginInterface::InitializeFoveatedRendering(float verticalFov, float aspectRatio) {
    if (device && !vrsManager.Initialize(device)) {
        return false;
    }

    // Calculate tangent of half FOV angles
    const float DEG2RAD = 0.01745329f;
    float halfVerticalFovRad = DEG2RAD * verticalFov / 2.0f;
    tanHalfVerticalFov = tanf(halfVerticalFovRad);
    tanHalfHorizontalFov = tanHalfVerticalFov * aspectRatio;

    // Initialize Gaze Manager
    if (!gazeManager.Initialize(device, tanHalfHorizontalFov, tanHalfVerticalFov)) {
        return false;
    }

    // Initialize Render Event Handler
    renderEventHandler = new RenderEventHandler(&vrsManager, &gazeManager);

    return true;
}

// Release foveated rendering resources
void PluginInterface::ReleaseFoveatedRendering() {
    gazeManager.Release();
    vrsManager.Release();

    if (renderEventHandler) {
        delete renderEventHandler;
        renderEventHandler = nullptr;
    }
}

// Configuration APIs
void PluginInterface::SetShadingRatePreset(ShadingRatePreset preset) {
    vrsManager.SetShadingRatePreset(preset);
}

void PluginInterface::SetFoveationPatternPreset(ShadingPatternPreset preset) {
    vrsManager.SetFoveationPatternPreset(preset);
}

void PluginInterface::ConfigureRegionRadii(TargetArea targetArea, float xRadius, float yRadius) {
    vrsManager.ConfigureRegionRadii(targetArea, xRadius, yRadius);
}

void PluginInterface::ConfigureShadingRate(TargetArea targetArea, ShadingRate rate) {
    vrsManager.ConfigureShadingRate(targetArea, rate);
}

void PluginInterface::UpdateGazeDirection(const Vector3& gazeDir) {
    gazeManager.UpdateGazeDirection(gazeDir, tanHalfHorizontalFov, tanHalfVerticalFov, 0.05f);
}

// Static callback function forwarding to instance method
void UNITY_INTERFACE_API PluginInterface::OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType) {
    if (s_pluginInstance) {
        s_pluginInstance->HandleGraphicsDeviceEventInternal(eventType);
    }
}

// Internal method to handle graphics device events
void PluginInterface::HandleGraphicsDeviceEventInternal(UnityGfxDeviceEventType eventType) {
    switch (eventType) {
    case kUnityGfxDeviceEventInitialize:
        if (unityInterfaces) {
            // Retrieve the Direct3D 11 device from Unity
            device = unityInterfaces->Get<IUnityGraphicsD3D11>()->GetDevice();
            nvApiWrapper.Initialize(device);
        }
        break;
    case kUnityGfxDeviceEventShutdown:
        // Unload NVidia API upon device shutdown
        nvApiWrapper.Unload();
        device = nullptr;
        break;
    }
}
