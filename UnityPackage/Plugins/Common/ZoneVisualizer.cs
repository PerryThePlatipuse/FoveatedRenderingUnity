using UnityEngine;
using UnityEngine.UI;

namespace GazeTracking
{
    /// <summary>
    /// ZoneVisualizer draws two circular borders on the screen based on provided radii.
    /// Attach this to a central GameObject in your scene.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ZoneVisualizer : MonoBehaviour
    {
        [Header("Circle Prefab")]
        [Tooltip("Prefab of a circular UI Image with border.")]
        public GameObject circlePrefab;

        [Header("Inner Circle Settings")]
        public Vector2 innerRadius = new Vector2(0.25f, 0.25f);
        public Color innerColor = Color.red;

        [Header("Middle Circle Settings")]
        public Vector2 middleRadius = new Vector2(0.33f, 0.33f);
        public Color middleColor = Color.green;

        private Image innerCircleImage;
        private Image middleCircleImage;

        private Canvas canvas;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject innerCircle = Instantiate(circlePrefab, canvas.transform);
            innerCircle.name = "InnerCircle";
            innerCircleImage = innerCircle.GetComponent<Image>();
            innerCircleImage.color = innerColor;
            SetCircleSize(innerCircleImage, innerRadius);

            GameObject middleCircle = Instantiate(circlePrefab, canvas.transform);
            middleCircle.name = "MiddleCircle";
            middleCircleImage = middleCircle.GetComponent<Image>();
            middleCircleImage.color = middleColor;
            SetCircleSize(middleCircleImage, middleRadius);
        }
        
        private void SetCircleSize(Image image, Vector2 radius)
        {
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            float diameterX = Screen.width * radius.x * 2.0f;
            float diameterY = Screen.height * radius.y * 2.0f;

            rect.sizeDelta = new Vector2(diameterX, diameterY);
        }
        
        public void UpdateRadii(Vector2 newInnerRadius, Vector2 newMiddleRadius)
        {
            innerRadius = newInnerRadius;
            middleRadius = newMiddleRadius;

            SetCircleSize(innerCircleImage, innerRadius);
            SetCircleSize(middleCircleImage, middleRadius);
        }
        
        void OnRectTransformDimensionsChange()
        {
            UpdateRadii(innerRadius, middleRadius);
        }
    }
}
