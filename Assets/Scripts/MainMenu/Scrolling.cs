using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scrolling : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;

    private void Update()
    {
        _img.uvRect = new Rect(_img.uvRect.x + _x * Time.deltaTime, _img.uvRect.y + _y * Time.deltaTime, _img.uvRect.width, _img.uvRect.height);
    }
}
