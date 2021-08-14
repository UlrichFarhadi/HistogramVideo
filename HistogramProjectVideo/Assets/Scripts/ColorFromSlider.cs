using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorFromSlider : MonoBehaviour
{
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public GameObject startPixel;

    public GameObject pixelR;
    public GameObject pixelG;
    public GameObject pixelB;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pixelR.GetComponent<Renderer>().material.color = new Color(sliderR.value / 255, 0.0f, 0.0f, 1.0f);
        pixelG.GetComponent<Renderer>().material.color = new Color(0.0f, sliderG.value / 255, 0.0f, 1.0f);
        pixelB.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, sliderB.value / 255, 1.0f);

        startPixel.GetComponent<Renderer>().material.color = new Color(sliderR.value / 255, sliderG.value / 255, sliderB.value / 255, 1.0f);
    }
}
