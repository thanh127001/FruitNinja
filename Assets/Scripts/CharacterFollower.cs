using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform character;

    private void LateUpdate()
    {
        if (character == null)
            return;

        float targetY = character.position.y;

        if (targetY <= transform.position.y)
            return;

        transform.position = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );
    }
}
