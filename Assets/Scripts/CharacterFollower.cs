using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(PositionConstraint))]
public class CharacterFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform character;

    private PositionConstraint positionConstraint;

    private void Awake()
    {
        positionConstraint = GetComponent<PositionConstraint>();

        positionConstraint.translationAxis = Axis.Y;
        positionConstraint.constraintActive = false;
    }

    private void LateUpdate()
    {
        if (character == null)
            return;

        bool shouldFollow =
            transform.position.y < character.position.y;

        if (positionConstraint.constraintActive != shouldFollow)
        {
            positionConstraint.constraintActive = shouldFollow;
        }
    }
}
