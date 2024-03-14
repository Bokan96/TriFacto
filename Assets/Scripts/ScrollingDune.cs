using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ScrollingDune : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    private Material material;

    void Start()
    {
        Image image = GetComponent<Image>();
        material = image.material;
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        material.mainTextureOffset = new Vector2(offset, 0);
    }
}
