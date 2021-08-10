using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglePixelMover : MonoBehaviour
{

    public GameObject pixelStart;
    public GameObject pixelR;
    public GameObject pixelG;
    public GameObject pixelB;
    public GameObject canvasUI;

    private float translateSpeed = 300.0f;

    private int state = 0;

    private Vector3 rDestCoord = new Vector3(-300.0f, -1.0f, 125.0f);
    private Vector3 gDestCoord = new Vector3(0.0f, -1.0f, 125.0f);
    private Vector3 bDestCoord = new Vector3(300.0f, -1.0f, 125.0f);

    // Start is called before the first frame update
    void Start()
    {
        //state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            state++;
            switch (state)
            {
                case 1:
                    pixelR.SetActive(true);
                    break;
                case 2:
                    pixelG.SetActive(true);
                    break;
                case 3:
                    pixelB.SetActive(true);
                    break;
                case 4:
                    canvasUI.SetActive(true);
                    break;
                default:
                    break;
            }
        }
        if (state >= 1)
        {
            pixelR.transform.position = Vector3.MoveTowards(pixelR.transform.position, rDestCoord, translateSpeed * Time.deltaTime);
        }
        if (state >= 2)
        {
            pixelG.transform.position = Vector3.MoveTowards(pixelG.transform.position, gDestCoord, translateSpeed * Time.deltaTime);
        }
        if (state >= 3)
        {
            pixelB.transform.position = Vector3.MoveTowards(pixelB.transform.position, bDestCoord, translateSpeed * Time.deltaTime);
        }


    }
}
