using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform character;

    [Header("Follow Settings")]
    [SerializeField] private float offsetY = 0f;

    private void LateUpdate()
    {
        if (character == null)
            return;

        float targetY = character.position.y + offsetY;

        // Follow instantly upward, never move down.
        if (targetY <= transform.position.y)
            return;

        Vector3 position = transform.position;
        position.y = targetY;
        transform.position = position;
    }
}
