using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAutoCollider : MonoBehaviour
{
    private SpriteRenderer sr;
    private BoxCollider2D bc2d;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        BoxCollider2D bc2d = gameObject.AddComponent<BoxCollider2D>();
        bc2d.size = sr.size;
    }
}
