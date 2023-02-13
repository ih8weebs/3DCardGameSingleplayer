
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 8f;
    [SerializeField] private float _scrollSensitivity = 8f;
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private Transform _target;
    [SerializeField] private float _distance = 5.0f;

    float _rotationY;
    float _rotationX;
    
    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;


    void LateUpdate() //the same as update , but is called after all items have been processed in update
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        { ZoomCamera(); }
        
        if(Input.GetMouseButton(1))
        { MoveCamera(); }
        
    }

    void ZoomCamera()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * _scrollSensitivity;

        scrollAmount *= (_distance * 0.3f);
        _distance += scrollAmount * -1f;

        _distance = Mathf.Clamp(_distance, 1f, 20f);
    }

    void MoveCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _rotationY += mouseX;
        _rotationX += mouseY;

        _rotationX = Mathf.Clamp(_rotationX, 0, 90f);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY, 0);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
        transform.localEulerAngles = _currentRotation;

        transform.position = _target.position - transform.forward * _distance;
    }
}
