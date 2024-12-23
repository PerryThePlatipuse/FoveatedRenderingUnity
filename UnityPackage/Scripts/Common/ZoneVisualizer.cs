// Assets/Plugins/Common/Scripts/ZoneVisualizer.cs

using UnityEngine;
using UnityEngine.UI;

namespace GazeTracking
{
    /// <summary>
    /// ZoneVisualizer draws two circular borders on the screen based on provided radii.
    /// Attach this to a central GameObject in your scene with a Canvas component.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ZoneVisualizer : MonoBehaviour
    {
        [Header("Circle Prefab")]
        [Tooltip("Prefab of a circular UI Image with border.")]
        public GameObject circlePrefab;

        [Header("Inner Circle Settings")]
        [Tooltip("Normalized radius for the inner circle (0 to 1).")]
        public Vector2 innerRadius = new Vector2(0.25f, 0.25f);
        [Tooltip("Color of the inner circle border.")]
        public Color innerColor = Color.red;

        [Header("Middle Circle Settings")]
        [Tooltip("Normalized radius for the middle circle (0 to 1).")]
        public Vector2 middleRadius = new Vector2(0.33f, 0.33f);
        [Tooltip("Color of the middle circle border.")]
        public Color middleColor = Color.green;

        private Image innerCircleImage;
        private Image middleCircleImage;

        private Canvas canvas;

        // UI elements for dynamic positioning
        private RectTransform innerRectTransform;
        private RectTransform middleRectTransform;

        // Last known screen size to handle resolution changes
        private Vector2 lastScreenSize;

        void Awake()
        {
            // Ensure a Canvas component is present
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Create Inner Circle
            GameObject innerCircle = Instantiate(circlePrefab, canvas.transform);
            innerCircle.name = "InnerCircle";
            innerCircleImage = innerCircle.GetComponent<Image>();
            innerCircleImage.color = innerColor;
            innerRectTransform = innerCircle.GetComponent<RectTransform>();
            SetCircleSize(innerCircleImage, innerRadius);

            // Create Middle Circle
            GameObject middleCircle = Instantiate(circlePrefab, canvas.transform);
            middleCircle.name = "MiddleCircle";
            middleCircleImage = middleCircle.GetComponent<Image>();
            middleCircleImage.color = middleColor;
            middleRectTransform = middleCircle.GetComponent<RectTransform>();
            SetCircleSize(middleCircleImage, middleRadius);

            // Initialize lastScreenSize
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }

        void Start()
        {
            // Ensure circles are correctly sized at start
            UpdateRadii(innerRadius, middleRadius);
        }

        void Update()
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (!currentScreenSize.Equals(lastScreenSize))
            {
                lastScreenSize = currentScreenSize;
                UpdateRadii(innerRadius, middleRadius);
            }
        }

        /// <summary>
        /// Sets the size of a circular Image based on normalized radii.
        /// </summary>
        /// <param name="image">The Image component to resize.</param>
        /// <param name="radius">Normalized radii (0 to 1).</param>
        private void SetCircleSize(Image image, Vector2 radius)
        {
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // Calculate diameter in pixels
            float diameterX = Screen.width * radius.x * 2.0f;
            float diameterY = Screen.height * radius.y * 2.0f;

            rect.sizeDelta = new Vector2(diameterX, diameterY);
        }

        /// <summary>
        /// Updates the radii of the inner and middle circles.
        /// </summary>
        /// <param name="newInnerRadius">New normalized radii for the inner circle.</param>
        /// <param name="newMiddleRadius">New normalized radii for the middle circle.</param>
        public void UpdateRadii(Vector2 newInnerRadius, Vector2 newMiddleRadius)
        {
            innerRadius = newInnerRadius;
            middleRadius = newMiddleRadius;

            SetCircleSize(innerCircleImage, innerRadius);
            SetCircleSize(middleCircleImage, middleRadius);
        }

        /// <summary>
        /// Sets the center position of the visualizer circles based on normalized coordinates.
        /// </summary>
        /// <param name="normalizedPosition">Normalized position (X: -1 to 1, Y: -1 to 1).</param>
        public void SetCenter(Vector2 normalizedPosition)
        {
            // Convert normalized position to screen pixels
            float screenX = (normalizedPosition.x + 1f) / 2f * Screen.width;
            float screenY = (normalizedPosition.y + 1f) / 2f * Screen.height;

            // Convert screen pixels to Canvas coordinates
            Vector2 canvasPosition = new Vector2(screenX, screenY);

            innerRectTransform.anchoredPosition = canvasPosition;
            middleRectTransform.anchoredPosition = canvasPosition;
        }
    }
}
