using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class GameplayAspectController : MonoBehaviour
{
    private const float TargetAspect = 9f / 16f;
    private const float SpawnOutsidePadding = 1.2f;
    private const int MaskSortingOrder = -1000;

    private readonly Image[] masks = new Image[4];
    private readonly List<CanvasState> uiCanvasStates = new List<CanvasState>();

    private Camera gameplayCamera;
    private Transform target;
    private GameObject maskRoot;
    private Vector2Int lastScreenSize = new Vector2Int(-1, -1);

    public void Configure(Camera camera, Transform gameplayTarget)
    {
        gameplayCamera = camera;
        target = gameplayTarget;
        ApplyIfNeeded(force: true);
    }

    private void Update()
    {
        ApplyIfNeeded(force: false);
    }

    public bool TryGetSpawnPosition(Vector2 spawnDirection, out Vector2 spawnPosition)
    {
        spawnPosition = default;
        if (gameplayCamera == null || target == null || spawnDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        GetWorldBounds(out Vector2 center, out float halfWidth, out float halfHeight);
        Vector2 direction = spawnDirection.normalized;
        float distanceToVerticalEdge = Mathf.Abs(direction.x) > 0.0001f
            ? halfWidth / Mathf.Abs(direction.x)
            : float.PositiveInfinity;
        float distanceToHorizontalEdge = Mathf.Abs(direction.y) > 0.0001f
            ? halfHeight / Mathf.Abs(direction.y)
            : float.PositiveInfinity;
        float distanceToEdge = Mathf.Min(distanceToVerticalEdge, distanceToHorizontalEdge);

        spawnPosition = center + direction * (distanceToEdge + SpawnOutsidePadding);
        return true;
    }

    public float GetAspectAdjustedTravelDuration(
        Vector2 spawnPosition,
        Vector2 targetPosition,
        float baseDuration)
    {
        if (gameplayCamera == null || baseDuration <= 0f)
        {
            return baseDuration;
        }

        GetWorldBounds(out _, out float halfWidth, out float halfHeight);
        Vector2 direction = (spawnPosition - targetPosition).normalized;
        float horizontalWeight = Mathf.Abs(direction.x);
        float verticalWeight = Mathf.Abs(direction.y);
        float weightSum = horizontalWeight + verticalWeight;
        if (weightSum <= 0.0001f)
        {
            return baseDuration;
        }

        float horizontalLength = halfWidth * 2f;
        float verticalLength = halfHeight * 2f;
        float directionLength =
            (horizontalLength * horizontalWeight + verticalLength * verticalWeight) /
            weightSum;
        float longestAxisLength = Mathf.Max(horizontalLength, verticalLength);
        return baseDuration * (longestAxisLength / Mathf.Max(0.0001f, directionLength));
    }

    private void ApplyIfNeeded(bool force)
    {
        if (gameplayCamera == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        if (!force && screenSize == lastScreenSize)
        {
            return;
        }

        lastScreenSize = screenSize;
        bool shouldUsePillarbox = screenSize.x > screenSize.y;
        Rect viewport = shouldUsePillarbox
            ? CalculateViewport((float)screenSize.x / screenSize.y)
            : new Rect(0f, 0f, 1f, 1f);
        gameplayCamera.rect = viewport;
        UpdateMasks(viewport, shouldUsePillarbox);
        ApplyUiPresentation(shouldUsePillarbox);
    }

    private static Rect CalculateViewport(float screenAspect)
    {
        if (screenAspect > TargetAspect)
        {
            float width = TargetAspect / screenAspect;
            return new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }

        float height = screenAspect / TargetAspect;
        return new Rect(0f, (1f - height) * 0.5f, 1f, height);
    }

    private void GetWorldBounds(out Vector2 center, out float halfWidth, out float halfHeight)
    {
        float depth = Mathf.Abs(Vector3.Dot(
            target.position - gameplayCamera.transform.position,
            gameplayCamera.transform.forward));
        Vector3 centerPoint = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
        Vector3 leftPoint = gameplayCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
        Vector3 rightPoint = gameplayCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
        Vector3 bottomPoint = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
        Vector3 topPoint = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));

        center = new Vector2(centerPoint.x, centerPoint.y);
        halfWidth = Vector3.Distance(leftPoint, rightPoint) * 0.5f;
        halfHeight = Vector3.Distance(bottomPoint, topPoint) * 0.5f;
    }

    private void UpdateMasks(Rect viewport, bool shouldUsePillarbox)
    {
        if (!shouldUsePillarbox)
        {
            if (maskRoot != null)
            {
                maskRoot.SetActive(false);
            }

            return;
        }

        EnsureMasks();
        maskRoot.SetActive(true);
        SetMask(0, new Vector2(0f, 0f), new Vector2(viewport.xMin, 1f));
        SetMask(1, new Vector2(viewport.xMax, 0f), new Vector2(1f, 1f));
        masks[2].gameObject.SetActive(false);
        masks[3].gameObject.SetActive(false);
    }

    private void EnsureMasks()
    {
        if (masks[0] != null)
        {
            return;
        }

        maskRoot = new GameObject("AspectRatioMasks", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = maskRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = MaskSortingOrder;

        for (int index = 0; index < masks.Length; index++)
        {
            GameObject maskObject = new GameObject(
                "Mask",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            maskObject.transform.SetParent(maskRoot.transform, false);
            Image mask = maskObject.GetComponent<Image>();
            mask.color = Color.black;
            mask.raycastTarget = false;
            masks[index] = mask;
        }
    }

    private void SetMask(int index, Vector2 anchorMin, Vector2 anchorMax)
    {
        Image mask = masks[index];
        RectTransform rectTransform = mask.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        mask.gameObject.SetActive(
            anchorMax.x - anchorMin.x > 0.0001f &&
            anchorMax.y - anchorMin.y > 0.0001f);
    }

    private void ApplyUiPresentation(bool shouldUsePillarbox)
    {
        CacheRootCanvases();
        for (int index = uiCanvasStates.Count - 1; index >= 0; index--)
        {
            CanvasState state = uiCanvasStates[index];
            if (state.Canvas == null)
            {
                uiCanvasStates.RemoveAt(index);
                continue;
            }

            state.Canvas.renderMode = shouldUsePillarbox
                ? RenderMode.ScreenSpaceCamera
                : state.OriginalRenderMode;
            state.Canvas.worldCamera = shouldUsePillarbox
                ? gameplayCamera
                : state.OriginalWorldCamera;
            state.Canvas.planeDistance = shouldUsePillarbox
                ? 1f
                : state.OriginalPlaneDistance;
        }
    }

    private void CacheRootCanvases()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null ||
                canvas.transform.parent != null ||
                (maskRoot != null && canvas.gameObject == maskRoot) ||
                HasCachedCanvas(canvas))
            {
                continue;
            }

            uiCanvasStates.Add(new CanvasState(canvas));
        }
    }

    private bool HasCachedCanvas(Canvas canvas)
    {
        foreach (CanvasState state in uiCanvasStates)
        {
            if (state.Canvas == canvas)
            {
                return true;
            }
        }

        return false;
    }

    private sealed class CanvasState
    {
        public readonly Canvas Canvas;
        public readonly RenderMode OriginalRenderMode;
        public readonly Camera OriginalWorldCamera;
        public readonly float OriginalPlaneDistance;

        public CanvasState(Canvas canvas)
        {
            Canvas = canvas;
            OriginalRenderMode = canvas.renderMode;
            OriginalWorldCamera = canvas.worldCamera;
            OriginalPlaneDistance = canvas.planeDistance;
        }
    }
}
