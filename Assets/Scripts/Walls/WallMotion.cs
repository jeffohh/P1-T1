using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMotion : MonoBehaviour
{
    public List<MotionStep> motionSteps = new List<MotionStep>();
    public bool loop = true;
    public bool playOnStart = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (playOnStart)
        {
            StartCoroutine(PlaySequence());
        }
    }

    public void ResetPosition()
    {
        StopAllCoroutines();
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    public IEnumerator PlaySequence()
    {
        do
        {
            Vector3 currentPos = initialPosition;
            Quaternion currentRot = initialRotation;

            foreach (MotionStep step in motionSteps)
            {
                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;

                Vector3 endPos = startPos + step.positionOffset;
                Quaternion endRot = startRot * Quaternion.Euler(step.rotationOffset);

                float time = 0;
                while (time < step.duration)
                {
                    float t = step.easing.Evaluate(time / step.duration);
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                    time += Time.deltaTime;
                    yield return null;
                }

                // Ensure final state
                transform.position = endPos;
                transform.rotation = endRot;
            }

        } while (loop);
    }
}
