using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCamera;      // Reference to the camera
    public float zoomSpeed = 2f;   // Speed of zooming in and out
    public float minZoom = 1f;     // Minimum zoom level
    public float maxZoom = 50f;    // Maximum zoom level
    public float panSpeed = 0.5f;  // Speed of panning the camera

    private Vector3 dragOrigin;    // Starting point when dragging with the middle mouse button

    private void Start()
    {
        mainCamera = Camera.main; // Default to main camera if none is assigned
    }

    private void Update()
    {
        // Zoom the camera using mouse scroll wheel
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        // Adjust the orthographic size of the camera
        mainCamera.orthographicSize -= zoomInput * zoomSpeed;

        // Clamp the orthographic size to the min/max zoom levels
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);

        // Panning the camera when the middle mouse button is pressed
        if (Input.GetMouseButtonDown(2)) // Middle mouse button pressed
        {
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition); // Record the starting point of the drag
        }

        if (Input.GetMouseButton(2)) // Middle mouse button held down
        {
            Vector3 currentMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition); // Get current mouse position
            Vector3 offset = dragOrigin - currentMousePosition; // Calculate the offset from the starting point

            // Move the camera by the offset, adjusting the z-axis to avoid moving along the depth
            mainCamera.transform.position += offset;
        }
    }
}
