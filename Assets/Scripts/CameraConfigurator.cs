using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraConfigurator : MonoBehaviour
{
    private const float PlayAreaWidth = 6.6f;
    private const float PlayAreaHeight = 13f;

    private Camera targetCamera;

    private void OnEnable()
    {
        targetCamera = GetComponent<Camera>();
        ConfigureCamera();
    }

    private void OnValidate()
    {
        targetCamera = GetComponent<Camera>();
        ConfigureCamera();
    }

    private void Update()
    {
        ConfigureCamera();
    }

    private void ConfigureCamera()
    {
        if (targetCamera == null)
            return;

        if (!targetCamera.orthographic)
            targetCamera.orthographic = true;

        Rect fullScreenRect = new Rect(0f, 0f, 1f, 1f);

        if (targetCamera.rect != fullScreenRect)
            targetCamera.rect = fullScreenRect;

        float aspect = targetCamera.aspect;

        if (aspect <= 0f)
            return;

        float sizeByHeight = PlayAreaHeight * 0.5f;
        float sizeByWidth = PlayAreaWidth / (2f * aspect);

        float targetSize = Mathf.Max(
            sizeByHeight,
            sizeByWidth
        );

        if (!Mathf.Approximately(
            targetCamera.orthographicSize,
            targetSize))
        {
            targetCamera.orthographicSize = targetSize;
        }
    }
}
