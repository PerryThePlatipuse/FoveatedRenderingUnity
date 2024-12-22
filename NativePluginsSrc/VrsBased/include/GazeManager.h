#pragma once

#include "Vector.h"
#include "Enums.h"
#include <d3d11.h>
#include <nvapi.h>

// Manages gaze data updates and interactions with NVidia VRS Gaze Handler
class GazeManager {
public:
    GazeManager();
    ~GazeManager();

    // Initialize the gaze handler with specified FOVs
    bool Initialize(ID3D11Device* device, float horizontalFov, float verticalFov);

    // Update normalized gaze directions
    void UpdateGazeDirections(const Vector3& leftGazeDir, const Vector3& rightGazeDir, float tanHalfHorizontalFov, float tanHalfVerticalFov, float stabilityThreshold);

    // Refresh gaze data in the NVidia VRS system
    bool RefreshGazeData(ID3D11DeviceContext* deviceContext);

    // Commit gaze data to the VRS Helper
    void CommitGazeData(ID3D11DeviceContext* deviceContext);

    // Release gaze handler resources
    void Release();

private:
    // Calculate normalized gaze location based on direction and offset
    Vector2 CalculateNormalizedGaze(const Vector3& gazeDirNormalized, float offset, float tanHalfHorizontalFov, float tanHalfVerticalFov);

    // Check if gaze has changed beyond the threshold
    bool HasGazeChanged(Vector2* current, const Vector2& newGaze, float threshold);

    ID3DNvGazeHandler* gazeHandler;
    Vector2 leftGazePos;
    Vector2 rightGazePos;
    float gazeStabilityThreshold;
};
