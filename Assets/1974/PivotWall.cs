using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotWall : MonoBehaviour
{
    public float rotationStep = 90f;
    public float rotationDuration = 0.25f;
    private bool isRotating = false;

    public void Hit(Vector2 hitPoint)
    {
        if (!isRotating)
        {
            Vector2 localHit = transform.InverseTransformPoint(hitPoint);
            float direction = 0f;
            
            if (Mathf.Abs(localHit.x) > Mathf.Abs(localHit.y))
            {
                direction = localHit.x > 0 ? -rotationStep : rotationStep;
            }
            else
            {
                direction = localHit.y > 0 ? rotationStep : -rotationStep;
            }

            StartCoroutine(RotateWall(direction));
        }
    }

    private IEnumerator RotateWall(float angle)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);

        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, (elapsedTime / rotationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }
}
