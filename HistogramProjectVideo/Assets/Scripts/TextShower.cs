using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextShower : MonoBehaviour
{
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public Text textR;
    public Text textG;
    public Text textB;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        textR.color = new Color(sliderR.value/255.0f, 0.0f, 0.0f, 1.0f);
        textG.color = new Color(0.0f, sliderG.value / 255.0f, 0.0f, 1.0f);
        textB.color = new Color(0.0f, 0.0f, sliderB.value / 255.0f, 1.0f);

    }
}
