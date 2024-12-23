#include "GazeManager.h"
#include "Utils.h"

// Constructor
GazeManager::GazeManager()
    : gazeHandler(nullptr), gazePos{0.0f, 0.0f}, gazeStabilityThreshold(0.05f) {
}

// Destructor
GazeManager::~GazeManager() {
    Release();
}

bool GazeManager::Initialize(ID3D11Device *device, float tanHalfHorizontalFov, float tanHalfVerticalFov) {
    NV_GAZE_HANDLER_INIT_PARAMS gazeInitParams = {};
    gazeInitParams.version = NV_GAZE_HANDLER_INIT_PARAMS_VER;
    gazeInitParams.GazeDataDeviceId = 0;
    gazeInitParams.GazeDataType = NV_GAZE_DATA_MONO;  // Mono mode
    gazeInitParams.fHorizontalFOV = tanHalfHorizontalFov;
    gazeInitParams.fVericalFOV = tanHalfVerticalFov;
    gazeInitParams.ppNvGazeHandler = &gazeHandler;

    NvAPI_Status status = NvAPI_D3D_InitializeNvGazeHandler(device, &gazeInitParams);
    return (status == NVAPI_OK);
}

void GazeManager::UpdateGazeDirection(const Vector3 &gazeDirNormalized, float tanHalfHorizontalFov, float tanHalfVerticalFov, float stabilityThreshold) {
    gazeStabilityThreshold = stabilityThreshold;

    Vector2 newGaze = CalculateNormalizedGaze(gazeDirNormalized, tanHalfHorizontalFov, tanHalfVerticalFov);

    HasGazeChanged(&gazePos, newGaze, gazeStabilityThreshold);
}

bool GazeManager::RefreshGazeData(ID3D11DeviceContext *deviceContext) {
    if (gazeHandler) {
        static unsigned long long gazeTimestamp = 0;

        NV_FOVEATED_RENDERING_UPDATE_GAZE_DATA_PARAMS gazeDataParams = {};
        gazeDataParams.version = NV_FOVEATED_RENDERING_UPDATE_GAZE_DATA_PARAMS_VER;
        gazeDataParams.Timestamp = ++gazeTimestamp;

        // Update Gaze Data
        gazeDataParams.sMonoData.version = NV_FOVEATED_RENDERING_GAZE_DATA_PER_EYE_VER;
        gazeDataParams.sMonoData.fGazeNormalizedLocation[0] = gazePos.x;
        gazeDataParams.sMonoData.fGazeNormalizedLocation[1] = gazePos.y;
        gazeDataParams.sMonoData.GazeDataValidityFlags = NV_GAZE_LOCATION_VALID;

        // Send Gaze Data to NVidia VRS Handler
        NvAPI_Status status = gazeHandler->UpdateGazeData(deviceContext, &gazeDataParams);
        return (status == NVAPI_OK);
    }

    return false;
}

void GazeManager::CommitGazeData(ID3DNvVRSHelper *vrsHelper, ID3D11DeviceContext *deviceContext) {
    if (vrsHelper && deviceContext) {
        NV_VRS_HELPER_LATCH_GAZE_PARAMS latchParams = {};
        latchParams.version = NV_VRS_HELPER_LATCH_GAZE_PARAMS_VER;

        NvAPI_Status status = vrsHelper->LatchGaze(deviceContext, &latchParams);
    }
}

Vector2 GazeManager::CalculateNormalizedGaze(const Vector3 &gazeDirNormalized, float tanHalfHorizontalFov, float tanHalfVerticalFov) {
    float normalizedX = (gazeDirNormalized.x / gazeDirNormalized.z) / tanHalfHorizontalFov;
    float normalizedY = (gazeDirNormalized.y / gazeDirNormalized.z) / tanHalfVerticalFov;

    return {-normalizedX / 2.0f, normalizedY / 2.0f};
}

bool GazeManager::HasGazeChanged(Vector2 *current, const Vector2 &newGaze, float threshold) {
    if (abs(current->x - newGaze.x) <= threshold &&
        abs(current->y - newGaze.y) <= threshold) {
        return false;
    }

    current->x = newGaze.x;
    current->y = newGaze.y;
    return true;
}

void GazeManager::Release() {
    if (gazeHandler) {
        gazeHandler->Release();
        gazeHandler = nullptr;
    }
}
