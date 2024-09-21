using Assets.Logger;
using UnityEngine;

public class SunRotation : MonoBehaviour
{

    [HideInInspector]
    public GameObject sun;
    [HideInInspector]
    public Light sunLight;

    public bool LogSunLocation = false;


    public Vector3 CenterPoint = Vector3.one;

    public float RotationRadius = 300;
    public float rotationSpeed = 0.1f; // Speed of rotation in degrees per second

    private float currentAngle = 30.0f; // Current angle of rotation

    void Start()
    {
        sun = gameObject;
        sunLight = gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        RotateLightAroundCenterPoint();
    }

    private void RotateLightAroundCenterPoint()
    {
        // Calculate the new position
        currentAngle += rotationSpeed * Time.deltaTime;
        float radians = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(radians) * RotationRadius, Mathf.Sin(radians) * RotationRadius, 0);
        transform.position = CenterPoint + offset;
        transform.LookAt(CenterPoint);
        if (LogSunLocation)
        {
            FileLogger.Log("Sun Postion: " + transform.position);
        }
    }
}