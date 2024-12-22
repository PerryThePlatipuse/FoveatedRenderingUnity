#pragma once

#include "VrsManager.h"
#include "GazeManager.h"
#include "RenderEventHandler.h"
#include "Enums.h"
#include "Vector.h"
#include <d3d11.h>
#include <IUnityInterface.h>
#include <IUnityGraphics.h>
#include <IUnityGraphicsD3D11.h>
#include "NvApiWrapper.h"

// Manages Unity plugin lifecycle and interactions
class PluginInterface {
public:
    PluginInterface();
    ~PluginInterface();

    // Initialize and load plugin components
    void Load(IUnityInterfaces* unityInterfaces);

    // Unload and clean up plugin components
    void Unload();

    // Handle Unity render events
    void HandleRenderEvent(int eventID);

    // Initialize foveated rendering
    bool InitializeFoveatedRendering(float verticalFov, float aspectRatio);

    // Release foveated rendering resources
    void ReleaseFoveatedRendering();

    // Configuration APIs
    void SetShadingRatePreset(ShadingRatePreset preset);
    void SetFoveationPatternPreset(ShadingPatternPreset preset);
    void ConfigureRegionRadii(TargetArea targetArea, float xRadius, float yRadius);
    void ConfigureShadingRate(TargetArea targetArea, ShadingRate rate);
    void UpdateGazeDirection(const Vector3& leftGazeDir, const Vector3& rightGazeDir);

private:
    // Callback for graphics device events
    static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

    // Internal method to handle graphics device events
    void HandleGraphicsDeviceEventInternal(UnityGfxDeviceEventType eventType);

    // Unity Graphics Interface
    IUnityInterfaces* unityInterfaces;
    IUnityGraphics* unityGraphics;
    ID3D11Device* device;

    // Managers
    VrsManager vrsManager;
    GazeManager gazeManager;
    RenderEventHandler* renderEventHandler;

    // NVidia API Wrapper
    NvApiWrapper nvApiWrapper;
};
