// Assets/Plugins/Common/Scripts/ZoneVisualizer.cs

using UnityEngine;
using UnityEngine.UI;

namespace GazeTracking
{
    /// <summary>
    /// ZoneVisualizer draws two elliptical borders on the screen based on provided radii.
    /// Attach this to a central GameObject in your scene with a Canvas component.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ZoneVisualizer : MonoBehaviour
    {
        [Header("Circle Prefab")]
        [Tooltip("Prefab of a circular UI Image with border. Ensure 'Preserve Aspect' is enabled.")]
        public GameObject circlePrefab;

        [Header("Inner Circle Settings")]
        [Tooltip("Normalized radii for the inner ellipse (0 to 1).")]
        public Vector2 innerRadius = new Vector2(0.25f, 0.25f);
        [Tooltip("Color of the inner ellipse border.")]
        public Color innerColor = Color.red;

        [Header("Middle Circle Settings")]
        [Tooltip("Normalized radii for the middle ellipse (0 to 1).")]
        public Vector2 middleRadius = new Vector2(0.33f, 0.33f);
        [Tooltip("Color of the middle ellipse border.")]
        public Color middleColor = Color.red;

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

            if (circlePrefab == null)
            {
                Debug.LogError("ZoneVisualizer: Circle Prefab is not assigned.");
                return;
            }

            // Create Inner Ellipse
            GameObject innerCircle = Instantiate(circlePrefab, canvas.transform);
            innerCircle.name = "InnerEllipse";
            innerCircleImage = innerCircle.GetComponent<Image>();
            if (innerCircleImage == null)
            {
                Debug.LogError("ZoneVisualizer: Circle Prefab does not contain an Image component.");
                return;
            }
            innerCircleImage.color = innerColor;
            innerRectTransform = innerCircle.GetComponent<RectTransform>();
            SetEllipseSize(innerRectTransform, innerRadius);

            // Create Middle Ellipse
            GameObject middleCircle = Instantiate(circlePrefab, canvas.transform);
            middleCircle.name = "MiddleEllipse";
            middleCircleImage = middleCircle.GetComponent<Image>();
            if (middleCircleImage == null)
            {
                Debug.LogError("ZoneVisualizer: Circle Prefab does not contain an Image component.");
                return;
            }
            middleCircleImage.color = middleColor;
            middleRectTransform = middleCircle.GetComponent<RectTransform>();
            SetEllipseSize(middleRectTransform, middleRadius);

            // Initialize lastScreenSize
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }

        void Start()
        {
            // Ensure ellipses are correctly sized at start
            if (innerCircleImage != null && middleCircleImage != null)
            {
                UpdateRadii(innerRadius, middleRadius);
            }
            else
            {
                Debug.LogWarning("ZoneVisualizer: Ellipse images are not initialized properly.");
            }
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
        /// Sets the size of an elliptical Image based on normalized radii.
        /// </summary>
        /// <param name="rect">The RectTransform component to resize.</param>
        /// <param name="radii">Normalized radii (0 to 1) for X and Y axes.</param>
        private void SetEllipseSize(RectTransform rect, Vector2 radii)
        {
            if (rect == null)
            {
                Debug.LogError("ZoneVisualizer: Attempted to set size on a null RectTransform.");
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // Calculate size in pixels based on normalized radii
            float width = Screen.width * radii.x * 2.0f;
            float height = Screen.height * radii.y * 2.0f;

            rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// Updates the radii of the inner and middle ellipses.
        /// </summary>
        /// <param name="newInnerRadius">New normalized radii for the inner ellipse.</param>
        /// <param name="newMiddleRadius">New normalized radii for the middle ellipse.</param>
        public void UpdateRadii(Vector2 newInnerRadius, Vector2 newMiddleRadius)
        {
            innerRadius = newInnerRadius;
            middleRadius = newMiddleRadius;

            SetEllipseSize(innerRectTransform, innerRadius);
            SetEllipseSize(middleRectTransform, middleRadius);
        }

        /// <summary>
        /// Sets the center position of the visualizer ellipses based on screen pixel coordinates.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels.</param>
        public void SetCenter(Vector2 screenPosition)
        {
            screenPosition.x = ()screenPosition.x + 1) / 2;
            screenPosition.y = ()screenPosition.y + 1) / 2;
            if (canvas == null)
            {
                Debug.LogError("ZoneVisualizer: Canvas component is missing.");
                return;
            }

            // Convert screen position to canvas anchored position
            Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
            Vector2 anchoredPosition = screenPosition - new Vector2(canvasSize.x / 2f, canvasSize.y / 2f);

            innerRectTransform.anchoredPosition = anchoredPosition;
            middleRectTransform.anchoredPosition = anchoredPosition;
        }
    }
}
