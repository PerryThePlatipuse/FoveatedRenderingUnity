#include "NvApiWrapper.h"

// Initialize NVidia API and register the Direct3D device
bool NvApiWrapper::Initialize(ID3D11Device *device) {
    NvAPI_Status status = NvAPI_Initialize();
    if (status != NVAPI_OK) {
        return false;
    }

    return RegisterDevice(device);
}

// Register the Direct3D 11 device with NVidia API
bool NvApiWrapper::RegisterDevice(ID3D11Device *device) {
    NvAPI_Status status = NvAPI_D3D_RegisterDevice(device);
    if (status != NVAPI_OK) {
        return false;
    }
    return true;
}

// Unload NVidia API
void NvApiWrapper::Unload() {
    NvAPI_Unload();
}
