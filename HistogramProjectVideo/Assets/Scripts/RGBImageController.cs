using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RGBImageController : MonoBehaviour
{
    // For Loaded Image
    [Tooltip("Image to be read")]
    [SerializeField] Texture2D imageTexture; // Image to be read
    Color[,] imageColorArray;

    // Compute Shader Stuff
    public ComputeShader pixelMoveShader;
    ComputeBuffer pixelMoveBuffer;
    RenderTexture renderTexture;
    public RawImage image;

    public ComputeShader clearImageShader;

    // Data for the PixelAgents Compute Buffer
    public float pixelSpeed = 1.0f;
    struct PixelAgentsDataStruct
    {
        public Vector2Int agentPosition;
        public Vector4 agentColor;

        public Vector2Int agentDestCoord;
        public float agentSpeed;

        public int goalReached;
    } // REMEMBER TO UPDATE THE LINE BELOW
    int PixelAgentsDataStructSize = sizeof(int) * 2 + sizeof(float) * 4 + sizeof(int) * 2 + sizeof(float) * 1 + sizeof(int) * 1;

    PixelAgentsDataStruct[] agentsR;
    PixelAgentsDataStruct[] agentsG;
    PixelAgentsDataStruct[] agentsB;
    int numAgents;

    // Histogram stuff
    struct BinData
    {
        public int numPixInBin;
    }

    public Camera cam;
    public float maxBinHeight = 300.0f; // To scale the histogram to a max height
    GameObject[] binObjectsR; // The bins of the histogram shown as gameobjects visually
    GameObject[] binObjectsG;
    GameObject[] binObjectsB;
    BinData[] binDataR; // Data used to change the height of the histogram.
    BinData[] binDataG;
    BinData[] binDataB;
    public GameObject selector; // Prefab for the bins
    int numPixelsInImage; // Number of pixels in the image
    int numBins = 256;
    public float binDims = 4.0f;
    public float histStartHeight = -150.0f;
    int biggestBinR = 0;
    int biggestBinG = 0;
    int biggestBinB = 0;
    [Range(0.0f, 1.0f)]
    public float histogramAlpha = 0.5f;

    // Start is called before the first frame update

    int state = 0;
    void Start()
    {
        //StartCoroutine(GenerateGrid());
        LoadImageColorArray(ref imageTexture, ref imageColorArray);
        numAgents = imageTexture.width * imageTexture.height;
        agentsR = new PixelAgentsDataStruct[numAgents];
        agentsG = new PixelAgentsDataStruct[numAgents];
        agentsB = new PixelAgentsDataStruct[numAgents];


        pixelMoveBuffer = new ComputeBuffer(numAgents, PixelAgentsDataStructSize);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();


        // Initialize Red Image
        InitializeAgents(400, -50, ref agentsR, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "r");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsR, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
        // Initialize Green Image
        InitializeAgents(800, -50, ref agentsG, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "g");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsG, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
        // Initialize Blue Image
        InitializeAgents(1200, -50, ref agentsB, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "b");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsB, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);

        // Initializing the histogram stuff
        binObjectsR = new GameObject[numBins];
        binObjectsG = new GameObject[numBins];
        binObjectsB = new GameObject[numBins];
        binDataR = new BinData[numBins];
        binDataG = new BinData[numBins];
        binDataB = new BinData[numBins];
        numPixelsInImage = imageTexture.width * imageTexture.height; // Redundant since it is the same as number of agents
        float histStartCoord = -binDims * 128;
        float binStartSize = 2.0f;
        for (int i = 0; i < numBins; i++)
        {
            GameObject goR = (GameObject)Instantiate(selector, new Vector3((float)histStartCoord + i * binDims, -0.0f, histStartHeight + (binStartSize / 2.0f)), Quaternion.identity);
            GameObject goG = (GameObject)Instantiate(selector, new Vector3((float)histStartCoord + i * binDims, -0.0f, histStartHeight + (binStartSize / 2.0f)), Quaternion.identity);
            GameObject goB = (GameObject)Instantiate(selector, new Vector3((float)histStartCoord + i * binDims, -0.0f, histStartHeight + (binStartSize / 2.0f)), Quaternion.identity);

            goR.transform.localScale = new Vector3(binDims, 0.0f, binStartSize);
            goG.transform.localScale = new Vector3(binDims, 0.0f, binStartSize);
            goB.transform.localScale = new Vector3(binDims, 0.0f, binStartSize);

            Vector4 colorR = new Vector4((float)i / 255.0f, 0.0f, 0.0f, histogramAlpha);
            Vector4 colorG = new Vector4(0.0f, (float)i / 255.0f, 0.0f, histogramAlpha);
            Vector4 colorB = new Vector4(0.0f, 0.0f, (float)i / 255.0f, histogramAlpha);
            goR.GetComponent<Renderer>().material.SetColor("_Color", colorR);
            goG.GetComponent<Renderer>().material.SetColor("_Color", colorG);
            goB.GetComponent<Renderer>().material.SetColor("_Color", colorB);
            binObjectsR[i] = goR;
            binObjectsG[i] = goG;
            binObjectsB[i] = goB;
        }
        // Set the goals of each agent:
        SetHistogramGoalToAgents(ref agentsR, ref binObjectsR, "r");
        SetHistogramGoalToAgents(ref agentsG, ref binObjectsG, "g");
        SetHistogramGoalToAgents(ref agentsB, ref binObjectsB, "b");

        BinData[] testBinsR = new BinData[numBins];
        BinData[] testBinsG = new BinData[numBins];
        BinData[] testBinsB = new BinData[numBins];
        // Find the biggest bin
        for (int i = 0; i < numAgents; i++)
        {
            int colorR = Mathf.RoundToInt(agentsR[i].agentColor.x * 255.0f);
            int colorG = Mathf.RoundToInt(agentsG[i].agentColor.y * 255.0f);
            int colorB = Mathf.RoundToInt(agentsB[i].agentColor.z * 255.0f);
            testBinsR[colorR].numPixInBin++;
            testBinsG[colorG].numPixInBin++;
            testBinsB[colorB].numPixInBin++;
        }
        for (int i = 0; i < numBins; i++)
        {
            if (testBinsR[i].numPixInBin >= biggestBinR)
            {
                biggestBinR = testBinsR[i].numPixInBin;
            }
            if (testBinsG[i].numPixInBin >= biggestBinG)
            {
                biggestBinG = testBinsG[i].numPixInBin;
            }
            if (testBinsB[i].numPixInBin >= biggestBinB)
            {
                biggestBinB = testBinsB[i].numPixInBin;
            }
        }


        /*
        lineRendererR = GetComponent<LineRenderer>();
        lineRendererG = GetComponent<LineRenderer>();
        lineRendererB = GetComponent<LineRenderer>();
        lineRendererR.positionCount = numBins;
        lineRendererG.positionCount = numBins;
        lineRendererB.positionCount = numBins;
        
        lineRendererR.startColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        lineRendererG.startColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        lineRendererB.startColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        lineRendererR.endColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        lineRendererG.endColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        lineRendererB.endColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        */
        




        Application.targetFrameRate = 30;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            state++;
            if (state == 1)
            {
                for (int i = 0; i < numBins; i++)
                {
                    binObjectsR[i].SetActive(true);
                    binObjectsG[i].SetActive(false);
                    binObjectsB[i].SetActive(false);
                }
            }
            if (state == 2)
            {
                for (int i = 0; i < numBins; i++)
                {
                    binObjectsR[i].SetActive(false);
                    binObjectsG[i].SetActive(true);
                    binObjectsB[i].SetActive(false);
                }
            }
            if (state == 3)
            {
                for (int i = 0; i < numBins; i++)
                {
                    binObjectsR[i].SetActive(false);
                    binObjectsG[i].SetActive(false);
                    binObjectsB[i].SetActive(true);
                }
            }
            if (state == 5)
            {
                for (int i = 0; i < numBins; i++)
                {
                    binObjectsR[i].SetActive(true);
                    binObjectsG[i].SetActive(true);
                    binObjectsB[i].SetActive(true);

                    binObjectsR[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f, 0.1f));
                    binObjectsG[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 1.0f, 0.0f, 0.1f));
                    binObjectsB[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 0.0f, 1.0f, 0.1f));

                }
            }
        }

        switch (state)
        {
            case 1:
                ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
                AnimateHistogram(ref agentsR, ref binDataR, ref binObjectsR, ref biggestBinR, "r"); // R
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsG, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsB, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                break;
            case 2:
                ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsR, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                AnimateHistogram(ref agentsG, ref binDataG, ref binObjectsG, ref biggestBinG, "g"); // G
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsB, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                break;
            case 3:
                ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsG, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agentsB, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);
                AnimateHistogram(ref agentsB, ref binDataB, ref binObjectsB, ref biggestBinB, "b"); // B
                break;
            case 4:
                float camSpeed = 2.0f;
                Vector3 camTargetPosition = new Vector3(0.0f, 687f, 123f);
                cam.transform.position = Vector3.Lerp(cam.transform.position, camTargetPosition, camSpeed * Time.deltaTime);
                break;
            case 5:
                break;

            default:
                break;
        }

    }

    void AnimateHistogram(ref PixelAgentsDataStruct[] agents_, ref BinData[] binData_, ref GameObject[] binObjects_, ref int biggestBin_, string color = "r")
    {
        //ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents_, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 1);

        for (int i = 0; i < agents_.Length; i++)
        {
            if (agents_[i].goalReached == 1)
            {
                continue;
            }

            agents_[i].agentSpeed = UnityEngine.Random.Range(6, 9);


            float hypDist = Mathf.Sqrt(Mathf.Pow((agents_[i].agentDestCoord.x - agents_[i].agentPosition.x), 2) + Mathf.Pow((agents_[i].agentDestCoord.y - agents_[i].agentPosition.y), 2));
            if (hypDist <= 10.0f)
            {
                agents_[i].goalReached = 1;
                int colorValue = 0;
                if (color == "r")
                {
                    colorValue = Mathf.RoundToInt(agents_[i].agentColor.x * 255.0f);
                }
                else if (color == "g")
                {
                    colorValue = Mathf.RoundToInt(agents_[i].agentColor.y * 255.0f);
                }
                else if (color == "b")
                {
                    colorValue = Mathf.RoundToInt(agents_[i].agentColor.z * 255.0f);
                }
                binData_[colorValue].numPixInBin++;

            }
            else
            {
                agents_[i].goalReached = 0;
            }


        }

        for (int i = 0; i < numBins; i++)
        {
            if (binData_[i].numPixInBin == 0)
            {
                continue;
            }
            float scalar = (float)binData_[i].numPixInBin / (float)biggestBin_; // Number from 0 to 1
            binObjects_[i].transform.position = new Vector3(binObjects_[i].transform.position.x, binObjects_[i].transform.position.y, histStartHeight + (scalar * maxBinHeight)/2.0f);
            binObjects_[i].transform.localScale = new Vector3(binObjects_[i].transform.localScale.x, binObjects_[i].transform.localScale.y, scalar * maxBinHeight);
            //Vector3 linePos = new Vector3(binObjects_[i].transform.position.x, binObjects_[i].transform.position.y, binObjects_[i].transform.position.z);
            //lineRenderer_.SetPosition(i, linePos);
        }
    }

    void SetHistogramGoalToAgents(ref PixelAgentsDataStruct[] agents_, ref GameObject[] binObjects_, string color = "r")
    {
        for (int i = 0; i < agents_.Length; i++)
        {
            int colorValue = 0;
            if (color == "r")
            {
                colorValue = Mathf.RoundToInt(agents_[i].agentColor.x * 255.0f);
            }
            else if (color == "g")
            {
                colorValue = Mathf.RoundToInt(agents_[i].agentColor.y * 255.0f);
            }
            else if (color == "b")
            {
                colorValue = Mathf.RoundToInt(agents_[i].agentColor.z * 255.0f);
            }
            

            Vector3 binScreenPos = cam.WorldToScreenPoint(binObjects_[colorValue].transform.position);
            agents_[i].agentDestCoord.x = (int)binScreenPos.x;
            agents_[i].agentDestCoord.y = (int)binScreenPos.y;

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

    void InitializeAgents(int xCornerStart, int yCornerStart, ref PixelAgentsDataStruct[] agents_, int rows, int cols, ref Color[,] imgColorArr, string colorName = "rgb")
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
                    Vector4 redColor = new Vector4(imageColorArray[i, j].r, 0.0f, 0.0f, 1.0f);
                    pixelColor = redColor;
                }
                else if (colorName == "g")
                {
                    Vector4 greenColor = new Vector4(0.0f, imageColorArray[i, j].g, 0.0f, 1.0f);
                    pixelColor = greenColor;
                }
                else if (colorName == "b")
                {
                    Vector4 blueColor = new Vector4(0.0f, 0.0f, imageColorArray[i, j].b, 1.0f);
                    pixelColor = blueColor;
                }
                PixelAgentsDataStruct tempAgent = new PixelAgentsDataStruct
                {
                    agentPosition = new Vector2Int(col, rows - row - 1), // If more needs to be added, add a comma after each line except the last line
                    agentColor = pixelColor,
                    agentDestCoord = new Vector2Int(800, 800), // Redundant
                    agentSpeed = UnityEngine.Random.Range(1, 4) // Redundant

                };

                agents_[currentAgent] = tempAgent;
                currentAgent++;
            }
        }

    }



    void PrintImageWithShader(string kernel, ref ComputeShader pixelMoveShader_, ref ComputeBuffer pixelMoveBuffer_, ref PixelAgentsDataStruct[] agents_, string shaderRWBuffer, string shaderResultRWTexture2D, ref RenderTexture renderTexture_, ref RawImage image_, int onMove)
    {
        uint threadGroupSizeX;

        int kernelHandle = pixelMoveShader_.FindKernel(kernel);

        pixelMoveShader_.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out _, out _);

        pixelMoveBuffer_.SetData(agents_); // The buffer is filled with "agents"
        pixelMoveShader_.SetBuffer(kernelHandle, shaderRWBuffer, pixelMoveBuffer_); // Buffer is linked with the RWStructuredbuffer "piexlMoveBuffer" in the shader
        pixelMoveShader_.SetInt("onMove", onMove);

        pixelMoveShader_.SetTexture(kernelHandle, shaderResultRWTexture2D, renderTexture_);

        pixelMoveShader_.Dispatch(kernelHandle, (int)(agents_.Length / threadGroupSizeX) + 1, 1, 1);
        pixelMoveBuffer.GetData(agents_);

        RenderTexture.active = renderTexture_;
        image_.material.mainTexture = renderTexture_;

    }

    void ClearImageWithShader(string kernel, ref ComputeShader clearImageShader_, string shaderResultRWTexture2D, ref RenderTexture renderTexture_, ref RawImage image_)
    {
        uint threadGroupSizeX;
        uint threadGroupSizeY;

        int kernelHandle = clearImageShader_.FindKernel(kernel);

        clearImageShader_.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out threadGroupSizeY, out _);

        clearImageShader_.SetTexture(kernelHandle, shaderResultRWTexture2D, renderTexture_);

        clearImageShader_.Dispatch(kernelHandle, (int)(Screen.width / threadGroupSizeX) + 1, (int)(Screen.height / threadGroupSizeY) + 1, 1);

        RenderTexture.active = renderTexture_;
        image_.material.mainTexture = renderTexture_;

    }

    private void OnDestroy()
    {
        pixelMoveBuffer.Dispose();
    }

}

