using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageController : MonoBehaviour
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

    PixelAgentsDataStruct[] agents;
    int numAgents;

    // Histogram stuff
    struct BinData
    {
        public int numPixInBin;
    }

    public Camera cam;
    public float maxBinHeight = 300.0f; // To scale the histogram to a max height
    public GameObject[] binObjects;
    BinData[] binData; // Data used to change the height of the histogram.
    public GameObject selector;
    int numPixelsInImage; // Number of pixels in the image
    int numBins = 256;
    public float binDims = 2.5f;
    public float histStartHeight = -50.0f;
    int biggestBin = 0;

    // Start is called before the first frame update

    public AnimationCurve cameraCurve;

    int state = 0;
    void Start()
    {
        //StartCoroutine(GenerateGrid());
        LoadImageColorArray(ref imageTexture, ref imageColorArray);
        numAgents = imageTexture.width * imageTexture.height;
        agents = new PixelAgentsDataStruct[numAgents];
        

        pixelMoveBuffer = new ComputeBuffer(agents.Length, PixelAgentsDataStructSize);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();



        InitializeAgents(800, -50, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "grayscale");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);

        // Initializing the histogram stuff
        binObjects = new GameObject[numBins];
        binData = new BinData[numBins];
        numPixelsInImage = imageTexture.width * imageTexture.height;
        float histStartCoord = -binDims*128;
        float binStartSize = 2.0f;
        for (int i = 0; i < numBins; i++)
        {
            GameObject go = (GameObject)Instantiate(selector, new Vector3((float)histStartCoord + i * binDims, 0.0f, histStartHeight + (binStartSize/2.0f)), Quaternion.identity);
            go.transform.localScale = new Vector3(binDims, 0.0f, binStartSize); // Set z H�jden til at v�re 0.0 da den skal �ndres med histogrammet vokser
            Vector4 grayColor = new Vector4((float)i/255.0f, (float)i / 255.0f, (float)i / 255.0f, 1.0f);
            go.GetComponent<Renderer>().material.SetColor("_Color", grayColor);
            binObjects[i] = go;
        }
        // Set the goals of each agent:
        SetHistogramGoalToAgentsGRAY(ref agents, ref binObjects);
        BinData[] testBins = new BinData[numBins];
        // Find the biggest bin
        for (int i = 0; i < agents.Length; i++)
        {
            int grayValue = Mathf.RoundToInt(agents[i].agentColor.x * 255.0f);
            testBins[grayValue].numPixInBin++;
        }
        for (int i = 0; i < testBins.Length; i++)
        {
            if (testBins[i].numPixInBin >= biggestBin)
            {
                biggestBin = testBins[i].numPixInBin;
            }
        }


        /*
        InitializeAgents((imageTexture.width + ((Screen.width - imageTexture.width * 5) / 4)) * 1 , 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "grayscale");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image);

        InitializeAgents(((imageTexture.width + ((Screen.width - imageTexture.width * 5) / 4))) * 2, 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "r");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image);

        InitializeAgents(((imageTexture.width + ((Screen.width - imageTexture.width * 5) / 4))) * 3, 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "g");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image);

        InitializeAgents(((imageTexture.width + ((Screen.width - imageTexture.width * 5) / 4))) * 4, 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "b");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image);

        */

        //StartCoroutine(GenerateGrid());

        Application.targetFrameRate = 60;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            state++;
        }

        // Make a state machine to do the shit and zoom in on the histogram

        switch (state)
        {
            case 1:
                AnimateHistogram();
                break;

            case 2:
                // Use Lerp here!
                float camSpeed = 2.0f;
                Vector3 camTargetPosition = new Vector3(0.0f, 687f, 123f);
                cam.transform.position = Vector3.Lerp(cam.transform.position, camTargetPosition, camSpeed * Time.deltaTime);
                break;

            default:
                break;
        }
        
        
       






        // ------------------------OLD SHIT----------------------------

        //PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 3);


        //ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
        //PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 1);

        //for (int i = 0; i < agents.Length; i++)
        //{
        //    agents[i].agentSpeed = UnityEngine.Random.Range(1, 4);
        //}
        /*
        Vector3 mousePos = Input.mousePosition;
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].agentDestCoord.x = (int)mousePos.x;
            agents[i].agentDestCoord.y = (int)mousePos.y;
            agents[i].agentSpeed = UnityEngine.Random.Range(1, 4);


            // This if statement below makes the pixels spawn at random locations if they get stuck in the center
            if ((agents[i].agentDestCoord.x == agents[i].agentPosition.x) && (agents[i].agentDestCoord.y == agents[i].agentPosition.y))
            {
                agents[i].agentPosition.x = UnityEngine.Random.Range(0, Screen.width);
                agents[i].agentPosition.y = UnityEngine.Random.Range(0, Screen.height);
            }
        }
        //Debug.Log(mousePos.x);
        //Debug.Log(mousePos.y);

        if (Input.GetKeyDown("a")) // GetKeyDown will only return true after a press, when user has released it and presses it down again
        {
            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].agentPosition.x = UnityEngine.Random.Range(0, Screen.width);
                agents[i].agentPosition.y = UnityEngine.Random.Range(0, Screen.height);
            }
        }
        */

    }

    void AnimateHistogram()
    {
        ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 1);

        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].goalReached == 1)
            {
                continue;
            }

            // Set agent speed
            //agents[i].agentSpeed = pixelSpeed * Time.deltaTime;
            agents[i].agentSpeed = UnityEngine.Random.Range(1, 3);


            float hypDist = Mathf.Sqrt(Mathf.Pow((agents[i].agentDestCoord.x - agents[i].agentPosition.x), 2) + Mathf.Pow((agents[i].agentDestCoord.y - agents[i].agentPosition.y), 2));
            if (hypDist <= 10.0f)
            {
                agents[i].goalReached = 1;
                int grayValue = Mathf.RoundToInt(agents[i].agentColor.x * 255.0f);
                binData[grayValue].numPixInBin++;

            }
            else
            {
                agents[i].goalReached = 0;
            }


        }
        /*
        for (int i = 0; i < numBins; i++) // Calculate the biggest bin
            // SHOULD BE DONE BEFORE THIS UPDATE ACTUALLY, WE CAN JUST CALCULATE THE HISTOGRAM BEFOREHAND
            // THIS MAKES THE ANIMATION MORE SMOOTH AND INDEPENDANT!!!
        {
            if (binData[i].numPixInBin > biggestBin)
            {
                biggestBin = binData[i].numPixInBin;
            }
        }
        */

        for (int i = 0; i < numBins; i++)
        {
            if (binData[i].numPixInBin == 0)
            {
                continue;
            }
            float scalar = (float)binData[i].numPixInBin / (float)biggestBin; // Number from 0 to 1
            binObjects[i].transform.position = new Vector3(binObjects[i].transform.position.x, binObjects[i].transform.position.y, histStartHeight + (scalar * maxBinHeight) / 2.0f);
            binObjects[i].transform.localScale = new Vector3(binObjects[i].transform.localScale.x, binObjects[i].transform.localScale.y, scalar * maxBinHeight);
        }
    }
    
    void SetHistogramGoalToAgentsGRAY(ref PixelAgentsDataStruct[] agents_, ref GameObject[] binObjects_)
    {
        for (int i = 0; i < agents_.Length; i++)
        {
            // Gray value of agent:
            int grayValue = Mathf.RoundToInt(agents_[i].agentColor.x * 255.0f);

            Vector3 binScreenPos = cam.WorldToScreenPoint(binObjects_[grayValue].transform.position);
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
                imgColorArr[rows-row-1, col] = imgTex.GetPixel(col, row); // GetPixel(0,0) is bottom left of texture?
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

    

    void PrintImageWithShader(string kernel, ref ComputeShader pixelMoveShader_, ref ComputeBuffer pixelMoveBuffer_,  ref PixelAgentsDataStruct[] agents_, string shaderRWBuffer, string shaderResultRWTexture2D, ref RenderTexture renderTexture_, ref RawImage image_, int onMove)
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




/*

    IEnumerator GenerateGrid()
    {
        int rows = imageColorArray.GetLength(0); // Array Height aka rows
        int cols = imageColorArray.GetLength(1); // Array Width aka cols

        GameObject referencePixel = (GameObject)Instantiate(Resources.Load("Models/PixObj"));


        //for (int row = 50, i = 0; row > -50; row--, i++)
        for (int row = rows/2, i = 0; row > -rows/2; row--, i++)
        {
            //for (int col = -50, j = 0; col < 50; col++, j++)
            for (int col = -cols/2, j = 0; col < cols/2; col++, j++)
            {
                GameObject pixel = (GameObject)Instantiate(referencePixel, transform);
                var pixelRenderer = pixel.GetComponent<Renderer>();
                pixelRenderer.material.SetColor("_Color", imageColorArray[i, j]);
                //pixelRenderer.material.SetFloat("_Metallic", 1f);

                
                pixel.transform.position = new Vector3(col, 0, row);
                //yield return new WaitForSeconds(0.0001f);
            }
        }

        Destroy(referencePixel);
        yield return null;
    }

    void DestroyPixelObjects()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

*/