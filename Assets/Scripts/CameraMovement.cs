using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 0.1f;
    
    // Update is called once per frame
    void LateUpdate()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(xDirection, 0, zDirection);

        transform.position += direction * _speed;
    }
}
