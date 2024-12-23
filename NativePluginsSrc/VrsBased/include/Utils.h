#pragma once

#include <algorithm>

template<typename T>
inline T Clamp(const T &input, const T &lower, const T &upper) {
    return max(min(input, upper), lower);
}
