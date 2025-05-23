using UnityEngine;

public class Agent : MonoBehaviour
{
    public Vector3 direction = Vector3.right;
    public float speed = 2f;

    private float minX, maxX, minY, maxY;
    private float buffer = 0.1f;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized;
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;

        minX = camPos.x - camWidth / 2f;
        maxX = camPos.x + camWidth / 2f;
        minY = camPos.y - camHeight / 2f;
        maxY = camPos.y + camHeight / 2f;
    }

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        bool bounced = false;

        if (pos.x < minX + buffer || pos.x > maxX - buffer)
        {
            bounced = true;
        }

        if (pos.y < minY + buffer || pos.y > maxY - buffer)
        {
            bounced = true;
        }

        if (bounced)
        {
            direction = Random.insideUnitCircle.normalized;
            // Reposicionar dentro de los límites para evitar que se quede fuera
            pos.x = Mathf.Clamp(pos.x, minX + buffer, maxX - buffer);
            pos.y = Mathf.Clamp(pos.y, minY + buffer, maxY - buffer);
            transform.position = pos;
        }
    }
}
