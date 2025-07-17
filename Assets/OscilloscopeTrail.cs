using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.UI;

public class SimpleOscilloscope : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform circle;
    public RectTransform container;
    private float speed = 60f;

    [Header("Y Bounds")]
    public bool enableYBounding = true;
    private float yThreshold = 100f;
    private float maxYBound = 100f;
    private float minYBound = -100f;

    [Header("Trail")]
    public RectTransform trailContainer; // Create an empty UI object for this
    private int trailLength = 300;
    private float trailThickness = 2f;

    [Header("Trail Bounds")]
    private float leftBoundary = 0f; // match container left
    private float rightBoundary;

    private float currentX = 0f;
    private bool isScrolling = false;
    private float containerWidth;
    private RectTransform[] trailPoints;
    private int trailIndex = 0;
    private float lastTrailX = 0f;
    private float lastTrailY = 0f;
    private float newY;

    private string currentObjectName;

    void Start()
    {
        currentObjectName = gameObject.name;

        // Force canvas update to get proper rect size
        Canvas.ForceUpdateCanvases();

        containerWidth = container.rect.width;

        // If still 0, try using sizeDelta
        if (containerWidth <= 0)
        {
            containerWidth = container.sizeDelta.x;
        }

        // If still 0, use a default
        if (containerWidth <= 0)
        {
            containerWidth = 290f; // Your expected width
            Debug.LogWarning("Container width was 0, using default 290");
        }

        currentX = 0f;

        // Set right boundary for trail bounds checking
        leftBoundary = -145f; // match container left
        rightBoundary = containerWidth/2f - 5f; // match trailing edge

        // Create trail points if trailContainer is assigned
        if (trailContainer != null)
        {
            CreateTrailPoints();
        }

        // Start circle at left edge
        circle.anchoredPosition = new Vector2(currentX, 0);
    }

    void CreateTrailPoints()
    {
        trailPoints = new RectTransform[trailLength];

        // Define your color list
        List<Color> lineColorList = new List<Color> {
        new Color(0.38f, 0.69f, 0.89f),  // EMG1
        new Color(0.38f, 0.69f, 0.89f),  // EMG2
        new Color(0.99f, 0.75f, 0.44f),  // EMG3
        new Color(0.99f, 0.75f, 0.44f),  // EMG4
        new Color(0.65f, 0.86f, 0.58f),  // EMG5
        new Color(0.65f, 0.86f, 0.58f),  // EMG6
        new Color(0.94f, 0.50f, 0.50f),  // EMG7
        new Color(0.94f, 0.50f, 0.50f),  // EMG8
        new Color(0.75f, 0.61f, 0.85f),  // EMG9
        new Color(0.75f, 0.61f, 0.85f),  // EMG10
        new Color(0.38f, 0.82f, 0.78f),  // EMG11
        new Color(0.38f, 0.82f, 0.78f),  // EMG12
        new Color(0.98f, 0.82f, 0.37f),  // EMG13
        new Color(0.98f, 0.82f, 0.37f),  // EMG14
        new Color(0.52f, 0.60f, 0.69f),  // EMG15
        new Color(0.52f, 0.60f, 0.69f),  // EMG16
    };

        // Parse channel number from object name (e.g., "EMG1", "EMG2", etc.)
        int channelNumber = 0;
        if (gameObject.name.StartsWith("ch"))
        {
            string numStr = gameObject.name.Substring(2);
            int.TryParse(numStr, out channelNumber);
        }

        // Default color in case parsing fails or index is out of range
        Color trailColor = new Color(0.4f, 0.0f, 0.6f, 0.8f); // fallback purple

        if (channelNumber >= 1 && channelNumber <= lineColorList.Count)
        {
            trailColor = lineColorList[channelNumber - 1];
        }

        for (int i = 0; i < trailLength; i++)
        {
            GameObject point = new GameObject($"TrailPoint_{i}");
            point.transform.SetParent(trailContainer, false);

            var image = point.AddComponent<UnityEngine.UI.Image>();
            image.color = trailColor;

            // Optional: assign anti-aliased sprite
            image.sprite = Resources.Load<Sprite>("SmoothLine");
            image.type = Image.Type.Sliced;

            RectTransform rt = point.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, trailThickness);
            rt.anchoredPosition = new Vector2(-1000, 0); // Off-screen initially

            trailPoints[i] = rt;
        }
    }

    float ApplyYBounding(float rawY)
    {
        if (!enableYBounding) return rawY;

        // Apply threshold-based clamping
        if (Mathf.Abs(rawY) > yThreshold)
        {
            return Mathf.Clamp(rawY, minYBound, maxYBound);
        }

        return rawY;
    }


    void FixedUpdate()
    {
        if (DataCollection.dataCollectionFlag)
        {
            // Generate new Y position
            string numberStr = currentObjectName.Substring(2);
            int channelNumber = int.Parse(numberStr);

            newY = EMG_Delsys.lastSample[channelNumber - 1] * DataCollection.scaleEMG *
                        DataVisualization.EMGMultiplier;
            if (channelNumber - 1 == 14)
            {
                //Debug.Log(newY);
            }

            // Apply Y bounding
            newY = ApplyYBounding(newY);

            if (!isScrolling)
            {
                // Normal movement - move circle right
                currentX += speed * Time.fixedDeltaTime;

                circle.anchoredPosition = new Vector2(currentX, newY);

                // Add to trail
                AddTrailPoint(currentX, newY);

                // Check if reached edge
                if (currentX >= containerWidth/2 - 5f)
                {
                    isScrolling = true;
                }
            }
            else
            {
                // Scrolling mode - circle stays at right edge
                circle.anchoredPosition = new Vector2(containerWidth/2 - 5f, newY);

                // Add trail point at right edge
                AddTrailPoint(containerWidth / 2 - 5f, newY);

                // Shift all trail points left
                ShiftTrailLeft();
            }
        }
    }

    void AddTrailPoint(float x, float y)
    {
        if (trailPoints == null) return;

        // Manually enforce bounds - clamp X position
        x = Mathf.Clamp(x, leftBoundary, rightBoundary);

        //Debug.Log(x);

        // Calculate distance from last point to create connecting segments
        float deltaX = x - lastTrailX;
        float deltaY = y - lastTrailY;
        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        // Only add point if we've moved enough (creates smoother line)
        if (distance > 1f || trailIndex == 0)
        {
            // Position the trail segment
            trailPoints[trailIndex].anchoredPosition = new Vector2(x, y);

            // If we have a previous point, make this segment connect to it
            if (trailIndex > 0 || isScrolling)
            {
                // Calculate angle and length for line segment
                float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;
                float length = Mathf.Max(distance, 2f); // Minimum length

                // Adjust the trail point to be a connecting line segment
                RectTransform rt = trailPoints[trailIndex];
                // Use length for width, trailThickness for height - this makes visible thick lines
                rt.sizeDelta = new Vector2(length, trailThickness);
                rt.rotation = Quaternion.Euler(0, 0, angle);

                // Position at midpoint between current and last position
                Vector2 midpoint = new Vector2((x + lastTrailX) / 2f, (y + lastTrailY) / 2f);
                rt.anchoredPosition = midpoint;
            }
            else
            {
                // First point - make it a simple thick dot
                RectTransform rt = trailPoints[trailIndex];
                rt.sizeDelta = new Vector2(trailThickness, trailThickness);
                rt.rotation = Quaternion.identity;
                rt.anchoredPosition = new Vector2(x, y);
            }

            lastTrailX = x;
            lastTrailY = y;

            // Move to next index (circular buffer)
            trailIndex = (trailIndex + 1) % trailLength;
        }
    }

    void ShiftTrailLeft()
    {

        if (trailPoints == null) return;

        float shiftAmount = speed * Time.fixedDeltaTime;

        for (int i = 0; i < trailLength; i++)
        {
            Vector2 pos = trailPoints[i].anchoredPosition;

            // Skip points that are already hidden
            if (pos.x < -500) continue;

            // Apply shift
            pos.x -= shiftAmount;

            // MANUAL BOUNDS ENFORCEMENT - Hard clamp to prevent overflow
            if (pos.x > rightBoundary)
            {
                pos.x = rightBoundary;
            }

            trailPoints[i].anchoredPosition = pos;

            // Get image component for visibility control
            var image = trailPoints[i].GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                // Simple cutoff - visible or invisible, no fading
                if (pos.x < leftBoundary)
                {
                    // Completely hide points beyond left boundary
                    image.color = new Color(0.4f, 0.0f, 0.6f, 0f);
                    pos.x = -1000; // Move far off-screen
                    trailPoints[i].anchoredPosition = pos;
                }
                else
                {
                    // Fully visible - no fading
                    image.color = new Color(0.4f, 0.0f, 0.6f, 0.8f);
                }
            }
        }
    }
}