#pragma once

#include "Enums.h"
#include "VrsManager.h"
#include "GazeManager.h"
#include <d3d11.h>

// Handles render events from Unity and interacts with VrsManager and GazeManager
class RenderEventHandler {
public:
    RenderEventHandler(VrsManager* vrsMgr, GazeManager* gazeMgr);
    ~RenderEventHandler();

    // Handle specific render event based on EventID
    void HandleEvent(EventID eventID, ID3D11DeviceContext* deviceContext);

private:
    VrsManager* vrsManager;
    GazeManager* gazeManager;
};
