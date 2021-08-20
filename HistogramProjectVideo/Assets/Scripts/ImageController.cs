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
    public float pixelSpeed;
    struct PixelAgentsDataStruct
    {
        public Vector2Int agentPosition;
        public Vector4 agentColor;
        
        public Vector2Int agentDestCoord;
        public int agentSpeed;
    } // REMEMBER TO UPDATE THE LINE BELOW
    int PixelAgentsDataStructSize = sizeof(int) * 2 + sizeof(float) * 4 + sizeof(int) * 2 + sizeof(int) * 1;

    PixelAgentsDataStruct[] agents;
    int numAgents;

    // Histogram stuff
    struct BinData
    {
        public int numPixInBin;
    }

    public float maxBinHeight = 300.0f; // To scale the histogram to a max height
    public GameObject[] binObjects;
    BinData[] binData; // Data used to change the height of the histogram.
    public GameObject selector;
    int numPixelsInImage; // Number of pixels in the image
    int numBins = 256;

    // Start is called before the first frame update
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



        InitializeAgents(0, 0, imageColorArray.GetLength(0), imageColorArray.GetLength(1), ref imageColorArray, "rgb");
        PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 0);

        // Initializing the histogram stuff
        binObjects = new GameObject[numBins];
        binData = new BinData[256];
        numPixelsInImage = imageTexture.width * imageTexture.height;
        float binDims = 2.5f;
        float histStartCoord = -binDims*128;
        for (int i = 0; i < numBins; i++)
        {
            GameObject go = (GameObject)Instantiate(selector, new Vector3((float)histStartCoord + i * binDims, 0.0f, -45.0f), Quaternion.identity);
            go.transform.localScale = new Vector3(binDims, 0.0f, 10.0f); // Set z Højden til at være 0.0 da den skal ændres med histogrammet vokser
            binObjects[i] = go;
        }
        // Set the goals of each agent:
        SetHistogramGoalToAgentsGRAY(ref agents, ref binObjects);


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

        
        // Make a state machine to do the shit and zoom in on the histogram









        // ------------------------OLD SHIT----------------------------
        
        //PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 3);
        
        
        //ClearImageWithShader("ClearImageKernel", ref clearImageShader, "ResultTexture", ref renderTexture, ref image);
        //PrintImageWithShader("ImageKernel", ref pixelMoveShader, ref pixelMoveBuffer, ref agents, "pixelMoveBuffer", "ResultTexture", ref renderTexture, ref image, 1);


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
    
    void SetHistogramGoalToAgentsGRAY(ref PixelAgentsDataStruct[] agents_, ref GameObject[] binObjects_)
    {
        for (int i = 0; i < agents_.Length; i++)
        {
            // Gray value of agent:
            int grayValue = Mathf.RoundToInt(agents_[i].agentColor.x * 255.0f);

            // Get the world screen coordinates
            // Set the agent goal to the screen coordinates of the binObject.
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
                    agentSpeed = UnityEngine.Random.Range(2, 5)
                    
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