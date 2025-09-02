using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructWall : MonoBehaviour
{
    public virtual void Destruct()
    {
        // Default implementation: simply destroy the wall
        Destroy(gameObject);
    }
}
