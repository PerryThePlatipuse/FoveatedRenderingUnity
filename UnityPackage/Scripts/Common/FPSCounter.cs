using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    private float pollingTime = 0.2f;
    private int frameCount = 0;
    private float timeElapsed = 0f;

    void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        if (fpsText == null)
        {
            Debug.LogError("FPSCounter: TextMeshProUGUI is not assigned!");
        }
    }

    void Update()
    {
        frameCount++;
        timeElapsed += Time.unscaledDeltaTime;

        if (timeElapsed >= pollingTime)
        {
            int fps = Mathf.RoundToInt(frameCount / timeElapsed);
            fpsText.text = $"{fps} FPS";
            frameCount = 0;
            timeElapsed = 0f;
        }
    }
}