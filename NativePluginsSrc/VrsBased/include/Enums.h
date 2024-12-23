#pragma once

// Event Identifiers for Render Events
enum class EventID {
    ENABLE_FOVEATED_RENDERING,
    DISABLE_FOVEATED_RENDERING,
    UPDATE_GAZE
};

// Target Areas for Foveated Rendering
enum class TargetArea {
    INNER,
    MIDDLE,
    PERIPHERAL
};

// Shading Rate Presets for Performance and Quality Balancing
enum class ShadingRatePreset {
    HIGHEST_PERFORMANCE = 1,  // 1x1  |  2x2  |  4x4
    HIGH_PERFORMANCE = 2,     // 1x1  |  2x2  |  2x2
    BALANCED = 3,             // 4xSS |  1x1  |  2x2
    HIGH_QUALITY = 4,         // 4xSS |  2xSS |  1x1
    HIGHEST_QUALITY = 5,      // 8xSS |  4xSS |  2xSS
    CUSTOM = 6,               // Default same as HIGHEST_PERFORMANCE
    MAX = CUSTOM
};

// Foveation Pattern Presets for Different Viewing Patterns
enum class ShadingPatternPreset {
    WIDE = 1,
    BALANCED = 2,
    NARROW = 3,
    CUSTOM = 4,
    MAX = CUSTOM
};

// Shading Rates for Different Pixel Coverage
enum class ShadingRate {
    CULL,               // No shading
    X16_PER_PIXEL,      // 16x supersampling
    X8_PER_PIXEL,       // 8x supersampling
    X4_PER_PIXEL,       // 4x supersampling
    X2_PER_PIXEL,       // 2x supersampling
    X1_PER_PIXEL,       // 1 shading pass / 1 pixel (normal shading)
    X1_PER_2X1_PIXELS,  // 1 shading pass / 2 pixels horizontally
    X1_PER_1X2_PIXELS,  // 1 shading pass / 2 pixels vertically
    X1_PER_2X2_PIXELS,  // 1 shading pass / 4 pixels
    X1_PER_4X2_PIXELS,  // 1 shading pass / 8 pixels
    X1_PER_2X4_PIXELS,  // 1 shading pass / 8 pixels
    X1_PER_4X4_PIXELS   // 1 shading pass / 16 pixels
};
