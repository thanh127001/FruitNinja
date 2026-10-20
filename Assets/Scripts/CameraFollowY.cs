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

        float desiredY = target.position.y + offsetY;

        if (desiredY <= highestCameraY)
        {
            verticalVelocity = 0f;
            return;
        }

        float nextY = smoothTime <= 0f
            ? desiredY
            : Mathf.SmoothDamp(
                highestCameraY,
                desiredY,
                ref verticalVelocity,
                smoothTime
            );

        highestCameraY = Mathf.Max(highestCameraY, nextY);
        Vector3 position = transform.position;
        position.y = highestCameraY;
        transform.position = position;
    }
}
