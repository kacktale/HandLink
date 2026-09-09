using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

internal readonly struct PointerInputSnapshot
{
    public PointerInputSnapshot(
        bool isPressed,
        bool isBlockedByUi,
        Vector2 screenPosition,
        Vector2 screenDelta)
    {
        IsPressed = isPressed;
        IsBlockedByUi = isBlockedByUi;
        ScreenPosition = screenPosition;
        ScreenDelta = screenDelta;
    }

    public bool IsPressed { get; }
    public bool IsBlockedByUi { get; }
    public bool IsGameplayPressed => IsPressed && !IsBlockedByUi;
    public Vector2 ScreenPosition { get; }
    public Vector2 ScreenDelta { get; }
}

internal static class PointerInputService
{
    private static int lastUpdatedFrame = -1;
    private static bool hasPreviousPosition;
    private static Vector2 previousScreenPosition;
    private static PointerInputSnapshot currentSnapshot;
    private static readonly List<RaycastResult> UiRaycastResults =
        new List<RaycastResult>(8);
    private static PointerEventData pointerEventData;
    private static EventSystem cachedEventSystem;

    public static PointerInputSnapshot Read()
    {
        if (lastUpdatedFrame == Time.frameCount)
        {
            return currentSnapshot;
        }

        lastUpdatedFrame = Time.frameCount;
        Pointer pointer = Pointer.current;
        bool isPressed = pointer != null && pointer.press.isPressed;

        if (!isPressed)
        {
            hasPreviousPosition = false;
            currentSnapshot =
                new PointerInputSnapshot(
                    false,
                    false,
                    Vector2.zero,
                    Vector2.zero);
            return currentSnapshot;
        }

        Vector2 screenPosition = pointer.position.ReadValue();
        bool isBlockedByUi = IsOverBlockingUi(screenPosition);
        if (isBlockedByUi)
        {
            hasPreviousPosition = false;
            currentSnapshot =
                new PointerInputSnapshot(
                    true,
                    true,
                    screenPosition,
                    Vector2.zero);
            return currentSnapshot;
        }

        Vector2 screenDelta = Vector2.zero;
        if (hasPreviousPosition)
        {
            screenDelta = previousScreenPosition - screenPosition;
        }

        previousScreenPosition = screenPosition;
        hasPreviousPosition = true;
        currentSnapshot =
            new PointerInputSnapshot(
                true,
                false,
                screenPosition,
                screenDelta);
        return currentSnapshot;
    }

    public static bool TryGetScreenPosition(out Vector2 screenPosition)
    {
        PointerInputSnapshot snapshot = Read();
        screenPosition = snapshot.ScreenPosition;
        return snapshot.IsGameplayPressed;
    }

    private static bool IsOverBlockingUi(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        if (cachedEventSystem != eventSystem || pointerEventData == null)
        {
            cachedEventSystem = eventSystem;
            pointerEventData = new PointerEventData(eventSystem);
        }

        pointerEventData.position = screenPosition;
        UiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, UiRaycastResults);

        for (int index = 0; index < UiRaycastResults.Count; index++)
        {
            if (UiRaycastResults[index]
                    .gameObject
                    .GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }
}

public class InputAxis : MonoBehaviour
{
    public bool gameStarted = false;

    [SerializeField, Min(0.1f)]
    private float horizontalInputSpeedMultiplier = 1f;

    protected Vector3 pointerWorldPosition;
    protected Vector2 distanceValue;

    private Camera inputCamera;
    private bool wasGameplayPressed;

    public virtual void Update()
    {
        PointerInputSnapshot snapshot = PointerInputService.Read();
        bool didBeginPress =
            snapshot.IsGameplayPressed && !wasGameplayPressed;
        wasGameplayPressed = snapshot.IsGameplayPressed;
        gameStarted = snapshot.IsGameplayPressed;
        distanceValue = Vector2.zero;

        if (!snapshot.IsGameplayPressed)
        {
            return;
        }

        if (didBeginPress)
        {
            GameAudio.Instance?.PlayMoveStart();
        }

        if (inputCamera == null)
        {
            inputCamera = Camera.main;
        }

        if (inputCamera == null)
        {
            return;
        }

        float worldPlaneDistance = -inputCamera.transform.position.z;
        pointerWorldPosition = inputCamera.ScreenToWorldPoint(
            new Vector3(
                snapshot.ScreenPosition.x,
                snapshot.ScreenPosition.y,
                worldPlaneDistance));

        Vector2 previousScreenPosition =
            snapshot.ScreenPosition + snapshot.ScreenDelta;
        Vector3 previousWorldPosition = inputCamera.ScreenToWorldPoint(
            new Vector3(
                previousScreenPosition.x,
                previousScreenPosition.y,
                worldPlaneDistance));
        distanceValue = previousWorldPosition - pointerWorldPosition;

        // Portrait displays expose a much narrower world width than height.
        // Compensate only the constrained axis so an identical physical drag
        // has comparable in-game movement in both directions.
        float horizontalAspectCompensation = Mathf.Max(
            1f,
            1f / Mathf.Max(0.1f, inputCamera.aspect));
        distanceValue.x *= horizontalAspectCompensation *
            horizontalInputSpeedMultiplier;
    }

    protected void ResetPointerInputTracking()
    {
        wasGameplayPressed = false;
    }
}
