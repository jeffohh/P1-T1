using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAutoCollider : MonoBehaviour
{
    private SpriteRenderer sr;
    private BoxCollider2D bc2d;

    // Start is called before the first frame update
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        bc2d = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        bc2d.size = sr.size;
    }
}
