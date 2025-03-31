// GazeTracking/GazePythonUpdater.cs

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

namespace GazeTracking
{
    // Assuming GazeUpdater is an abstract class with the following abstract methods:
    // public abstract void Initialize();
    // public abstract void Cleanup();
    // public abstract Vector2 GetGazeDirectionVector();
    public class GazePythonUpdater : GazeUpdater
    {
        [Header("Network Settings")]
        public int port = 50666;
        public bool showDebug = true;

        [Header("Gaze Data")]
        public Vector2 gazeCoordinates;
        public bool isConnected = false;

        private UdpClient _udpClient;
        private Thread _receiveThread;
        private bool _threadRunning = false;
        private IPEndPoint _remoteEndPoint;

        public override void Initialize()
        {
            // Initialize UDP receiver
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            InitializeReceiver();

            if (showDebug)
                Debug.Log("[GazePythonUpdater] Initialized on port " + port);
        }

        private void InitializeReceiver()
        {
            try
            {
                _receiveThread = new Thread(new ThreadStart(ReceiveData));
                _receiveThread.IsBackground = true;
                _threadRunning = true;
                _receiveThread.Start();
                isConnected = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GazePythonUpdater] Init error: {e.Message}");
                isConnected = false;
            }
        }

        private void ReceiveData()
        {
            try
            {
                _udpClient = new UdpClient(port);

                if (showDebug)
                    Debug.Log($"[GazePythonUpdater] Listening on port {port}...");

                while (_threadRunning)
                {
                    try
                    {
                        byte[] receivedBytes = _udpClient.Receive(ref _remoteEndPoint);
                        string receivedData = Encoding.UTF8.GetString(receivedBytes);

                        if (showDebug)
                            Debug.Log($"[GazePythonUpdater] Raw data: {receivedData}");

                        ParseCoordinates(receivedData);
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode != 10004 && showDebug)
                            Debug.LogWarning($"[GazePythonUpdater] Socket error: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        if (showDebug)
                            Debug.LogError($"[GazePythonUpdater] Error: {e.Message}");
                    }
                }
            }
            finally
            {
                if (_udpClient != null)
                    _udpClient.Close();
            }
        }

        private void ParseCoordinates(string data)
        {
            try
            {
                string[] parts = data.Split(',');
                if (parts.Length == 2)
                {
                    if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    {
                        gazeCoordinates = new Vector2(x, y);

                        if (showDebug)
                            Debug.Log($"[GazePythonUpdater] Valid coordinates: {gazeCoordinates}");
                    }
                    else if (showDebug)
                    {
                        Debug.LogWarning($"[GazePythonUpdater] Parse error in: {data}");
                    }
                }
                else if (showDebug)
                {
                    Debug.LogWarning($"[GazePythonUpdater] Invalid format: {data}");
                }
            }
            catch (Exception e)
            {
                if (showDebug)
                    Debug.LogError($"[GazePythonUpdater] Parse exception: {e.Message}");
            }
        }

        public override Vector2 GetGazeDirectionVector()
        {
            // Return the latest gaze coordinates
            return gazeCoordinates;
        }

        public override void Cleanup()
        {
            _threadRunning = false;

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                if (!_receiveThread.Join(1000))
                {
                    if (showDebug)
                        Debug.LogWarning("[GazePythonUpdater] Force thread abort");
                    _receiveThread.Abort();
                }
            }

            if (_udpClient != null)
                _udpClient.Close();

            isConnected = false;

            if (showDebug)
                Debug.Log("[GazePythonUpdater] Shutdown complete");
        }
    }
}