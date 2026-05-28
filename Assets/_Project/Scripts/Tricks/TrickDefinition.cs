using UnityEngine;

[CreateAssetMenu(fileName = "Trick", menuName = "TheRamp/Trick Definition")]
public class TrickDefinition : ScriptableObject
{
    public string trickName = "Kickflip";
    public SwipeDirection swipeDirection = SwipeDirection.Down;

    // Degrees rotated around each LOCAL axis over the trick, applied
    // simultaneously: x = pitch (end-over-end), y = yaw (spin), z = roll (flip).
    public Vector3 rotationDegrees = new Vector3(0f, 0f, 360f);
    public float duration = 0.5f;
    public int score = 100;
}
