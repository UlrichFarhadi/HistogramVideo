using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HistCompareScript : MonoBehaviour
{



    // Image loading stuff
    [Tooltip("Image to be read")]
    [SerializeField] Texture2D imageTexture; // Image to be read
    Color[,] imageColorArray;

    struct PixelAgentsDataStruct
    {
        public Vector2Int agentPosition;
        public Vector4 agentColor;

        public Vector2Int agentDestCoord;
        public float agentSpeed;

        public int goalReached;
    }

    int numBins = 256;

    PixelAgentsDataStruct[] agents;
    int numAgents;


    // Histogram stuff
    struct BinData
    {
        public int numPixInBin;
    }

    public GameObject selectorBin;
    public GameObject selectorCdf;

    BinData[] histBinData1;
    BinData[] histBinData2;

    GameObject[] histBinObjects1;
    GameObject[] histBinObjects2;

    GameObject[] cdfObjects1;
    GameObject[] cdfObjects2;

    float histHeight = 300.0f;
    float binWidth = 1.5f;


    void Start()
    {

        histBinData1 = new BinData[numBins];
        histBinData2 = new BinData[numBins];

        histBinObjects1 = new GameObject[numBins];
        histBinObjects2 = new GameObject[numBins];

        cdfObjects1 = new GameObject[numBins];
        cdfObjects2 = new GameObject[numBins];

        LoadImageColorArray(ref imageTexture, ref imageColorArray);
        numAgents = imageTexture.width * imageTexture.height;
        agents = new PixelAgentsDataStruct[numAgents];
        InitializeAgents(0, 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "grayscale");


        // Load the individual histograms
        LoadBinData(ref agents, ref histBinData1);
        LoadBinData(ref agents, ref histBinData2);


    }

    int index = 0;
    int state = 0;
    void Update()
    {
        if (aPressed())
        {
            state++;
        }
        
        switch (state)
        {
            case 0:
                index = 0;
                break;
            case 1:
                SpawnHistogram(-400.0f, -200.0f, binWidth, histHeight, ref histBinData1, ref histBinObjects1, ref index);
                //index--;
                //SpawnHistogram(400.0f - 256*binWidth, 000.0f, binWidth, histHeight, ref histBinData2, ref histBinObjects2, ref index);
                Application.targetFrameRate = 150;
                if (index == numBins - 1)
                {
                    index = 0;
                    state++;
                    break;
                }
                break;
            case 2:
                index = 0;
                break;
            case 3:
                SpawnCdf(-350.0f + 256 * binWidth, -200.0f, binWidth, histHeight, numAgents, ref histBinData1, ref cdfObjects1, ref index);
                Application.targetFrameRate = 150;
                if (index == numBins - 1)
                {
                    index = 0;
                    state++;
                    break;
                }
                break;
            default:
                break;

        }
        
    }

    void LoadBinData(ref PixelAgentsDataStruct[] agents_, ref BinData[] binData_)
    {
        for (int i = 0; i < agents_.Length; i++)
        {
            int grayValue = Mathf.RoundToInt(agents_[i].agentColor.x * 255.0f);
            binData_[grayValue].numPixInBin++;
        }
    }

    void ForLoopTimeWaster(int iterations)
    {
        int ass = 0;
        for (int i = 0; i < iterations; i++)
        {
            ass++;
        }
        return;
    }

    IEnumerator TimeDelay(float timeDelay)
    {

        yield return new WaitForSeconds(timeDelay);

    }

    void TimeWaster()
    {
        return;
    }

    int findMaxBin(ref BinData[] binData_)
    {
        int currentMaxBin = 0;
        for (int i = 0; i < binData_.Length; i++)
        {
            if (binData_[i].numPixInBin >= currentMaxBin)
            {
                currentMaxBin = binData_[i].numPixInBin;
            }
        }
        return currentMaxBin;
    }

    void SpawnHistogram(float xStart, float yStart, float binWidth_, float binHeight_, ref BinData[] binData_, ref GameObject[] binObjects, ref int i)
    {
        float binStartSize = 0.1f;
        int biggestBin = findMaxBin(ref binData_);

        float binOffset = (float)binData_[i].numPixInBin / (float)biggestBin; // Number from 0 to 1
        GameObject go = (GameObject)Instantiate(selectorBin, new Vector3((float)xStart + i * binWidth_, 0.0f, binData_[i].numPixInBin == 0 ? yStart + binStartSize / 2.0f : yStart + ((binOffset * binHeight_) / 2.0f)), Quaternion.identity);
        go.transform.localScale = new Vector3(binWidth_, 0.0f, binData_[i].numPixInBin == 0 ? binStartSize : (binOffset * binHeight_)); // Set z Højden til at være 0.0 da den skal ændres med histogrammet vokser
        Vector4 grayColor = new Vector4((float)i / 255.0f, (float)i / 255.0f, (float)i / 255.0f, 1.0f);
        go.GetComponent<Renderer>().material.SetColor("_Color", grayColor);
        binObjects[i] = go;

        i++;

        return;
    }

    void SpawnCdf(float xStart, float yStart, float binWidth_, float binHeight_, int numPixels, ref BinData[] binData_, ref GameObject[] cdfObjects_, ref int i)
    {
        float currentCdfHeight = 0.0f;
        for (int j = 0; j <= i; j++)
        {
            currentCdfHeight += ((float)binData_[j].numPixInBin / (float)numPixels);
        }
        GameObject go = (GameObject)Instantiate(selectorCdf, new Vector3((float)xStart + i * binWidth, 0.0f, yStart + currentCdfHeight * binHeight_), Quaternion.identity);
        go.transform.localScale = new Vector3(2.0f, 0.0f, 2.0f);
        cdfObjects_[i] = go;

        i++;

        return;
    }
    


    bool aPressed()
    {
        if (Input.GetKeyDown("a"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }





    public void LoadImageColorArray(ref Texture2D imgTex, ref Color[,] imgColorArr)
    {
        int rows = imageTexture.height;
        int cols = imageTexture.width;

        imgColorArr = new Color[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                imgColorArr[rows - row - 1, col] = imgTex.GetPixel(col, row); // GetPixel(0,0) is bottom left of texture?
                                                                              // The -1 after each index is so we dont exceed array size by 1 in row
            }
        }
    }

    void InitializeAgents(int xCornerStart, int yCornerStart, int rows, int cols, ref Color[,] imgColorArr, string colorName = "rgb")
    {
        //int rows = imageColorArray.GetLength(0); // Array Height aka rows
        //int cols = imageColorArray.GetLength(1); // Array Width aka cols

        int currentAgent = 0;

        for (int row = yCornerStart, i = 0; row < yCornerStart + rows; row++, i++)
        {
            for (int col = xCornerStart, j = 0; col < xCornerStart + cols; col++, j++)
            {
                Vector4 pixelColor = imageColorArray[i, j];
                if (colorName == "rgb")
                {
                    pixelColor = imageColorArray[i, j];
                }
                else if (colorName == "grayscale")
                {
                    float grayScaleColor = (0.299f * imageColorArray[i, j].r + 0.587f * imageColorArray[i, j].g + 0.114f * imageColorArray[i, j].b);
                    Vector4 grayColor = new Vector4(grayScaleColor, grayScaleColor, grayScaleColor, 1.0f);
                    pixelColor = grayColor;
                }
                else if (colorName == "r")
                {
                    float rColor = 0.299f * imageColorArray[i, j].r;
                    Vector4 redColor = new Vector4(rColor, 0.0f, 0.0f, 1.0f);
                    pixelColor = redColor;
                }
                else if (colorName == "g")
                {
                    float gColor = 0.587f * imageColorArray[i, j].g;
                    Vector4 greenColor = new Vector4(0.0f, gColor, 0.0f, 1.0f);
                    pixelColor = greenColor;
                }
                else if (colorName == "b")
                {
                    float bColor = 0.114f * imageColorArray[i, j].b;
                    Vector4 blueColor = new Vector4(0.0f, 0.0f, bColor, 1.0f);
                    pixelColor = blueColor;
                }
                PixelAgentsDataStruct tempAgent = new PixelAgentsDataStruct
                {
                    agentPosition = new Vector2Int(col, rows - row - 1), // If more needs to be added, add a comma after each line except the last line
                    agentColor = pixelColor,
                    agentDestCoord = new Vector2Int(800, 800),
                    agentSpeed = UnityEngine.Random.Range(1, 4)

                };

                agents[currentAgent] = tempAgent;
                currentAgent++;
            }
        }

    }


}
