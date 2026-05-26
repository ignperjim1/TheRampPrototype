using System;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    [SerializeField, Range(0.05f, 0.5f)] private float swipeThresholdScreenFraction = 0.15f;

    public event Action<SwipeDirection> OnSwipe;

    private Vector2 startPosition;
    private bool waitingForRelease;
    private bool wasPressed;

    private void Update()
    {
        bool isPressed = Input.GetMouseButton(0);

        if (isPressed && !wasPressed)
        {
            startPosition = Input.mousePosition;
            waitingForRelease = false;
        }

        if (isPressed && !waitingForRelease)
        {
            Vector2 delta = (Vector2)Input.mousePosition - startPosition;
            float thresholdPixels = swipeThresholdScreenFraction * Screen.height;

            if (delta.magnitude >= thresholdPixels)
            {
                OnSwipe?.Invoke(ResolveDirection(delta));
                waitingForRelease = true;
            }
        }

        if (!isPressed && wasPressed)
        {
            waitingForRelease = false;
        }

        wasPressed = isPressed;
    }

    private static SwipeDirection ResolveDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
    }
}
