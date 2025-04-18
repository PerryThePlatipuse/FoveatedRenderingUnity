// Assets/Plugins/Common/Scripts/ZoneVisualizer.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace GazeTracking
{
    /// <summary>
    /// ZoneVisualizer draws two elliptical borders on the screen based on provided radii.
    /// Attach this to a central GameObject in your scene with a Canvas component.
    /// </summary>
    /// 
    [RequireComponent(typeof(Canvas))]
    [DefaultExecutionOrder(1000)]
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

        [Header("Visualization Control")]
        [Tooltip("Toggle to enable or disable the visualization.")]
        [SerializeField]
        public bool isVisualizationEnabled = true;

        /// <summary>
        /// Property to get or set the visualization enabled state.
        /// Automatically updates the visualization when set.
        /// </summary>
        public bool IsVisualizationEnabled
        {
            get => isVisualizationEnabled;
            set
            {
                if (isVisualizationEnabled != value)
                {
                    isVisualizationEnabled = value;
                    SetVisualizationEnabled(isVisualizationEnabled);
                }
            }
        }

        private Image innerCircleImage;
        private Image middleCircleImage;

        private Canvas canvas;

        // UI elements for dynamic positioning
        private RectTransform innerRectTransform;
        private RectTransform middleRectTransform;

        // References to the ellipse GameObjects
        private GameObject innerCircle;
        private GameObject middleCircle;

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
            innerCircle = Instantiate(circlePrefab, canvas.transform);
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
            middleCircle = Instantiate(circlePrefab, canvas.transform);
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
            // Initialize visualization state
            SetVisualizationEnabled(isVisualizationEnabled);

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

            // Optional: Toggle visualization with the "V" key for testing
            if (Input.GetKeyDown(KeyCode.V))
            {
                IsVisualizationEnabled = !IsVisualizationEnabled;
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
            if (canvas == null)
            {
                Debug.LogError("ZoneVisualizer: Canvas component is missing.");
                return;
            }
            Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
            screenPosition.x = canvasSize.x * (-screenPosition.x) / 2;
            screenPosition.y = canvasSize.y * (screenPosition.y) / 2;

            innerRectTransform.anchoredPosition = screenPosition;
            middleRectTransform.anchoredPosition = screenPosition;
        }

        /// <summary>
        /// Enables or disables the visualization of the ellipses.
        /// </summary>
        /// <param name="enabled">True to show the visualization, false to hide it.</param>
        public void SetVisualizationEnabled(bool enabled)
        {
            if (innerCircle != null && middleCircle != null)
            {
                innerCircle.SetActive(enabled);
                middleCircle.SetActive(enabled);
            }
            else
            {
                Debug.LogWarning("ZoneVisualizer: Ellipse GameObjects are not initialized.");
            }
        }
    }
}
