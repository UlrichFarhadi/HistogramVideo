using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pdfcdfController : MonoBehaviour
{
    public float moveTimer = 0.0f;
    public float magicTimer = 2.0f;
    public Animator animator;
  

    public GameObject bin1;
    public GameObject bin2;
    public GameObject bin3;
    public GameObject bin4;

    Vector3 startCoordBin1;
    Vector3 startCoordBin2;
    Vector3 startCoordBin3;
    Vector3 startCoordBin4;

    Vector3 destBin1;
    Vector3 destBin2;
    Vector3 destBin3;
    Vector3 destBin4;

    GameObject[] cdfArr;
    public GameObject trailer;
    int cdfIndex = 0;
    public GameObject selector;


    void Start()
    {
        cdfArr = new GameObject[4];

        bin1.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.25f, 0.25f, 0.25f, 1.0f));
        bin2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.35f, 0.35f, 0.35f, 1.0f));
        bin3.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.50f, 0.50f, 0.50f, 1.0f));
        bin4.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.70f, 0.70f, 0.70f, 1.0f));

        startCoordBin1 = new Vector3(bin1.transform.position.x, bin1.transform.position.y, bin1.transform.position.z);
        startCoordBin2 = new Vector3(bin2.transform.position.x, bin2.transform.position.y, bin2.transform.position.z);
        startCoordBin3 = new Vector3(bin3.transform.position.x, bin3.transform.position.y, bin3.transform.position.z);
        startCoordBin4 = new Vector3(bin4.transform.position.x, bin4.transform.position.y, bin4.transform.position.z);

    }

    int state = 0;

    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            state++;
            animator.ResetTrigger("startMoveTimer");
            animator.ResetTrigger("startMoveTimerReverse");
        }



        switch (state)
        {
            case 1:
                destBin1 = new Vector3(100.0f, bin1.transform.position.y, bin1.transform.position.z);
                state++;
                break;
            case 2:
                moveAnimateBin(ref bin1, 1, destBin1);
                animator.SetTrigger("startMoveTimer");
                break;
            case 3:
                GameObject go0 = (GameObject)Instantiate(selector, new Vector3(100.0f, 0.0f, -250 + 30.0f), Quaternion.identity);
                go0.transform.localScale = new Vector3(10.0f, 0.0f, 10.0f);
                cdfArr[0] = go0;
                state++;
                break;
            case 4:
                moveAnimateBin(ref bin1, 1, destBin1);
                animator.SetTrigger("startMoveTimerReverse");
                break;
            case 5:
                destBin1 = new Vector3(200.0f, bin1.transform.position.y, bin1.transform.position.z);
                destBin2 = new Vector3(200.0f, bin2.transform.position.y, bin2.transform.position.z + 30.0f);
                state++;
                break;
            case 6:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                animator.SetTrigger("startMoveTimer");
                break;
            case 7:
                GameObject go1 = (GameObject)Instantiate(selector, new Vector3(200.0f, 0.0f, -250 + 30.0f + 100.0f), Quaternion.identity);
                go1.transform.localScale = new Vector3(10.0f, 0.0f, 10.0f);
                cdfArr[1] = go1;
                state++;
                break;
            case 8:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                animator.SetTrigger("startMoveTimerReverse");
                break;
            case 9:
                destBin1 = new Vector3(300.0f, bin1.transform.position.y, bin1.transform.position.z);
                destBin2 = new Vector3(300.0f, bin2.transform.position.y, bin2.transform.position.z + 30.0f);
                destBin3 = new Vector3(300.0f, bin3.transform.position.y, bin3.transform.position.z + 30.0f + 100.0f);
                state++;
                break;
            case 10:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                moveAnimateBin(ref bin3, 3, destBin3);
                animator.SetTrigger("startMoveTimer");
                break;
            case 11:
                GameObject go2 = (GameObject)Instantiate(selector, new Vector3(300.0f, 0.0f, -250 + 30.0f + 100.0f + 220.0f), Quaternion.identity);
                go2.transform.localScale = new Vector3(10.0f, 0.0f, 10.0f);
                cdfArr[2] = go2;
                state++;
                break;
            case 12:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                moveAnimateBin(ref bin3, 3, destBin3);
                animator.SetTrigger("startMoveTimerReverse");
                break;
            case 13:
                destBin1 = new Vector3(400.0f, bin1.transform.position.y, bin1.transform.position.z);
                destBin2 = new Vector3(400.0f, bin2.transform.position.y, bin2.transform.position.z + 30.0f);
                destBin3 = new Vector3(400.0f, bin3.transform.position.y, bin3.transform.position.z + 30.0f + 100.0f);
                destBin4 = new Vector3(400.0f, bin4.transform.position.y, bin4.transform.position.z + 30.0f + 100.0f + 220.0f);
                state++;
                break;
            case 14:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                moveAnimateBin(ref bin3, 3, destBin3);
                moveAnimateBin(ref bin4, 4, destBin4);
                animator.SetTrigger("startMoveTimer");
                break;
            case 15:
                GameObject go3 = (GameObject)Instantiate(selector, new Vector3(400.0f, 0.0f, -250 + 30.0f + 100.0f + 220.0f + 60.0f), Quaternion.identity);
                go3.transform.localScale = new Vector3(10.0f, 0.0f, 10.0f);
                cdfArr[3] = go3;
                state++;
                break;
            case 16:
                moveAnimateBin(ref bin1, 1, destBin1);
                moveAnimateBin(ref bin2, 2, destBin2);
                moveAnimateBin(ref bin3, 3, destBin3);
                moveAnimateBin(ref bin4, 4, destBin4);
                animator.SetTrigger("startMoveTimerReverse");
                break;
            case 17:
                if (cdfIndex == 3)
                {
                    state++;
                    break;
                }
                if (Vector3.Distance(trailer.transform.position, cdfArr[cdfIndex + 1].transform.position) <= 0.1)
                {
                    cdfIndex++;
                    break;
                }
                
                float step = 250.0f * Time.deltaTime;
                /*
                if (Vector3.Distance(trailer.transform.position, cdfArr[cdfIndex + 1].transform.position) <= Vector3.Distance(cdfArr[cdfIndex].transform.position, cdfArr[cdfIndex + 1].transform.position) / 2.0f)
                {
                    trailer.transform.position = Vector3.MoveTowards(trailer.transform.position, cdfArr[cdfIndex + 1].transform.position, 1.0f);
                    if (Vector3.Distance(trailer.transform.position, cdfArr[cdfIndex + 1].transform.position) <= 0.1)
                    {
                        cdfIndex++;
                    }
                    break;
                }
                */
                trailer.transform.position = Vector3.MoveTowards(trailer.transform.position, cdfArr[cdfIndex + 1].transform.position, step);
                break;
            default:
                break;
        }
        

    }

    void moveAnimateBin(ref GameObject bin_, int binNr, Vector3 destBin_)
    {
        if (binNr == 1)
        {
            bin_.transform.position = Vector3.Lerp(startCoordBin1, destBin_, moveTimer / magicTimer);
        }
        else if (binNr == 2)
        {
            bin_.transform.position = Vector3.Lerp(startCoordBin2, destBin_, moveTimer / magicTimer);
        }
        else if (binNr == 3)
        {
            bin_.transform.position = Vector3.Lerp(startCoordBin3, destBin_, moveTimer / magicTimer);
        }
        else if (binNr == 4)
        {
            bin_.transform.position = Vector3.Lerp(startCoordBin4, destBin_, moveTimer / magicTimer);
        }
    }
}
