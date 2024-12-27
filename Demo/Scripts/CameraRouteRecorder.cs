using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

[Serializable]
public class CameraKeyframe
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public CameraKeyframe(Vector3 pos, Quaternion rot, float t)
    {
        position = pos;
        rotation = rot;
        time = t;
    }
}

[Serializable]
public class CameraRoute
{
    public List<CameraKeyframe> keyframes = new List<CameraKeyframe>();
}


public class CameraRouteRecorder : MonoBehaviour
{
    public float recordInterval = 0.2f;
    private CameraRoute route = new CameraRoute();
    private bool isRecording = false;

    private void Update()
    {
        // Start recording when the user presses the "R" key
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }
    }

    private void StartRecording()
    {
        route.keyframes.Clear();
        isRecording = true;
        StartCoroutine(RecordRoutine());
        Debug.Log("Recording started.");
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
        SaveRoute();
        Debug.Log("Recording stopped and route saved.");
    }

    private IEnumerator RecordRoutine()
    {
        while (isRecording)
        {
            CameraKeyframe keyframe = new CameraKeyframe(
                transform.position,
                transform.rotation,
                Time.time
            );
            route.keyframes.Add(keyframe);
            yield return new WaitForSeconds(recordInterval);
        }
    }

    private void SaveRoute()
    {
        string json = JsonUtility.ToJson(route, true);
        string path = Path.Combine(Application.dataPath, "CameraRouteMountains.json");
        File.WriteAllText(path, json);
        Debug.Log($"Route saved to {path}");
    }
}
