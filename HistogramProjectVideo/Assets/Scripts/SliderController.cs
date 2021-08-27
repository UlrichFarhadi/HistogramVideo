using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator animSlider;

    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;

    public int sliderAnimR = 43;
    public int sliderAnimG = 148;
    public int sliderAnimB = 128;

    int state = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            state++;

        }


            switch (state)
        {
            case 1:
                animSlider.SetTrigger("StartFadeToYellow");
                sliderR.value = sliderAnimR;
                sliderG.value = sliderAnimG;
                sliderB.value = sliderAnimB;
                //state++;
                break;
            case 2:
                animSlider.SetTrigger("StartFadeToMagenta");
                sliderR.value = sliderAnimR;
                sliderG.value = sliderAnimG;
                sliderB.value = sliderAnimB;
                break;
            case 3:
                animSlider.SetTrigger("StartFadeToCyan");
                sliderR.value = sliderAnimR;
                sliderG.value = sliderAnimG;
                sliderB.value = sliderAnimB;
                break;
            case 4:
                animSlider.SetTrigger("StartFadeToBlack");
                sliderR.value = sliderAnimR;
                sliderG.value = sliderAnimG;
                sliderB.value = sliderAnimB;
                break;
            case 5:
                animSlider.SetTrigger("StartFadeToWhite");
                sliderR.value = sliderAnimR;
                sliderG.value = sliderAnimG;
                sliderB.value = sliderAnimB;
                break;
            default:
                break;
        }

    }
}
