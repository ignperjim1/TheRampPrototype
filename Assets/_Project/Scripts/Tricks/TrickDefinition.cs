using UnityEngine;

[CreateAssetMenu(fileName = "Trick", menuName = "TheRamp/Trick Definition")]
public class TrickDefinition : ScriptableObject
{
    public string trickName = "Kickflip";
    public SwipeDirection swipeDirection = SwipeDirection.Down;
    public Vector3 rotationAxis = Vector3.forward;
    public float rotationDegrees = 360f;
    public float duration = 0.5f;
    public int score = 100;
}
