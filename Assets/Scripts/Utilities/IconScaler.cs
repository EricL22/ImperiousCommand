using UnityEngine;

public class IconScaler : MonoBehaviour
{
    public float baselineOrthoSize = 2f;

    private Camera mainCamera;
    private Vector3 initialScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null) return;
        // Calculate scale factor based on the ratio of current ortho size to the baseline.
        float scaleFactor = Mathf.Min(mainCamera.orthographicSize / baselineOrthoSize, 5f);
        transform.localScale = initialScale * scaleFactor;
    }
}
