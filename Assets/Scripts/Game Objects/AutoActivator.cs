using UnityEngine;

public class AutoActivator : MonoBehaviour
{

    public GameObject targetObject;

    public float delay = 3f;

    private void Start()
    {
        Invoke(nameof(ActivateObject), delay);
    }

    private void ActivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[AutoActivator] NO targetObject！");
        }
    }
}
