using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollowY : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float offsetY = 0f;
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;

    private float highestCameraY;

    private void Awake()
    {
        highestCameraY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float desiredY = target.position.y + offsetY;
        if (desiredY <= highestCameraY)
            return;

        // Frame-rate-independent exponential smoothing without velocity resets.
        float blend = smoothTime <= 0f
            ? 1f
            : 1f - Mathf.Exp(-Time.deltaTime / smoothTime);

        float nextY = Mathf.Lerp(highestCameraY, desiredY, blend);
        highestCameraY = Mathf.Max(highestCameraY, nextY);

        Vector3 position = transform.position;
        position.y = highestCameraY;
        transform.position = position;
    }
}
