#include <iostream>
#include <iomanip>
#include <chrono>
#include <thread>
#include <windows.h>
#include "eyeware/tracker_client.h"

int main() {
    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
    int screenHeight = GetSystemMetrics(SM_CYSCREEN);

    eyeware::TrackerClient tracker;

    while (true) {
        if (tracker.connected()) {
            auto head_pose = tracker.get_head_pose_info();
            auto screen_gaze = tracker.get_screen_gaze_info();

            if (!screen_gaze.is_lost) {
                float normalizedX = static_cast<float>(screen_gaze.x) / screenWidth;
                float normalizedY = static_cast<float>(screen_gaze.y) / screenHeight;

                std::cout << "<x=" << std::fixed << std::setprecision(4) << normalizedX
                          << ", y=" << std::fixed << std::setprecision(4) << normalizedY
                          << ">" << std::endl;
            }

            std::this_thread::sleep_for(std::chrono::milliseconds(16));
        } else {
            const int MESSAGE_PERIOD_IN_SECONDS = 2;
            std::this_thread::sleep_for(std::chrono::seconds(MESSAGE_PERIOD_IN_SECONDS));
            std::cout << "No connection with tracker server" << std::endl;
        }
    }

    return 0;
}
