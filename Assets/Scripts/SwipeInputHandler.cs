using UnityEngine;

public class SwipeInputHandler : MonoBehaviour
{
    public PlayerController playerController;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float minSwipeDistance = 50f; // Minimum swipe distance to trigger

    void Update()
    {
        HandleSwipeInput();
    }

    void HandleSwipeInput()
    {
#if UNITY_WEBGL || UNITY_EDITOR
        // WebGL and Editor: use mouse input
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            DetectSwipe();
        }
#else
        // Android: use touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                startTouchPosition = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                DetectSwipe();
            }
        }
#endif
    }

    void DetectSwipe()
    {
        Vector2 delta = endTouchPosition - startTouchPosition;

        if (delta.magnitude < minSwipeDistance)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal Swipe
            if (delta.x > 0)
                playerController.Right();
            else
                playerController.Left();
        }
        else
        {
            // Vertical Swipe
            if (delta.y > 0)
                playerController.Jump();
            else
                playerController.Slide();
        }
    }
}
