namespace FoveatedRenderingVRS
{

    /// <summary>
    /// Defines various event identifiers for foveated rendering operations.
    /// </summary>
    public enum FoveatedEventID
    {
        ENABLE_FOVEATED_RENDERING,
        DISABLE_FOVEATED_RENDERING,
        UPDATE_GAZE
    };

    /// <summary>
    /// Specifies target areas for foveated rendering.
    /// </summary>
    public enum TargetArea
    {
        INNER,
        MIDDLE,
        PERIPHERAL
    };

    /// <summary>
    /// Preset options for shading rates.
    /// </summary>
    public enum ShadingRatePreset
    {
        SHADING_RATE_HIGHEST_PERFORMANCE = 1, // 1x1  |  2x2  |  4x4
        SHADING_RATE_HIGH_PERFORMANCE = 2,    // 1x1  |  2x2  |  2x2
        SHADING_RATE_BALANCED = 3,            // 4xSS |  1x1  |  2x2
        SHADING_RATE_HIGH_QUALITY = 4,        // 4xSS |  2xSS |  1x1
        SHADING_RATE_HIGHEST_QUALITY = 5,     // 8xSS |  4xSS |  2xSS
        SHADING_RATE_CUSTOM = 6,              // Custom shading rates
        SHADING_RATE_MAX = SHADING_RATE_CUSTOM
    };

    /// <summary>
    /// Preset options for shading patterns.
    /// </summary>
    public enum ShadingPatternPreset
    {
        SHADING_PATTERN_WIDE = 1,
        SHADING_PATTERN_BALANCED = 2,
        SHADING_PATTERN_NARROW = 3,
        SHADING_PATTERN_CUSTOM = 4,
        SHADING_PATTERN_MAX = SHADING_PATTERN_CUSTOM
    };

    /// <summary>
    /// Defines various shading rates.
    /// </summary>
    public enum ShadingRate
    {
        CULL,               // No shading
        SUPESAMPLING_X16,   // 16x supersampling
        SUPESAMPLING_X8,    // 8x supersampling
        SUPESAMPLING_X4,    // 4x supersampling
        SUPESAMPLING_X2,    // 2x supersampling
        NORMAL,             // 1 shading pass / 1 pixel (normal shading)
        REDUCTION_2X1,      // 1 shading pass / 2 pixels horizontally
        REDUCTION_1X2,      // 1 shading pass / 2 pixels vertically
        REDUCTION_2X2,      // 1 shading pass / 4 pixels
        REDUCTION_4X2,      // 1 shading pass / 8 pixels
        REDUCTION_2X4,      // 1 shading pass / 8 pixels
        REDUCTION_4X4       // 1 shading pass / 16 pixels
    };
}
