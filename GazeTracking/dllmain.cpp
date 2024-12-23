#include <thread>
#include <atomic>
#include <mutex>
#include <cstdlib>
#include <ctime>

// Define export macros
#ifdef _WIN32
#define EXPORT_API __declspec(dllexport)
#else
#error "Only win supported"
#endif

namespace GazePlugin
{
    std::atomic<bool> running(false);
    std::thread gazeThread;
    std::mutex gazeMutex;
    float gazeX = 0.0f;
    float gazeY = 0.0f;

    extern "C" EXPORT_API void InitializeGazeTracking()
    {
        if (running)
            return;

        running = true;
        srand(static_cast<unsigned int>(time(0)));

        gazeThread = std::thread([]() {
            while (running)
            {
                {
                    std::lock_guard<std::mutex> lock(gazeMutex);
                    // Simulate gaze data
                    gazeX = static_cast<float>((rand() % 100) / 100.0f - 0.5f) * 2.0f; // -1 to +1
                    gazeY = static_cast<float>((rand() % 100) / 100.0f - 0.5f) * 2.0f; // -1 to +1
                }
                std::this_thread::sleep_for(std::chrono::seconds(1));
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
