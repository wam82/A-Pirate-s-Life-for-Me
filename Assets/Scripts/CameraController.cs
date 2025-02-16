using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerShip;
    public Vector3 offset = new Vector3(1.25f, 20, -40);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (playerShip != null)
        {
            Vector3 desiredPosition = playerShip.position + playerShip.TransformDirection(offset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(playerShip);
        }
    }
}
