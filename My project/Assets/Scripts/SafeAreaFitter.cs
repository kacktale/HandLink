using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaFitter : MonoBehaviour
{
    private RectTransform targetRectTransform;
    private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
    private Vector2Int lastScreenSize = new Vector2Int(-1, -1);
    private ScreenOrientation lastOrientation;
    private bool isApplying;

    private void Awake()
    {
        if (!TryGetComponent(out targetRectTransform))
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        ApplyIfNeeded(force: true);
    }

    private void Update()
    {
        ApplyIfNeeded(force: false);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled && !isApplying)
        {
            ApplyIfNeeded(force: false);
        }
    }

    private void ApplyIfNeeded(bool force)
    {
        if (targetRectTransform == null)
        {
            if (!TryGetComponent(out targetRectTransform))
            {
                return;
            }
        }

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        Rect safeArea = Screen.safeArea;
        ScreenOrientation orientation = Screen.orientation;

        if (screenWidth <= 0 || screenHeight <= 0)
        {
            return;
        }

        Vector2Int screenSize = new Vector2Int(screenWidth, screenHeight);
        if (!force &&
            safeArea == lastSafeArea &&
            screenSize == lastScreenSize &&
            orientation == lastOrientation)
        {
            return;
        }

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
        lastOrientation = orientation;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x = Mathf.Clamp01(anchorMin.x / screenWidth);
        anchorMin.y = Mathf.Clamp01(anchorMin.y / screenHeight);
        anchorMax.x = Mathf.Clamp01(anchorMax.x / screenWidth);
        anchorMax.y = Mathf.Clamp01(anchorMax.y / screenHeight);

        isApplying = true;
        targetRectTransform.anchorMin = anchorMin;
        targetRectTransform.anchorMax = anchorMax;
        targetRectTransform.offsetMin = Vector2.zero;
        targetRectTransform.offsetMax = Vector2.zero;
        isApplying = false;
    }
}
