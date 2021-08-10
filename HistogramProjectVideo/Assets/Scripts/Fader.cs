using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    private bool fadeOut, fadeIn;
    public float fadeSpeed = 6;
    //private GameObject fadeObject;
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown("i"))
        {
            StartCoroutine(FadeInObject());
        }
        if (Input.GetKeyDown("o"))
        {
            StartCoroutine(FadeOutObject());
        }
    }

    public IEnumerator FadeInObject()
    {
        while (this.GetComponent<Renderer>().material.color.a < 1)
        {
            Color objectColor = this.GetComponent<Renderer>().material.color;
            float fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            this.GetComponent<Renderer>().material.color = objectColor;
            yield return null;
        }
    }

    public IEnumerator FadeOutObject()
    {
        while (this.GetComponent<Renderer>().material.color.a > 0)
        {
            Color objectColor = this.GetComponent<Renderer>().material.color;
            float fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            this.GetComponent<Renderer>().material.color = objectColor;
            yield return null;
        }
    }


}


