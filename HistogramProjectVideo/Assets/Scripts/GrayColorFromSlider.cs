using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrayColorFromSlider : MonoBehaviour
{
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public GameObject grayPixel;

    public Text textGray;

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
        
        float grayColor = (sliderR.value * 0.299f / 255.0f) + (sliderG.value * 0.587f / 255.0f) + (sliderB.value * 0.114f / 255.0f);
        grayPixel.GetComponent<Renderer>().material.color = new Color(grayColor, grayColor, grayColor, 1.0f);

        textGray.text = ((int)(grayColor * 255.0f)).ToString();
    }
}
