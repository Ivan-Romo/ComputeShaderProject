using UnityEngine;
using UnityEngine.UI;

public class PixelSimulatorGPU : MonoBehaviour
{
    public ComputeShader computeShader;
    public RawImage display;
    public int width = 1200 * 2;
    public int height = 650 * 2;
    public int agentCount = 10000;

    RenderTexture texture;
    ComputeBuffer agentPositionsBuffer, agentDirectionsBuffer;
    Vector2[] agentPositions, agentDirections;

    int kernel;

    public float sensorAngle = 50f;
    public float sensorDistance = 30f;
    public float turnSpeed = 20f;

    void Start()
    {

        texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        texture.enableRandomWrite = true;
        texture.Create();
        display.texture = texture;

        //

        //agentPositions = new Vector2[agentCount];
        //agentDirections = new Vector2[agentCount];
        //for (int i = 0; i < agentCount; i++)
        //{
        //    agentPositions[i] = new Vector2(Random.Range(0, width), Random.Range(0, height));
        //    float angle = Random.Range(0, Mathf.PI * 2);
        //    agentDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        //}
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float spawnRadius = Mathf.Min(width, height) * 0.5f;

        agentPositions = new Vector2[agentCount];
        agentDirections = new Vector2[agentCount];

        for (int i = 0; i < agentCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * spawnRadius;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 pos = center + offset;
            agentPositions[i] = pos;

            Vector2 toCenter = (center - pos).normalized;
            agentDirections[i] = toCenter;
        }
        
        //
        agentPositionsBuffer = new ComputeBuffer(agentCount, sizeof(float) * 2);
        agentDirectionsBuffer = new ComputeBuffer(agentCount, sizeof(float) * 2);
        agentPositionsBuffer.SetData(agentPositions);
        agentDirectionsBuffer.SetData(agentDirections);

        kernel = computeShader.FindKernel("CSMain");
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.SetTexture(kernel, "Result", texture);
        computeShader.SetBuffer(kernel, "agentPositions", agentPositionsBuffer);
        computeShader.SetBuffer(kernel, "agentDirections", agentDirectionsBuffer);
    }

    void Update()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("sensorAngle", sensorAngle);
        computeShader.SetFloat("sensorDistance", sensorDistance);
        computeShader.SetFloat("turnSpeed", turnSpeed);


        int fadeKernel = computeShader.FindKernel("FadeTrail");
        computeShader.SetFloat("fadeFactor", 0.99f); 
        computeShader.SetTexture(fadeKernel, "Result", texture);
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.Dispatch(fadeKernel, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);


        computeShader.SetInt("agentCount", agentCount);
        computeShader.Dispatch(kernel, Mathf.CeilToInt(agentCount / 256f), 1, 1);
    }

    void OnDestroy()
    {
        agentPositionsBuffer.Release();
        agentDirectionsBuffer.Release();
    }
}
