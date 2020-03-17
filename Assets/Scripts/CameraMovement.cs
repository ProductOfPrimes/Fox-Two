using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    float scrollSpeedMax = 5.0f;

    [SerializeField]
    float panSpeed = 10.0f;

    [SerializeField]
    float damping = 0.1f;

    [SerializeField]
    float interpolationSpeedPanning = 30f;

    [SerializeField]
    float interpolationSpeedZooming = 15f;

    [SerializeField]
    float distMin = 2;

    [SerializeField]
    float distMax = 10;

    private Vector3 velocity = Vector3.zero;

    [SerializeField]
    [Range(0,1)]
    private float zoomAmount = 0;

    // Start is called before the first frame update
    void Start()
    {
        zoomAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 deltaV = new Vector3(Input.GetAxis("Horizontal") * panSpeed, 0, Input.GetAxis("Vertical") * panSpeed);

        if (deltaV.sqrMagnitude > 0)
        {
            velocity = Vector3.Lerp(velocity, deltaV, interpolationSpeedPanning * Time.deltaTime);
        } else 
        {
            velocity *= 1.0f - damping;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newZoom = zoomAmount + scroll * Time.deltaTime * scrollSpeedMax;
        zoomAmount = Mathf.Lerp(zoomAmount, newZoom, interpolationSpeedZooming * Time.deltaTime);
        zoomAmount = Mathf.Clamp(zoomAmount, 0, 1);

        float xRotation = Mathf.Lerp(60, 45, zoomAmount);
        transform.rotation = Quaternion.Euler(xRotation, 0, 0);


        Vector3 nextPos = transform.position + (velocity * Time.deltaTime);
        nextPos.y = Mathf.Lerp(distMax, distMin, zoomAmount);

        transform.position = nextPos;

        if(Input.GetMouseButton(2))
        {
            transform.Translate(new Vector3(-Input.GetAxis("Mouse X") * panSpeed, 0, -Input.GetAxis("Mouse Y") * panSpeed) * Time.deltaTime, Space.World);
        }
    }
}
