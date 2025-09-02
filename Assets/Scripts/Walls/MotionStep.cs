using UnityEngine;

[System.Serializable]
public class MotionStep
{
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public float duration = 1f;
    public AnimationCurve easing = AnimationCurve.Linear(0, 0, 1, 1);
}
