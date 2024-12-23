#include <thread>
#include <atomic>
#include <mutex>
#include <cstdlib>
#include <ctime>
#include <windows.h>

#ifdef _WIN32
#define EXPORT_API __declspec(dllexport)
#else
#error "Only Windows is supported"
#endif

namespace GazePlugin
{
    std::atomic<bool> running(false);
    std::thread gazeThread;
    std::mutex gazeMutex;
    float gazeX = 0.0f;
    float gazeY = 0.0f;

    void GetNormalizedMousePosition(float& x, float& y)
    {
        POINT cursorPos;
        if (GetCursorPos(&cursorPos))
        {
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            x = ((static_cast<float>(cursorPos.x) / screenWidth) * 2.0f) - 1.0f;
            y = 1.0f - ((static_cast<float>(cursorPos.y) / screenHeight) * 2.0f);

            if (x < -1.0f) x = -1.0f;
            if (x > 1.0f) x = 1.0f;
            if (y < -1.0f) y = -1.0f;
            if (y > 1.0f) y = 1.0f;
        }
        else
        {
            x = 0.0f;
            y = 0.0f;
        }
    }

    extern "C" EXPORT_API void InitializeGazeTracking()
    {
        if (running)
            return;

        running = true;

        gazeThread = std::thread([]() {
            while (running)
            {
                float currentX = 0.0f;
                float currentY = 0.0f;

                GetNormalizedMousePosition(currentX, currentY);

                {
                    std::lock_guard<std::mutex> lock(gazeMutex);
                    gazeX = currentX;
                    gazeY = currentY;
                }

                std::this_thread::sleep_for(std::chrono::milliseconds(16)); // ~60 FPS
            }
            });
    }

    extern "C" EXPORT_API void CleanupGazeTracking()
    {
        running = false;
        if (gazeThread.joinable())
            gazeThread.join();
    }

    extern "C" EXPORT_API void GetGazeDirection(float* x, float* y)
    {
        std::lock_guard<std::mutex> lock(gazeMutex);
        if (x) *x = gazeX;
        if (y) *y = gazeY;
    }
}
