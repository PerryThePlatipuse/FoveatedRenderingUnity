#pragma once

#include <d3d11.h>
#include <nvapi.h>

// Wrapper class for NVidia API initialization and device registration
class NvApiWrapper {
public:
    // Initializes NVidia API and registers the Direct3D 11 device
    bool Initialize(ID3D11Device* device);

    // Unloads NVidia API
    void Unload();

private:
    // Registers the Direct3D 11 device with NVidia API
    bool RegisterDevice(ID3D11Device* device);
};
