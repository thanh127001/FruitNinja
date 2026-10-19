using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    private Rigidbody2D rb;

    private bool jumpable = true;
    private bool hasJumped = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !hasJumped)
            return;

        // Continuous upward force, independent of Jump.
        rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Force);
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !jumpable)
            return;

        jumpable = false;
        hasJumped = true;

        float direction;

        if (transform.position.x < 0f)
            direction = 1f;
        else if (transform.position.x > 0f)
            direction = -1f;
        else
            direction = Random.value < 0.5f ? -1f : 1f;

        // Reset horizontal velocity only.
        rb.linearVelocityX = 0f;

        // Horizontal impulse only.
        rb.AddForce(
            Vector2.right * direction * horizontalForce,
            ForceMode2D.Impulse
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider is BoxCollider2D &&
            collision.gameObject.layer == LayerMask.NameToLayer("VerticalBorder"))
        {
            jumpable = true;
        }
    }
}
