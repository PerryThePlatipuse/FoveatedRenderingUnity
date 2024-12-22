#pragma once

#include "Enums.h"
#include <d3d11.h>
#include <nvapi.h>

// Manages Virtual Reality Shading (VRS) configurations and interactions
class VrsManager {
public:
    VrsManager();
    ~VrsManager();

    // Initialize VRS helper with the Direct3D device
    bool Initialize(ID3D11Device* device);

    // Configure shading rate preset
    void SetShadingRatePreset(ShadingRatePreset preset);

    // Configure foveation pattern preset
    void SetFoveationPatternPreset(ShadingPatternPreset preset);

    // Configure region radii for foveated rendering
    void ConfigureRegionRadii(TargetArea targetArea, float xRadius, float yRadius);

    // Configure shading rate for a specific target area
    void ConfigureShadingRate(TargetArea targetArea, ShadingRate rate);

    // Apply shading rate pattern to the device context
    void ApplyShadingRatePattern(ID3D11DeviceContext* deviceContext, NV_VRS_RENDER_MODE renderMode);

    // Remove shading rate pattern from the device context
    void RemoveShadingRatePattern(ID3D11DeviceContext* deviceContext);

    // Release VRS helper resources
    void Release();

private:
    // Internal helper methods
    void UpdateShadingRatePresetParams(NV_VRS_HELPER_ENABLE_PARAMS& enableParams);
    void UpdateFoveationPatternPresetParams(NV_VRS_HELPER_ENABLE_PARAMS& enableParams);

    ID3DNvVRSHelper* vrsHelper;

    // Configuration presets
    NV_FOVEATED_RENDERING_SHADING_RATE_PRESET shadingRatePreset;
    NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET foveationPatternPreset;

    // Custom radii for foveation regions
    float innerRegionRadii[2];
    float middleRegionRadii[2];
    float peripheralRegionRadii[2];

    // Custom shading rates for foveation regions
    NV_PIXEL_SHADING_RATE innerShadingRate;
    NV_PIXEL_SHADING_RATE middleShadingRate;
    NV_PIXEL_SHADING_RATE peripheralShadingRate;
};
