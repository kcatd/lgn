using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackgroundImage : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Sprite[]   images;

    Image                       bgImage;

    // Start is called before the first frame update
    void Start()
    {
        bgImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Randomize()
    {
        bgImage.sprite = images[Random.Range(0, images.Length)];
    }
}
