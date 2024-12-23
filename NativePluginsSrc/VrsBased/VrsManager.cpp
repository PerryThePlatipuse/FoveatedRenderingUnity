#include "VrsManager.h"
#include "Utils.h"
#include "Enums.h"

// Constructor
VrsManager::VrsManager()
    : vrsHelper(nullptr),
    shadingRatePreset(NV_FOVEATED_RENDERING_SHADING_RATE_PRESET_HIGHEST_PERFORMANCE),
    foveationPatternPreset(NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET_NARROW),
    innerRegionRadii{ 0.25f, 0.25f },
    middleRegionRadii{ 0.33f, 0.33f },
    peripheralRegionRadii{ 1.0f, 1.0f },
    innerShadingRate(NV_PIXEL_X1_PER_RASTER_PIXEL),
    middleShadingRate(NV_PIXEL_X1_PER_1X2_RASTER_PIXELS),
    peripheralShadingRate(NV_PIXEL_X1_PER_2X2_RASTER_PIXELS) {
}

// Destructor
VrsManager::~VrsManager() {
    Release();
}

bool VrsManager::Initialize(ID3D11Device* device) {
    NV_VRS_HELPER_INIT_PARAMS vrsInitParams = {};
    vrsInitParams.version = NV_VRS_HELPER_INIT_PARAMS_VER;
    vrsInitParams.ppVRSHelper = &vrsHelper;

    NvAPI_Status status = NvAPI_D3D_InitializeVRSHelper(device, &vrsInitParams);
    return (status == NVAPI_OK);
}

void VrsManager::SetShadingRatePreset(ShadingRatePreset preset) {
    shadingRatePreset = static_cast<NV_FOVEATED_RENDERING_SHADING_RATE_PRESET>(Clamp(
        static_cast<int>(preset),
        static_cast<int>(NV_FOVEATED_RENDERING_SHADING_RATE_PRESET_HIGHEST_PERFORMANCE),
        static_cast<int>(NV_FOVEATED_RENDERING_SHADING_RATE_PRESET_CUSTOM)
    ));
}

void VrsManager::SetFoveationPatternPreset(ShadingPatternPreset preset) {
    foveationPatternPreset = static_cast<NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET>(Clamp(
        static_cast<int>(preset),
        static_cast<int>(NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET_WIDE),
        static_cast<int>(NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET_CUSTOM)
    ));
}

void VrsManager::ConfigureRegionRadii(TargetArea targetArea, float xRadius, float yRadius) {
    float clampedX = Clamp(xRadius, 0.01f, 10.0f);
    float clampedY = Clamp(yRadius, 0.01f, 10.0f);

    switch (targetArea) {
    case TargetArea::INNER:
        innerRegionRadii[0] = clampedX;
        innerRegionRadii[1] = clampedY;
        break;
    case TargetArea::MIDDLE:
        middleRegionRadii[0] = clampedX;
        middleRegionRadii[1] = clampedY;
        break;
    case TargetArea::PERIPHERAL:
    default:
        peripheralRegionRadii[0] = clampedX;
        peripheralRegionRadii[1] = clampedY;
        break;
    }
}

void VrsManager::ConfigureShadingRate(TargetArea targetArea, ShadingRate rate) {
    NV_PIXEL_SHADING_RATE clampedRate = static_cast<NV_PIXEL_SHADING_RATE>(Clamp(
        static_cast<int>(rate),
        static_cast<int>(NV_PIXEL_X0_CULL_RASTER_PIXELS),
        static_cast<int>(NV_PIXEL_X1_PER_4X4_RASTER_PIXELS)
    ));

    switch (targetArea) {
    case TargetArea::INNER:
        innerShadingRate = clampedRate;
        break;
    case TargetArea::MIDDLE:
        middleShadingRate = clampedRate;
        break;
    case TargetArea::PERIPHERAL:
    default:
        peripheralShadingRate = clampedRate;
        break;
    }
}

void VrsManager::ApplyShadingRatePattern(ID3D11DeviceContext* deviceContext, NV_VRS_RENDER_MODE renderMode) {
    if (vrsHelper && deviceContext) {
        NV_VRS_HELPER_ENABLE_PARAMS enableParams = {};
        enableParams.version = NV_VRS_HELPER_ENABLE_PARAMS_VER;
        enableParams.RenderMode = renderMode;
        enableParams.ContentType = NV_VRS_CONTENT_TYPE_FOVEATED_RENDERING;
        enableParams.sFoveatedRenderingDesc.version = NV_FOVEATED_RENDERING_DESC_VER;

        UpdateShadingRatePresetParams(enableParams);
        UpdateFoveationPatternPresetParams(enableParams);

        // Enable VRS with the configured parameters
        NvAPI_Status status = vrsHelper->Enable(deviceContext, &enableParams);
        // Handle errors if needed (e.g., log or attempt recovery)
    }
}

void VrsManager::RemoveShadingRatePattern(ID3D11DeviceContext* deviceContext) {
    if (vrsHelper && deviceContext) {
        NV_VRS_HELPER_DISABLE_PARAMS disableParams = {};
        disableParams.version = NV_VRS_HELPER_DISABLE_PARAMS_VER;

        NvAPI_Status status = vrsHelper->Disable(deviceContext, &disableParams);
        // Handle errors if needed (e.g., log or attempt recovery)
    }
}

void VrsManager::UpdateShadingRatePresetParams(NV_VRS_HELPER_ENABLE_PARAMS& enableParams) {
    enableParams.sFoveatedRenderingDesc.ShadingRatePreset = shadingRatePreset;
    if (shadingRatePreset == NV_FOVEATED_RENDERING_SHADING_RATE_PRESET_CUSTOM) {
        auto customShading = &(enableParams.sFoveatedRenderingDesc.ShadingRateCustomPresetDesc);
        customShading->version = NV_FOVEATED_RENDERING_CUSTOM_SHADING_RATE_PRESET_DESC_VER1;
        customShading->InnerMostRegionShadingRate = innerShadingRate;
        customShading->MiddleRegionShadingRate = middleShadingRate;
        customShading->PeripheralRegionShadingRate = peripheralShadingRate;
    }
}

void VrsManager::UpdateFoveationPatternPresetParams(NV_VRS_HELPER_ENABLE_PARAMS& enableParams) {
    enableParams.sFoveatedRenderingDesc.FoveationPatternPreset = foveationPatternPreset;
    if (foveationPatternPreset == NV_FOVEATED_RENDERING_FOVEATION_PATTERN_PRESET_CUSTOM) {
        auto customPattern = &(enableParams.sFoveatedRenderingDesc.FoveationPatternCustomPresetDesc);
        customPattern->version = NV_FOVEATED_RENDERING_CUSTOM_FOVEATION_PATTERN_PRESET_DESC_VER1;
        customPattern->fInnermostRadii[0] = innerRegionRadii[0];
        customPattern->fInnermostRadii[1] = innerRegionRadii[1];
        customPattern->fMiddleRadii[0] = middleRegionRadii[0];
        customPattern->fMiddleRadii[1] = middleRegionRadii[1];
        customPattern->fPeripheralRadii[0] = peripheralRegionRadii[0];
        customPattern->fPeripheralRadii[1] = peripheralRegionRadii[1];
    }
}

void VrsManager::Release() {
    if (vrsHelper) {
        vrsHelper->Release();
        vrsHelper = nullptr;
    }
}
