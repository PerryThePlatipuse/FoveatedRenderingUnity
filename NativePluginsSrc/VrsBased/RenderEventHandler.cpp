#include "RenderEventHandler.h"

RenderEventHandler::RenderEventHandler(VrsManager *vrsMgr, GazeManager *gazeMgr)
    : vrsManager(vrsMgr), gazeManager(gazeMgr) {
}

RenderEventHandler::~RenderEventHandler() {
}

void RenderEventHandler::HandleEvent(EventID eventID, ID3D11DeviceContext *deviceContext, ID3DNvVRSHelper *vrsHelper) {
    switch (eventID) {
        case EventID::ENABLE_FOVEATED_RENDERING:
            vrsManager->ApplyShadingRatePattern(deviceContext, NV_VRS_RENDER_MODE_MONO);
            break;
        case EventID::DISABLE_FOVEATED_RENDERING:
            vrsManager->RemoveShadingRatePattern(deviceContext);
            break;
        case EventID::UPDATE_GAZE:
            if (gazeManager->RefreshGazeData(deviceContext)) {
                gazeManager->CommitGazeData(vrsHelper, deviceContext);
            }
            break;
        default:
            break;
    }
}
