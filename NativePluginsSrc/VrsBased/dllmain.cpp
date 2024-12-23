#include "Enums.h"
#include "PluginInterface.h"
#include "Vector.h"
#include <IUnityGraphics.h>
#include <IUnityGraphicsD3D11.h>
#include <IUnityInterface.h>
#include <string>

// Singleton instance of PluginInterface
static PluginInterface *s_plugin = nullptr;
static const std::string PLUGIN_NAME = "VrsBased";
static const std::string VERSION = "1.0";

// Unity Plugin Interface Functions
extern "C" {
// UnityPluginLoad: Called by Unity when the plugin is loaded
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces *unityInterfaces) {
    if (!s_plugin) {
        s_plugin = new PluginInterface();
        s_plugin->Load(unityInterfaces);
    }
}

// UnityPluginUnload: Called by Unity when the plugin is unloaded
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
    if (s_plugin) {
        s_plugin->Unload();
        delete s_plugin;
        s_plugin = nullptr;
    }
}

// GetRenderEventFunc: Retrieves the render event function for Unity
UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc() {
    return [](int eventID) {
        if (s_plugin) {
            s_plugin->HandleRenderEvent(eventID);
        }
    };
}

// VRS APIs exposed to Unity

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API InitializeFoveatedRendering(float verticalFov, float aspectRatio) {
    if (s_plugin) {
        return s_plugin->InitializeFoveatedRendering(verticalFov, aspectRatio);
    }
    return false;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ReleaseFoveatedRendering() {
    if (s_plugin) {
        s_plugin->ReleaseFoveatedRendering();
    }
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetShadingRatePreset(ShadingRatePreset preset) {
    if (s_plugin) {
        s_plugin->SetShadingRatePreset(preset);
    }
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetFoveationPatternPreset(ShadingPatternPreset preset) {
    if (s_plugin) {
        s_plugin->SetFoveationPatternPreset(preset);
    }
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ConfigureRegionRadii(TargetArea targetArea, Vector2 radii) {
    if (s_plugin) {
        s_plugin->ConfigureRegionRadii(targetArea, radii.x, radii.y);
    }
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ConfigureShadingRate(TargetArea targetArea, ShadingRate rate) {
    if (s_plugin) {
        s_plugin->ConfigureShadingRate(targetArea, rate);
    }
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UpdateGazeDirection(Vector3 gazeDir) {
    if (s_plugin) {
        s_plugin->UpdateGazeDirection(gazeDir);
    }
}
}
