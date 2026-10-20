using UnityEngine;

public class CameraFollowY : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float offsetY = 0f;
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;

    private float verticalVelocity;
    private float highestCameraY;

    private void Awake()
    {
        highestCameraY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Never allow the target camera height to move downward.
        float desiredY = Mathf.Max(highestCameraY, target.position.y + offsetY);

        float nextY = smoothTime <= 0f
            ? desiredY
            : Mathf.SmoothDamp(
                highestCameraY,
                desiredY,
                ref verticalVelocity,
                smoothTime
            );

        // Prevent even small downward movements due to smoothing.
        highestCameraY = Mathf.Max(highestCameraY, nextY);

        Vector3 position = transform.position;
        position.y = highestCameraY;
        transform.position = position;
    }
}
