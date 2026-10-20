using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform character;

    [Header("Follow Settings")]
    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.15f;

    private float velocityY;

    private void LateUpdate()
    {
        if (character == null)
            return;

        float currentY = transform.position.y;
        float targetY = character.position.y;

        if (targetY <= currentY)
        {
            velocityY = 0f;
            return;
        }

        float newY = Mathf.SmoothDamp(
            currentY,
            targetY,
            ref velocityY,
            smoothTime
        );

        transform.position = new Vector3(
            transform.position.x,
            Mathf.Max(currentY, newY),
            transform.position.z
        );
    }
}
