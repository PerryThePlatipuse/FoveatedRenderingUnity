#pragma once

#include <algorithm>

// Clamp utility function template to constrain values within a range
template<typename T>
inline T Clamp(const T& input, const T& lower, const T& upper) {
    return std::max(std::min(input, upper), lower);
}
