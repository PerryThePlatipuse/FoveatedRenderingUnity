using System;
using UnityEngine;


public class CameraMover : MonoBehaviour
{
    public float baseSpeed = 10f;
    public float sprintSpeed = 100f;
    public float lookSensitivity = 3f;
    public float scrollSensitivity = 10f;
    public float rapidScrollSensitivity = 50f;
    private bool isLooking = false;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
        HandleInput();
    }

    private void HandleMovement()
    {
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = sprint ? sprintSpeed : baseSpeed;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction -= transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction += transform.right;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            direction += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction -= transform.forward;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            direction += Vector3.up;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            direction -= Vector3.up;

        transform.position += direction.normalized * currentSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        if (isLooking)
        {
            float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * lookSensitivity;
            float rotY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * lookSensitivity;
            transform.localEulerAngles = new Vector3(rotY, rotX, 0f);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float sensitivity = scroll > 0 ? scrollSensitivity : rapidScrollSensitivity;
            cam.fieldOfView -= scroll > 0 ? 1 : -1;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            EnableLookMode();
        if (Input.GetKeyUp(KeyCode.Mouse1))
            DisableLookMode();
    }

    private void EnableLookMode()
    {
        isLooking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DisableLookMode()
    {
        isLooking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        DisableLookMode();
    }
}
