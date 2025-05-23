using UnityEngine;
using UnityEngine.UI;

public class PixelSimulator : MonoBehaviour
{
    public RawImage display;
    public int width = 1200 * 2;
    public int height = 650 * 2;
    public int agentCount = 10000;

    Texture2D texture;
    Color[] pixels;

    Vector2[] agentPositions;
    Vector2[] agentDirections;

    float sensorAngle = 50f;        // Grados
    float sensorDistance = 30f;
    float turnSpeed = 20f;          // Cuanto gira hacia el lado más fuerte

    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        texture.SetPixels(pixels);
        texture.Apply();
        display.texture = texture;

        agentPositions = new Vector2[agentCount];
        agentDirections = new Vector2[agentCount];

        for (int i = 0; i < agentCount; i++)
        {
            agentPositions[i] = new Vector2(Random.Range(0, width), Random.Range(0, height)); //new Vector2(Random.Range(width / 2 - 100, width/2+100), Random.Range(height/2-100, height/2+100));
            float angle = Random.Range(0, Mathf.PI * 2);
            agentDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    void Update()
    {
        // Desvanecer rastros
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] *= 0.95f;

        for (int i = 0; i < agentCount; i++)
        {
            Vector2 pos = agentPositions[i];
            Vector2 dir = agentDirections[i];

            // Sensor directions
            float angleRad = Mathf.Atan2(dir.y, dir.x);
            float leftAngle = angleRad + Mathf.Deg2Rad * sensorAngle;
            float rightAngle = angleRad - Mathf.Deg2Rad * sensorAngle;

            float forwardScent = SampleScent(pos + dir * sensorDistance);
            float leftScent = SampleScent(pos + new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle)) * sensorDistance);
            float rightScent = SampleScent(pos + new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle)) * sensorDistance);

            // Gira hacia la dirección con más feromona
            if (leftScent > forwardScent && leftScent > rightScent)
                dir = Rotate(dir, sensorAngle * Time.deltaTime * turnSpeed);
            else if (rightScent > forwardScent && rightScent > leftScent)
                dir = Rotate(dir, -sensorAngle * Time.deltaTime * turnSpeed);

            agentDirections[i] = dir.normalized;
            agentPositions[i] += dir;

            // Rebote en bordes
            if (agentPositions[i].x < 0 || agentPositions[i].x >= width)
            {
                agentDirections[i].x *= -1;
                agentPositions[i].x = Mathf.Clamp(agentPositions[i].x, 1, width - 2);
            }
            if (agentPositions[i].y < 0 || agentPositions[i].y >= height)
            {
                agentDirections[i].y *= -1;
                agentPositions[i].y = Mathf.Clamp(agentPositions[i].y, 1, height - 2);
            }

            // Dejar traza blanca
            int x = Mathf.Clamp((int)agentPositions[i].x, 0, width - 1);
            int y = Mathf.Clamp((int)agentPositions[i].y, 0, height - 1);
            int index = y * width + x;
            pixels[index] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    float SampleScent(Vector2 pos)
    {
        int x = Mathf.Clamp((int)pos.x, 0, width - 1);
        int y = Mathf.Clamp((int)pos.y, 0, height - 1);
        return pixels[y * width + x].grayscale; // Puedes usar solo .r también
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}
