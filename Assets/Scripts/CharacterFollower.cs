using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform character;

    [Header("Follow Settings")]
    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.15f;

    [SerializeField, Min(0f)]
    private float maxSpeed = 30f;

    private float velocityY;

    private void LateUpdate()
    {
        if (character == null)
            return;

        float currentY = transform.position.y;

        // Never target a position below the current Y.
        float targetY = Mathf.Max(
            currentY,
            character.position.y
        );

        float newY = Mathf.SmoothDamp(
            currentY,
            targetY,
            ref velocityY,
            smoothTime,
            maxSpeed,
            Time.deltaTime
        );

        transform.position = new Vector3(
            transform.position.x,
            Mathf.Max(currentY, newY),
            transform.position.z
        );

        // Stop only after the upward velocity has decayed.
        if (velocityY < 0f)
            velocityY = 0f;
    }
}
